namespace SpicySpa


open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Antiforgery

open Giraffe
open Giraffe.Serialization

open FSharp.Control.Tasks
open Scriban
open MongoDB.Bson

[<RequireQualifiedAccess>]
module Helpers =


    [<Literal>]
    let private Components = "./Views/Components"

    [<Literal>]
    let private Layouts = "./Views/Layouts"

    [<Literal>]
    let private Pages = "./Views/Pages"

    type HtmlKind =
        | Component of name: string
        | Layout of name: string
        | Page of section: string * name: string
        | Partial of section: string * name: string

    let getHtmlPath (kind: HtmlKind) =
        match kind with
        | Component name -> $"{Components}/{name}.html"
        | Layout name -> $"{Layouts}/{name}.html"
        | Page (section, name) -> $"{Pages}/{section}/{name}.html"
        | Partial (section, name) -> $"{Pages}/{section}/Partials/{name}.html"

    let getTemplate (path: string) =
        task {
            let path = Path.GetFullPath(path)
            let! content = File.ReadAllTextAsync(path)
            return Template.Parse(content, path)
        }

    /// Creates a hidden input with a CSRF Token
    /// Also Adds the CSFR Token in the response's cookies
    let csrfInputWithSideEffects (antiforgery: IAntiforgery) (ctx: HttpContext) =
        let tokens = antiforgery.GetAndStoreTokens(ctx)

        $"""<input type="hidden" hidden readonly name="%s{tokens.FormFieldName}" value="%s{tokens.RequestToken}">"""


    let extractPagination (ctx: HttpContext) =
        let page =
            ctx.TryGetQueryStringValue "page"
            |> Option.map
                (fun value ->
                    match System.Int32.TryParse value with
                    | true, value -> value
                    | false, _ -> 1)
            |> Option.defaultValue 1

        let limit =
            ctx.TryGetQueryStringValue "limit"
            |> Option.map
                (fun value ->
                    match System.Int32.TryParse value with
                    | true, value -> value
                    | false, _ -> 20)
            |> Option.defaultValue 20

        (page, limit)


    type ObjectIdConverter() =
        inherit JsonConverter<ObjectId>()

        override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
            ObjectId.Parse(reader.GetString())

        override _.Write(writer: Utf8JsonWriter, value: ObjectId, options: JsonSerializerOptions) =
            writer.WriteStartObject()
            writer.WritePropertyName("$oid")
            writer.WriteStringValue(value.ToString())
            writer.WriteEndObject()


    let JsonSerializer =
        let opts = JsonSerializerOptions()
        opts.AllowTrailingCommas <- true
        opts.ReadCommentHandling <- JsonCommentHandling.Skip
        opts.IgnoreNullValues <- true
        opts.Converters.Add(JsonFSharpConverter())
        opts.Converters.Add(ObjectIdConverter())

        { new IJsonSerializer with
            member __.Deserialize<'T>(arg1: byte []): 'T =
                let spn = ReadOnlySpan(arg1)
                JsonSerializer.Deserialize<'T>(spn, opts)

            member __.Deserialize<'T>(arg1: string): 'T =
                JsonSerializer.Deserialize<'T>(arg1, opts)

            member __.DeserializeAsync(arg1: IO.Stream): Task<'T> =
                JsonSerializer
                    .DeserializeAsync<'T>(arg1, opts)
                    .AsTask()

            member __.SerializeToBytes<'T>(arg1: 'T): byte array =
                JsonSerializer.SerializeToUtf8Bytes(arg1, opts)

            member __.SerializeToStreamAsync<'T> (arg1: 'T) (arg2: IO.Stream): Task =
                JsonSerializer.SerializeAsync(arg2, arg1, opts)

            member __.SerializeToString<'T>(arg1: 'T): string =
                JsonSerializer.Serialize(arg1, typeof<'T>, opts) }

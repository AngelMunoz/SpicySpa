namespace SpicySpa

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Antiforgery
open System.IO
open System.Threading.Tasks
open FSharp.Control.Tasks
open Giraffe
open Scriban

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

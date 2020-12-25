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

    let getTemplate (path: string) =
        task {
            let path = Path.GetFullPath(path)
            let! content = File.ReadAllTextAsync(path)
            return Template.Parse(content, path)
        }

    let htmx (layout: string) (next: HttpFunc) (context: HttpContext) =
        let isHtmx =
            context.Request.Headers.ContainsKey("HX-Request")

        htmlString layout next context


    /// Creates a hidden input with a CSRF Token
    /// Also Adds the CSFR Token in the response's cookies
    let csrfInputWithSideEffects (antiforgery: IAntiforgery) (ctx: HttpContext) =
        let tokens = antiforgery.GetAndStoreTokens(ctx)

        $"""<input type="hidden" hidden readonly name="%s{tokens.FormFieldName}" value="%s{tokens.RequestToken}">"""

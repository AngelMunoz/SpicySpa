namespace SpicySpa

open Giraffe
open Microsoft.AspNetCore.Http
open System.Threading.Tasks
open FSharp.Control.Tasks
open Feliz.ViewEngine

open type Feliz.ViewEngine.prop
open Microsoft.AspNetCore.Antiforgery

[<RequireQualifiedAccess>]
module Helpers =

    let htmx (layout: ReactElement) (next: HttpFunc) (context: HttpContext) =
        let isHtmx =
            context.Request.Headers.ContainsKey("HX-Request")

        if isHtmx
        then htmlString (Render.htmlView layout) next context
        else htmlString (Render.htmlDocument layout) next context


    /// Creates a hidden input with a CSRF Token 
    /// Also Adds the CSFR Token in the response's cookies
    let csrfInputWithSideEffects (antiforgery: IAntiforgery) (ctx: HttpContext) =
        let tokens = antiforgery.GetAndStoreTokens(ctx)

        Html.input [
            type' "hidden"
            name tokens.FormFieldName
            hidden true
            readOnly true
            value tokens.RequestToken
        ]

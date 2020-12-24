namespace SpicySpa.Pages

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http

open Giraffe
open Feliz.ViewEngine

open Saturn.Auth

open type Feliz.ViewEngine.prop

open FSharp.Control.Tasks

open SpicySpa
open SpicySpa.Components
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies


[<RequireQualifiedAccess>]
module Auth =

    let private authForm (isLogin: bool) =

        let nameField =
            if isLogin then
                Html.none
            else
                Html.section [
                    className "field"
                    children [
                        Html.label [
                            className "label"
                            text "Name"
                        ]
                        Html.div [
                            className "control"
                            children [
                                Html.input [
                                    className "input"
                                    type' "text"
                                    name "name"
                                    id "name"
                                ]
                            ]
                        ]
                    ]
                ]

        let emailField =
            Html.section [
                className "field"
                children [
                    Html.label [
                        className "label"
                        text "Email"
                    ]
                    Html.div [
                        className "control"
                        children [
                            Html.input [
                                className "input"
                                type' "email"
                                name "email"
                                id "email"
                            ]
                        ]
                    ]
                ]
            ]

        let passwordField =
            Html.section [
                className "field"
                children [
                    Html.label [
                        className "label"
                        text "password"
                    ]
                    Html.div [
                        className "control"
                        children [
                            Html.input [
                                className "input"
                                type' "password"
                                name "password"
                                id "password"
                            ]
                        ]
                    ]
                ]
            ]

        let firstColumn =
            Html.section [
                className "column"
                children [
                    Html.button [
                        className $"""button is-link"""
                        custom ("hx-get", (if isLogin then "/auth/signup" else "/"))
                        custom ("hx-target", "body")
                        text ((if isLogin then "Sign up" else "Sign in"))
                    ]
                ]
            ]

        let secondColumn =
            Html.section [
                className "column"
                children [
                    Html.button [
                        className $"""button is-primary"""
                        type' "submit"
                        text ((if isLogin then "Sign in" else "Sign up"))
                    ]

                ]

            ]

        let formBottom =
            Html.section [
                className "columns mt-1"
                children [ firstColumn; secondColumn ]
            ]

        Html.form [
            custom ("hx-post", (if isLogin then "/auth/login" else "/auth/signup"))
            custom ("hx-target", "body")
            className "box"
            children [
                nameField
                emailField
                passwordField
                formBottom
            ]
        ]

    let Login =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            if not ctx.User.Identity.IsAuthenticated then
                let antiforgery = ctx.GetService<IAntiforgery>()

                let html =
                    let content =
                        Html.article [
                            className "is-flex is-flex-direction-column is-align-items-center"
                            children [
                                Helpers.csrfInputWithSideEffects antiforgery ctx
                                authForm true
                            ]
                        ]

                    Layouts.Custom
                        "Welcome"
                        content
                        None
                        None
                        None
                        (Some [
                            Html.link [
                                rel "stylesheet"
                                href "auth.css"
                            ]
                         ])

                Helpers.htmx html next ctx
            else
                redirectTo false "/profile" next ctx

    let SignUp =
        fun (next: HttpFunc) (ctx: HttpContext) ->

            if not ctx.User.Identity.IsAuthenticated then
                let antiforgery = ctx.GetService<IAntiforgery>()

                let html =
                    let content =
                        Html.article [
                            className "is-flex is-flex-direction-column is-align-items-center"
                            children [
                                Helpers.csrfInputWithSideEffects antiforgery ctx
                                authForm false
                            ]
                        ]

                    Layouts.Custom
                        "Welcome"
                        content
                        None
                        None
                        None
                        (Some [
                            Html.link [
                                rel "stylesheet"
                                href "auth.css"
                            ]
                         ])

                Helpers.htmx html next ctx
            else
                redirectTo false "/profile" next ctx

    let ProcessLogin =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = ctx.TryBindFormAsync<LoginPayload>()

                match payload with
                | Ok payload ->
                    printfn $"%A{payload}"

                    let principal =
                        ClaimsPrincipal(
                            ClaimsIdentity(
                                [ Claim(ClaimTypes.Name, "Sample") ],
                                CookieAuthenticationDefaults.AuthenticationScheme
                            )
                        )

                    do! ctx.SignInAsync(principal)
                    return! redirectTo false "/profile" next ctx
                | Error err ->
                    let content =
                        Html.article [
                            children [
                                Flash err None
                                authForm true
                            ]
                        ]

                    return! Helpers.htmx content next ctx
            }

    let ProcessSignup =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = ctx.TryBindFormAsync<LoginPayload>()

                match payload with
                | Ok payload ->
                    printfn $"%A{payload}"

                    let principal =
                        ClaimsPrincipal(
                            ClaimsIdentity(
                                [ Claim(ClaimTypes.Name, "Sample") ],
                                CookieAuthenticationDefaults.AuthenticationScheme
                            )
                        )

                    do! ctx.SignInAsync(principal)
                    return! redirectTo false "/profile" next ctx
                | Error err ->
                    let content =
                        Html.article [
                            children [
                                Flash err None
                                authForm false
                            ]
                        ]

                    return! Helpers.htmx content next ctx
            }

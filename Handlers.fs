namespace SpicySpa.Handlers

open System.Security.Claims
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http

open Giraffe

open Saturn.Auth
open Scriban

open FSharp.Control.Tasks


open SpicySpa
open SpicySpa.Components


[<RequireQualifiedAccess>]
module Auth =

    let private authForm (isLogin: bool) =
        task {
            let! template = Helpers.getTemplate ("./Components/AuthForm.html")
            return! template.RenderAsync({| isLogin = isLogin |})
        }

    let Login =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                if not ctx.User.Identity.IsAuthenticated then
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! contentTemplate = Helpers.getTemplate ("./Pages/Auth.html")
                    let! form = authForm true

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form |}


                    let! html =
                        let scripts =
                            ResizeArray([ """<link rel="stylesheet" href="auth.css">""" ])

                        Layouts.Custom "Welcome" contentTpl None None None (Some scripts)

                    return! Helpers.htmx html next ctx
                else
                    return! redirectTo false "/profile" next ctx

            }

    let SignUp =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                if not ctx.User.Identity.IsAuthenticated then
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! contentTemplate = Helpers.getTemplate ("./Pages/Auth.html")
                    let! form = authForm false

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form |}


                    let! html =
                        let scripts =
                            ResizeArray([ """<link rel="stylesheet" href="auth.css">""" ])

                        Layouts.Custom "Welcome" contentTpl None None None (Some scripts)

                    return! Helpers.htmx html next ctx
                else
                    return! redirectTo false "/profile" next ctx

            }

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
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! contentTemplate = Helpers.getTemplate ("./Pages/Auth.html")
                    let! form = authForm false
                    let! flash = Components.Flash err None

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form
                               flash = flash |}

                    return! Helpers.htmx contentTpl next ctx
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
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! contentTemplate = Helpers.getTemplate ("./Pages/Auth.html")
                    let! form = authForm false
                    let! flash = Components.Flash err None

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form
                               flash = flash |}

                    return! Helpers.htmx contentTpl next ctx
            }


[<RequireQualifiedAccess>]
module Profile =
    let private infoPartial (user: User) (flash: string option) =
        task {
            let flash = defaultArg flash null
            let! cardHeader = CardHeader "My Profile" None

            let! cardContent =
                task {
                    let! template = Helpers.getTemplate ("./Components/ProfileInfo.html")
                    return! template.RenderAsync(user)
                }

            let! footer =
                let actions =
                    ResizeArray(
                        [ """
                        <a class="card-footer-item" hx-get="/profile/edit" hx-swap="outerHTML" hx-target="#infopartial">
                            Edit
                        </a>
                        """ ]
                    )

                Components.CardActionsFooter actions

            let! content = CustomCard cardContent (cardHeader |> Some) (footer |> Some)

            let tpl =
                Template.Parse
                    """
                    <article id="infopartial">
                        {{ flash | object.eval_template }}
                        {{ content | object.eval_template }}
                    </article>
                    """

            return! tpl.RenderAsync {| flash = flash; content = content |}
        }

    let Index =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! partial =
                    infoPartial
                        { _id = 1
                          name = "Sample"
                          email = "sample@SpicySpa.com" }
                        None

                let! tpl = Helpers.getTemplate ("./Pages/Profile.html")
                let! content = tpl.RenderAsync({| content = partial |})

                let! html = Layouts.Default content

                return! Helpers.htmx html next ctx
            }

    let UserInfoPartial =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! user = ctx.TryBindFormAsync<UserDTO>()

                match user with
                | Ok user ->
                    let! partial =
                        infoPartial
                            { _id = 1
                              name = user.name
                              email = user.email }
                            None

                    let tpl = Template.Parse(partial)
                    let! content = tpl.RenderAsync()
                    return! Helpers.htmx content next ctx
                | Error err ->
                    let! flash = Flash err (Some ActionType.Warning)

                    let! partial =
                        infoPartial
                            { _id = 1
                              name = "Sample"
                              email = "sample@SpicySpa.com" }
                            (Some flash)

                    let tpl = Template.Parse(partial)
                    let! content = tpl.RenderAsync()
                    return! Helpers.htmx content next ctx
            }

    let EditUserInfoPartial =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()

                let! cardHeader = CardHeader "Update My Profile" None

                let! cardContent =
                    task {
                        let! tpl = Helpers.getTemplate ("./Components/ProfileForm.html")
                        return! tpl.RenderAsync()
                    }

                let! footer =
                    let actions =
                        ResizeArray(
                            [ """
                            <a
                                class="card-footer-item"
                                hx-post="/profile/save"
                                hx-swap="outerHTML"
                                hx-include="#editform"
                                hx-target="#editpartial">
                                Save
                            </a>
                            """ ]
                        )

                    CardActionsFooter actions

                let! card = CustomCard cardContent (Some cardHeader) (Some footer)

                let! html =
                    let template =
                        $"""
                         <article id="editpartial">
                            %s{Helpers.csrfInputWithSideEffects antiforgery ctx}
                            %s{card}
                         </article>
                         """

                    Template.Parse(template).RenderAsync()

                return! Helpers.htmx html next ctx
            }

namespace SpicySpa.Handlers

open System.Security.Claims
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http

open Giraffe

open Scriban

open FSharp.Control.Tasks


open SpicySpa


[<RequireQualifiedAccess>]
module Auth =

    let private authForm (isLogin: bool) =
        task {
            let! template =
                let partial = Helpers.Partial("Auth", "AuthForm")

                let path = Helpers.getHtmlPath partial
                Helpers.getTemplate path

            return! template.RenderAsync({| isLogin = isLogin |})
        }

    let Login =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                if not ctx.User.Identity.IsAuthenticated then
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! contentTemplate =
                        let page = Helpers.Page("Auth", "Auth")
                        let path = Helpers.getHtmlPath page
                        Helpers.getTemplate path

                    let! form = authForm true

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form |}


                    let! html =
                        let styles =
                            ResizeArray([ """<link rel="stylesheet" href="auth.css">""" ])

                        Layouts.Custom "Welcome" contentTpl None None (Some styles) None

                    return! htmlString html next ctx
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

                    let! contentTemplate =
                        let page = Helpers.Page("Auth", "Auth")
                        let path = Helpers.getHtmlPath page
                        Helpers.getTemplate path

                    let! form = authForm false

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form |}


                    let! html =
                        let styles =
                            ResizeArray([ """<link rel="stylesheet" href="auth.css">""" ])

                        Layouts.Custom "Welcome" contentTpl None None (Some styles) None

                    return! htmlString html next ctx
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

                    let! contentTemplate =
                        let page = Helpers.Page("Auth", "Auth")
                        let path = Helpers.getHtmlPath page
                        Helpers.getTemplate path

                    let! form = authForm false
                    let! flash = Components.Flash err None

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form
                               flash = flash |}

                    return! htmlString contentTpl next ctx
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

                    let! contentTemplate =
                        let page = Helpers.Page("Auth", "Auth")
                        let path = Helpers.getHtmlPath page
                        Helpers.getTemplate path

                    let! form = authForm false
                    let! flash = Components.Flash err None

                    let! contentTpl =
                        contentTemplate.RenderAsync
                            {| antiforgery = antiforgeryTpl
                               AuthForm = form
                               flash = flash |}

                    return! htmlString contentTpl next ctx
            }


[<RequireQualifiedAccess>]
module Profile =
    let private infoPartial (user: User) (flash: string option) =
        task {
            let flash = defaultArg flash null
            let! cardHeader = Components.CardHeader "My Profile" None

            let! cardContent =
                task {
                    let path =
                        let kind =
                            Helpers.HtmlKind.Partial("Profile", "ProfileInfo")

                        Helpers.getHtmlPath kind

                    let! template = Helpers.getTemplate (path)
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

            let! content = Components.CustomCard cardContent (cardHeader |> Some) (footer |> Some)

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

                let! tpl =
                    let page = Helpers.Page("Profile", "Profile")
                    let path = Helpers.getHtmlPath page
                    Helpers.getTemplate path

                let! content = tpl.RenderAsync({| content = partial |})

                let! html =
                    Layouts.DefaultWithScripts
                        content
                        (ResizeArray([ """<script src="WebComponents/Sample.js" type="module"></script>""" ]))

                return! htmlString html next ctx
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
                    return! htmlString content next ctx
                | Error err ->
                    let! flash = Components.Flash err (Some ActionType.Warning)

                    let! partial =
                        infoPartial
                            { _id = 1
                              name = "Sample"
                              email = "sample@SpicySpa.com" }
                            (Some flash)

                    let tpl = Template.Parse(partial)
                    let! content = tpl.RenderAsync()
                    return! htmlString content next ctx
            }

    let EditUserInfoPartial =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()

                let! cardHeader = Components.CardHeader "Update My Profile" None

                let! cardContent =
                    task {
                        let! tpl =
                            let page =
                                Helpers.Partial("Profile", "ProfileForm")

                            let path = Helpers.getHtmlPath page
                            Helpers.getTemplate path

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

                    Components.CardActionsFooter actions

                let! card = Components.CustomCard cardContent (Some cardHeader) (Some footer)

                let! html =
                    let template =
                        $"""
                         <article id="editpartial">
                           %s{Helpers.csrfInputWithSideEffects antiforgery ctx}
                           %s{card}
                         </article>
                         """

                    Template.Parse(template).RenderAsync()

                return! htmlString html next ctx
            }

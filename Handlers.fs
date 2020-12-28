namespace SpicySpa.Handlers

open System.Security.Claims

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http

open MongoDB.Bson

open FSharp.Control.Tasks

open Giraffe
open Giraffe.HttpStatusCodeHandlers

open Scriban

open SpicySpa
open SpicySpa.Database





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

    let private getErrorResponse (antiforgeryTpl: string) (message: string) (action: ActionType) =
        task {
            let! contentTemplate =
                let page = Helpers.Page("Auth", "Auth")
                let path = Helpers.getHtmlPath page
                Helpers.getTemplate path

            let! form = authForm false
            let! flash = Components.Flash message (Some action)

            return!
                contentTemplate.RenderAsync
                    {| antiforgery = antiforgeryTpl
                       AuthForm = form
                       flash = flash |}
        }

    let ProcessLogin =


        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = ctx.TryBindFormAsync<LoginPayload>()

                match payload with
                | Ok payload ->
                    printfn $"%A{payload}"

                    let! canLogin = Users.VerifyPassword payload.email payload.password

                    if canLogin then
                        let! user = Users.FindUserByEmail payload.email

                        match user with
                        | Some user ->
                            let principal =
                                ClaimsPrincipal(
                                    ClaimsIdentity(
                                        [ Claim(ClaimTypes.Name, user.name)
                                          Claim(ClaimTypes.NameIdentifier, user._id.ToString())
                                          Claim(ClaimTypes.Email, user.email) ],
                                        CookieAuthenticationDefaults.AuthenticationScheme
                                    )
                                )

                            do! ctx.SignInAsync(principal)
                            return! redirectTo false "/profile" next ctx
                        | None ->
                            let antiforgery = ctx.GetService<IAntiforgery>()

                            let antiforgeryTpl =
                                Helpers.csrfInputWithSideEffects antiforgery ctx

                            let! content = getErrorResponse antiforgeryTpl "Not Found" ActionType.Warning

                            return! RequestErrors.badRequest (htmlString content) next ctx
                    else
                        let antiforgery = ctx.GetService<IAntiforgery>()

                        let antiforgeryTpl =
                            Helpers.csrfInputWithSideEffects antiforgery ctx

                        let! content = getErrorResponse antiforgeryTpl "Not Found" ActionType.Warning

                        return! RequestErrors.badRequest (htmlString content) next ctx
                | Error err ->
                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! content = getErrorResponse antiforgeryTpl err ActionType.Danger

                    return! RequestErrors.badRequest (htmlString content) next ctx
            }

    let ProcessSignup =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! payload = ctx.TryBindFormAsync<SignupPayload>()

                match payload with
                | Ok payload ->
                    printfn $"%A{payload}"

                    let! result = Users.CreateUser payload

                    if result.ok > 0.0 then
                        let! user = Users.FindUserByEmail payload.email

                        match user with
                        | Some user ->
                            let principal =
                                ClaimsPrincipal(
                                    ClaimsIdentity(
                                        [ Claim(ClaimTypes.Name, user.name)
                                          Claim(ClaimTypes.NameIdentifier, user._id.ToString())
                                          Claim(ClaimTypes.Email, user.email) ],
                                        CookieAuthenticationDefaults.AuthenticationScheme
                                    )
                                )

                            do! ctx.SignInAsync(principal)
                            return! redirectTo false "/profile" next ctx
                        | None ->
                            let antiforgery = ctx.GetService<IAntiforgery>()

                            let antiforgeryTpl =
                                Helpers.csrfInputWithSideEffects antiforgery ctx

                            let! content =
                                getErrorResponse antiforgeryTpl "Created But failed to fetch it" ActionType.Warning

                            return! RequestErrors.badRequest (htmlString content) next ctx

                    else
                        let antiforgery = ctx.GetService<IAntiforgery>()

                        let antiforgeryTpl =
                            Helpers.csrfInputWithSideEffects antiforgery ctx

                        let! content = getErrorResponse antiforgeryTpl "Failed to create user" ActionType.Warning

                        return! RequestErrors.badRequest (htmlString content) next ctx
                | Error err ->

                    let antiforgery = ctx.GetService<IAntiforgery>()

                    let antiforgeryTpl =
                        Helpers.csrfInputWithSideEffects antiforgery ctx

                    let! content = getErrorResponse antiforgeryTpl err ActionType.Danger

                    return! RequestErrors.badRequest (htmlString content) next ctx
            }


[<RequireQualifiedAccess>]
module Profile =
    let private infoPartial (user: UserDTO) (flash: string option) =
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
                let! user =
                    let claim =
                        ctx.User.FindFirst(ClaimTypes.NameIdentifier)

                    let _id = ObjectId.Parse(claim.Value)
                    Users.FindUser _id

                match user with
                | None ->
                    let! flash = Components.Flash "Not Found" (Some ActionType.Warning)

                    let content = """
                        <article id="infopartial">
                          <h1>A bad request was sent</h1>
                          <p> We were unable to get the information to update the user</p>
                          {{ flash | object.eval_template }}
                        </article>
                        """

                    let! content =
                        Template
                            .Parse(content)
                            .RenderAsync({| flash = flash |})

                    return! htmlString content next ctx
                | Some user ->
                    let! partial = infoPartial user None

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
                let! user = ctx.TryBindFormAsync<EditFormPayload>()

                let _id =
                    let claim =
                        ctx.User.FindFirst(ClaimTypes.NameIdentifier)

                    ObjectId.Parse(claim.Value)

                match user with
                | Ok user ->

                    let! saveOp = Users.UpdateFields _id (Some user.name) None

                    if saveOp.ok > 0.0 && saveOp.nModified > 0 then
                        let! user = Users.FindUser _id

                        match user with
                        | None ->
                            let! flash = Components.Flash "User Not Found" (Some ActionType.Danger)

                            let partial = """
                                <article id="infopartial">
                                  <h1>Hmm... something weird just happened</h1>
                                  <p> We were unable to get the information to update the user</p>
                                  {{ flash | object.eval_template }}
                                </article>"""

                            let! content =
                                Template
                                    .Parse(partial)
                                    .RenderAsync({| flash = flash |})

                            return! ServerErrors.internalError (htmlString content) next ctx
                        | Some user ->

                            let! partial = infoPartial user None

                            let tpl = Template.Parse(partial)
                            let! content = tpl.RenderAsync()
                            return! htmlString content next ctx
                    else
                        let! flash = Components.Flash "Couldn't get name from form" (Some ActionType.Danger)

                        let partial = """
                            <article id="infopartial">
                              <h1>Hmm... something weird just happened</h1>
                              <p> We were unable to get the information to update the user</p>
                              {{ flash | object.eval_template }}
                            </article>"""

                        let! content =
                            Template
                                .Parse(partial)
                                .RenderAsync({| flash = flash |})

                        return! ServerErrors.internalError (htmlString content) next ctx
                | Error err ->
                    let! flash = Components.Flash err (Some ActionType.Warning)

                    let partial = """
                        <article id="infopartial">
                          <h1>A bad request was sent</h1>
                          <p> We were unable to get the information to update the user</p>
                          {{ flash | object.eval_template }}
                        </article>"""

                    let! content =
                        Template
                            .Parse(partial)
                            .RenderAsync({| flash = flash |})

                    return! RequestErrors.badRequest (htmlString content) next ctx
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

                        let! user =
                            let claim =
                                ctx.User.FindFirst(ClaimTypes.NameIdentifier)

                            let _id = ObjectId.Parse(claim.Value)
                            Users.FindUser _id

                        return!
                            match user with
                            | Some user -> tpl.RenderAsync({| name = user.name |})
                            | None -> tpl.RenderAsync()
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


[<RequireQualifiedAccess>]
module Products =

    let Index =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let (page, limit) = Helpers.extractPagination ctx

                let! products = Products.FindProducts page limit
                let! tpl = Helpers.getTemplate (Helpers.getHtmlPath (Helpers.Page("Products", "Products")))

                let serialized =
                    Helpers.JsonSerializer.SerializeToString products

                let withCountOnly =
                    Helpers.JsonSerializer.SerializeToString { products with list = [] }

                let items =
                    {| list =
                           products.list
                           |> Seq.map (fun item -> Helpers.JsonSerializer.SerializeToString item) |}

                let! content =
                    tpl.RenderAsync(
                        {| serialized = serialized
                           items = items
                           withCountOnly = withCountOnly |}
                    )

                let! html =
                    Layouts.DefaultWithScripts
                        content
                        (ResizeArray([ """<script src="WebComponents/Products.js" type="module"></script>""" ]))

                return! htmlString html next ctx
            }

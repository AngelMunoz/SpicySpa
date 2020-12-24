namespace ServerSpa.Pages

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http

open Giraffe
open Feliz.ViewEngine

open Saturn.Auth

open type Feliz.ViewEngine.prop

open FSharp.Control.Tasks

open ServerSpa
open ServerSpa.Components

[<RequireQualifiedAccess>]
module Profile =
    let private infoPartial (user: User) (flash: ReactElement option) =
        let flash = defaultArg flash Html.none
        let cardHeader = CardHeader "My Profile" None |> Some

        let cardContent =
            Html.div [
                id "#content"
                children [
                    Html.p [
                        children [
                            Html.label [
                                className "label"
                                text "Unique Id:"
                            ]
                            Html.text user._id
                        ]

                    ]
                    Html.p [
                        children [
                            Html.label [
                                className "label"
                                text "Name"
                            ]
                            Html.text user.name
                        ]

                    ]
                    Html.p [
                        children [
                            Html.label [
                                className "label"
                                text "Email"
                            ]
                            Html.text user.email
                        ]
                    ]

                ]

            ]

        let footer =
            CardActionsFooter [
                Html.a [
                    custom ("hx-get", "/profile/edit")
                    custom ("hx-swap", "outerHTML")
                    custom ("hx-target", "#infopartial")
                    className "card-footer-item"
                    text "Edit"
                ]
            ]
            |> Some

        Html.article [
            id "infopartial"
            children [
                flash
                CustomCard cardContent cardHeader footer
            ]
        ]

    let Index =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let html =
                let content =
                    Html.div [
                        className "container"
                        children [
                            infoPartial
                                { _id = 1
                                  name = "Sample"
                                  email = "sample@serverspa.com" }
                                None
                        ]
                    ]

                Layouts.Default content

            Helpers.htmx html next ctx

    let UserInfoPartial =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! user = ctx.TryBindFormAsync<UserDTO>()

                match user with
                | Ok user ->
                    return!
                        Helpers.htmx
                            (infoPartial
                                { _id = 1
                                  name = user.name
                                  email = user.email }
                                None)
                            next
                            ctx
                | Error err ->
                    let flash =
                        Flash err (Some ActionType.Warning) |> Some

                    return!
                        Helpers.htmx
                            (infoPartial
                                { _id = 1
                                  name = "Sample"
                                  email = "sample@serverspa.com" }
                                flash)
                            next
                            ctx
            }

    let EditUserInfoPartial =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let antiforgery = ctx.GetService<IAntiforgery>()

                let html =
                    let cardHeader =
                        CardHeader "Update My Profile" None |> Some

                    let cardContent =
                        let nameSection =
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
                                                value "Sample"
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                        let emailSection =
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
                                                value "sample@serverspa.com"
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                        Html.div [
                            children [
                                Html.form [
                                    id "editform"
                                    children [ nameSection; emailSection ]
                                ]
                            ]
                        ]

                    let footer =
                        CardActionsFooter [
                            Html.a [
                                custom ("hx-post", "/profile/save")
                                custom ("hx-swap", "outerHTML")
                                custom ("hx-include", "#editform")
                                custom ("hx-target", "#editpartial")
                                className "card-footer-item"
                                text "Save"
                            ]
                        ]
                        |> Some

                    Html.article [
                        id "editpartial"
                        children [
                            Helpers.csrfInputWithSideEffects antiforgery ctx
                            CustomCard cardContent cardHeader footer
                        ]
                    ]

                return! Helpers.htmx html next ctx
            }

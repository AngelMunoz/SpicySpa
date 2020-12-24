namespace ServerSpa

open Feliz.ViewEngine

open type Feliz.ViewEngine.prop



module Components =
    type ActionType =
        | Primary
        | Link
        | Info
        | Success
        | Warning
        | Danger
        | Default

        member this.AsString() =
            match this with
            | Primary -> "is-primary"
            | Link -> "is-link"
            | Info -> "is-info"
            | Success -> "is-success"
            | Warning -> "is-warning"
            | Danger -> "is-danger"
            | Default -> ""

    let Flash (msg: string) (action: ActionType option) =
        let action = defaultArg action Default

        Html.div [
            classes [
                "notification"
                action.AsString()
            ]
            prop.children [
                Html.button [ prop.className "delete" ]
                Html.text msg
            ]
        ]


    let CardHeader (title: string) (icon: ReactElement option) =
        let icon = defaultArg icon Html.none

        Html.header [
            className "card-header"
            children [
                Html.p [
                    className "card-header-title"
                    text title
                ]
                icon
            ]
        ]

    let CardFooter (content: ReactElement) =
        Html.footer [
            className "card-footer"
            children [ content ]
        ]

    let CardActionsFooter (actions: ReactElement list) =
        Html.footer [
            className "card-footer"
            children actions
        ]



    let CustomCard (content: ReactElement) (header: ReactElement option) (footer: ReactElement option) =

        let header = defaultArg header Html.none
        let footer = defaultArg footer Html.none

        let content =
            Html.div [
                className "card-content"
                children [ content ]
            ]

        Html.div [
            className "card"
            children [ header; content; footer ]
        ]

    let DefaultCard (content: ReactElement) = CustomCard content None None


    let DefaultFooter =
        Html.footer [
            className "flex flex-shrink"
        ]

    let Navbar (navItems: ReactElement list option) =
        let navItems = defaultArg navItems []

        Html.nav [
            ariaLabel "main navigation"
            className "navbar"
            role "navigation"
            children [
                Html.section [
                    className "navbar-brand"
                    children [
                        Html.a [
                            className "navbar-item"
                            href "/"
                            text "Server Spa"
                        ]
                        Html.a [
                            ariaExpanded false
                            ariaLabel "menu"
                            className "navbar-burger"
                            role "button"
                            children [
                                Html.span [ ariaHidden true ]
                                Html.span [ ariaHidden true ]
                                Html.span [ ariaHidden true ]
                            ]
                        ]
                    ]
                ]
                Html.section [
                    classes [ "navbar-menu" ]
                    children navItems
                ]
            ]
        ]

    let DefaultNavbar = Navbar None

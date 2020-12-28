namespace SpicySpa

open Scriban
open FSharp.Control.Tasks

[<RequireQualifiedAccess>]
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

[<RequireQualifiedAccess>]
module Components =


    let Flash (msg: string) (action: ActionType option) =
        task {
            let action = defaultArg action ActionType.Default

            let! template =
                let cmp = Helpers.Component "Flash"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return!
                template.RenderAsync
                    {| message = msg
                       action = action.AsString() |}
        }



    let CardHeader (title: string) (icon: string option) =
        let icon = defaultArg icon null

        task {
            let! template =
                let cmp = Helpers.Component "CardHeader"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return! template.RenderAsync {| title = title; icon = icon |}
        }

    let CardFooter (content: string) =
        task {
            let! template =
                let cmp = Helpers.Component "CardFooter"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return! template.RenderAsync {| content = content |}
        }

    let CardActionsFooter (actions: ResizeArray<string>) =
        task {
            let! template =
                let cmp = Helpers.Component "CardActionsFooter"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return! template.RenderAsync {| actions = actions |}
        }



    let CustomCard (content: string) (header: string option) (footer: string option) =

        let header = defaultArg header null
        let footer = defaultArg footer null

        task {
            let! template =
                let cmp = Helpers.Component "Card"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return!
                template.RenderAsync
                    {| content = content
                       header = header
                       footer = footer |}

        }

    let DefaultCard (content: string) = CustomCard content None None


    let DefaultFooter =
        task {
            let! template =
                let cmp = Helpers.Component "DefaultFooter"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return! template.RenderAsync()
        }


    let Navbar (navitems: ResizeArray<string> option) =
        let navitems = defaultArg navitems (ResizeArray())

        task {
            let! template =
                let cmp = Helpers.Component "Navbar"
                let path = Helpers.getHtmlPath cmp
                Helpers.getTemplate path

            return! template.RenderAsync {| navitems = navitems |}
        }

    let DefaultNavbar = Navbar None

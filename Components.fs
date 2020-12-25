namespace SpicySpa

open Scriban
open FSharp.Control.Tasks

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
        task {
            let action = defaultArg action Default
            let! template = Helpers.getTemplate ("./Components/Flash.html")

            return!
                template.RenderAsync
                    {| message = msg
                       action = action.AsString() |}
        }



    let CardHeader (title: string) (icon: string option) =
        let icon = defaultArg icon null

        task {
            let! template = Helpers.getTemplate ("./Components/CardHeader.html")
            return! template.RenderAsync {| title = title; icon = icon |}
        }

    let CardFooter (content: string) =
        task {
            let! template = Helpers.getTemplate ("./Components/CardFooter.html")
            return! template.RenderAsync {| content = content |}
        }

    let CardActionsFooter (actions: ResizeArray<string>) =
        task {
            let! template = Helpers.getTemplate ("./Components/CardActionsFooter.html")
            return! template.RenderAsync {| actions = actions |}
        }



    let CustomCard (content: string) (header: string option) (footer: string option) =

        let header = defaultArg header null
        let footer = defaultArg footer null

        task {
            let! template = Helpers.getTemplate ("./Components/Card.html")

            return!
                template.RenderAsync
                    {| content = content
                       header = header
                       footer = footer |}

        }

    let DefaultCard (content: string) = CustomCard content None None


    let DefaultFooter =
        task {
            let! template = Helpers.getTemplate ("./Components/DefaultFooter.html")
            return! template.RenderAsync()
        }


    let Navbar (navitems: ResizeArray<string> option) =
        let navitems = defaultArg navitems (ResizeArray())

        task {
            let! template = Helpers.getTemplate ("./Components/Navbar.html")
            return! template.RenderAsync {| navitems = navitems |}
        }

    let DefaultNavbar = Navbar None

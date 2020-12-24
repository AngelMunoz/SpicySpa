namespace SpicySpa

open Feliz.ViewEngine
open type Feliz.ViewEngine.prop

[<RequireQualifiedAccess>]
module Layouts =

    let Custom (title: string)
               (main: ReactElement)
               (header: ReactElement option)
               (footer: ReactElement option)
               (scripts: ReactElement list option)
               (stylesheets: ReactElement list option)
               =

        let header =
            defaultArg header Components.DefaultNavbar

        let footer =
            defaultArg footer Components.DefaultFooter

        let scripts = defaultArg scripts []
        let stylesheets = defaultArg stylesheets []

        Html.html [
            Html.head [
                Html.meta [
                    charset "utf-8"
                    name "viewport"
                    content "width=device-width, initial-scale=1"
                ]
                Html.title title
                Html.link [
                    rel "stylesheet"
                    href "https://cdn.jsdelivr.net/npm/bulma@0.9.1/css/bulma.min.css"
                ]
                Html.link [
                    rel "stylesheet"
                    href "index.css"
                ]
                yield! stylesheets
                Html.script [
                    src "https://polyfill.io/v3/polyfill.min.js?features=es2015%2Ces2016%2Ces2017%2Ces2018%2Ces2019"
                ]
                Html.script [
                    src "https://unpkg.com/turbolinks@5.2.0/dist/turbolinks.js"
                ]
            ]
            Html.body [
                header
                Html.main [
                    className "app-main"
                    children main
                ]
                footer
                Html.script [
                    src "https://unpkg.com/htmx.org@1.0.2/dist/htmx.min.js"
                ]
                Html.script [ src "index.js" ]
                yield! scripts
            ]
        ]

    let Default (main: ReactElement) =
        Custom "Server Spa" main None None None None

    let Forbidden =
        fun next ctx ->
            let content =
                Components.Flash "You're not allowed to access this resouce" None

            Helpers.htmx (Default content) next ctx

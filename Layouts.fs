namespace SpicySpa

open Scriban
open FSharp.Control.Tasks
open Giraffe.ResponseWriters

[<RequireQualifiedAccess>]
module Layouts =

    let Custom (title: string)
               (main: string)
               (header: string option)
               (footer: string option)
               (scripts: ResizeArray<string> option)
               (stylesheets: ResizeArray<string> option)
               =
        task {
            let! header =
                task {
                    let! def = Components.DefaultNavbar
                    return defaultArg header def
                }

            let! footer =
                task {
                    let! def = Components.DefaultFooter
                    return defaultArg footer def
                }

            let scripts = defaultArg scripts (ResizeArray())
            let stylesheets = defaultArg stylesheets (ResizeArray())

            let! template =
                let layout = Helpers.Layout "Default"
                let path = Helpers.getHtmlPath layout
                Helpers.getTemplate path

            return!
                template.RenderAsync
                    {| title = title
                       header = header
                       main = main
                       footer = footer
                       stylesheets = stylesheets
                       scripts = scripts |}
        }

    let Default (main: string) =
        Custom "Server Spa" main None None None None

    let DefaultWithScripts (main: string) (scripts: ResizeArray<string>) =
        Custom "Server Spa" main None None (Some scripts) None

    let Forbidden =
        fun next ctx ->
            task {
                let! content = Components.Flash "You're not allowed to access this resouce" None
                let! content = Default content
                return! htmlString content next ctx
            }

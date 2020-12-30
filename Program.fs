namespace SpicySpa

open Microsoft.AspNetCore.Http

open FSharp.Control.Tasks

open Giraffe

open Saturn.Application
open Saturn.Pipeline
open Saturn.PipelineHelpers
open Saturn.CSRF
open Saturn.Endpoint


open SpicySpa.Handlers
open Microsoft.AspNetCore.Authentication.Cookies

module Program =

    let setTurbolinksLocationHeader: HttpHandler =
        let isTurbolink (ctx: HttpContext) =
            ctx.Request.Headers.ContainsKey "Turbolinks-Referrer"

        fun next ctx ->
            task {
                if isTurbolink ctx then
                    ctx.SetHttpHeader "Turbolinks-Location" (ctx.Request.Path + ctx.Request.QueryString)

                return! next ctx
            }

    let browser =
        pipeline {
            plug putSecureBrowserHeaders
            set_header "x-pipeline-type" "Browser"
            plug setTurbolinksLocationHeader
        }

    let api =
        pipeline {
            must_accept [ "application/json" ]
            requires_authentication (ResponseWriters.json {| message = "Failed to authenticate" |})
            set_header "x-pipeline-type" "Api"
        }

    let defaultView =
        router {
            get "/" Auth.Login
            get "/index.html" (redirectTo false "/")
            get "/default.html" (redirectTo false "/")

        }

    let authRouter =
        router {
            get "/signup" Auth.SignUp
            post "/signup" (csrf >=> Auth.ProcessSignup)
            post "/login" (csrf >=> Auth.ProcessLogin)
        }

    let profileRouter =
        router {
            get
                "/"
                (requiresAuthentication Layouts.Forbidden
                 >=> Profile.Index)

            get
                "/edit"
                (requiresAuthentication Layouts.Forbidden
                 >=> Profile.EditUserInfoPartial)

            post
                "/save"
                (requiresAuthentication Layouts.Forbidden
                 >=> csrf
                 >=> Profile.UserInfoPartial)
        }

    let productsRouter =
        router {
            get
                "/"
                (requiresAuthentication Layouts.Forbidden
                 >=> Products.Index)
        }

    let browserRouter =
        router {
            pipe_through browser

            forward "" defaultView
            forward "/auth" authRouter
            forward "/profile" profileRouter
            forward "/products" productsRouter
        }

    let jsonRouter =
        router {
            pipe_through api
            get "/products" Products.JsonProducts
        }

    let appRouter =
        router {
            forward "" browserRouter
            forward "/api" jsonRouter
        }

    let app =
        application {
            use_json_serializer Helpers.JsonSerializer

            use_endpoint_router appRouter

            use_antiforgery_with_config
                (fun cfg ->
                    cfg.HeaderName <- "XSRF-TOKEN"
                    cfg.Cookie.Name <- "XSRF-TOKEN")

            use_cookies_authentication "http://localhost:5000"
            use_static "wwwroot"
            use_developer_exceptions
            use_gzip
        }

    [<EntryPoint>]
    let main _ =
        run app
        0 // return an integer exit code

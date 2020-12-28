namespace SpicySpa

open System
open System.Threading.Tasks

open FSharp.Control.Tasks

open MongoDB.Bson
open MongoDB.Driver

open Mondocks.Queries
open Mondocks.Aggregation
open Mondocks.Types

open BCrypt.Net


module Database =

    let dburl =
        Environment.GetEnvironmentVariable("SPICY_DB_URL")
        |> Option.ofObj
        |> Option.defaultValue "mongodb://localhost:27017/"

    [<Literal>]
    let private DbName = "spicydb"

    [<Literal>]
    let private UsersCol = "spc_users"

    [<Literal>]
    let private ProductsCol = "spc_products"

    let db =
        lazy (MongoClient(dburl).GetDatabase(DbName))


    [<RequireQualifiedAccess>]
    module Users =
        let CreateUser (user: SignupPayload) =
            let user =
                { user with
                      password = BCrypt.EnhancedHashPassword user.password }

            let createCmd = insert UsersCol { documents [ user ] }

            db.Value.RunCommandAsync<InsertResult>(JsonCommand createCmd)

        let FindUser (_id: ObjectId) =
            task {
                let q =
                    find UsersCol {
                        filter {| _id = _id |}
                        projection {| email = 1; name = 1 |}
                    }

                let! result = db.Value.RunCommandAsync<FindResult<UserDTO>>(JsonCommand q)

                return (result.cursor.firstBatch |> Seq.tryHead)
            }

        let FindUserByEmail (email: string) =
            task {
                let q =
                    find UsersCol {
                        filter {| email = email |}
                        projection {| email = 1; name = 1 |}
                    }

                let! result = db.Value.RunCommandAsync<FindResult<UserDTO>>(JsonCommand q)

                return (result.cursor.firstBatch |> Seq.tryHead)
            }


        let VerifyPassword (email: string) (password: string) =
            task {
                let q =
                    find UsersCol {
                        filter {| email = email |}
                        projection {| password = 1 |}
                    }

                let! result = db.Value.RunCommandAsync<FindResult<{| _id: ObjectId; password: string |}>>(JsonCommand q)

                return
                    match result.cursor.firstBatch |> Seq.tryHead with
                    | None -> false
                    | Some found -> BCrypt.EnhancedVerify(password, found.password)
            }

        let UpdateFields (_id: ObjectId) (name: Option<string>) (password: Option<string>) =

            let updatePwCmd =
                let updateVal =
                    let updateSet =
                        match name, password with
                        | None, None -> raise (exn "An update must include at least one of [name] or [password]")
                        | Some name, Some password -> box {| name = name; password = password |}
                        | Some name, None -> box {| name = name |}
                        | None, Some password -> box {| password = password |}

                    box
                        {| q = {| _id = _id |}
                           u = {| ``$set`` = updateSet |}
                           upsert = false
                           multi = false |}

                update UsersCol { updates [ updateVal ] }

            db.Value.RunCommandAsync<UpdateResult>(JsonCommand updatePwCmd)

    [<RequireQualifiedAccess>]
    module Products =

        let FindProducts (page: int) (amount: int): Task<PaginatedResult<Product>> =
            task {
                let queryFilter = {|  |}

                let q =
                    find ProductsCol {
                        filter query
                        limit amount
                        skip ((page - 1) * amount)
                    }

                let countCmd =
                    count {
                        collection ProductsCol
                        query queryFilter
                    }

                let! result = db.Value.RunCommandAsync<FindResult<Product>>(JsonCommand q)
                let! countResult = db.Value.RunCommandAsync<CountResult>(JsonCommand countCmd)

                return
                    { list = result.cursor.firstBatch
                      count = countResult.n }
            }

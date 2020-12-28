namespace SpicySpa

open MongoDB.Bson

[<CLIMutable>]
type LoginPayload = { email: string; password: string }

[<CLIMutable>]
type SignupPayload =
    { name: string
      email: string
      password: string }

[<CLIMutable>]
type User =
    { _id: ObjectId
      name: string
      email: string
      password: string }

[<CLIMutable>]
type UserDTO =
    { _id: ObjectId
      name: string
      email: string }

[<CLIMutable>]
type EditFormPayload = { name: string }


type Product =
    { _id: ObjectId
      name: string
      price: decimal }

type PaginatedResult<'T> = { list: seq<'T>; count: int }

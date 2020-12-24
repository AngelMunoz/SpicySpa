namespace SpicySpa

[<CLIMutable>]
type LoginPayload = { email: string; password: string }

[<CLIMutable>]
type SignupPayload =
    { name: string
      email: string
      password: string }

[<CLIMutable>]
type User =
    { _id: int
      name: string
      email: string }

[<CLIMutable>]
type UserDTO = { name: string; email: string }

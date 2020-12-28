type Product = { _id: { $oid: string }, name: string, price: number }

type PaginatedResult<T> = { list: T[], count: number }
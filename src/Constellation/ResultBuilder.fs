module Constellation.ResultBuilder

type ResultBuilder() =
  member inline _.Return(x) = Ok x

  member inline _.ReturnFrom(x: Result<_, _>) = x

  member inline _.Bind(m, f) = Result.bind f m

  member inline _.Zero() = Error

  member inline _.Combine(m, f) = Result.bind f m

  member inline _.Run(f) = f ()

let result = ResultBuilder()

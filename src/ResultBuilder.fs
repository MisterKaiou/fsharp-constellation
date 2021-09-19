module Constalation.ResultBuilder

type ResultBuilder() =
    member _.Return(x) = Ok x
    
    member _.ReturnFrom (x: Result<_, _>) = x
    
    member _.Bind(m, f) = Result.bind f m
    
    member _.Zero() = Error
    
    member _.Combine(m, f) = Result.bind f m
    
    member _.Run(f) = f()
    
let result = ResultBuilder()
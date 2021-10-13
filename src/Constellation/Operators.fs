module Constellation.Operators

let inline (>||) value fun1 fun2 = ((fun1 value), (fun2 value))
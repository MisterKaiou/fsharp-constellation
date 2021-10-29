module Constellation.Operators

/// <summary> Sends the value to the left to the two functions to the right.</summary>
/// <param name="value">The value to send to both functions.</param>
/// <param name="fun1">The first function.</param>
/// <param name="fun2">The second function.</param>
/// <returns> A tuple with the functions results.</returns>
let inline (>||) value fun1 fun2 = ((fun1 value), (fun2 value))
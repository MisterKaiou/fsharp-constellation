module Tests

open Expecto
open Constellation.Context
open System

[<Literal>] 
let private connString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="

let CosmosContext_Dispose_ShouldReflectOnAllInstances() = 
    let subjectOne = new CosmosContext(connString, "TestDb")
    let subjectTwo = new CosmosContext(connString, "TestDb")
    (subjectOne :> IDisposable).Dispose()
    
    Expect.throwsT<ObjectDisposedException>
      <| (fun _ -> subjectOne.Client.ClientOptions |> ignore) 
      <| "Any operation should throw ObjectDiposedException on all instances"
    
    Expect.throwsT<ObjectDisposedException>
      <| (fun _ -> subjectTwo.Client.ClientOptions |> ignore) 
      <| "Any operation should throw ObjectDiposedException on all instances"


let CosmosContext_CosmosClient_ShouldBeSameInstanceOnAllCosmosContextInstances() =
    let subjectOne = new CosmosContext(connString, "SomeDb")
    let subjectTwo = new CosmosContext(connString, "OtherDb")
    let s = subjectTwo.GetContainer "ToDos"
    
    Expect.isTrue 
      <| (obj.ReferenceEquals(subjectOne.Client, subjectTwo.Client)) 
      <| "The CosmosClient instances should be the same throughout all CosmosContextInstances"

[<Tests>]
let tests =
    testList "samples" [
        testCase "universe exists (╭ರᴥ•́)" <| fun _ ->
            let subject = true
            Expect.isTrue subject "I compute, therefore I am."
        
        testCase
            " A disposed Constellation Context should reflect on all instances" 
            CosmosContext_Dispose_ShouldReflectOnAllInstances
        
        testCase
            " Only a single CosmosClient instance should exist"
            CosmosContext_CosmosClient_ShouldBeSameInstanceOnAllCosmosContextInstances
    ]
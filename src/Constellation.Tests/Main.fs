namespace Constellation.Tests

module Entry =

    open Expecto
    
    [<EntryPoint>]
    let main argv =
        runTestsInAssemblyWithCLIArgs [] argv
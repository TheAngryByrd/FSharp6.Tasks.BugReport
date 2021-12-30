module Main

open FSharp6.Tasks.BugReport.TransactionScope

[<EntryPoint>]
let main _argv =
    printfn "Native task computation."
    Native.scope1().GetAwaiter().GetResult()
    
    printfn "Ply task computation."
    Ply.scope1().GetAwaiter().GetResult()
    
    printfn "C# task."
    CSharp.TransactionFlow.Perform().GetAwaiter().GetResult()
    0
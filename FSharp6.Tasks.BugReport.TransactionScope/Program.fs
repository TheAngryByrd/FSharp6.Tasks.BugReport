module Main

[<EntryPoint>]
let main _argv =
    printfn "Native task computation."
    Native.scope1().GetAwaiter().GetResult()
    
    printfn "Ply task computation."
    Ply.scope1().GetAwaiter().GetResult()
    0
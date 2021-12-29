module Main

open System.IO
open System.Threading.Tasks

let getMemoryStream () : Task<MemoryStream> =
    let ms = new MemoryStream()
    Task.FromResult ms

module Native =
    
    let getFileContentStream () : Task<Stream> =
        task {
            let! stream = getMemoryStream()
            return upcast stream
        }
        
    let getFileContentStreamWorkaround () : Task<Stream> =
        task {
            let! stream = getMemoryStream()
            return stream :> Stream
        }
        
module Ply =
    open FSharp.Control.Tasks
    
    let getFileContentStream () : Task<Stream> =
        task {
            let! stream = getMemoryStream()
            return upcast stream
        }

[<EntryPoint>]
let main _argv =
    0
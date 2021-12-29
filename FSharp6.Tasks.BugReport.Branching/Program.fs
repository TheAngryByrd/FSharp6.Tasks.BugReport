module Main

open System.IO
open System.Threading.Tasks

[<AllowNullLiteral>]
type Request() = class end

[<AllowNullLiteral>]
type Response() =
    member val ResponseStream = new MemoryStream() with get

type Client() =
    member x.GetResponseAsync(request : Request) =
        Response() |> Task.FromResult

module Native =
    
    let getFileContent () =
        task {
            let client = Client()
            let request = Request()
            if isNull request then
                return None
            else
                let! response = client.GetResponseAsync(request)
                if isNull response then
                    return failwith "Failed to receive the item from blob." 
                
                use stream = response.ResponseStream
                let length = (stream.Length |> int)
                printfn $"File size is {length} bytes."
                return Some length
        }
        
    let getFileContentWorkaround () =
        task {
            let client = Client()
            let request = Request()
            let! response =
                if isNull request then
                    Task.FromResult None
                else
                    client.GetResponseAsync(request).ContinueWith(fun (s : Task<Response>) -> Some s.Result)
            
            match response with
            | None ->
                return None
            | Some null ->
                return failwith "Failed to receive the item from blob."
            | Some r ->
                use stream = r.ResponseStream
                printfn $"File size is %i{stream.Length} bytes."
                return Some stream.Length
        }
        
module Ply =
    open FSharp.Control.Tasks
    
    let getFileContent () =
        task {
            let client = Client()
            let request = Request()
            if isNull request then
                return None
            else
                let! response = client.GetResponseAsync(request)
                if isNull response then return failwith "Failed to receive the item from blob." 
                
                use stream = response.ResponseStream
                let length = (stream.Length |> int)
                printfn $"File size is {length} bytes."
                return Some length
        }

[<EntryPoint>]
let main _argv =
    0
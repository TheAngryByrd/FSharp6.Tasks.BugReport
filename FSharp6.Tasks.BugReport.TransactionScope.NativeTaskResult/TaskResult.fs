module NetStandard20.TaskResult

open System
open System.Threading.Tasks

type TaskResultBuilder() =

    /// <summary>
    /// Method lets us transform data types into our internal representation. This is the identity method to recognize the self type.
    ///
    /// See https://stackoverflow.com/questions/35286541/why-would-you-use-builder-source-in-a-custom-computation-expression-builder
    /// </summary>
    member inline _.Source(task : Task<Result<'a, 'e>>) : Task<Result<'a, 'e>> =
        task

    member inline _.Zero() : Task<Result<unit, 'e>> =
        () |> Result.Ok |> Task.FromResult

    member inline _.Bind(taskResult : Task<Result<'a, 'e>>, binder : 'a -> Task<Result<'b, 'e>>) : Task<Result<'b, 'e>> =
        task {
            match! taskResult with
            | Error e ->
                return Error e
            | Ok d ->
                return! binder d
        }

    member inline _.Delay(generator : unit -> Task<Result<'a, 'e>>) : unit -> Task<Result<'a, 'e>> =
        generator
        //TODO: version below fixes the issue
        (*fun () -> task {
            let! s = generator ()
            return s
        }*)

    member inline _.Using(resource : 'a :> IDisposable, binder : 'a -> Task<Result<'b, 'e>>) : Task<Result<'b, 'e>> =
        task {
            //TODO: change this to use - net6 will result in IAsyncDisposable compile time error
            use res = resource
            let! result = binder res
            return result
        }
        
    member inline _.Run(f : unit -> Task<'m>) = f ()
    
let taskResult = TaskResultBuilder()

// Having members as extensions gives them lower priority in
// overload resolution between Task<'a> and Task<Either<'a>>.
[<AutoOpen>]
module TaskResultCEExtensions =
    type TaskResultBuilder with
        member inline _.Source(source : Task<'a>) : Task<Result<'a, 'e>> =
            task {
                let! s = source
                return Result.Ok s
            }
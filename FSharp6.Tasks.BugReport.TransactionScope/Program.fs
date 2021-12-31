module Main

open System.Transactions
open Npgsql
open Net6.TaskResult

module Db =
    let connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres;Pooling=true"
    
    let openConnectionAsync() = 
        task {
            let conn = new NpgsqlConnection(connectionString)
            do! conn.OpenAsync(Async.DefaultCancellationToken)
            return conn
        }
    
    let createTransactionScope () =
        let opt = TransactionOptions(IsolationLevel=IsolationLevel.ReadCommitted, Timeout=TransactionManager.MaximumTimeout)
        new TransactionScope(TransactionScopeOption.Required, opt, TransactionScopeAsyncFlowOption.Enabled)
       
    let logCurrentCurrentTransactionId (context : string) =
        task {
            use! connection = openConnectionAsync()
            use cmd = connection.CreateCommand()
            cmd.CommandText <- "SELECT txid_current() AS transaction_id;"
            use! reader = cmd.ExecuteReaderAsync()
            let! _more = reader.ReadAsync()
            let transactionId = reader.GetInt64(0)
            printfn $"%s{context}: transaction_id=%i{transactionId}."
        }

let performScopedWork context =
    // TODO: change to taskResult here to change transaction scope behavior
    taskResult {
        use scope = Db.createTransactionScope()
        do! Db.logCurrentCurrentTransactionId $"%s{context}"
        scope.Complete()
    }

let scope3 () =
    taskResult {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope3"
        scope.Complete()
    }
    

let scope2 () =
    taskResult {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope2"
        do! scope3 ()
        scope.Complete()
    }
    

let scope1 () =
    taskResult {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope1"
        do! scope2()
        scope.Complete()
    }

[<EntryPoint>]
let main _argv =
    printfn "Native task computation."
    let _ = scope1().GetAwaiter().GetResult()
    0
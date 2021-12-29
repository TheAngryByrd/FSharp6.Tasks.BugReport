﻿module Ply

open System.Transactions
open Npgsql
open FSharp.Control.Tasks

module Db =
    let connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=lims;Pooling=true"
    
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
    task {
        use scope = Db.createTransactionScope()
        do! Db.logCurrentCurrentTransactionId $"%s{context}_1"
        do! Db.logCurrentCurrentTransactionId $"%s{context}_2"
        scope.Complete()
    }

let scope3 () =
    task {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope3"
        scope.Complete()
    }
    

let scope2 () =
    task {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope2"
        do! scope3 ()
        scope.Complete()
    }
    

let scope1 () =
    task {
        use scope = Db.createTransactionScope()
        do! performScopedWork "scope1"
        do! scope2()
        scope.Complete()
    }
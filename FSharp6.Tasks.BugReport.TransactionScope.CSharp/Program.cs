using System.Transactions;

namespace FSharp6.Tasks.BugReport.TransactionScope.CSharp;

using Npgsql;
using System.Threading.Tasks;
using System.Transactions;
using static System.Console;

public static class TransactionFlow
{
    private const string ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres;Pooling=true";

    private static async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        return conn;
    }

    private static TransactionScope CreateTransactionScope()
    {
        var opt = new TransactionOptions { 
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.MaximumTimeout
        };
        return new TransactionScope(TransactionScopeOption.Required, opt, TransactionScopeAsyncFlowOption.Enabled);
    }


    private static async Task LogCurrentCurrentTransactionId(string context)
    {
        await using var connection = await OpenConnectionAsync();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT txid_current() AS transaction_id;";
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        var transactionId = reader.GetInt64(0);
        WriteLine($"{context}: transaction_id={transactionId}.");
    }

    private static async Task PerformScopedWork(string context)
    {
        using var scope = CreateTransactionScope();
        await LogCurrentCurrentTransactionId($"{context}_1");
        await LogCurrentCurrentTransactionId($"{context}_2");
        scope.Complete();
    }

    private static async Task Scope3()
    {
        using var scope = CreateTransactionScope();
        await PerformScopedWork("scope3");
        scope.Complete();
    }
    
    private static async Task Scope2()
    {
        using var scope = CreateTransactionScope();
        await PerformScopedWork("scope2");
        await Scope3();
        scope.Complete();
    }
    

    private static async Task Scope1()
    {
        using var scope = CreateTransactionScope();
        await PerformScopedWork("scope1");
        await Scope2();
        scope.Complete();
    }

    public static async Task Perform() => await Scope1();
}

public static class Program
{
    public static async Task Main() => await TransactionFlow.Perform();
}


using Microsoft.Data.Sqlite;
using System;

var connection = new SqliteConnection("Data Source=CompliGenie.db");
connection.Open();

Console.WriteLine("=== CompliGenie Database Verification ===\n");

// List all tables
Console.WriteLine("Tables in database:");
var tableCmd = connection.CreateCommand();
tableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '%Migration%' ORDER BY name;";
using (var reader = tableCmd.ExecuteReader())
{
    while (reader.Read())
    {
        Console.WriteLine($"  - {reader.GetString(0)}");
    }
}

// Check for composite keys
Console.WriteLine("\nChecking composite keys:");
var tables = new[] { "Policies", "Payments", "WebhookEvents", "FinancialAlerts" };
foreach (var table in tables)
{
    var cmd = connection.CreateCommand();
    cmd.CommandText = $"PRAGMA table_info({table});";
    var keyCount = 0;
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            if (reader.GetInt32(5) > 0) // pk column
                keyCount++;
        }
    }
    Console.WriteLine($"  - {table}: {(keyCount > 1 ? "? Has composite key" : "Single key")}");
}

// Count records
Console.WriteLine("\nRecord counts:");
foreach (var table in new[] { "Tenants", "Policies", "Payments" })
{
    var cmd = connection.CreateCommand();
    cmd.CommandText = $"SELECT COUNT(*) FROM {table}";
    var count = Convert.ToInt32(cmd.ExecuteScalar());
    Console.WriteLine($"  - {table}: {count} records");
}

connection.Close();

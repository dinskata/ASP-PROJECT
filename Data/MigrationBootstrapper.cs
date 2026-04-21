using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ASP_PROJECT.Data;

public static class MigrationBootstrapper
{
    public static async Task BaselineEnsureCreatedDatabaseAsync(ApplicationDbContext dbContext, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A valid database connection string is required for migration bootstrapping.");
        }

        if (!await dbContext.Database.CanConnectAsync())
        {
            return;
        }

        var historyExists = await TableExistsAsync(connectionString, "__EFMigrationsHistory");
        if (historyExists)
        {
            return;
        }

        var hasAppTables = await TableExistsAsync(connectionString, "AspNetUsers");
        if (!hasAppTables)
        {
            return;
        }

        var looksLikeLatestSchema =
            await TableExistsAsync(connectionString, "AuditLogs") &&
            await TableExistsAsync(connectionString, "UserVenueAssignments") &&
            await ColumnExistsAsync(connectionString, "Registrations", "AmountPaid") &&
            await ColumnExistsAsync(connectionString, "Reviews", "ModerationStatus");

        if (!looksLikeLatestSchema)
        {
            throw new InvalidOperationException(
                "The existing database was created before migrations and does not match the current schema. Delete the local database once, then run the app again.");
        }

        await dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'__EFMigrationsHistory', N'U') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END
            """);

        foreach (var migrationId in dbContext.Database.GetMigrations())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                migrationId,
                "9.0.6");
        }
    }

    private static async Task<bool> TableExistsAsync(string connectionString, string tableName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_NAME = @tableName
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(string connectionString, string tableName, string columnName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName
            """;

        var tableParameter = command.CreateParameter();
        tableParameter.ParameterName = "@tableName";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);

        var columnParameter = command.CreateParameter();
        columnParameter.ParameterName = "@columnName";
        columnParameter.Value = columnName;
        command.Parameters.Add(columnParameter);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}

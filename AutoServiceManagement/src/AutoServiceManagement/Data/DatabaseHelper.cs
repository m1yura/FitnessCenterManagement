using AutoServiceManagement.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AutoServiceManagement.Data;

/// <summary>
/// ADO.NET helper for SQL Server operations.
/// </summary>
public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper()
    {
        _connectionString = AppSettings.ConnectionString;
    }

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Opens a new SQL connection.
    /// </summary>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Executes a query and returns a data reader with results.
    /// </summary>
    public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
    {
        var connection = CreateConnection();
        var command = CreateCommand(sql, connection, parameters);
        return command.ExecuteReader(CommandBehavior.CloseConnection);
    }

    /// <summary>
    /// Executes INSERT, UPDATE, DELETE and returns affected row count.
    /// </summary>
    public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = CreateCommand(sql, connection, parameters);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a scalar query and returns the first column of the first row.
    /// </summary>
    public object? ExecuteScalar(string sql, params SqlParameter[] parameters)
    {
        using var connection = CreateConnection();
        using var command = CreateCommand(sql, connection, parameters);
        connection.Open();
        return command.ExecuteScalar();
    }

    /// <summary>
    /// Tests database connectivity.
    /// </summary>
    public bool TestConnection(out string message)
    {
        try
        {
            using var connection = CreateConnection();
            connection.Open();
            message = $"Connected to: {connection.Database}";
            return true;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    private static SqlCommand CreateCommand(string sql, SqlConnection connection, SqlParameter[] parameters)
    {
        var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        return command;
    }
}

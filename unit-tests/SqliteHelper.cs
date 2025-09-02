using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using SQLitePCL;

public class SqliteHelper : IDisposable
{
    public readonly IDbConnection _db;
    public SqliteHelper()
    {
        Batteries.Init();
        _db = new SqliteConnection("Data Source=:memory:");
        _db.Open();
    }
    public void SetupProceduresTable()
    {
        _db.Execute(@"
            CREATE TABLE Procedures (
                name TEXT PRIMARY KEY,
                body TEXT
            );
        ");

        _db.Execute("INSERT INTO Procedures (name, body) VALUES (@name, @body);",
            new { name = "MyProc", body = "SELECT 1;" });
    }
    public void SetupViewsTable()
    {
        _db.Execute(@"
            CREATE TABLE Views (
                name TEXT PRIMARY KEY,
                body TEXT
            );
        ");

        _db.Execute("INSERT INTO Views (name, body) VALUES (@name, @body);",
            new { name = "MyView", body = "SELECT 2;" });
    }
    public void SetupTablesTable()
    {
        _db.Execute(@"
            CREATE TABLE TableInfo (
                FullName TEXT,
                columnName TEXT,
                columnType TEXT,
                isNullable TEXT,
                maxLength TEXT,
                isPrimaryKey TEXT,
                isForeignKey TEXT
            );
        ");

        _db.Execute(@"
            INSERT INTO TableInfo (FullName, columnName, columnType, isNullable, maxLength, isPrimaryKey, isForeignKey)
            VALUES (@FullName, @columnName, @columnType, @isNullable, @maxLength, @isPrimaryKey, @isForeignKey);
        ",
        new
        {
            FullName = "MyTable",
            columnName = "Id",
            columnType = "INTEGER",
            isNullable = "NO",
            maxLength = "0",
            isPrimaryKey = "YES",
            isForeignKey = "NO"
        });

        _db.Execute(@"
            INSERT INTO TableInfo (FullName, columnName, columnType, isNullable, maxLength, isPrimaryKey, isForeignKey)
            VALUES (@FullName, @columnName, @columnType, @isNullable, @maxLength, @isPrimaryKey, @isForeignKey);
        ",
        new
        {
            FullName = "MyTable",
            columnName = "Name",
            columnType = "TEXT",
            isNullable = "YES",
            maxLength = "100",
            isPrimaryKey = "NO",
            isForeignKey = "NO"
        });

    }
    public void Dispose()
    {
        _db?.Dispose();
    }
}

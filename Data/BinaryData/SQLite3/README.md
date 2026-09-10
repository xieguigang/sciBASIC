# Managed SQLite3 Database File Reader

A pure managed parser for SQLite3 database files that decodes the database header, B-tree pages, master table and column types, exposes tables and rows, and parses CREATE TABLE schema SQL without the native SQLite engine.

## Overview
- Decodes the file-level structures of the SQLite format: `DatabaseHeader`, `BTreeHeader`, interior and leaf table B-tree pages, cells and overflow pages.
- Enumerates tables through the `sqlite_master` master table (`Sqlite3MasterTable`, `Sqlite3SchemaRow`) and rows through `Sqlite3Table.EnumerateRows`.
- Parses the CREATE TABLE statement of each table into a column schema (`Schema`, `SQLParser`, `Token`, `TokenTypes`), so rows can be addressed by column name or ordinal.
- Decodes SQLite serial types and text encodings (`SqliteDataType`, `DataTypeParser`, `SqliteEncoding`), with varint and record-format helpers.

## Key Types
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Sqlite3Database` — opens a `.db` file via `OpenFile`, exposes `Header` and `GetTables` / `GetTable`.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3Table` — a user table; `EnumerateRows` streams its rows.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3Row` — a row, addressable by column ordinal or name.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3MasterTable` / `Sqlite3SchemaRow` — the `sqlite_master` catalog entries.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers.DatabaseHeader` — the 100-byte database header.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.BTreeLeafTablePage` / `BTreeInteriorTablePage` — B-tree page structures.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema.SQLParser` / `Schema` — CREATE TABLE SQL tokenizer and column schema.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums.SqliteDataType` — SQLite storage class enumeration.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables

Using db As Sqlite3Database = Sqlite3Database.OpenFile("demo.db")
    For Each t As Sqlite3SchemaRow In db.GetTables
        Call Console.WriteLine(t.TableName)
    Next

    Dim tbl As Sqlite3Table = db.GetTable("genes")

    For Each row As Sqlite3Row In tbl.EnumerateRows()
        Dim id As Object = row("id")
    Next
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.IO.SQLite3`
- TargetFramework: `net10.0`
- Tags: `scibasic;sqlite3;database;btree;sql-parser;reader`

## License
GPL-3.0-or-later

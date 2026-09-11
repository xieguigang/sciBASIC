# Managed SQLite3 Database File Reader/Writer

A pure managed parser and writer for SQLite3 database files. It decodes the database header, B-tree pages, master table and column types, exposes tables and rows, and parses CREATE TABLE schema SQL without the native SQLite engine. It can also create new database files, define tables, insert/update/delete rows and commit the whole database back to disk.

## Overview
- Decodes the file-level structures of the SQLite format: `DatabaseHeader`, `BTreeHeader`, interior and leaf table B-tree pages, cells and overflow pages.
- Enumerates tables through the `sqlite_master` master table (`Sqlite3MasterTable`, `Sqlite3SchemaRow`) and rows through `Sqlite3Table.EnumerateRows`.
- Parses the CREATE TABLE statement of each table into a column schema (`Schema`, `SQLParser`, `Token`, `TokenTypes`), so rows can be addressed by column name or ordinal.
- Decodes SQLite serial types and text encodings (`SqliteDataType`, `DataTypeParser`, `SqliteEncoding`), with varint and record-format helpers.
- Writes SQLite3 files (`Sqlite3Writer`): create a new file, define tables, insert/update/delete rows, drop tables and commit. The writer uses an in-memory model and rebuilds the whole file on `Commit` (atomic temp-file replace), then the produced file can be reopened by the reader above.

## Key Types
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Sqlite3Database` — opens a `.db` file via `OpenFile`, exposes `Header` and `GetTables` / `GetTable`.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3Table` — a user table; `EnumerateRows` streams its rows.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3Row` — a row, addressable by column ordinal or name.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Tables.Sqlite3MasterTable` / `Sqlite3SchemaRow` — the `sqlite_master` catalog entries.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Headers.DatabaseHeader` — the 100-byte database header.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.BTreeLeafTablePage` / `BTreeInteriorTablePage` — B-tree page structures.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.SQLSchema.SQLParser` / `Schema` — CREATE TABLE SQL tokenizer and column schema.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Core.Objects.Enums.SqliteDataType` — SQLite storage class enumeration.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer.Sqlite3Writer` — creates/opens a database for writing (`CreateFile` / `OpenFile`), manages tables and commits.
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer.Sqlite3TableWriter` — a writable table with chained CRUD (`AddRow` / `UpdateRow` / `DeleteRow` / `EnumerateRows`).
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer.Sqlite3Column` — column definition (`Name` / `Type` / `NotNull` / `PrimaryKey`).
- `Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer.Sqlite3DataRow` — an in-memory row of the writer.

## Quick Start
### Reading
```vbnet
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite
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

### Writing
```vbnet
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite.Writer

' 新建文件 + 建表 + 插入数据
Using w As Sqlite3Writer = Sqlite3Writer.CreateFile("demo.db")
    Dim tbl = w.CreateTable("genes", {
        New Sqlite3Column("id", "INTEGER", notNull:=True, primaryKey:=True),
        New Sqlite3Column("name", "VARCHAR"),
        New Sqlite3Column("mass", "FLOAT"),
        New Sqlite3Column("data", "BLOB"),
        New Sqlite3Column("flag", "BOOLEAN")
    })

    Dim rowId As Long = tbl.AddRow({1, "p53", 43653.0, New Byte() {1, 2, 3}, True})
    Call tbl.UpdateRow(rowId, "name", "TP53")
    w.Commit()
End Using

' 打开已有文件 + 追加 / 更新 / 删除
Using w As Sqlite3Writer = Sqlite3Writer.OpenFile("demo.db")
    Dim tbl = w.GetTable("genes")
    Call tbl.AddRow({2, "BRCA1", 207721.0, Nothing, False})
    Call tbl.UpdateRowByPrimaryKey(1, {1, "TP53", 43653.0, Nothing, True})
    Call tbl.DeleteRow(2)
    w.Commit()
End Using
```

## Writer Notes & Limitations
- The writer keeps an in-memory model and rebuilds the entire file on `Commit` (written to a temp file and atomically replaced, so a failed commit does not corrupt the original). `Commit` is idempotent. This trades memory for correctness — the whole database must fit in memory.
- Supported column types: NULL, integer (1/2/3/4/6/8 bytes), 64-bit IEEE float, UTF-8 text, BLOB and boolean; large values are automatically stored in overflow pages.
- `INTEGER PRIMARY KEY` is treated as a rowid alias: the column is stored as NULL in the record and the rowid carries the value (symmetric with the reader).
- Not supported: secondary indexes, views, triggers, WITHOUT ROWID tables and custom collations. When opening an existing database, index entries are ignored and only table rows are preserved.

## Package
- Assembly: `Microsoft.VisualBasic.Data.IO.SQLite3`
- TargetFramework: `net10.0`
- Tags: `scibasic;sqlite3;database;btree;sql-parser;reader`

## License
GPL-3.0-or-later

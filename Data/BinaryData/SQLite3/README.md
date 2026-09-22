# 托管 SQLite3 数据库文件读写器

## 引言

在 .NET 里访问 SQLite 通常需要原生库（`e_sqlite3` / `sqlite3.dll`）。但在某些场景下，我们并不需要 SQL 引擎——只需要**把 `.db` 文件当成一种表格文件来读写**：

- 只读导出：把上游系统给的 `.db` 直接读成表格；
- 便携生成：把程序内的表格写成标准 `.db`，无需依赖原生库。

本包就是这样一个**纯托管**的 SQLite3 文件解析器与写出器。

## 设计目标

- **零原生依赖**：直接解码文件格式；
- **schema 可解析**：把 `CREATE TABLE` SQL 解析成列模式，从而支持**按列名**取值；
- **可写**：不止读，还能建库、建表、增删改并原子提交。

## 核心特性

- **文件级结构解码**：`DatabaseHeader`、`BTreeHeader`、内部 / 叶子表 B 树页、cell 与溢出页；
- **表与行枚举**：通过 `sqlite_master` 主表枚举表（`Sqlite3MasterTable`、`Sqlite3SchemaRow`），通过 `Sqlite3Table.EnumerateRows` 枚举行；
- **列模式解析**：把每张表的 `CREATE TABLE` 语句解析为列模式（`Schema`、`SQLParser`、`Token`、`TokenTypes`），从而可按列名或序号取值；
- **存储类与文本编码**：`SqliteDataType`、`DataTypeParser`、`SqliteEncoding`，配 varint 与记录格式辅助；
- **写出**：`Sqlite3Writer` 可创建新文件、定义表、增 / 改 / 删行、删表并提交；写出器维护内存模型，在 `Commit` 时**重建整个文件**（临时文件原子替换），产物可被上述读取器重新打开。

## 关键类型与 API

- `...ManagedSqlite.Sqlite3Database` —— 通过 `OpenFile` 打开 `.db`，暴露 `Header` 与 `GetTables` / `GetTable`；
- `...ManagedSqlite.Core.Tables.Sqlite3Table` —— 用户表；`EnumerateRows` 流式枚举行；
- `...ManagedSqlite.Core.Tables.Sqlite3Row` —— 一行，可按列序号或列名取值；
- `...ManagedSqlite.Core.Tables.Sqlite3MasterTable` / `Sqlite3SchemaRow` —— `sqlite_master` 目录条目；
- `...ManagedSqlite.Core.Objects.Headers.DatabaseHeader` —— 100 字节数据库头；
- `...ManagedSqlite.Core.Objects.BTreeLeafTablePage` / `BTreeInteriorTablePage` —— B 树页结构；
- `...ManagedSqlite.Core.SQLSchema.SQLParser` / `Schema` —— `CREATE TABLE` 分词器与列模式；
- `...ManagedSqlite.Core.Objects.Enums.SqliteDataType` —— SQLite 存储类枚举；
- `...ManagedSqlite.Writer.Sqlite3Writer` —— 创建 / 打开数据库用于写出（`CreateFile` / `OpenFile`），管理表并提交。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.IO.ManagedSqlite

Using db = Sqlite3Database.OpenFile("sample.db")
    For Each table In db.GetTables()
        Console.WriteLine(table.Name)

        For Each row In table.EnumerateRows()
            Console.WriteLine(row("name"))
        Next
    Next
End Using
```

## 实现要点与不支持项

- **记录格式与列的对应关系**：SQLite 的每行是一条「记录」（serial type 序列 + 值区），列名到序号的映射来自解析出的 `CREATE TABLE` 模式——两者缺一不可。
- **写出为何是「重建文件」**：直接原地修改 B 树需要完整的页分裂 / 合并实现，复杂度极高；本包选择在内存中建模、提交时重建文件并原子替换，牺牲写入性能换取可靠性与实现简洁。
- **不支持**：二级索引、视图、触发器、`WITHOUT ROWID` 表与自定义排序规则。打开既有数据库时，索引条目会被忽略，只保留表数据。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.IO.SQLite3`
- TargetFramework：`net10.0`
- Tags：`scibasic;sqlite3;database;btree;sql-parser;reader;writer;managed`
- 许可：GPL-3.0-or-later

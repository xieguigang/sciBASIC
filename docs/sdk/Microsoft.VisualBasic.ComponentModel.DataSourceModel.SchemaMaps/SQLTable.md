# SQLTable
_namespace: [Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps](./index.md)_

SQL之中的一个数据表的抽象描述接口



### Methods

#### GetDeleteSQL
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.SQLTable.GetDeleteSQL
```
DELETE FROM table_name WHERE field = value;

#### GetInsertSQL
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.SQLTable.GetInsertSQL
```
INSERT INTO table_name (field1, field2,...) VALUES (value1, value2,....)
> http://www.w3school.com.cn/sql/sql_insert.asp

#### GetUpdateSQL
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.SQLTable.GetUpdateSQL
```
UPDATE table_name SET field = <new value> WHERE field = <value>
> http://www.w3school.com.cn/sql/sql_update.asp

#### ToString
```csharp
Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.SQLTable.ToString
```
Display the INSERT INTO sql from function @``M:Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps.SQLTable.GetInsertSQL``.



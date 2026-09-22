# 数据框架扩展：对象序列化扩展方法

## 引言

把「一个复杂对象」存进 CSV 看起来很简单，直到你遇到嵌套对象、集合属性与循环引用：

- 一个对象里含有一个列表，列表元素又是另一个对象；
- 存成一张宽表会导致列数爆炸与大量空单元格；
- 存成多张表又要处理主外键关系。

本包提供的方案是**两种极端 + 一个折中**：

1. **一张宽表**：`DataTableStream.StreamTo(Of T)` 把对象集合直接填进 `DataTable`；
2. **多张关联 CSV + JSON 模式**：`SaveData(Of T)` 把对象图拆成目录下的一组 CSV 与一份模式文件；
3. **扁平大纲表**：`OutliningDataLoader` 读回以缩进编码层级的「outlined CSV」。

## 设计目标

- **可回读**：不只是导出，`Load(Of T)` 能按同一套模式还原对象；
- **无侵入**：通过特性（`ColumnAttribute` 等）与约定完成映射，业务类不需要基类；
- **模板可用**：CSV 模板特性让非开发人员也能提供数据结构。

## 核心特性

- 把 `IEnumerable(Of T)` 流式写入 `System.Data.DataTable`：列由反射存储提供者模式生成，逐行填充（支持列映射、布局与转置）；
- 把复杂对象图导出为**一组相互关联的 CSV 表 + 一份 JSON 模式文件**，并把同一对象图压平为单个 `EntityObject()` 汇总表；
- 把层级化的「outlined」CSV 文档（用前导空单元格编码缩进层级）读回为强类型对象，包括子表行；
- 提供 Java 风格的 `Properties` 存储（文本与 XML），支持注释、默认值、排序，以及基于反射的对象填充 / 导出；另含 CSV 模板特性与辅助方法。

## 关键类型与 API

- `...Data.Framework.DataTableStream` —— `StreamTo(Of T)` 扩展，把对象集合填入 `DataTable`；
- `...Data.Framework.SchemasAPI` —— 复杂对象持久化的 `SaveData(Of T)` 与 `Summary(Of T)` 扩展；
- `...Data.Framework.Serialize.Writer` —— 增量写出器，`Dispose` 时刷出关联 CSV 表；
- `...Data.Framework.Serialize.ObjectSchema.Schema` —— 描述拆分后各表的 JSON 模式；
- `...Data.Framework.Serialize.ObjectSchema.Class` / `.Field` —— 逐类型模式模型与字段 / 主键绑定；
- `...Data.Framework.Outlining.OutliningDataLoader` / `.Builder` —— 基于缩进的 outlined CSV 的类型化加载；
- `...Data.Framework.IO.Properties.Properties` / `.Reflector` —— 持久化键值属性表与对象映射；
- `...Data.Framework.TemplateAttribute` / `TemplateHelper` —— CSV 模板元数据与辅助方法。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.Framework.Serialize.ObjectSchema

' 把复杂对象图导出为一个目录下的关联 csv 表 + schema json
Call compounds.SaveData("./export/")

' 或把普通对象集合填入 DataTable
Dim table As New System.Data.DataTable()
Call compounds.StreamTo(table)
```

## 实现要点

- **为什么需要「拆分 + 模式」**：把嵌套结构压进一张表会大量重复数据；拆成多表并记录主外键（`Class` / `Field` 中的绑定）既避免重复，又能在读回时重建对象图。
- **Outlined CSV 的巧思**：用「前导空单元格」表达层级，使得层级结构可以完全落在 CSV 的二维网格里，既不破坏 CSV 兼容性，又能表达父子关系。
- **与 `DataFrame` 的分工**：本包专注「对象 ↔ 表格」的转换，表格本身的列式运算与 I/O 由 `Microsoft.VisualBasic.Data.Framework` 提供。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Framework.Extensions`
- TargetFramework：`net10.0`
- Tags：`scibasic;dataframe;extensions;serialization;datatable;outlining;template`
- 许可：GPL-3.0-or-later

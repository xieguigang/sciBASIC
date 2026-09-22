# RDF/XML 与 Turtle 三元组序列化模型

## 引言

RDF 描述的是「**主语 — 谓语 — 宾语**」三元组构成的图：知识不以表格或对象的形式存储，而以可被机器融合的断言集合存在。这种模型在语义网、知识图谱与元数据交换（DCMI / Dublin Core）中非常常见。

本包负责 `application/rdf+xml` 这一 MIME 类型，提供两条互补的路径：

- **XML 路径**：用可序列化的类模型表示 RDF 文档（描述节点、资源引用、Bag 容器、DCMI 元数据），适合既能读也能写的场景；
- **Turtle 路径**：用流式解析器读取 RDF 1.1 Turtle（`.ttl`）文本，产出三元组并按需再物化为实体对象。

## 设计目标

- **可继承的模型**：`Description` / `RDFEntity` / `RDFProperty` 都是基类，应用侧只需继承即可描述自己的元数据；
- **类型字面量可往返**：内置 XSD 数据类型 URI 与「.NET 类型 ↔ RDF 字面量类型」的映射；
- **流式读取**：Turtle 解析按三元组产出，不需要把整个文件建成图再返回。

## 核心特性

- **RDF 文档 XML 模型**：`RDF(Of T)` 根容器、描述元素、资源引用与 `rdf:Bag` / `rdf:li` 集合；
- **DCMI 元数据**：Dublin Core 字段（title、creator、subject、publisher、rights 等）与 XSD 数据类型映射；
- **Turtle 流式读取**：产出主谓宾三元组，并可通过 `BuildObject` 再物化为 `RDFEntity`；
- **可继承基类**：`Description`、`RDFEntity`、`RDFProperty` 供应用侧扩展。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.application.rdf_xml`（根） | RDF XML 模型：`RDF(Of T)`、`Description`、`DCMI`、`Resource`、`DataTypes` |
| `....rdf_xml.DataModel` | 数据模型辅助类型 |
| `....rdf_xml.Turtle` | RDF 1.1 Turtle 解析器 |

## 关键类型与 API

- `RDF(Of T As Description)` —— 必须被继承的 `rdf:RDF` 文档根元素；负责声明 rdf / rdfs 命名空间；
- `Description` / `RDFEntity` / `RDFProperty` —— 描述节点、带类型的实体节点与属性模型（含值域与注释）；
- `DCMI` —— Dublin Core 元数据属性（title、creator、subject、publisher、rights 等）；
- `Resource` / `RDFType` / `Array` / `li` —— `rdf:resource` 引用、`rdf:type`、Bag 容器与列表项；
- `DataTypes` —— XSD 数据类型 URI，以及 .NET 类型与 RDF 字面量类型之间的映射；
- `Turtle.TurtleFile` —— 流式 RDF 1.1 Turtle 解析器；`ReadObjects()` 产出 `Triple`，处理 `@prefix`；
- `Turtle.Triple` / `Turtle.Relation` / `Turtle.ttl_property` —— 解析出的主语及其谓语 / 宾语关系；
- `Turtle.BuildObject` —— 从三元组序列物化 `RDFEntity` 对象。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.application.rdf_xml.Turtle

Using ttl As New TurtleFile("data.ttl")
    For Each triple As Triple In ttl.ReadObjects()
        For Each rel As Relation In triple.relations
            Console.WriteLine($"{triple.subject} {rel.predicate} {String.Join(",", rel.objs)}")
        Next
    Next
End Using
```

## 实现要点（含已知约束）

- **命名空间约束**：在 RDF 反序列化时，**元素命名空间不能与根元素的 RDF 命名空间相同**，否则无法读回注释数据。这正是实现中把元素类型命名空间设置为 `NA` 字符串的原因。该约束已保留在本包的命名空间文档中。
- **XML 与 Turtle 的取舍**：需要「写」时用 XML 模型（`XmlSerializer` 直接可用）；只需要「读」且数据量大时用 Turtle 流式解析，避免整体建模。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.application.rdf_xml`
- TargetFramework：`net10.0-windows;net10.0`
- Tags：`scibasic;rdf;rdf-xml;turtle;semantic-web;linked-data;dublin-core;dcmi`
- 许可：GPL-3.0-or-later

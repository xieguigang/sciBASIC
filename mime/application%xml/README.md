# XML 工具箱：XSD 模型、MathML 与 XPath 查询

## 引言

.NET 自带 `System.Xml`，但在实际工程里仍然缺三样东西：

1. **把 XML 直接灌进对象图**（而不是手写 `XmlReader` 循环）；
2. **对 XML 做轻量查询**（不必引入完整 XSLT / XPath 引擎）；
3. **表达数学公式**（MathML 内容标记 → 可计算的表达式对象）。

本包补齐这三块，并提供 XSD 的强类型对象模型与完整的 XSD 工具链：从 XSD 生成可供 `XmlSerializer` 使用的 VB 类、从 VB 类反射导出 XSD、以及用 XSD 校验序列化出来的 XML。

## 设计目标

- **扁平且通用**：`XmlElement` 是一个通用的元素节点（名称、命名空间、属性、子元素、文本、注释），任何 XML 都能承载；
- **反射驱动**：`GraphWriter` 通过反射把 XML 直接填充到普通 .NET 对象，不需要为每个模型写映射；
- **查询与模型解耦**：XPath 只在 `IXmlDocumentTree` 抽象上求值，因此可以作用于任意实现；
- **XSD 双向可用**：以 `XmlSchemaSet` 编译结果作为统一中间表示，既能生成类也能反射导出 schema，不引入 `System.CodeDom`、Roslyn 或 `xsd.exe` 子进程。

## 核心特性

- **XSD 元素数据模型**：schema、element、complexType / simpleType、attribute、sequence、restriction，把 XSD 文档映射为可序列化类；
- **XSD → VB 类文件**：`XsdTools.CreateClassFromXsd` 生成带 `XmlRoot` / `XmlType` / `XmlElement` / `XmlAttribute` 特性的 `.vb` 文件，直接用于 XML 序列化与反序列化；
- **VB 类 → XSD 文件**：`XsdTools.CreateXsdFromType` / `CreateXsdFromAssembly` 通过 `XmlReflectionImporter` + `XmlSchemaExporter` 反射导出单一声明、UTF-8 编码的合法 XSD；
- **XSD 校验 XML**：`XsdTools.ValidateXml` 流式校验 `XmlSerializer` 生成的 XML，返回含级别、消息、行列号的结构化结果；
- **MathML 内容标记**：把 `<math>` / `<apply>` 树解析为表达式对象，或直接从 MathML 文本读出 lambda 表达式；
- **轻量 XPath 查询引擎**：把路径表达式解析为选择器链，并在任意 `IXmlDocumentTree` 上求值；
- **通用 `XmlElement` 树**：由原始 XML 文本解析得到，另有通过反射水合对象的图写入器。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.application.xml`（根） | `XmlElement`、`XmlParser`、`XsdTools`、`CodeGenerator`、`SchemaExporter`、`XmlValidator` |
| `....application.xml.XmlGeneric` | 通用元素树、图写入器（反射加载对象） |
| `....application.xml.XPath` | 路径表达式模型、解析器与查询执行 |
| `....application.xml.MathML` | MathML 内容标记：表达式模型与内容构建器 |
| `....application.xml.xsd` | XSD 文档对象模型 |

## 关键类型与 API

- `XmlElement` —— 通用元素节点：名称、命名空间、属性、子元素、文本与注释；
- `XmlParser` —— 把 XML 文本解析为通用 `XmlElement` 树（简写 `XmlElement.ParseXmlText`）；
- `XPath.XPath` / `XPath.XPathParser` —— 表达式模型（子节点、根、当前节点、父节点、属性选择器）与其解析器；
- `XPath.XPathQuery` —— 在 `IXmlDocumentTree` 上以 `QuerySingle` / `QueryAll` 执行解析后的表达式；
- `MathML.LambdaExpression` / `MathML.Expression.BinaryExpression` / `SymbolExpression` / `MathFunctionExpression` —— 解析后的数学表达式，带 `ToString` 渲染；
- `MathML.ContentBuilder` —— 把 MathML XML 转换为表达式对象（`ParseXml`）；
- `xsd.schema` / `xsd.element` / `xsd.complexType` / `xsd.simpleType` —— XSD 文档对象模型；
- `XsdTools` —— 三个公开方法：`CreateClassFromXsd`（XSD → VB 类文件）、`CreateXsdFromType` / `CreateXsdFromAssembly`（反射 → XSD 文件）、`ValidateXml` / `IsValid`（XSD 校验 XML）；
- `CodeGenerator` —— 生成 VB 源码文本或写入 `.vb` 文件（`GenerateCode`、`GenerateClassFile`）；
- `SchemaExporter` —— 反射导出 `XmlSchema` 集合 / 单 XSD 文本 / 单类型或整个程序集的 XSD 文件；
- `XmlValidator` / `XsdValidationResult` / `XsdValidationError` —— 流式校验与结构化诊断（`ThrowIfInvalid()`、`Errors`、`ErrorCount`、`Warnings`）；
- `XmlGeneric.GraphWriter` —— 把 XML 载入对象图（`Load`、`LoadXml(Of T)`）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Dim root As XmlElement = XmlParser.ParseXml(xmlText)
Dim f As LambdaExpression = LambdaExpression.FromMathML(root)

Console.WriteLine(f.ToString())
```

### XSD 闭环示例

```vbnet
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.application.xml

' 1) XSD -> 可直接用于 XmlSerializer 的 VB 类文件
XsdTools.CreateClassFromXsd("books.xsd", "Books.vb")

' 2) VB 类 -> XSD 文件（反射）
XsdTools.CreateXsdFromType(GetType(Order), "order.xsd")

' 3) XmlSerializer 生成 XML，再用 XSD 校验
Dim serializer As New XmlSerializer(GetType(Order))
Using output As System.IO.File.Create("order.xml")
    serializer.Serialize(output, myOrder)
End Using

Dim result As XsdValidationResult = XsdTools.ValidateXml("order.xsd", "order.xml")
Console.WriteLine(result.ToString())
result.ThrowIfInvalid()
```

## XSD 工具链

三个能力都集中在 `XsdTools` 模块，能力层分别是 `CodeGenerator` + `SchemaModelBuilder`（XSD → VB）、`SchemaExporter`（反射 → XSD）、`XmlValidator`（校验）。XSD 经 `XmlSchemaSet.Compile()` 归一化后再遍历，因此 `xs:include` / `xs:import`、`xs:group` / `xs:attributeGroup`、继承链都由框架展开，无需重复实现解析器。

### XSD 构造 → 生成的 VB 构造 → 限制

| XSD 构造 | 生成结果 | 限制 / 降级策略 |
|---|---|---|
| 全局 `complexType` | `Public Class` + `<XmlType>` | 无 |
| 全局 `element` 引用具名类型 | `Public Class : Inherits <类型>` + `<XmlRoot>` | 无 |
| 全局 `element` 内联匿名 `complexType` | 顶层匿名类 + 继承它的 `<XmlRoot>` 类 | 无 |
| 全局 `element` 为简单/内置类型 | `<XmlText> Property Value As String` 包装类 | 原始数据类型与约束降级为注释 |
| `simpleType` + `enumeration` | `Public Enum` + `<XmlEnum>` | 其余 facet 降级为注释 |
| `simpleType` + `restriction` / `list` / `union` | 底层 CLR 类型（由编译后的 `XmlSchemaDatatype` 决定） | facet 不由 `XmlSerializer` 强制 |
| `sequence` / `all` | 属性 + `<XmlElement>` | 顺序由声明顺序表达，不生成 `Order` |
| `choice` | `Object` 属性 + 多个 `<XmlElement>` + `<XmlChoiceIdentifier>` + 判别枚举 | 含嵌套 compositor 的 choice 降级为独立成员 |
| `minOccurs="0"` / `nillable` 的值类型元素 | `Nullable(Of T)` | 属性不适用 `Nullable`，改用 `xxxSpecified` 模式 |
| `maxOccurs>1` / `unbounded` | `T()` 数组 | 无 |
| `attribute` / `attributeGroup` | `<XmlAttribute>` 属性 | `use="required"` 不强制；group 由编译器展开 |
| `group` | 由 `XmlSchemaSet` 展开为成员 | 无 |
| `complexContent extension` | `Inherits` + 仅新增成员（按编译后成员差集计算） | 无 |
| `complexContent restriction` | 独立完整类 | 不生成 .NET 继承关系，写入降级注释 |
| `simpleContent extension` | `<XmlText> String` + `<XmlAttribute>` | 底层类型降级为 `String` |
| `substitutionGroup` | 基类上的 `<XmlInclude>` + 派生元素类 | head/substitute 为匿名类型时降级为注释 |
| `xs:any` / `xs:anyAttribute` | `<XmlAnyElement>` / `<XmlAnyAttribute>` | `namespace` / `processContents` 不强制 |
| `include` / `import` | 由受控 `XmlResolver` 解析 | 仅允许主 XSD 目录内的本地文件，拒绝远程与越界路径 |
| `targetNamespace` / `elementFormDefault` | `XmlType` / `XmlRoot` / `XmlElement` 的 `Namespace` 或 `Form` | 无 |
| `abstract` | `MustInherit Class` | 不能直接实例化 |
| `annotation` / `documentation` | `'''` XML 文档注释 | 无 |
| `xs:key` / `keyref` / `unique`、全局 `attribute`、`mixed` 文本节点 | 仅生成注释 | `XmlSerializer` 无法表达 |
| `pattern` / `length` / `min|maxInclusive` / `default` / `fixed` | 仅生成注释 | `XmlSerializer` 不强制约束 |

所有降级项会集中写在生成文件末尾的注释区，保证生成的代码始终可编译、可被 `XmlSerializer` 实例化。

### 校验行为

- 采用 forward-only `XmlReader` 全量读取，时间与文档长度线性相关、内存占用恒定；错误条数默认上限 1000，超出时 `XsdValidationResult.IsTruncated` 为 `True`。
- `DtdProcessing.Prohibit` 且 `XmlResolver = Nothing`，禁止 XXE / 外部实体；根元素找不到 schema 声明时产生的 warning 会被提升为 error，避免「未校验却被判为有效」。
- 校验错误默认只记录在结果里，不抛异常；需要失败即中断时调用 `ThrowIfInvalid()`。

## 实现要点

- **为什么要有通用元素树**：真实世界的 XML 结构千变万化，为每种文档都写一套强类型模型并不现实；`XmlElement` 提供了统一的中间表示，需要强类型时再由 `GraphWriter` 反射水合。
- **XPath 的抽象边界**：把求值目标抽象为 `IXmlDocumentTree`，使得同一套表达式可以作用在 DOM、流式读取器或自研树上。
- **为什么不用 CodeDOM / Roslyn**：`net10.0` 下 `VBCodeProvider` 不提供 VB 代码生成，`XmlSchemaImporter` / `XmlCodeExporter` 也不可用；直接用 `StringBuilder` 发射 VB 源码反而无需新增依赖，且能精确控制 `XmlSerializer` 特性的组合。
- **每个 schema 独立写入**：`XmlSchemaExporter` 可能为一个类型生成多个命名空间，`SchemaExporter` 为每个 `XmlSchema` 使用独立的流和 `XmlWriter`，避免 Core 中 `GenerateXsdFromType` 那种「一个文本里出现多个 XML 声明」的非法输出。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.application.xml`
- TargetFramework：`net10.0`
- Tags：`scibasic;xml;xsd;mathml;xpath;xml-parser;code-generator`
- 许可：GPL-3.0-or-later

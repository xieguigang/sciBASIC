# XML 工具箱：XSD 模型、MathML 与 XPath 查询

## 引言

.NET 自带 `System.Xml`，但在实际工程里仍然缺三样东西：

1. **把 XML 直接灌进对象图**（而不是手写 `XmlReader` 循环）；
2. **对 XML 做轻量查询**（不必引入完整 XSLT / XPath 引擎）；
3. **表达数学公式**（MathML 内容标记 → 可计算的表达式对象）。

本包补齐这三块，并提供 XSD 的强类型对象模型，让「schema 驱动」的代码生成与校验成为可能。

## 设计目标

- **扁平且通用**：`XmlElement` 是一个通用的元素节点（名称、命名空间、属性、子元素、文本、注释），任何 XML 都能承载；
- **反射驱动**：`GraphWriter` 通过反射把 XML 直接填充到普通 .NET 对象，不需要为每个模型写映射；
- **查询与模型解耦**：XPath 只在 `IXmlDocumentTree` 抽象上求值，因此可以作用于任意实现。

## 核心特性

- **XSD 元素数据模型**：schema、element、complexType / simpleType、attribute、sequence、restriction，把 XSD 文档映射为可序列化类；
- **MathML 内容标记**：把 `<math>` / `<apply>` 树解析为表达式对象，或直接从 MathML 文本读出 lambda 表达式；
- **轻量 XPath 查询引擎**：把路径表达式解析为选择器链，并在任意 `IXmlDocumentTree` 上求值；
- **通用 `XmlElement` 树**：由原始 XML 文本解析得到，另有通过反射水合对象的图写入器。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.application.xml`（根） | `XmlElement`、`XmlParser`、`CodeGenerator` |
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
- `XmlGeneric.GraphWriter` —— 把 XML 载入对象图（`Load`、`LoadXml(Of T)`）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.application.xml.MathML

Dim root As XmlElement = XmlParser.ParseXml(xmlText)
Dim f As LambdaExpression = LambdaExpression.FromMathML(root)

Console.WriteLine(f.ToString())
```

## 实现要点

- **为什么要有通用元素树**：真实世界的 XML 结构千变万化，为每种文档都写一套强类型模型并不现实；`XmlElement` 提供了统一的中间表示，需要强类型时再由 `GraphWriter` 反射水合。
- **XPath 的抽象边界**：把求值目标抽象为 `IXmlDocumentTree`，使得同一套表达式可以作用在 DOM、流式读取器或自研树上。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.application.xml`
- TargetFramework：`net10.0-windows;net10.0`
- Tags：`scibasic;xml;xsd;mathml;xpath;xml-parser;code-generator`
- 许可：GPL-3.0-or-later

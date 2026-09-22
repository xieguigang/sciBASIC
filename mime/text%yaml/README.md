# YAML → JSON 对象模型解析器与 Front Matter 读取器

## 引言

YAML 是配置文件的事实标准，但它同时也是**最难解析的常用数据格式之一**：缩进即结构、支持锚点与别名复用、支持块标量（`|` / `>`）与 chomping 指示符、还允许流式集合与多行引号标量混用。

本包的策略是：**把 YAML 转换成 `JsonElement` 对象模型**（来自 `Microsoft.VisualBasic.MIME.application.json`）。这样一来，YAML 与 JSON 共享同一套对象模型，下游代码无需为两种格式各写一遍。

## 设计目标

- **与 JSON 共用模型**：解析结果是 `JsonElement`，因此配置读取、序列化与查询逻辑都能复用；
- **预处理承载复杂度**：多行标量、块标量、注释剥离等难处理的部分集中在一个预处理阶段（`YamlLine` / `MultiLineState`）完成；
- **直接映射到强类型**：提供 `LoadYAML` / `LoadYAMLDocument`，把 YAML 文件或文本直接反序列化为 .NET 类。

## 核心特性

- **块映射 / 块序列 / 嵌套结构**解析为 `JsonElement` 对象；
- **流式集合、块标量**（`|` / `>` 风格，含 `+` / `-` chomping 指示符）与多行引号标量在预处理阶段处理；
- **锚点（`&name`）、别名（`*name`）与合并键（`<<: *name`）**通过每个解析器实例的锚点表解析；引号外的注释被剥离；标量类型自动推断；
- **通用反序列化辅助**：把 YAML 文件或字符串直接映射到 .NET 类；
- **轻量 markdown front matter 读取器**：读取文档顶部 `---` 分隔的元数据块。

## 命名空间地图

| 类型 | 职责 |
|---|---|
| `YamlParser` | 解析器：`Parse` 处理 YAML 文本，`ParseFile` 读取文件，二者均返回 `JsonElement` |
| `YamlLine` | 预处理后的单行 YAML：缩进层级与列表标记 |
| `MultiLineState` | 在预处理阶段跟踪多行字符串收集与块标量 chomping |
| `Serialization` | 模块：`LoadYAML` / `LoadYAMLDocument` 强类型反序列化 |
| `YamlFrontMatterParser` | 模块：`Parse`（front matter → 字典）与 `StripFrontMatter`（只留正文） |

以上类型均位于根命名空间 `Microsoft.VisualBasic.MIME.text.yaml`。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.text.yaml

Dim parser As New YamlParser()
Dim root As JsonElement = parser.ParseFile("config.yml")

' 反序列化为强类型模型
Dim config As AppConfig = Serialization.LoadYAML(Of AppConfig)("config.yml")
Dim fromText As AppConfig = Serialization.LoadYAMLDocument(Of AppConfig)(yamlText)

' markdown front matter
Dim meta As Dictionary(Of String, String) = YamlFrontMatterParser.Parse(markdownText)
Dim body As String = YamlFrontMatterParser.StripFrontMatter(markdownText)
```

## 实现要点

- **预处理是必要的**：YAML 的缩进敏感性意味着不能像 JSON 那样边扫边解析；先把每行规范化为「缩进层级 + 列表标记 + 内容」的三元组（`YamlLine`），后续构建对象树才可能可靠。
- **锚点表的生命周期**：锚点与别名在一个解析器实例内有效，因此复用同一个 `YamlParser` 实例解析互不相关的文档时，锚点不会相互污染。
- **Front matter 的用途**：静态站点生成器与文档工具链常用它承载标题、作者、标签等元数据；本包提供「取出元数据」与「剥离元数据只要正文」两种用法。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.text.yaml`
- TargetFramework：`net10.0`
- Tags：`scibasic;yaml;yaml-parser;front-matter;json;configuration;deserialization`
- 许可：GPL-3.0-or-later

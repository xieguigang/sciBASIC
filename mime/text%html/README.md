# HTML/CSS 解析器、DOM 与 CSS 样式环境

## 引言

从网页里取数据，通常要同时解决两个问题：

1. **结构**：把 HTML 文本变成可查询的 DOM（否则只能用正则去碰运气）；
2. **样式**：很多语义信息藏在 CSS 里（颜色、字号、显隐），要真正「看懂」页面就得解析 CSS 并把它与元素对应起来。

本包提供了完整的这两层：`Token` → `HtmlDocument` DOM → 查询 API，以及 `CssParser` → `Selector` / `CSSFile` → `CSSEnvirnment` 的样式链路。在 `NET48` 构建下还额外包含一套 GDI+ 的 CSS 盒模型布局与绘制引擎。

## 设计目标

- **DOM 查询优先**：提供 `getElementById` / `getElementsByTagName` / `getElementsByClassName` 等熟悉的选择方法；
- **CSS 值可落地**：把 CSS 字符串转成 `Fill`（画刷）、`Stroke`（画笔）、`CSSsize`、`CSSFont`，可直接交给绘图层使用；
- **渲染能力可选**：盒模型布局引擎只在需要绘制时参与编译（`NET48`），因此 `net10.0` 构建保持轻量。

## 核心特性

- **词法与 DOM**：`TokenIcer` 产出 `Token` 流（`HtmlTokens`），`HtmlParser.ParseTree` 构建可查询的 `HtmlDocument`，`HtmlDocument.LoadDocument` 支持从文件或 URL 加载并可剥离标签；
- **DOM 查询**：`HtmlElement` 提供 `getElementById`、`getElementsByName`、`getElementsByTagName`、`getElementsByClassName`、`GetAllChilds`、`GetAllChildsByNodeName`、`hasAttribute`；
- **CSS 解析**：`CssParser.ParseStyle` 与 `CssParser.GetTagWithCSS` 构建 `Selector` 与 `CSSFile` 模型；`CssProperty`、`CSSSelectorTypes`、`HtmlTags` 枚举支持的属性、选择器与标签；
- **CSS 值模型**：`Fill`、`Stroke`、`Padding`、`CSSsize`、`CSSFont`、`CSSEnvirnment` 把 CSS 字符串转换为画刷、画笔、尺寸与字体，供成像层使用；
- **附加工具**：`HtmlCompress`、`DocumentFormatter`（纯文本中的链接 / URL / 邮件高亮）、`StripHTMLDirectly` 文本提取。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MIME.Html`（根） | `DocumentFormatter`、扩展与共享工具 |
| `....Html.Language` | 词法分析器、HTML 解析器、`Token` 模型 |
| `....Html.Language.CSS` | CSS 解析器、`Selector`、`CSSFile` |
| `....Html.Document` / `.Document.Head` / `.Document.Elements` | DOM：`HtmlDocument`、`HtmlElement`、锚点 / 图片 / 文本元素、`<head>` 元数据 |
| `....Html.CSS` / `.CSS.Elements` | 样式环境与元素样式模型（字体、填充、描边、内边距、尺寸） |
| `....Html.Render` / `.Render.CSS` | HTML 渲染器与 CSS 盒模型布局引擎（**仅 NET48**） |
| `....Html.HTML.jQuery` | jQuery 风格的 DOM 链式查询 API |
| `....Html.XmlMeta` | 共享的 XML 节点模型 |

## 关键类型与 API

- `...Language.TokenIcer` —— 把 HTML 源文本切分为 `Token` 流；
- `...Language.HtmlParser` —— 由 HTML 文本构建 `HtmlDocument` DOM；
- `...Document.HtmlDocument` —— DOM 根文档，可从文件句柄或 URL 加载；
- `...Document.HtmlElement` —— DOM 节点：属性、子节点与 id / name / tag / class 查询；
- `...Language.CSS.CssParser` —— 把样式字符串与选择器块解析为 `Selector` / `CSSFile`；
- `...Language.CSS.Selector` —— 一条 CSS 规则：选择器文本、类型与其属性集合；
- `...Html.CSS.CSSEnvirnment` —— 画布上下文（尺寸、DPI、基准字体、基准描边），用于解析 CSS 长度、字体与描边；
- `...Html.Render.HtmlRenderer` —— GDI+ 绘制入口（**仅 NET48**）与 `LoadEnvironment`。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS

Dim html As String = IO.File.ReadAllText("page.html")
Dim doc As HtmlDocument = HtmlParser.ParseTree(html)

For Each link As HtmlElement In doc.getElementsByTagName("a")
    Console.WriteLine(link.ToString())
Next

Dim styles As CSSFile = CssParser.GetTagWithCSS("a { color: #0066cc; }")
Dim rule As Selector = CssParser.ParseStyle("h1 { font-size: 24; font-family: Microsoft YaHei; }")
```

## 实现要点与构建说明

- **两段式解析**：先做纯词法切分（`TokenIcer`），再构建 DOM（`HtmlParser`）；这种分离让 DOM 之外的工具（如压缩器、格式化器）可以直接复用词法层。
- **构建条件**：`Render` 命名空间下的 GDI+ 盒模型布局与绘制引擎（`HtmlRenderer.Render`、`InitialContainer`、`CssBox`、`CssLayoutEngine`、`CssTable`）**只在定义了 `NET48` 常量时参与编译**，因此不属于 `net10.0` 构建的一部分。

## 包信息

- Assembly：`Microsoft.VisualBasic.MIME.Html`
- TargetFramework：`net10.0`
- Tags：`scibasic;html;css;html-parser;dom;css-parser;html-renderer;layout-engine`
- 许可：GPL-3.0-or-later

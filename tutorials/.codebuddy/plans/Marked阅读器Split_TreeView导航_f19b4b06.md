---
name: Marked阅读器Split+TreeView导航
overview: 将 Marked 项目的 TabControl 改造为左右 SplitContainer 布局，并在左侧内嵌 TreeView 导航，实时解析 markdown 标题、点击节点时同步跳转 markdown 文本框与右侧 WebView 渲染区。仅改动 Marked 工程的 FormMarkWeb.vb 与 FormMarkWeb.Designer.vb，不改动共享 markdown 库。
design:
  styleKeywords:
    - 简洁实用
    - 三栏布局
    - 桌面工具风格
    - 清晰层级
  fontSystem:
    fontFamily: Segoe UI
    heading:
      size: 12px
      weight: 600
    subheading:
      size: 12px
      weight: 400
    body:
      size: 12px
      weight: 400
  colorSystem:
    primary:
      - "#0078D7"
    background:
      - "#FFFFFF"
      - "#F0F0F0"
    text:
      - "#000000"
      - "#333333"
    functional:
      - "#0078D7"
      - "#E0E0E0"
todos:
  - id: redesign-layout
    content: 重写 FormMarkWeb.Designer.vb：删除 TabControl，新增 SplitContainer1(左右)/SplitContainer2(左内树+文本)/TreeView1/Timer1 并重新挂载控件
    status: completed
  - id: add-parse-helpers
    content: 在 FormMarkWeb.vb 新增 HeadingInfo 类型与 ParseHeadings/InjectHeadingIds/BuildTree 解析渲染辅助方法
    status: completed
    dependencies:
      - redesign-layout
  - id: wire-rebuild
    content: 实现 Rebuild 串联解析+注入+渲染+建树，并接入 ToolStripButton1_Click、TextBox1_TextChanged 防抖与 FormMarkWeb_Load
    status: completed
    dependencies:
      - add-parse-helpers
  - id: tree-jump
    content: 实现 TreeView1_AfterSelect 双栏跳转：文本框选中滚动 + WebView scrollIntoView（含 NavigationCompleted 兜底）
    status: completed
    dependencies:
      - wire-rebuild
  - id: build-verify
    content: 构建 Marked.vbproj 并手动验证三栏布局、实时同步与标题跳转
    status: completed
    dependencies:
      - tree-jump
---

## 用户需求

将 Marked 项目的 markdown 阅读器从「TabControl 分页」改为「SplitContainer 左右分栏」，并在左侧输入区嵌入 TreeView 标题导航，点击标题节点时同步跳转到 markdown 文本框与右侧 HTML 渲染区对应的标题位置。

## 产品概述

一个 Windows 桌面版 Markdown 阅读器。上方为工具栏（打开 .md 文件），主区域为三栏结构：最左为标题导航树，中间为 Markdown 源文本编辑框，最右为 HTML 渲染预览。打开文件或在文本框中编辑时，导航树与右侧预览实时（防抖）更新；点击任意标题节点，左侧文本框光标定位并滚动到该标题行，右侧预览平滑滚动到对应标题。

## 核心功能

- 左右分栏布局：左侧 Markdown 输入区，右侧 WebView2 HTML 渲染区。
- 左侧内嵌标题导航树：从 Markdown 源按层级（h1-h6，含 ATX 与 setext）解析标题并以树形展示。
- 实时同步：打开文件、在文本框编辑（防抖约 300ms）时重新解析标题、重建树并重新渲染右侧 HTML。
- 双向跳转：点击树节点，左侧文本框选中并滚动到该标题行，右侧预览滚动到对应标题锚点。

## 技术栈

- 语言/框架：VB.NET + Windows Forms（.NET 10，net10.0-windows，UseWPF 工程已开启但本任务仅用 WinForms 控件）。
- 渲染：复用现有 `Microsoft.VisualBasic.MIME.text.markdown` 的 `MarkdownRender`（Shared Project 引用，不改动）与 `G:\galaxy\src\WebViewHelper\WebViewLoader`（WebView2 初始化与导航）。
- 控件：SplitContainer（外层水平 + 左面板内垂直子分栏）、TreeView、TextBox（多行）、WebView2、Timer（防抖）。

## 实现方案

整体策略：保留 `FormMarkWeb` 单一窗体，仅重构其布局与交互逻辑，不触及共享 markdown 库。标题解析与 HTML 锚点 id 注入在本工程内实现，保证「源标题序号」与「渲染 HTML 中 `<h*>` 出现顺序」严格一一对应。

关键决策：

1. **布局**：外层 `SplitContainer1`（Orientation=Horizontal，Dock=Fill，SplitterDistance≈55%）→ Panel1=Markdown 区、Panel2=HTML 区；Panel1 内嵌 `SplitContainer2`（Orientation=Vertical，SplitterDistance≈240）→ Panel1=TreeView1、Panel2=TextBox1。Delete 原 TabControl1/TabPage1/TabPage2。
2. **标题解析自实现**：逐行扫描 Markdown 源，规则与 `MarkdownParser` 对齐——ATX `^#{1,6}\s+(.*?)\s*#*\s*；setext（当前行后紧跟 `=+` 或 `-{2,}` 行）→ h1/h2。每条记录 `(Index, Level, Text, LineStart)`。此集合顺序 = 渲染 HTML 中 `<h*>` 顺序。
3. **HTML id 注入**：`render.Transform(md)` 后用正则 `<(h[1-6])>` 按出现顺序替换为 `<$1 id="md-h{Index}">`，使序号与源码解析索引一致（共享 `HtmlRender.Header` 不带 id，故本工程注入）。
4. **树层级**：用栈按 level 构建父子关系；节点 `Tag` 存 `HeadingInfo`。
5. **跳转**：文本框用 `SelectionStart=LineStart`、`SelectionLength=行长度`、`ScrollToCaret()`、`Focus()`；WebView 用 `Await CoreWebView2.ExecuteScriptAsync("document.getElementById('md-h{Index}').scrollIntoView({behavior:'smooth',block:'start'});")`。因 HTML 经整页导航重载，跳转在 `NavigationCompleted` 后用 `_pendingScrollIndex` 兜底执行。
6. **防抖**：新增 `Timer1`（Interval=300，Enabled=False），`TextBox1.TextChanged` 中 `Timer1.Stop/Start` 重置；`_loading` 标志避免打开文件时的重复链路；首次打开直接调用 `Rebuild()`。

性能与可靠性：`Rebuild` 在编辑停顿 300ms 后仅执行一次，避免逐字符重渲染 WebView（整页导航开销较高）；解析为 O(n) 行扫描 + 一次正则替换，开销极小；仅在文本变化时重渲染，空文档安全降级（清空树、空 HTML）。

## 实现注意事项

- 复用现有 `WebViewLoader.Init/NavigateToLargeString`，不要在本工程重复实现 WebView2 初始化。
- `FormMarkWeb_Load` 中 `Await WebViewLoader.Init(WebView21)` 后，`CoreWebView2` 才可用，再 `AddHandler CoreWebView2.NavigationCompleted`。
- 标题文本需清洗（去除 `**`、`*`、`` ` ``、`~~ `等内联标记）以保证树显示与源码行定位一致；源码行定位直接使用解析时记录的 `LineStart`，无需再次搜索。
- 保持 `ToolStrip1`/`ToolStripButton1` 不变；删除 `TabControl1_SelectedIndexChanged`。
- 避免重复导航：若 `TextBox1.Text` 未变则不触发 `Rebuild` 内的渲染。

## 架构设计

单窗体 Model-Light 结构：UI 控件（SplitContainer/TreeView/TextBox/WebView2）← 交互逻辑（FormMarkWeb.vb）← 解析与渲染辅助（ParseHeadings / InjectHeadingIds / BuildTree / Rebuild）。数据流向：Markdown 文本 → ParseHeadings（源标题+行偏移）→ InjectHeadingIds（带锚点 HTML）→ MarkdownRender.Transform → WebView 导航；同一标题集合 → BuildTree → TreeView；点击节点 → 文本框定位 + WebView 锚点滚动。

## 目录结构

```
G:\pixelArtist\src\framework\tutorials\Marked\
├── FormMarkWeb.Designer.vb   # [MODIFY] 重写 InitializeComponent：删除 TabControl1/TabPage1/TabPage2；新增 SplitContainer1(水平)、SplitContainer2(垂直,左面板内)、TreeView1、Timer1；保留 ToolStrip1/ToolStripButton1/TextBox1/WebView21 并重新挂载到新容器。需在 WithEvents 区声明 SplitContainer1/SplitContainer2/TreeView1/Timer1。
└── FormMarkWeb.vb           # [MODIFY] 删除 TabControl1_SelectedIndexChanged；新增 HeadingInfo 类型、ParseHeadings/InjectHeadingIds/BuildTree/Rebuild 方法，Timer1_Tick、TextBox1_TextChanged、TreeView1_AfterSelect、OnNavigationCompleted 事件处理；改造 ToolStripButton1_Click 与 FormMarkWeb_Load 接入新逻辑。
```

## 关键代码结构

```
Public Class HeadingInfo
    Public Index As Integer      ' 标题序号，与渲染 HTML 中 <h*> 出现顺序一致
    Public Level As Integer      ' 1..6
    Public Text As String        ' 清洗后的标题文本（用于树显示）
    Public LineStart As Integer  ' 该标题在 Markdown 源中的行起始字符偏移（用于文本框跳转）
End Class
```

## 设计风格

沿用现有 Windows 桌面程序风格，保持简洁实用的三栏编辑/预览布局。外层水平分栏左为编辑区、右为预览区；左侧再垂直分栏为「窄树导航 + 文本编辑」。TreeView 采用系统默认树形视觉，选中项高亮；TextBox 等宽可读字体；WebView2 白色背景渲染。整体强调信息层级清晰、操作直观，不做花哨装饰，符合工具类桌面软件定位。

## 布局分块

- 顶部工具栏（ToolStrip）：仅保留「打开」按钮，固定高度 25px。
- 主区域三栏：最左 TreeView（宽度约 240px，Dock Fill 于左子面板）；中间 TextBox（多行、无边框、Segoe UI 12pt、双滚动条、Dock Fill）；最右 WebView2（Dock Fill，白色背景）。
- 分隔条可拖拽，树与文本、文本与预览之间均可通过 SplitterDistance 调整比例。
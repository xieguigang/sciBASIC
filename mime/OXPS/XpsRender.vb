' ============================================================================
' XpsRender.vb - XPS / OXPS 报告渲染器
'
' 统一报告接口 ApplicationServices.Report.ReportRender 在 OPC 包格式上的实现，
' 与 WordRender（.docx）、PdfRender（.pdf）同构：
'
'   - Document：本次渲染写入的 XpsDocument 实例（未设置时自动创建）；
'   - DocumentTitle：正文之前的文档大标题；
'   - TableOfContents：正文之前的静态目录；
'   - Flavor / FontFile / EmbedFont：输出方言与字体嵌入策略；
'   - Author/Title/Subject/Description/Tags：包内文档属性（docProps/core.xml）；
'   - Render：把内容块写入文档；Save：排版并输出 .oxps / .xps。
'
' 使用示例：
'   Dim render As New XpsRender(XpsFlavor.Oxps) With {
'       .DocumentTitle = "2024 年度实验数据分析报告",
'       .TableOfContents = True,
'       .Author = "科研数据分析系统"
'   }
'
'   Call render.Render(blocks)
'   Call render.Save("./report.oxps")
'
' 也可以一句话生成报告文件：
'   Call XpsRender.Generate(blocks, "./report.oxps", "2024 年度实验数据分析报告")
' ============================================================================

Imports Microsoft.VisualBasic.ApplicationServices.Report

''' <summary>
''' XPS / OXPS 报告渲染器：把 markdown 内容块（<see cref="Block"/>）渲染为
''' XPS/OXPS 固定版式文档，是 <see cref="ReportRender"/> 在 OPC 包格式上的实现。
''' </summary>
''' <remarks>
''' 输出方言、字体与元数据在文档第一次被访问（<see cref="Render"/>、<see cref="Save"/>
''' 或读取 <see cref="Document"/>）时写入且只写入一次；之后多次调用 <see cref="Render"/>
''' 会在同一文档中顺序追加内容，最终由一次 <see cref="Save"/> 排版落盘。
''' </remarks>
Public Class XpsRender : Inherits ReportRender

    ''' <summary>输出方言（默认 OpenXPS）。</summary>
    Public Property Flavor As XpsFlavor = XpsFlavor.Oxps

    ''' <summary>可选的字体文件路径（.ttf）；为空时自动查找系统字体。</summary>
    Public Property FontFile As String = Nothing

    ''' <summary>是否嵌入字体；False 时输出不嵌字体的轻量文档。</summary>
    Public Property EmbedFont As Boolean = True

    ''' <summary>文档大标题。非空时在正文内容之前写入。</summary>
    Public Property DocumentTitle As String = ""

    ''' <summary>是否在正文内容之前插入静态目录。</summary>
    Public Property TableOfContents As Boolean = False

    ''' <summary>目录收集的最大标题层级（1-6），仅在 <see cref="TableOfContents"/> 为 True 时生效。</summary>
    Public Property TocMaxLevel As Integer = 3

    ' === 文档元数据（写入 docProps/core.xml） ===

    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""
    ''' <summary>文档标题。</summary>
    Public Property Title As String = ""
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""
    ''' <summary>文档关键字。</summary>
    Public Property Tags As String() = {}

    Private _document As XpsDocument
    Private _preludeApplied As Boolean

    ''' <summary>创建使用默认配置（A4、OXPS）的报告渲染器。</summary>
    Public Sub New()
    End Sub

    ''' <summary>创建指定输出方言的报告渲染器。</summary>
    ''' <param name="flavor">输出方言。</param>
    Public Sub New(flavor As XpsFlavor)
        Me.Flavor = flavor
    End Sub

    ''' <summary>基于已有的 <see cref="XpsDocument"/> 实例创建报告渲染器。</summary>
    ''' <param name="doc">待写入内容的文档实例，可在其中预先设置样式与页面。</param>
    Public Sub New(doc As XpsDocument)
        _document = doc
    End Sub

    ''' <summary>
    ''' 本次渲染所写入的 XPS 文档实例。未显式设置时会自动创建；方言、字体与
    ''' 文前内容（大标题 / 目录）也在这里写入。
    ''' </summary>
    ''' <returns></returns>
    Public Property Document As XpsDocument
        Get
            Return EnsureDocument()
        End Get
        Set(value As XpsDocument)
            _document = value
            _preludeApplied = False
        End Set
    End Property

    ''' <summary>
    ''' 把给定的 markdown 内容块写入 <see cref="Document"/>。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    Public Overrides Sub Render(report As IEnumerable(Of Block))
        Dim doc As XpsDocument = EnsureDocument()

        If report Is Nothing Then
            Return
        End If

        Call doc.WriteBlocks(report)
    End Sub

    ''' <summary>
    ''' 排版并保存为 .oxps / .xps 文件（目录不存在时自动创建）。
    ''' </summary>
    ''' <param name="filePath">输出文件路径；扩展名建议与 <see cref="Flavor"/> 一致。</param>
    Public Sub Save(filePath As String)
        Call EnsureDocument().Save(filePath)
    End Sub

    ''' <summary>
    ''' 文档初始化：创建文档实例、透传方言与字体配置、写入文档大标题与目录。
    ''' 该操作只会执行一次，后续渲染在同一文档中追加内容。
    ''' </summary>
    Private Function EnsureDocument() As XpsDocument
        If _document Is Nothing Then
            _document = New XpsDocument(Author, Title, Tags, Subject, Description) With {
                .Flavor = Flavor,
                .FontFile = FontFile,
                .EmbedFont = EmbedFont
            }
            _preludeApplied = False
        End If

        If Not _preludeApplied Then
            If Not String.IsNullOrEmpty(DocumentTitle) Then
                Call _document.DocTitle(DocumentTitle)
            End If
            If TableOfContents Then
                Call _document.Toc(TocMaxLevel)
            End If

            _preludeApplied = True
        End If

        Return _document
    End Function

    ''' <summary>
    ''' 一次性把给定的 markdown 内容块渲染并保存为 XPS/OXPS 报告文件。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    ''' <param name="filePath">输出的 .oxps / .xps 文件路径。</param>
    ''' <param name="documentTitle">可选的文档大标题，非空时写在正文之前。</param>
    ''' <param name="flavor">输出方言，默认 OpenXPS。</param>
    ''' <returns>已保存的 <see cref="XpsDocument"/> 实例。</returns>
    Public Shared Function Generate(report As IEnumerable(Of Block),
                                   filePath As String,
                                   Optional documentTitle As String = "",
                                   Optional flavor As XpsFlavor = XpsFlavor.Oxps) As XpsDocument
        Dim render As New XpsRender(flavor) With {.DocumentTitle = If(documentTitle, "")}

        Call render.Render(report)
        Call render.Save(filePath)

        Return render.Document
    End Function
End Class

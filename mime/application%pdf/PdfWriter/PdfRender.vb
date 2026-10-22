' ============================================================================
' PdfRender.vb - PDF 报告渲染器
'
' 将 markdown 内容块模型（Report.Block）渲染为 PDF 文档，是 ReportRender
' 在二进制文档格式上的实现（与 docx 的 WordRender 同构）：
'
'   - Document：本次渲染写入的 PdfDocument 实例（未设置时自动创建）；
'   - DocumentTitle：正文之前的文档大标题（PdfDocument.DocTitle）；
'   - TableOfContents：正文之前的目录（PdfDocument.Toc）；
'   - Author/Title/Subject/Description/Tags：写入 PDF 文档信息字典的元数据；
'   - Render：把内容块写入文档；Save：排版并输出 .pdf 文件。
'
' 使用示例：
'   Dim blocks As IEnumerable(Of Block) = JsonReport.Parse(json)
'   Dim render As New PdfRender() With {
'       .DocumentTitle = "2024 年度实验数据分析报告",
'       .TableOfContents = True,
'       .Author = "科研数据分析系统"
'   }
'
'   Call render.Render(blocks)
'   Call render.Save("./report.pdf")
'
' 也可以一句话生成报告文件：
'   Call PdfRender.Generate(blocks, "./report.pdf", "2024 年度实验数据分析报告")
'
' 与其它渲染器的对照：
'   - HtmlRender / TextRender → 文本格式（TextReportRender，输出 Output 字符串）；
'   - RtfRender               → RTF 文本（TextReportRender）；
'   - WordRender              → .docx 二进制包（ReportRender）；
'   - PdfRender               → .pdf 二进制文档（ReportRender）。
' ============================================================================

Imports Microsoft.VisualBasic.ApplicationServices.Report

''' <summary>
''' PDF 报告渲染器：把 markdown 内容块（<see cref="Block"/>）渲染为 PDF 文档，
''' 是 <see cref="ReportRender"/> 在 PDF 二进制文档格式上的实现。
''' </summary>
''' <remarks>
''' 文档题目与目录会在文档第一次被访问（<see cref="Render"/>、<see cref="Save"/> 或读取
''' <see cref="Document"/>）时写入且只写入一次；之后多次调用 <see cref="Render"/> 会在
''' 同一文档中顺序追加内容，最终由一次 <see cref="Save"/> 排版分页并落盘。
''' </remarks>
Public Class PdfRender : Inherits ReportRender

    ''' <summary>
    ''' 文档大标题（首页标题）。非空时在正文内容之前写入
    ''' （<see cref="PdfDocument.DocTitle"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property DocumentTitle As String = ""

    ''' <summary>
    ''' 是否在正文内容之前插入目录（<see cref="PdfDocument.Toc"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property TableOfContents As Boolean = False

    ''' <summary>
    ''' 目录收集的最大标题层级（取值 1-6），仅在 <see cref="TableOfContents"/> 为 True 时生效。
    ''' </summary>
    ''' <returns></returns>
    Public Property TocMaxLevel As Integer = 3

    ' === 文档元数据（写入 PDF 的 document information 字典） ===

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

    Private _document As PdfDocument
    Private _preludeApplied As Boolean

    ''' <summary>创建一个使用 <see cref="PdfDocument"/> 默认样式的 PDF 报告渲染器。</summary>
    Public Sub New()
    End Sub

    ''' <summary>基于已有的 <see cref="PdfDocument"/> 实例创建 PDF 报告渲染器。</summary>
    ''' <param name="doc">待写入内容的文档实例，可在其中预先设置样式与页面。</param>
    Public Sub New(doc As PdfDocument)
        _document = doc
    End Sub

    ''' <summary>
    ''' 本次渲染所写入的 PDF 文档实例。未显式设置时会自动创建；文档题目与目录的
    ''' 初始化动作也在这里完成。
    ''' </summary>
    ''' <returns></returns>
    Public Property Document As PdfDocument
        Get
            Return EnsureDocument()
        End Get
        Set(value As PdfDocument)
            _document = value
            _preludeApplied = False
        End Set
    End Property

    ''' <summary>
    ''' 将给定的 markdown 内容块写入 <see cref="Document"/>。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    Public Overrides Sub Render(report As IEnumerable(Of Block))
        Dim doc As PdfDocument = EnsureDocument()

        If report Is Nothing Then
            Return
        End If

        Call doc.WriteBlocks(report)
    End Sub

    ''' <summary>
    ''' 排版并保存为 .pdf 文件（目录不存在时自动创建）。
    ''' </summary>
    ''' <param name="filePath">输出的 .pdf 文件路径。</param>
    Public Sub Save(filePath As String)
        Call EnsureDocument().Save(filePath)
    End Sub

    ''' <summary>
    ''' 文档初始化：创建文档实例、写入文档大标题和目录。该操作只会执行一次，
    ''' 后续渲染在同一文档中追加内容。
    ''' </summary>
    Private Function EnsureDocument() As PdfDocument
        If _document Is Nothing Then
            _document = New PdfDocument(Author, Title, Tags, Subject, Description)
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
    ''' 一次性把给定的 markdown 内容块渲染并保存为 PDF 报告文件。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    ''' <param name="filePath">输出的 .pdf 文件路径。</param>
    ''' <param name="documentTitle">可选的文档大标题，非空时写在正文之前。</param>
    ''' <returns>已保存的 <see cref="PdfDocument"/> 实例。</returns>
    Public Shared Function Generate(report As IEnumerable(Of Block),
                                   filePath As String,
                                   Optional documentTitle As String = "") As PdfDocument
        Dim render As New PdfRender() With {.DocumentTitle = If(documentTitle, "")}

        Call render.Render(report)
        Call render.Save(filePath)

        Return render.Document
    End Function
End Class

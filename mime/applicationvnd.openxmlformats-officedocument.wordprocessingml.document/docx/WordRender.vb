' ============================================================================
' WordRender.vb - word 报告渲染器
'
' 将 markdown 内容块模型（Report.Block）渲染为 microsoft office word 文档，
' 是 ReportRender 在二进制文档格式上的实现：
'
'   - Theme：版式主题，可直接套用 ThesisFormatting（学位论文格式）或自定义主题；
'   - DocumentTitle：正文之前的文档题目（WordDocument.DocTitle）；
'   - TableOfContents：正文之前的目录（WordDocument.Toc）；
'   - Author/Title/Subject/Description/Tags：写入 docx 包属性的文档元数据；
'   - Render：把 Block 序列写入文档；Save：保存为 .docx 文件。
'
' 使用示例：
'   Dim blocks As IEnumerable(Of Block) = JsonReport.Parse(json)
'   Dim render As New WordRender(New ThesisFormatting())
'
'   render.DocumentTitle = "基于转录组测序的差异表达基因分析"
'   render.TableOfContents = True
'   render.Author = "某某某"
'
'   Call render.Render(blocks)
'   Call render.Save("./thesis.docx")
'
' 也可以一句话生成报告文件：
'   Call WordRender.Generate(blocks, "./thesis.docx", New ThesisFormatting())
' ============================================================================

Imports Microsoft.VisualBasic.ApplicationServices.Report

''' <summary>
''' word 报告渲染器：将 markdown 内容块（<see cref="Block"/>）渲染为 microsoft office
''' word 文档，是 <see cref="ReportRender"/> 在二进制文档格式上的实现。
''' 通过 <see cref="Theme"/> 属性可以套用 <see cref="ThesisFormatting"/> 等版式主题。
''' </summary>
''' <remarks>
''' 主题、文档题目和目录会在文档第一次被访问（<see cref="Render"/>、<see cref="Save"/>
''' 或读取 <see cref="Document"/>）时写入文档，且只写入一次；之后多次调用
''' <see cref="Render"/> 会在同一个文档中顺序追加内容。
''' </remarks>
Public Class WordRender : Inherits ReportRender

    ''' <summary>
    ''' 文档版式主题。非空时在文档初始化阶段通过 <see cref="ThesisFormatting.ApplyTo"/>
    ''' 应用到 <see cref="Document"/>（若 <see cref="Document"/> 是外部提供的实例，则主题会
    ''' 覆盖其样式设置）；Nothing 表示使用 <see cref="WordDocument"/> 的内置默认样式。
    ''' </summary>
    ''' <returns></returns>
    Public Property Theme As ThesisFormatting

    ''' <summary>
    ''' 文档题目（封面/首页标题）。非空时在正文内容之前写入
    ''' （<see cref="WordDocument.DocTitle"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property DocumentTitle As String = ""

    ''' <summary>
    ''' 是否在正文内容之前插入目录（<see cref="WordDocument.Toc"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property TableOfContents As Boolean = False

    ''' <summary>
    ''' 目录收集的最大标题层级（取值 1-6），仅在 <see cref="TableOfContents"/> 为 True 时生效。
    ''' </summary>
    ''' <returns></returns>
    Public Property TocMaxLevel As Integer = 3

    ' === 文档元数据（写入 docx 包属性） ===

    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""
    ''' <summary>文档标题（docx 包属性中的 title，与 <see cref="DocumentTitle"/> 无关）。</summary>
    Public Property Title As String = ""
    ''' <summary>文档主题。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述。</summary>
    Public Property Description As String = ""
    ''' <summary>文档关键词。</summary>
    Public Property Tags As String() = {}

    Private _document As WordDocument
    Private _preludeApplied As Boolean

    ''' <summary>创建一个使用 <see cref="WordDocument"/> 默认样式的 word 报告渲染器。</summary>
    Public Sub New()
    End Sub

    ''' <summary>创建一个应用了指定版式主题的 word 报告渲染器。</summary>
    ''' <param name="theme">版式主题，例如 <see cref="ThesisFormatting"/>；Nothing 表示默认样式。</param>
    Public Sub New(theme As ThesisFormatting)
        Me.Theme = theme
    End Sub

    ''' <summary>基于已有的 <see cref="WordDocument"/> 实例创建 word 报告渲染器。</summary>
    ''' <param name="doc">待写入内容的文档实例。</param>
    ''' <param name="theme">可选的主题，会在第一次渲染时覆盖该文档的样式。</param>
    Public Sub New(doc As WordDocument, Optional theme As ThesisFormatting = Nothing)
        _document = doc
        Me.Theme = theme
    End Sub

    ''' <summary>
    ''' 本次渲染所写入的 word 文档实例。未显式设置时会自动创建；应用主题、文档题目
    ''' 和目录的初始化动作也在这里完成。
    ''' </summary>
    ''' <returns></returns>
    Public Property Document As WordDocument
        Get
            Return EnsureDocument()
        End Get
        Set(value As WordDocument)
            _document = value
            _preludeApplied = False
        End Set
    End Property

    ''' <summary>
    ''' 将给定的 markdown 内容块渲染（写入）到 <see cref="Document"/>。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    Public Overrides Sub Render(report As IEnumerable(Of Block))
        Dim doc As WordDocument = EnsureDocument()

        If report Is Nothing Then
            Return
        End If

        Call doc.WriteBlocks(report)
    End Sub

    ''' <summary>
    ''' 将当前文档保存为 .docx 文件。
    ''' </summary>
    ''' <param name="filePath">输出的文件路径，扩展名应为 .docx。</param>
    Public Sub Save(filePath As String)
        Call EnsureDocument().Save(filePath)
    End Sub

    ''' <summary>
    ''' 文档初始化：创建文档实例、应用主题、写入文档题目和目录。
    ''' 该操作只会执行一次，后续渲染在同一文档中追加内容。
    ''' </summary>
    Private Function EnsureDocument() As WordDocument
        If _document Is Nothing Then
            _document = New WordDocument(Author, Title, Tags, Subject, Description)
            _preludeApplied = False
        End If

        If Not _preludeApplied Then
            Dim doc As WordDocument = _document

            If Theme IsNot Nothing Then
                Call Theme.ApplyTo(doc)
            End If
            If Not String.IsNullOrEmpty(DocumentTitle) Then
                Call doc.DocTitle(DocumentTitle)
            End If
            If TableOfContents Then
                Call doc.Toc(TocMaxLevel)
            End If

            _preludeApplied = True
        End If

        Return _document
    End Function

    ''' <summary>
    ''' 一次性把给定的 markdown 内容块渲染并保存为 word 报告文件。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    ''' <param name="filePath">输出的 .docx 文件路径。</param>
    ''' <param name="theme">可选的版式主题，例如 <see cref="ThesisFormatting"/>。</param>
    ''' <param name="documentTitle">可选的文档题目，非空时写在正文之前。</param>
    ''' <returns>已保存的 <see cref="WordDocument"/> 实例。</returns>
    Public Shared Function Generate(report As IEnumerable(Of Block),
                                   filePath As String,
                                   Optional theme As ThesisFormatting = Nothing,
                                   Optional documentTitle As String = "") As WordDocument
        Dim render As New WordRender(theme) With {.DocumentTitle = If(documentTitle, "")}

        Call render.Render(report)
        Call render.Save(filePath)

        Return render.Document
    End Function
End Class

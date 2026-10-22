' ============================================================================
' RtfRender.vb - RTF 报告渲染器
'
' 将 markdown 内容块模型（Report.Block）渲染为 RTF 文档文本，是
' ReportRender 在富文本格式（RTF 1.x）上的实现：RTF 本身是文本格式，因此
' 本渲染器继承 TextReportRender，序列化结果直接通过 Output 属性输出，
' 需要落盘时再调用 Save 写入文件。
'
' 与 WordRender 的分工：
'   - WordRender  → 二进制 .docx 包（ReportRender 直接子类）；
'   - RtfRender    → RTF 文本（TextReportRender 子类，Output 即 .rtf 文件内容）。
'
' 使用示例：
'   Dim render As New RtfRender() With {
'       .DocumentTitle = "实验数据分析报告",
'       .TableOfContents = True,
'       .Author = "sciBASIC#"
'   }
'
'   Call render.Render(blocks)
'   Console.WriteLine(render.Output)    ' 完整的 RTF 文档文本
'   Call render.Save("./report.rtf")
'
' 也可以一句话生成报告文件：
'   Call RtfRender.Generate(blocks, "./report.rtf", "实验数据分析报告")
' ============================================================================

Imports Microsoft.VisualBasic.ApplicationServices.Report

''' <summary>
''' RTF 报告渲染器：把 markdown 内容块（<see cref="Block"/>）渲染为 RTF 1.x 文档文本，
''' 是 <see cref="ReportRender"/> 在富文本格式上的实现（继承 <see cref="TextReportRender"/>，
''' 序列化结果由 <see cref="TextReportRender.Output"/> 输出）。
''' </summary>
''' <remarks>
''' 渲染时先由 <see cref="RtfDocument"/> 收集内容块，再经 <see cref="RtfSerializer"/>
''' 序列化为完整 RTF 文本；文档题目与目录会在文档第一次被访问时写入且只写入一次，
''' 之后多次调用 <see cref="Render"/> 会在同一文档中顺序追加内容。
''' </remarks>
Public Class RtfRender : Inherits TextReportRender

    ''' <summary>
    ''' 文档题目（首页标题）。非空时在正文内容之前写入
    ''' （<see cref="RtfDocument.DocTitle"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property DocumentTitle As String = ""

    ''' <summary>
    ''' 是否在正文内容之前插入静态目录（<see cref="RtfDocument.Toc"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property TableOfContents As Boolean = False

    ''' <summary>
    ''' 目录收集的最大标题层级（取值 1-6），仅在 <see cref="TableOfContents"/> 为 True 时生效。
    ''' </summary>
    ''' <returns></returns>
    Public Property TocMaxLevel As Integer = 3

    ' === 文档元数据（写入 RTF \info 分组） ===

    ''' <summary>文档作者。</summary>
    Public Property Author As String = ""
    ''' <summary>文档标题（\info 的 title）。</summary>
    Public Property Title As String = ""
    ''' <summary>文档主题（\info 的 subject）。</summary>
    Public Property Subject As String = ""
    ''' <summary>文档描述（\info 的 doccomm）。</summary>
    Public Property Description As String = ""
    ''' <summary>文档关键字（\info 的 keywords），多个关键字以逗号分隔写入。</summary>
    Public Property Tags As String() = {}

    Private _document As RtfDocument
    Private _preludeApplied As Boolean

    ''' <summary>创建一个使用 <see cref="RtfDocument"/> 默认样式的 RTF 报告渲染器。</summary>
    Public Sub New()
    End Sub

    ''' <summary>基于已有的 <see cref="RtfDocument"/> 实例创建 RTF 报告渲染器。</summary>
    ''' <param name="doc">待写入内容的文档实例，可在其中预先设置样式与页面。</param>
    Public Sub New(doc As RtfDocument)
        _document = doc
    End Sub

    ''' <summary>
    ''' 本次渲染所写入的 RTF 文档实例。未显式设置时会自动创建；文档题目与目录的
    ''' 初始化动作也在这里完成。
    ''' </summary>
    ''' <returns></returns>
    Public Property Document As RtfDocument
        Get
            Return EnsureDocument()
        End Get
        Set(value As RtfDocument)
            _document = value
            _preludeApplied = False
        End Set
    End Property

    ''' <summary>
    ''' 将给定的 markdown 内容块写入 <see cref="Document"/>，并把整份文档序列化为
    ''' RTF 文本输出到 <see cref="TextReportRender.Output"/>。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    Public Overrides Sub Render(report As IEnumerable(Of Block))
        Dim doc As RtfDocument = EnsureDocument()

        If report IsNot Nothing Then
            Call doc.WriteBlocks(report)
        End If

        Call RefreshOutput()
    End Sub

    ''' <summary>
    ''' 将 <see cref="TextReportRender.Output"/> 中的 RTF 文本写入 .rtf 文件
    ''' （目录不存在时自动创建）。
    ''' </summary>
    ''' <param name="filePath">输出的 .rtf 文件路径。</param>
    Public Sub Save(filePath As String)
        Call RefreshOutput()
        Call RtfSerializer.SaveText(filePath, Me.Output)
    End Sub

    ''' <summary>
    ''' 文档初始化：创建文档实例、写入文档题目和目录。该操作只会执行一次，
    ''' 后续渲染在同一文档中追加内容。
    ''' </summary>
    Private Function EnsureDocument() As RtfDocument
        If _document Is Nothing Then
            _document = New RtfDocument(Author, Title, Tags, Subject, Description)
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

    ''' <summary>把当前文档重新序列化到文本缓冲区（<see cref="TextReportRender.Output"/>）。</summary>
    Private Sub RefreshOutput()
        Call buffer.Clear()
        Call buffer.Append(EnsureDocument().ToRtfText())
    End Sub

    ''' <summary>
    ''' 一次性把给定的 markdown 内容块渲染并保存为 RTF 报告文件。
    ''' </summary>
    ''' <param name="report">the markdown content blocks of the target document</param>
    ''' <param name="filePath">输出的 .rtf 文件路径。</param>
    ''' <param name="documentTitle">可选的文档题目，非空时写在正文之前。</param>
    ''' <returns>已保存的 <see cref="RtfDocument"/> 实例。</returns>
    Public Shared Function Generate(report As IEnumerable(Of Block),
                                   filePath As String,
                                   Optional documentTitle As String = "") As RtfDocument
        Dim render As New RtfRender() With {.DocumentTitle = If(documentTitle, "")}

        Call render.Render(report)
        Call render.Save(filePath)

        Return render.Document
    End Function
End Class

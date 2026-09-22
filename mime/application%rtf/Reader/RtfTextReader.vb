' ============================================================================
' RtfTextReader.vb - RTF 文本 / 元数据读取器
'
' 对外契约：整篇纯文本、按段落切分的文本数组、文档元数据（标题/作者/生成程序等）
' 以及逐段产出的流式入口，风格与 docx 的 DocxTextReader、pdf 的 PDF.GetText 一致。
'
' 解析由 RtfLexer 完成；本类只负责文件/流的读取与结果暴露。
' 文件不存在抛 FileNotFoundException；文档内容损坏时不抛异常，返回已解析出的部分。
' ============================================================================

Imports System.IO

''' <summary>
''' RTF 文档元数据（对应 RTF 的 \info 组与 \*\generator）。
''' 缺失字段为空字符串，不会抛出异常。
''' </summary>
Public Class RtfDocumentInfo

    ''' <summary>文档标题（\info\title）。</summary>
    Public Property Title As String = ""
    ''' <summary>文档作者（\info\author）。</summary>
    Public Property Author As String = ""
    ''' <summary>文档主题（\info\subject）。</summary>
    Public Property Subject As String = ""
    ''' <summary>关键字（\info\keywords）。</summary>
    Public Property Keywords As String = ""
    ''' <summary>公司（\info\company）。</summary>
    Public Property Company As String = ""
    ''' <summary>备注（\info\doccomm）。</summary>
    Public Property Comments As String = ""
    ''' <summary>生成该文档的应用程序（\*\generator）。</summary>
    Public Property Generator As String = ""

    Public Overrides Function ToString() As String
        Return $"Title=""{Title}"", Author=""{Author}"", Generator=""{Generator}"""
    End Function

End Class

''' <summary>
''' RTF 文本提取器：从 .rtf 文件或 RTF 文本流中读取正文。
''' </summary>
''' <example>
''' <code>
''' Dim reader As New RtfTextReader()
''' Dim text As String = reader.ExtractText("demo.rtf")
''' Dim paragraphs As String() = reader.ExtractParagraphs("demo.rtf")
''' Dim info As RtfDocumentInfo = reader.ExtractMetadata("demo.rtf")
''' </code>
''' </example>
Public Class RtfTextReader

    ' ========================================================================
    ' 纯文本
    ' ========================================================================

    ''' <summary>
    ''' 从 .rtf 文件中提取纯文本（段落以换行分隔）。
    ''' </summary>
    ''' <param name="filePath">.rtf 文件路径。</param>
    ''' <exception cref="FileNotFoundException">文件不存在。</exception>
    Public Function ExtractText(filePath As String) As String
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"文件不存在: {filePath}", filePath)
        End If

        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            Return ExtractText(stream)
        End Using
    End Function

    ''' <summary>
    ''' 从 RTF 文本流中提取纯文本（段落以换行分隔）。
    ''' </summary>
    Public Function ExtractText(stream As Stream) As String
        Return New RtfLexer(ReadAllBytes(stream)).GetText()
    End Function

    ' ========================================================================
    ' 段落
    ' ========================================================================

    ''' <summary>
    ''' 从 .rtf 文件中提取按段落切分的文本数组。
    ''' </summary>
    ''' <param name="filePath">.rtf 文件路径。</param>
    ''' <exception cref="FileNotFoundException">文件不存在。</exception>
    Public Function ExtractParagraphs(filePath As String) As String()
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"文件不存在: {filePath}", filePath)
        End If

        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            Return ExtractParagraphs(stream)
        End Using
    End Function

    ''' <summary>
    ''' 从 RTF 文本流中提取按段落切分的文本数组。
    ''' </summary>
    ''' <remarks>段落内的软换行（\line）保留在对应段落文本中；尾部的空段落会被去除。</remarks>
    Public Function ExtractParagraphs(stream As Stream) As String()
        Return New RtfLexer(ReadAllBytes(stream)).GetParagraphs()
    End Function

    ' ========================================================================
    ' 元数据
    ' ========================================================================

    ''' <summary>
    ''' 读取 .rtf 文件的文档元数据（标题 / 作者 / 主题 / 关键字 / 公司 / 备注 / 生成程序）。
    ''' </summary>
    ''' <param name="filePath">.rtf 文件路径。</param>
    ''' <exception cref="FileNotFoundException">文件不存在。</exception>
    Public Function ExtractMetadata(filePath As String) As RtfDocumentInfo
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"文件不存在: {filePath}", filePath)
        End If

        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            Return ExtractMetadata(stream)
        End Using
    End Function

    ''' <summary>读取 RTF 文本流的文档元数据。</summary>
    Public Function ExtractMetadata(coreDoc As Stream) As RtfDocumentInfo
        Return New RtfLexer(ReadAllBytes(coreDoc)).GetInfo()
    End Function

    ' ========================================================================
    ' 流式入口（与 pdf 侧 PDF.GetText 对应）
    ' ========================================================================

    ''' <summary>逐段产出 .rtf 文件中的文本。</summary>
    ''' <param name="filePath">.rtf 文件路径。</param>
    ''' <exception cref="FileNotFoundException">文件不存在。</exception>
    Public Shared Iterator Function GetText(filePath As String) As IEnumerable(Of String)
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"文件不存在: {filePath}", filePath)
        End If

        Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            For Each paragraph As String In GetText(stream)
                Yield paragraph
            Next
        End Using
    End Function

    ''' <summary>逐段产出 RTF 文本流中的文本。</summary>
    Public Shared Iterator Function GetText(stream As Stream) As IEnumerable(Of String)
        For Each paragraph As String In New RtfLexer(ReadAllBytes(stream)).GetParagraphs()
            Yield paragraph
        Next
    End Function

    ' ========================================================================
    ' 辅助
    ' ========================================================================

    Private Shared Function ReadAllBytes(stream As Stream) As Byte()
        If stream Is Nothing Then Return New Byte() {}

        If TypeOf stream Is MemoryStream Then
            Return DirectCast(stream, MemoryStream).ToArray()
        End If

        Using ms As New MemoryStream()
            Call stream.CopyTo(ms)
            Return ms.ToArray()
        End Using
    End Function

End Class

' ============================================================================
' DocxTextReader.vb - 从 .docx 文件中提取纯文本
'
' 打开 .docx (ZIP 包)，解析 word/document.xml，
' 提取所有 <w:t> 元素的文本内容，并按段落分行。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Text

''' <summary>
''' .docx 文本提取器。
''' </summary>
Public Class DocxTextReader

    ''' <summary>
    ''' 从 .docx 文件中提取纯文本。
    ''' </summary>
    ''' <param name="filePath">.docx 文件路径。</param>
    ''' <returns>纯文本内容，段落以换行符分隔。</returns>
    Public Function ExtractText(filePath As String) As String
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"文件不存在: {filePath}")
        End If

        Using archive As ZipArchive = New ZipArchive(New FileStream(filePath, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read)
            Dim entry As ZipArchiveEntry = archive.GetEntry("word/document.xml")
            If entry Is Nothing Then
                Throw New InvalidOperationException("无效的 .docx 文件: 缺少 word/document.xml")
            End If

            Using es As Stream = entry.Open()
                Dim doc As XDocument = XDocument.Load(es)
                Dim sb As New StringBuilder()

                ' 获取 w 命名空间
                Dim w As XNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"

                ' 遍历 body 中的所有元素
                Dim body As XElement = doc.Root.Element(w + "body")
                If body Is Nothing Then Return ""

                For Each el As XElement In body.Elements()
                    If el.Name = w + "p" Then
                        ' 段落
                        Dim text As String = ExtractParagraphText(el, w)
                        sb.AppendLine(text)
                    ElseIf el.Name = w + "tbl" Then
                        ' 表格
                        Dim tableText As String = ExtractTableText(el, w)
                        sb.Append(tableText)
                    End If
                Next

                Return sb.ToString().TrimEnd()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 从段落元素中提取文本。
    ''' </summary>
    Private Function ExtractParagraphText(p As XElement, w As XNamespace) As String
        Dim sb As New StringBuilder()

        For Each el As XElement In p.Descendants()
            If el.Name = w + "t" Then
                sb.Append(el.Value)
            ElseIf el.Name = w + "br" Then
                ' 换行符
                Dim typeAttr As XAttribute = el.Attribute(w + "type")
                If typeAttr IsNot Nothing AndAlso typeAttr.Value = "page" Then
                    sb.Append(vbFormFeed)
                Else
                    sb.Append(vbLf)
                End If
            ElseIf el.Name = w + "tab" Then
                sb.Append(vbTab)
            End If
        Next

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 从表格元素中提取文本。
    ''' </summary>
    Private Function ExtractTableText(tbl As XElement, w As XNamespace) As String
        Dim sb As New StringBuilder()
        sb.AppendLine()

        For Each tr As XElement In tbl.Elements(w + "tr")
            Dim cells As New List(Of String)()
            For Each tc As XElement In tr.Elements(w + "tc")
                Dim cellText As New StringBuilder()
                For Each p As XElement In tc.Descendants(w + "p")
                    Dim t As String = ExtractParagraphText(p, w)
                    If cellText.Length > 0 Then cellText.Append(" ")
                    cellText.Append(t)
                Next
                cells.Add(cellText.ToString())
            Next
            sb.AppendLine(String.Join(" | ", cells))
        Next

        sb.AppendLine()
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' 提取文本并按段落分割为字符串数组。
    ''' </summary>
    Public Function ExtractParagraphs(filePath As String) As String()
        Dim text As String = ExtractText(filePath)
        Return text.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
    End Function

    ''' <summary>
    ''' 提取文档的元数据 (docProps/core.xml)。
    ''' </summary>
    Public Function ExtractMetadata(filePath As String) As Dictionary(Of String, String)
        Dim result As New Dictionary(Of String, String)()

        Using archive As ZipArchive = New ZipArchive(New FileStream(filePath, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read)
            Dim entry As ZipArchiveEntry = archive.GetEntry("docProps/core.xml")
            If entry Is Nothing Then Return result

            Using es As Stream = entry.Open()
                Dim doc As XDocument = XDocument.Load(es)
                For Each el As XElement In doc.Root.Elements()
                    ' 去掉命名空间前缀，只用本地名称
                    Dim localName As String = el.Name.LocalName
                    result(localName) = el.Value
                Next
            End Using
        End Using

        Return result
    End Function

End Class

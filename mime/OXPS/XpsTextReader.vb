' ============================================================================
' XpsTextReader.vb - XPS / OXPS 纯文本提取
'
' XPS 的页面没有任何「文本元素」，文字全部承载在 <Glyphs UnicodeString="..."> 的
' 属性里。因此提取流程是：解压包 → 定位页面部件 → 逐页读取 Glyphs → 按基线
' 位置把它们重组成行。
'
' 页面定位优先走 OPC 关系链（fdseq → fdoc → fpage），失败时回退为
' 「按 Documents/*/Pages/*.fpage 路径自然排序」，以便容忍第三方生成的文档。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

''' <summary>
''' XPS / OXPS 文本提取器：把固定页面文档还原为纯文本、按页文本、段落数组与
''' 文档属性。API 形态与 <c>DocxTextReader</c> / <c>RtfTextReader</c> 保持一致。
''' </summary>
Public Class XpsTextReader

    ''' <summary>同一行的基线容差（磅）：OriginY 相差不超过该值视为同一行。</summary>
    Public Const LineTolerance As Double = 1.0

    ''' <summary>XPS 控制字符转义（_x0009_ / _x000A_ / _x000D_ 等）。</summary>
    Private Shared ReadOnly CharEscape As New Regex("_x([0-9A-Fa-f]{4})_", RegexOptions.Compiled)

    ''' <summary>容错模式下的 UnicodeString 抓取正则。</summary>
    Private Shared ReadOnly GlyphsFallback As New Regex(
        "UnicodeString\s*=\s*(?:""([^""]*)""|'([^']*)')", RegexOptions.Compiled)

    ' ========================================================================
    ' 纯文本
    ' ========================================================================

    ''' <summary>
    ''' 提取整篇纯文本（按页序，页内按视觉行重组）。
    ''' </summary>
    ''' <param name="filePath">.xps / .oxps 文件路径。</param>
    ''' <returns>纯文本；空文档返回空字符串。</returns>
    Public Function ExtractText(filePath As String) As String
        Return String.Join(vbCrLf, ExtractPages(filePath))
    End Function

    ''' <summary>
    ''' 提取整篇纯文本（按页序，页内按视觉行重组）。
    ''' </summary>
    ''' <param name="stream">包含 xps / oxps 包的可读流。</param>
    ''' <returns>纯文本；空文档返回空字符串。</returns>
    Public Function ExtractText(stream As Stream) As String
        Return String.Join(vbCrLf, ExtractPages(stream))
    End Function

    ''' <summary>
    ''' 按页提取文本。
    ''' </summary>
    ''' <param name="filePath">.xps / .oxps 文件路径。</param>
    ''' <returns>每页一段文本。</returns>
    Public Function ExtractPages(filePath As String) As String()
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"找不到 XPS/OXPS 文件: {filePath}", filePath)
        End If

        Using fs As Stream = File.OpenRead(filePath)
            Return ExtractPages(fs)
        End Using
    End Function

    ''' <summary>
    ''' 按页提取文本。
    ''' </summary>
    ''' <param name="stream">包含 xps / oxps 包的可读流。</param>
    ''' <returns>每页一段文本。</returns>
    Public Function ExtractPages(stream As Stream) As String()
        Dim pages As New List(Of String)()

        Using archive As New ZipArchive(stream, ZipArchiveMode.Read, leaveOpen:=True)
            For Each partName As String In ResolvePageParts(archive)
                Call pages.Add(ExtractPageText(archive, partName))
            Next
        End Using

        Return pages.ToArray()
    End Function

    ''' <summary>
    ''' 按段落（非空文本行）提取整篇内容。
    ''' </summary>
    ''' <param name="filePath">.xps / .oxps 文件路径。</param>
    ''' <returns>段落数组。</returns>
    Public Function ExtractParagraphs(filePath As String) As String()
        Return ToParagraphs(ExtractText(filePath))
    End Function

    ''' <summary>
    ''' 按段落（非空文本行）提取整篇内容。
    ''' </summary>
    ''' <param name="stream">包含 xps / oxps 包的可读流。</param>
    ''' <returns>段落数组。</returns>
    Public Function ExtractParagraphs(stream As Stream) As String()
        Return ToParagraphs(ExtractText(stream))
    End Function

    Private Shared Function ToParagraphs(text As String) As String()
        If String.IsNullOrEmpty(text) Then
            Return New String() {}
        End If

        Dim lines As New List(Of String)()

        For Each line As String In text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)
            If Not String.IsNullOrEmpty(line.Trim()) Then
                Call lines.Add(line)
            End If
        Next

        Return lines.ToArray()
    End Function

    ' ========================================================================
    ' 文档属性
    ' ========================================================================

    ''' <summary>
    ''' 提取文档属性（docProps/core.xml）。
    ''' </summary>
    ''' <param name="filePath">.xps / .oxps 文件路径。</param>
    ''' <returns>文档属性；缺失时返回空属性对象。</returns>
    Public Function ExtractMetadata(filePath As String) As XpsDocumentInfo
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then
            Throw New FileNotFoundException($"找不到 XPS/OXPS 文件: {filePath}", filePath)
        End If

        Using fs As Stream = File.OpenRead(filePath)
            Return ExtractMetadata(fs)
        End Using
    End Function

    ''' <summary>
    ''' 提取文档属性（docProps/core.xml）。
    ''' </summary>
    ''' <param name="stream">包含 xps / oxps 包的可读流。</param>
    ''' <returns>文档属性；缺失时返回空属性对象。</returns>
    Public Function ExtractMetadata(stream As Stream) As XpsDocumentInfo
        Dim info As New XpsDocumentInfo()

        Using archive As New ZipArchive(stream, ZipArchiveMode.Read, leaveOpen:=True)
            Dim core As XDocument = LoadXml(archive, XpsSpec.CorePropertiesPart)

            If core Is Nothing Then
                Return info
            End If

            info.Title = ReadElement(core, "title")
            info.Author = ReadElement(core, "creator")
            info.Subject = ReadElement(core, "subject")
            info.Description = ReadElement(core, "description")
            info.Keywords = ReadElement(core, "keywords")
            info.Creator = ReadElement(core, "lastModifiedBy")
        End Using

        Return info
    End Function

    ''' <summary>按本地名读取元素文本（忽略命名空间前缀差异）。</summary>
    Private Shared Function ReadElement(document As XDocument, localName As String) As String
        If document Is Nothing OrElse document.Root Is Nothing Then
            Return ""
        End If

        Dim element As XElement = document.Root.Elements() _
            .FirstOrDefault(Function(e) String.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))

        If element Is Nothing Then
            Return ""
        End If

        Return element.Value
    End Function

    ' ========================================================================
    ' 页面定位
    ' ========================================================================

    ''' <summary>
    ''' 解析页面部件顺序：优先 fdseq → fdoc → PageContent，失败时回退路径扫描。
    ''' </summary>
    Private Shared Function ResolvePageParts(archive As ZipArchive) As List(Of String)
        Dim parts As New List(Of String)()

        Try
            Dim sequencePart As String = FindPart(archive, Function(name) name.EndsWith(".fdseq", StringComparison.OrdinalIgnoreCase))

            If Not String.IsNullOrEmpty(sequencePart) Then
                Dim sequence As XDocument = LoadXml(archive, sequencePart)

                If sequence IsNot Nothing Then
                    For Each reference As XElement In ElementsByName(sequence, "DocumentReference")
                        Dim documentPart As String = ResolvePartName(sequencePart, Attribute(reference, "Source"))
                        Dim document As XDocument = LoadXml(archive, documentPart)

                        If document Is Nothing Then
                            Continue For
                        End If

                        For Each content As XElement In ElementsByName(document, "PageContent")
                            Dim pagePart As String = ResolvePartName(documentPart, Attribute(content, "Source"))

                            If Not String.IsNullOrEmpty(pagePart) AndAlso FindEntry(archive, pagePart) IsNot Nothing Then
                                Call parts.Add(pagePart)
                            End If
                        Next
                    Next
                End If
            End If
        Catch ex As Exception
            ' 关系链损坏时静默回退到路径扫描
            Call parts.Clear()
        End Try

        If parts.Count = 0 Then
            Dim scanned As New List(Of String)()

            For Each entry As ZipArchiveEntry In archive.Entries
                Dim name As String = Normalize(entry.FullName)

                If name.StartsWith("Documents/", StringComparison.OrdinalIgnoreCase) AndAlso
                   name.IndexOf("/Pages/", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso
                   name.EndsWith(".fpage", StringComparison.OrdinalIgnoreCase) Then

                    Call scanned.Add(name)
                End If
            Next

            scanned.Sort(AddressOf ComparePageParts)

            Return scanned
        End If

        Return parts
    End Function

    ''' <summary>按「路径 + 文件名中的数字」自然排序，避免 10.fpage 排在 2.fpage 之前。</summary>
    Private Shared Function ComparePageParts(left As String, right As String) As Integer
        Dim digitsLeft As Integer = TrailingNumber(left)
        Dim digitsRight As Integer = TrailingNumber(right)

        If digitsLeft <> digitsRight Then
            Return digitsLeft.CompareTo(digitsRight)
        End If

        Return String.CompareOrdinal(left, right)
    End Function

    Private Shared Function TrailingNumber(name As String) As Integer
        Dim match As Match = Regex.Match(name, "(\d+)(?!.*\d)")

        If match.Success Then
            Dim value As Integer

            If Integer.TryParse(match.Value, value) Then
                Return value
            End If
        End If

        Return 0
    End Function

    Private Shared Function FindPart(archive As ZipArchive, predicate As Func(Of String, Boolean)) As String
        For Each entry As ZipArchiveEntry In archive.Entries
            Dim name As String = Normalize(entry.FullName)

            If predicate(name) Then
                Return name
            End If
        Next

        Return Nothing
    End Function

    Private Shared Function FindEntry(archive As ZipArchive, partName As String) As ZipArchiveEntry
        Dim name As String = Normalize(partName)

        Return archive.Entries.FirstOrDefault(
            Function(entry) String.Equals(Normalize(entry.FullName), name, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>把关系里的 Source 解析为包内部件名（相对或绝对均可）。</summary>
    Private Shared Function ResolvePartName(basePart As String, source As String) As String
        If String.IsNullOrEmpty(source) Then
            Return Nothing
        End If

        Dim name As String = source.Replace("\"c, "/"c)

        If name.StartsWith("/") Then
            Return name.TrimStart("/"c)
        End If

        Dim folder As String = ""

        If Not String.IsNullOrEmpty(basePart) Then
            Dim index As Integer = basePart.LastIndexOf("/"c)

            If index > 0 Then
                folder = basePart.Substring(0, index)
            End If
        End If

        If String.IsNullOrEmpty(folder) Then
            Return Normalize(name)
        End If

        Return Normalize(folder & "/" & name)
    End Function

    ''' <summary>折叠路径中的 "." 与 ".."，并统一为 "/"。</summary>
    Private Shared Function Normalize(path As String) As String
        If String.IsNullOrEmpty(path) Then
            Return ""
        End If

        Dim stack As New List(Of String)()

        For Each segment As String In path.Replace("\"c, "/"c).Split("/"c)
            Select Case segment
                Case "", "."
                    Continue For
                Case ".."
                    If stack.Count > 0 Then
                        Call stack.RemoveAt(stack.Count - 1)
                    End If
                Case Else
                    Call stack.Add(segment)
            End Select
        Next

        Return String.Join("/", stack)
    End Function

    ' ========================================================================
    ' 单页文本
    ' ========================================================================

    Private Shared Function ExtractPageText(archive As ZipArchive, partName As String) As String
        Dim entry As ZipArchiveEntry = FindEntry(archive, partName)

        If entry Is Nothing Then
            Return ""
        End If

        Dim xml As String

        Using es As Stream = entry.Open()
            Using reader As New StreamReader(es, Encoding.UTF8, detectEncodingFromByteOrderMarks:=True)
                xml = reader.ReadToEnd()
            End Using
        End Using

        If String.IsNullOrEmpty(xml) Then
            Return ""
        End If

        Dim runs As List(Of (Y As Double, X As Double, Text As String)) = ParseGlyphs(xml)

        If runs.Count = 0 Then
            Return ""
        End If

        Return JoinLines(runs)
    End Function

    ''' <summary>
    ''' 解析页面中的 Glyphs：优先使用 XML 解析器（可正确处理实体与属性顺序），
    ''' 失败时回退为正则抓取 UnicodeString。
    ''' </summary>
    Private Shared Function ParseGlyphs(xml As String) As List(Of (Y As Double, X As Double, Text As String))
        Dim runs As New List(Of (Y As Double, X As Double, Text As String))()

        Try
            Dim document As XDocument = XDocument.Parse(xml)

            For Each glyphs As XElement In ElementsByName(document, "Glyphs")
                Dim text As String = Decode(Attribute(glyphs, "UnicodeString"))

                If String.IsNullOrEmpty(text) Then
                    Continue For
                End If

                Call runs.Add((ParseNumber(Attribute(glyphs, "OriginY")),
                               ParseNumber(Attribute(glyphs, "OriginX")),
                               text))
            Next

            If runs.Count > 0 Then
                Return runs
            End If
        Catch ex As Exception
            ' 畸形 XML：走正则容错路径
        End Try

        For Each match As Match In GlyphsFallback.Matches(xml)
            Dim text As String = Decode(If(match.Groups(1).Success, match.Groups(1).Value, match.Groups(2).Value))

            If Not String.IsNullOrEmpty(text) Then
                Call runs.Add((0, runs.Count, text))
            End If
        Next

        Return runs
    End Function

    ''' <summary>
    ''' 把 Glyphs 按基线聚类成行：同一行（OriginY 差值 ≤ <see cref="LineTolerance"/>）
    ''' 内按 OriginX 排序后拼接。
    ''' </summary>
    Private Shared Function JoinLines(runs As List(Of (Y As Double, X As Double, Text As String))) As String
        Dim ordered As List(Of (Y As Double, X As Double, Text As String)) =
            runs.OrderBy(Function(r) r.Y).ThenBy(Function(r) r.X).ToList()
        Dim lines As New List(Of String)()
        Dim buffer As New StringBuilder()
        Dim currentY As Double = ordered(0).Y

        For Each run In ordered
            If System.Math.Abs(run.Y - currentY) > LineTolerance AndAlso buffer.Length > 0 Then
                Call lines.Add(buffer.ToString())
                Call buffer.Clear()
                currentY = run.Y
            End If

            Call buffer.Append(run.Text)
        Next

        If buffer.Length > 0 Then
            Call lines.Add(buffer.ToString())
        End If

        Return String.Join(vbCrLf, lines)
    End Function

    ' ========================================================================
    ' XML 辅助
    ' ========================================================================

    Private Shared Function LoadXml(archive As ZipArchive, partName As String) As XDocument
        Dim entry As ZipArchiveEntry = FindEntry(archive, partName)

        If entry Is Nothing Then
            Return Nothing
        End If

        Try
            Using es As Stream = entry.Open()
                Using reader As New StreamReader(es, Encoding.UTF8, detectEncodingFromByteOrderMarks:=True)
                    Return XDocument.Parse(reader.ReadToEnd())
                End Using
            End Using
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Shared Function ElementsByName(document As XDocument, localName As String) As IEnumerable(Of XElement)
        If document Is Nothing OrElse document.Root Is Nothing Then
            Return New XElement() {}
        End If

        Return document.Root.Descendants() _
            .Where(Function(e) String.Equals(e.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function Attribute(element As XElement, name As String) As String
        If element Is Nothing Then
            Return Nothing
        End If

        Dim attr As XAttribute = element.Attributes() _
            .FirstOrDefault(Function(a) String.Equals(a.Name.LocalName, name, StringComparison.OrdinalIgnoreCase))

        If attr Is Nothing Then
            Return Nothing
        End If

        Return attr.Value
    End Function

    Private Shared Function ParseNumber(text As String) As Double
        If String.IsNullOrEmpty(text) Then
            Return 0
        End If

        Dim value As Double

        If Double.TryParse(text, System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, value) Then
            Return value
        End If

        Return 0
    End Function

    ''' <summary>解码 XPS 的 ``_xHHHH_`` 控制字符转义。</summary>
    Private Shared Function Decode(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return ""
        End If
        If text.IndexOf("_x", StringComparison.Ordinal) < 0 Then
            Return text
        End If

        Return CharEscape.Replace(text, Function(match)
                                              Dim code As Integer

                                              If Integer.TryParse(match.Groups(1).Value,
                                                                  System.Globalization.NumberStyles.HexNumber,
                                                                  System.Globalization.CultureInfo.InvariantCulture,
                                                                  code) Then
                                                  Return ChrW(code).ToString()
                                              End If

                                              Return match.Value
                                          End Function)
    End Function

End Class

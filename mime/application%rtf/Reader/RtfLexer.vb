#Region "Microsoft.VisualBasic::453d29dc74246b65323fb98722fe36e8, mime\application%rtf\Reader\RtfLexer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 568
    '    Code Lines: 385 (67.78%)
    ' Comment Lines: 68 (11.97%)
    '    - Xml Docs: 39.71%
    ' 
    '   Blank Lines: 115 (20.25%)
    '     File Size: 20.01 KB


    ' Module RtfCodePage
    ' 
    '     Function: EncodeChar, EncodeText, GetEncoding
    ' 
    ' Class RtfLexer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Ascii, CanEmitBody, DecodePending, GetInfo, GetParagraphs
    '               GetText, HandleControlWord, HexValue, IsAlpha, IsDigit
    '               MetaBuffer, MetaValue, ParseControl, SkipFallback
    ' 
    '     Sub: AddPending, EmitChar, EmitText, EndParagraph, FlushText
    '          Parse
    '     Structure GroupState
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' RtfLexer.vb - RTF 词法 / 分组解析器（单遍扫描）
'
' 直接在字节流上扫描，因此能同时正确处理三类文档：
'   1. 本库生成的纯 ASCII（\uN? 转义）文档；
'   2. Word 生成的 \ansicpg936 + \'hh 转义文档；
'   3. 旧式 Rtf 类生成的 ansicpg936 原始多字节（未转义）文档。
'
' 要点：
'   - 用显式栈处理 {} 分组，并随分组保存/恢复 \ansicpg / \cpg / \uc 的作用域；
'   - \* 引导的未知目标、fonttbl/colortbl/stylesheet/pict/... 等目标整组跳过；
'   - \info 容器与 title/author/subject/keywords/operator/doccomm/generator 捕获为元数据；
'   - \par/\line/\row/\sect/\page 产出段落边界，\tab/\cell 产出制表符；
'   - \binN 按长度跳过二进制；\uN 后按 \ucN 跳过回退字符；
'   - 对未闭合分组等畸形输入安全终止，不向调用方抛出异常。
'
' 复杂度：时间 O(输入长度) 单遍，内存与输出文本同阶；不使用递归下降，深度无栈溢出风险。
' ============================================================================

Imports System.Text

''' <summary>
''' RTF 所依赖的代码页访问：写入侧用于把非 ASCII 文本（字体名、生成器）编码为 ``\'hh``，
''' 读取侧用于按 \ansicpg / \cpg 解码 ``\'hh`` 与原始多字节字节序列。
''' </summary>
''' <remarks>
''' 优先使用 <c>CodePagesEncodingProvider</c> 注册系统代码页（如 cp936）；
''' 运行时不提供时静默降级为单字节拉丁映射，保证解析不会因此失败。
''' </remarks>
Friend Module RtfCodePage

    ''' <summary>默认代码页（RTF 未声明 \ansicpg 时按 Windows-1252 解释）。</summary>
    Public Const DefaultCodePage As Integer = 1252

    ''' <summary>中文代码页。</summary>
    Public Const ChineseCodePage As Integer = 936

    Private registered As Boolean = False
    Private ReadOnly cache As New Dictionary(Of Integer, Encoding)()
    Private ReadOnly HexDigits As Char() = "0123456789abcdef".ToCharArray()

    ''' <summary>按代码页取编码；不可用时返回 Nothing。</summary>
    Public Function GetEncoding(codePage As Integer) As Encoding
        If codePage <= 0 Then codePage = DefaultCodePage

        SyncLock cache
            If Not registered Then
                registered = True

                Try
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
                Catch ex As Exception
                    ' 运行时缺少代码页提供程序：降级处理
                End Try
            End If

            Dim enc As Encoding = Nothing
            If cache.TryGetValue(codePage, enc) Then Return enc

            Try
                enc = Encoding.GetEncoding(codePage)
            Catch ex As Exception
                enc = Nothing
            End Try

            cache(codePage) = enc

            Return enc
        End SyncLock
    End Function

    ''' <summary>
    ''' 把字符串编码为纯 ASCII 的 RTF 文本：``\`` ``{`` ``}`` ``;`` 转义，
    ''' 非 ASCII 字符按文档代码页输出为 ``\'hh``。用于字体名、生成器等必须为 ASCII 的字段。
    ''' </summary>
    Public Function EncodeText(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""

        Dim sb As New StringBuilder(text.Length + 8)
        Dim enc As Encoding = GetEncoding(ChineseCodePage)

        For Each c As Char In text
            Dim code As Integer = AscW(c)

            If code < &H20 Then
                ' 控制字符一并丢弃
            ElseIf code < &H7F Then
                Select Case c
                    Case "\"c
                        Call sb.Append("\\")
                    Case "{"c
                        Call sb.Append("\{")
                    Case "}"c
                        Call sb.Append("\}")
                    Case ";"c
                        Call sb.Append("\;")
                    Case Else
                        Call sb.Append(c)
                End Select
            Else
                Dim bytes As Byte() = EncodeChar(enc, c)

                If bytes IsNot Nothing Then
                    For Each b As Byte In bytes
                        Call sb.Append("\'").Append(HexDigits(CInt(b) >> 4)).Append(HexDigits(CInt(b) And &HF))
                    Next
                Else
                    Call sb.Append("?"c)
                End If
            End If
        Next

        Return sb.ToString()
    End Function

    Private Function EncodeChar(enc As Encoding, c As Char) As Byte()
        If enc Is Nothing Then Return Nothing

        Try
            Return enc.GetBytes(c.ToString())
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

End Module

''' <summary>
''' RTF 词法解析器：单遍扫描 RTF 字节流，产出正文段落与文档元数据。
''' </summary>
Friend Class RtfLexer

    Private Const BSlash As Byte = 92       ' \
    Private Const BOpen As Byte = 123       ' {
    Private Const BClose As Byte = 125      ' }
    Private Const BQuote As Byte = 39       ' '
    Private Const BSpace As Byte = 32       ' '

    ''' <summary>分组状态快照，用于 {} 配对时恢复各类设置的作用域。</summary>
    Private Structure GroupState
        Public SkipDepth As Integer
        Public Charset As Integer
        Public UcSkip As Integer
        Public InInfo As Boolean
        Public MetaDest As String
        Public Starred As Boolean
    End Structure

    ''' <summary>整组跳过的 RTF 目标（非正文内容）。</summary>
    Private Shared ReadOnly IgnorableDestinations As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "fonttbl", "colortbl", "expandedcolortbl", "stylesheet",
        "listtable", "listoverridetable", "rsidtbl", "listtext",
        "pict", "shppict", "nonshppict", "shp", "object", "objclass", "objdata", "result",
        "header", "headerl", "headerr", "headerf",
        "footer", "footerl", "footerr", "footerf",
        "footnote", "ftnsep", "ftnsepc", "ftncn", "aftnsep", "aftnsepc", "aftncn",
        "annotation", "atnid", "atnauthor", "atnref", "atndate", "atnicn",
        "comment", "fldinst", "datafield", "datastore", "docvar", "upr",
        "themedata", "colorschememapping", "latentstyles", "xmlnstbl",
        "filetbl", "revtbl", "pgptbl", "wgrffmtfilter", "formfield", "privat",
        "panose", "falt", "fname", "fontemb", "fontfile", "mmathPr", "mmath"
    }

    ''' <summary>被捕获为文档元数据的目标。</summary>
    Private Shared ReadOnly MetaDestinations As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "title", "author", "subject", "keywords", "operator", "company", "doccomm", "generator"
    }

    Private ReadOnly data As Byte()
    Private ReadOnly sb As New StringBuilder(1024)
    Private ReadOnly paragraphs As New List(Of String)()
    Private ReadOnly metaBuf As New Dictionary(Of String, StringBuilder)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly stack As New Stack(Of GroupState)()

    Private pending As Byte()
    Private pendingLen As Integer = 0

    Private depth As Integer = 0
    Private skipDepth As Integer = -1
    Private inInfo As Boolean = False
    Private metaDest As String = Nothing
    Private starred As Boolean = False
    Private charset As Integer = RtfCodePage.DefaultCodePage
    Private ucSkip As Integer = 1

    Public Sub New(bytes As Byte())
        Me.data = If(bytes, New Byte() {})
        Me.pending = New Byte(1023) {}
        Call Parse()
    End Sub

    ''' <summary>整篇纯文本（段落以换行分隔）。</summary>
    Public Function GetText() As String
        Return String.Join(vbCrLf, GetParagraphs())
    End Function

    ''' <summary>按段落切分的文本（已去除尾部空段）。</summary>
    Public Function GetParagraphs() As String()
        Dim list As New List(Of String)(paragraphs)

        While list.Count > 0 AndAlso String.IsNullOrEmpty(list(list.Count - 1))
            list.RemoveAt(list.Count - 1)
        End While

        Return list.ToArray()
    End Function

    ''' <summary>文档元数据（缺失字段为空字符串）。</summary>
    Public Function GetInfo() As RtfDocumentInfo
        Return New RtfDocumentInfo() With {
            .Title = MetaValue("title"),
            .Author = MetaValue("author"),
            .Subject = MetaValue("subject"),
            .Keywords = MetaValue("keywords"),
            .Company = MetaValue("company"),
            .Comments = MetaValue("doccomm"),
            .Generator = MetaValue("generator")
        }
    End Function

    ' ========================================================================
    ' 扫描
    ' ========================================================================

    Private Sub Parse()
        Dim i As Integer = 0
        Dim n As Integer = data.Length

        While i < n
            Dim b As Byte = data(i)

            If b = BOpen Then
                stack.Push(New GroupState With {
                    .SkipDepth = skipDepth,
                    .Charset = charset,
                    .UcSkip = ucSkip,
                    .InInfo = inInfo,
                    .MetaDest = metaDest,
                    .Starred = starred
                })
                depth += 1
                i += 1
            ElseIf b = BClose Then
                Call FlushText()

                If stack.Count = 0 Then
                    ' 多余的右花括号：判定为畸形输入，结束解析
                    Exit While
                End If

                Dim state As GroupState = stack.Pop()
                depth -= 1
                skipDepth = state.SkipDepth
                charset = state.Charset
                ucSkip = state.UcSkip
                inInfo = state.InInfo
                metaDest = state.MetaDest
                starred = state.Starred

                i += 1
            ElseIf b = BSlash Then
                i = ParseControl(i + 1, n)
            ElseIf b = 13 OrElse b = 10 Then
                ' RTF 流中的 CR/LF 只是排版换行（Word 会按 255 字符折行），不属于正文
                i += 1
            Else
                If skipDepth < 0 Then Call AddPending(b)
                i += 1
            End If
        End While

        Call FlushText()
        Call EndParagraph()
    End Sub

    ''' <summary>解析一个控制字或控制符号，返回下一个待处理位置。</summary>
    Private Function ParseControl(i As Integer, n As Integer) As Integer
        If i >= n Then
            Call FlushText()
            Return i
        End If

        Dim b As Byte = data(i)

        ' \'hh 的字节与相邻文本同属一个代码页字节序列，多字节代码页下可能只是半个字符，
        ' 因此必须在刷新缓冲区之前处理，否则会把双字节字符从中间截断
        If b = BQuote Then
            If i + 2 < n Then
                Dim hi As Integer = HexValue(data(i + 1))
                Dim lo As Integer = HexValue(data(i + 2))

                If hi >= 0 AndAlso lo >= 0 Then
                    If skipDepth < 0 Then Call AddPending(CByte(hi * 16 + lo))
                    Return i + 3
                End If
            End If

            Return n
        End If

        Call FlushText()

        If IsAlpha(b) Then
            Dim start As Integer = i

            While i < n AndAlso i - start < 32 AndAlso IsAlpha(data(i))
                i += 1
            End While

            Dim word As String = Ascii(data, start, i - start)

            ' 可选负号 + 数字参数
            Dim sign As Integer = 1
            If i < n AndAlso data(i) = 45 Then      ' '-'
                sign = -1
                i += 1
            End If

            Dim numStart As Integer = i
            While i < n AndAlso IsDigit(data(i))
                i += 1
            End While

            Dim hasNumber As Boolean = i > numStart
            Dim number As Integer = 0

            If hasNumber Then
                Call Integer.TryParse(Ascii(data, numStart, i - numStart), number)
            End If

            number *= sign

            ' 控制字之后的单个空格是分隔符，需吞掉
            If i < n AndAlso data(i) = BSpace Then i += 1

            Return HandleControlWord(word, number, hasNumber, i, n)
        End If

        Select Case ChrW(b)
            Case "*"c
                starred = True
            Case "~"c
                Call EmitText(ChrW(&HA0))
            Case "_"c
                Call EmitText(ChrW(&H2011))
            Case "{"c
                Call EmitText("{")
            Case "}"c
                Call EmitText("}")
            Case "\"c
                Call EmitText("\")
            Case Else
                ' 未知控制符号：忽略
        End Select

        Return i + 1
    End Function

    Private Function HandleControlWord(word As String, number As Integer, hasNumber As Boolean,
                                      i As Integer, n As Integer) As Integer
        Dim w As String = word.ToLower()

        ' \* 之后的目标若无法识别，则整组跳过
        If starred Then
            starred = False

            If Not MetaDestinations.Contains(w) Then
                skipDepth = depth
                Return i
            End If
        End If

        Select Case w
            Case "par", "sect", "page", "row", "nestrow"
                Call EndParagraph()

            Case "line"
                If CanEmitBody() Then Call sb.Append(vbCrLf)

            Case "tab", "cell", "nestcell"
                If CanEmitBody() Then Call sb.Append(vbTab)

            Case "u"
                If skipDepth < 0 Then
                    ' \uN 的 N 为有符号 16 位
                    Call EmitText(ChrW(If(number < 0, number + &H10000, number)))
                    Return SkipFallback(i, n)
                End If

            Case "uc"
                If hasNumber Then ucSkip = System.Math.Max(0, number)

            Case "ansicpg", "cpg"
                If hasNumber AndAlso number > 0 Then charset = number

            Case "bin"
                If hasNumber AndAlso number > 0 Then Return System.Math.Min(n, i + number)

            Case "info"
                inInfo = True

            Case "bullet"
                Call EmitText(ChrW(&H2022))

            Case "lquote"
                Call EmitText(ChrW(&H2018))

            Case "rquote"
                Call EmitText(ChrW(&H2019))

            Case "ldblquote"
                Call EmitText(ChrW(&H201C))

            Case "rdblquote"
                Call EmitText(ChrW(&H201D))

            Case "endash"
                Call EmitText(ChrW(&H2013))

            Case "emdash"
                Call EmitText(ChrW(&H2014))

            Case "enspace", "emspace", "qmspace"
                Call EmitText(" ")

            Case Else
                If MetaDestinations.Contains(w) Then
                    metaDest = w
                ElseIf IgnorableDestinations.Contains(w) Then
                    skipDepth = depth
                End If
        End Select

        Return i
    End Function

    ''' <summary>跳过 \uN 之后的 \ucN 个回退字符（回退字符可能是 \'hh 或单个字符）。</summary>
    Private Function SkipFallback(i As Integer, n As Integer) As Integer
        Dim skipped As Integer = 0

        While skipped < ucSkip AndAlso i < n
            If data(i) = BSlash AndAlso i + 3 < n AndAlso data(i + 1) = BQuote Then
                i += 4
                skipped += 1
            ElseIf data(i) = BSlash Then
                ' 已进入其它控制字，回退字符结束
                Exit While
            Else
                i += 1
                skipped += 1
            End If
        End While

        Return i
    End Function

    ' ========================================================================
    ' 文本输出
    ' ========================================================================

    Private Function CanEmitBody() As Boolean
        Return skipDepth < 0 AndAlso metaDest Is Nothing AndAlso Not inInfo
    End Function

    Private Sub EmitText(text As String)
        If String.IsNullOrEmpty(text) Then Return
        If skipDepth >= 0 Then Return

        If metaDest IsNot Nothing Then
            Call MetaBuffer(metaDest).Append(text)
        ElseIf inInfo Then
            ' \info 容器内未被目标捕获的文本不属于正文，直接丢弃
        Else
            Call sb.Append(text)
        End If
    End Sub

    Private Sub EmitChar(c As Char)
        Call EmitText(c)
    End Sub

    Private Sub EndParagraph()
        If skipDepth >= 0 Then Return
        If metaDest IsNot Nothing OrElse inInfo Then Return

        paragraphs.Add(sb.ToString().Trim())
        Call sb.Clear()
    End Sub

    Private Sub AddPending(b As Byte)
        If pendingLen >= pending.Length Then
            Array.Resize(pending, pending.Length * 2)
        End If

        pending(pendingLen) = b
        pendingLen += 1
    End Sub

    Private Sub FlushText()
        If pendingLen = 0 Then Return

        Dim text As String = DecodePending()
        pendingLen = 0

        Call EmitText(text)
    End Sub

    ''' <summary>按当前代码页解码缓冲区中的字节（多字节代码页下连续字节会被正确合并）。</summary>
    Private Function DecodePending() As String
        Dim enc As Encoding = RtfCodePage.GetEncoding(charset)

        If enc IsNot Nothing Then
            Return enc.GetString(pending, 0, pendingLen)
        End If

        ' 降级：单字节拉丁映射
        Dim fallback As New StringBuilder(pendingLen)

        For k As Integer = 0 To pendingLen - 1
            Call fallback.Append(ChrW(pending(k)))
        Next

        Return fallback.ToString()
    End Function

    Private Function MetaBuffer(dest As String) As StringBuilder
        Dim buf As StringBuilder = Nothing

        If metaBuf.TryGetValue(dest, buf) Then Return buf

        buf = New StringBuilder()
        metaBuf(dest) = buf

        Return buf
    End Function

    Private Function MetaValue(name As String) As String
        Dim buf As StringBuilder = Nothing

        If Not metaBuf.TryGetValue(name, buf) Then Return ""

        Return buf.ToString().Trim().TrimEnd(";"c).Trim()
    End Function

    ' ========================================================================
    ' 字节辅助
    ' ========================================================================

    Private Shared Function IsAlpha(b As Byte) As Boolean
        Return (b >= 65 AndAlso b <= 90) OrElse (b >= 97 AndAlso b <= 122)
    End Function

    Private Shared Function IsDigit(b As Byte) As Boolean
        Return b >= 48 AndAlso b <= 57
    End Function

    Private Shared Function HexValue(b As Byte) As Integer
        If b >= 48 AndAlso b <= 57 Then Return b - 48
        If b >= 97 AndAlso b <= 102 Then Return b - 87
        If b >= 65 AndAlso b <= 70 Then Return b - 55
        Return -1
    End Function

    Private Shared Function Ascii(data As Byte(), start As Integer, length As Integer) As String
        Return Encoding.ASCII.GetString(data, start, length)
    End Function

End Class


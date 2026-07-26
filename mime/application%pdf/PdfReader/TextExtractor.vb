#Region "Microsoft.VisualBasic::7ecad1e6b9406dac5991aa96cb28ef3e, mime\application%pdf\PdfReader\TextExtractor.vb"

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

    '   Total Lines: 400
    '    Code Lines: 309 (77.25%)
    ' Comment Lines: 47 (11.75%)
    '    - Xml Docs: 6.38%
    ' 
    '   Blank Lines: 44 (11.00%)
    '     File Size: 15.89 KB


    ' Class TextExtractor
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DecodeText, ExtractAll, ExtractFromPage, GetInheritedResource, ParseInlineArray
    '               PopNumber
    ' 
    '     Sub: LoadFonts, ParseContentStream, ProcessOperator, ShowText, ShowTextArray
    '          SkipInlineDict, SkipInlineImage
    ' 
    ' Class FontInfo
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  TextExtractor.vb  -  内容流文本提取器
'  ----------------------------------------------------------------------------
'  职责：从 PDF 页面对象提取纯文本。
'  流程：
'    1. 从页面 /Resources 加载字体表（含 ToUnicode CMap、Encoding）
'    2. 获取页面 /Contents（可能是单个流引用或流引用数组）
'    3. 解码内容流（FlateDecode 等）
'    4. 词法解析内容流，维护操作数栈
'    5. 处理文本操作符：BT/ET/Tf/Td/TD/Tm/Tj/TJ/'/"/T*
'    6. 通过字体 ToUnicode 或 Encoding 将字符码映射为 Unicode
'  支持的字体类型：
'    - Type1/TrueType + WinAnsiEncoding/StandardEncoding
'    - Type0 (CID) + ToUnicode CMap（2 字节字符码）
' ============================================================================

Imports System.IO
Imports System.Text
Imports std = System.Math

Public Class TextExtractor
    Private ReadOnly _reader As PdfReader
    Private _fonts As New Dictionary(Of String, FontInfo)()
    Private _currentFont As String = ""
    Private _result As New StringBuilder()
    Private _lastY As Double = Double.NaN

    Public Sub New(reader As PdfReader)
        _reader = reader
    End Sub

    ''' <summary>提取单页文本。</summary>
    Public Function ExtractFromPage(page As PdfDictionary) As String
        _result.Clear()
        _fonts.Clear()
        _currentFont = ""
        _lastY = Double.NaN

        ' 加载字体（Resources 可能继承自父节点）
        Dim resources = TryCast(page.Get("Resources"), PdfDictionary)
        If resources Is Nothing Then
            resources = TryCast(GetInheritedResource(page, "Resources"), PdfDictionary)
        End If
        If resources IsNot Nothing Then
            LoadFonts(resources)
        End If

        ' 获取内容流
        Dim contents = page.Get("Contents")
        Dim contentData As Byte() = Nothing
        If TypeOf contents Is PdfReference Then
            Dim stream = TryCast(_reader.Resolve(DirectCast(contents, PdfReference)), PdfStream)
            If stream IsNot Nothing Then contentData = _reader.DecodeStream(stream)
        ElseIf TypeOf contents Is PdfArray Then
            Using ms As New MemoryStream()
                For Each item In DirectCast(contents, PdfArray).Items
                    Dim ref = TryCast(item, PdfReference)
                    If ref IsNot Nothing Then
                        Dim stream = TryCast(_reader.Resolve(ref), PdfStream)
                        If stream IsNot Nothing Then
                            Dim d = _reader.DecodeStream(stream)
                            If d IsNot Nothing AndAlso d.Length > 0 Then
                                ms.Write(d, 0, d.Length)
                                ' 流之间补一个空白分隔符，避免操作符粘连
                                ms.WriteByte(10)
                            End If
                        End If
                    End If
                Next
                contentData = ms.ToArray()
            End Using
        End If

        If contentData Is Nothing OrElse contentData.Length = 0 Then Return ""
        ParseContentStream(contentData)
        Return _result.ToString()
    End Function

    ''' <summary>提取整个文档文本。</summary>
    Public Function ExtractAll() As String
        Dim sb As New StringBuilder()
        Dim pages = _reader.GetPages()
        For i = 0 To pages.Count - 1
            sb.Append(ExtractFromPage(pages(i)))
            sb.AppendLine()
            sb.AppendLine()
        Next
        Return sb.ToString()
    End Function

    ' ---------------- 资源继承 ----------------

    Private Function GetInheritedResource(page As PdfDictionary, key As String) As PdfObject
        Dim obj = page.Get(key)
        If obj IsNot Nothing Then Return obj
        Dim parent = TryCast(page.Get("Parent"), PdfReference)
        If parent IsNot Nothing Then
            Dim parentDict = TryCast(_reader.Resolve(parent), PdfDictionary)
            If parentDict IsNot Nothing Then Return GetInheritedResource(parentDict, key)
        End If
        Return Nothing
    End Function

    ' ---------------- 字体加载 ----------------

    Private Sub LoadFonts(resources As PdfDictionary)
        Dim fontDict = TryCast(resources.Get("Font"), PdfDictionary)
        If fontDict Is Nothing Then Return
        For Each name In fontDict.Names
            Dim fontRef = TryCast(fontDict.Get(name), PdfReference)
            If fontRef Is Nothing Then Continue For
            Dim fontObj = TryCast(_reader.Resolve(fontRef), PdfDictionary)
            If fontObj Is Nothing Then Continue For
            Dim info As New FontInfo()
            Dim subtype = TryCast(fontObj.Get("Subtype"), PdfName)
            If subtype IsNot Nothing Then info.Subtype = subtype.Value
            Dim baseFont = TryCast(fontObj.Get("BaseFont"), PdfName)
            If baseFont IsNot Nothing Then info.BaseFont = baseFont.Value

            ' Type0 字体使用 2 字节字符码
            If info.Subtype = "Type0" Then info.IsTwoByte = True

            ' ToUnicode CMap
            Dim tuRef = TryCast(fontObj.Get("ToUnicode"), PdfReference)
            If tuRef IsNot Nothing Then
                Dim tuStream = TryCast(_reader.Resolve(tuRef), PdfStream)
                If tuStream IsNot Nothing Then
                    Dim tuData = _reader.DecodeStream(tuStream)
                    info.ToUnicode = New ToUnicodeCMap()
                    info.ToUnicode.Parse(tuData)
                End If
            End If

            ' Encoding
            Dim encObj = fontObj.Get("Encoding")
            If TypeOf encObj Is PdfName Then
                info.Encoding = DirectCast(encObj, PdfName).Value
            ElseIf TypeOf encObj Is PdfDictionary Then
                info.EncodingDict = DirectCast(encObj, PdfDictionary)
            End If

            _fonts(name) = info
        Next
    End Sub

    ' ---------------- 内容流解析 ----------------

    Private Sub ParseContentStream(data As Byte())
        Dim lexer As New PdfLexer(data)
        Dim operandStack As New Stack(Of PdfObject)()
        Do
            Dim token = lexer.NextToken()
            If token.Type = PdfTokenType.EOF Then Exit Do
            Select Case token.Type
                Case PdfTokenType.Number
                    operandStack.Push(New PdfNumber(token.NumberValue))
                Case PdfTokenType.Name
                    operandStack.Push(New PdfName(token.TextValue))
                Case PdfTokenType.LiteralString, PdfTokenType.HexString
                    operandStack.Push(New PdfString(token.TextValue, token.ByteValue))
                Case PdfTokenType.ArrayOpen
                    Dim arr = ParseInlineArray(lexer)
                    operandStack.Push(arr)
                Case PdfTokenType.DictOpen
                    ' 跳过内联字典（标记内容属性等）
                    SkipInlineDict(lexer)
                Case PdfTokenType.Keyword
                    If token.TextValue = "BI" Then
                        SkipInlineImage(lexer)
                    Else
                        ProcessOperator(token.TextValue, operandStack)
                    End If
                Case PdfTokenType.Obj, PdfTokenType.EndObj, PdfTokenType.Stream, PdfTokenType.EndStream
                    ' 忽略
                Case Else
                    ' 忽略
            End Select
        Loop
    End Sub

    Private Function ParseInlineArray(lexer As PdfLexer) As PdfArray
        Dim arr As New PdfArray()
        Do
            Dim t = lexer.NextToken()
            If t.Type = PdfTokenType.ArrayClose Then Exit Do
            If t.Type = PdfTokenType.EOF Then Exit Do
            Select Case t.Type
                Case PdfTokenType.Number : arr.Add(New PdfNumber(t.NumberValue))
                Case PdfTokenType.Name : arr.Add(New PdfName(t.TextValue))
                Case PdfTokenType.LiteralString, PdfTokenType.HexString : arr.Add(New PdfString(t.TextValue, t.ByteValue))
                Case PdfTokenType.ArrayOpen
                    arr.Add(ParseInlineArray(lexer))
            End Select
        Loop
        Return arr
    End Function

    Private Sub SkipInlineDict(lexer As PdfLexer)
        Dim depth = 1
        Do
            Dim t = lexer.NextToken()
            If t.Type = PdfTokenType.EOF Then Exit Do
            If t.Type = PdfTokenType.DictOpen Then
                depth += 1
            ElseIf t.Type = PdfTokenType.DictClose Then
                depth -= 1
                If depth = 0 Then Exit Do
            End If
        Loop
    End Sub

    Private Sub SkipInlineImage(lexer As PdfLexer)
        ' BI ... ID <data> EI
        ' 跳过键值对直到 ID
        Do
            Dim t = lexer.NextToken()
            If t.Type = PdfTokenType.EOF Then Exit Sub
            If t.Type = PdfTokenType.Keyword AndAlso t.TextValue = "ID" Then Exit Do
        Loop
        ' 跳过图像数据，扫描到 EI
        Do
            Dim t = lexer.NextToken()
            If t.Type = PdfTokenType.EOF Then Exit Sub
            If t.Type = PdfTokenType.Keyword AndAlso t.TextValue = "EI" Then Exit Do
        Loop
    End Sub

    ' ---------------- 文本操作符处理 ----------------

    Private Sub ProcessOperator(op As String, stack As Stack(Of PdfObject))
        Select Case op
            Case "BT", "ET", "q", "Q", "cs", "CS", "sc", "SC", "scn", "SCN",
                 "g", "G", "rg", "RG", "k", "K", "w", "J", "j", "M", "d",
                 "i", "ri", "gs", "W", "W*", "n", "re", "S", "s", "f", "F",
                 "f*", "B", "B*", "b", "b*", "h", "BDC", "BMC", "EMC", "MP",
                 "BX", "EX", "d0", "d1"
                ' 这些操作符不产生文本，清空操作数栈
                stack.Clear()

            Case "Tf"
                ' /F1 12 Tf
                If stack.Count >= 2 Then
                    stack.Pop() ' size
                    Dim font = stack.Pop()
                    Dim fontName = TryCast(font, PdfName)
                    If fontName IsNot Nothing Then _currentFont = fontName.Value
                End If
                stack.Clear()

            Case "Tm"
                ' a b c d e f Tm  -> e,f 是平移
                If stack.Count >= 6 Then
                    Dim f = PopNumber(stack) ' f = y 平移
                    Dim e = PopNumber(stack) ' e = x 平移
                    stack.Pop() : stack.Pop() : stack.Pop() : stack.Pop()
                    If Not Double.IsNaN(_lastY) AndAlso std.Abs(_lastY - f) > 2 Then
                        _result.AppendLine()
                    End If
                    _lastY = f
                End If
                stack.Clear()

            Case "Td", "TD"
                ' tx ty Td / tx ty TD
                If stack.Count >= 2 Then
                    Dim ty = PopNumber(stack)
                    Dim tx = PopNumber(stack)
                    If op = "TD" Then
                        ' TD 同时设置行距为 -ty
                        If ty < 0 Then _result.AppendLine()
                    Else
                        If ty < -1 Then _result.AppendLine()
                    End If
                End If
                stack.Clear()

            Case "T*"
                _result.AppendLine()
                stack.Clear()

            Case "Tj"
                If stack.Count >= 1 Then ShowText(stack.Pop())
                stack.Clear()

            Case "TJ"
                If stack.Count >= 1 Then ShowTextArray(stack.Pop())
                stack.Clear()

            Case "'" ' 移到下一行并显示文本
                _result.AppendLine()
                If stack.Count >= 1 Then ShowText(stack.Pop())
                stack.Clear()

            Case """" ' 移到下一行，设置字距，显示文本
                If stack.Count >= 3 Then
                    stack.Pop() : stack.Pop()
                    ShowText(stack.Pop())
                End If
                stack.Clear()

            Case "Tc", "Tw", "Tz", "TL", "Tr", "Ts"
                ' 字距/缩放等参数，不影响文本内容
                stack.Clear()

            Case Else
                stack.Clear()
        End Select
    End Sub

    Private Function PopNumber(stack As Stack(Of PdfObject)) As Double
        If stack.Count = 0 Then Return 0
        Dim o = stack.Pop()
        Dim n = TryCast(o, PdfNumber)
        If n IsNot Nothing Then Return n.Value
        Return 0
    End Function

    ' ---------------- 文本显示 ----------------

    Private Sub ShowText(obj As PdfObject)
        Dim s = TryCast(obj, PdfString)
        If s Is Nothing OrElse s.RawBytes Is Nothing Then Return
        Dim text = DecodeText(s.RawBytes)
        _result.Append(text)
    End Sub

    Private Sub ShowTextArray(obj As PdfObject)
        Dim arr = TryCast(obj, PdfArray)
        If arr Is Nothing Then
            ShowText(obj)
            Return
        End If
        For Each item In arr.Items
            If TypeOf item Is PdfString Then
                ShowText(item)
            ElseIf TypeOf item Is PdfNumber Then
                Dim n = DirectCast(item, PdfNumber).Value
                ' 大负位移通常表示空格
                If n < -100 Then _result.Append(" "c)
            End If
        Next
    End Sub

    ' ---------------- 字符码 -> Unicode ----------------

    Private Function DecodeText(bytes As Byte()) As String
        Dim font As FontInfo = Nothing
        If Not _fonts.TryGetValue(_currentFont, font) Then
            ' 无字体信息，按 Latin-1 解码
            Return Encoding.GetEncoding("ISO-8859-1").GetString(bytes)
        End If

        ' 优先使用 ToUnicode CMap
        If font.ToUnicode IsNot Nothing AndAlso font.ToUnicode.HasMapping() Then
            Dim sb As New StringBuilder()
            If font.IsTwoByte Then
                Dim i = 0
                While i < bytes.Length
                    Dim code As Integer
                    If i + 1 < bytes.Length Then
                        code = (bytes(i) << 8) Or bytes(i + 1)
                        i += 2
                    Else
                        code = bytes(i)
                        i += 1
                    End If
                    Dim mapped = font.ToUnicode.Lookup(code)
                    If mapped IsNot Nothing Then sb.Append(mapped)
                End While
            Else
                For Each b In bytes
                    Dim mapped = font.ToUnicode.Lookup(b)
                    If mapped IsNot Nothing Then sb.Append(mapped)
                Next
            End If
            Return sb.ToString()
        End If

        ' 回退到 Encoding
        If font.Encoding = "WinAnsiEncoding" Then
            Return Encoding.GetEncoding("windows-1252").GetString(bytes)
        ElseIf font.Encoding = "MacRomanEncoding" Then
            Return Encoding.GetEncoding("macintosh").GetString(bytes)
        End If

        ' 默认 Latin-1
        Return Encoding.GetEncoding("ISO-8859-1").GetString(bytes)
    End Function

End Class

''' <summary>字体信息缓存。</summary>
Public Class FontInfo
    Public Subtype As String = ""
    Public BaseFont As String = ""
    Public Encoding As String = ""
    Public EncodingDict As PdfDictionary
    Public ToUnicode As ToUnicodeCMap
    Public IsTwoByte As Boolean = False
End Class


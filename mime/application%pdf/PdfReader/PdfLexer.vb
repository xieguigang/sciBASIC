#Region "Microsoft.VisualBasic::33a70b84dbb62a8d82e9f703806e859e, mime\application%pdf\PdfReader\PdfLexer.vb"

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

    '   Total Lines: 385
    '    Code Lines: 319 (82.86%)
    ' Comment Lines: 33 (8.57%)
    '    - Xml Docs: 9.09%
    ' 
    '   Blank Lines: 33 (8.57%)
    '     File Size: 14.60 KB


    ' Enum PdfTokenType
    ' 
    '     EndObj, EndStream, EOF, HexString, Keyword
    '     LiteralString, Name, Number, Obj, StartXRef
    '     Stream, Trailer, XRef
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class PdfToken
    ' 
    '     Properties: ByteValue, NumberValue, Position, TextValue, Type
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' Class PdfLexer
    ' 
    '     Properties: Data, Length, Position
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DecodePdfString, IsDelimiter, IsWhitespace, NextToken, ReadHexString
    '               ReadLiteralString, ReadName, ReadNumberOrKeyword, ReadStreamData, ReadStreamDataScan
    ' 
    '     Sub: SkipStreamEOL, SkipWhitespaceAndComments
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  PdfLexer.vb  -  PDF 词法分析器
'  ----------------------------------------------------------------------------
'  将 PDF 字节流切分为 Token。PDF 词法规则参见 ISO 32000-1 第 7.2 节。
'  支持的 Token 类型：
'    - Number        整数/实数
'    - Name          /Name
'    - LiteralString (...)
'    - HexString     <...>
'    - DictOpen/Close << >>
'    - ArrayOpen/Close [ ]
'    - Keyword       true/false/null/R/obj/endobj/stream/endstream/xref/...
'    - Obj/EndObj/Stream/EndStream/XRef/Trailer/StartXRef  关键字 Token
'    - EOF
'  本词法器同时提供 ReadStreamData() 用于读取流二进制数据。
' ============================================================================

Imports System.Text

Public Enum PdfTokenType
    Number
    Name
    LiteralString
    HexString
    DictOpen      ' <<
    DictClose     ' >>
    ArrayOpen     ' [
    ArrayClose    ' ]
    Keyword
    Obj
    EndObj
    Stream
    EndStream
    XRef
    Trailer
    StartXRef
    EOF
End Enum

Public Class PdfToken
    Public ReadOnly Property Type As PdfTokenType
    Public ReadOnly Property TextValue As String
    Public ReadOnly Property ByteValue As Byte()
    Public ReadOnly Property NumberValue As Double
    Public ReadOnly Property Position As Long
    Public Sub New(type As PdfTokenType, text As String, bytes As Byte(), num As Double, pos As Long)
        Me.Type = type
        Me.TextValue = text
        Me.ByteValue = bytes
        Me.NumberValue = num
        Me.Position = pos
    End Sub
End Class

Public Class PdfLexer
    Private ReadOnly _data As Byte()
    Private _pos As Integer
    Private ReadOnly _length As Integer

    Public Sub New(data As Byte())
        _data = data
        _pos = 0
        _length = If(data, New Byte(-1) {}).Length
    End Sub

    Public Property Position As Integer
        Get
            Return _pos
        End Get
        Set(value As Integer)
            _pos = value
        End Set
    End Property

    Public ReadOnly Property Length As Integer
        Get
            Return _length
        End Get
    End Property

    Public ReadOnly Property Data As Byte()
        Get
            Return _data
        End Get
    End Property

    ' PDF 空白字符：NUL TAB LF FF CR SP
    Private Shared Function IsWhitespace(b As Byte) As Boolean
        Return b = 0 OrElse b = 9 OrElse b = 10 OrElse b = 12 OrElse b = 13 OrElse b = 32
    End Function

    ' PDF 分隔符：( ) < > [ ] { } / %
    Private Shared Function IsDelimiter(b As Byte) As Boolean
        Return b = 40 OrElse b = 41 OrElse b = 60 OrElse b = 62 OrElse
               b = 91 OrElse b = 93 OrElse b = 123 OrElse b = 125 OrElse b = 47 OrElse b = 37
    End Function

    Private Sub SkipWhitespaceAndComments()
        While _pos < _length
            Dim b = _data(_pos)
            If IsWhitespace(b) Then
                _pos += 1
            ElseIf b = 37 Then ' %
                ' 注释直到行尾
                While _pos < _length AndAlso _data(_pos) <> 10 AndAlso _data(_pos) <> 13
                    _pos += 1
                End While
            Else
                Exit While
            End If
        End While
    End Sub

    Public Function NextToken() As PdfToken
        SkipWhitespaceAndComments()
        If _pos >= _length Then Return New PdfToken(PdfTokenType.EOF, Nothing, Nothing, 0, _pos)

        Dim startPos = _pos
        Dim b = _data(_pos)

        ' << 或 hex 字符串
        If b = 60 Then ' <
            If _pos + 1 < _length AndAlso _data(_pos + 1) = 60 Then
                _pos += 2
                Return New PdfToken(PdfTokenType.DictOpen, "<<", Nothing, 0, startPos)
            End If
            Return ReadHexString(startPos)
        End If

        ' >> 或孤立的 >
        If b = 62 Then ' >
            If _pos + 1 < _length AndAlso _data(_pos + 1) = 62 Then
                _pos += 2
                Return New PdfToken(PdfTokenType.DictClose, ">>", Nothing, 0, startPos)
            End If
            _pos += 1
            Return NextToken()
        End If

        If b = 91 Then ' [
            _pos += 1
            Return New PdfToken(PdfTokenType.ArrayOpen, "[", Nothing, 0, startPos)
        End If

        If b = 93 Then ' ]
            _pos += 1
            Return New PdfToken(PdfTokenType.ArrayClose, "]", Nothing, 0, startPos)
        End If

        If b = 47 Then ' /
            Return ReadName(startPos)
        End If

        If b = 40 Then ' (
            Return ReadLiteralString(startPos)
        End If

        ' 数字或关键字
        Return ReadNumberOrKeyword(startPos)
    End Function

    Private Function ReadName(startPos As Integer) As PdfToken
        _pos += 1 ' 跳过 /
        Dim sb As New StringBuilder()
        While _pos < _length
            Dim b = _data(_pos)
            If IsWhitespace(b) OrElse IsDelimiter(b) Then Exit While
            If b = 35 AndAlso _pos + 2 < _length Then ' #xx 十六进制转义
                Dim hex As String = ChrW(_data(_pos + 1)) & ChrW(_data(_pos + 2))
                Dim val As Integer
                If Integer.TryParse(hex, Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture, val) Then
                    sb.Append(ChrW(val))
                    _pos += 3
                    Continue While
                End If
            End If
            sb.Append(ChrW(b))
            _pos += 1
        End While
        Return New PdfToken(PdfTokenType.Name, sb.ToString(), Nothing, 0, startPos)
    End Function

    Private Function ReadLiteralString(startPos As Integer) As PdfToken
        _pos += 1 ' 跳过 (
        Dim depth = 1
        Dim bytes As New List(Of Byte)()
        While _pos < _length AndAlso depth > 0
            Dim b = _data(_pos)
            If b = 92 Then ' \
                _pos += 1
                If _pos >= _length Then Exit While
                Dim esc = _data(_pos)
                Select Case esc
                    Case 110  ' n
                        bytes.Add(10) : _pos += 1
                    Case 114  ' r
                        bytes.Add(13) : _pos += 1
                    Case 116  ' t
                        bytes.Add(9) : _pos += 1
                    Case 98   ' b
                        bytes.Add(8) : _pos += 1
                    Case 102  ' f
                        bytes.Add(12) : _pos += 1
                    Case 40   ' (
                        bytes.Add(40) : _pos += 1
                    Case 41   ' )
                        bytes.Add(41) : _pos += 1
                    Case 92   ' \
                        bytes.Add(92) : _pos += 1
                    Case 13, 10 ' 行续接
                        If esc = 13 AndAlso _pos + 1 < _length AndAlso _data(_pos + 1) = 10 Then _pos += 1
                        _pos += 1
                    Case Else
                        ' 八进制转义 \ddd
                        If esc >= 48 AndAlso esc <= 55 Then
                            Dim oct As New StringBuilder()
                            oct.Append(ChrW(esc))
                            _pos += 1
                            For i = 1 To 2
                                If _pos < _length AndAlso _data(_pos) >= 48 AndAlso _data(_pos) <= 55 Then
                                    oct.Append(ChrW(_data(_pos)))
                                    _pos += 1
                                Else
                                    Exit For
                                End If
                            Next
                            Dim val As Integer = Convert.ToInt32(oct.ToString(), 8)
                            bytes.Add(CByte(val And &HFF))
                        Else
                            bytes.Add(esc) : _pos += 1
                        End If
                End Select
            ElseIf b = 40 Then ' (
                depth += 1
                bytes.Add(b)
                _pos += 1
            ElseIf b = 41 Then ' )
                depth -= 1
                If depth > 0 Then bytes.Add(b)
                _pos += 1
            Else
                bytes.Add(b)
                _pos += 1
            End If
        End While
        Dim raw = bytes.ToArray()
        Dim s = DecodePdfString(raw)
        Return New PdfToken(PdfTokenType.LiteralString, s, raw, 0, startPos)
    End Function

    Private Function ReadHexString(startPos As Integer) As PdfToken
        _pos += 1 ' 跳过 <
        Dim hexStr As New StringBuilder()
        While _pos < _length AndAlso _data(_pos) <> 62 ' >
            Dim b = _data(_pos)
            If Not IsWhitespace(b) Then
                hexStr.Append(ChrW(b))
            End If
            _pos += 1
        End While
        If _pos < _length Then _pos += 1 ' 跳过 >
        ' 奇数位补 0
        If hexStr.Length Mod 2 = 1 Then hexStr.Append("0"c)
        Dim raw As Byte() = Nothing
        If hexStr.Length >= 2 Then
            ReDim raw(hexStr.Length \ 2 - 1)
            For i = 0 To raw.Length - 1
                raw(i) = Convert.ToByte(hexStr.ToString().Substring(i * 2, 2), 16)
            Next
        Else
            raw = New Byte(-1) {}
        End If
        Dim s = DecodePdfString(raw)
        Return New PdfToken(PdfTokenType.HexString, s, raw, 0, startPos)
    End Function

    Private Function ReadNumberOrKeyword(startPos As Integer) As PdfToken
        Dim sb As New StringBuilder()
        While _pos < _length
            Dim b = _data(_pos)
            If IsWhitespace(b) OrElse IsDelimiter(b) Then Exit While
            sb.Append(ChrW(b))
            _pos += 1
        End While
        Dim text = sb.ToString()
        If text = "" Then Return New PdfToken(PdfTokenType.EOF, Nothing, Nothing, 0, startPos)

        Select Case text
            Case "true" : Return New PdfToken(PdfTokenType.Keyword, "true", Nothing, 1, startPos)
            Case "false" : Return New PdfToken(PdfTokenType.Keyword, "false", Nothing, 0, startPos)
            Case "null" : Return New PdfToken(PdfTokenType.Keyword, "null", Nothing, 0, startPos)
            Case "obj" : Return New PdfToken(PdfTokenType.Obj, text, Nothing, 0, startPos)
            Case "endobj" : Return New PdfToken(PdfTokenType.EndObj, text, Nothing, 0, startPos)
            Case "stream" : Return New PdfToken(PdfTokenType.Stream, text, Nothing, 0, startPos)
            Case "endstream" : Return New PdfToken(PdfTokenType.EndStream, text, Nothing, 0, startPos)
            Case "xref" : Return New PdfToken(PdfTokenType.XRef, text, Nothing, 0, startPos)
            Case "trailer" : Return New PdfToken(PdfTokenType.Trailer, text, Nothing, 0, startPos)
            Case "startxref" : Return New PdfToken(PdfTokenType.StartXRef, text, Nothing, 0, startPos)
            Case "R" : Return New PdfToken(PdfTokenType.Keyword, "R", Nothing, 0, startPos)
        End Select

        ' 数字？
        Dim numVal As Double
        If Double.TryParse(text, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, numVal) Then
            Return New PdfToken(PdfTokenType.Number, text, Nothing, numVal, startPos)
        End If

        ' 未知关键字
        Return New PdfToken(PdfTokenType.Keyword, text, Nothing, 0, startPos)
    End Function

    ''' <summary>解码 PDF 字符串（字面量或十六进制）为 Unicode。</summary>
    Public Shared Function DecodePdfString(bytes As Byte()) As String
        If bytes Is Nothing OrElse bytes.Length = 0 Then Return ""
        ' UTF-16BE BOM
        If bytes.Length >= 2 AndAlso bytes(0) = &HFE AndAlso bytes(1) = &HFF Then
            Return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2)
        End If
        ' 否则按 Latin-1 / PDFDocEncoding（近似）处理
        Return Encoding.GetEncoding("ISO-8859-1").GetString(bytes)
    End Function

    ''' <summary>按指定长度读取流二进制数据（位于 stream 关键字之后）。</summary>
    Public Function ReadStreamData(length As Integer) As Byte()
        SkipStreamEOL()
        If length <= 0 Then Return New Byte(-1) {}
        If _pos + length > _length Then length = _length - _pos
        If length <= 0 Then Return New Byte(-1) {}
        Dim result(length - 1) As Byte
        Array.Copy(_data, _pos, result, 0, length)
        _pos += length
        Return result
    End Function

    ''' <summary>当 /Length 未知或为引用时，扫描至 endstream 关键字读取流数据。</summary>
    Public Function ReadStreamDataScan() As Byte()
        SkipStreamEOL()
        Dim pattern As Byte() = Encoding.ASCII.GetBytes("endstream")
        Dim startPos = _pos
        Dim foundAt = -1
        For i = startPos To _length - pattern.Length
            Dim match = True
            For j = 0 To pattern.Length - 1
                If _data(i + j) <> pattern(j) Then
                    match = False
                    Exit For
                End If
            Next
            If match Then
                foundAt = i
                Exit For
            End If
        Next
        If foundAt < 0 Then
            Dim n = _length - startPos
            If n <= 0 Then Return New Byte(-1) {}
            Dim result0(n - 1) As Byte
            Array.Copy(_data, startPos, result0, 0, n)
            _pos = _length
            Return result0
        End If
        ' 去掉 endstream 前的 EOL
        Dim endPos = foundAt
        If endPos > startPos AndAlso _data(endPos - 1) = 10 Then endPos -= 1
        If endPos > startPos AndAlso _data(endPos - 1) = 13 Then endPos -= 1
        Dim length = endPos - startPos
        If length <= 0 Then Return New Byte(-1) {}
        Dim result(length - 1) As Byte
        Array.Copy(_data, startPos, result, 0, length)
        _pos = foundAt
        Return result
    End Function

    Private Sub SkipStreamEOL()
        ' stream 关键字后跟 CRLF 或 LF
        If _pos < _length AndAlso _data(_pos) = 13 Then
            _pos += 1
            If _pos < _length AndAlso _data(_pos) = 10 Then _pos += 1
        ElseIf _pos < _length AndAlso _data(_pos) = 10 Then
            _pos += 1
        End If
    End Sub

End Class



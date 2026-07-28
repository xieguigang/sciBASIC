#Region "Microsoft.VisualBasic::c49eb5e99333c6d480bd6bd6b760cad6, mime\application%pdf\PdfReader\ToUnicodeCMap.vb"

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

    '   Total Lines: 214
    '    Code Lines: 162 (75.70%)
    ' Comment Lines: 33 (15.42%)
    '    - Xml Docs: 9.09%
    ' 
    '   Blank Lines: 19 (8.88%)
    '     File Size: 8.11 KB


    ' Class ToUnicodeCMap
    ' 
    '     Properties: MaxCode
    ' 
    '     Function: HasMapping, HexToInt, HexToUnicodeString, IncrementLastChar, Lookup
    '               ReadHexToken, SkipTo
    ' 
    '     Sub: Parse, ParseBfChar, ParseBfCharSection, ParseBfRange, ParseBfRangeSection
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
'  ToUnicodeCMap.vb  -  ToUnicode CMap 解析器
'  ----------------------------------------------------------------------------
'  ToUnicode CMap 把字体内部的字符码（1 或 2 字节）映射到 Unicode。
'  典型结构：
'    /CIDInit ... begincmap
'    1 begincodespacerange
'    <0000> <FFFF>
'    endcodespacerange
'    N beginbfchar
'    <src> <dest>
'    endbfchar
'    M beginbfrange
'    <lo> <hi> <base>      ; 基础码点递增
'    <lo> <hi> [<d1> <d2>] ; 显式数组
'    endbfrange
'    endcmap
'  本解析器提取 bfchar 与 bfrange，构造 码 -> Unicode 字符串 的映射。
' ============================================================================

Imports System.Text

Public Class ToUnicodeCMap
    Private ReadOnly _bfChars As New Dictionary(Of Integer, String)()
    Private _maxCode As Integer = 0

    ''' <summary>解析 CMap 字节流。</summary>
    Public Sub Parse(data As Byte())
        If data Is Nothing OrElse data.Length = 0 Then Return
        Dim text = Encoding.ASCII.GetString(data)
        ParseBfChar(text)
        ParseBfRange(text)
    End Sub

    Public Function Lookup(code As Integer) As String
        Dim s As String = Nothing
        If _bfChars.TryGetValue(code, s) Then Return s
        Return Nothing
    End Function

    Public Function HasMapping() As Boolean
        Return _bfChars.Count > 0
    End Function

    Public ReadOnly Property MaxCode As Integer
        Get
            Return _maxCode
        End Get
    End Property

    ' ---------------- bfchar ----------------

    Private Sub ParseBfChar(text As String)
        Dim pos = 0
        Do
            Dim idx = text.IndexOf("beginbfchar", pos, StringComparison.Ordinal)
            If idx < 0 Then Exit Do
            Dim endIdx = text.IndexOf("endbfchar", idx + 10, StringComparison.Ordinal)
            If endIdx < 0 Then Exit Do
            Dim section = text.Substring(idx + 10, endIdx - idx - 10)
            ParseBfCharSection(section)
            pos = endIdx + 9
        Loop
    End Sub

    Private Sub ParseBfCharSection(section As String)
        Dim i = 0
        Do While i < section.Length
            i = SkipTo(section, i, "<"c)
            If i < 0 Then Exit Do
            Dim srcHex = ReadHexToken(section, i, i)
            If srcHex Is Nothing Then Exit Do
            Dim src = HexToInt(srcHex)
            i = SkipTo(section, i, "<"c)
            If i < 0 Then Exit Do
            Dim destHex = ReadHexToken(section, i, i)
            If destHex Is Nothing Then Exit Do
            Dim dest = HexToUnicodeString(destHex)
            _bfChars(src) = dest
            If src > _maxCode Then _maxCode = src
        Loop
    End Sub

    ' ---------------- bfrange ----------------

    Private Sub ParseBfRange(text As String)
        Dim pos = 0
        Do
            Dim idx = text.IndexOf("beginbfrange", pos, StringComparison.Ordinal)
            If idx < 0 Then Exit Do
            Dim endIdx = text.IndexOf("endbfrange", idx + 12, StringComparison.Ordinal)
            If endIdx < 0 Then Exit Do
            Dim section = text.Substring(idx + 12, endIdx - idx - 12)
            ParseBfRangeSection(section)
            pos = endIdx + 10
        Loop
    End Sub

    Private Sub ParseBfRangeSection(section As String)
        Dim i = 0
        Do While i < section.Length
            i = SkipTo(section, i, "<"c)
            If i < 0 Then Exit Do
            Dim loHex = ReadHexToken(section, i, i)
            If loHex Is Nothing Then Exit Do
            Dim lo = HexToInt(loHex)
            i = SkipTo(section, i, "<"c)
            If i < 0 Then Exit Do
            Dim hiHex = ReadHexToken(section, i, i)
            If hiHex Is Nothing Then Exit Do
            Dim hi = HexToInt(hiHex)
            ' 跳过空白
            While i < section.Length AndAlso Char.IsWhiteSpace(section(i))
                i += 1
            End While
            If i >= section.Length Then Exit Do
            If section(i) = "["c Then
                ' 数组形式：<lo> <hi> [<d1> <d2> ...]
                i += 1
                For code = lo To hi
                    i = SkipTo(section, i, "<"c)
                    If i < 0 Then Exit For
                    Dim dHex = ReadHexToken(section, i, i)
                    If dHex Is Nothing Then Exit For
                    _bfChars(code) = HexToUnicodeString(dHex)
                    If code > _maxCode Then _maxCode = code
                Next
                ' 跳过 ]
                While i < section.Length AndAlso section(i) <> "]"c
                    i += 1
                End While
                If i < section.Length Then i += 1
            ElseIf section(i) = "<"c Then
                Dim destHex = ReadHexToken(section, i, i)
                If destHex Is Nothing Then Exit Do
                Dim baseStr = HexToUnicodeString(destHex)
                ' 基础码点递增：lo->base, lo+1->base+1, ...
                For code = lo To hi
                    _bfChars(code) = baseStr
                    If code > _maxCode Then _maxCode = code
                    baseStr = IncrementLastChar(baseStr)
                Next
            End If
        Loop
    End Sub

    ' ---------------- 辅助函数 ----------------

    Private Shared Function SkipTo(s As String, start As Integer, ch As Char) As Integer
        Dim i = start
        While i < s.Length AndAlso s(i) <> ch
            i += 1
        End While
        If i >= s.Length Then Return -1
        Return i
    End Function

    Private Shared Function ReadHexToken(s As String, start As Integer, ByRef endPos As Integer) As String
        ' start 指向 '<'
        Dim i = start + 1
        Dim sb As New StringBuilder()
        While i < s.Length AndAlso s(i) <> ">"c
            sb.Append(s(i))
            i += 1
        End While
        If i >= s.Length Then
            endPos = i
            Return Nothing
        End If
        endPos = i + 1
        Return sb.ToString()
    End Function

    Private Shared Function HexToInt(hex As String) As Integer
        If String.IsNullOrEmpty(hex) Then Return 0
        Return Convert.ToInt32(hex, 16)
    End Function

    ''' <summary>把十六进制串解释为 UTF-16BE 字符串。</summary>
    Private Shared Function HexToUnicodeString(hex As String) As String
        If String.IsNullOrEmpty(hex) Then Return ""
        If hex.Length Mod 2 <> 0 Then hex &= "0"
        Dim bytes(hex.Length \ 2 - 1) As Byte
        For i = 0 To bytes.Length - 1
            bytes(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
        Next
        ' 偶数字节直接按 UTF-16BE 解码
        If bytes.Length Mod 2 = 0 Then
            Return Encoding.BigEndianUnicode.GetString(bytes)
        End If
        ' 奇数字节补零
        Dim padded(bytes.Length) As Byte
        Array.Copy(bytes, padded, bytes.Length)
        Return Encoding.BigEndianUnicode.GetString(padded)
    End Function

    ''' <summary>字符串最后一个码点 +1，用于 bfrange 递增。</summary>
    Private Shared Function IncrementLastChar(s As String) As String
        If String.IsNullOrEmpty(s) Then Return s
        Dim chars = s.ToCharArray()
        Dim lastIdx = chars.Length - 1
        ' 处理代理对：若最后是低位代理，则递增其对应码点
        If Char.IsLowSurrogate(chars(lastIdx)) AndAlso lastIdx > 0 AndAlso Char.IsHighSurrogate(chars(lastIdx - 1)) Then
            Dim hi = Char.ConvertToUtf32(chars(lastIdx - 1), chars(lastIdx))
            Dim newCode = hi + 1
            Dim newStr = Char.ConvertFromUtf32(newCode)
            Return New String(chars, 0, lastIdx - 1) & newStr
        End If
        Dim cp = AscW(chars(lastIdx))
        Dim newChar = ChrW(cp + 1)
        Return New String(chars, 0, lastIdx) & newChar
    End Function

End Class

#Region "Microsoft.VisualBasic::b2c3d4e5f60718293a4b5c6d7e8f901, mime\application%pdf\PdfWriter\PdfFontResource.vb"

    ' Author:
    ' 
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2026 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

    ' Class PdfFontResource
    ' 
    '     Function: LatinFont, CJKFontName, CharWidth, IsCJK, EncodeCJKHex,
    '               GetRegistry, MeasureText
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfFontResource.vb - 字体资源管理（引用系统字体，不嵌入）
'
' 西文映射到 PDF 标准 14 字体（Helvetica / Helvetica-Bold / Helvetica-Oblique /
' Helvetica-BoldOblique / Courier / Courier-Bold）配 /WinAnsiEncoding；
' 中文构建 /Type0 + /Encoding /UniGB-UCS2-H + /CIDFontType0 后代字体 +
' /CIDSystemInfo(Adobe,GB1)，/FontDescriptor 不含 /FontFile2（引用不嵌入）。
'
' 字符宽度测量：标准字体内置宽度表（Helvetica AFM 近似）+ CJK 按全角估算。
' 字体解析在此隔离，未来若要改为嵌入子集，只需替换本类。
' ============================================================================

Imports System.Text

''' <summary>
''' 一个已注册的 PDF 字体资源条目，供 <see cref="PdfWriter"/> 序列化为 /Font 字典。
''' </summary>
Public Class PdfFontEntry

    ''' <summary>资源名（如 F1 / CJK）。</summary>
    Public Name As String
    ''' <summary>是否为 Type0 复合字体（CJK）。</summary>
    Public IsType0 As Boolean
    ''' <summary>BaseFont 名称（标准字体或 CID 字体名）。</summary>
    Public BaseFont As String
    ''' <summary>西文编码（标准字体用，如 /WinAnsiEncoding）。</summary>
    Public Encoding As String

End Class

''' <summary>
''' 字体资源管理器。集中管理与解析文档所用字体并提供字符宽度测量与文本编码。
''' 不嵌入任何字体文件，依赖阅读器本地具备相应字体进行替换渲染。
''' </summary>
Public Class PdfFontResource

    ''' <summary>标准 14 字体中 Helvetica 各可见 ASCII 字符的宽度（单位 1/1000 em）。</summary>
    Private Shared ReadOnly HelveticaWidths As Dictionary(Of Integer, Integer) = InitHelveticaWidths()

    ''' <summary>已注册字体表。</summary>
    Private ReadOnly registry As New List(Of PdfFontEntry)()

    ''' <summary>CJK Type0 字体是否已注册。</summary>
    Private cjkRegistered As Boolean = False

    Public Sub New()
        ' 注册西文标准字体（引用不嵌入）
        RegisterStandard("F1", "Helvetica", "/WinAnsiEncoding")
        RegisterStandard("F2", "Helvetica-Bold", "/WinAnsiEncoding")
        RegisterStandard("F3", "Helvetica-Oblique", "/WinAnsiEncoding")
        RegisterStandard("F4", "Helvetica-BoldOblique", "/WinAnsiEncoding")
        RegisterStandard("F5", "Courier", "/WinAnsiEncoding")
        RegisterStandard("F6", "Courier-Bold", "/WinAnsiEncoding")
    End Sub

    Private Sub RegisterStandard(name As String, baseFont As String, encoding As String)
        registry.Add(New PdfFontEntry() With {
            .Name = name,
            .IsType0 = False,
            .BaseFont = baseFont,
            .Encoding = encoding
        })
    End Sub

    ''' <summary>选择适合给定粗斜体样式的西文字体资源名。</summary>
    Public Function LatinFont(bold As Boolean, italic As Boolean, code As Boolean) As String
        If code Then
            Return If(bold, "F6", "F5")
        End If
        If bold AndAlso italic Then Return "F4"
        If bold Then Return "F2"
        If italic Then Return "F3"
        Return "F1"
    End Function

    ''' <summary>取得或注册 CJK Type0 字体资源名（引用系统字体，不嵌入）。</summary>
    Public Function CJKFontName() As String
        If Not cjkRegistered Then
            registry.Add(New PdfFontEntry() With {
                .Name = "CJK",
                .IsType0 = True,
                .BaseFont = "Adobe-GB1",
                .Encoding = "/UniGB-UCS2-H"
            })
            cjkRegistered = True
        End If
        Return "CJK"
    End Function

    ''' <summary>返回所有已注册的字体条目，供序列化使用。</summary>
    Public Function GetRegistry() As List(Of PdfFontEntry)
        Return registry
    End Function

    ''' <summary>判断字符是否为 CJK（需要走 Type0 复合字体）。</summary>
    Public Shared Function IsCJK(c As Char) As Boolean
        Dim v = AscW(c)
        ' 常见 CJK 统一表意文字区、全角标点、片假名等
        Return (v >= &H3000 AndAlso v <= &H303F) OrElse
               (v >= &H3400 AndAlso v <= &H4DBF) OrElse
               (v >= &H4E00 AndAlso v <= &H9FFF) OrElse
               (v >= &HF900 AndAlso v <= &HFAFF) OrElse
               (v >= &HFF00 AndAlso v <= &HFFEF) OrElse
               (v >= &H30A0 AndAlso v <= &H30FF)
    End Function

    ''' <summary>
    ''' 测量字符串宽度（磅）。按 CJK / 西文分段分别累加：
    ''' CJK 每个字符按全角 1000/1000 em 估算；西文字符查 Helvetica 宽度表。
    ''' </summary>
    Public Shared Function MeasureText(text As String, size As Double) As Double
        Dim total As Double = 0
        If String.IsNullOrEmpty(text) Then Return 0
        For Each c As Char In text
            If IsCJK(c) Then
                total += 1000
            Else
                total += CharWidthLatin(c)
            End If
        Next
        Return total / 1000.0 * size
    End Function

    ''' <summary>单个西文字符宽度（单位 1/1000 em）。未知字符按 556（Helvetica 空格宽度近似）。</summary>
    Private Shared Function CharWidthLatin(c As Char) As Integer
        Dim code = AscW(c)
        If c = " "c Then Return 278
        If HelveticaWidths.ContainsKey(code) Then
            Return HelveticaWidths(code)
        End If
        If code >= 32 AndAlso code <= 126 Then
            Return 556
        End If
        ' 其他非 CJK 字符（如拉丁扩展）按全角估算
        Return 556
    End Function

    ''' <summary>
    ''' 将包含 CJK 的文本编码为 UTF-16BE 十六进制串（供 Type0 字体以 &lt;...&gt; 形式输出）。
    ''' </summary>
    Public Shared Function EncodeCJKHex(text As String) As String
        If String.IsNullOrEmpty(text) Then Return ""
        Dim sb As New StringBuilder()
        Dim bytes = Encoding.BigEndianUnicode.GetBytes(text)
        For Each b In bytes
            sb.Append(b.ToString("X2"))
        Next
        Return sb.ToString()
    End Function

    ''' <summary>初始化 Helvetica 标准宽度表（可见 ASCII 区间近似）。</summary>
    Private Shared Function InitHelveticaWidths() As Dictionary(Of Integer, Integer)
        ' 取自 Adobe Helvetica AFM 的常用宽度（1/1000 em）
        Dim d As New Dictionary(Of Integer, Integer)()
        ' 控制字符宽度忽略；空格单独处理
        Dim pairs As Integer(,) = {
            {33, 278}, {34, 355}, {35, 556}, {36, 556}, {37, 889}, {38, 667},
            {39, 191}, {40, 333}, {41, 333}, {42, 389}, {43, 584}, {44, 278},
            {45, 333}, {46, 278}, {47, 278}, {48, 556}, {49, 556}, {50, 556},
            {51, 556}, {52, 556}, {53, 556}, {54, 556}, {55, 556}, {56, 556},
            {57, 556}, {58, 278}, {59, 278}, {60, 584}, {61, 584}, {62, 584},
            {63, 556}, {64, 1015}, {65, 667}, {66, 667}, {67, 722}, {68, 722},
            {69, 667}, {70, 611}, {71, 778}, {72, 722}, {73, 278}, {74, 500},
            {75, 667}, {76, 556}, {77, 833}, {78, 722}, {79, 778}, {80, 667},
            {81, 778}, {82, 722}, {83, 667}, {84, 611}, {85, 722}, {86, 667},
            {87, 944}, {88, 667}, {89, 667}, {90, 611}, {91, 278}, {92, 278},
            {93, 278}, {94, 469}, {95, 556}, {96, 333}, {97, 556}, {98, 556},
            {99, 500}, {100, 556}, {101, 556}, {102, 278}, {103, 556},
            {104, 556}, {105, 222}, {106, 222}, {107, 500}, {108, 222},
            {109, 833}, {110, 556}, {111, 556}, {112, 556}, {113, 556},
            {114, 333}, {115, 500}, {116, 278}, {117, 556}, {118, 500},
            {119, 722}, {120, 500}, {121, 500}, {122, 500}, {123, 334},
            {124, 260}, {125, 334}, {126, 584}
        }
        For i = 0 To pairs.GetLength(0) - 1
            d(pairs(i, 0)) = pairs(i, 1)
        Next
        Return d
    End Function

End Class

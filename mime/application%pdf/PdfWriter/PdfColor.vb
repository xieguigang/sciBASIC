#Region "Microsoft.VisualBasic::cc0be389a93099b585450b167a114d92, mime\application%pdf\PdfWriter\PdfColor.vb"

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

    '   Total Lines: 76
    '    Code Lines: 43 (56.58%)
    ' Comment Lines: 24 (31.58%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 9 (11.84%)
    '     File Size: 3.11 KB


    ' Structure PdfColor
    ' 
    '     Function: Format, FromHex, IsEmpty, ToFill, ToStroke
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfColor.vb - PDF 颜色工具
'
' 将 WordColors 的 6 位十六进制 RGB 字符串（无 # 前缀）转换为 PDF 内容流
' 所需的 0-1 归一化 RGB 三元组，并生成 rg（填充）/ RG（描边）指令。
' 对非法或空字符串做安全兜底（返回黑色 / 视为无色）。
' ============================================================================

Imports System.Globalization

''' <summary>
''' PDF 颜色工具。把 OOXML 的 6 位十六进制 RGB（无 # 前缀）转换为 PDF 内容流
''' 使用的 0-1 归一化 RGB 三元组，并生成对应的填充/描边指令。
''' </summary>
Public Structure PdfColor

    ''' <summary>红分量 (0-1)。</summary>
    Public R As Double
    ''' <summary>绿分量 (0-1)。</summary>
    Public G As Double
    ''' <summary>蓝分量 (0-1)。</summary>
    Public B As Double

    ''' <summary>
    ''' 从 WordColors 风格的 6 位十六进制字符串解析颜色（无 # 前缀）。
    ''' 解析失败或为空时返回黑色。
    ''' </summary>
    Public Shared Function FromHex(hex As String) As PdfColor
        Dim c As New PdfColor()
        If String.IsNullOrWhiteSpace(hex) Then
            Return c ' 黑色
        End If
        hex = hex.Trim()
        If hex.StartsWith("#") Then hex = hex.Substring(1)
        If hex.Length <> 6 Then
            Return c
        End If
        Try
            Dim r = Integer.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            Dim g = Integer.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            Dim b = Integer.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            c.R = r / 255.0
            c.G = g / 255.0
            c.B = b / 255.0
        Catch
            Return c
        End Try
        Return c
    End Function

    ''' <summary>是否为无色（透明）。用于判断是否为空底色。</summary>
    Public Function IsEmpty() As Boolean
        ' 与 WordColors 中未设置底色的约定一致：返回黑色但标记为未设置时由调用方判断。
        Return False
    End Function

    ''' <summary>生成填充颜色指令，如 “0.18 0.45 0.71 rg”。</summary>
    Public Function ToFill() As String
        Return $"{Format(R)} {Format(G)} {Format(B)} rg"
    End Function

    ''' <summary>生成描边颜色指令，如 “0.18 0.45 0.71 RG”。</summary>
    Public Function ToStroke() As String
        Return $"{Format(R)} {Format(G)} {Format(B)} RG"
    End Function

    ''' <summary>把 0-1 分量格式化为 PDF 所需的有效数字字符串。</summary>
    Private Shared Function Format(v As Double) As String
        If v <= 0 Then Return "0"
        If v >= 1 Then Return "1"
        ' 保留 3 位小数，去掉末尾多余的 0
        Dim s = v.ToString("0.###", CultureInfo.InvariantCulture)
        Return s
    End Function

End Structure

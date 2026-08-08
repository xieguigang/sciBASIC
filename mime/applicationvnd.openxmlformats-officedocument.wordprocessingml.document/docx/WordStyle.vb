#Region "Microsoft.VisualBasic::ece5e22f67d737e3e1c6dd264475589d, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\docx\WordStyle.vb"

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

    '   Total Lines: 121
    '    Code Lines: 54 (44.63%)
    ' Comment Lines: 40 (33.06%)
    '    - Xml Docs: 77.50%
    ' 
    '   Blank Lines: 27 (22.31%)
    '     File Size: 4.61 KB


    ' Class WordColors
    ' 
    ' 
    ' 
    ' Class WordStyle
    ' 
    '     Properties: Alignment, BackColor, Bold, FirstLineIndent, FontName
    '                 FontNameEastAsia, ForeColor, Italic, LineSpacing, Size
    '                 SpaceAfter, SpaceBefore, Underline
    ' 
    '     Function: Clone
    ' 
    ' Class TableStyle
    ' 
    '     Properties: AltRowBackColor, BorderColor, BorderSize, CellPadding, HeaderBackColor
    '                 HeaderBold, HeaderForeColor
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' WordStyle.vb - Word 文档样式定义
'
' 定义文档中使用的所有样式：
'   - WordStyle: 段落/标题/正文 的字体样式
'   - TableStyle: 表格样式
'   - WordColors: 常用颜色常量
' ============================================================================


''' <summary>
''' 常用颜色常量（使用 OOXML 的 6 位十六进制 RGB 格式）。
''' </summary>
Public Class WordColors
    Public Const Black As String = "000000"
    Public Const White As String = "FFFFFF"
    Public Const Red As String = "FF0000"
    Public Const Blue As String = "0000FF"
    Public Const Green As String = "008000"
    Public Const Yellow As String = "FFFF00"
    Public Const Cyan As String = "00FFFF"
    Public Const Magenta As String = "FF00FF"
    Public Const Gray As String = "808080"
    Public Const DarkGray As String = "404040"
    Public Const LightGray As String = "D9D9D9"
    Public Const Navy As String = "000080"
    Public Const DarkBlue As String = "00008B"
    Public Const Orange As String = "FFA500"
    Public Const Purple As String = "800080"
    Public Const Brown As String = "A52A2A"
    ' 主题色
    Public Const Heading1Color As String = "2E74B5"
    Public Const Heading2Color As String = "2E74B5"
    Public Const Heading3Color As String = "1F4D78"
    Public Const TableHeaderBg As String = "4472C4"
    Public Const TableHeaderFg As String = "FFFFFF"
    Public Const TableAltRowBg As String = "F2F2F2"
    Public Const CodeBg As String = "F5F5F5"
    Public Const QuoteBg As String = "FFF8E1"
    Public Const LinkColor As String = "0563C1"
End Class

''' <summary>
''' Word 文档文字样式。
''' 控制字体名称、字号、粗体/斜体/下划线、前景色/背景色、对齐方式、行间距等。
''' </summary>
Public Class WordStyle

    ''' <summary>西文字体名称（如 "Calibri"）。</summary>
    Public Property FontName As String = "Calibri"

    ''' <summary>东亚字体名称（如 "Microsoft YaHei"）。</summary>
    Public Property FontNameEastAsia As String = "Microsoft YaHei"

    ''' <summary>字号（磅，如 12 表示 12pt）。</summary>
    Public Property Size As Double = 11

    ''' <summary>是否粗体。</summary>
    Public Property Bold As Boolean = False

    ''' <summary>是否斜体。</summary>
    Public Property Italic As Boolean = False

    ''' <summary>是否下划线。</summary>
    Public Property Underline As Boolean = False

    ''' <summary>前景色（文字颜色），6 位十六进制 RGB。</summary>
    Public Property ForeColor As String = WordColors.Black

    ''' <summary>背景色（底纹），空字符串表示无底纹。</summary>
    Public Property BackColor As String = ""

    ''' <summary>对齐方式：left / center / right / justify。</summary>
    Public Property Alignment As String = "left"

    ''' <summary>行间距倍数（1.0 / 1.15 / 1.5 / 2.0）。</summary>
    Public Property LineSpacing As Double = 1.15

    ''' <summary>段前间距（磅）。</summary>
    Public Property SpaceBefore As Double = 0

    ''' <summary>段后间距（磅）。</summary>
    Public Property SpaceAfter As Double = 6

    ''' <summary>首行缩进（磅，0 表示无缩进）。</summary>
    Public Property FirstLineIndent As Double = 0

    ''' <summary>创建深拷贝。</summary>
    Public Function Clone() As WordStyle
        Return DirectCast(Me.MemberwiseClone(), WordStyle)
    End Function

End Class

''' <summary>
''' Word 表格样式。
''' </summary>
Public Class TableStyle

    ''' <summary>表头背景色。</summary>
    Public Property HeaderBackColor As String = WordColors.TableHeaderBg

    ''' <summary>表头文字颜色。</summary>
    Public Property HeaderForeColor As String = WordColors.TableHeaderFg

    ''' <summary>表头是否加粗。</summary>
    Public Property HeaderBold As Boolean = True

    ''' <summary>边框颜色。</summary>
    Public Property BorderColor As String = WordColors.Black

    ''' <summary>边框粗细（以 1/8 pt 为单位，4 = 0.5pt, 8 = 1pt）。</summary>
    Public Property BorderSize As Integer = 4

    ''' <summary>交替行背景色（空字符串表示不交替）。</summary>
    Public Property AltRowBackColor As String = WordColors.TableAltRowBg

    ''' <summary>单元格内边距（twips）。</summary>
    Public Property CellPadding As Integer = 120

End Class


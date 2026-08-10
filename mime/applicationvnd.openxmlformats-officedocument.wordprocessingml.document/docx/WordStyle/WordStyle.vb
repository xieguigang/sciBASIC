#Region "Microsoft.VisualBasic::af20f97699404edb565f06f1b8c18e0a, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\docx\WordStyle\WordStyle.vb"

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

    '   Total Lines: 60
    '    Code Lines: 18 (30.00%)
    ' Comment Lines: 26 (43.33%)
    '    - Xml Docs: 69.23%
    ' 
    '   Blank Lines: 16 (26.67%)
    '     File Size: 2.28 KB


    ' Class WordStyle
    ' 
    '     Properties: Alignment, BackColor, Bold, FirstLineIndent, FontName
    '                 FontNameEastAsia, ForeColor, Italic, LineSpacing, Size
    '                 SpaceAfter, SpaceBefore, Underline
    ' 
    '     Function: Clone
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

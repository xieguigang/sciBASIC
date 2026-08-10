#Region "Microsoft.VisualBasic::7c4d6f0700827f98d4416500482a0aea, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\docx\WordStyle\TableStyle.vb"

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

    '   Total Lines: 27
    '    Code Lines: 9 (33.33%)
    ' Comment Lines: 10 (37.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (29.63%)
    '     File Size: 964 B


    ' Class TableStyle
    ' 
    '     Properties: AltRowBackColor, BorderColor, BorderSize, CellPadding, HeaderBackColor
    '                 HeaderBold, HeaderForeColor
    ' 
    ' /********************************************************************************/

#End Region

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

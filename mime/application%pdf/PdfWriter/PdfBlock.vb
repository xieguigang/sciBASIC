#Region "Microsoft.VisualBasic::d4e5f60718293a4b5c6d7e8f9012345, mime\application%pdf\PdfWriter\PdfBlock.vb"

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

    ' Enum PdfBlockType
    ' 
    ' Class PdfBlock
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' PdfBlock.vb - 内容块模型
'
' 将文档写入 API（标题/正文/表格/图片/...）收集为中间内容块队列，
' 在 Save 时由布局引擎统一做换行、分页与绘制。样式必须在入队时
' Clone 快照，避免后续 XxxStyle() 调用回溯影响已写入内容。
' ============================================================================

''' <summary>文档内容块类型。</summary>
Public Enum PdfBlockType
    ''' <summary>文档大标题。</summary>
    Title
    ''' <summary>分级标题。</summary>
    Heading
    ''' <summary>正文段落。</summary>
    Paragraph
    ''' <summary>代码块。</summary>
    Code
    ''' <summary>引用块。</summary>
    Quote
    ''' <summary>列表项（有序/无序）。</summary>
    List
    ''' <summary>任务列表项。</summary>
    TaskList
    ''' <summary>定义列表项。</summary>
    DefList
    ''' <summary>水平分割线。</summary>
    Hr
    ''' <summary>分页符。</summary>
    PageBreak
    ''' <summary>目录占位。</summary>
    Toc
    ''' <summary>表格。</summary>
    Table
    ''' <summary>图片。</summary>
    Image
End Enum

''' <summary>
''' 单个文档内容块。承载文本、级别、样式快照、表格/图片数据等，
''' 由 <see cref="PdfDocument"/> 收集并在 <see cref="PdfLayoutEngine"/> 中消费。
''' </summary>
Public Class PdfBlock

    ''' <summary>块类型。</summary>
    Public Type As PdfBlockType

    ''' <summary>主文本（标题/段落/代码/引用/图注等）。</summary>
    Public Text As String

    ''' <summary>标题级别（1-6），其它块忽略。</summary>
    Public Level As Integer = 1

    ''' <summary>列表有序标志。</summary>
    Public Ordered As Boolean = False

    ''' <summary>任务列表勾选状态。</summary>
    Public Checked As Boolean = False

    ''' <summary>定义列表术语（与 Text 组成 术语:定义）。</summary>
    Public Term As String

    ''' <summary>样式快照（入队时 Clone）。</summary>
    Public Style As WordStyle

    ''' <summary>表格表头。</summary>
    Public TableHeaders As String()

    ''' <summary>表格数据行（交错数组）。</summary>
    Public TableRows As String()()

    ''' <summary>单元格对齐方式。</summary>
    Public TableAlignments As String()

    ''' <summary>表格自适应模式（window/contents/等宽）。</summary>
    Public TableMode As String = "equal"

    ''' <summary>表格是否居中。</summary>
    Public TableCenter As Boolean = False

    ''' <summary>三线表样式。</summary>
    Public TableThreeLine As Boolean = False

    ''' <summary>图片路径。</summary>
    Public ImagePath As String

    ''' <summary>图片目标宽（pt，0 表示按原生比例推导）。</summary>
    Public ImageWidth As Double = 0

    ''' <summary>图片目标高（pt，0 表示按原生比例推导）。</summary>
    Public ImageHeight As Double = 0

    ''' <summary>图片图注。</summary>
    Public ImageCaption As String

End Class

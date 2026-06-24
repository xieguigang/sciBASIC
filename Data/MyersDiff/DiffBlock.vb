#Region "Microsoft.VisualBasic::c9de9404f97ff8851bcab5f620b5df8f, Data\MyersDiff\DiffBlock.vb"

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

    '   Total Lines: 30
    '    Code Lines: 14 (46.67%)
    ' Comment Lines: 11 (36.67%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 5 (16.67%)
    '     File Size: 1.24 KB


    ' Class DiffBlock
    ' 
    '     Properties: Items, NewCount, NewStart, OldCount, OldStart
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

' -----------------------------------------------------------------------
' 差异块：连续相同类型差异项的分组，用于统一差异格式输出
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异块，即连续相同类型差异项的分组。
''' </summary>
Public Class DiffBlock
    ''' <summary>该块中旧序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property OldStart As Integer

    ''' <summary>该块中旧序列的行数。</summary>
    Public Property OldCount As Integer

    ''' <summary>该块中新序列的起始行号（从 1 开始，用于显示）。</summary>
    Public Property NewStart As Integer

    ''' <summary>该块中新序列的行数。</summary>
    Public Property NewCount As Integer

    ''' <summary>该块包含的差异项列表。</summary>
    Public Property Items As New List(Of DiffItem)()

    Public Sub New(oldStart As Integer, oldCount As Integer,
                   newStart As Integer, newCount As Integer)
        Me.OldStart = oldStart
        Me.OldCount = oldCount
        Me.NewStart = newStart
        Me.NewCount = newCount
    End Sub
End Class

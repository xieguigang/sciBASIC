#Region "Microsoft.VisualBasic::bbbada10989e5ff9c0b34e481f825ea1, Data\MyersDiff\DiffItem.vb"

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

    '   Total Lines: 41
    '    Code Lines: 24 (58.54%)
    ' Comment Lines: 10 (24.39%)
    '    - Xml Docs: 70.00%
    ' 
    '   Blank Lines: 7 (17.07%)
    '     File Size: 1.53 KB


    ' Class DiffItem
    ' 
    '     Properties: NewIndex, OldIndex, Type, Value
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region



' -----------------------------------------------------------------------
' 差异项：记录单个编辑操作
' -----------------------------------------------------------------------
''' <summary>
''' 表示一个差异项，包含编辑类型、旧序列索引和新序列索引。
''' </summary>
Public Class DiffItem
    ''' <summary>编辑操作类型。</summary>
    Public Property Type As EditType

    ''' <summary>旧序列中的索引（从 0 开始）；对于 Insert 操作为 -1。</summary>
    Public Property OldIndex As Integer

    ''' <summary>新序列中的索引（从 0 开始）；对于 Delete 操作为 -1。</summary>
    Public Property NewIndex As Integer

    ''' <summary>涉及的元素值（行文本或字符）。</summary>
    Public Property Value As String

    Public Sub New(type As EditType, oldIndex As Integer, newIndex As Integer, value As String)
        Me.Type = type
        Me.OldIndex = oldIndex
        Me.NewIndex = newIndex
        Me.Value = value
    End Sub

    Public Overrides Function ToString() As String
        Select Case Type
            Case EditType.Equal
                Return String.Format("  {0}", Value)
            Case EditType.Delete
                Return String.Format("- {0} [old:{1}]", Value, OldIndex)
            Case EditType.Insert
                Return String.Format("+ {0} [new:{1}]", Value, NewIndex)
            Case Else
                Return Value
        End Select
    End Function
End Class

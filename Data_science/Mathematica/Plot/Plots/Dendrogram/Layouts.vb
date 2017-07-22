#Region "Microsoft.VisualBasic::4f61c671a58d66f9c10ab4be68d0189b, ..\sciBASIC#\Data_science\Mathematical\Plots\Dendrogram\Layouts.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.Tree
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Dendrogram

    ''' <summary>
    ''' 采用迭代进行谱系图的布局的生成
    ''' </summary>
    Public Module Layouts

        Public Class TreeLayout : Inherits TreeNodeBase(Of TreeLayout)

            Public Property Location As PointF

            Public Overrides ReadOnly Property MySelf As TreeLayout
                Get
                    Return Me
                End Get
            End Property

            Public Sub New(name As String)
                MyBase.New(name)
            End Sub

            Public Overrides Function ToString() As String
                Return Name & ": " & Location.GetJson
            End Function
        End Class

        ''' <summary>
        ''' 水平放置的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' ```
        '''      +
        '''      |
        '''      |
        ''' +----+------+
        ''' |           |
        ''' |           |
        ''' +           +
        ''' ```
        ''' 
        ''' 对于水平布局而言：
        ''' height = <see cref="Rectangle.Height"/>
        ''' width  = branch_leaf_nodes_counts/total_leaf_nodes_counts 
        ''' </remarks>
        ''' 
        <Extension>
        Public Function HorizontalLayout(tree As Tree, screen As Rectangle, Optional getDistance As Func(Of Tree, Tree, Double) = Nothing) As TreeLayout
            Dim totalLeafs = tree.LeafNodesCounts
            Dim x! = screen.Left
            Dim widths As New Dictionary(Of Tree, Single)
            Dim childLayouts As New List(Of TreeLayout)

            For Each child As Tree In tree.ChildNodes
                Dim width! = (child.LeafNodesCounts / totalLeafs) * screen.Width + 1 ' 当前的这个分支所占据的绘制宽度
                Dim dh! = screen.Height / child.MaxTravelDepth
                Dim height! = screen.Height - dh
                Dim y = screen.Top + dh
                Dim nextBlock As New Rectangle(x, y, width, height)

                Call childLayouts.Add(child.HorizontalLayout(nextBlock, getDistance))
                Call widths.Add(child, width)

                x += width
            Next

            Dim layout As New TreeLayout(tree.Name) With {
                .ChildNodes = childLayouts,
                .Location = New PointF(screen.Left + If(widths.Count = 0, screen.Left + screen.Width / 2, widths.Values.Min), screen.Top)
            }
            Return layout
        End Function

        <Extension>
        Public Function LeafNodesCounts(tree As Tree) As Integer
            Return tree.Where(Function(child) child.IsLeaf).Count
        End Function

        '''' <summary>
        '''' 垂直放置的
        '''' </summary>
        '''' <returns></returns>
        'Public Function Vertical(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double), screen As Rectangle) As BinaryTree(Of ILayoutedObject(Of T))

        'End Function

        '''' <summary>
        '''' 圆弧状的
        '''' </summary>
        '''' <returns></returns>
        '''' <remarks>
        '''' http://stackoverflow.com/questions/5089030/how-do-i-create-a-radial-cluster-like-the-following-code-example-in-python
        '''' </remarks>
        'Public Function Radial(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double), screen As Rectangle) As BinaryTree(Of ILayoutedObject(Of T))

        'End Function
    End Module
End Namespace

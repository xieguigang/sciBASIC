#Region "Microsoft.VisualBasic::a1b6e0ac151e35c04c44e3e2c1d6fb65, ..\sciBASIC#\Data_science\Mathematical\Plots\Dendrogram\Layouts.vb"

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
Imports Microsoft.VisualBasic.Imaging

Namespace Dendrogram

    Public Module Layouts

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
        ''' </remarks>
        Public Function HorizontalLayout(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double), screen As Rectangle) As BinaryTree(Of ILayoutedObject(Of T))
            Dim maxTotalHeight#
            Dim heights As New Dictionary(Of (parent As T, child As T), Double)

            With tree.Root.__calcHeights(getDistance, heights)
                heights = .heights
                maxTotalHeight = .maxTotal
            End With

            ' 计算出比例尺
            Dim scale# = screen.Height / maxTotalHeight


        End Function

        <Extension>
        Private Function __calcHeights(Of T)(parent As TreeNode(Of T),
                                             getDistance As Func(Of T, T, Double),
                                             heights As Dictionary(Of (parent As T, child As T), Double)) As (heights As Dictionary(Of (parent As T, child As T), Double), maxTotal#)

            Dim l As TreeNode(Of T) = parent.Left
            Dim r As TreeNode(Of T) = parent.Right
            Dim ld#
            Dim rd#
            Dim maxSubTotalHeights#

            If Not l Is Nothing Then
                ld = getDistance(parent.Value, l.Value)
                heights.Add((parent.Value, l.Value), ld)

                With l.__calcHeights(getDistance, heights)
                    maxSubTotalHeights = .maxTotal
                    heights = .heights
                End With
            End If
            If Not r Is Nothing Then
                rd = getDistance(parent.Value, r.Value)
                heights.Add((parent.Value, r.Value), rd)

                With r.__calcHeights(getDistance, heights)
                    maxSubTotalHeights = Math.Max(maxSubTotalHeights, .maxTotal)
                    heights = .heights
                End With
            End If

            maxSubTotalHeights += Math.Max(ld, rd)

            Return (heights, maxSubTotalHeights)
        End Function

        ''' <summary>
        ''' 垂直放置的
        ''' </summary>
        ''' <returns></returns>
        Public Function Vertical(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double), screen As Rectangle) As BinaryTree(Of ILayoutedObject(Of T))

        End Function

        ''' <summary>
        ''' 圆弧状的
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://stackoverflow.com/questions/5089030/how-do-i-create-a-radial-cluster-like-the-following-code-example-in-python
        ''' </remarks>
        Public Function Radial(Of T)(tree As BinaryTree(Of T), getDistance As Func(Of T, T, Double), screen As Rectangle) As BinaryTree(Of ILayoutedObject(Of T))

        End Function
    End Module
End Namespace

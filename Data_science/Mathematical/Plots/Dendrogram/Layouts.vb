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
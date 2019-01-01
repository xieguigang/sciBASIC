Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double
Imports Microsoft.VisualBasic.Language

Namespace Layouts.Cola.GridRouter

    Public Class GridRouter(Of Node)

        Public leaves As NodeWrapper()
        Public groups As NodeWrapper()
        Public nodes As NodeWrapper()
        Public cols As GridLine()
        Public rows As GridLine()
        Public root As NodeWrapper
        Public verts As Vert()
        Public edges
        Public backToFront
        Public obstacles
        Public passableEdges

        ''' <summary>
        ''' get the depth of the given node in the group hierarchy
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Function getDepth(v As NodeWrapper) As Double
            Dim depth As Integer = 0

            While (Not v.parent Is root)
                depth += 1
                v = v.parent
            End While

            Return depth
        End Function

        ''' <summary>
        ''' in the given axis, find sets of leaves overlapping in that axis
        ''' center of each GridLine Is average of all nodes in column
        ''' </summary>
        ''' <param name="axisOverlap"></param>
        ''' <returns></returns>
        Private Function getGridLines(axisOverlap As Func(Of Rectangle2D, Rectangle2D, Double), axisCenter As Func(Of Rectangle2D, Double)) As GridLine()
            Dim columns As New List(Of GridLine)
            Dim ls = leaves.ToList

            While (ls.Count > 0)
                ' find a column of all leaves overlapping in axis with the first leaf
                Dim overlapping As NodeWrapper() = ls.Where(Function(v) axisOverlap(v.rect, ls(0).rect) <> 0).ToArray
                Dim col As New GridLine With {
                    .nodes = overlapping,
                    .pos = overlapping.Select(Function(v) axisCenter(v.rect)).Average
                }
                columns.Add(col)

                For Each v In col.nodes
                    Call ls.Remove(v)
                Next
            End While

            Call columns.Sort(New GridLine.Comparer)

            Return columns.ToArray
        End Function

        ''' <summary>
        ''' find path from v to root including both v And root
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Private Function findLineage(v As NodeWrapper) As NodeWrapper()
            Dim lineage As New List(Of NodeWrapper) From {v}
            Do
                v = v.parent
                lineage.Add(v)
            Loop While (Not v Is root)

            Return lineage.ReverseIterator.ToArray
        End Function
    End Class
End Namespace
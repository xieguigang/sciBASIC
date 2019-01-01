Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports number = System.Double
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.ComponentModel.Collection

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

        ''' <summary>
        ''' find path connecting a And b through their lowest common ancestor
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Private Function findAncestorPathBetween(a As NodeWrapper, b As NodeWrapper) As (commonAncestor As NodeWrapper, lineages As NodeWrapper())
            Dim aa = findLineage(a), ba = findLineage(b)
            Dim i = 0

            While (aa(i) Is ba(i))
                i += 1
            End While

            ' i-1 to include common ancestor only once (as first element)
            Return (commonAncestor:=aa(i - 1), lineages:=aa.slice(i).Concat(ba.slice(i)).ToArray)
        End Function

        ''' <summary>
        ''' when finding a path between two nodes a And b, siblings of a And b on the
        ''' paths from a And b to their least common ancestor are obstacles
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Function siblingObstacles(a As NodeWrapper, b As NodeWrapper) As NodeWrapper()
            Dim path = findAncestorPathBetween(a, b)
            Dim lineageLookup As New Index(Of Integer)
            path.lineages.ForEach(Sub(v, i)
                                      lineageLookup.Add(v.id)
                                  End Sub)
            Dim obstacles = path.commonAncestor.children.Where(Function(v) lineageLookup.NotExists(v)).ToArray

            path.lineages.Where(Function(v) Not v.parent Is path.commonAncestor).ForEach(Sub(v, i)
                                                                                             obstacles = obstacles.Concat(v.parent.children.Where(Function(C) C = v.id))
                                                                                         End Sub)

            Return obstacles.Select(Function(v) nodes(v)).ToArray
        End Function

        ''' <summary>
        ''' for the given routes, extract all the segments orthogonal to the axis x
        ''' And return all them grouped by x position
        ''' </summary>
        ''' <param name="routes"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function getSegmentSets(routes As route()(), x%, y%) As List(Of segmentset)
            ' vsegments Is a list of vertical segments sorted by x position
            Dim vsegments As New List(Of route)

            For ei As Integer = 0 To routes.Length - 1
                Dim route = routes(ei)

                For si As Integer = 0 To route.Length - 1
                    Dim s = route(si)
                    s.edgeid = ei
                    s.i = si
                    Dim sdx = s(1)(x) - s(0)(x)
                    If (Math.Abs(sdx) < 0.1) Then
                        vsegments.Add(s)
                    End If
                Next
            Next

            vsegments.Sort(New route.Comparer With {.i = x})

            ' vsegmentsets Is a set of sets of segments grouped by x position
            Dim vsegmentsets As New List(Of segmentset)
            Dim segmentset As segmentset = Nothing

            For i As Integer = 0 To vsegments.Count - 1
                Dim s = vsegments(i)
                If (segmentset Is Nothing OrElse Math.Abs(s(0)(x) - segmentset.pos) > 0.1) Then
                    segmentset = New segmentset With {.pos = s(0)(x), .segments = New List(Of route)}
                    vsegmentsets.Add(segmentset)
                End If
                segmentset.segments.Add(s)
            Next

            Return vsegmentsets
        End Function
    End Class

    Public Class segmentset
        Public pos As number
        Public segments As List(Of route)

    End Class

    Public Class route
        Public edgeid As Integer
        Public i As Integer

        Public matrix As Double()()

        Default Public Property item(i As Integer) As Double()
            Get
                Return matrix(i)
            End Get
            Set(value As Double())
                matrix(i) = value
            End Set
        End Property

        Public Structure Comparer : Implements IComparer(Of route)

            Public Property i As Integer

            Public Function Compare(x As route, y As route) As Integer Implements IComparer(Of route).Compare
                Return x(0)(i) - y(0)(i)
            End Function
        End Structure
    End Class
End Namespace
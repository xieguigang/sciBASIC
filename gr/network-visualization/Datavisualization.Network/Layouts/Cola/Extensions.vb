Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports number = System.Double

Namespace Layouts.Cola

    Public Module Extensions

        Public Function compareEvents(a As [Event], b As [Event]) As number
            If (a.pos > b.pos) Then
                Return 1
            End If
            If (a.pos < b.pos) Then
                Return -1
            End If
            If (a.isOpen) Then
                ' open must come before close
                Return -1
            End If

            If (b.isOpen) Then
                ' open must come before close
                Return 1
            End If

            Return 0
        End Function


        Public Function computeGroupBounds(g As ProjectionGroup) As Rectangle2D
            g.bounds = If(Not g.leaves Is Nothing, g.leaves.reduce(Of Leaf, Rectangle2D)(Function(r As Rectangle2D, C As Leaf)
                                                                                             Return C.bounds.Union(r)
                                                                                         End Function, New Rectangle2D()), New Rectangle2D())

            If Not g.groups Is Nothing Then
                g.bounds = g.groups.reduce(Of ProjectionGroup, Rectangle2D)(Function(r As Rectangle2D, C As ProjectionGroup)
                                                                                Return computeGroupBounds(C).Union(r)
                                                                            End Function, g.bounds)
            End If

            g.bounds = g.bounds.inflate(g.padding)
            Return g.bounds
        End Function

        <Extension>
        Private Function reduce(Of T, T2, V)(seq As IEnumerable(Of T), produce As Func(Of V, T, V), init As V) As V
            For Each x As T In seq
                init = produce(init, x)
            Next

            Return init
        End Function

        ''' <summary>
        ''' Returns the endpoints of a line that connects the centre of two rectangles.
        ''' </summary>
        ''' <param name="source">The source Rectangle.</param>
        ''' <param name="target">The target Rectangle.</param>
        ''' <param name="ah">The size of the arrow head, a distance to shorten the line by.</param>
        ''' <returns></returns>
        Public Function makeEdgeBetween(source As Rectangle2D, target As Rectangle2D, ah As number) As DirectedEdge
            Dim si = source.rayIntersection(target.CenterX, target.CenterY) Or New Point2D(source.CenterX, source.CenterY).AsDefault
            Dim ti = target.rayIntersection(source.CenterX, source.CenterY) Or New Point2D(target.CenterX, target.CenterY).AsDefault
            Dim dx = ti.X - si.X
            Dim dy = ti.Y - si.Y
            Dim l = Math.Sqrt(dx * dx + dy * dy), al = l - ah

            Return New DirectedEdge With {
                .sourceIntersection = si,
                .targetIntersection = ti,
                .arrowStart = New Point2D With {
                    .X = si.X + al * dx / l,
                    .Y = si.Y + al * dy / l
                }
            }
        End Function

        ''' <summary>
        ''' Returns the intersection of a line from the given point to the centre
        ''' of the target rectangle where it intersects the rectanngle.
        ''' </summary>
        ''' <param name="s">The source point.</param>
        ''' <param name="target">The target Rectangle.</param>
        ''' <param name="ah">The size of the arrow head, a distance to shorten the
        ''' line by.</param>
        ''' <returns>The point an arrow head of the specified size would need to start.</returns>
        Public Function makeEdgeTo(s As Point2D, target As Rectangle2D, ah As number) As Point2D
            Dim ti = target.rayIntersection(s.X, s.Y)

            If (ti Is Nothing) Then
                ti = New Point2D With {
                    .X = target.CenterX,
                    .Y = target.CenterY
                }
            End If

            Dim dx = ti.X - s.X
            Dim dy = ti.Y - s.Y
            Dim l = Math.Sqrt(dx * dx + dy * dy)

            Return New Point2D With {
                .X = ti.X - ah * dx / l,
                .Y = ti.Y - ah * dy / l
            }
        End Function

        ''' <summary>
        ''' Remove overlap between spans while keeping their centers as close as possible to the specified desiredCenters.
        ''' Lower And upper bounds will be respected if the spans physically fit between them
        ''' (otherwise they'll be moved and their new position returned).
        ''' If no upper/lower bound Is specified then the bounds of the moved spans will be returned.
        ''' returns a New center for each span.
        ''' </summary>
        ''' <param name="spans"></param>
        ''' <param name="lowerBound"></param>
        ''' <param name="upperBound"></param>
        ''' <returns></returns>
        Public Function removeOverlapInOneDimension(spans As (size As number, desiredCenter As number)(), lowerBound As number, upperBound As number) As (newCenters As number(), lowerBound As number, upperBound As number)

            Dim vs As Variable() = spans.Select(Function(s) New Variable(s.desiredCenter)).ToArray
            Dim cs As New List(Of Constraint)
            Dim n = spans.Length

            For i As Integer = 0 To n - 2
                Dim left = spans(i), right = spans(i + 1)
                cs.Add(New Constraint(vs(i), vs(i + 1), (left.size + right.size) / 2))
            Next

            Dim leftMost = vs(0),
            rightMost = vs(n - 1),
            leftMostSize = spans(0).size / 2,
            rightMostSize = spans(n - 1).size / 2
            Dim vLower As Variable = Nothing, vUpper As Variable = Nothing

            If (lowerBound) Then
                vLower = New Variable(lowerBound, leftMost.weight * 1000)
                vs.Add(vLower)
                cs.Add(New Constraint(vLower, leftMost, leftMostSize))
            End If

            If (upperBound) Then
                vUpper = New Variable(upperBound, rightMost.weight * 1000)
                vs.Add(vUpper)
                cs.Add(New Constraint(rightMost, vUpper, rightMostSize))
            End If

            Dim Solver = New Solver(vs, cs)
            Solver.solve()

            Return (
            newCenters:=vs.slice(0, spans.Length).Select(Function(v) v.position()).ToArray,
            lowerBound:=If(vLower, vLower.position(), leftMost.position() - leftMostSize),
            upperBound:=If(vUpper, vUpper.position(), rightMost.position() + rightMostSize)
        )
        End Function
    End Module
End Namespace
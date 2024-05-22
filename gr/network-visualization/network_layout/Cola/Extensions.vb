#Region "Microsoft.VisualBasic::d39fdedf5ac121c70c87da4d11c08391, gr\network-visualization\network_layout\Cola\Extensions.vb"

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

    '   Total Lines: 177
    '    Code Lines: 122 (68.93%)
    ' Comment Lines: 29 (16.38%)
    '    - Xml Docs: 93.10%
    ' 
    '   Blank Lines: 26 (14.69%)
    '     File Size: 7.48 KB


    '     Class Leaf
    ' 
    '         Properties: bounds, variable
    ' 
    '     Interface ProjectionGroup
    ' 
    '         Properties: bounds, groups, leaves, maxVar, minVar
    '                     padding, stiffness
    ' 
    '     Module Extensions
    ' 
    '         Function: compareEvents, computeGroupBounds, makeEdgeBetween, makeEdgeTo, removeOverlapInOneDimension
    ' 
    '         Sub: setXCentre, setYCentre
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.My.JavaScript
Imports stdNum = System.Math

Namespace Cola

    Public Class Leaf : Inherits JavaScriptObject
        Public Overridable Property bounds As Rectangle2D
        Public Overridable Property variable As Variable
    End Class

    Public Interface ProjectionGroup
        Property bounds As Rectangle2D
        Property padding As Double?
        Property stiffness As Double?
        Property leaves As List(Of Leaf)
        Property groups As List(Of ProjectionGroup)
        Property minVar As Variable
        Property maxVar As Variable
    End Interface

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub setXCentre(rect As Rectangle2D, cx As Double)
            rect.X += (cx - rect.CenterX)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub setYCentre(rect As Rectangle2D, cy As Double)
            rect.Y += (cy - rect.CenterY)
        End Sub

        Public Function compareEvents(a As [Event], b As [Event]) As Integer
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
            g.bounds = If(Not g.leaves Is Nothing, g.leaves.reduce(Function(r As Rectangle2D, C As Leaf)
                                                                       Return C.bounds.Union(r)
                                                                   End Function, New Rectangle2D()), New Rectangle2D())

            If Not g.groups Is Nothing Then
                g.bounds = g.groups.reduce(Function(r As Rectangle2D, C As ProjectionGroup)
                                               Return computeGroupBounds(C).Union(r)
                                           End Function, g.bounds)
            End If

            g.bounds = g.bounds.inflate(g.padding)
            Return g.bounds
        End Function

        ''' <summary>
        ''' Returns the endpoints of a line that connects the centre of two rectangles.
        ''' </summary>
        ''' <param name="source">The source Rectangle.</param>
        ''' <param name="target">The target Rectangle.</param>
        ''' <param name="ah">The size of the arrow head, a distance to shorten the line by.</param>
        ''' <returns></returns>
        Public Function makeEdgeBetween(source As Rectangle2D, target As Rectangle2D, ah As Double) As DirectedEdge
            Dim si = source.rayIntersection(target.CenterX, target.CenterY) Or New Point2D(source.CenterX, source.CenterY).AsDefault
            Dim ti = target.rayIntersection(source.CenterX, source.CenterY) Or New Point2D(target.CenterX, target.CenterY).AsDefault
            Dim dx = ti.X - si.X
            Dim dy = ti.Y - si.Y
            Dim l = stdNum.Sqrt(dx * dx + dy * dy), al = l - ah

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
        Public Function makeEdgeTo(s As Point2D, target As Rectangle2D, ah As Double) As Point2D
            Dim ti = target.rayIntersection(s.X, s.Y)

            If (ti Is Nothing) Then
                ti = New Point2D With {
                    .X = target.CenterX,
                    .Y = target.CenterY
                }
            End If

            Dim dx = ti.X - s.X
            Dim dy = ti.Y - s.Y
            Dim l = stdNum.Sqrt(dx * dx + dy * dy)

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
        Public Function removeOverlapInOneDimension(spans As (size As Double, desiredCenter As Double)(), lowerBound As Double, upperBound As Double) As (newCenters As Double(), lowerBound As Double, upperBound As Double)

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

#Region "Microsoft.VisualBasic::1f349bb3b9a677d67f31852ee5185540, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\PolylineSimplify\SimplifyUtility.vb"

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

    '   Total Lines: 165
    '    Code Lines: 98 (59.39%)
    ' Comment Lines: 36 (21.82%)
    '    - Xml Docs: 47.22%
    ' 
    '   Blank Lines: 31 (18.79%)
    '     File Size: 6.54 KB


    '     Class SimplifyUtility
    ' 
    '         Function: GetSquareDistance, GetSquareSegmentDistance, Simplify, SimplifyArray, SimplifyDouglasPeucker
    '                   SimplifyRadialDistance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' High-performance polyline simplification library
'
' This is a port of simplify-js by Vladimir Agafonkin, Copyright (c) 2012
' https://github.com/mourner/simplify-js
' 
' The code is ported from JavaScript to C#.
' The library is created as portable and 
' is targeting multiple Microsoft plattforms.
'
' This library was ported by imshz @ http://www.shz.no
' https://github.com/imshz/simplify-net
'
' This code is provided as is by the author. For complete license please
' read the original license at https://github.com/mourner/simplify-js

Namespace Drawing2D.Math2D.PolylineSimplify

    ''' <summary>
    ''' Simplification of a 2D-polyline.
    ''' </summary>
    Public Class SimplifyUtility : Implements ISimplifyUtility

        ' square distance between 2 points
        Private Function GetSquareDistance(p1 As Point, p2 As Point) As Double
            Dim dx = p1.X - p2.X, dy = p1.Y - p2.Y

            Return dx * dx + dy * dy
        End Function

        ' square distance from a point to a segment
        Private Function GetSquareSegmentDistance(p As Point, p1 As Point, p2 As Point) As Double
            Dim x = p1.X
            Dim y = p1.Y
            Dim dx = p2.X - x
            Dim dy = p2.Y - y

            If Not dx.Equals(0.0) OrElse Not dy.Equals(0.0) Then
                Dim t = ((p.X - x) * dx + (p.Y - y) * dy) / (dx * dx + dy * dy)

                If t > 1 Then
                    x = p2.X
                    y = p2.Y
                ElseIf t > 0 Then
                    x += dx * t
                    y += dy * t
                End If
            End If

            dx = p.X - x
            dy = p.Y - y

            Return dx * dx + dy * dy
        End Function

        ' rest of the code doesn't care about point format

        ' basic distance-based simplification
        Private Function SimplifyRadialDistance(points As Point(), sqTolerance As Double) As List(Of Point)
            Dim prevPoint = points(0)
            Dim newPoints = New List(Of Point) From {
                prevPoint
            }
            Dim point As Point = Nothing

            For i As Integer = 1 To points.Length - 1
                point = points(i)

                If GetSquareDistance(point, prevPoint) > sqTolerance Then
                    newPoints.Add(point)
                    prevPoint = point
                End If
            Next

            If point IsNot Nothing AndAlso Not prevPoint.Equals(point) Then newPoints.Add(point)

            Return newPoints
        End Function

        ' simplification using optimized Douglas-Peucker algorithm with recursion elimination
        Private Function SimplifyDouglasPeucker(points As Point(), sqTolerance As Double) As List(Of Point)
            Dim len = points.Length
            Dim markers = New Integer?(len - 1) {}
            Dim first As Integer? = 0
            Dim last As Integer? = len - 1
            Dim index As Integer? = 0
            Dim stack = New List(Of Integer?)()
            Dim newPoints = New List(Of Point)()

            markers(last.Value) = 1
            markers(first.Value) = 1

            While last IsNot Nothing
                Dim maxSqDist = 0.0R

                For i As Integer = first + 1 To last - 1
                    Dim sqDist = GetSquareSegmentDistance(points(i), points(first.Value), points(last.Value))

                    If sqDist > maxSqDist Then
                        index = i
                        maxSqDist = sqDist
                    End If
                Next

                If maxSqDist > sqTolerance Then
                    markers(index.Value) = 1
                    stack.AddRange({first, index, index, last})
                End If


                If stack.Count > 0 Then
                    last = stack(stack.Count - 1)
                    stack.RemoveAt(stack.Count - 1)
                Else
                    last = Nothing
                End If

                If stack.Count > 0 Then
                    first = stack(stack.Count - 1)
                    stack.RemoveAt(stack.Count - 1)
                Else
                    first = Nothing
                End If
            End While

            For i = 0 To len - 1
                If markers(i) IsNot Nothing Then newPoints.Add(points(i))
            Next

            Return newPoints
        End Function

        ''' <summary>
        ''' Simplifies a list of points to a shorter list of points.
        ''' </summary>
        ''' <param name="points">Points original list of points</param>
        ''' <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        ''' <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        ''' <returns>Simplified list of points</returns>
        Public Function Simplify(points As Point(),
                                 Optional tolerance As Double = 0.3,
                                 Optional highestQuality As Boolean = False) As List(Of Point) Implements ISimplifyUtility.Simplify
            If points Is Nothing OrElse points.Length = 0 Then Return New List(Of Point)()

            Dim sqTolerance = tolerance * tolerance

            If highestQuality Then Return SimplifyDouglasPeucker(points, sqTolerance)

            Dim points2 = SimplifyRadialDistance(points, sqTolerance)
            Return SimplifyDouglasPeucker(points2.ToArray(), sqTolerance)
        End Function

        ''' <summary>
        ''' Simplifies a list of points to a shorter list of points.
        ''' </summary>
        ''' <param name="points">Points original list of points</param>
        ''' <param name="tolerance">Tolerance tolerance in the same measurement as the point coordinates</param>
        ''' <param name="highestQuality">Enable highest quality for using Douglas-Peucker, set false for Radial-Distance algorithm</param>
        ''' <returns>Simplified list of points</returns>
        Public Shared Function SimplifyArray(points As Point(),
                                             Optional tolerance As Double = 0.3,
                                             Optional highestQuality As Boolean = False) As List(Of Point)
            Return New SimplifyUtility().Simplify(points, tolerance, highestQuality)
        End Function
    End Class
End Namespace

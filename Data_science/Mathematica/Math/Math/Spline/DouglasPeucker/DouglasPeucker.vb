#Region "Microsoft.VisualBasic::63dd99e780c4aaabda70492e8f2f9170, Data_science\Mathematica\Math\Math\Spline\DouglasPeucker\DouglasPeucker.vb"

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

    '   Total Lines: 222
    '    Code Lines: 103 (46.40%)
    ' Comment Lines: 89 (40.09%)
    '    - Xml Docs: 50.56%
    ' 
    '   Blank Lines: 30 (13.51%)
    '     File Size: 8.75 KB


    '     Module DouglasPeucker
    ' 
    '         Function: distanceFromPointToLine, distanceFromPointToLineSquared, findPerpendicularDistance, pointDistance, RDPppd
    '                   RDPsd
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Python
Imports std = System.Math

Namespace Interpolation

    ''' <summary>
    ''' Ramer–Douglas–Peucker algorithm
    ''' 
    ''' The Ramer–Douglas–Peucker algorithm, also known as the Douglas–Peucker 
    ''' algorithm and iterative end-point fit algorithm, is an algorithm that 
    ''' decimates a curve composed of line segments to a similar curve with fewer
    ''' points. It was one of the earliest successful algorithms developed for 
    ''' cartographic generalization.
    ''' 
    ''' > https://en.wikipedia.org/wiki/Ramer%E2%80%93Douglas%E2%80%93Peucker_algorithm
    ''' </summary>
    Public Module DouglasPeucker

        ' /**
        '  * Ramer Douglas Peucker
        '  *
        '  * The Ramer-Douglas–Peucker algorithm Is an algorithm For reducing the number Of points In a curve that Is approximated by a series Of points. 
        '  * It does so by "thinking" Of a line between the first And last point In a Set Of points that form the curve. 
        '  * It checks which point In between Is farthest away from this line. 
        '  * If the point (And As follows, all other In-between points) Is closer than a given distance 'epsilon', it removes all these in-between points. 
        '  * If on the other hand this 'outlier point' is farther away from our imaginary line than epsilon, the curve is split in two parts. 
        '  * The Function is recursively called On both resulting curves, And the two reduced forms Of the curve are put back together.
        '  *
        '  *   1) From the first point up to And including the outlier
        '  *   2) The outlier And the remaining points.
        '  *
        '  * I hope that by looking at this source code for my Ramer Douglas Peucker
        '  * implementation you will be able to get a correct reduction of your
        '  * dataset.
        '  *
        '  * @licence Feel free to use it as you please, a mention of my name Is always nice.
        '  *
        '  * Marius Karthaus
        '  * http://www.LowVoice.nl
        '  * https://karthaus.nl/rdp/
        '  *
        ' */


        ''' <summary>
        ''' ## RDP with ShortestDistance
        '''         
        ''' this Is the implementation with shortest Distance 
        ''' (as of 2013-09 suggested by the wikipedia page. Thanks 
        ''' Edward Lee for pointing this out)
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="epsilon"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function RDPsd(points As PointF(), Optional epsilon As Double = 0.1) As PointF()
            Dim firstPoint = points(0)
            Dim lastPoint = points(points.Length - 1)

            If points.Length < 3 Then
                Return points
            End If

            Dim index = -1
            Dim dist As Double = 0

            For i As Integer = 1 To points.Length - 2
                Dim cDist = distanceFromPointToLine(points(i), firstPoint, lastPoint)

                If cDist > dist Then
                    dist = cDist
                    index = i
                End If
            Next

            If dist > epsilon Then
                ' iterate
                Dim l1 = points.slice(0, index + 1).ToArray
                Dim l2 = points.slice(index).ToArray
                Dim r1 = RDPsd(l1, epsilon)
                Dim r2 = RDPsd(l2, epsilon)
                ' concat r2 to r1 minus the end/startpoint that will be the same
                Dim rs = r1 _
                    .slice(0, r1.Length - 1) _
                    .Concat(r2) _
                    .ToArray

                Return rs
            Else
                Return New PointF() {firstPoint, lastPoint}
            End If
        End Function

        ''' <summary>
        ''' ## RDP with PerpendicularDistance
        ''' 
        ''' this Is the implementation with perpendicular Distance
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="epsilon"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function RDPppd(points As PointF(), Optional epsilon As Double = 0.1) As PointF()
            Dim firstPoint = points(0)
            Dim lastPoint = points(points.Length - 1)

            If points.Length < 3 Then
                Return points
            End If

            Dim index = -1
            Dim dist As Double = 0

            For i As Integer = 1 To points.Length - 2
                Dim cDist = findPerpendicularDistance(points(i), firstPoint, lastPoint)

                If cDist > dist Then
                    dist = cDist
                    index = i
                End If
            Next

            If dist > epsilon Then
                ' iterate
                Dim l1 = points.slice(0, index + 1).ToArray
                Dim l2 = points.slice(index).ToArray
                Dim r1 = RDPppd(l1, epsilon)
                Dim r2 = RDPppd(l2, epsilon)
                ' concat r2 to r1 minus the end/startpoint that will be the same
                Dim rs = r1 _
                    .slice(0, r1.Length - 1) _
                    .Concat(r2) _
                    .ToArray

                Return rs
            Else
                Return New PointF() {firstPoint, lastPoint}
            End If
        End Function

        Private Function findPerpendicularDistance(p As PointF, p1 As PointF, p2 As PointF) As Double
            ' if start And end point are on the same x the distance Is the difference in X.
            Dim result As Double
            Dim slope As Double
            Dim intercept As Double

            If p1.X = p2.X Then
                result = std.Abs(p.X - p1.X)
            Else
                slope = (p2.Y - p1.Y) / (p2.X - p1.X)
                intercept = p1.Y - (slope * p1.X)
                result = std.Abs(slope * p.X - p.Y + intercept) / std.Sqrt(std.Pow(slope, 2) + 1)
            End If

            Return result
        End Function

        ''' <summary>
        ''' code as suggested by Edward Lee
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function distanceFromPointToLine(p As PointF, a As PointF, b As PointF) As Double
            Return std.Sqrt(distanceFromPointToLineSquared(p, a, b))
        End Function

        ''' <summary>
        ''' This Is the difficult part. Commenting as we go.
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <returns></returns>
        Private Function distanceFromPointToLineSquared(p As PointF, i As PointF, j As PointF) As Double
            ' First, we need the length of the line segment.
            Dim lineLength As Double = pointDistance(i, j)

            If lineLength = 0 Then
                ' If it's 0, the line is actually just a point.
                Return pointDistance(p, Nothing)
            End If

            Dim t As Double = ((p.X - i.X) * (j.X - i.X) + (p.Y - i.Y) * (j.Y - i.Y)) / lineLength

            ' t Is very important. t Is a number that essentially compares the individual coordinates
            ' distances between the point And each point on the line.

            ' If t Is less than 0, the point Is behind i, And closest To i.
            If t < 0 Then
                Return pointDistance(p, i)
            End If

            ' if greater than 1, it's closest to j.
            If t > 1 Then
                Return pointDistance(p, j)
            End If

            ' this figure represents the point on the line that p Is closest to.
            Return pointDistance(p, New PointF With {.X = i.X + t * (j.X - i.X), .Y = i.Y + t * (j.Y - i.Y)})
        End Function

        ''' <summary>
        ''' returns distance between two points. Easy geometry.
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function pointDistance(i As PointF, j As PointF) As Double
            Return (i.X - j.X) ^ 2 + (i.Y - j.Y) ^ 2
        End Function
    End Module
End Namespace

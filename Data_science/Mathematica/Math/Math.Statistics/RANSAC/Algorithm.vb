#Region "Microsoft.VisualBasic::85f4b640b545c8825d0d5ce4d1c50ef6, Data_science\Mathematica\Math\Math.Statistics\RANSAC\Algorithm.vb"

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

    '   Total Lines: 231
    '    Code Lines: 159 (68.83%)
    ' Comment Lines: 31 (13.42%)
    '    - Xml Docs: 70.97%
    ' 
    '   Blank Lines: 41 (17.75%)
    '     File Size: 9.74 KB


    '     Class Algorithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetBounds, GetPoints, Points2Plane, Qualify
    '                   StandardDeviation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace RANSAC

    ''' <summary>
    ''' RANSAC (RANdom SAmple Consensus) is an algorithm for measuring 
    ''' system parameters for some input data.
    ''' A common task For this algorithm Is To find the median subset 
    ''' Of a specified Set. For the regular spaces, the subset Is:
    '''
    ''' + 2D space - a line
    ''' + 3D space - a plane
    ''' 
    ''' This implementation presents a search Of a median plane For 
    ''' a random (Or loaded) Set Of points In 3D space.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/scikodot/ransac
    ''' </remarks>
    Friend Class Algorithm

        Private p As Double
        Private N As Integer
        Private points As Point()

        Private genPlane As Double()
        Private inliersPercentage As Double
        Private lowLine As Double() = New Double(1) {}, highLine As Double() = New Double(1) {}

        Sub New(p As Double, points As Point())
            Me.p = p
            Me.N = points.Length
            Me.points = points

            Console.WriteLine("Threshold (p): {0:F6}" & vbLf & "Points' amount (N): {1}", p, N)
            Console.WriteLine("Points (first 10):")
            For i = 0 To If(points.Length < 10, points.Length, 10) - 1
                Console.WriteLine("{0:F6}" & vbTab & "{1:F6}" & vbTab & "{2:F6}", points(CInt(i)).x, points(CInt(i)).y, points(CInt(i)).z)
            Next
            Console.WriteLine()
        End Sub

        ''' <summary>
        ''' RANSAC algorithm implementation
        ''' </summary>
        ''' <param name="alpha">
        ''' probability of finding best-fitting plane
        ''' </param>
        ''' <param name="w">
        ''' probability of choosing an inlier from the set of data
        ''' </param>
        Public Function Evaluate(Optional w As Double = 0.5,
                                 Optional alpha As Double = 0.999,
                                 Optional seed As Integer = 9956) As SampleOutput

            Dim bestSupport As Integer = 0
            Dim bestPlane = New Double(3) {}
            Dim bestStd = Double.PositiveInfinity
            Dim trials As Integer = stdNum.Round(stdNum.Log(1 - alpha) / stdNum.Log(1 - stdNum.Pow(w, 3)), MidpointRounding.AwayFromZero)
            Dim rng As New Random(seed)
            Dim j As i32 = 0
            'getting all valid points, as well as their distances to the plane
            Dim qualifiedDistances As Double() = Nothing

            While ++j <= trials
                'retrieving 3 random points from the list
                Dim planePoints = GetPoints(rng)

                'constructing a new plane upon retrieved points
                Dim plane = Points2Plane(planePoints)
                Dim qualifiedPoints = Qualify(plane, points, p, qualifiedDistances)

                'calculating standard deviation of qualified points
                Dim std = StandardDeviation(qualifiedDistances)

                'checking whether the current plane fits data better or not
                If qualifiedPoints.Length > bestSupport OrElse qualifiedPoints.Length = bestSupport AndAlso std < bestStd Then
                    'if yes, consider current plane as best-fitting
                    bestSupport = qualifiedPoints.Length
                    bestPlane = plane
                    bestStd = std
                End If
            End While

            'outputting results
            Return New SampleOutput With {
                .bestPlane = bestPlane,
                .bestStd = bestStd,
                .bestSupport = bestSupport,
                .inliersPercentage = inliersPercentage,
                .N = N
            }
        End Function

        Private Function GetPoints(rng As Random) As Tuple(Of Point, Point, Point)
            Dim indices As List(Of Integer) = New List(Of Integer)()

            While indices.Count < 3
                Dim index = rng.Next(0, points.Length)
                If Not indices.Contains(index) Then indices.Add(index)
            End While

            Dim point1 = points(indices(0)), point2 = points(indices(1)), point3 = points(indices(2))

            Return Tuple.Create(point1, point2, point3)
        End Function

        Private Function Points2Plane(points As Tuple(Of Point, Point, Point)) As Double()
            Dim point1 = points.Item1
            Dim point2 = points.Item2
            Dim point3 = points.Item3

            Dim v1 As Vector = New Vector(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z)
            Dim v2 As Vector = New Vector(point3.x - point1.x, point3.y - point1.y, point3.z - point1.z)
            Dim n As Vector = New Vector(v1.y * v2.z - v2.y * v1.z, -v1.x * v2.z + v2.x * v1.z, v1.x * v2.y - v2.x * v1.y)

            Dim plane = New Double(3) {}
            plane(0) = n.x
            plane(1) = n.y
            plane(2) = n.z
            plane(3) = -n.x * point1.x - n.y * point1.y - n.z * point1.z

            For i = 0 To 3
                plane(i) *= -If(plane(3) <> 0, stdNum.Sign(plane(3)), 1) / n.magnitude
            Next

            Return plane
        End Function

        Private Shared Function Qualify(plane As Double(), points As Point(), threshold As Double, <Out> ByRef distances As Double()) As Point()
            Dim validPoints As List(Of Point) = New List(Of Point)()
            Dim validDistances As List(Of Double) = New List(Of Double)()

            For i = 0 To points.Length - 1
                Dim distance = stdNum.Abs(plane(0) * points(i).x + plane(1) * points(i).y + plane(2) * points(i).z + plane(3)) / stdNum.Sqrt(stdNum.Pow(plane(0), 2) + stdNum.Pow(plane(1), 2) + stdNum.Pow(plane(2), 2))

                If distance <= threshold Then
                    validPoints.Add(points(i))
                    validDistances.Add(distance)
                End If
            Next

            distances = validDistances.ToArray()
            Return validPoints.ToArray()
        End Function

        Private Shared Function StandardDeviation([set] As Double()) As Double
            Dim mean As Double = 0, variance As Double = 0

            For i = 0 To [set].Length - 1
                mean += [set](i)
            Next
            mean /= [set].Length

            For i = 0 To [set].Length - 1
                variance += stdNum.Pow([set](i) - mean, 2)
            Next
            variance /= [set].Length - 1

            Return stdNum.Sqrt(variance)
        End Function

        Public Function GetBounds(plane As Double()) As Tuple(Of Double, Double, Double, Double, Double, Double)
            Dim p = New Double(2) {}
            Dim bounds As List(Of Point) = New List(Of Point)()
            Dim line1 = New Double(1) {}, line2 = New Double(1) {}
            If plane(0) <> 0 AndAlso plane(1) <> 0 AndAlso plane(2) <> 0 Then
                Dim x As List(Of Double) = New List(Of Double)()
                For i = 0 To 1
                    p(2) = If((i And 1) = 1, -100, 100)

                    For j = 0 To 3
                        Dim k = If((j And 2) = 2, 1, 0)
                        p(k) = If((j And 1) = 1, -100, 100)
                        p(k Xor 1) = -(plane(k) * p(k) + plane(2) * p(2) + plane(3)) / plane(k Xor 1)

                        If k = 1 Then x.Add(p(k Xor 1))
                    Next

                    If i = 0 Then
                        line1(0) = -plane(0) / plane(1)
                        line1(1) = -(plane(2) * p(2) + plane(3)) / plane(1)
                    Else
                        line2(0) = -plane(0) / plane(1)
                        line2(1) = -(plane(2) * p(2) + plane(3)) / plane(1)
                    End If
                Next

                If line1(1) < line2(1) Then
                    lowLine = line1
                    highLine = line2
                Else
                    lowLine = line2
                    highLine = line1
                End If

                Dim x_min As Double = x.Min(), x_max As Double = x.Max()
                If x_min < -100 Then x_min = -100
                If x_max > 100 Then x_max = 100

                Return Tuple.Create(x_min, x_max, 0.0, 0.0, 0.0, 0.0)
            ElseIf plane(0) = 0 OrElse plane(1) = 0 OrElse plane(2) = 0 Then
                Dim index = 0
                If plane(0) = 0 Then
                    index = 0
                ElseIf plane(1) = 0 Then
                    index = 1
                ElseIf plane(2) = 0 Then
                    index = 2
                End If

                Dim sum = (index + 1) Mod 3 + (index + 2) Mod 3
                For j = 0 To 3
                    Dim k = If((j And 2) = 2, (index + 1) Mod 3, (index + 2) Mod 3)
                    p(k) = If((j And 1) = 1, -100, 100)
                    p(k Xor sum) = -(plane(k) * p(k) + plane(3)) / plane(k Xor sum)

                    If stdNum.Abs(p(k Xor sum)) <= 100 Then bounds.Add(New Point(p))
                Next
            End If

            Dim x_low_bound = bounds.Min(Function(t) t.x), x_high_bound = bounds.Max(Function(t) t.x)
            Dim y_low_bound = bounds.Min(Function(t) t.y), y_high_bound = bounds.Max(Function(t) t.y)
            Dim z_low_bound = bounds.Min(Function(t) t.z), z_high_bound = bounds.Max(Function(t) t.z)

            Return Tuple.Create(x_low_bound, x_high_bound, y_low_bound, y_high_bound, z_low_bound, z_high_bound)
        End Function
    End Class
End Namespace

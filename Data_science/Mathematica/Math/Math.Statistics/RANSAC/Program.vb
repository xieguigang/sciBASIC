Imports System.Runtime.InteropServices
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
    Friend Class Program

        Private p As Double
        Private N As Integer
        Private points As Point()

        Private genPlane As Double()
        Private inliersPercentage As Double
        Private lowLine As Double() = New Double(1) {}, highLine As Double() = New Double(1) {}

        Private Sub Main(p As Double, points As Point())
            Me.p = p
            Me.N = points.Length
            Me.points = points

            Console.WriteLine("Threshold (p): {0:F6}" & Microsoft.VisualBasic.Constants.vbLf & "Points' amount (N): {1}", p, N)
            Console.WriteLine("Points (first 10):")
            For i = 0 To If(points.Length < 10, points.Length, 10) - 1
                Console.WriteLine("{0:F6}" & Microsoft.VisualBasic.Constants.vbTab & "{1:F6}" & Microsoft.VisualBasic.Constants.vbTab & "{2:F6}", points(CInt(i)).x, points(CInt(i)).y, points(CInt(i)).z)
            Next
            Console.WriteLine()

            'RANSAC algorithm implementation
            Dim w As Double = 0.5F  'probability of choosing an inlier from the set of data
            Dim alpha As Double = 0.999F  'probability of finding best-fitting plane

            Dim bestSupport = 0
            Dim bestPlane = New Double(3) {}
            Dim bestStd = Double.PositiveInfinity
            Dim trials As Integer = stdNum.Round(stdNum.Log(1 - alpha) / stdNum.Log(1 - stdNum.Pow(w, 3)), MidpointRounding.AwayFromZero)

            Dim rng As Random = New Random()
            Dim j = 0

            'getting all valid points, as well as their distances to the plane
            Dim qualifiedDistances As Double() = Nothing
            While stdNum.Min(Threading.Interlocked.Increment(j), j - 1) <= trials
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
            Console.WriteLine("###OUTPUT###")
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Best plane: {0:F6} {1:F6} {2:F6} {3:F6}", bestPlane(0), bestPlane(1), bestPlane(2), bestPlane(3))
            Console.ResetColor()
            Console.WriteLine("Best support (i.e. matched points): {0}", bestSupport)
            Console.WriteLine("Best standard deviation: {0}" & Microsoft.VisualBasic.Constants.vbLf, bestStd)

            Console.WriteLine("Lost points: {0}" & Microsoft.VisualBasic.Constants.vbLf & "Accuracy: {1:F6}" & Microsoft.VisualBasic.Constants.vbLf, CInt(N * inliersPercentage) - bestSupport, bestSupport / (N * inliersPercentage))
        End Sub

        Private Function GetPoints(ByVal rng As Random) As Tuple(Of Point, Point, Point)
            Dim indices As List(Of Integer) = New List(Of Integer)()

            While indices.Count < 3
                Dim index = rng.Next(0, points.Length)
                If Not indices.Contains(index) Then indices.Add(index)
            End While

            Dim point1 = points(indices(0)), point2 = points(indices(1)), point3 = points(indices(2))

            Return Tuple.Create(point1, point2, point3)
        End Function

        Private Function Points2Plane(ByVal points As Tuple(Of Point, Point, Point)) As Double()
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

        Private Shared Function Qualify(ByVal plane As Double(), ByVal points As Point(), ByVal threshold As Double, <Out> ByRef distances As Double()) As Point()
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

        Private Shared Function StandardDeviation(ByVal [set] As Double()) As Double
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

        '        Private Shared Sub GenerateData()
        '            Dim rng As Random = New Random()

        '            p = rng.NextDouble()
        '            If p > 0.5 Then p -= 0.5
        '            If p < 0.01 Then p += 0.01

        '            Program.N = rng.Next(25000, 25001)

        '            Dim p1 As Point = New Point With {
        '    .x = rng.NextDouble() * 200 - 100,
        '    .y = rng.NextDouble() * 200 - 100,
        '    .z = rng.NextDouble() * 200 - 100
        '}
        '            Dim p2 As Point = New Point With {
        '    .x = rng.NextDouble() * 200 - 100,
        '    .y = rng.NextDouble() * 200 - 100,
        '    .z = rng.NextDouble() * 200 - 100
        '}
        '            Dim p3 As Point = New Point With {
        '    .x = rng.NextDouble() * 200 - 100,
        '    .y = rng.NextDouble() * 200 - 100,
        '    .z = rng.NextDouble() * 200 - 100
        '}

        '            genPlane = Points2Plane(Tuple.Create(p1, p2, p3))

        '            Dim n As Vector = New Vector(genPlane(0), genPlane(1), genPlane(2))

        '            Dim bounds = GetBounds(genPlane)
        '            Dim x_l = bounds.Item1, x_h = bounds.Item2
        '            Dim y_l = bounds.Item3, y_h = bounds.Item4
        '            Dim z_l = bounds.Item5, z_h = bounds.Item6

        '            inliersPercentage = 1 - rng.NextDouble()
        '            If inliersPercentage < 0.5 Then inliersPercentage += 0.5

        '            Dim genPoints As List(Of Point) = New List(Of Point)()
        '            For i = 0 To Program.N - 1
        '                Dim distance As Double
        '                If i <= inliersPercentage * Program.N Then
        '                    distance = rng.NextDouble() * p
        '                Else
        '                    distance = (1 - rng.NextDouble()) * 2 * p + p
        '                End If

        '                Dim x As Double = 0, y As Double = 0, z As Double = 0
        '                If genPlane(0) <> 0 AndAlso genPlane(1) <> 0 AndAlso genPlane(2) <> 0 Then
        '                    x = rng.NextDouble() * (x_h - x_l) + x_l

        '                    y_l = lowLine(0) * x + lowLine(1)
        '                    If y_l < -100 Then y_l = -100

        '                    y_h = highLine(0) * x + highLine(1)
        '                    If y_h > 100 Then y_h = 100

        '                    y = rng.NextDouble() * (y_h - y_l) + y_l

        '                    z = -(genPlane(0) * x + genPlane(1) * y + genPlane(3)) / genPlane(2)
        '                ElseIf genPlane(0) = 0 AndAlso genPlane(1) = 0 Then
        '                    x = rng.NextDouble() * 200 - 100
        '                    y = rng.NextDouble() * 200 - 100
        '                    z = -genPlane(3) / genPlane(2)
        '                ElseIf genPlane(1) = 0 AndAlso genPlane(2) = 0 Then
        '                    y = rng.NextDouble() * 200 - 100
        '                    z = rng.NextDouble() * 200 - 100
        '                    x = -genPlane(3) / genPlane(0)
        '                ElseIf genPlane(2) = 0 AndAlso genPlane(0) = 0 Then
        '                    x = rng.NextDouble() * 200 - 100
        '                    z = rng.NextDouble() * 200 - 100
        '                    y = -genPlane(3) / genPlane(1)
        '                ElseIf genPlane(0) = 0 Then
        '                    x = rng.NextDouble() * 200 - 100
        '                    y = rng.NextDouble() * (y_h - y_l) + y_l
        '                    z = -(genPlane(1) * y + genPlane(3)) / genPlane(2)
        '                ElseIf genPlane(1) = 0 Then
        '                    y = rng.NextDouble() * 200 - 100
        '                    x = rng.NextDouble() * (x_h - x_l) + x_l
        '                    z = -(genPlane(0) * x + genPlane(3)) / genPlane(2)
        '                ElseIf genPlane(2) = 0 Then
        '                    z = rng.NextDouble() * 200 - 100
        '                    x = rng.NextDouble() * (x_h - x_l) + x_l
        '                    y = -(genPlane(0) * x + genPlane(3)) / genPlane(1)
        '                End If

        '                Dim point As Vector = New Vector(x, y, z)
        '                point += n.normalized * distance * If(rng.Next(0, 2) = 0, 1, -1)

        '                genPoints.Add(point)
        '            Next

        '            points = genPoints.ToArray()
        '        End Sub

        Public Function GetBounds(ByVal plane As Double()) As Tuple(Of Double, Double, Double, Double, Double, Double)
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

Imports std = System.Math
Imports System.Drawing

''' <summary>
''' A collection of static methods for analyzing and comparing 2D curves.
''' </summary>
Public Module CurveAnalysis
#Region "Data Structures"

    ''' <summary>
    ''' Represents a curve as a list of points.
    ''' </summary>
    Public Class Curve
        Inherits List(Of Point)

        Public Sub New()
        End Sub
        Public Sub New(ByVal collection As IEnumerable(Of Point))
            MyBase.New(collection)
        End Sub
    End Class

#End Region

#Region "Options Classes"

    ''' <summary>
    ''' Options for shape similarity calculation.
    ''' </summary>
    Public Class ShapeSimilarityOpts
        Public Property EstimationPoints As Integer = 50
        Public Property CheckRotations As Boolean = True
        Public Property Rotations As Integer = 10
        Public Property RestrictRotationAngle As Double = std.PI
    End Class

    ''' <summary>
    ''' Options for Procrustes curve normalization.
    ''' </summary>
    Public Class ProcrustesNormalizeCurveOpts
        Public Property Rebalance As Boolean = True
        Public Property EstimationPoints As Integer = 50
    End Class

    ''' <summary>
    ''' Options for curve subdivision.
    ''' </summary>
    Public Class SubdivideCurveOpts
        Public Property MaxLen As Double = 0.05
    End Class

    ''' <summary>
    ''' Options for curve rebalancing.
    ''' </summary>
    Public Class RebalanceCurveOpts
        Public Property NumPoints As Integer = 50
    End Class

#End Region

#Region "Private Helper Methods"

    ''' <summary>
    ''' Sums all elements in a list of numbers.
    ''' </summary>
    Private Function ArrSum(ByVal list As IList(Of Double)) As Double
        Return list.Sum()
    End Function

    ''' <summary>
    ''' Calculates the average of a list of numbers.
    ''' </summary>
    Private Function ArrAverage(ByVal list As IList(Of Double)) As Double
        Return list.Average()
    End Function

    ''' <summary>
    ''' Subtracts point v2 from v1.
    ''' </summary>
    Private Function Subtract(ByVal v1 As Point, ByVal v2 As Point) As Point
        Return New Point(v1.X - v2.X, v1.Y - v2.Y)
    End Function

    ''' <summary>
    ''' Calculates the magnitude (length) of a vector from the origin.
    ''' </summary>
    Private Function Magnitude(ByVal point As Point) As Double
        Return std.Sqrt(point.X * point.X + point.Y * point.Y)
    End Function

    ''' <summary>
    ''' Discrete Frechet distance between 2 curves.
    ''' Based on http://www.kr.tuwien.ac.at/staff/eiter/et-archive/cdtr9464.pdf
    ''' Modified to be iterative and have better memory usage.
    ''' </summary>
    Private Function FrechetDist(ByVal curve1 As Curve, ByVal curve2 As Curve) As Double
        Dim longCurve As Curve = If(curve1.Count >= curve2.Count, curve1, curve2)
        Dim shortCurve As Curve = If(curve1.Count >= curve2.Count, curve2, curve1)
        Dim prevResultsCol = New List(Of Double)()
        Dim i As Integer = 0
        Do While i < longCurve.Count
            Dim curResultsCol = New List(Of Double)()
            Dim j As Integer = 0
            Do While j < shortCurve.Count
                curResultsCol.Add(CalcVal(shortCurve, longCurve, i, j, prevResultsCol, curResultsCol))
                j += 1
            Loop
            prevResultsCol = curResultsCol
            i += 1
        Loop

        Return prevResultsCol.Last()
    End Function

    Private Function CalcVal(ByVal shortCurve As Curve, ByVal longCurve As Curve, ByVal i As Integer, ByVal j As Integer, ByVal prevResultsCol As IList(Of Double), ByVal curResultsCol As IList(Of Double)) As Double
        If i = 0 AndAlso j = 0 Then
            Return PointDistance(longCurve(0), shortCurve(0))
        End If
        If i > 0 AndAlso j = 0 Then
            Return std.Max(prevResultsCol(0), PointDistance(longCurve(i), shortCurve(0)))
        End If
        Dim lastResult = curResultsCol.Last()
        If i = 0 AndAlso j > 0 Then
            Return std.Max(lastResult, PointDistance(longCurve(0), shortCurve(j)))
        End If

        Return std.Max(std.Min(prevResultsCol(j), std.Min(prevResultsCol(j - 1), lastResult)), PointDistance(longCurve(i), shortCurve(j)))
    End Function

#End Region

#Region "Public API"

    ''' <summary>
    ''' Estimate how similar the shapes of 2 curves are to each other,
    ''' accounting for translation, scale, and rotation.
    ''' </summary>
    ''' <param name="curve1">The first curve.</param>
    ''' <param name="curve2">The second curve.</param>
    ''' <param name="options">Options for the similarity calculation.</param>
    ''' <returns>A value between 1 and 0 depending on how similar the shapes are, where 1 means identical.</returns>
    Public Function ShapeSimilarity(ByVal curve1 As Curve, ByVal curve2 As Curve, Optional ByVal options As ShapeSimilarityOpts = Nothing) As Double
        options = If(options, New ShapeSimilarityOpts())

        If std.Abs(options.RestrictRotationAngle) > std.PI Then
            Throw New ArgumentException("restrictRotationAngle cannot be larger than PI")
        End If

        Dim normalizedCurve1 = ProcrustesNormalizeCurve(curve1, New ProcrustesNormalizeCurveOpts With {.EstimationPoints = options.EstimationPoints})
        Dim normalizedCurve2 = ProcrustesNormalizeCurve(curve2, New ProcrustesNormalizeCurveOpts With {.EstimationPoints = options.EstimationPoints})

        Dim geoAvgCurveLen = std.Sqrt(CurveLength(normalizedCurve1) * CurveLength(normalizedCurve2))

        Dim thetasToCheck = New List(Of Double) From {0}

        If options.CheckRotations Then
            Dim procrustesTheta = FindProcrustesRotationAngle(normalizedCurve1, normalizedCurve2)
            ' use a negative rotation rather than a large positive rotation
            If procrustesTheta > std.PI Then
                procrustesTheta -= 2 * std.PI
            End If
            If procrustesTheta <> 0 AndAlso std.Abs(procrustesTheta) < options.RestrictRotationAngle Then
                thetasToCheck.Add(procrustesTheta)
            End If
            For i As Integer = 0 To options.Rotations - 1
                Dim theta = -1 * options.RestrictRotationAngle + (2.0 * i * options.RestrictRotationAngle) / (options.Rotations - 1)
                ' 0 and Math.PI are already being checked, no need to check twice
                If theta <> 0 AndAlso theta <> std.PI Then
                    thetasToCheck.Add(theta)
                End If
            Next i
        End If

        Dim minFrechetDist = Double.PositiveInfinity
        ' check some other thetas here just in case the procrustes theta isn't the best rotation
        For Each theta In thetasToCheck
            Dim rotatedCurve1 = RotateCurve(normalizedCurve1, theta)
            Dim dist = FrechetDist(rotatedCurve1, normalizedCurve2)
            If dist < minFrechetDist Then
                minFrechetDist = dist
            End If
        Next theta

        ' divide by Math.Sqrt(2) to try to get the low results closer to 0
        Return std.Max(1 - minFrechetDist / (geoAvgCurveLen / std.Sqrt(2)), 0)
    End Function

    ''' <summary>
    ''' Translate and scale curve by Procrustes Analysis.
    ''' Optionally runs RebalanceCurve first (default true).
    ''' From https://en.wikipedia.org/wiki/Procrustes_analysis
    ''' </summary>
    ''' <param name="curve">The curve to normalize.</param>
    ''' <param name="options">Options for normalization.</param>
    ''' <returns>The normalized curve.</returns>
    Public Function ProcrustesNormalizeCurve(ByVal curve As Curve, Optional ByVal options As ProcrustesNormalizeCurveOpts = Nothing) As Curve
        options = If(options, New ProcrustesNormalizeCurveOpts())

        Dim balancedCurve = If(options.Rebalance, RebalanceCurve(curve, New RebalanceCurveOpts With {.NumPoints = options.EstimationPoints}), curve)

        Dim meanX = ArrAverage(balancedCurve.Select(Function(p) p.X).ToList())
        Dim meanY = ArrAverage(balancedCurve.Select(Function(p) p.Y).ToList())
        Dim mean = New Point(meanX, meanY)
        Dim translatedCurve = balancedCurve.Select(Function(p) Subtract(p, mean)).ToList()
        Dim scale = std.Sqrt(ArrAverage(translatedCurve.Select(Function(p) p.X * p.X + p.Y * p.Y).ToList()))

        Return New Curve(translatedCurve.Select(Function(p) New Point(p.X / scale, p.Y / scale)))
    End Function

    ''' <summary>
    ''' Find the angle to rotate `curve` to match the rotation of `relativeCurve` using procrustes analysis.
    ''' From https://en.wikipedia.org/wiki/Procrustes_analysis
    ''' `curve` and `relativeCurve` must have the same number of points.
    ''' `curve` and `relativeCurve` should both be run through ProcrustesNormalizeCurve first.
    ''' </summary>
    ''' <param name="curve">The curve to rotate.</param>
    ''' <param name="relativeCurve">The curve to match rotation with.</param>
    ''' <returns>The rotation angle in radians.</returns>
    Public Function FindProcrustesRotationAngle(ByVal curve As Curve, ByVal relativeCurve As Curve) As Double
        If curve.Count <> relativeCurve.Count Then
            Throw New ArgumentException("curve and relativeCurve must have the same length")
        End If

        Dim numerator = ArrSum(curve.Select(Function(p, i) p.Y * relativeCurve(i).X - p.X * relativeCurve(i).Y).ToList())
        Dim denominator = ArrSum(curve.Select(Function(p, i) p.X * relativeCurve(i).X + p.Y * relativeCurve(i).Y).ToList())

        Return std.Atan2(numerator, denominator)
    End Function

    ''' <summary>
    ''' Rotate `curve` to match the rotation of `relativeCurve` using procrustes analysis.
    ''' From https://en.wikipedia.org/wiki/Procrustes_analysis
    ''' `curve` and `relativeCurve` must have the same number of points.
    ''' `curve` and `relativeCurve` should both be run through ProcrustesNormalizeCurve first.
    ''' </summary>
    ''' <param name="curve">The curve to rotate.</param>
    ''' <param name="relativeCurve">The curve to match rotation with.</param>
    ''' <returns>The rotated curve.</returns>
    Public Function ProcrustesNormalizeRotation(ByVal curve As Curve, ByVal relativeCurve As Curve) As Curve
        Dim angle = FindProcrustesRotationAngle(curve, relativeCurve)
        Return RotateCurve(curve, angle)
    End Function

    ''' <summary>
    ''' Calculate the distance between 2 points.
    ''' </summary>
    ''' <param name="point1">The first point.</param>
    ''' <param name="point2">The second point.</param>
    ''' <returns>The Euclidean distance.</returns>
    Public Function PointDistance(ByVal point1 As Point, ByVal point2 As Point) As Double
        Return Magnitude(Subtract(point1, point2))
    End Function

    ''' <summary>
    ''' Calculate the length of the curve.
    ''' </summary>
    ''' <param name="points">The curve as a list of points.</param>
    ''' <returns>The total length of the curve.</returns>
    Public Function CurveLength(ByVal points As Curve) As Double
        If points.Count < 2 Then
            Return 0
        End If

        Dim length As Double = 0
        Dim lastPoint = points(0)
        For Each point In points.Skip(1)
            length += PointDistance(point, lastPoint)
            lastPoint = point
        Next point
        Return length
    End Function

    ''' <summary>
    ''' Return a new point, p3, which is on the same line as p1 and p2, but <dist> away from p2.
    ''' p1, p2, p3 will always lie on the line in that order (as long as dist is positive).
    ''' </summary>
    ''' <param name="p1">The first point defining the line.</param>
    ''' <param name="p2">The second point defining the line.</param>
    ''' <param name="dist">The distance from p2 to the new point.</param>
    ''' <returns>The new point.</returns>
    Public Function ExtendPointOnLine(ByVal p1 As Point, ByVal p2 As Point, ByVal dist As Double) As Point
        Dim vect = Subtract(p2, p1)
        Dim norm = dist / Magnitude(vect)
        Return New Point(p2.X + norm * vect.X, p2.Y + norm * vect.Y)
    End Function

    ''' <summary>
    ''' Break up long segments in the curve into smaller segments of len maxLen or smaller.
    ''' </summary>
    ''' <param name="curve">The input curve.</param>
    ''' <param name="options">Options for subdivision.</param>
    ''' <returns>The subdivided curve.</returns>
    Public Function SubdivideCurve(ByVal curve As Curve, Optional ByVal options As SubdivideCurveOpts = Nothing) As Curve
        options = If(options, New SubdivideCurveOpts())

        Dim newCurve = New Curve(curve.Take(1))
        For Each point In curve.Skip(1)
            Dim prevPoint = newCurve.Last()
            Dim segLen = PointDistance(point, prevPoint)
            If segLen > options.MaxLen Then
                Dim numNewPoints = CInt(std.Truncate(std.Ceiling(segLen / options.MaxLen)))
                Dim newSegLen = segLen / numNewPoints
                For i As Integer = 0 To numNewPoints - 1
                    newCurve.Add(ExtendPointOnLine(point, prevPoint, -1 * newSegLen * (i + 1)))
                Next i
            Else
                newCurve.Add(point)
            End If
        Next point
        Return newCurve
    End Function

    ''' <summary>
    ''' Redraw the curve using `numPoints` points equally spaced along the length of the curve.
    ''' This may result in a slightly different shape than the original if `numPoints` is low.
    ''' </summary>
    ''' <param name="curve">The input curve.</param>
    ''' <param name="options">Options for rebalancing.</param>
    ''' <returns>The rebalanced curve.</returns>
    Public Function RebalanceCurve(ByVal curve As Curve, ByVal options As RebalanceCurveOpts) As Curve
        Dim curveLen = CurveLength(curve)
        Dim segmentLen = curveLen / (options.NumPoints - 1)
        Dim outlinePoints = New Curve From {curve(0)}
        Dim endPoint = curve.Last()
        Dim remainingCurvePoints = New Curve(curve.Skip(1))

        Dim i As Integer = 0
        Do While i < options.NumPoints - 2
            Dim lastPoint = outlinePoints.Last()
            Dim remainingDist = segmentLen
            Dim outlinePointFound = False
            Do While Not outlinePointFound
                Dim nextPointDist = PointDistance(lastPoint, remainingCurvePoints(0))
                If nextPointDist < remainingDist Then
                    remainingDist -= nextPointDist
                    lastPoint = remainingCurvePoints(0)
                    remainingCurvePoints.RemoveAt(0)
                Else
                    Dim nextPoint = ExtendPointOnLine(lastPoint, remainingCurvePoints(0), remainingDist)
                    outlinePoints.Add(nextPoint)
                    outlinePointFound = True
                End If
            Loop
            i += 1
        Loop
        outlinePoints.Add(endPoint)
        Return outlinePoints
    End Function

    ''' <summary>
    ''' Rotate the curve around the origin.
    ''' </summary>
    ''' <param name="curve">The curve to rotate.</param>
    ''' <param name="theta">The angle to rotate by, in radians.</param>
    ''' <returns>The rotated curve.</returns>
    Public Function RotateCurve(ByVal curve As Curve, ByVal theta As Double) As Curve
        Dim cosTheta = std.Cos(-theta)
        Dim sinTheta = std.Sin(-theta)
        Return New Curve(curve.Select(Function(p) New Point(cosTheta * p.X - sinTheta * p.Y, sinTheta * p.X + cosTheta * p.Y)))
    End Function

#End Region
End Module

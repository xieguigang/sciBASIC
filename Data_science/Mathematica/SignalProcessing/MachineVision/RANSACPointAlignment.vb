#Region "Microsoft.VisualBasic::35999816b815180c76fc6c230c289bb7, Data_science\Mathematica\SignalProcessing\MachineVision\RANSACPointAlignment.vb"

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

'   Total Lines: 299
'    Code Lines: 200 (66.89%)
' Comment Lines: 40 (13.38%)
'    - Xml Docs: 7.50%
' 
'   Blank Lines: 59 (19.73%)
'     File Size: 10.92 KB


' Class RANSACPointAlignment
' 
'     Function: AlignPolygons, CalculateCenter, ComputeSimilarityTransform, CountInliers, FindClosestPoint
'               RefineTransform
'     Structure Point
' 
'         Constructor: (+1 Overloads) Sub New
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' A static utility class for aligning two 2D polygons using the RANSAC algorithm.
''' This version is optimized for correctness and includes a proper least-squares refinement step.
''' </summary>
Public NotInheritable Class RANSACPointAlignment

    Private Sub New()
        ' Prevent instantiation of this static class
    End Sub

    ''' <summary>
    ''' Aligns a source polygon to a target polygon using RANSAC.
    ''' </summary>
    ''' <param name="sourcePoly">The polygon to be transformed.</param>
    ''' <param name="targetPoly">The polygon to align to.</param>
    ''' <param name="iterations">The number of RANSAC iterations.</param>
    ''' <param name="distanceThreshold">The distance threshold to consider a point an inlier.</param>
    ''' <returns>The best-fit Transform object.</returns>
    Public Shared Function AlignPolygons(sourcePoly As Polygon2D,
                                         targetPoly As Polygon2D,
                                         Optional iterations As Integer = 1000,
                                         Optional distanceThreshold As Double = 0.1) As AffineTransform
        ' 预检查：点数不足时返回单位变换
        If sourcePoly.length < 2 OrElse targetPoly.length < 2 Then
            Return New AffineTransform
        End If

        Dim bestTransform As New AffineTransform
        Dim maxInliers As Integer = 0
        Dim thresholdSq = distanceThreshold * distanceThreshold

        ' 确保不越界
        Dim maxIndex = std.Min(sourcePoly.length, targetPoly.length) - 1
        If maxIndex < 1 Then Return New AffineTransform ' 不足一对点

        Dim bar As Tqdm.ProgressBar = Nothing

        ' RANSAC 迭代
        For Each iter As Integer In TqdmWrapper.Range(0, iterations, bar:=bar, wrap_console:=App.EnableTqdm)
            ' 1. Randomly select two different point indices
            Dim idx1, idx2 As Integer
            Do
                idx1 = rand.Next(maxIndex + 1)
                idx2 = rand.Next(maxIndex + 1)
            Loop While idx1 = idx2

            ' 2. Get the corresponding point pairs (ASSUMING CORRESPONDENCE)
            Dim p1 = sourcePoly(idx1)
            Dim p2 = sourcePoly(idx2)
            Dim q1 = targetPoly(idx1)
            Dim q2 = targetPoly(idx2)

            ' 3. Compute an affine transform from these two pairs
            ' A 2-point pair is not enough to uniquely define a full affine transform (6 DOF).
            ' However, it's enough for a similarity transform (4 DOF). We can use it as a hypothesis.
            ' A better hypothesis is to use 3 point pairs.
            Dim idx3 As Integer
            Do
                idx3 = rand.Next(sourcePoly.length)
            Loop While idx3 = idx1 OrElse idx3 = idx2

            Dim p3 = sourcePoly(idx3)
            Dim q3 = targetPoly(idx3)

            Dim hypothesisTransform = ComputeAffineFrom3Pairs(p1, p2, p3, q1, q2, q3)

            ' 4. Count inliers for this hypothesis
            Dim currentInliers As Integer = 0
            For i As Integer = 0 To sourcePoly.length - 1
                Dim sourcePt = sourcePoly(i)
                Dim targetPt = targetPoly(i)

                Dim transformedSourcePt = hypothesisTransform.ApplyToPoint(sourcePt)
                Dim dx = transformedSourcePt.X - targetPt.X
                Dim dy = transformedSourcePt.Y - targetPt.Y

                If dx * dx + dy * dy <= thresholdSq Then
                    currentInliers += 1
                End If
            Next

            ' 5. Update best transform if this one is better
            If currentInliers > maxInliers Then
                maxInliers = currentInliers
                bestTransform = hypothesisTransform

                Call bar.SetLabel($"max-inliers: {currentInliers}; best-transform: {bestTransform}")
            End If
        Next

        ' 6. Refine the best transform using all inliers with Least Squares
        If maxInliers >= 3 Then
            Return RefineTransformWithLeastSquares(sourcePoly, targetPoly, bestTransform, distanceThreshold)
        End If

        Return bestTransform
    End Function

    ''' <summary>
    ''' Computes an affine transform from exactly three point pairs.
    ''' </summary>
    Private Shared Function ComputeAffineFrom3Pairs(p1 As PointF, p2 As PointF, p3 As PointF,
                                               q1 As PointF, q2 As PointF, q3 As PointF) As AffineTransform
        ' Solve the linear system for 6 parameters (a,b,c,d,e,f)
        ' |x1 y1 1 0  0  0|   |a|   |x'1|
        ' |x2 y2 1 0  0  0|   |b|   |x'2|
        ' |x3 y3 1 0  0  0| * |c| = |x'3|
        ' |0  0  0 x1 y1 1|   |d|   |y'1|
        ' |0  0  0 x2 y2 1|   |e|   |y'2|
        ' |0  0  0 x3 y3 1|   |f|   |y'3|
        ' We can solve this by splitting it into two 3x3 systems.
        Dim sourceX = {p1.X, p2.X, p3.X}
        Dim sourceY = {p1.Y, p2.Y, p3.Y}
        Dim targetX = {q1.X, q2.X, q3.X}
        Dim targetY = {q1.Y, q2.Y, q3.Y}

        ' Using the same least squares solver for the exact case
        Dim pairs As New List(Of (source As PointF, target As PointF)) From {
        (p1, q1), (p2, q2), (p3, q3)
    }
        Dim a, b, c, d, e, f As Double
        SolveLeastSquaresAffine(pairs, a, b, c, d, e, f)

        Return New AffineTransform With {.a = a, .b = b, .c = c, .d = d, .e = e, .f = f}
    End Function

    ''' <summary>
    ''' Refines the transformation using all inliers with a least-squares fit for an affine transform.
    ''' </summary>
    Private Shared Function RefineTransformWithLeastSquares(sourcePoly As Polygon2D, targetPoly As Polygon2D, initialTransform As AffineTransform, threshold As Double) As AffineTransform
        Dim inlierPairs As New List(Of (source As PointF, target As PointF))
        Dim thresholdSq = threshold * threshold

        ' 1. Collect all inlier point pairs based on the initial transform
        For i As Integer = 0 To sourcePoly.length - 1
            Dim sourcePt = sourcePoly(i)
            Dim targetPt = targetPoly(i)

            Dim transformedSourcePt = initialTransform.ApplyToPoint(sourcePt)
            Dim dx = transformedSourcePt.X - targetPt.X
            Dim dy = transformedSourcePt.Y - targetPt.Y

            If dx * dx + dy * dy <= thresholdSq Then
                inlierPairs.Add((sourcePt, targetPt))
            End If
        Next

        If inlierPairs.Count < 3 Then
            ' Not enough points for a stable affine fit
            Return initialTransform
        End If

        ' 2. Solve for affine parameters using Least Squares
        Dim a, b, c, d, e, f As Double
        SolveLeastSquaresAffine(inlierPairs, a, b, c, d, e, f)

        Return New AffineTransform With {.a = a, .b = b, .c = c, .d = d, .e = e, .f = f}
    End Function

    ''' <summary>
    ''' Solves the least-squares problem for affine transformation parameters.
    ''' This function is unchanged from your original, as it was mathematically correct.
    ''' </summary>
    Private Shared Sub SolveLeastSquaresAffine(points As List(Of (source As PointF, target As PointF)), ByRef a As Double, ByRef b As Double, ByRef c As Double, ByRef d As Double, ByRef e As Double, ByRef f As Double)
        Dim n = points.Count
        Dim sumX, sumY, sumX2, sumY2, sumXY, sumXp, sumYp, sumXpX, sumXpY, sumYpX, sumYpY As Double

        For Each pair In points
            Dim x = pair.source.X
            Dim y = pair.source.Y
            Dim xp = pair.target.X
            Dim yp = pair.target.Y

            sumX += x : sumY += y
            sumX2 += x * x : sumY2 += y * y : sumXY += x * y
            sumXp += xp : sumYp += yp
            sumXpX += xp * x : sumXpY += xp * y
            sumYpX += yp * x : sumYpY += yp * y
        Next

        ' Matrix A'A and vector A'b
        Dim ata_00 = sumX2, ata_01 = sumXY, ata_02 = sumX
        Dim ata_10 = sumXY, ata_11 = sumY2, ata_12 = sumY
        Dim ata_20 = sumX, ata_21 = sumY, ata_22 = n

        Dim atb_0 = sumXpX, atb_1 = sumXpY, atb_2 = sumXp
        Dim atb_3 = sumYpX, atb_4 = sumYpY, atb_5 = sumYp

        ' Solve two 3x3 systems (one for x', one for y')
        Dim det = ata_00 * (ata_11 * ata_22 - ata_12 * ata_21) -
              ata_01 * (ata_10 * ata_22 - ata_12 * ata_20) +
              ata_02 * (ata_10 * ata_21 - ata_11 * ata_20)

        If std.Abs(det) < 0.000000001 Then
            ' Matrix is singular, cannot solve, return identity
            a = 1 : b = 0 : c = 0
            d = 0 : e = 1 : f = 0
            Return
        End If
        Dim invDet = 1.0 / det

        Dim inv_00 = (ata_11 * ata_22 - ata_12 * ata_21) * invDet
        Dim inv_01 = (ata_02 * ata_21 - ata_01 * ata_22) * invDet
        Dim inv_02 = (ata_01 * ata_12 - ata_02 * ata_11) * invDet
        Dim inv_10 = (ata_12 * ata_20 - ata_10 * ata_22) * invDet
        Dim inv_11 = (ata_00 * ata_22 - ata_02 * ata_20) * invDet
        Dim inv_12 = (ata_02 * ata_10 - ata_00 * ata_12) * invDet
        Dim inv_20 = (ata_10 * ata_21 - ata_11 * ata_20) * invDet
        Dim inv_21 = (ata_01 * ata_20 - ata_00 * ata_21) * invDet
        Dim inv_22 = (ata_00 * ata_11 - ata_01 * ata_10) * invDet

        ' p = inv(A'A) * A'b
        a = inv_00 * atb_0 + inv_01 * atb_1 + inv_02 * atb_2
        b = inv_10 * atb_0 + inv_11 * atb_1 + inv_12 * atb_2
        c = inv_20 * atb_0 + inv_21 * atb_1 + inv_22 * atb_2

        d = inv_00 * atb_3 + inv_01 * atb_4 + inv_02 * atb_5
        e = inv_10 * atb_3 + inv_11 * atb_4 + inv_12 * atb_5
        f = inv_20 * atb_3 + inv_21 * atb_4 + inv_22 * atb_5
    End Sub
End Class
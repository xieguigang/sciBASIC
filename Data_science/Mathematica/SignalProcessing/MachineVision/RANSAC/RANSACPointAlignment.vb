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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing.HungarianAlgorithm
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

''' <summary>
''' A static utility class for aligning two 2D polygons using the RANSAC algorithm.
''' This version is optimized for correctness and includes a proper least-squares refinement step.
''' </summary>
Public Module RANSACPointAlignment

    Public Function MakeHungarianAssignment(sourcePoly As Polygon2D, targetPoly As Polygon2D, t As AffineTransform) As Integer()
        Dim distanceMap As New DistanceMap(Of PointF)(t.ApplyTo(sourcePoly).AsEnumerable, targetPoly.AsEnumerable, AddressOf Distance)
        Dim assignMap As Integer() = HungarianAlgorithm.FindAssignments(distanceMap.GetMap)

        Return assignMap
    End Function

    ''' <summary>
    ''' Aligns a source polygon to a target polygon using RANSAC.
    ''' </summary>
    ''' <param name="sourcePoly">The polygon to be transformed.</param>
    ''' <param name="targetPoly">The polygon to align to.</param>
    ''' <param name="iterations">The number of RANSAC iterations.</param>
    ''' <param name="distanceThreshold">The distance threshold to consider a point an inlier.</param>
    ''' <returns>The best-fit Transform object.</returns>
    Public Function AlignPolygons(Of T As Layout2D)(sourcePoly As T(),
                                                    targetPoly As T(),
                                                    properties As Func(Of T, Double()),
                                                    Optional iterations As Integer = 1000,
                                                    Optional distanceThreshold As Double = 0.1) As AffineTransform
        ' Pre-check: need at least 3 points
        If sourcePoly.Length < 2 OrElse targetPoly.Length < 2 Then
            Return New AffineTransform
        End If

        ' 1. Compute descriptors for all points in both polygons
        Dim sourceDescriptors = PointWithDescriptor.NormalizeProperties(PointWithDescriptor.ComputeDescriptors(sourcePoly, properties).ToArray)
        Dim targetDescriptors = PointWithDescriptor.NormalizeProperties(PointWithDescriptor.ComputeDescriptors(targetPoly, properties).ToArray)

        ' 2. Generate candidate matches based on descriptor similarity
        Dim candidateMatches As (source As PointF, target As PointF)() = PointWithDescriptor _
            .GenerateCandidateMatches(sourceDescriptors, targetDescriptors) _
            .ToArray

        If candidateMatches.Length < 3 Then
            ' Not enough candidate matches to proceed
            Return New AffineTransform
        Else
            Return candidateMatches.MakeAlignment(iterations, distanceThreshold)
        End If
    End Function

    ''' <summary>
    ''' Aligns a source polygon to a target polygon using RANSAC.
    ''' </summary>
    ''' <param name="sourcePoly">The polygon to be transformed.</param>
    ''' <param name="targetPoly">The polygon to align to.</param>
    ''' <param name="iterations">The number of RANSAC iterations.</param>
    ''' <param name="distanceThreshold">The distance threshold to consider a point an inlier.</param>
    ''' <returns>The best-fit Transform object.</returns>
    Public Function AlignPolygons(sourcePoly As Polygon2D,
                                  targetPoly As Polygon2D,
                                  Optional iterations As Integer = 1000,
                                  Optional distanceThreshold As Double = 0.1) As AffineTransform

        ' Pre-check: need at least 3 points
        If sourcePoly.length < 2 OrElse targetPoly.length < 2 Then
            Return New AffineTransform
        End If

        ' 1. Compute descriptors for all points in both polygons
        Dim sourceDescriptors = PointWithDescriptor.ComputeDescriptors(sourcePoly).ToArray
        Dim targetDescriptors = PointWithDescriptor.ComputeDescriptors(targetPoly).ToArray

        ' 2. Generate candidate matches based on descriptor similarity
        Dim candidateMatches As (source As PointF, target As PointF)() = PointWithDescriptor _
            .GenerateCandidateMatches(sourceDescriptors, targetDescriptors) _
            .ToArray

        If candidateMatches.Length < 3 Then
            ' Not enough candidate matches to proceed
            Return New AffineTransform
        Else
            Return candidateMatches.MakeAlignment(iterations, distanceThreshold)
        End If
    End Function

    ''' <summary>
    ''' Aligns a source polygon to a target polygon using RANSAC.
    ''' </summary>
    ''' <param name="iterations">The number of RANSAC iterations.</param>
    ''' <param name="distanceThreshold">The distance threshold to consider a point an inlier.</param>
    ''' <returns>The best-fit Transform object.</returns>
    <Extension>
    Private Function MakeAlignment(candidateMatches As (source As PointF, target As PointF)(), iterations As Integer, distanceThreshold As Double) As AffineTransform
        Dim bestTransform As New AffineTransform
        Dim maxInliers As Integer = 0
        Dim thresholdSq = distanceThreshold * distanceThreshold
        Dim bar As Tqdm.ProgressBar = Nothing
        Dim updateHits As i32 = 0

        ' RANSAC 迭代
        For Each iter As Integer In TqdmWrapper.Range(0, iterations, bar:=bar, wrap_console:=App.EnableTqdm)
            ' make sampling of 3 data points from the generated candidate matches
            Dim matches = candidateMatches.OrderBy(Function(x) rand.NextDouble()).Take(3).ToArray()

            Dim p1 = matches(0).source, q1 = matches(0).target
            Dim p2 = matches(1).source, q2 = matches(1).target
            Dim p3 = matches(2).source, q3 = matches(2).target

            ' Compute a transform hypothesis from these 3 matches
            Dim hypothesisTransform As AffineTransform = ComputeAffineFrom3Pairs(p1, p2, p3, q1, q2, q3)
            ' Count inliers for this hypothesis across ALL candidate matches
            Dim currentInliers As Integer = 0

            For Each pair As (source As PointF, target As PointF) In candidateMatches
                Dim transformedSourcePt = hypothesisTransform.ApplyToPoint(pair.source)
                Dim dx = transformedSourcePt.X - pair.target.X
                Dim dy = transformedSourcePt.Y - pair.target.Y

                If dx * dx + dy * dy <= thresholdSq Then
                    currentInliers += 1
                End If
            Next

            ' 5. Update best transform if this one is better
            If currentInliers > maxInliers Then
                maxInliers = currentInliers
                bestTransform = hypothesisTransform

                Call bar.SetLabel($"UPDATE[{++updateHits}] inliers: {currentInliers}; best-transform: {bestTransform}")
            End If
        Next

        ' 6. Refine the best transform using all inliers with Least Squares
        If maxInliers >= 3 Then
            Return RefineTransformWithLeastSquares(candidateMatches, bestTransform, distanceThreshold)
        End If

        Return bestTransform
    End Function

    ''' <summary>
    ''' Computes an affine transform from exactly three point pairs.
    ''' </summary>
    Private Function ComputeAffineFrom3Pairs(p1 As PointF, p2 As PointF, p3 As PointF,
                                             q1 As PointF, q2 As PointF, q3 As PointF) As AffineTransform
        ' Solve the linear system for 6 parameters (a,b,c,d,e,f)
        ' |x1 y1 1 0  0  0|   |a|   |x'1|
        ' |x2 y2 1 0  0  0|   |b|   |x'2|
        ' |x3 y3 1 0  0  0| * |c| = |x'3|
        ' |0  0  0 x1 y1 1|   |d|   |y'1|
        ' |0  0  0 x2 y2 1|   |e|   |y'2|
        ' |0  0  0 x3 y3 1|   |f|   |y'3|
        ' We can solve this by splitting it into two 3x3 systems.
        ' Dim sourceX = {p1.X, p2.X, p3.X}
        ' Dim sourceY = {p1.Y, p2.Y, p3.Y}
        ' Dim targetX = {q1.X, q2.X, q3.X}
        ' Dim targetY = {q1.Y, q2.Y, q3.Y}

        ' Using the same least squares solver for the exact case
        Dim pairs As (source As PointF, target As PointF)() = {(p1, q1), (p2, q2), (p3, q3)}
        Dim a, b, c, d, e, f As Double

        Call SolveLeastSquaresAffine(pairs, a, b, c, d, e, f)

        Return New AffineTransform With {
            .a = a,
            .b = b,
            .c = c,
            .d = d,
            .e = e,
            .f = f
        }
    End Function

    ''' <summary>
    ''' Refines the transformation using all inliers with a least-squares fit for an affine transform.
    ''' </summary>
    Private Function RefineTransformWithLeastSquares(candidateMatches As (source As PointF, target As PointF)(), initialTransform As AffineTransform, threshold As Double) As AffineTransform
        Dim inlierPairs As New List(Of (source As PointF, target As PointF))
        Dim thresholdSq = threshold * threshold
        Dim errors As New List(Of Double)

        For Each pair As (source As PointF, target As PointF) In candidateMatches
            Dim transformedSourcePt = initialTransform.ApplyToPoint(pair.source)
            Dim dx = transformedSourcePt.X - pair.target.X
            Dim dy = transformedSourcePt.Y - pair.target.Y
            Dim dSq As Double = dx * dx + dy * dy

            Call errors.Add(dSq)

            If dSq <= thresholdSq Then
                inlierPairs.Add(pair)
            End If
        Next

        If inlierPairs.Count < 3 Then
            Return initialTransform
        Else
            Call $"error of this RANSAC alignment: {errors.Average}".debug
        End If

        Dim a, b, c, d, e, f As Double

        Call SolveLeastSquaresAffine(inlierPairs.ToArray, a, b, c, d, e, f)

        Return New AffineTransform With {
            .a = a,
            .b = b,
            .c = c,
            .d = d,
            .e = e,
            .f = f
        }
    End Function

    ''' <summary>
    ''' Solves the least-squares problem for affine transformation parameters.
    ''' This function is unchanged from your original, as it was mathematically correct.
    ''' </summary>
    Private Sub SolveLeastSquaresAffine(ByRef points As (source As PointF, target As PointF)(), ByRef a As Double, ByRef b As Double, ByRef c As Double, ByRef d As Double, ByRef e As Double, ByRef f As Double)
        Dim n As Integer = points.Length
        Dim sumX, sumY, sumX2, sumY2, sumXY, sumXp, sumYp, sumXpX, sumXpY, sumYpX, sumYpY As Double

        For Each pair In points
            Dim x As Double = pair.source.X
            Dim y As Double = pair.source.Y
            Dim xp As Double = pair.target.X
            Dim yp As Double = pair.target.Y

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
End Module
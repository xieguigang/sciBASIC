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
                                         Optional distanceThreshold As Double = 0.1) As Transform
        ' 预检查：点数不足时返回单位变换
        If sourcePoly.length < 2 OrElse targetPoly.length < 2 Then
            Return New Transform()
        End If

        Dim bestTransform As New Transform()
        Dim maxInliers As Integer = 0

        ' RANSAC 迭代
        For iter As Integer = 1 To iterations
            ' 1. 随机选择两个不同的点
            Dim idx1, idx2 As Integer
            Do
                idx1 = rand.Next(sourcePoly.length)
                idx2 = rand.Next(sourcePoly.length)
            Loop While idx1 = idx2

            Dim p1 = sourcePoly(idx1)
            Dim p2 = sourcePoly(idx2)

            ' 2. 在目标多边形中找到对应的最近邻点
            Dim q1 = FindClosestPoint(targetPoly, p1)
            Dim q2 = FindClosestPoint(targetPoly, p2)

            ' 3. 计算初始变换参数
            ' 注意：这里我们计算一个相似变换（均匀缩放）作为初始估计
            Dim initialTransform = ComputeSimilarityTransform(p1, p2, q1, q2)

            ' 4. 统计内点数量
            Dim inliers = CountInliers(sourcePoly, targetPoly, initialTransform, distanceThreshold)

            ' 5. 更新最佳变换
            If inliers > maxInliers Then
                maxInliers = inliers
                bestTransform = initialTransform
            End If
        Next

        ' 6. 使用所有内点重新精炼变换（关键优化）
        If maxInliers > 0 Then
            Return RefineTransform(sourcePoly, targetPoly, bestTransform, distanceThreshold)
        End If

        ' 如果没有找到好的匹配，返回默认或最佳估计
        Return bestTransform
    End Function

    ''' <summary>
    ''' Finds the closest point in a polygon to a given point.
    ''' </summary>
    ''' <remarks>This is a performance bottleneck. For large polygons, a spatial index (e.g., KD-Tree) is recommended.</remarks>
    Private Shared Function FindClosestPoint(poly As Polygon2D, pt As PointF) As PointF
        Dim minDistSq As Double = Double.PositiveInfinity
        Dim closestPt As PointF

        For i As Integer = 0 To poly.length - 1
            Dim polyPt As PointF = poly(i)
            Dim dx = polyPt.X - pt.X
            Dim dy = polyPt.Y - pt.Y
            Dim distSq = dx * dx + dy * dy

            If distSq < minDistSq Then
                minDistSq = distSq
                closestPt = polyPt
            End If
        Next

        Return closestPt
    End Function

    ''' <summary>
    ''' Computes a similarity transform (uniform scale, rotation, translation) from two point pairs.
    ''' </summary>
    Private Shared Function ComputeSimilarityTransform(p1 As PointF, p2 As PointF, q1 As PointF, q2 As PointF) As Transform
        ' Translate points so that p1 is at the origin
        Dim p2_trans As New PointF(p2.X - p1.X, p2.Y - p1.Y)
        Dim q2_trans As New PointF(q2.X - q1.X, q2.Y - q1.Y)

        ' Compute rotation angle
        Dim angleSource = std.Atan2(p2_trans.Y, p2_trans.X)
        Dim angleTarget = std.Atan2(q2_trans.Y, q2_trans.X)
        Dim theta = angleTarget - angleSource

        ' Normalize angle to [-PI, PI]
        While theta > std.PI
            theta -= 2 * std.PI
        End While
        While theta < -std.PI
            theta += 2 * std.PI
        End While

        ' Compute uniform scale
        Dim sourceLen = std.Sqrt(p2_trans.X * p2_trans.X + p2_trans.Y * p2_trans.Y)
        Dim targetLen = std.Sqrt(q2_trans.X * q2_trans.X + q2_trans.Y * q2_trans.Y)
        Dim scale = If(sourceLen > 0.000001, targetLen / sourceLen, 1.0)

        ' Compute translation
        ' The translation is simply the difference between q1 and the transformed p1
        Dim tx = q1.X - (p1.X * scale * std.Cos(theta) - p1.Y * scale * std.Sin(theta))
        Dim ty = q1.Y - (p1.X * scale * std.Sin(theta) + p1.Y * scale * std.Cos(theta))

        Return New Transform With {
            .theta = theta,
            .scalex = scale,
            .scaley = scale, ' Uniform scale for initial estimate
            .tx = tx,
            .ty = ty
        }
    End Function

    ''' <summary>
    ''' Counts the number of inlier points after applying a transformation.
    ''' </summary>
    Private Shared Function CountInliers(sourcePoly As Polygon2D, targetPoly As Polygon2D, t As Transform, threshold As Double) As Integer
        Dim inliers = 0
        Dim cosTheta = std.Cos(t.theta)
        Dim sinTheta = std.Sin(t.theta)
        Dim thresholdSq = threshold * threshold

        For i As Integer = 0 To sourcePoly.length - 1
            Dim sourcePt = sourcePoly(i)

            ' Apply transformation: Scale -> Rotate -> Translate
            Dim xScaled = sourcePt.X * t.scalex
            Dim yScaled = sourcePt.Y * t.scaley

            Dim xRotated = xScaled * cosTheta - yScaled * sinTheta
            Dim yRotated = xScaled * sinTheta + yScaled * cosTheta

            Dim transformedPt As New PointF(xRotated + t.tx, yRotated + t.ty)

            ' Find nearest neighbor in target and check distance
            Dim closestTargetPt = FindClosestPoint(targetPoly, transformedPt)
            Dim dx = transformedPt.X - closestTargetPt.X
            Dim dy = transformedPt.Y - closestTargetPt.Y

            If dx * dx + dy * dy <= thresholdSq Then
                inliers += 1
            End If
        Next

        Return inliers
    End Function

    ''' <summary>
    ''' Refines the transformation using all inliers with a least-squares fit for an affine transform.
    ''' </summary>
    ''' <remarks>
    ''' This is the most critical optimization. It solves for the best-fit affine parameters
    ''' (a, b, c, d, e, f) in the equations:
    ''' x' = a*x + b*y + c
    ''' y' = d*x + e*y + f
    ''' Then, it decomposes these parameters back into rotation, scale, and translation.
    ''' </remarks>
    Private Shared Function RefineTransform(sourcePoly As Polygon2D, targetPoly As Polygon2D, initialTransform As Transform, threshold As Double) As Transform
        Dim inlierPairs As New List(Of (source As PointF, target As PointF))
        Dim cosTheta = std.Cos(initialTransform.theta)
        Dim sinTheta = std.Sin(initialTransform.theta)
        Dim thresholdSq = threshold * threshold

        ' 1. Collect all inlier point pairs based on the initial transform
        For i As Integer = 0 To sourcePoly.length - 1
            Dim sourcePt = sourcePoly(i)

            Dim xScaled = sourcePt.X * initialTransform.scalex
            Dim yScaled = sourcePt.Y * initialTransform.scaley
            Dim xRotated = xScaled * cosTheta - yScaled * sinTheta
            Dim yRotated = xScaled * sinTheta + yScaled * cosTheta
            Dim transformedPt As New PointF(xRotated + initialTransform.tx, yRotated + initialTransform.ty)

            Dim closestTargetPt = FindClosestPoint(targetPoly, transformedPt)
            Dim dx = transformedPt.X - closestTargetPt.X
            Dim dy = transformedPt.Y - closestTargetPt.Y

            If dx * dx + dy * dy <= thresholdSq Then
                inlierPairs.Add((sourcePt, closestTargetPt))
            End If
        Next

        If inlierPairs.Count < 3 Then
            ' Not enough points for a stable affine fit
            Return initialTransform
        End If

        ' 2. Solve for affine parameters using Least Squares
        ' We want to solve A * p = b, where p = [a, b, c, d, e, f]'
        Dim a, b, c, d, e, f As Double
        SolveLeastSquaresAffine(inlierPairs, a, b, c, d, e, f)

        ' 3. Decompose the affine matrix to get rotation, scale, and translation
        ' The affine matrix is:
        ' | a b |
        ' | d e |
        ' Using polar decomposition: A = R * S, where R is rotation and S is scale/shear
        ' For a pure rotation+scale (no shear), we can extract:
        Dim scaleX = std.Sqrt(a * a + d * d)
        Dim scaleY = std.Sqrt(b * b + e * e)
        Dim theta = std.Atan2(d, a) ' Rotation from the first column

        ' Translation is simply c and f
        Dim tx = c
        Dim ty = f

        Return New Transform With {
            .theta = theta,
            .scalex = scaleX,
            .scaley = scaleY,
            .tx = tx,
            .ty = ty
        }
    End Function

    ''' <summary>
    ''' Solves the least-squares problem for affine transformation parameters.
    ''' </summary>
    Private Shared Sub SolveLeastSquaresAffine(points As List(Of (source As PointF, target As PointF)), ByRef a As Double, ByRef b As Double, ByRef c As Double, ByRef d As Double, ByRef e As Double, ByRef f As Double)
        ' This implementation uses the normal equations: (A'A)p = A'b
        ' For robustness, a library like MathNet.Numerics is recommended.
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
        ' Inverting a 3x3 matrix manually:
        Dim det = ata_00 * (ata_11 * ata_22 - ata_12 * ata_21) -
                  ata_01 * (ata_10 * ata_22 - ata_12 * ata_20) +
                  ata_02 * (ata_10 * ata_21 - ata_11 * ata_20)

        If std.Abs(det) < 0.000000001 Then
            ' Matrix is singular, cannot solve
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
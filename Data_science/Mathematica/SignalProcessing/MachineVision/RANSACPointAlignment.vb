Imports Microsoft.VisualBasic.Imaging.Math2D
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public Class RANSACPointAlignment

    Dim centerSource As (cx As Double, cy As Double)
    Dim centerTarget As (cx As Double, cy As Double)

    ' 主对齐函数：返回包含独立缩放因子的元组
    Public Shared Function AlignPolygons(sourcePoly As Polygon2D, targetPoly As Polygon2D, Optional iterations As Integer = 1000, Optional distanceThreshold As Double = 0.1) As Transform

        ' 计算源和目标多边形的中心点
        Dim ransac As New RANSACPointAlignment With {
            .centerSource = CalculateCenter(sourcePoly),
            .centerTarget = CalculateCenter(targetPoly)
        }
        ' 初始化最佳参数
        Dim bestTheta As Double = 0
        Dim bestTx As Double = 0
        Dim bestTy As Double = 0
        Dim bestScalex As Double = 1.0
        Dim bestScaley As Double = 1.0
        Dim maxInliers As Integer = 0

        ' 预检查：点数不足时返回单位变换
        If sourcePoly.length < 2 OrElse targetPoly.length < 2 Then
            Return (0, 0, 0, 1.0, 1.0)
        End If

        ' RANSAC 迭代
        For iter As Integer = 1 To iterations
            ' 随机选择两个点
            Dim idx1 = rand.Next(sourcePoly.length)
            Dim idx2 = rand.Next(sourcePoly.length)
            If idx1 = idx2 Then Continue For

            Dim p1 = (sourcePoly.xpoints(idx1), sourcePoly.ypoints(idx1))
            Dim p2 = (sourcePoly.xpoints(idx2), sourcePoly.ypoints(idx2))

            Dim q1 = FindClosestPoint(targetPoly, p1.Item1, p1.Item2)
            Dim q2 = FindClosestPoint(targetPoly, p2.Item1, p2.Item2)

            ' 计算相似变换参数（包括独立缩放因子）
            Dim transformParams = ransac.ComputeSimilarityTransform(p1, p2, q1, q2)

            ' 统计内点数量
            Dim inliers = ransac.CountInliers(sourcePoly, targetPoly, transformParams.theta, transformParams.tx, transformParams.ty, transformParams.scalex, transformParams.scaley, distanceThreshold)

            ' 更新最佳变换
            If inliers > maxInliers Then
                maxInliers = inliers
                bestTheta = transformParams.theta
                bestTx = transformParams.tx
                bestTy = transformParams.ty
                bestScalex = transformParams.scalex
                bestScaley = transformParams.scaley
            End If
        Next

        ' 用所有内点重新精炼变换
        If maxInliers > 0 Then
            Return ransac.RefineTransform(sourcePoly, targetPoly, bestTheta, bestTx, bestTy, bestScalex, bestScaley, distanceThreshold)
        End If

        Return (bestTheta, bestTx, bestTy, bestScalex, bestScaley)
    End Function

    ' 计算多边形中心点
    Private Shared Function CalculateCenter(poly As Polygon2D) As (cx As Double, cy As Double)
        Dim cx As Double = 0
        Dim cy As Double = 0
        For i As Integer = 0 To poly.length - 1
            cx += poly.xpoints(i)
            cy += poly.ypoints(i)
        Next
        cx /= poly.length
        cy /= poly.length
        Return (cx, cy)
    End Function

    ' 在目标多边形中找最近邻点（保持不变）
    Private Shared Function FindClosestPoint(
        poly As Polygon2D,
        x As Double,
        y As Double
    ) As (x As Double, y As Double)
        Dim minDist = Double.PositiveInfinity
        Dim closestX = 0.0
        Dim closestY = 0.0

        For i = 0 To poly.length - 1
            Dim dx = poly.xpoints(i) - x
            Dim dy = poly.ypoints(i) - y
            Dim dist = dx * dx + dy * dy ' 避免开方优化性能

            If dist < minDist Then
                minDist = dist
                closestX = poly.xpoints(i)
                closestY = poly.ypoints(i)
            End If
        Next

        Return (closestX, closestY)
    End Function

    ' 计算相似变换参数（独立缩放因子）
    Private Function ComputeSimilarityTransform(
        p1 As (Double, Double),
        p2 As (Double, Double),
        q1 As (Double, Double),
        q2 As (Double, Double)
    ) As (theta As Double, tx As Double, ty As Double, scalex As Double, scaley As Double)

        ' 计算源向量和目标向量
        Dim vSourceX = p2.Item1 - p1.Item1
        Dim vSourceY = p2.Item2 - p1.Item2
        Dim vTargetX = q2.Item1 - q1.Item1
        Dim vTargetY = q2.Item2 - q1.Item2

        ' 计算旋转角度
        Dim dot = vSourceX * vTargetX + vSourceY * vTargetY
        Dim cross = vSourceX * vTargetY - vSourceY * vTargetX
        Dim theta = std.Atan2(cross, dot)

        ' 计算独立缩放因子：分别处理X和Y方向
        Dim scalex = 1.0
        Dim scaley = 1.0

        ' 计算源点和目标点相对于各自中心点的坐标
        Dim s1x = p1.Item1 - centerSource.cx
        Dim s1y = p1.Item2 - centerSource.cy
        Dim s2x = p2.Item1 - centerSource.cx
        Dim s2y = p2.Item2 - centerSource.cy

        Dim t1x = q1.Item1 - centerTarget.cx
        Dim t1y = q1.Item2 - centerTarget.cy
        Dim t2x = q2.Item1 - centerTarget.cx
        Dim t2y = q2.Item2 - centerTarget.cy

        ' 将目标点旋转-theta角度，对齐到源点方向
        Dim cosNegTheta = std.Cos(-theta)
        Dim sinNegTheta = std.Sin(-theta)
        Dim rotatedT1x = t1x * cosNegTheta - t1y * sinNegTheta
        Dim rotatedT1y = t1x * sinNegTheta + t1y * cosNegTheta
        Dim rotatedT2x = t2x * cosNegTheta - t2y * sinNegTheta
        Dim rotatedT2y = t2x * sinNegTheta + t2y * cosNegTheta

        ' 计算缩放因子（避免除零）
        If std.Abs(s1x) > 0.0000000001 Then scalex = rotatedT1x / s1x
        If std.Abs(s2x) > 0.0000000001 Then scalex = (scalex + rotatedT2x / s2x) / 2
        If std.Abs(s1y) > 0.0000000001 Then scaley = rotatedT1y / s1y
        If std.Abs(s2y) > 0.0000000001 Then scaley = (scaley + rotatedT2y / s2y) / 2

        ' 计算平移量：目标中心 - 变换后的源中心
        Dim transformedCenterX = centerSource.cx * scalex
        Dim transformedCenterY = centerSource.cy * scaley
        Dim cosTheta = std.Cos(theta)
        Dim sinTheta = std.Sin(theta)
        Dim rotatedCenterX = transformedCenterX * cosTheta - transformedCenterY * sinTheta
        Dim rotatedCenterY = transformedCenterX * sinTheta + transformedCenterY * cosTheta
        Dim tx = centerTarget.cx - rotatedCenterX
        Dim ty = centerTarget.cy - rotatedCenterY

        Return (theta, tx, ty, scalex, scaley)
    End Function

    ' 统计内点数量（应用绕中心点的变换）
    Private Function CountInliers(
        sourcePoly As Polygon2D,
        targetPoly As Polygon2D,
        theta As Double,
        tx As Double,
        ty As Double,
        scalex As Double,
        scaley As Double,
        threshold As Double
    ) As Integer

        Dim inliers = 0
        Dim cosTheta = std.Cos(theta)
        Dim sinTheta = std.Sin(theta)
        Dim thresholdSq = threshold * threshold

        For i = 0 To sourcePoly.length - 1
            ' 绕中心点缩放
            Dim xScaled = centerSource.cx + (sourcePoly.xpoints(i) - centerSource.cx) * scalex
            Dim yScaled = centerSource.cy + (sourcePoly.ypoints(i) - centerSource.cy) * scaley

            ' 绕中心点旋转
            Dim xRel = xScaled - centerSource.cx
            Dim yRel = yScaled - centerSource.cy
            Dim xRotated = centerSource.cx + (xRel * cosTheta - yRel * sinTheta)
            Dim yRotated = centerSource.cy + (xRel * sinTheta + yRel * cosTheta)

            ' 平移
            Dim xTrans = xRotated + tx
            Dim yTrans = yRotated + ty

            ' 找最近邻并计算距离平方
            Dim close = FindClosestPoint(targetPoly, xTrans, yTrans)
            Dim dx = xTrans - close.x
            Dim dy = yTrans - close.y
            Dim distSq = dx * dx + dy * dy

            If distSq <= thresholdSq Then
                inliers += 1
            End If
        Next

        Return inliers
    End Function

    ' 用所有内点重新精炼变换（最小二乘优化，考虑独立缩放因子）
    Private Function RefineTransform(
        sourcePoly As Polygon2D,
        targetPoly As Polygon2D,
        initTheta As Double,
        initTx As Double,
        initTy As Double,
        initScalex As Double,
        initScaley As Double,
        threshold As Double
    ) As (theta As Double, tx As Double, ty As Double, scalex As Double, scaley As Double)

        ' 收集所有内点对
        Dim inlierPairs As New List(Of (sx As Double, sy As Double, tx As Double, ty As Double))
        Dim cosTheta = std.Cos(initTheta)
        Dim sinTheta = std.Sin(initTheta)
        Dim thresholdSq = threshold * threshold

        For i = 0 To sourcePoly.length - 1
            Dim sx = sourcePoly.xpoints(i)
            Dim sy = sourcePoly.ypoints(i)

            ' 应用初始变换（绕中心点缩放和旋转，然后平移）
            Dim xScaled = centerSource.cx + (sx - centerSource.cx) * initScalex
            Dim yScaled = centerSource.cy + (sy - centerSource.cy) * initScaley
            Dim xRel = xScaled - centerSource.cx
            Dim yRel = yScaled - centerSource.cy
            Dim xRotated = centerSource.cx + (xRel * cosTheta - yRel * sinTheta)
            Dim yRotated = centerSource.cy + (xRel * sinTheta + yRel * cosTheta)
            Dim xTrans = xRotated + initTx
            Dim yTrans = yRotated + initTy

            Dim close = FindClosestPoint(targetPoly, xTrans, yTrans)
            Dim dx = xTrans - close.x
            Dim dy = yTrans - close.y
            Dim distSq = dx * dx + dy * dy

            If distSq <= thresholdSq Then
                inlierPairs.Add((sx, sy, close.x, close.y))
            End If
        Next

        ' 使用所有内点对，通过最小二乘法重新计算相似变换参数
        If inlierPairs.Count >= 2 Then
            Dim sumTheta = 0.0
            Dim sumTx = 0.0
            Dim sumTy = 0.0
            Dim sumScalex = 0.0
            Dim sumScaley = 0.0
            Dim count = 0

            For i = 0 To inlierPairs.Count - 2
                For j = i + 1 To inlierPairs.Count - 1
                    Dim pair1 = inlierPairs(i)
                    Dim pair2 = inlierPairs(j)
                    Dim p1 = (pair1.sx, pair1.sy)
                    Dim p2 = (pair2.sx, pair2.sy)
                    Dim q1 = (pair1.tx, pair1.ty)
                    Dim q2 = (pair2.tx, pair2.ty)

                    Dim params = ComputeSimilarityTransform(p1, p2, q1, q2)
                    sumTheta += params.theta
                    sumTx += params.tx
                    sumTy += params.ty
                    sumScalex += params.scalex
                    sumScaley += params.scaley
                    count += 1
                Next
            Next

            If count > 0 Then
                Return (sumTheta / count, sumTx / count, sumTy / count, sumScalex / count, sumScaley / count)
            End If
        End If

        Return (initTheta, initTx, initTy, initScalex, initScaley)
    End Function
End Class
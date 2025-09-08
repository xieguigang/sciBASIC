Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Public Class RANSACPointAlignment

    ' 主对齐函数：现在返回包含缩放因子的元组
    Public Shared Function AlignPolygons(
        sourcePoly As Polygon2D,
        targetPoly As Polygon2D,
        iterations As Integer,
        distanceThreshold As Double
    ) As (theta As Double, tx As Double, ty As Double, scale As Double) ' 修改返回值类型

        ' 初始化最佳参数
        Dim bestTheta As Double = 0
        Dim bestTx As Double = 0
        Dim bestTy As Double = 0
        Dim bestScale As Double = 1.0 ' 初始化缩放因子为1（无缩放）
        Dim maxInliers As Integer = 0
        Dim rand As New Random()

        ' 预检查：点数不足时返回单位变换
        If sourcePoly.length < 2 OrElse targetPoly.length < 2 Then
            Return (0, 0, 0, 1.0) ' 返回单位缩放
        End If

        ' RANSAC 迭代
        For iter = 1 To iterations
            ' 1. 随机选择两个点
            Dim idx1 = rand.Next(sourcePoly.length)
            Dim idx2 = rand.Next(sourcePoly.length)
            If idx1 = idx2 Then Continue For

            ' 获取源点
            Dim p1 = (sourcePoly.xpoints(idx1), sourcePoly.ypoints(idx1))
            Dim p2 = (sourcePoly.xpoints(idx2), sourcePoly.ypoints(idx2))

            ' 2. 在目标多边形中找最近邻
            Dim q1 = FindClosestPoint(targetPoly, p1.Item1, p1.Item2)
            Dim q2 = FindClosestPoint(targetPoly, p2.Item1, p2.Item2)

            ' 3. 计算相似变换参数（旋转、平移、缩放）
            Dim transformParams As (theta As Double, tx As Double, ty As Double, scale As Double) = ComputeSimilarityTransform(p1, p2, q1, q2) ' 修改为计算相似变换

            ' 4. 统计内点数量
            Dim inliers = CountInliers(sourcePoly, targetPoly, transformParams.theta, transformParams.tx, transformParams.ty, transformParams.scale, distanceThreshold) ' 传入缩放因子

            ' 5. 更新最佳变换
            If inliers > maxInliers Then
                maxInliers = inliers
                bestTheta = transformParams.theta
                bestTx = transformParams.tx
                bestTy = transformParams.ty
                bestScale = transformParams.scale
            End If
        Next

        ' 可选：用所有内点重新精炼变换
        If maxInliers > 0 Then
            Return RefineTransform(sourcePoly, targetPoly, bestTheta, bestTx, bestTy, bestScale, distanceThreshold) ' 传入缩放因子
        End If

        Return (bestTheta, bestTx, bestTy, bestScale)
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

    ' 计算相似变换参数（旋转角度、平移量、缩放因子）
    Private Shared Function ComputeSimilarityTransform(
        p1 As (Double, Double),
        p2 As (Double, Double),
        q1 As (Double, Double),
        q2 As (Double, Double)
    ) As (theta As Double, tx As Double, ty As Double, scale As Double)
        ' 计算源向量和目标向量
        Dim vSourceX = p2.Item1 - p1.Item1
        Dim vSourceY = p2.Item2 - p1.Item2
        Dim vTargetX = q2.Item1 - q1.Item1
        Dim vTargetY = q2.Item2 - q1.Item2

        ' 计算缩放因子：目标向量模长与源向量模长的比值
        Dim lenSource = std.Sqrt(vSourceX * vSourceX + vSourceY * vSourceY)
        Dim lenTarget = std.Sqrt(vTargetX * vTargetX + vTargetY * vTargetY)

        If lenSource < 0.0000000001 Then ' 避免除零
            Return (0, 0, 0, 1.0) ' 返回单位变换
        End If

        Dim scale = lenTarget / lenSource ' 缩放因子

        ' 计算旋转角度（向量夹角）
        Dim dot = vSourceX * vTargetX + vSourceY * vTargetY
        Dim cross = vSourceX * vTargetY - vSourceY * vTargetX
        Dim theta = std.Atan2(cross, dot) ' 旋转角度（弧度）

        ' 计算源点和目标点的中心
        Dim cxSource = (p1.Item1 + p2.Item1) / 2
        Dim cySource = (p1.Item2 + p2.Item2) / 2
        Dim cxTarget = (q1.Item1 + q2.Item1) / 2
        Dim cyTarget = (q1.Item2 + q2.Item2) / 2

        ' 应用缩放和旋转到源中心点
        Dim cosTheta = std.Cos(theta)
        Dim sinTheta = std.Sin(theta)
        Dim transformedCx = scale * (cxSource * cosTheta - cySource * sinTheta)
        Dim transformedCy = scale * (cxSource * sinTheta + cySource * cosTheta)

        ' 计算平移量：目标中心 - 变换后的源中心
        Dim tx = cxTarget - transformedCx
        Dim ty = cyTarget - transformedCy

        Return (theta, tx, ty, scale)
    End Function

    ' 统计内点数量（考虑缩放）
    Private Shared Function CountInliers(
        sourcePoly As Polygon2D,
        targetPoly As Polygon2D,
        theta As Double,
        tx As Double,
        ty As Double,
        scale As Double, ' 新增缩放参数
        threshold As Double
    ) As Integer
        Dim inliers = 0
        Dim cosTheta = std.Cos(theta)
        Dim sinTheta = std.Sin(theta)
        Dim thresholdSq = threshold * threshold ' 避免开方

        For i = 0 To sourcePoly.length - 1
            ' 应用变换（包含缩放）
            Dim x = sourcePoly.xpoints(i)
            Dim y = sourcePoly.ypoints(i)
            Dim xTrans = scale * (x * cosTheta - y * sinTheta) + tx ' 应用缩放
            Dim yTrans = scale * (x * sinTheta + y * cosTheta) + ty ' 应用缩放

            ' 找最近邻并计算距离平方
            Dim close As (closestX As Double, closestY As Double) = FindClosestPoint(targetPoly, xTrans, yTrans)
            Dim dx = xTrans - close.closestX
            Dim dy = yTrans - close.closestY
            Dim distSq = dx * dx + dy * dy

            If distSq <= thresholdSq Then
                inliers += 1
            End If
        Next

        Return inliers
    End Function

    ' 用所有内点重新精炼变换（最小二乘优化，考虑缩放）
    Private Shared Function RefineTransform(
        sourcePoly As Polygon2D,
        targetPoly As Polygon2D,
        initTheta As Double,
        initTx As Double,
        initTy As Double,
        initScale As Double, ' 新增初始缩放因子
        threshold As Double
    ) As (theta As Double, tx As Double, ty As Double, scale As Double) ' 修改返回值类型
        ' 收集所有内点对
        Dim inlierPairs As New List(Of (sx As Double, sy As Double, tx As Double, ty As Double))
        Dim cosTheta = std.Cos(initTheta)
        Dim sinTheta = std.Sin(initTheta)
        Dim thresholdSq = threshold * threshold

        For i = 0 To sourcePoly.length - 1
            Dim sx = sourcePoly.xpoints(i)
            Dim sy = sourcePoly.ypoints(i)
            ' 应用初始变换（包含缩放）
            Dim xTrans = initScale * (sx * cosTheta - sy * sinTheta) + initTx
            Dim yTrans = initScale * (sx * sinTheta + sy * cosTheta) + initTy

            Dim close As (closestX As Double, closestY As Double) = FindClosestPoint(targetPoly, xTrans, yTrans)
            Dim dx = xTrans - close.closestX
            Dim dy = yTrans - close.closestY
            Dim distSq = dx * dx + dy * dy

            If distSq <= thresholdSq Then
                inlierPairs.Add((sx, sy, close.closestX, close.closestY))
            End If
        Next

        ' 使用所有内点对，通过最小二乘法重新计算相似变换参数
        ' 这里是一个简化的实现。在实际应用中，您可能需要使用更稳健的优化方法（如SVD）。
        If inlierPairs.Count >= 2 Then ' 至少需要2个点来估计相似变换
            Dim sumScale = 0.0
            Dim sumTheta = 0.0
            Dim sumTx = 0.0
            Dim sumTy = 0.0
            Dim count = 0

            ' 遍历内点对，计算参数并平均（这是一种简化方法）
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
                    sumScale += params.scale
                    count += 1
                Next
            Next

            If count > 0 Then
                Dim bestTheta = sumTheta / count
                Dim bestTx = sumTx / count
                Dim bestTy = sumTy / count
                Dim bestScale = sumScale / count
                Return (bestTheta, bestTx, bestTy, bestScale)
            End If
        End If

        ' 如果精炼失败，返回初始值
        Return (initTheta, initTx, initTy, initScale)
    End Function
End Class
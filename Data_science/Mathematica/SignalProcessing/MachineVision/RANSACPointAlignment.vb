Imports Microsoft.VisualBasic.Imaging.Math2D
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Public Class RANSACPointAlignment

    Dim centerSource As Point
    Dim centerTarget As Point

    Dim sourcePoly As Polygon2D
    Dim targetPoly As Polygon2D

    ''' <summary>
    ''' the distance threshold
    ''' </summary>
    Dim threshold As Double = 0.1

    Private Structure Point

        Dim x As Double
        Dim y As Double

        Sub New(x As Double, y As Double)
            Me.x = x
            Me.y = y
        End Sub
    End Structure

    ' 主对齐函数：返回包含独立缩放因子的元组
    Public Shared Function AlignPolygons(sourcePoly As Polygon2D,
                                         targetPoly As Polygon2D,
                                         Optional iterations As Integer = 1000,
                                         Optional distanceThreshold As Double = 0.1) As Transform
        ' 计算源和目标多边形的中心点
        Dim ransac As New RANSACPointAlignment With {
            .centerSource = CalculateCenter(sourcePoly),
            .centerTarget = CalculateCenter(targetPoly),
            .sourcePoly = sourcePoly,
            .targetPoly = targetPoly,
            .threshold = distanceThreshold
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

            If idx1 = idx2 Then
                Continue For
            End If

            Dim p1 As New Point(sourcePoly.xpoints(idx1), sourcePoly.ypoints(idx1))
            Dim p2 As New Point(sourcePoly.xpoints(idx2), sourcePoly.ypoints(idx2))

            Dim q1 = FindClosestPoint(targetPoly, p1.x, p1.y)
            Dim q2 = FindClosestPoint(targetPoly, p2.x, p2.y)

            ' 计算相似变换参数（包括独立缩放因子）
            Dim transformParams As Transform = ransac.ComputeSimilarityTransform(p1, p2, q1, q2)

            ' 统计内点数量
            Dim inliers = ransac.CountInliers(transformParams)

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
            Return ransac.RefineTransform((bestTheta, bestTx, bestTy, bestScalex, bestScaley))
        End If

        Return (bestTheta, bestTx, bestTy, bestScalex, bestScaley)
    End Function

    ' 计算多边形中心点
    Private Shared Function CalculateCenter(poly As Polygon2D) As Point
        Return New Point(poly.xpoints.Average, poly.ypoints.Average)
    End Function

    ' 在目标多边形中找最近邻点（保持不变）
    Private Shared Function FindClosestPoint(
        poly As Polygon2D,
        x As Double,
        y As Double
    ) As Point

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

        Return New Point(closestX, closestY)
    End Function

    ' 计算相似变换参数（独立缩放因子）
    Private Function ComputeSimilarityTransform(p1 As Point, p2 As Point, q1 As Point, q2 As Point) As Transform
        ' 计算源向量和目标向量
        Dim vSourceX = p2.x - p1.x
        Dim vSourceY = p2.y - p1.y
        Dim vTargetX = q2.x - q1.x
        Dim vTargetY = q2.y - q1.y

        ' 计算旋转角度（修正计算方式）
        Dim theta = std.Atan2(vTargetY, vTargetX) - std.Atan2(vSourceY, vSourceX)

        ' 将角度归一化到[-π, π]
        While theta > std.PI
            theta -= 2 * std.PI
        End While
        While theta < -std.PI
            theta += 2 * std.PI
        End While

        ' 计算缩放因子（修正计算方式）
        Dim sourceLength = std.Sqrt(vSourceX * vSourceX + vSourceY * vSourceY)
        Dim targetLength = std.Sqrt(vTargetX * vTargetX + vTargetY * vTargetY)

        Dim scale = 1.0
        If sourceLength > 0.0000001 Then
            scale = targetLength / sourceLength
        End If

        ' 使用相同的缩放因子（或根据需要独立计算）
        Dim scalex = scale
        Dim scaley = scale

        ' 计算平移量（修正计算方式）
        ' 先将p1点应用旋转和缩放
        Dim cosTheta = std.Cos(theta)
        Dim sinTheta = std.Sin(theta)

        Dim transformedX = p1.x * scalex * cosTheta - p1.y * scaley * sinTheta
        Dim transformedY = p1.x * scalex * sinTheta + p1.y * scaley * cosTheta

        ' 计算平移量：目标点 - 变换后的源点
        Dim tx = q1.x - transformedX
        Dim ty = q1.y - transformedY

        Return (theta, tx, ty, scalex, scaley)
    End Function

    ' 统计内点数量（应用绕中心点的变换）
    Private Function CountInliers(t As Transform) As Integer
        Dim inliers = 0
        Dim cosTheta = std.Cos(t.theta)
        Dim sinTheta = std.Sin(t.theta)
        Dim thresholdSq = threshold * threshold

        For i As Integer = 0 To sourcePoly.length - 1
            ' 获取源点
            Dim x = sourcePoly.xpoints(i)
            Dim y = sourcePoly.ypoints(i)

            ' 应用变换：先缩放，再旋转，最后平移
            Dim xScaled = x * t.scalex
            Dim yScaled = y * t.scaley

            Dim xRotated = xScaled * cosTheta - yScaled * sinTheta
            Dim yRotated = xScaled * sinTheta + yScaled * cosTheta

            Dim xTrans = xRotated + t.tx
            Dim yTrans = yRotated + t.ty

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
    Private Function RefineTransform(init As Transform) As Transform
        ' 收集所有内点对
        Dim inlierPairs As New List(Of (sx As Double, sy As Double, tx As Double, ty As Double))
        Dim cosTheta = std.Cos(init.theta)
        Dim sinTheta = std.Sin(init.theta)
        Dim thresholdSq = threshold * threshold

        For i As Integer = 0 To sourcePoly.length - 1
            Dim sx = sourcePoly.xpoints(i)
            Dim sy = sourcePoly.ypoints(i)

            ' 应用初始变换
            Dim xScaled = sx * init.scalex
            Dim yScaled = sy * init.scaley
            Dim xRotated = xScaled * cosTheta - yScaled * sinTheta
            Dim yRotated = xScaled * sinTheta + yScaled * cosTheta
            Dim xTrans = xRotated + init.tx
            Dim yTrans = yRotated + init.ty

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
            ' 计算中心点
            Dim sourceCenterX = inlierPairs.Select(Function(p) p.sx).Average()
            Dim sourceCenterY = inlierPairs.Select(Function(p) p.sy).Average()
            Dim targetCenterX = inlierPairs.Select(Function(p) p.tx).Average()
            Dim targetCenterY = inlierPairs.Select(Function(p) p.ty).Average()

            ' 计算旋转角度和缩放因子
            Dim sumNumerator = 0.0
            Dim sumDenominator = 0.0
            Dim sumScaleX = 1.0
            Dim sumScaleY = 1.0
            Dim count = 0

            For Each pair In inlierPairs
                ' 相对于中心点的坐标
                Dim sxRel = pair.sx - sourceCenterX
                Dim syRel = pair.sy - sourceCenterY
                Dim txRel = pair.tx - targetCenterX
                Dim tyRel = pair.ty - targetCenterY

                ' 计算旋转角度的分子和分母
                sumNumerator += sxRel * tyRel - syRel * txRel
                sumDenominator += sxRel * txRel + syRel * tyRel

                ' 计算缩放因子
                If std.Abs(sxRel) > 0.0000001 Then
                    sumScaleX += std.Abs(txRel / sxRel)
                    count += 1
                End If
                If std.Abs(syRel) > 0.0000001 Then
                    sumScaleY += std.Abs(tyRel / syRel)
                    count += 1
                End If
            Next

            ' 计算旋转角度
            Dim theta = std.Atan2(sumNumerator, sumDenominator)

            ' 计算平均缩放因子
            Dim scalex = If(count > 0, sumScaleX / (count / 2), 1.0)
            Dim scaley = If(count > 0, sumScaleY / (count / 2), 1.0)

            ' 计算平移量
            cosTheta = std.Cos(theta)
            sinTheta = std.Sin(theta)

            Dim transformedCenterX = sourceCenterX * scalex * cosTheta - sourceCenterY * scaley * sinTheta
            Dim transformedCenterY = sourceCenterX * scalex * sinTheta + sourceCenterY * scaley * cosTheta

            Dim tx = targetCenterX - transformedCenterX
            Dim ty = targetCenterY - transformedCenterY

            Return (theta, tx, ty, scalex, scaley)
        End If

        Return init
    End Function
End Class
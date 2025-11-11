Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Interpolation

    ' 数学计算工具类
    Public Class MathUtils
        ' 计算两点之间的欧氏距离
        Public Shared Function EuclideanDistance(p1 As Vector2D, p2 As Vector2D) As Double
            Return std.Sqrt((p1.x - p2.x) ^ 2 + (p1.y - p2.y) ^ 2)
        End Function

        ' 计算点到线段的距离
        Public Shared Function DistanceToSegment(point As Vector2D, segStart As Vector2D, segEnd As Vector2D) As Double
            Dim l2 = EuclideanDistance(segStart, segEnd) ^ 2
            If l2 = 0 Then Return EuclideanDistance(point, segStart)

            Dim t = std.Max(0, std.Min(1, ((point.x - segStart.x) * (segEnd.x - segStart.x) +
                               (point.y - segStart.y) * (segEnd.y - segStart.y)) / l2))

            Dim projection As New Vector2D(
            segStart.x + t * (segEnd.x - segStart.x),
            segStart.y + t * (segEnd.y - segStart.y)
        )

            Return EuclideanDistance(point, projection)
        End Function

        ' 高斯核函数
        Public Shared Function GaussianKernel(distance As Double, bandwidth As Double) As Double
            Return std.Exp(-0.5 * (distance / bandwidth) ^ 2)
        End Function
    End Class

    Public Class PrincipalCurve
        Private _dataPoints As List(Of Vector2D)
        Private _curvePoints As List(Of Vector2D)
        Private _bandwidth As Double
        Private _maxIterations As Integer
        Private _tolerance As Double

        Public Sub New(dataPoints As IEnumerable(Of Vector2D),
                  Optional bandwidth As Double = 1.0,
                  Optional maxIterations As Integer = 100,
                  Optional tolerance As Double = 0.001)
            Me._dataPoints = dataPoints.ToList()
            Me._bandwidth = bandwidth
            Me._maxIterations = maxIterations
            Me._tolerance = tolerance
            Me._curvePoints = New List(Of Vector2D)()
        End Sub

        ' 获取主曲线点
        Public ReadOnly Property CurvePoints As List(Of Vector2D)
            Get
                Return _curvePoints
            End Get
        End Property

        ' 初始化曲线（使用线性主成分）
        Private Function InitializeCurve() As List(Of Vector2D)
            Dim points = _dataPoints.ToArray()
            Dim n = points.Length

            ' 计算数据中心点
            Dim meanX = points.Average(Function(p) p.x)
            Dim meanY = points.Average(Function(p) p.y)
            Dim center As New Vector2D(meanX, meanY)

            ' 计算协方差矩阵
            Dim covXX = 0.0, covXY = 0.0, covYY = 0.0
            For Each p In points
                covXX += (p.x - meanX) * (p.x - meanX)
                covXY += (p.x - meanX) * (p.y - meanY)
                covYY += (p.y - meanY) * (p.y - meanY)
            Next

            covXX /= n : covXY /= n : covYY /= n

            ' 计算主成分方向（特征向量）
            Dim trace = covXX + covYY
            Dim determinant = covXX * covYY - covXY * covXY
            Dim discriminant = std.Sqrt((trace / 2) ^ 2 - determinant)

            Dim lambda1 = trace / 2 + discriminant
            Dim lambda2 = trace / 2 - discriminant

            ' 主特征向量
            Dim eigenVectorX As Double, eigenVectorY As Double
            If std.Abs(covXY) > 0.0001 Then
                eigenVectorX = lambda1 - covYY
                eigenVectorY = covXY
            Else
                eigenVectorX = 1
                eigenVectorY = 0
            End If

            Dim magnitude = std.Sqrt(eigenVectorX ^ 2 + eigenVectorY ^ 2)
            eigenVectorX /= magnitude
            eigenVectorY /= magnitude

            ' 生成初始曲线点（沿主成分方向）
            Dim stdX = std.Sqrt(covXX)
            Dim stdY = std.Sqrt(covYY)
            Dim curveLength = 3 * std.Max(stdX, stdY) ' 3倍标准差

            Dim initialCurve = New List(Of Vector2D)()
            Dim steps = 20
            For i = 0 To steps - 1
                Dim t = (i - steps / 2) * curveLength / steps
                Dim point As New Vector2D(
                center.x + t * eigenVectorX,
                center.y + t * eigenVectorY
            )
                initialCurve.Add(point)
            Next

            Return initialCurve
        End Function

        ' 主曲线拟合算法
        Public Sub Fit()
            ' 初始化曲线
            _curvePoints = InitializeCurve()

            Dim previousError As Double = Double.MaxValue
            Dim currentError As Double = 0

            For iter = 0 To _maxIterations - 1
                ' 投影步骤：将每个数据点投影到曲线上
                Dim projections = ProjectDataToCurve()

                ' 重构步骤：根据投影重新估计曲线
                ReconstructCurve(projections)

                ' 平滑曲线
                SmoothCurve()

                ' 计算重构误差
                currentError = CalculateReconstructionError(projections)

                ' 检查收敛
                If iter > 0 AndAlso std.Abs(previousError - currentError) < _tolerance Then
                    Exit For
                End If

                previousError = currentError
            Next
        End Sub

        ' 将数据点投影到曲线上
        Private Function ProjectDataToCurve() As List(Of ProjectionInfo)
            Dim projections = New List(Of ProjectionInfo)()

            For Each dataPoint In _dataPoints
                Dim minDistance = Double.MaxValue
                Dim closestSegmentIndex = -1
                Dim closestT As Double = 0
                Dim closestPoint As Vector2D = Nothing

                ' 找到曲线上最近的点
                For i = 0 To _curvePoints.Count - 2
                    Dim segStart = _curvePoints(i)
                    Dim segEnd = _curvePoints(i + 1)

                    ' 计算点到线段的距离和投影参数
                    Dim segmentVector = segEnd - segStart
                    Dim segmentLength = segmentVector.Length

                    If segmentLength = 0 Then Continue For

                    Dim toPoint = dataPoint - segStart
                    Dim segmentDirection = segmentVector.Normalize()

                    Dim t = (toPoint.X * segmentDirection.X + toPoint.Y * segmentDirection.Y) / segmentLength
                    t = std.Max(0, std.Min(1, t)) ' 限制在[0,1]范围内

                    Dim projectionPoint = segStart + t * segmentVector
                    Dim distance = dataPoint.DistanceTo(projectionPoint)

                    If distance < minDistance Then
                        minDistance = distance
                        closestSegmentIndex = i
                        closestT = t
                        closestPoint = projectionPoint
                    End If
                Next

                If closestSegmentIndex >= 0 Then
                    projections.Add(New ProjectionInfo(
                    dataPoint, closestPoint, closestSegmentIndex + closestT, minDistance))
                End If
            Next

            Return projections
        End Function

        ' 根据投影重新构建曲线
        Private Sub ReconstructCurve(projections As List(Of ProjectionInfo))
            If projections.Count = 0 Then Return

            ' 按投影位置排序
            Dim sortedProjections = projections.OrderBy(Function(p) p.CurveParameter).ToList()

            ' 为每个曲线点计算加权平均
            For i = 0 To _curvePoints.Count - 1
                Dim curveParam = i ' 曲线参数化位置

                Dim totalWeight = 0.0
                Dim weightedSumX = 0.0
                Dim weightedSumY = 0.0

                For Each proj In sortedProjections
                    ' 使用高斯核计算权重（基于参数距离）
                    Dim paramDistance = std.Abs(proj.CurveParameter - curveParam)
                    Dim weight = MathUtils.GaussianKernel(paramDistance, _bandwidth)

                    ' 也考虑数据点到当前曲线点的距离
                    Dim spatialDistance = _curvePoints(i).DistanceTo(proj.DataPoint)
                    Dim spatialWeight = MathUtils.GaussianKernel(spatialDistance, _bandwidth * 2)

                    Dim totalWeightHere = weight * spatialWeight

                    totalWeight += totalWeightHere
                    weightedSumX += totalWeightHere * proj.DataPoint.X
                    weightedSumY += totalWeightHere * proj.DataPoint.Y
                Next

                If totalWeight > 0 Then
                    _curvePoints(i) = New Vector2D(weightedSumX / totalWeight, weightedSumY / totalWeight)
                End If
            Next
        End Sub

        ' 平滑曲线（防止过拟合）
        Private Sub SmoothCurve()
            If _curvePoints.Count < 3 Then Return

            Dim smoothedPoints = New List(Of Vector2D)()
            smoothedPoints.Add(_curvePoints(0)) ' 保持起点不变

            For i = 1 To _curvePoints.Count - 2
                ' 简单移动平均平滑
                Dim smoothedX = (_curvePoints(i - 1).X + _curvePoints(i).X + _curvePoints(i + 1).X) / 3
                Dim smoothedY = (_curvePoints(i - 1).Y + _curvePoints(i).Y + _curvePoints(i + 1).Y) / 3
                smoothedPoints.Add(New Vector2D(smoothedX, smoothedY))
            Next

            smoothedPoints.Add(_curvePoints(_curvePoints.Count - 1)) ' 保持终点不变
            _curvePoints = smoothedPoints
        End Sub

        ' 计算重构误差
        Private Function CalculateReconstructionError(projections As List(Of ProjectionInfo)) As Double
            If projections.Count = 0 Then Return 0
            Return projections.Average(Function(p) p.Distance)
        End Function

        ' 投影信息辅助类
        Private Class ProjectionInfo
            Public Property DataPoint As Vector2D
            Public Property ProjectionPoint As Vector2D
            Public Property CurveParameter As Double
            Public Property Distance As Double

            Public Sub New(dataPoint As Vector2D, projectionPoint As Vector2D,
                      curveParameter As Double, distance As Double)
                Me.DataPoint = dataPoint
                Me.ProjectionPoint = projectionPoint
                Me.CurveParameter = curveParameter
                Me.Distance = distance
            End Sub
        End Class
    End Class
End Namespace
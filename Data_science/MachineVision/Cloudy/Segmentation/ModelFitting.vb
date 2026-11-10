Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' 一次基元检测的结果。
    ''' </summary>
    Public Class PrimitiveDetection

        ''' <summary>拟合出的基元。</summary>
        Public Property Model As PrimitiveModel

        ''' <summary>内点下标（指向输入点云）。</summary>
        Public Property InlierIndices As Integer()

        ''' <summary>内点率（内点数 / 参与检测的点数）。</summary>
        Public Property Fitness As Double

        ''' <summary>RANSAC 迭代次数。</summary>
        Public Property Iterations As Integer

        ''' <summary>使用的内点距离阈值。</summary>
        Public Property InlierDistance As Double

        Public ReadOnly Property InlierCount As Integer
            Get
                Return If(InlierIndices Is Nothing, 0, InlierIndices.Length)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Model?.Type}: {InlierCount} inliers ({Fitness:P1}), d={InlierDistance:G4}"
        End Function
    End Class

    ''' <summary>
    ''' 通用 RANSAC 基元拟合 + 顺序多模型分解。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>RANSAC 的三个要素</b>（本类统一处理）：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>最小样本拟合：由 <see cref="PrimitiveModel"/> 的子类提供
    ''' （平面 3 点、球 4 点、圆柱/圆锥需要法向，采样 6~20 点）。</description></item>
    ''' <item><description>内点判据：点到模型距离 ≤ 阈值。</description></item>
    ''' <item><description>评分：内点数量（几何一致性），必要时可用内点残差平方和做 tie-break。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>顺序多模型分解</b>（<see cref="ExtractAll(PointCloudBuffer, PrimitiveType(), RansacOptions, CloudyOptions)"/>）：
    ''' 反复“找出最显著的基元 → 移除其内点 → 继续”，直到剩余点比例过低。
    ''' 这是工业上提取“台面 + 孔位 + 轴类”的标准做法，
    ''' 但要注意<b>贪心的顺序</b>：大平面往往先被提取，
    ''' 如果关心小特征（例如倒角），应先用裁剪/聚类缩小范围再拟合。
    ''' </para>
    ''' <para>
    ''' <b>确定性</b>：随机采样使用显式种子，相同输入结果一致。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(iter · sample · n_score)</c>；评分阶段按
    ''' <see cref="RansacOptions.ScoreSampleSize"/> 抽样，避免每次假设都扫描全点云。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ModelFitting

        Private Sub New()
        End Sub

        ''' <summary>RANSAC 参数。</summary>
        Public Class RansacOptions

            ''' <summary>最大迭代次数（默认 800）。</summary>
            Public Property MaxIterations As Integer = 800

            ''' <summary>内点距离阈值；<c>≤ 0</c> 表示按点距自适应（2 倍）。</summary>
            Public Property InlierDistance As Double = 0.0

            ''' <summary>接受一个基元所需的最小内点数（默认 50）。</summary>
            Public Property MinInliers As Integer = 50

            ''' <summary>评分阶段最多使用多少个点（默认 4000）。</summary>
            Public Property ScoreSampleSize As Integer = 4000

            ''' <summary>顺序分解的最多基元数（默认 8）。</summary>
            Public Property MaxModels As Integer = 8

            ''' <summary>顺序分解的终止条件：剩余点比例低于该值即停止（默认 0.1）。</summary>
            Public Property RemainingRatio As Double = 0.1

            ''' <summary>圆柱/圆锥假设的采样点数（默认 24；越多越稳但越慢）。</summary>
            Public Property AxisModelSamples As Integer = 24

            ''' <summary>随机种子（<c>0</c> 表示默认种子）。</summary>
            Public Property Seed As Integer = 0
        End Class

        ''' <summary>
        ''' 在点云中检测单一类型的基元。
        ''' </summary>
        Public Shared Function Detect(cloud As PointCloudBuffer, type As PrimitiveType,
                                      Optional options As RansacOptions = Nothing,
                                      Optional optionsCloudy As CloudyOptions = Nothing) As PrimitiveDetection
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As RansacOptions = If(options, New RansacOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim indices(cloud.Count - 1) As Integer

            For i As Integer = 0 To indices.Length - 1
                indices(i) = i
            Next

            Return DetectOn(cloud, indices, type, opt, global_)
        End Function

        ''' <summary>
        ''' 在“子集”上检测基元（内部实现，供顺序分解复用）。
        ''' </summary>
        Private Shared Function DetectOn(cloud As PointCloudBuffer, active As Integer(), type As PrimitiveType,
                                         opt As RansacOptions, global_ As CloudyOptions) As PrimitiveDetection
            Dim n As Integer = active.Length

            If n < 8 Then Return Nothing

            Dim spacing As Double = cloud.EstimateAverageSpacing()
            Dim inlierDistance As Double = If(opt.InlierDistance > 0.0, opt.InlierDistance,
                                              std.Max(spacing * 2.0, 1.0E-6))

            Dim needsNormals As Boolean = type = PrimitiveType.Cylinder OrElse type = PrimitiveType.Cone
            Dim source As PointCloudBuffer = cloud

            If needsNormals AndAlso Not cloud.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)
            End If

            Dim rnd As Random = If(opt.Seed = 0, global_.CreateRandom(CInt(type) * 977), New Random(opt.Seed + CInt(type)))

            ' 评分用的采样下标（最多 ScoreSampleSize 个）
            Dim scoreCount As Integer = std.Min(n, std.Max(16, opt.ScoreSampleSize))
            Dim scoreIndices(scoreCount - 1) As Integer
            Dim stride As Double = CDbl(n) / scoreCount

            For i As Integer = 0 To scoreCount - 1
                scoreIndices(i) = active(std.Min(n - 1, CInt(std.Floor(i * stride))))
            Next

            Dim bestModel As PrimitiveModel = Nothing
            Dim bestScore As Integer = -1
            Dim bestResidual As Double = Double.MaxValue

            Dim sampleSize As Integer = SampleSizeFor(type, opt)

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                Dim sample(sampleSize - 1) As Vector3d
                Dim sampleNormals(sampleSize - 1) As Vector3d

                For k As Integer = 0 To sampleSize - 1
                    Dim pick As Integer = active(rnd.Next(n))

                    sample(k) = source.Item(pick)

                    If needsNormals Then sampleNormals(k) = source.Normal(pick)
                Next

                Dim model As PrimitiveModel

                Try
                    model = FitModel(type, sample, sampleNormals)
                Catch ex As ArgumentException
                    Continue For
                End Try

                If model Is Nothing Then Continue For

                Dim inliers As Integer = 0
                Dim residual As Double = 0.0

                For k As Integer = 0 To scoreCount - 1
                    Dim d As Double = model.DistanceTo(source.Item(scoreIndices(k)))

                    If d <= inlierDistance Then
                        inliers += 1
                        residual += d * d
                    End If
                Next

                If inliers > bestScore OrElse (inliers = bestScore AndAlso residual < bestResidual) Then
                    bestScore = inliers
                    bestResidual = residual
                    bestModel = model
                End If
            Next

            If bestModel Is Nothing Then Return Nothing

            ' 用全部内点做一次重拟合
            Dim inlierList As New List(Of Integer)()

            For k As Integer = 0 To n - 1
                If bestModel.DistanceTo(source.Item(active(k))) <= inlierDistance Then
                    Call inlierList.Add(active(k))
                End If
            Next

            If inlierList.Count < std.Max(3, opt.MinInliers) Then Return Nothing

            Dim refined As PrimitiveModel = Refit(type, source, inlierList, bestModel)

            If refined IsNot Nothing Then
                Dim refinedInliers As New List(Of Integer)()

                For k As Integer = 0 To n - 1
                    If refined.DistanceTo(source.Item(active(k))) <= inlierDistance Then
                        Call refinedInliers.Add(active(k))
                    End If
                Next

                If refinedInliers.Count >= inlierList.Count Then
                    bestModel = refined
                    inlierList = refinedInliers
                End If
            End If

            Return New PrimitiveDetection With {
                .Model = bestModel,
                .InlierIndices = inlierList.ToArray(),
                .Fitness = CDbl(inlierList.Count) / n,
                .Iterations = opt.MaxIterations,
                .InlierDistance = inlierDistance
            }
        End Function

        ''' <summary>
        ''' 顺序提取多种基元（提取 → 移除 → 继续）。
        ''' </summary>
        Public Shared Function ExtractAll(cloud As PointCloudBuffer, types As PrimitiveType(),
                                          Optional options As RansacOptions = Nothing,
                                          Optional optionsCloudy As CloudyOptions = Nothing) As List(Of PrimitiveDetection)
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If types Is Nothing OrElse types.Length = 0 Then
                Throw New ArgumentException("at least one primitive type is required!", NameOf(types))
            End If

            Dim opt As RansacOptions = If(options, New RansacOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            Dim active As New List(Of Integer)(cloud.Count)

            For i As Integer = 0 To cloud.Count - 1
                Call active.Add(i)
            Next

            Dim results As New List(Of PrimitiveDetection)()
            Dim threshold As Integer = CInt(std.Floor(cloud.Count * std.Max(0.0, opt.RemainingRatio)))

            For modelIndex As Integer = 1 To std.Max(1, opt.MaxModels)
                If active.Count <= std.Max(threshold, opt.MinInliers) Then Exit For

                Dim best As PrimitiveDetection = Nothing

                For Each type As PrimitiveType In types
                    Dim detection As PrimitiveDetection = DetectOn(cloud, active.ToArray(), type, opt, global_)

                    If detection Is Nothing Then Continue For
                    If best Is Nothing OrElse detection.InlierCount > best.InlierCount Then best = detection
                Next

                If best Is Nothing Then Exit For

                Call results.Add(best)

                ' 从活动集合中移除已提取的内点
                Dim removed As New HashSet(Of Integer)(best.InlierIndices)

                Call active.RemoveAll(Function(index) removed.Contains(index))
            Next

            Return results
        End Function

        Private Shared Function SampleSizeFor(type As PrimitiveType, opt As RansacOptions) As Integer
            Select Case type
                Case PrimitiveType.Plane
                    Return 3
                Case PrimitiveType.Sphere
                    Return 4
                Case Else
                    Return std.Max(8, opt.AxisModelSamples)
            End Select
        End Function

        Private Shared Function FitModel(type As PrimitiveType, sample As Vector3d(),
                                         sampleNormals As Vector3d()) As PrimitiveModel
            Select Case type
                Case PrimitiveType.Plane
                    Return PlaneModel.FromPoints(sample(0), sample(1), sample(2))
                Case PrimitiveType.Sphere
                    Return SphereModel.Fit(sample)
                Case PrimitiveType.Cylinder
                    Return CylinderModel.Fit(sample, sampleNormals)
                Case PrimitiveType.Cone
                    Return ConeModel.Fit(sample, sampleNormals)
            End Select

            Return Nothing
        End Function

        Private Shared Function Refit(type As PrimitiveType, cloud As PointCloudBuffer,
                                      inliers As List(Of Integer), fallback As PrimitiveModel) As PrimitiveModel
            Dim points(inliers.Count - 1) As Vector3d
            Dim normals(inliers.Count - 1) As Vector3d

            For i As Integer = 0 To inliers.Count - 1
                points(i) = cloud.Item(inliers(i))

                If cloud.HasNormals Then normals(i) = cloud.Normal(inliers(i))
            Next

            Try
                Select Case type
                    Case PrimitiveType.Plane
                        Return PlaneModel.Fit(points)
                    Case PrimitiveType.Sphere
                        Return SphereModel.Fit(points)
                    Case PrimitiveType.Cylinder
                        If cloud.HasNormals Then Return CylinderModel.Fit(points, normals)
                    Case PrimitiveType.Cone
                        If cloud.HasNormals Then Return ConeModel.Fit(points, normals)
                End Select
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is InvalidOperationException
                ' 退化（共线/共面/法向不足）时保留 RANSAC 的原始模型
            End Try

            Return fallback
        End Function

    End Class
End Namespace

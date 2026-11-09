Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 统计滤波(SOR)：按“k 近邻平均距离”的全局分布剔除离群点。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>判据</b>：对每个点求它到最近 <c>k</c> 个邻点的平均距离 <c>d(i)</c>，
    ''' 计算全部 <c>d(i)</c> 的均值 <c>μ</c> 与标准差 <c>σ</c>，
    ''' 保留 <c>d(i) ≤ μ + α·σ</c> 的点（<c>α</c> 默认 1.0，工业上常用 1.0~3.0）。
    ''' </para>
    ''' <para>
    ''' <b>为什么用“平均距离”而不是“到物体中心的距离”</b>：真实扫描里点云是曲面薄壳，
    ''' 离群点（飞点、反光杂点）出现在空间任何位置，离中心可能很近；
    ''' 只有局部密度才能区分“孤立的少量点”与“合法的稀疏表面”。
    ''' </para>
    ''' <para>
    ''' <b>阈值语义</b>：<c>α</c> 越大越保守（保留更多点）。它假设 <c>d(i)</c> 近似高斯分布——
    ''' 对均匀表面的扫描数据这个假设通常成立；对密度变化剧烈的场景（近处密、远处疏），
    ''' 建议先用体素滤波统一密度，或改用半径滤波（<see cref="RadiusOutlierRemoval"/>）。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·k·log n)</c>（KD 树 kNN），归约部分走 SIMD。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class StatisticalOutlierRemoval

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 统计滤波结果。
        ''' </summary>
        Public Class SorResult

            ''' <summary>过滤后的点云。</summary>
            Public Property Cloud As PointCloudBuffer

            ''' <summary>保留掩码（<c>True</c> 表示保留）。</summary>
            Public Property Mask As Boolean()

            ''' <summary>逐点的 k 近邻平均距离。</summary>
            Public Property MeanDistances As Double()

            ''' <summary>平均距离的全局均值。</summary>
            Public Property GlobalMean As Double

            ''' <summary>平均距离的全局标准差。</summary>
            Public Property GlobalStdDev As Double

            ''' <summary>实际使用的阈值。</summary>
            Public Property Threshold As Double

            ''' <summary>被剔除的点数。</summary>
            Public ReadOnly Property RemovedCount As Integer
                Get
                    If Mask Is Nothing Then Return 0

                    Dim removed As Integer = 0

                    For i As Integer = 0 To Mask.Length - 1
                        If Not Mask(i) Then removed += 1
                    Next

                    Return removed
                End Get
            End Property

            ''' <summary>被剔除的比例。</summary>
            Public ReadOnly Property RemovedRatio As Double
                Get
                    If Mask Is Nothing OrElse Mask.Length = 0 Then Return 0.0

                    Return CDbl(RemovedCount) / Mask.Length
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return $"SOR: μ={GlobalMean:G4}, σ={GlobalStdDev:G4}, threshold={Threshold:G4}, " &
                    $"removed={RemovedCount} ({RemovedRatio:P2})"
            End Function
        End Class

        ''' <summary>
        ''' 计算逐点的 k 近邻平均距离（不含自身）。
        ''' </summary>
        ''' <param name="cloud">点云</param>
        ''' <param name="k">近邻个数（默认 20）</param>
        ''' <param name="options">并行选项</param>
        ''' <param name="tree">可复用的 KD 树（<c>Nothing</c> 时内部构建）</param>
        Public Shared Function ComputeMeanDistances(cloud As PointCloudBuffer, Optional k As Integer = 20,
                                                    Optional options As CloudyOptions = Nothing,
                                                    Optional tree As KdTree3D = Nothing) As Double()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then Return Array.Empty(Of Double)()
            If k < 1 Then Throw New ArgumentOutOfRangeException(NameOf(k), "the neighbor count should be at least 1!")

            Dim index As KdTree3D = If(tree, New KdTree3D(cloud))
            Dim dists As Double() = GC.AllocateUninitializedArray(Of Double)(n)
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(index))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()

                        For i As Integer = start To ends - 1
                            Call index.Knn(cloud.X(i), cloud.Y(i), cloud.Z(i), k, buffer, i)
                            dists(i) = buffer.MeanDistance()
                        Next
                    End Sub, 512)
            Finally
                Call pool.Dispose()
            End Try

            Return dists
        End Function

        ''' <summary>
        ''' 执行统计滤波。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="k">近邻个数（默认 20：足够稳定，又不会把局部细节抹平）</param>
        ''' <param name="stdRatio">标准差倍数阈值 α（默认 1.0）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Apply(cloud As PointCloudBuffer, Optional k As Integer = 20,
                                     Optional stdRatio As Double = 1.0,
                                     Optional options As CloudyOptions = Nothing) As SorResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If stdRatio <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(stdRatio), "the std ratio should be positive!")

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then
                Return New SorResult With {
                    .Cloud = cloud.Clone(),
                    .Mask = Array.Empty(Of Boolean)(),
                    .MeanDistances = Array.Empty(Of Double)(),
                    .GlobalMean = 0.0,
                    .GlobalStdDev = 0.0,
                    .Threshold = 0.0
                }
            End If

            If n <= k Then
                ' 点太少，邻域统计没有意义：原样返回，避免把所有点都判成离群
                Dim allKept(n - 1) As Boolean

                For i As Integer = 0 To n - 1
                    allKept(i) = True
                Next

                Return New SorResult With {
                    .Cloud = cloud.Clone(),
                    .Mask = allKept,
                    .MeanDistances = New Double(n - 1) {},
                    .GlobalMean = 0.0,
                    .GlobalStdDev = 0.0,
                    .Threshold = Double.MaxValue
                }
            End If

            Dim dists As Double() = ComputeMeanDistances(cloud, k, opt)

            Dim mean As Double = SimdReduce.Mean(dists)
            Dim variance As Double = SimdReduce.SumSquares(dists) / n - mean * mean

            If variance < 0.0 Then variance = 0.0

            Dim sigma As Double = std.Sqrt(variance)
            Dim threshold As Double = mean + stdRatio * sigma
            Dim mask As Boolean() = New Boolean(n - 1) {}

            For i As Integer = 0 To n - 1
                mask(i) = dists(i) <= threshold
            Next

            Return New SorResult With {
                .Cloud = cloud.Where(mask),
                .Mask = mask,
                .MeanDistances = dists,
                .GlobalMean = mean,
                .GlobalStdDev = sigma,
                .Threshold = threshold
            }
        End Function

    End Class
End Namespace

Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 法向与表面变化度估计：局部邻域协方差矩阵的闭式特征分解（PCA）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>数学原理</b>：对点 <c>p</c> 的邻域求协方差矩阵
    ''' <c>C = (1/n)·Σ (q − c)(q − c)ᵀ</c>（<c>c</c> 为邻域质心），
    ''' 它的三个特征值 <c>λ1 ≥ λ2 ≥ λ3 ≥ 0</c> 描述邻域在三个主方向上的分散程度。
    ''' 最小特征值对应的特征向量就是拟合平面的法向（邻域在那个方向上最“薄”）。
    ''' </para>
    ''' <para>
    ''' <b>曲率</b>：<c>λ3 / (λ1+λ2+λ3)</c>（表面变化度）。平面邻域 <c>λ3≈0</c> 得 0；
    ''' 各向同性噪声得 1/3。它对裂纹、凹陷、毛刺非常敏感，
    ''' 是工业缺陷检测里最常用的标量特征之一。
    ''' </para>
    ''' <para>
    ''' <b>邻域半径 vs kNN</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>
    ''' <b>kNN</b>（默认 k=15）保证每个点都有固定数量的样本，协方差估计的统计稳定性一致，
    ''' 但在密度变化剧烈处会导致“邻域半径随密度变化”，平滑度不一致。
    ''' </description></item>
    ''' <item><description>
    ''' <b>固定半径</b>保证空间尺度一致（曲率/法向在物理上有可比性），
    ''' 但稀疏处可能邻点不足。工业检测（形位公差、特征尺寸）应优先用固定半径。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>性能</b>：邻域下标是随机访问，先用
    ''' <see cref="SimdIndexed.Gather3(Double(), Double(), Double(), Integer(), Integer, Integer, Double(), Double(), Double())"/>
    ''' 把 k 个邻域点压实到连续缓冲，再交给
    ''' <see cref="SimdVector3.Covariance3(Double(), Double(), Double(), Integer, Integer)"/> 做整块累加。
    ''' 这样协方差计算是向量化的，而 gather 只发生一次。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NormalEstimation

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 并行用的线程本地临时缓冲（避免逐点分配）。
        ''' </summary>
        Private Class Scratch
            Public X As Double() = New Double(63) {}
            Public Y As Double() = New Double(63) {}
            Public Z As Double() = New Double(63) {}

            Public Sub EnsureCapacity(n As Integer)
                If X.Length >= n Then Return

                Dim size As Integer = std.Max(n, X.Length * 2)

                Array.Resize(X, size)
                Array.Resize(Y, size)
                Array.Resize(Z, size)
            End Sub
        End Class

        ''' <summary>
        ''' 估计法向与曲率（不做法向定向）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="k">kNN 的近邻个数（<paramref name="radius"/> 大于 0 时作为邻域点数上限）</param>
        ''' <param name="radius">邻域半径；<c>≤ 0</c> 表示使用 kNN</param>
        ''' <param name="options">并行选项</param>
        ''' <returns>带法向与 <see cref="PointCloudBuffer.Curvature"/> 的新点云</returns>
        Public Shared Function Estimate(cloud As PointCloudBuffer, Optional k As Integer = 15,
                                        Optional radius As Double = 0.0,
                                        Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            Return EstimateCore(cloud, k, radius, Nothing, options)
        End Function

        ''' <summary>
        ''' 估计法向与曲率，并把法向统一朝向视点。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="k">kNN 近邻个数</param>
        ''' <param name="radius">邻域半径（<c>≤ 0</c> 表示 kNN）</param>
        ''' <param name="viewpoint">视点（扫描仪位置）；为 <c>Nothing</c> 时不定向</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Estimate(cloud As PointCloudBuffer, k As Integer, radius As Double,
                                        viewpoint As Vector3d,
                                        Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            Return EstimateCore(cloud, k, radius, New Vector3d?(viewpoint), options)
        End Function

        Private Shared Function EstimateCore(cloud As PointCloudBuffer, k As Integer, radius As Double,
                                             viewpoint As Vector3d?, options As CloudyOptions) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then Return cloud.Clone()

            Dim kk As Integer = std.Max(3, k)
            Dim useRadius As Boolean = radius > 0.0

            Dim tree As New KdTree3D(cloud)
            Dim out As PointCloudBuffer = cloud.Clone()

            Call out.EnsureNormals()

            Dim nx As Double() = out.Nx
            Dim ny As Double() = out.Ny
            Dim nz As Double() = out.Nz
            Dim curvature As Double() = GC.AllocateUninitializedArray(Of Double)(n)

            Dim pool As New ThreadLocal(Of Scratch)(Function() New Scratch())
            Dim query As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree, kk + 1))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim scratch As Scratch = pool.Value
                        Dim neighbors As NeighborhoodQuery = query.Value
                        Dim buffer As Neighborhood = neighbors.Rent()

                        For i As Integer = start To ends - 1
                            Dim qx As Double = cloud.X(i)
                            Dim qy As Double = cloud.Y(i)
                            Dim qz As Double = cloud.Z(i)

                            If useRadius Then
                                Call tree.RadiusSearch(qx, qy, qz, radius, buffer, i, kk * 4)
                            Else
                                Call tree.Knn(qx, qy, qz, kk, buffer, i)
                            End If

                            Dim count As Integer = buffer.Count

                            If count < 3 Then
                                ' 邻域退化（孤立点）：给出稳定的兜底法向，避免下游出现零向量
                                nx(i) = 0.0
                                ny(i) = 0.0
                                nz(i) = 1.0
                                curvature(i) = 0.0

                                Continue For
                            End If

                            Call scratch.EnsureCapacity(count)
                            Call SimdIndexed.Gather3(cloud.X, cloud.Y, cloud.Z, buffer.Indices, 0, count,
                                                     scratch.X, scratch.Y, scratch.Z)

                            Dim moments As Vector3Moments = SimdVector3.Covariance3(
                                scratch.X, scratch.Y, scratch.Z, 0, count)
                            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                                moments.Cxx, moments.Cxy, moments.Cxz, moments.Cyy, moments.Cyz, moments.Czz)

                            Dim normal As Vector3d = eigen.V3

                            If viewpoint.HasValue Then
                                Dim toViewer As Vector3d = viewpoint.Value - New Vector3d(qx, qy, qz)

                                If Vector3d.Dot(normal, toViewer) < 0.0 Then normal = -normal
                            End If

                            nx(i) = normal.X
                            ny(i) = normal.Y
                            nz(i) = normal.Z
                            curvature(i) = eigen.SurfaceVariation
                        Next
                    End Sub, 256)
            Finally
                Call pool.Dispose()
                Call query.Dispose()
            End Try

            Call out.SetNormals(nx, ny, nz)

            out.Curvature = curvature

            Return out
        End Function

        ''' <summary>
        ''' 单独计算表面变化度（曲率），不写回法向。
        ''' </summary>
        ''' <remarks>
        ''' 用于“只需要曲率”的场景（区域生长的种子排序、曲率自适应采样）。
        ''' 内部复用 <see cref="Estimate(PointCloudBuffer, Integer, Double, CloudyOptions)"/>，
        ''' 因此如果同一份点云还需要法向，直接调用 <c>Estimate</c> 更划算。
        ''' </remarks>
        Public Shared Function EstimateCurvature(cloud As PointCloudBuffer, Optional k As Integer = 15,
                                                 Optional radius As Double = 0.0,
                                                 Optional options As CloudyOptions = Nothing) As Double()
            Dim estimated As PointCloudBuffer = Estimate(cloud, k, radius, options)

            Return estimated.Curvature
        End Function

    End Class
End Namespace

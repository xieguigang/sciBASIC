' ---------------------------------------------------------------------------
' 3D 高斯溅射渲染门面
'
' 渲染流程（与 3DGS 参考实现一致）：
'   1. SplatProjection：把每个高斯投影到屏幕空间（相机变换 / 协方差 / conic）
'   2. SplatRasterizer：可见性剔除 → 深度升序排序 → tile 分箱
'   3. SplatBackends.Default：按 tile 做 front-to-back 的 alpha 合成
'        C(pixel) = Σ_i c_i·α_i·Π_{j<i}(1−α_j)
'      α_i = o_i·exp(−½·dᵀ·Σ'⁻¹·d)，越近的高斯先合成（near → far）。
'
' 与旧实现的差异（均已由调用方确认）：
'   * 深度排序由 far → near 修正为 near → far，与 front-to-back 合成公式一致；
'   * 3D 协方差补上世界→相机的旋转（Σ_cam = R_cam·Σ_world·R_camᵀ）。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

''' <summary>
''' 3D 高斯溅射渲染器。
''' </summary>
Public Class SplatRenderer

    ''' <summary>渲染时考虑的半径（标准差倍数）</summary>
    Public Const SPLAT_RADIUS_SIGMA As Double = 3.0

    ''' <summary>默认渲染参数</summary>
    Public Shared ReadOnly Property [Default] As SplatRenderOptions
        Get
            Return SplatRenderOptions.Default
        End Get
    End Property

    ''' <summary>
    ''' 渲染单个视图。
    ''' </summary>
    ''' <param name="model">高斯模型</param>
    ''' <param name="camera">相机</param>
    ''' <returns>渲染的图像 [H, W, 3]，值范围 [0, 1]</returns>
    Public Shared Function Render(model As GaussianModel, camera As Camera) As Tensor
        Return Render(model, camera, SplatRenderOptions.Default)
    End Function

    ''' <summary>
    ''' 渲染单个视图（显式参数）。
    ''' </summary>
    Public Shared Function Render(model As GaussianModel, camera As Camera,
                                  options As SplatRenderOptions) As Tensor
        Dim request = SplatRasterizer.Build(model, camera, options)
        Dim image As Double() = Nothing
        Dim transmittance As Double() = Nothing

        Call Rasterize(request, image, transmittance)

        Return Tensor.Wrap(image, request.Height, request.Width, 3)
    End Function

    ''' <summary>
    ''' 渲染单个视图并输出光栅化请求与透射率（供反向传播/调试复用）。
    ''' </summary>
    Public Shared Function RenderWithRequest(model As GaussianModel, camera As Camera,
                                             options As SplatRenderOptions,
                                             ByRef request As SplatRasterRequest,
                                             ByRef transmittance As Double()) As Tensor
        request = SplatRasterizer.Build(model, camera, options)

        Dim image As Double() = Nothing
        Call Rasterize(request, image, transmittance)

        Return Tensor.Wrap(image, request.Height, request.Width, 3)
    End Function

    ''' <summary>
    ''' 渲染多个视图（视图级并行）。
    ''' </summary>
    Public Shared Function RenderAll(model As GaussianModel, cameras As List(Of Camera),
                                     Optional options As SplatRenderOptions = Nothing) As Tensor()
        If options Is Nothing Then options = SplatRenderOptions.Default

        Dim result(cameras.Count - 1) As Tensor

        System.Threading.Tasks.Parallel.For(0, cameras.Count, options.ParallelOptions(),
            Sub(v)
                result(v) = Render(model, cameras(v), options)
            End Sub)

        Return result
    End Function

    ''' <summary>分配图像与透射率缓冲并调用默认后端完成光栅化。</summary>
    Friend Shared Sub Rasterize(request As SplatRasterRequest,
                                ByRef image As Double(), ByRef transmittance As Double())
        Dim pixels = request.Width * request.Height

        image = New Double(pixels * 3 - 1) {}
        transmittance = New Double(pixels - 1) {}

        Dim backend = SplatBackends.Default

        If backend Is Nothing OrElse Not backend.Rasterize(request, image, transmittance) Then
            Call SplatBackends.Reset()
            Call SplatBackends.Default.Rasterize(request, image, transmittance)
        End If
    End Sub

    ''' <summary>
    ''' 计算 MSE 损失（渲染图像 vs 目标图像）。
    ''' </summary>
    ''' <remarks>
    ''' 纯逐元素归约：元素数达到阈值且宿主注册了 GPU 张量后端时交给张量后端
    ''' （fp32 链，一次性调度），否则走 Core 的 SIMD 归约（AVX2/FMA）。
    ''' </remarks>
    Public Shared Function ComputeMSE(rendered As Tensor, target As Tensor) As Double
        Dim n = rendered.Data.Length
        If n = 0 Then Return 0

        Dim options = SplatRenderOptions.Default

        If options.UseTensorLossBackend AndAlso IsGpuTensorBackend() AndAlso
           n >= Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute.MixedPrecisionPolicy.ElementwiseThreshold Then

            Dim a = rendered.ToTensorF()
            Dim b = target.ToTensorF()

            Return CDbl(TensorF.computeKernelF.MseLoss(a, b).Data(0))
        End If

        Return SimdReduce.DistanceSquared(rendered.Data, target.Data) / n
    End Function

    ''' <summary>
    ''' 一次遍历同时算出 MSE 与损失对渲染图像的梯度 <c>dL/dC = 2(C − C_gt)/n</c>。
    ''' </summary>
    ''' <param name="rendered">渲染图像</param>
    ''' <param name="target">目标图像</param>
    ''' <param name="scale">梯度缩放因子（多视图平均时传 1/视图数）</param>
    Public Shared Function MseAndGradient(rendered As Tensor, target As Tensor,
                                          Optional scale As Double = 1.0) As (Loss As Double, Gradient As Double())
        Dim r = rendered.Data
        Dim t = target.Data
        Dim n = r.Length

        If n = 0 Then Return (0.0, New Double() {})

        Dim g(n - 1) As Double
        Dim factor = 2.0 * scale / n
        Dim chunks = std.Min(std.Max(1, Environment.ProcessorCount), std.Max(1, n \ 16384))
        Dim chunkSums(chunks - 1) As Double
        Dim perChunk = (n + chunks - 1) \ chunks

        System.Threading.Tasks.Parallel.For(0, chunks,
            Sub(c)
                Dim from = c * perChunk
                Dim [to] = std.Min(n, from + perChunk) - 1
                Dim sum = 0.0

                For i = from To [to]
                    Dim d = r(i) - t(i)
                    g(i) = d * factor
                    sum += d * d
                Next

                chunkSums(c) = sum
            End Sub)

        Dim total = 0.0

        For c = 0 To chunks - 1
            total += chunkSums(c)
        Next

        Return (total / n, g)
    End Function

    ''' <summary>
    ''' 计算 PSNR（峰值信噪比）。
    ''' </summary>
    Public Shared Function ComputePSNR(rendered As Tensor, target As Tensor) As Double
        Return ComputePSNR(ComputeMSE(rendered, target))
    End Function

    ''' <summary>由已知 MSE 计算 PSNR。</summary>
    Public Shared Function ComputePSNR(mse As Double) As Double
        If mse < 0.000000000001 Then Return 100.0
        Return 10.0 * std.Log10(1.0 / mse)
    End Function

    ''' <summary>当前 fp32 张量后端是否为 GPU 实现。</summary>
    Public Shared Function IsGpuTensorBackend() As Boolean
        Dim name = TensorF.computeKernelF?.Name

        If name Is Nothing Then Return False

        Return name.IndexOf("CUDA", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    ''' <summary>当前 fp64 张量后端是否为 GPU 实现。</summary>
    Public Shared Function IsGpuDoubleBackend() As Boolean
        Dim name = Tensor.computeKernel?.Name

        If name Is Nothing Then Return False

        Return name.IndexOf("CUDA", StringComparison.OrdinalIgnoreCase) >= 0
    End Function

End Class

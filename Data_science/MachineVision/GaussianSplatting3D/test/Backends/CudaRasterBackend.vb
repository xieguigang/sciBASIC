' ---------------------------------------------------------------------------
' 宿主侧 CUDA 光栅化后端（扩展点示例）
'
' 这个类演示"如何把库内光栅化换成 GPU 实现"，并把契约写清楚。库本身不引用
' ILCudaTensor —— 后端只在宿主工程里实现，通过 SplatBackends.Register 注入。
'
' 实现一个真正的 CUDA 光栅化器需要三段内核（本文件的 Commit/Rasterize 结构
' 就是按这三段划分的）：
'
'   1) 分箱内核：输入 request.TileOffsets / TileGaussians（已经在 CPU 侧算好，
'      是"每 tile 的高斯模型下标、深度有序"），把它们作为 Integer* 上传显存。
'   2) 前向内核：每个 block 负责一个 tile；block 内用 shared memory 保存
'      16×16 的颜色累积与透射率（布局与 TileScratch 完全一致：
'      off = (y - tileY0) * tileSize + (x - tileX0)）。逐高斯 front-to-back：
'          q     = a·dx² + 2b·dx·dy + c·dy²
'          alpha = min(alphaMax, opacity·exp(-q/2))
'          C    += color·alpha·T;  T *= (1 - alpha)
'      跳过条件必须与 CPU 内核逐字一致：q > 9、alpha < 1e-6、T < 1e-6。
'   3) 反向内核对每个 tile 的像素链重放（前向→后向），把 dcolor/dopacity 直接
'      累加、把 dconic/dmean 用 atomicAdd 累加到每高斯 3+2 个槽位，最后交给
'      库内的 SplatProjectionGradient.Chain 完成"屏幕空间 → 参数"的链式回传。
'
' 注意：ILCuda 的内核参数只支持 DeviceBuffer/Single/Double/Integer/Long/IntPtr
' （见 cuda/ILCuda/Runtime/CudaKernel.vb），所以像素缓冲必须是 DeviceBuffer(Of Double)。
'
' 在实测（320×240、2000~20000 高斯）下，CPU tile 内核已经把单视图压到毫秒级，
' 显存往返 + 启动开销与内核时间的比例并不划算；因此本示例默认返回 False
' （由调用方自动回退），把"什么时候值得上 GPU"留给实际部署时用 bench 判定。
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.MachineVision.GaussianSplatting3D
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor

''' <summary>
''' CUDA 光栅化后端骨架：演示扩展点与契约，未实现内核时返回 False 触发回退。
''' </summary>
Public Class CudaRasterBackend
    Implements ISplatRasterBackend

    ''' <summary>是否已注册 CUDA 张量后端（决定本后端是否"可用"）。</summary>
    Private ReadOnly _available As Boolean

    Public Sub New(Optional available As Boolean = False)
        _available = available
    End Sub

    Public ReadOnly Property Name As String Implements ISplatRasterBackend.Name
        Get
            Return "CUDA-Splat"
        End Get
    End Property

    Public Function IsAvailable() As Boolean Implements ISplatRasterBackend.IsAvailable
        Return _available
    End Function

    ''' <summary>
    ''' 前向：返回 False 表示"本后端本次不可用"，调用方会自动改用 CPU 后端。
    ''' </summary>
    Public Function Rasterize(request As SplatRasterRequest, image As Double(),
                               transmittance As Double()) As Boolean Implements ISplatRasterBackend.Rasterize

        If Not _available Then Return False

        ' TODO: 上传 request.TileOffsets / TileGaussians 与 SplatScreenData 的 SoA 数组，
        '       启动 per-tile 内核，再把 image / transmittance 拷回主机。
        Return False
    End Function

    Public Function RasterizeBackward(request As SplatRasterRequest, image As Double(), imageGradient As Double(),
                                       grads As SplatGradients) As Boolean Implements ISplatRasterBackend.RasterizeBackward

        If Not _available Then Return False

        ' TODO: 反向内核对每 tile 的像素链重放，dconic/dmean 用 atomicAdd 累加。
        Return False
    End Function

    ''' <summary>把本后端注册进库（并可选设为默认）。</summary>
    Public Shared Function Install(Optional makeDefault As Boolean = True) As CudaRasterBackend
        Dim available = gpu.CudaTensor.Current IsNot Nothing
        Dim backend As New CudaRasterBackend(available)

        Call SplatBackends.Register(backend, makeDefault AndAlso available)

        Return backend
    End Function

End Class

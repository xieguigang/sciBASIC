' ------------------------------------------------------------------------
' demo 的 GPU 流水线：rowStats -> gram -> finalize
'
' 三个内核的源码就在本工程的 Kernels\metrics.cu 里（作为内嵌资源），
' 启动时由 Program 通过 KernelSources.Register(...) 注入框架一起编译。
'
' 这里只负责"编排"：向框架的 KernelCatalog 登记内核元数据，
' 用 LaunchPlanner 推导分块形状，然后按顺序启动内核。
' ------------------------------------------------------------------------

Imports Enigma.ILCuda.Runtime

Namespace Metrics

    ''' <summary>
    ''' demo 内核的函数名（与 Kernels\metrics.cu 中的 extern "C" 名称一一对应）。
    ''' 业务方自带的核名不属于框架，因此在自己的代码里定义。
    ''' </summary>
    Public Module MetricsKernelNames
        Public Const RowStats As String = "rowStatsKernel"
        Public Const Gram As String = "gramKernel"
        Public Const FinalizeMetrics As String = "finalizeKernel"
    End Module

    ''' <summary>行间相似度矩阵（皮尔逊相关系数 / 欧氏距离）的 GPU 实现</summary>
    Public Module GpuMetrics

        ''' <summary>gramKernel 使用的分块大小（必须与 metrics.cu 中的 TILE 一致）</summary>
        Public Const TileSize As Integer = 16
        ''' <summary>rowStatsKernel 每个 block 的线程数</summary>
        Public Const RowStatsBlockSize As Integer = 256

        ''' <summary>
        ''' 把 demo 内核的元数据登记到框架的内核注册表（便于统一查询与默认启动参数）
        ''' </summary>
        Public Sub RegisterKernels()
            KernelCatalog.Add(New KernelInfo(
                MetricsKernelNames.RowStats, "metrics.cu", RowStatsBlockSize,
                2 * RowStatsBlockSize * 4, "每个 block 归约一行，得到 sum / sumSq"))

            KernelCatalog.Add(New KernelInfo(
                MetricsKernelNames.Gram, "metrics.cu", TileSize * TileSize,
                2 * TileSize * TileSize * 4, "16x16 分块点积矩阵（只算上三角）"))

            KernelCatalog.Add(New KernelInfo(
                MetricsKernelNames.FinalizeMetrics, "metrics.cu", TileSize * TileSize, 0,
                "由点积与行统计量同时产出相关矩阵与距离矩阵"))
        End Sub

        ''' <summary>
        ''' 在 GPU 上计算矩阵行间的皮尔逊相关系数矩阵与欧氏距离矩阵
        ''' </summary>
        ''' <param name="stream">可选的执行流；传入后内核在该流上排队，调用方负责同步</param>
        Public Function Compute(engine As CudaEngine, x As MatrixData,
                                Optional stream As CudaStream = Nothing) As MatrixMetricsResult
            Dim rows = x.Rows
            Dim cols = x.Cols
            Dim cells = rows * rows

            Dim result As New MatrixMetricsResult()

            Using bufferX As New DeviceBuffer(Of Single)(x.Data.Length),
                  bufferSum As New DeviceBuffer(Of Single)(rows),
                  bufferSumSq As New DeviceBuffer(Of Single)(rows),
                  bufferDot As New DeviceBuffer(Of Single)(cells),
                  bufferCorr As New DeviceBuffer(Of Single)(cells),
                  bufferDist As New DeviceBuffer(Of Single)(cells)

                Dim uploadWatch = Stopwatch.StartNew()
                bufferX.Write(x.Data)
                uploadWatch.Stop()

                Dim kernelMs As Double = 0

                ' 二维分块：行列各按 TILE 切块
                Dim gramConfig = LaunchPlanner.For2D(rows, rows, TileSize, TileSize,
                                                     2 * TileSize * TileSize * 4)
                Dim finalizeConfig = LaunchPlanner.For2D(rows, rows, TileSize, TileSize, 0)

                Using timer As New CudaTimer()
                    timer.Start()

                    ' 1) 每行的 sum / sumSq：一个 block 负责一行，因此 grid = rows
                    engine.GetKernel(MetricsKernelNames.RowStats).Launch(
                        stream, rows, 1, RowStatsBlockSize, 1, 2 * RowStatsBlockSize * 4,
                        bufferX, rows, cols, bufferSum, bufferSumSq)

                    ' 2) 分块点积矩阵（只算上三角）
                    engine.GetKernel(MetricsKernelNames.Gram).Launch(
                        stream, gramConfig,
                        bufferX, rows, cols, bufferDot)

                    ' 3) 收尾：同时产出相关矩阵与距离矩阵
                    engine.GetKernel(MetricsKernelNames.FinalizeMetrics).Launch(
                        stream, finalizeConfig,
                        bufferDot, bufferSum, bufferSumSq, rows, cols, bufferCorr, bufferDist)

                    kernelMs = timer.Finish()
                End Using

                Dim downloadWatch = Stopwatch.StartNew()
                Dim corr = bufferCorr.Read()
                Dim dist = bufferDist.Read()
                downloadWatch.Stop()

                result.Correlation = New MetricResult(rows, corr)
                result.Distance = New MetricResult(rows, dist)
                result.KernelMs = kernelMs
                result.CopyMs = uploadWatch.Elapsed.TotalMilliseconds + downloadWatch.Elapsed.TotalMilliseconds
            End Using

            Return result
        End Function

        ''' <summary>
        ''' 演示异步执行：用页锁定内存 + 独立流把上传与计算串在同一条流上，
        ''' 主机端不必在每次拷贝处阻塞。
        ''' </summary>
        Public Function ComputeAsync(engine As CudaEngine, x As MatrixData) As MatrixMetricsResult
            Dim rows = x.Rows
            Dim cols = x.Cols
            Dim cells = rows * rows

            Dim result As New MatrixMetricsResult()

            Using stream As New CudaStream(CudaStreamFlags.NonBlocking),
                  stageIn As New PinnedHostBuffer(Of Single)(x.Data.Length),
                  stageCorr As New PinnedHostBuffer(Of Single)(cells),
                  stageDist As New PinnedHostBuffer(Of Single)(cells),
                  bufferX As New DeviceBuffer(Of Single)(x.Data.Length),
                  bufferSum As New DeviceBuffer(Of Single)(rows),
                  bufferSumSq As New DeviceBuffer(Of Single)(rows),
                  bufferDot As New DeviceBuffer(Of Single)(cells),
                  bufferCorr As New DeviceBuffer(Of Single)(cells),
                  bufferDist As New DeviceBuffer(Of Single)(cells)

                stageIn.Write(x.Data)

                Dim uploadWatch = Stopwatch.StartNew()
                bufferX.WriteAsync(stageIn, stream)
                uploadWatch.Stop()

                Dim gramConfig = LaunchPlanner.For2D(rows, rows, TileSize, TileSize,
                                                     2 * TileSize * TileSize * 4)
                Dim finalizeConfig = LaunchPlanner.For2D(rows, rows, TileSize, TileSize, 0)

                Using timer As New CudaTimer()
                    timer.Start()

                    engine.GetKernel(MetricsKernelNames.RowStats).Launch(
                        stream, rows, 1, RowStatsBlockSize, 1, 2 * RowStatsBlockSize * 4,
                        bufferX, rows, cols, bufferSum, bufferSumSq)
                    engine.GetKernel(MetricsKernelNames.Gram).Launch(
                        stream, gramConfig, bufferX, rows, cols, bufferDot)
                    engine.GetKernel(MetricsKernelNames.FinalizeMetrics).Launch(
                        stream, finalizeConfig,
                        bufferDot, bufferSum, bufferSumSq, rows, cols, bufferCorr, bufferDist)

                    result.KernelMs = timer.Finish()
                End Using

                Dim downloadWatch = Stopwatch.StartNew()
                bufferCorr.ReadAsync(stageCorr, stream)
                bufferDist.ReadAsync(stageDist, stream)
                stream.Synchronize()
                downloadWatch.Stop()

                result.Correlation = New MetricResult(rows, stageCorr.Read())
                result.Distance = New MetricResult(rows, stageDist.Read())
                result.CopyMs = uploadWatch.Elapsed.TotalMilliseconds + downloadWatch.Elapsed.TotalMilliseconds
            End Using

            Return result
        End Function
    End Module
End Namespace

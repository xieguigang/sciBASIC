' ------------------------------------------------------------------------
' 启动配置推导
'
' 手写 grid / block 很容易出错（漏掉向上取整、block 不是 warp 的整数倍、
' 分块矩阵漏算边界块等），这里统一收敛成两个入口：
'   LaunchPlanner.For1D(totalElements)          -> 一维向量运算
'   LaunchPlanner.For2D(rows, cols, tileX, tileY) -> 二维分块运算（矩阵）
'
' 需要"最佳 block 尺寸"时可以用 SuggestBlockSize：优先问驱动的
' cuOccupancyMaxPotentialBlockSize，老驱动没有该入口时退回启发式。
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>一次内核启动的形状</summary>
    Public Structure LaunchConfig
        ''' <summary>grid 的 X / Y 维度</summary>
        Public Property GridX As Integer
        ''' <summary>grid 的 Y 维度</summary>
        Public Property GridY As Integer
        ''' <summary>block 的 X 维度（每 block 线程数 = BlockX * BlockY）</summary>
        Public Property BlockX As Integer
        ''' <summary>block 的 Y 维度</summary>
        Public Property BlockY As Integer
        ''' <summary>动态共享内存字节数</summary>
        Public Property SharedMemBytes As Integer

        Public Sub New(gridX As Integer, gridY As Integer, blockX As Integer, blockY As Integer,
                       Optional sharedMemBytes As Integer = 0)
            Me.GridX = gridX
            Me.GridY = gridY
            Me.BlockX = blockX
            Me.BlockY = blockY
            Me.SharedMemBytes = sharedMemBytes
        End Sub

        ''' <summary>每 block 的线程数</summary>
        Public ReadOnly Property ThreadsPerBlock As Integer
            Get
                Return BlockX * BlockY
            End Get
        End Property

        ''' <summary>启动的 block 总数</summary>
        Public ReadOnly Property TotalBlocks As Long
            Get
                Return CLng(GridX) * CLng(GridY)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Dim smem = If(SharedMemBytes > 0, $", smem={SharedMemBytes}B", "")
            Return $"grid({GridX},{GridY}) block({BlockX},{BlockY}){smem}"
        End Function
    End Structure

    ''' <summary>grid / block 推导助手</summary>
    Public Module LaunchPlanner

        ''' <summary>默认的一维 block 尺寸</summary>
        Public Const DefaultBlockSize As Integer = 256
        ''' <summary>假定的 warp 大小（真实值以 CudaDevice.WarpSize 为准）</summary>
        Public Const AssumedWarpSize As Integer = 32
        ''' <summary>一维启动时 block 数的上限；配合 grid-stride 内核可以处理任意规模</summary>
        Public Const MaxBlocks1D As Integer = 4096

        ''' <summary>向上取整除法</summary>
        Public Function CeilDiv(value As Integer, divisor As Integer) As Integer
            If divisor <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(divisor))
            If value <= 0 Then Return 0
            Return (value + divisor - 1) \ divisor
        End Function

        ''' <summary>把 block 尺寸对齐到 warp 的整数倍（上限 1024）</summary>
        Public Function AlignToWarp(blockSize As Integer, Optional warpSize As Integer = AssumedWarpSize) As Integer
            If blockSize <= 0 Then blockSize = DefaultBlockSize
            If blockSize > 1024 Then blockSize = 1024
            If warpSize <= 0 Then warpSize = AssumedWarpSize

            Dim aligned = CInt(System.Math.Ceiling(blockSize / CDbl(warpSize))) * warpSize
            If aligned > 1024 Then aligned = 1024

            Return System.Math.Max(aligned, warpSize)
        End Function

        ''' <summary>
        ''' 一维向量运算的启动配置。
        ''' 内核内使用 grid-stride 循环时可以放心把 block 数限制在 MaxBlocks1D 以内。
        ''' </summary>
        Public Function For1D(totalElements As Integer,
                              Optional blockSize As Integer = DefaultBlockSize,
                              Optional maxBlocks As Integer = MaxBlocks1D) As LaunchConfig
            If totalElements <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(totalElements))
            If blockSize <= 0 Then blockSize = DefaultBlockSize

            Dim blocks = CeilDiv(totalElements, blockSize)

            If maxBlocks > 0 AndAlso blocks > maxBlocks Then blocks = maxBlocks

            Return New LaunchConfig(System.Math.Max(1, blocks), 1, blockSize, 1, 0)
        End Function

        ''' <summary>二维分块运算（矩阵）的启动配置，例如 16x16 的分块矩阵乘</summary>
        Public Function For2D(rows As Integer, cols As Integer,
                              Optional tileX As Integer = 16,
                              Optional tileY As Integer = 16,
                              Optional sharedMemBytes As Integer = 0) As LaunchConfig
            If rows <= 0 OrElse cols <= 0 Then Throw New ArgumentOutOfRangeException("rows/cols 必须为正数")
            If tileX <= 0 OrElse tileY <= 0 Then Throw New ArgumentOutOfRangeException("tile 必须为正数")

            Return New LaunchConfig(CeilDiv(cols, tileX), CeilDiv(rows, tileY), tileX, tileY, sharedMemBytes)
        End Function

        ''' <summary>
        ''' 询问驱动该内核的最佳 block 尺寸；驱动不支持 occupancy 入口时
        ''' 退回"按 SM 数量 × 每 SM 线程数"的启发式，并按 warp 对齐。
        ''' </summary>
        Public Function SuggestBlockSize(device As CudaDevice, kernel As CudaKernel,
                                         Optional maxBlock As Integer = DefaultBlockSize) As Integer
            If maxBlock <= 0 Then maxBlock = DefaultBlockSize

            Dim minGrid As Integer = 0
            Dim suggested As Integer = 0

            Try
                If device IsNot Nothing AndAlso kernel IsNot Nothing Then
                    Dim status = CudaDriverApi.cuOccupancyMaxPotentialBlockSize(
                        minGrid, suggested, kernel.Handle, IntPtr.Zero, 0UI, maxBlock)

                    If status = CUresult.CUDA_SUCCESS AndAlso suggested > 0 Then
                        Return AlignToWarp(suggested, If(device.WarpSize > 0, device.WarpSize, AssumedWarpSize))
                    End If
                End If
            Catch
                ' 老驱动没有该入口，走下面的启发式
            End Try

            Return AlignToWarp(maxBlock, If(device Is Nothing OrElse device.WarpSize <= 0, AssumedWarpSize, device.WarpSize))
        End Function

        ''' <summary>
        ''' 结合设备能力给出一维启动配置（block 尺寸由驱动 occupancy 推导）
        ''' </summary>
        Public Function Auto1D(device As CudaDevice, kernel As CudaKernel, totalElements As Integer,
                               Optional maxBlock As Integer = DefaultBlockSize,
                               Optional sharedMemBytes As Integer = 0) As LaunchConfig
            Dim block = SuggestBlockSize(device, kernel, maxBlock)
            Dim config = For1D(totalElements, block)
            config.SharedMemBytes = sharedMemBytes
            Return config
        End Function
    End Module
End Namespace

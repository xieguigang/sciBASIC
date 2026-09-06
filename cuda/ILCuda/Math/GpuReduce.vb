' ------------------------------------------------------------------------
' 通用归约（sum / max / min）
'
' 两阶段实现，全程在 GPU 上完成，不需要主机端中途同步：
'   阶段一：grid 个 block，各出一个部分结果（共享内存树形归约）；
'   阶段二：1 个 block 把部分结果汇总。
' ------------------------------------------------------------------------

Imports Enigma.ILCuda.Runtime

Namespace Math

    ''' <summary>归约算子的种类</summary>
    Public Enum ReduceOp
        Sum
        Max
        Min
    End Enum

    ''' <summary>单精度向量的通用归约</summary>
    Public Module GpuReduce

        ''' <summary>第一阶段每 block 的线程数</summary>
        Public Const StageBlockSize As Integer = 256
        ''' <summary>第一阶段 block 数上限（配合 grid-stride 可处理任意长度）</summary>
        Public Const MaxStageBlocks As Integer = 1024
        ''' <summary>第二阶段（收尾）每 block 的线程数</summary>
        Public Const FinalBlockSize As Integer = 256

        ''' <summary>对显存向量做归约</summary>
        Public Function Reduce(engine As CudaEngine, x As DeviceBuffer(Of Single), op As ReduceOp,
                               Optional stream As CudaStream = Nothing) As Single
            If engine Is Nothing Then Throw New ArgumentNullException(NameOf(engine))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))

            Dim n = x.Count
            Dim blocks = System.Math.Min(LaunchPlanner.CeilDiv(n, StageBlockSize), MaxStageBlocks)
            If blocks <= 0 Then blocks = 1

            Dim stageName = StageKernelOf(op)
            Dim finalName = FinalKernelOf(op)
            Dim sharedMem = StageBlockSize * 4

            Using partials As New DeviceBuffer(Of Single)(blocks),
                  output As New DeviceBuffer(Of Single)(1)

                ' 阶段一：每 block 一个部分结果（reduceXxxKernel(x, partial, n)）
                engine.GetKernel(stageName).Launch(
                    stream, blocks, 1, StageBlockSize, 1, sharedMem,
                    x, partials, n)

                ' 阶段二：汇总（reduceFinalXxxKernel(partial, out, count)）
                engine.GetKernel(finalName).Launch(
                    stream, 1, 1, FinalBlockSize, 1, FinalBlockSize * 4,
                    partials, output, blocks)

                Dim result = output.Read()
                Return result(0)
            End Using
        End Function

        ''' <summary>求和</summary>
        Public Function Sum(engine As CudaEngine, x As DeviceBuffer(Of Single),
                            Optional stream As CudaStream = Nothing) As Single
            Return Reduce(engine, x, ReduceOp.Sum, stream)
        End Function

        ''' <summary>最大值</summary>
        Public Function Max(engine As CudaEngine, x As DeviceBuffer(Of Single),
                            Optional stream As CudaStream = Nothing) As Single
            Return Reduce(engine, x, ReduceOp.Max, stream)
        End Function

        ''' <summary>最小值</summary>
        Public Function Min(engine As CudaEngine, x As DeviceBuffer(Of Single),
                            Optional stream As CudaStream = Nothing) As Single
            Return Reduce(engine, x, ReduceOp.Min, stream)
        End Function

        ''' <summary>一次同时算出 sum / max / min（共用一次上传，避免重复分配显存）</summary>
        Public Function Summarize(engine As CudaEngine, x As DeviceBuffer(Of Single),
                                  Optional stream As CudaStream = Nothing) As VectorSummary
            Return New VectorSummary With {
                .Sum = Sum(engine, x, stream),
                .Max = Max(engine, x, stream),
                .Min = Min(engine, x, stream)
            }
        End Function

        Private Function StageKernelOf(op As ReduceOp) As String
            Select Case op
                Case ReduceOp.Sum
                    Return KernelNames.ReduceSum
                Case ReduceOp.Max
                    Return KernelNames.ReduceMax
                Case Else
                    Return KernelNames.ReduceMin
            End Select
        End Function

        Private Function FinalKernelOf(op As ReduceOp) As String
            Select Case op
                Case ReduceOp.Sum
                    Return KernelNames.ReduceFinalSum
                Case ReduceOp.Max
                    Return KernelNames.ReduceFinalMax
                Case Else
                    Return KernelNames.ReduceFinalMin
            End Select
        End Function
    End Module

    ''' <summary>一个向量的汇总统计</summary>
    Public Class VectorSummary
        Public Property Sum As Single
        Public Property Max As Single
        Public Property Min As Single

        Public Overrides Function ToString() As String
            Return $"sum={Sum:G6}, min={Min:G6}, max={Max:G6}"
        End Function
    End Class
End Namespace

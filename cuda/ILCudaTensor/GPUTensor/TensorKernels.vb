' ---------------------------------------------------------------------------
' P3 手写张量内核的函数名（与 Kernels\tensor.cu 中的 extern "C" 名称一一对应）
'
' 这些内核在内嵌资源 tensor.cu 里定义，由 CudaTensor.Register 通过
' KernelSources.Register 注入 NVRTC 编译单元。
' ---------------------------------------------------------------------------

Namespace GPUTensor

    Friend Module TensorKernelNames

        ''' <summary>末轴 softmax</summary>
        Public Const RowSoftmax As String = "tensorRowSoftmaxKernel"
        ''' <summary>末轴 log-softmax</summary>
        Public Const RowLogSoftmax As String = "tensorRowLogSoftmaxKernel"
        ''' <summary>末轴求和</summary>
        Public Const RowSum As String = "tensorRowReduceSumKernel"
        ''' <summary>末轴求均值</summary>
        Public Const RowMean As String = "tensorRowReduceMeanKernel"
        ''' <summary>末轴最大值</summary>
        Public Const RowMax As String = "tensorRowReduceMaxKernel"
        ''' <summary>末轴最小值</summary>
        Public Const RowMin As String = "tensorRowReduceMinKernel"
        ''' <summary>末轴 argmax</summary>
        Public Const RowArgMax As String = "tensorRowArgMaxKernel"
        ''' <summary>末轴 argmin</summary>
        Public Const RowArgMin As String = "tensorRowArgMinKernel"
        ''' <summary>双精度分块矩阵乘 C(m x n) = A(m x k) * B(k x n)</summary>
        Public Const GemmDouble As String = "tensorGemmDoubleKernel"

    End Module

End Namespace

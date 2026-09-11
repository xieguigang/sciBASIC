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

        ' ---- 两段式全局归约 ----
        ''' <summary>全局求和 - 阶段一（部分结果）</summary>
        Public Const PartialSum As String = "tensorReducePartialSumKernel"
        ''' <summary>全局求和 - 阶段二（汇总）</summary>
        Public Const FinalSum As String = "tensorReduceFinalSumKernel"
        ''' <summary>全局最大值 - 阶段一</summary>
        Public Const PartialMax As String = "tensorReducePartialMaxKernel"
        ''' <summary>全局最大值 - 阶段二</summary>
        Public Const FinalMax As String = "tensorReduceFinalMaxKernel"
        ''' <summary>全局最小值 - 阶段一</summary>
        Public Const PartialMin As String = "tensorReducePartialMinKernel"
        ''' <summary>全局最小值 - 阶段二</summary>
        Public Const FinalMin As String = "tensorReduceFinalMinKernel"

    End Module

End Namespace

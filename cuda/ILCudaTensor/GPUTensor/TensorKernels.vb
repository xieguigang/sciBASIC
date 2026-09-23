#Region "Microsoft.VisualBasic::3dbcf5f9be3446c6838aa8cf351d7e26, cuda\ILCudaTensor\GPUTensor\TensorKernels.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 73
    '    Code Lines: 30 (41.10%)
    ' Comment Lines: 35 (47.95%)
    '    - Xml Docs: 74.29%
    ' 
    '   Blank Lines: 8 (10.96%)
    '     File Size: 4.37 KB


    '     Module TensorKernelNames
    ' 
    '         Fields: Conv2DBackwardBias, Conv2DBackwardFilter, Conv2DBackwardInput, Conv2DForward
    '                 FinalMax, FinalMin, FinalSum, GemmDouble
    '                 LifUpdateDouble, LifUpdateFp32, MaxPool2DBackward, MaxPool2DForward
    '                 PartialMax, PartialMin, PartialSum, RowArgMax
    '                 RowArgMin, RowLogSoftmax, RowMax, RowMean
    '                 RowMin, RowSoftmax, RowSum, SpmmCsr
    '                 TrainAccumulate, TrainAdamW, TrainMaskedCrossEntropy, TrainTranspose
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
        ''' <summary>CSR 稀疏 × 稠密（Kernels\spmm.cu）</summary>
        Public Const SpmmCsr As String = "tensorSpmmCsrKernel"

        ' ---- 卷积与池化（Kernels\conv.cu / Kernels\pool.cu）----
        ''' <summary>卷积前向（NHWC）</summary>
        Public Const Conv2DForward As String = "tensorConv2DForwardKernel"
        ''' <summary>卷积反向 - 对输入的梯度</summary>
        Public Const Conv2DBackwardInput As String = "tensorConv2DBackwardInputKernel"
        ''' <summary>卷积反向 - 对卷积核的梯度</summary>
        Public Const Conv2DBackwardFilter As String = "tensorConv2DBackwardFilterKernel"
        ''' <summary>卷积反向 - 对偏置的梯度</summary>
        Public Const Conv2DBackwardBias As String = "tensorConv2DBackwardBiasKernel"
        ''' <summary>最大池化前向（同时产出 argMax）</summary>
        Public Const MaxPool2DForward As String = "tensorMaxPool2DForwardKernel"
        ''' <summary>最大池化反向（按 argMax 散射）</summary>
        Public Const MaxPool2DBackward As String = "tensorMaxPool2DBackwardKernel"

        ' ---- 训练内核（Kernels\train.cu，单精度 FP32）----
        ''' <summary>二维转置（单精度，可直读设备常驻权重）</summary>
        Public Const TrainTranspose As String = "tensorTransposeFp32Kernel"
        ''' <summary>AdamW 原地更新参数与一阶/二阶矩，并清零梯度累加器</summary>
        Public Const TrainAdamW As String = "tensorAdamWFp32Kernel"
        ''' <summary>梯度累加：<c>accum += alpha * src</c></summary>
        Public Const TrainAccumulate As String = "tensorAccumulateFp32Kernel"
        ''' <summary>融合掩码交叉熵：softmax + NLL + <c>d(logits) = (softmax − onehot) / count</c></summary>
        Public Const TrainMaskedCrossEntropy As String = "tensorMaskedCrossEntropyFp32Kernel"

        ' ---- 脉冲网络融合内核（Kernels\lif.cu）----
        ''' <summary>融合的递归 LIF 单步（双精度状态，就地更新 H / S / counts）</summary>
        Public Const LifUpdateDouble As String = "tensorLifUpdateDoubleKernel"
        ''' <summary>融合的递归 LIF 单步（单精度 H / counts，脉冲仍为双精度）</summary>
        Public Const LifUpdateFp32 As String = "tensorLifUpdateFp32Kernel"

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

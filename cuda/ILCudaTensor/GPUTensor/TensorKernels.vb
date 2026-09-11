#Region "Microsoft.VisualBasic::bcb5f2f07118e8e50c04db0f82336bfc, cuda\ILCudaTensor\GPUTensor\TensorKernels.vb"

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

    '   Total Lines: 47
    '    Code Lines: 19 (40.43%)
    ' Comment Lines: 22 (46.81%)
    '    - Xml Docs: 68.18%
    ' 
    '   Blank Lines: 6 (12.77%)
    '     File Size: 2.47 KB


    '     Module TensorKernelNames
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


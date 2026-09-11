#Region "Microsoft.VisualBasic::itensorCompute::Data_science\MachineLearning\TensorFlow\Compute\ITensorCompute.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

' ---------------------------------------------------------------------------
' 张量计算的“可插拔后端”契约
'
' 这个接口把 Tensor / Math / nn 里所有**纯数值计算**的算子抽象出来，
' <see cref="Tensor.computeKernel"/> 持有它的一个实例作为当前生效的后端：
'
'   * 默认后端（<c>SIMDTensor</c>）：基于 System.Numerics.Vector 的 SIMD 加速 CPU 实现；
'   * 可选后端（<c>ILCuda.GPUTensor.CudaTensor</c>）：基于 ILCuda 的 GPU 实现。
'
' 下游深度学习代码只需要调用 <c>Tensor</c> / <c>Math</c> / <c>nn</c> 的公开 API，
' 不需要感知后端，即可通过一次 <c>Register()</c> 调用在 CPU / GPU 之间无损切换。
' ---------------------------------------------------------------------------

Namespace Compute

    ''' <summary>
    ''' 张量计算后端契约。
    ''' </summary>
    ''' <remarks>
    ''' 形状校验、广播等“语义边界”仍然由 <see cref="Tensor"/> 层负责，
    ''' 后端只需要实现纯粹的数值计算。后端应当是**无状态**（或线程安全）的，
    ''' 因为 <see cref="Tensor.computeKernel"/> 是一个全局共享变量。
    ''' </remarks>
    Public Interface ITensorCompute

        ''' <summary>
        ''' 后端名称，用于诊断输出（例如 "SIMD" / "CUDA"）。
        ''' </summary>
        ReadOnly Property Name As String

#Region "逐元素 - 二元"

        Function Add(a As Tensor, b As Tensor) As Tensor
        Function Subtract(a As Tensor, b As Tensor) As Tensor
        Function Multiply(a As Tensor, b As Tensor) As Tensor
        Function Divide(a As Tensor, b As Tensor) As Tensor
        Function Maximum(a As Tensor, b As Tensor) As Tensor
        Function Minimum(a As Tensor, b As Tensor) As Tensor

#End Region

#Region "逐元素 - 一元"

        Function Exp(t As Tensor) As Tensor
        Function Log(t As Tensor) As Tensor
        Function Sqrt(t As Tensor) As Tensor
        Function Square(t As Tensor) As Tensor
        Function Abs(t As Tensor) As Tensor
        Function Sin(t As Tensor) As Tensor
        Function Cos(t As Tensor) As Tensor
        Function Tanh(t As Tensor) As Tensor
        Function Sigmoid(t As Tensor) As Tensor
        Function Negate(t As Tensor) As Tensor
        Function Reciprocal(t As Tensor) As Tensor

        Function Relu(t As Tensor) As Tensor
        Function LeakyRelu(t As Tensor, alpha As Double) As Tensor
        Function Elu(t As Tensor, alpha As Double) As Tensor
        Function Gelu(t As Tensor) As Tensor
        Function Swish(t As Tensor) As Tensor

#End Region

#Region "标量运算"

        Function AddScalar(t As Tensor, scalar As Double) As Tensor
        Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor
        Function DivideScalar(t As Tensor, scalar As Double) As Tensor
        Function Pow(t As Tensor, exponent As Double) As Tensor
        Function Clip(t As Tensor, low As Double, high As Double) As Tensor

#End Region

#Region "矩阵运算"

        Function MatMul(a As Tensor, b As Tensor) As Tensor
        Function Transpose(t As Tensor) As Tensor

#End Region

#Region "归约运算"

        ''' <summary>沿指定轴（Nothing 表示整体）求和</summary>
        Function Sum(t As Tensor, axis As Integer?) As Tensor
        ''' <summary>整体求和</summary>
        Function SumAll(t As Tensor) As Double
        ''' <summary>沿指定轴（Nothing 表示整体）求均值</summary>
        Function Mean(t As Tensor, axis As Integer?) As Tensor
        ''' <summary>整体求均值</summary>
        Function MeanAll(t As Tensor) As Double
        Function Max(t As Tensor, axis As Integer?) As Tensor
        Function Min(t As Tensor, axis As Integer?) As Tensor
        Function Prod(t As Tensor, axis As Integer?) As Tensor
        ''' <summary>总体标准差</summary>
        Function StdDev(t As Tensor) As Double
        ''' <summary>L2 范数（欧几里得范数）</summary>
        Function L2Norm(t As Tensor) As Double
        Function ArgMax(t As Tensor, axis As Integer?) As Tensor
        Function ArgMin(t As Tensor, axis As Integer?) As Tensor

#End Region

#Region "神经网络"

        Function Softmax(t As Tensor, axis As Integer) As Tensor
        Function LogSoftmax(t As Tensor, axis As Integer) As Tensor
        Function SigmoidCrossEntropyWithLogits(labels As Tensor, logits As Tensor) As Tensor
        Function MseLoss(predictions As Tensor, targets As Tensor) As Tensor
        Function L2Loss(t As Tensor) As Tensor
        Function HuberLoss(predictions As Tensor, targets As Tensor, delta As Double) As Tensor

#End Region

    End Interface

End Namespace

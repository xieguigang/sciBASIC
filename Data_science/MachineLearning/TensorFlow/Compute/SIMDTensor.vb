#Region "Microsoft.VisualBasic::simdTensor::Data_science\MachineLearning\TensorFlow\Compute\SIMDTensor.vb"

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
' 默认计算后端：基于 System.Numerics.Vector 的 SIMD 加速 CPU 实现。
'
' 这里只重写“确实有向量指令收益”的热点算子（逐元素算术/数学函数、整体归约、
' 矩阵乘），其余算子在 <see cref="TensorComputeBase"/> 中已经有正确的标量实现，
' 直接继承即可。
'
' 所有实现都复用 Microsoft.VisualBasic.Core 中的 Math.SIMD 基础库：
'   SimdEngine  —— 逐元素算术
'   SimdMath    —— 逐元素数学函数
'   SimdReduce  —— 单线程归约
'   SimdParallel—— 分块并行 + 块内向量化
' ---------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' 默认的 SIMD 加速 CPU 计算后端。
    ''' </summary>
    Public Class SIMDTensor
        Inherits TensorComputeBase

        ''' <summary>
        ''' 矩阵乘走到 SIMD 行列点积路径的最小规模（m*k*n）。
        ''' 低于该规模时构建交错数组的开销会超过收益，直接退回标量实现。
        ''' </summary>
        Public Const MatrixDotThreshold As Integer = 10000

        ''' <summary>全局默认实例（<see cref="Tensor.computeKernel"/> 的初始值）</summary>
        Public Shared ReadOnly Property [Default] As SIMDTensor = New SIMDTensor()

        Public Overrides ReadOnly Property Name As String = "SIMD"

        ''' <summary>
        ''' 把 <see cref="Tensor.computeKernel"/> 切换为 SIMD 加速的 CPU 实现。
        ''' </summary>
        Public Shared Sub Register()
            SyncLock Tensor.SyncRoot
                Tensor.computeKernel = [Default]
            End SyncLock
        End Sub

#Region "逐元素 - 二元"

        Public Overrides Function Add(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相加")
            Return Wrap(SimdParallel.Add(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Subtract(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相减")
            Return Wrap(SimdParallel.Subtract(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Multiply(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相乘")
            Return Wrap(SimdParallel.Multiply(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Divide(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "相除")
            Return Wrap(SimdEngine.Divide(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Maximum(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "取最大值")
            Return Wrap(SimdEngine.Max(Of Double)(a.Data, b.Data), a.Shape)
        End Function

        Public Overrides Function Minimum(a As Tensor, b As Tensor) As Tensor
            RequireSameShape(a, b, "取最小值")
            Return Wrap(SimdEngine.Min(Of Double)(a.Data, b.Data), a.Shape)
        End Function

#End Region

#Region "逐元素 - 一元"

        Public Overrides Function Exp(t As Tensor) As Tensor
            Return Wrap(SimdMath.Exp(t.Data), t.Shape)
        End Function

        Public Overrides Function Log(t As Tensor) As Tensor
            Return Wrap(SimdMath.Log(t.Data), t.Shape)
        End Function

        Public Overrides Function Sqrt(t As Tensor) As Tensor
            Return Wrap(SimdMath.Sqrt(t.Data), t.Shape)
        End Function

        Public Overrides Function Square(t As Tensor) As Tensor
            Return Wrap(SimdMath.Square(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Abs(t As Tensor) As Tensor
            Return Wrap(SimdMath.Abs(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Negate(t As Tensor) As Tensor
            Return Wrap(SimdMath.Negate(Of Double)(t.Data), t.Shape)
        End Function

        Public Overrides Function Reciprocal(t As Tensor) As Tensor
            Return Wrap(SimdMath.Reciprocal(t.Data), t.Shape)
        End Function

#End Region

#Region "标量运算"

        Public Overrides Function AddScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdEngine.AddScalar(Of Double)(t.Data, scalar), t.Shape)
        End Function

        Public Overrides Function MultiplyScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdParallel.MultiplyScalar(scalar, t.Data), t.Shape)
        End Function

        Public Overrides Function DivideScalar(t As Tensor, scalar As Double) As Tensor
            Return Wrap(SimdEngine.DivideScalar(t.Data, scalar), t.Shape)
        End Function

        Public Overrides Function Pow(t As Tensor, exponent As Double) As Tensor
            Return Wrap(SimdMath.PowScalar(t.Data, exponent), t.Shape)
        End Function

        Public Overrides Function Clip(t As Tensor, low As Double, high As Double) As Tensor
            Return Wrap(SimdMath.Clamp(Of Double)(t.Data, low, high), t.Shape)
        End Function

#End Region

#Region "矩阵运算"

        Public Overrides Function MatMul(a As Tensor, b As Tensor) As Tensor
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("矩阵乘法需要二维张量")
            End If
            If a.Shape(1) <> b.Shape(0) Then
                Throw New ArgumentException($"矩阵维度不匹配: {a.Shape(1)} != {b.Shape(0)}")
            End If

            Dim m = a.Shape(0)
            Dim k = a.Shape(1)
            Dim n = b.Shape(1)

            If m * k * n < MatrixDotThreshold Then
                Return MyBase.MatMul(a, b)
            End If

            Dim rowsA = ToJagged(a.Data, m, k)
            Dim rowsB = ToJagged(b.Data, k, n)
            Dim product = SimdParallel.MatrixDot(rowsA, rowsB)

            Dim flat(m * n - 1) As Double
            For i As Integer = 0 To m - 1
                Call Array.Copy(product(i), 0, flat, i * n, n)
            Next

            Return Wrap(flat, New Integer() {m, n})
        End Function

        Private Shared Function ToJagged(flat As Double(), rows As Integer, cols As Integer) As Double()()
            Dim jagged(rows - 1)() As Double
            For i As Integer = 0 To rows - 1
                Dim row(cols - 1) As Double
                Call Array.Copy(flat, i * cols, row, 0, cols)
                jagged(i) = row
            Next
            Return jagged
        End Function

#End Region

#Region "归约运算"

        Public Overrides Function SumAll(t As Tensor) As Double
            Return SimdParallel.Sum(t.Data)
        End Function

        Public Overrides Function MeanAll(t As Tensor) As Double
            If t.Length = 0 Then Return 0
            Return SimdParallel.Sum(t.Data) / t.Length
        End Function

        Public Overrides Function Sum(t As Tensor, axis As Integer?) As Tensor
            If Not axis.HasValue Then Return Tensor.Scalar(SumAll(t))
            Return MyBase.Sum(t, axis)
        End Function

        Public Overrides Function Mean(t As Tensor, axis As Integer?) As Tensor
            If Not axis.HasValue Then Return Tensor.Scalar(MeanAll(t))
            Return MyBase.Mean(t, axis)
        End Function

        Public Overrides Function Max(t As Tensor, axis As Integer?) As Tensor
            If axis.HasValue OrElse t.Length = 0 Then Return MyBase.Max(t, axis)
            Return Tensor.Scalar(SimdParallel.Max(t.Data))
        End Function

        Public Overrides Function Min(t As Tensor, axis As Integer?) As Tensor
            If axis.HasValue OrElse t.Length = 0 Then Return MyBase.Min(t, axis)
            Return Tensor.Scalar(SimdParallel.Min(t.Data))
        End Function

        Public Overrides Function L2Norm(t As Tensor) As Double
            Return SimdParallel.L2Norm(t.Data)
        End Function

        Public Overrides Function StdDev(t As Tensor) As Double
            If t.Length = 0 Then Return 0

            Dim src = t.Data
            Dim mean = SimdParallel.Sum(src) / src.Length
            Dim centered = SimdEngine.SubtractScalar(Of Double)(src, mean)

            Return std.Sqrt(SimdParallel.SumSquares(centered) / src.Length)
        End Function

#End Region

    End Class

End Namespace

#Region "Microsoft.VisualBasic::8e1ff93e65c5dbbaa608a326cb7b97bb, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\SimdExtensions.vb"

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

    '   Total Lines: 355
    '    Code Lines: 171 (48.17%)
    ' Comment Lines: 136 (38.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 48 (13.52%)
    '     File Size: 11.35 KB


    '     Module SimdExtensions
    ' 
    '         Function: (+3 Overloads) SimdAbs, (+5 Overloads) SimdAdd, SimdAddScalar, SimdClamp, SimdDivide
    '                   SimdDivideScalar, (+2 Overloads) SimdDot, SimdL1Norm, SimdL2Norm, (+2 Overloads) SimdMax
    '                   (+2 Overloads) SimdMean, SimdMin, (+5 Overloads) SimdMultiply, SimdMultiplyScalar, (+2 Overloads) SimdNegate
    '                   SimdReciprocal, (+2 Overloads) SimdSqrt, SimdSquare, (+4 Overloads) SimdSubtract, (+2 Overloads) SimdSum
    '                   SimdSumSquares
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Math.SIMD

    ''' <summary>
    ''' 面向数组的 SIMD 加速扩展方法门面。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么方法名都带 <c>Simd</c> 前缀</b>：这个模块会被大量
    ''' <c>Imports Microsoft.VisualBasic.Math.SIMD</c> 的文件引用，如果直接扩展
    ''' <c>Sum</c> / <c>Min</c> / <c>Max</c> 这些名字，就会与
    ''' <c>Microsoft.VisualBasic.Linq</c> 以及 <see cref="System.Linq"/> 中的同名扩展
    ''' 产生二义性编译错误。加前缀之后既保留了链式调用的可读性，又不会污染既有代码。
    ''' </para>
    ''' <para>
    ''' 所有方法都会在数据规模足够大时自动切换到
    ''' <see cref="SimdParallel"/> 的分块并行路径，调用方不需要关心这个细节。
    ''' </para>
    ''' </remarks>
    Public Module SimdExtensions

#Region "reduce"

        ''' <summary>
        ''' 向量求和：<c>SUM(v)</c>
        ''' </summary>
        <Extension>
        Public Function SimdSum(v As Double()) As Double
            Return SimdParallel.Sum(v)
        End Function

        ''' <summary>
        ''' 向量求和：<c>SUM(v)</c>
        ''' </summary>
        <Extension>
        Public Function SimdSum(v As Single()) As Single
            Return SimdReduce.Sum(v)
        End Function

        ''' <summary>
        ''' 平方和：<c>SUM(v(i) ^ 2)</c>
        ''' </summary>
        <Extension>
        Public Function SimdSumSquares(v As Double()) As Double
            Return SimdParallel.SumSquares(v)
        End Function

        ''' <summary>
        ''' 均值：<c>SUM(v) / N</c>
        ''' </summary>
        <Extension>
        Public Function SimdMean(v As Double()) As Double
            Return SimdReduce.Mean(v)
        End Function

        ''' <summary>
        ''' 均值：<c>SUM(v) / N</c>
        ''' </summary>
        <Extension>
        Public Function SimdMean(v As Single()) As Single
            Return SimdReduce.Mean(v)
        End Function

        ''' <summary>
        ''' 最小值
        ''' </summary>
        <Extension>
        Public Function SimdMin(v As Double()) As Double
            Return SimdParallel.Min(v)
        End Function

        ''' <summary>
        ''' 最大值
        ''' </summary>
        <Extension>
        Public Function SimdMax(v As Double()) As Double
            Return SimdParallel.Max(v)
        End Function

        ''' <summary>
        ''' 最大值（<see cref="Single"/>）
        ''' </summary>
        <Extension>
        Public Function SimdMax(v As Single()) As Single
            Return SimdReduce.Max(v)
        End Function

        ''' <summary>
        ''' 点积：<c>SUM(v1(i) * v2(i))</c>，处理器支持 FMA 时会自动使用融合乘加。
        ''' </summary>
        <Extension>
        Public Function SimdDot(v1 As Double(), v2 As Double()) As Double
            Return SimdParallel.Dot(v1, v2)
        End Function

        ''' <summary>
        ''' 点积：<c>SUM(v1(i) * v2(i))</c>
        ''' </summary>
        <Extension>
        Public Function SimdDot(v1 As Single(), v2 As Single()) As Double
            Return SIMDIntrinsics.DotFma(v1, v2)
        End Function

        ''' <summary>
        ''' L1 范数：<c>SUM(|v(i)|)</c>
        ''' </summary>
        <Extension>
        Public Function SimdL1Norm(v As Double()) As Double
            Return SimdParallel.L1Norm(v)
        End Function

        ''' <summary>
        ''' L2 范数（欧几里得范数）：<c>SQRT(SUM(v(i) ^ 2))</c>
        ''' </summary>
        <Extension>
        Public Function SimdL2Norm(v As Double()) As Double
            Return SimdParallel.L2Norm(v)
        End Function

#End Region

#Region "math"

        ''' <summary>
        ''' 逐元素绝对值
        ''' </summary>
        <Extension>
        Public Function SimdAbs(v As Double()) As Double()
            Return SimdMath.Abs(Of Double)(v)
        End Function

        ''' <summary>
        ''' 逐元素绝对值（<see cref="Single"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAbs(v As Single()) As Single()
            Return SimdMath.Abs(Of Single)(v)
        End Function

        ''' <summary>
        ''' 逐元素绝对值（<see cref="Integer"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAbs(v As Integer()) As Integer()
            Return SimdMath.Abs(Of Integer)(v)
        End Function

        ''' <summary>
        ''' 逐元素取负
        ''' </summary>
        <Extension>
        Public Function SimdNegate(v As Double()) As Double()
            Return SimdMath.Negate(Of Double)(v)
        End Function

        ''' <summary>
        ''' 逐元素取负（<see cref="Long"/>）
        ''' </summary>
        <Extension>
        Public Function SimdNegate(v As Long()) As Long()
            Return SimdMath.Negate(Of Long)(v)
        End Function

        ''' <summary>
        ''' 逐元素平方
        ''' </summary>
        <Extension>
        Public Function SimdSquare(v As Double()) As Double()
            Return SimdMath.Square(Of Double)(v)
        End Function

        ''' <summary>
        ''' 逐元素平方根
        ''' </summary>
        <Extension>
        Public Function SimdSqrt(v As Double()) As Double()
            Return SimdMath.Sqrt(v)
        End Function

        ''' <summary>
        ''' 逐元素平方根（<see cref="Single"/>）
        ''' </summary>
        <Extension>
        Public Function SimdSqrt(v As Single()) As Single()
            Return SimdMath.Sqrt(v)
        End Function

        ''' <summary>
        ''' 逐元素倒数
        ''' </summary>
        <Extension>
        Public Function SimdReciprocal(v As Double()) As Double()
            Return SimdMath.Reciprocal(v)
        End Function

        ''' <summary>
        ''' 逐元素区间钳制
        ''' </summary>
        <Extension>
        Public Function SimdClamp(v As Double(), min As Double, max As Double) As Double()
            Return SimdMath.Clamp(Of Double)(v, min, max)
        End Function

#End Region

#Region "arithmetic"

        ''' <summary>
        ''' 逐元素相加
        ''' </summary>
        <Extension>
        Public Function SimdAdd(v1 As Double(), v2 As Double()) As Double()
            Return SimdParallel.Add(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相减
        ''' </summary>
        <Extension>
        Public Function SimdSubtract(v1 As Double(), v2 As Double()) As Double()
            Return SimdParallel.Subtract(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相乘
        ''' </summary>
        <Extension>
        Public Function SimdMultiply(v1 As Double(), v2 As Double()) As Double()
            Return SimdParallel.Multiply(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相除
        ''' </summary>
        <Extension>
        Public Function SimdDivide(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Divide(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加标量
        ''' </summary>
        <Extension>
        Public Function SimdAddScalar(v As Double(), scalar As Double) As Double()
            Return SimdEngine.AddScalar(Of Double)(v, scalar)
        End Function

        ''' <summary>
        ''' 向量乘标量
        ''' </summary>
        <Extension>
        Public Function SimdMultiplyScalar(v As Double(), scalar As Double) As Double()
            Return SimdParallel.MultiplyScalar(scalar, v)
        End Function

        ''' <summary>
        ''' 向量除以标量
        ''' </summary>
        <Extension>
        Public Function SimdDivideScalar(v As Double(), scalar As Double) As Double()
            Return SimdEngine.DivideScalar(v, scalar)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Single"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAdd(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Add(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相乘（<see cref="Single"/>）
        ''' </summary>
        <Extension>
        Public Function SimdMultiply(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Multiply(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Integer"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAdd(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Add(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相减（<see cref="Integer"/>）
        ''' </summary>
        <Extension>
        Public Function SimdSubtract(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Subtract(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相乘（<see cref="Integer"/>）
        ''' </summary>
        <Extension>
        Public Function SimdMultiply(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Multiply(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Long"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAdd(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Add(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相减（<see cref="Long"/>）
        ''' </summary>
        <Extension>
        Public Function SimdSubtract(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Subtract(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相乘（<see cref="Long"/>）
        ''' </summary>
        <Extension>
        Public Function SimdMultiply(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Multiply(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相加（<see cref="Short"/>）
        ''' </summary>
        <Extension>
        Public Function SimdAdd(v1 As Short(), v2 As Short()) As Short()
            Return SimdEngine.Add(Of Short)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相减（<see cref="Short"/>）
        ''' </summary>
        <Extension>
        Public Function SimdSubtract(v1 As Short(), v2 As Short()) As Short()
            Return SimdEngine.Subtract(Of Short)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素相乘（<see cref="Short"/>）
        ''' </summary>
        <Extension>
        Public Function SimdMultiply(v1 As Short(), v2 As Short()) As Short()
            Return SimdEngine.Multiply(Of Short)(v1, v2)
        End Function

#End Region
    End Module
End Namespace

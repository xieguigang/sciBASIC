#Region "Microsoft.VisualBasic::6782b5aaa2c98bfec1e1b73e3eb62d48, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Multiply.vb"

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

    '   Total Lines: 96
    '    Code Lines: 40 (41.67%)
    ' Comment Lines: 43 (44.79%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (13.54%)
    '     File Size: 3.94 KB


    '     Class Multiply
    ' 
    '         Function: f32_op_multiply_f32, f32_op_multiply_f32_scalar, f32_scalar_op_multiply_f32, f64_op_multiply_f64, f64_op_multiply_f64_scalar
    '                   f64_scalar_op_multiply_f64, int32_op_multiply_int32, int32_op_multiply_int32_scalar, int32_scalar_op_multiply_int32, int64_op_multiply_int64
    '                   int64_op_multiply_int64_scalar, int64_scalar_op_multiply_int64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素乘法。乘法满足交换律，因此标量与向量两种书写顺序的结果是等价的。
    ''' </summary>
    ''' <remarks>
    ''' 旧版本中这四个函数的 <c>enable</c> 分支实际上只是 <c>GoTo legacy</c> 的占位代码，
    ''' 完全没有真实的 AVX 路径；现在统一由 <see cref="SimdEngine"/> 提供跨平台向量化实现。
    ''' </remarks>
    Public Class Multiply

        ''' <summary>
        ''' 标量乘向量：<c>v1 * v2(i)</c>
        ''' </summary>
        Public Shared Function f32_scalar_op_multiply_f32(v1 As Single, v2 As Single()) As Single()
            Return SimdEngine.MultiplyScalar(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量乘标量：<c>v1(i) * v2</c>
        ''' </summary>
        Public Shared Function f32_op_multiply_f32_scalar(v1 As Single(), v2 As Single) As Single()
            Return SimdEngine.MultiplyScalar(Of Single)(v2, v1)
        End Function

        ''' <summary>
        ''' 向量乘向量：<c>v1(i) * v2(i)</c>
        ''' </summary>
        Public Shared Function f32_op_multiply_f32(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Multiply(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量乘向量：<c>v1 * v2(i)</c>
        ''' </summary>
        Public Shared Function f64_scalar_op_multiply_f64(v1 As Double, v2 As Double()) As Double()
            Return SimdEngine.MultiplyScalar(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量乘标量：<c>v1(i) * v2</c>
        ''' </summary>
        Public Shared Function f64_op_multiply_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Return SimdEngine.MultiplyScalar(Of Double)(v2, v1)
        End Function

        ''' <summary>
        ''' 向量乘向量：<c>v1(i) * v2(i)</c>
        ''' </summary>
        Public Shared Function f64_op_multiply_f64(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Multiply(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量乘向量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_scalar_op_multiply_int32(v1 As Integer, v2 As Integer()) As Integer()
            Return SimdEngine.MultiplyScalar(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量乘标量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_multiply_int32_scalar(v1 As Integer(), v2 As Integer) As Integer()
            Return SimdEngine.MultiplyScalar(Of Integer)(v2, v1)
        End Function

        ''' <summary>
        ''' 向量乘向量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_multiply_int32(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Multiply(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量乘向量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_scalar_op_multiply_int64(v1 As Long, v2 As Long()) As Long()
            Return SimdEngine.MultiplyScalar(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量乘标量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_multiply_int64_scalar(v1 As Long(), v2 As Long) As Long()
            Return SimdEngine.MultiplyScalar(Of Long)(v2, v1)
        End Function

        ''' <summary>
        ''' 向量乘向量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_multiply_int64(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Multiply(Of Long)(v1, v2)
        End Function
    End Class
End Namespace

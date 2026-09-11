#Region "Microsoft.VisualBasic::f638e11c520415f196826c30e23e1c84, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Subtract.vb"

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
    '     File Size: 3.90 KB


    '     Class Subtract
    ' 
    '         Function: f32_op_subtract_f32, f32_op_subtract_f32_scalar, f32_scalar_op_subtract_f32, f64_op_subtract_f64, f64_op_subtract_f64_scalar
    '                   f64_scalar_op_subtract_f64, int32_op_subtract_int32, int32_op_subtract_int32_scalar, int32_scalar_op_subtract_int32, int64_op_subtract_int64
    '                   int64_op_subtract_int64_scalar, int64_scalar_op_subtract_int64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素减法。注意标量与向量的顺序决定了结果的方向。
    ''' </summary>
    ''' <remarks>
    ''' 原有的三个 ``f64`` 函数在旧版本里是**纯标量循环**（完全没有向量化），
    ''' 这里改写为 <see cref="SimdEngine"/> 的薄封装之后才真正走上了 SIMD 路径。
    ''' </remarks>
    Public Class Subtract

        ''' <summary>
        ''' 标量减向量：<c>v1 - v2(i)</c>
        ''' </summary>
        Public Shared Function f64_scalar_op_subtract_f64(v1 As Double, v2 As Double()) As Double()
            Return SimdEngine.ScalarSubtract(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减标量：<c>v1(i) - v2</c>
        ''' </summary>
        Public Shared Function f64_op_subtract_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Return SimdEngine.SubtractScalar(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减向量：<c>v1(i) - v2(i)</c>
        ''' </summary>
        Public Shared Function f64_op_subtract_f64(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Subtract(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量减向量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_scalar_op_subtract_f32(v1 As Single, v2 As Single()) As Single()
            Return SimdEngine.ScalarSubtract(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减标量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_subtract_f32_scalar(v1 As Single(), v2 As Single) As Single()
            Return SimdEngine.SubtractScalar(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减向量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_subtract_f32(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Subtract(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量减向量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_scalar_op_subtract_int32(v1 As Integer, v2 As Integer()) As Integer()
            Return SimdEngine.ScalarSubtract(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减标量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_subtract_int32_scalar(v1 As Integer(), v2 As Integer) As Integer()
            Return SimdEngine.SubtractScalar(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减向量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_subtract_int32(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Subtract(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 标量减向量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_scalar_op_subtract_int64(v1 As Long, v2 As Long()) As Long()
            Return SimdEngine.ScalarSubtract(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减标量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_subtract_int64_scalar(v1 As Long(), v2 As Long) As Long()
            Return SimdEngine.SubtractScalar(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量减向量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_subtract_int64(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Subtract(Of Long)(v1, v2)
        End Function
    End Class
End Namespace

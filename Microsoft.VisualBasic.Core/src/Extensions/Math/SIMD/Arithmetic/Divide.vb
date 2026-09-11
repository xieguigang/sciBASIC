#Region "Microsoft.VisualBasic::simdDivide::Extensions\Math\SIMD\Arithmetic\Divide.vb"

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

    '     Class Divide

    '         Function: f32_op_divide_f32, f32_op_divide_f32_scalar, f32_scalar_op_divide_f32,
    '                   f64_op_divide_f64, f64_op_divide_f64_scalar, f64_scalar_op_divide_f64,
    '                   int32_op_divide_int32_scalar

    ' /********************************************************************************/

#End Region

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素除法。
    ''' </summary>
    ''' <remarks>
    ''' 整数除法没有对应的硬件指令（<c>vdivpd</c>/<c>vdivps</c> 只作用于浮点），
    ''' 因此 <see cref="int32_op_divide_int32_scalar(Integer(), Double)"/> 保持标量实现。
    ''' </remarks>
    Public Class Divide

        ''' <summary>
        ''' 标量除以向量：<c>v1 / v2(i)</c>
        ''' </summary>
        Public Shared Function f64_scalar_op_divide_f64(v1 As Double, v2 As Double()) As Double()
            Return SimdEngine.ScalarDivide(v1, v2)
        End Function

        ''' <summary>
        ''' 向量除以标量：<c>v1(i) / v2</c>
        ''' </summary>
        Public Shared Function f64_op_divide_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Return SimdEngine.DivideScalar(v1, v2)
        End Function

        ''' <summary>
        ''' 整数向量除以 <see cref="Double"/> 标量，结果提升为 <see cref="Double"/>。
        ''' </summary>
        ''' <remarks>
        ''' 这个函数没有向量化：把 <see cref="Integer"/> 逐元素提升为 <see cref="Double"/>
        ''' 再相除的转换开销会抵消掉向量化收益，当前调用点也不在热路径上。
        ''' </remarks>
        Public Shared Function int32_op_divide_int32_scalar(v1 As Integer(), v2 As Double) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim result As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) / v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量除以向量：<c>v1(i) / v2(i)</c>
        ''' </summary>
        ''' <remarks>
        ''' <b>语义提醒</b>：为了与历史实现保持一致，当分子 <c>v1(i) = 0</c> 时结果被直接置为
        ''' <c>0</c>（而不是让 <c>0 / 0</c> 产生 <see cref="Double.NaN"/>）。向量化实现通过
        ''' 掩码选择完成同样的语义。
        ''' </remarks>
        Public Shared Function f64_op_divide_f64(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.DivideZeroSafe(v1, v2)
        End Function

        ''' <summary>
        ''' 标量除以向量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_scalar_op_divide_f32(v1 As Single, v2 As Single()) As Single()
            Return SimdEngine.ScalarDivide(v1, v2)
        End Function

        ''' <summary>
        ''' 向量除以标量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_divide_f32_scalar(v1 As Single(), v2 As Single) As Single()
            Return SimdEngine.DivideScalar(v1, v2)
        End Function

        ''' <summary>
        ''' 向量除以向量（<see cref="Single"/>）。
        ''' </summary>
        ''' <remarks>
        ''' 这是一个新增的普通除法；如果同样需要“分子为零则结果为零”的历史语义，
        ''' 请使用 <see cref="SimdEngine.DivideZeroSafe(Double(), Double())"/>。
        ''' </remarks>
        Public Shared Function f32_op_divide_f32(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Divide(v1, v2)
        End Function
    End Class
End Namespace

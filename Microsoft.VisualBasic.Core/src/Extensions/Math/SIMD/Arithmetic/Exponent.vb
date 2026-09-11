#Region "Microsoft.VisualBasic::simdExponent::Extensions\Math\SIMD\Arithmetic\Exponent.vb"

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

    '     Class Exponent

    '         Function: f32_exp, f32_op_exponent_f32, f32_op_exponent_f32_scalar, f32_scalar_op_exponent_f32,
    '                   f64_exp, f64_op_exponent_f64, f64_op_exponent_f64_scalar, f64_scalar_op_exponent_f64

    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Math.SIMD

    ''' <summary>
    ''' 幂运算与自然指数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' VB 的 <c>^</c> 运算符会被编译成 <see cref="System.Math.Pow(Double, Double)"/>，
    ''' 而幂函数没有逐元素硬件指令，因此“指数也在变化”的重载依旧是标量实现。
    ''' </para>
    ''' <para>
    ''' 真正可被向量化的是**指数固定**的场景：当指数为 <c>2</c> 时可以直接用乘法
    ''' （<c>x * x</c> 与正确舍入的 <c>pow(x, 2)</c> 结果逐位相同）、指数为 <c>0.5</c>
    ''' 时可以用开方。这正是 <see cref="SimdMath.PowScalar(Double(), Double)"/> 所做的特化，
    ''' 也是 <c>SquareDistance</c> 这类热点（指数恒为 2）的主要加速来源。
    ''' </para>
    ''' <para>
    ''' 注意：指数为 <c>3</c> / <c>4</c> 的特化用的是连续乘法，与正确舍入的 <c>pow</c>
    ''' 相比可能相差几个 ULP（相对误差量级 <c>1e-16</c>）。
    ''' </para>
    ''' </remarks>
    Public Class Exponent

        ''' <summary>
        ''' 标量的向量次幂：<c>v1 ^ v2(i)</c>
        ''' </summary>
        Public Shared Function f64_scalar_op_exponent_f64(v1 As Double, v2 As Double()) As Double()
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v2.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim result As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1 ^ v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量的固定次幂：<c>v1(i) ^ v2</c>
        ''' </summary>
        ''' <remarks>
        ''' 指数为 2 / 3 / 4 / 0.5 时会走到向量化的快速路径，其余指数退回标量 <c>^</c>。
        ''' </remarks>
        Public Shared Function f64_op_exponent_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Return SimdMath.PowScalar(v1, v2)
        End Function

        ''' <summary>
        ''' 向量对向量的逐元素次幂：<c>v1(i) ^ v2(i)</c>
        ''' </summary>
        Public Shared Function f64_op_exponent_f64(v1 As Double(), v2 As Double()) As Double()
            Return SimdMath.Pow(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素自然指数：<c>Exp(v(i))</c>
        ''' </summary>
        Public Shared Function f64_exp(v As Double()) As Double()
            Return SimdMath.Exp(v)
        End Function

        ''' <summary>
        ''' 标量的向量次幂（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_scalar_op_exponent_f32(v1 As Single, v2 As Single()) As Single()
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v2.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim result As Single() = New Single(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1 ^ v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量的固定次幂（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_exponent_f32_scalar(v1 As Single(), v2 As Single) As Single()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            If v2 = 2.0F Then Return SimdMath.Square(Of Single)(v1)
            If v2 = 0.5F Then Return SimdMath.Sqrt(v1)

            Dim result As Single() = New Single(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) ^ v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对向量的逐元素次幂（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_exponent_f32(v1 As Single(), v2 As Single()) As Single()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim result As Single() = New Single(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) ^ v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 逐元素自然指数（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_exp(v As Single()) As Single()
            Return SimdMath.Exp(v)
        End Function
    End Class
End Namespace

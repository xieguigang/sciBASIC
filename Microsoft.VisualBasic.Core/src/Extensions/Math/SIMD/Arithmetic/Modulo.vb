#Region "Microsoft.VisualBasic::simdModulo::Extensions\Math\SIMD\Arithmetic\Modulo.vb"

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

    '     Class Modulo

    '         Function: f64_op_modulo_f64, f64_op_modulo_f64_scalar, f64_scalar_op_modulo_f64,
    '                   int32_op_modulo_int32, int32_op_modulo_int32_scalar, int32_scalar_op_modulo_int32,
    '                   int64_op_modulo_int64, int64_op_modulo_int64_scalar, int64_scalar_op_modulo_int64

    ' /********************************************************************************/

#End Region

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素取余。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么这里没有 SIMD 实现</b>：x86/ARM 的 SIMD 指令集都没有提供逐元素的
    ''' 整数除法/取余指令（除法的串行化特性使其无法被有效地向量化），浮点取余同样
    ''' 没有硬件支持。因此这个类型保持标量实现 —— 强行写一个“看起来是向量化”
    ''' 的版本只会比标量更慢。
    ''' </para>
    ''' <para>
    ''' <b>为什么不使用 <see cref="System.Math.IEEERemainder"/> 替换</b>：
    ''' VB 的 <c>Mod</c> 运算符与 <c>IEEERemainder</c> 在负数上的取整方向并不相同，
    ''' 替换会静默改变既有调用方的数值语义。
    ''' </para>
    ''' </remarks>
    Public Class Modulo

        ''' <summary>
        ''' 标量对向量取余：<c>v1 Mod v2(i)</c>
        ''' </summary>
        Public Shared Function f64_scalar_op_modulo_f64(v1 As Double, v2 As Double()) As Double()
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v2.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim result As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1 Mod v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对标量取余：<c>v1(i) Mod v2</c>
        ''' </summary>
        Public Shared Function f64_op_modulo_f64_scalar(v1 As Double(), v2 As Double) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim result As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对向量取余：<c>v1(i) Mod v2(i)</c>
        ''' </summary>
        Public Shared Function f64_op_modulo_f64(v1 As Double(), v2 As Double()) As Double()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim result As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 标量对向量取余（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_scalar_op_modulo_int32(v1 As Integer, v2 As Integer()) As Integer()
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v2.Length
            If len = 0 Then Return Array.Empty(Of Integer)()

            Dim result As Integer() = New Integer(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1 Mod v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对标量取余（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_modulo_int32_scalar(v1 As Integer(), v2 As Integer) As Integer()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Integer)()

            Dim result As Integer() = New Integer(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对向量取余（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_modulo_int32(v1 As Integer(), v2 As Integer()) As Integer()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Integer)()

            Dim result As Integer() = New Integer(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 标量对向量取余（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_scalar_op_modulo_int64(v1 As Long, v2 As Long()) As Long()
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))

            Dim len As Integer = v2.Length
            If len = 0 Then Return Array.Empty(Of Long)()

            Dim result As Long() = New Long(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1 Mod v2(i)
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对标量取余（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_modulo_int64_scalar(v1 As Long(), v2 As Long) As Long()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Long)()

            Dim result As Long() = New Long(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2
            Next

            Return result
        End Function

        ''' <summary>
        ''' 向量对向量取余（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_modulo_int64(v1 As Long(), v2 As Long()) As Long()
            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Long)()

            Dim result As Long() = New Long(len - 1) {}

            For i As Integer = 0 To len - 1
                result(i) = v1(i) Mod v2(i)
            Next

            Return result
        End Function
    End Class
End Namespace

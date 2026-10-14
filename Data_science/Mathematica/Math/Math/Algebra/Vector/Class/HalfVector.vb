#Region "Microsoft.VisualBasic::49b974652f41f31f5ed8db8743dc013e, Data_science\Mathematica\Math\Math\Algebra\Vector\Class\HalfVector.vb"

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

    '   Total Lines: 132
    '    Code Lines: 103 (78.03%)
    ' Comment Lines: 3 (2.27%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 26 (19.70%)
    '     File Size: 5.30 KB


    '     Class HalfVector
    ' 
    '         Properties: Mean, Sum
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: AsVector, Dot, Max, Min, Norm, ToSingle, ToString
    '         Operators: (+2 Overloads) -, (+4 Overloads) *, (+2 Overloads) /, (+3 Overloads) ^, (+3 Overloads) +
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Numerics
Imports Microsoft.VisualBasic.Math.Scripting.Rscript
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LinearAlgebra

    ''' <summary>
    ''' 精度比较低的半精度向量
    ''' </summary>
    ''' <remarks>
    ''' ## 底层类型
    ''' 
    ''' 本类型使用的是 BCL 的 <see cref="System.Half"/>（IEEE 754 binary16），
    ''' 而不是仓库历史版本里的 ``Microsoft.VisualBasic.Math.Numerics.Half``。
    ''' 后者是 2018 年从 .NET Framework 时代移植的实现，存在若干缺陷：
    ''' 
    ''' + 转换使用向零截断（truncate）而不是 round-to-nearest-even；
    ''' + Double 转 Half 时先塌到 Single，产生二次舍入；
    ''' + ±0 不相等、Max/Min 不传播 NaN、GetHashCode 与 Equals 契约冲突。
    ''' 
    ''' ## 计算路径
    ''' 
    ''' 所有的逐元素算子都调用 <see cref="HalfSimd"/> 的 8 路融合内核：
    ''' 在每个 128 位向量里一次处理 8 个 <see cref="System.Half"/>
    ''' （转换 → Single lane 运算 → 转回），不会产生中间的 ``Single()`` 数组；
    ''' 长度不足 8 的尾部走 <see cref="System.Half"/> 的标量路径，结果与向量路径逐位一致。
    ''' </remarks>
    Public Class HalfVector : Inherits GenericVector(Of Half)

        ''' <summary>
        ''' 所有元素之和（Double 精度累加）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Sum As Double
            Get
                Return HalfSimd.Sum(buffer)
            End Get
        End Property

        ''' <summary>
        ''' 所有元素的平均值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Mean As Double
            Get
                Return Sum / [Dim]
            End Get
        End Property

        ''' <summary>
        ''' 从 <see cref="System.Half"/> 数组直接构造（不产生拷贝）
        ''' </summary>
        ''' <param name="data">由 <see cref="HalfSimd"/> 等内核产生的结果数组</param>
        Sub New(data As Half())
            Call MyBase.New()

            buffer = data
        End Sub

        Sub New(data As IEnumerable(Of Half))
            Call MyBase.New(data)
        End Sub

        Sub New(values As IEnumerable(Of Double))
            Call MyBase.New(HalfSimd.ToHalf(values.ToArray))
        End Sub

        Sub New(values As IEnumerable(Of Single))
            Call MyBase.New(HalfSimd.ToHalf(values.ToArray))
        End Sub

        Sub New(value As Single, n As Integer)
            Call MyBase.New(CType(value, Half).Replicate(n))
        End Sub

        ''' <summary>
        ''' 把半精度向量批量转换为单精度向量（8 路转换内核）
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToSingle() As Single()
            Return HalfSimd.ToSingle(buffer)
        End Function

        ''' <summary>
        ''' 把单精度数组批量转换为半精度向量（8 路转换内核）
        ''' </summary>
        ''' <param name="values">单精度数组</param>
        ''' <returns></returns>
        Public Shared Function FromSingles(values As Single()) As HalfVector
            Return New HalfVector(HalfSimd.ToHalf(values))
        End Function

        ''' <summary>
        ''' 与另一个半精度向量做点积（Double 精度累加）
        ''' </summary>
        ''' <param name="other">另一个向量，维度必须相同</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Dot(other As HalfVector) As Double
            Return HalfSimd.Dot(buffer, other.buffer)
        End Function

        ''' <summary>
        ''' 欧几里得范数（L2 范数）
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Norm() As Double
            Return HalfSimd.Norm(buffer)
        End Function

        ''' <summary>
        ''' 向量中的最小元素
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Min() As Half
            Return HalfSimd.Min(buffer)
        End Function

        ''' <summary>
        ''' 向量中的最大元素
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Max() As Half
            Return HalfSimd.Max(buffer)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function AsVector() As Vector
            Return New Vector(HalfSimd.ToSingle(buffer))
        End Function

        Public Overrides Function ToString() As String
            Return buffer.Select(Function(n) CSng(n).ToString).GetJson
        End Function

        ' ------------------------------------------------------------
        ' 标量算子：全部走 8 路融合内核
        ' ------------------------------------------------------------

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(HalfSimd.AddScalar(v.buffer, CSng(add)))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(HalfSimd.AddScalar(v.buffer, add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v As HalfVector, add As Integer) As HalfVector
            Return New HalfVector(HalfSimd.AddScalar(v.buffer, CSng(add)))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator +(v1 As HalfVector, v2 As HalfVector) As HalfVector
            Return New HalfVector(HalfSimd.Add(v1.buffer, v2.buffer))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(HalfSimd.SubtractScalar(v.buffer, CSng(add)))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(HalfSimd.SubtractScalar(v.buffer, add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(v1 As HalfVector, v2 As HalfVector) As HalfVector
            Return New HalfVector(HalfSimd.Subtract(v1.buffer, v2.buffer))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, add As Double) As HalfVector
            Return New HalfVector(HalfSimd.MultiplyScalar(v.buffer, CSng(add)))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(multiply As Double, v As HalfVector) As HalfVector
            Return v * multiply
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(HalfSimd.MultiplyScalar(v.buffer, add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v1 As HalfVector, v2 As HalfVector) As HalfVector
            Return New HalfVector(HalfSimd.Multiply(v1.buffer, v2.buffer))
        End Operator

        ''' <summary>
        ''' 与单精度向量逐元素相乘，结果为单精度向量
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(v As HalfVector, multiple As Vector) As Vector
            If v.Dim <> multiple.Dim Then
                Throw New InvalidConstraintException($"the dimension size between two vector should be equals!")
            Else
                Dim a As Single() = HalfSimd.ToSingle(v.buffer)
                Dim out As Single() = GC.AllocateUninitializedArray(Of Single)(a.Length)

                For i As Integer = 0 To a.Length - 1
                    out(i) = a(i) * multiple(i)
                Next

                Return New Vector(out)
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v As HalfVector, div As Double) As HalfVector
            Return New HalfVector(HalfSimd.DivideScalar(v.buffer, CSng(div)))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(HalfSimd.DivideScalar(v.buffer, add))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator /(v1 As HalfVector, v2 As HalfVector) As HalfVector
            Return New HalfVector(HalfSimd.Divide(v1.buffer, v2.buffer))
        End Operator

        ' ------------------------------------------------------------
        ' 幂运算：没有对应的半精度 SIMD 指令，保持标量路径
        ' ------------------------------------------------------------

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(v As HalfVector, pow As Double) As HalfVector
            Return New HalfVector(Power(v.buffer, pow, baseIsVector:=True))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(base#, v As HalfVector) As HalfVector
            Return New HalfVector(Power(v.buffer, base, baseIsVector:=False))
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator ^(v As HalfVector, add As Single) As HalfVector
            Return New HalfVector(Power(v.buffer, add, baseIsVector:=True))
        End Operator

        ''' <summary>
        ''' 幂运算的标量内核：<c>baseIsVector=True</c> 时计算 <c>x ^ pow</c>，否则计算 <c>pow ^ x</c>
        ''' </summary>
        Private Shared Function Power(x As Half(), pow As Double, baseIsVector As Boolean) As Half()
            Dim out As Half() = GC.AllocateUninitializedArray(Of Half)(x.Length)

            If baseIsVector Then
                For i As Integer = 0 To x.Length - 1
                    out(i) = CType(Math.Pow(CSng(x(i)), pow), Half)
                Next
            Else
                For i As Integer = 0 To x.Length - 1
                    out(i) = CType(Math.Pow(pow, CSng(x(i))), Half)
                Next
            End If

            Return out
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::simdCompare::Extensions\Math\SIMD\Engine\SimdCompare.vb"

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

    '     Class SimdCompare

    '         Function: All, Any, CountTrue, Equal, GreaterThan, GreaterThanOrEqual, LaneTrue,
    '                   LessThan, LessThanOrEqual, NotEqual, Select, Where

    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports System.Runtime.CompilerServices

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素比较、掩码与条件选择。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="System.Numerics.Vector(Of T)"/> 的比较原语返回的是“掩码向量”而不是布尔数组，
    ''' 这里统一把掩码逐通道转换回 <see cref="Boolean"/>，以便与其他 .NET 代码自然互操作。
    ''' </remarks>
    Public NotInheritable Class SimdCompare

        Private Sub New()
        End Sub

#Region "comparison"

        ''' <summary>
        ''' 逐元素大于：<c>out(i) = v1(i) &gt; v2(i)</c>
        ''' </summary>
        Public Shared Function GreaterThan(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.GreaterThan(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素小于：<c>out(i) = v1(i) &lt; v2(i)</c>
        ''' </summary>
        Public Shared Function LessThan(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.LessThan(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素大于等于。
        ''' </summary>
        Public Shared Function GreaterThanOrEqual(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.GreaterThanOrEqual(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素小于等于。
        ''' </summary>
        Public Shared Function LessThanOrEqual(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.LessThanOrEqual(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素相等比较。
        ''' </summary>
        Public Shared Function Equal(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.Equals(Of T)(a, b))
        End Function

        ''' <summary>
        ''' 逐元素不等比较。
        ''' </summary>
        Public Shared Function NotEqual(Of T As Structure)(v1 As T(), v2 As T()) As Boolean()
            Return Compare(Of T)(v1, v2, Function(a, b) Vector.OnesComplement(Vector.Equals(Of T)(a, b)))
        End Function

        Private Shared Function Compare(Of T As Structure)(v1 As T(), v2 As T(),
                                                           op As SimdEngine.VectorBinaryOp(Of T)) As Boolean()

            If v1 Is Nothing Then Throw New ArgumentNullException(NameOf(v1))
            If v2 Is Nothing Then Throw New ArgumentNullException(NameOf(v2))
            If v1.Length <> v2.Length Then
                Throw New ArgumentException($"the length of the two vectors not agree: {v1.Length} vs {v2.Length}!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of Boolean)()

            Dim out As Boolean() = New Boolean(len - 1) {}
            Dim count As Integer = Vector(Of T).Count
            Dim i As Integer = 0

            If SimdEngine.CanVectorize(Of T)(len) Then
                Dim last As Integer = len - count

                Do While i <= last
                    Call CompareBlock(Of T)(v1, v2, out, i, op)
                    i += count
                Loop
                If i < len Then
                    Call CompareBlock(Of T)(v1, v2, out, last, op)
                End If

                Return out
            End If

            Do While i < len
                out(i) = LaneTrue(Of T)(op(New Vector(Of T)(v1(i)), New Vector(Of T)(v2(i))), 0)
                i += 1
            Loop

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Sub CompareBlock(Of T As Structure)(v1 As T(), v2 As T(), out As Boolean(),
                                                           offset As Integer,
                                                           op As SimdEngine.VectorBinaryOp(Of T))
            Dim mask As Vector(Of T) = op(New Vector(Of T)(v1, offset), New Vector(Of T)(v2, offset))
            Dim count As Integer = Vector(Of T).Count

            For k As Integer = 0 To count - 1
                out(offset + k) = LaneTrue(Of T)(mask, k)
            Next
        End Sub

        ''' <summary>
        ''' 判断掩码向量的某个通道是否为真。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 掩码通道的取值只有两种可能：**全 1** 或者**全 0**（这是 SIMD 比较原语的约定：
        ''' 浮点类型下“全 1”位模式读出 <c>NaN</c>，整数类型下读出 <c>-1</c>）。
        ''' 因此只需要检查该通道覆盖的<b>第一个字节</b>就能判定真假：全 1 时是
        ''' <c>0xFF</c>，全 0 时是 <c>0x00</c>，不存在歧义，而且每次判定只需要一次
        ''' 通道提取。
        ''' </para>
        ''' <para>
        ''' <b>为什么不能重解释成固定的整数向量</b>：<c>Vector(Of Integer)</c> 的通道宽度是
        ''' 32 位，而 <see cref="Double"/> 的通道是 64 位，两者并不是一一对应的关系
        ''' （一个 double 通道会跨越两个 32 位通道）。按字节定位则对所有元素宽度
        ''' （<see cref="Short"/>/<see cref="Integer"/>/<see cref="Long"/>/<see cref="Single"/>/<see cref="Double"/>）
        ''' 都成立，因为任何 <c>Vector(Of T)</c> 都铺满整个向量寄存器，
        ''' 总字节数恒等，所以这种按位重解释在长度上总是安全的。
        ''' </para>
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function LaneTrue(Of T As Structure)(mask As Vector(Of T), lane As Integer) As Boolean
            Dim bytes As Vector(Of Byte) = Unsafe.As(Of Vector(Of T), Vector(Of Byte))(mask)
            Dim size As Integer = Vector(Of Byte).Count \ Vector(Of T).Count

            Return bytes.GetElement(lane * size) <> 0
        End Function

#End Region

#Region "selection"

        ''' <summary>
        ''' 按掩码在两组数据之间做条件选择：
        ''' <c>out(i) = If(mask(i), trueValues(i), falseValues(i))</c>
        ''' </summary>
        Public Shared Function [Select](mask As Boolean(), trueValues As Double(), falseValues As Double()) As Double()
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))
            If trueValues Is Nothing Then Throw New ArgumentNullException(NameOf(trueValues))
            If falseValues Is Nothing Then Throw New ArgumentNullException(NameOf(falseValues))

            Dim len As Integer = mask.Length
            If len = 0 Then Return Array.Empty(Of Double)()

            Dim out As Double() = SimdEngine.NewArray(Of Double)(len)
            Dim count As Integer = Vector(Of Double).Count
            Dim zero As Vector(Of Double) = Vector(Of Double).Zero
            Dim i As Integer = 0

            If SimdEngine.CanVectorize(Of Double)(len) Then
                ' 把 Boolean 掩码预先铺成 0.0/1.0，之后即可用一次向量比较得到掩码向量
                Dim staged As Double() = SimdEngine.NewArray(Of Double)(len)

                For k As Integer = 0 To len - 1
                    staged(k) = If(mask(k), 1.0, 0.0)
                Next

                Dim last As Integer = len - count

                Do While i <= last
                    Dim flags As Vector(Of Double) = Vector.GreaterThan(Of Double)(New Vector(Of Double)(staged, i), zero)

                    Vector.ConditionalSelect(Of Double)(
                        flags,
                        New Vector(Of Double)(trueValues, i),
                        New Vector(Of Double)(falseValues, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Dim flags As Vector(Of Double) = Vector.GreaterThan(Of Double)(New Vector(Of Double)(staged, last), zero)

                    Vector.ConditionalSelect(Of Double)(
                        flags,
                        New Vector(Of Double)(trueValues, last),
                        New Vector(Of Double)(falseValues, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = If(mask(i), trueValues(i), falseValues(i))
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 按掩码在两组数据之间做条件选择（<see cref="Single"/>）。
        ''' </summary>
        Public Shared Function [Select](mask As Boolean(), trueValues As Single(), falseValues As Single()) As Single()
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))
            If trueValues Is Nothing Then Throw New ArgumentNullException(NameOf(trueValues))
            If falseValues Is Nothing Then Throw New ArgumentNullException(NameOf(falseValues))

            Dim len As Integer = mask.Length
            If len = 0 Then Return Array.Empty(Of Single)()

            Dim out As Single() = SimdEngine.NewArray(Of Single)(len)
            Dim count As Integer = Vector(Of Single).Count
            Dim zero As Vector(Of Single) = Vector(Of Single).Zero
            Dim i As Integer = 0

            If SimdEngine.CanVectorize(Of Single)(len) Then
                Dim staged As Single() = SimdEngine.NewArray(Of Single)(len)

                For k As Integer = 0 To len - 1
                    staged(k) = If(mask(k), 1.0F, 0.0F)
                Next

                Dim last As Integer = len - count

                Do While i <= last
                    Dim flags As Vector(Of Single) = Vector.GreaterThan(Of Single)(New Vector(Of Single)(staged, i), zero)

                    Vector.ConditionalSelect(Of Single)(
                        flags,
                        New Vector(Of Single)(trueValues, i),
                        New Vector(Of Single)(falseValues, i)).CopyTo(out, i)
                    i += count
                Loop
                If i < len Then
                    Dim flags As Vector(Of Single) = Vector.GreaterThan(Of Single)(New Vector(Of Single)(staged, last), zero)

                    Vector.ConditionalSelect(Of Single)(
                        flags,
                        New Vector(Of Single)(trueValues, last),
                        New Vector(Of Single)(falseValues, last)).CopyTo(out, last)
                End If

                Return out
            End If

            Do While i < len
                out(i) = If(mask(i), trueValues(i), falseValues(i))
                i += 1
            Loop

            Return out
        End Function

        ''' <summary>
        ''' 取出掩码为真的元素（压缩）。
        ''' </summary>
        ''' <remarks>
        ''' 元素压缩在 AVX-512 之前没有高效的硬件指令，这里保持顺序标量实现。
        ''' </remarks>
        Public Shared Function Where(mask As Boolean(), v As Double()) As Double()
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))
            If v Is Nothing Then Throw New ArgumentNullException(NameOf(v))

            Dim len As Integer = mask.Length
            If len <> v.Length Then
                Throw New ArgumentException($"the length of the mask and the value vector not agree: {len} vs {v.Length}!")
            End If

            Dim buffer As New List(Of Double)

            For i As Integer = 0 To len - 1
                If mask(i) Then
                    Call buffer.Add(v(i))
                End If
            Next

            Return buffer.ToArray
        End Function

#End Region

#Region "mask statistics"

        ''' <summary>
        ''' 掩码中是否存在任何为真的元素。
        ''' </summary>
        Public Shared Function Any(mask As Boolean()) As Boolean
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))

            For i As Integer = 0 To mask.Length - 1
                If mask(i) Then Return True
            Next

            Return False
        End Function

        ''' <summary>
        ''' 掩码是否全部为真（空掩码返回 True）。
        ''' </summary>
        Public Shared Function All(mask As Boolean()) As Boolean
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))

            For i As Integer = 0 To mask.Length - 1
                If Not mask(i) Then Return False
            Next

            Return True
        End Function

        ''' <summary>
        ''' 统计掩码中为真的元素个数。
        ''' </summary>
        Public Shared Function CountTrue(mask As Boolean()) As Integer
            If mask Is Nothing Then Throw New ArgumentNullException(NameOf(mask))

            Dim n As Integer = 0

            For i As Integer = 0 To mask.Length - 1
                If mask(i) Then n += 1
            Next

            Return n
        End Function

#End Region
    End Class
End Namespace

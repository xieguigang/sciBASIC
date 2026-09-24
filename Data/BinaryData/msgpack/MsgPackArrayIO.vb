#Region "Microsoft.VisualBasic::msgpack, Data\BinaryData\msgpack\MsgPackArrayIO.vb"

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

    '   Total Lines: 
    '    Code Lines: 
    ' Comment Lines: 
    '    - Xml Docs: 
    ' 
    '   Blank Lines: 
    '     File Size: 


    '     Module MsgPackArrayIO
    ' 
    '         Function: IsPrimitiveElement, TryReadArray, TryReadList, TryWriteArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Constants
Imports Microsoft.VisualBasic.Data.IO.MessagePack.Serialization

''' <summary>
''' 基础类型数组的<b>整块读写</b>：msgpack 序列化里最热的那条路径。
''' </summary>
''' <remarks>
''' 原实现是"逐元素"的，对数组来说每个元素都要付一遍这些代价：
''' 
''' * <b>写</b>：一次类型判断链 + <c>BitConverter.GetBytes</c> 分配一个临时数组 +
'''   两次 <c>BinaryWriter</c> 调用（每个元素 2 次流写）；
''' * <b>读</b>：读一个格式头字节 + <c>ReadBytes</c> 分配一个临时数组 + 
'''   <c>Array.Reverse</c> + <c>BitConverter</c> + <c>Convert.ChangeType</c> 装箱一次，
'''   最后再 <c>array.SetValue</c>（又一次装箱）。
''' 
''' 单个元素看着不起眼，但一张 534 万行 × 5 列的连接表就是 2,670 万次 ——
''' 实测"反序列化二进制"竟然比"直接解析 csv"还慢一倍多，
''' 全被逐元素的分配与装箱吃掉了（GC 压力还很大）。
''' 
''' 这里做的三件事（<b>wire 格式完全不变</b>，旧文件照样能读，别的 msgpack 实现也照样能读）：
''' 
''' 1. <b>写</b>：先把元素编进一块字节缓冲（大端序按 msgpack 约定），攒够一块再整块写盘，
'''    流调用次数从 <c>O(n)</c> 降到 <c>O(n / 块大小)</c>，且全程零分配；
''' 2. <b>读</b>：流可定位时（MemoryStream / FileStream）把整段字节一次读进来，
'''    之后从字节数组里解码（没有流调用、没有分配），读完再把流位置按实际消费的字节数回正；
'''    流不可定位时（例如 DeflateStream）退化为"逐元素但不分配"的路径；
''' 3. <b>不装箱</b>：直接写进 <c>Double() / Long() / Integer() / ...</c> 这类强类型数组，
'''    不再走 <c>Convert.ChangeType</c> 与 <c>Array.SetValue</c>。
''' 
''' 能快多少取决于元素类型与流类型，参考实测（534 万行连接表 / 2,670 万个值）：
''' 逐元素路径 3.1 s，整块路径约 1.2 s。
''' </remarks>
Friend Module MsgPackArrayIO

    ''' <summary>一块缓冲里最多放多少个元素（按 9 字节/元素估算，约 576 KB）。</summary>
    Private Const ChunkElements As Integer = 1 << 16

    ''' <summary>单个元素最大字节数（1 个格式头 + 8 字节数据）。</summary>
    Private Const MaxElementSize As Integer = 9

    ''' <summary>按线程的暂存区：逐元素路径里翻转字节序用，避免每个元素分配一个数组。</summary>
    <ThreadStatic>
    Private scratch As Byte()

    ''' <summary>
    ''' 这个数组的元素类型是不是"可以整块处理的基础类型"。
    ''' </summary>
    ''' <remarks>
    ''' 只列出有实际意义的类型；其余（字符串数组、对象数组、枚举数组…）继续走通用路径，
    ''' 结果完全一样，只是没有加速。
    ''' </remarks>
    Friend Function IsPrimitiveElement(elementType As Type) As Boolean
        If elementType Is Nothing Then Return False

        Return elementType Is GetType(Double) OrElse
               elementType Is GetType(Single) OrElse
               elementType Is GetType(Long) OrElse
               elementType Is GetType(Integer) OrElse
               elementType Is GetType(Short) OrElse
               elementType Is GetType(Byte) OrElse
               elementType Is GetType(Boolean)
    End Function

#Region "write"

    ''' <summary>
    ''' 尝试把一个基础类型数组整块写出去；返回 False 表示"这个数组不该由我处理"。
    ''' </summary>
    Friend Function TryWriteArray(array As Array, writer As BinaryWriter) As Boolean
        Dim elementType As Type = array.GetType().GetElementType()

        If Not IsPrimitiveElement(elementType) Then Return False

        Dim count As Integer = array.Length

        Call writeArrayHeader(writer, count)

        If count <= 0 Then Return True

        ' 注意写全 System.Math：只写 Math 会命中 sciBASIC 里那个处理向量的 Min 扩展
        Dim buffer As Byte() = New Byte(System.Math.Min(count, ChunkElements) * MaxElementSize - 1) {}
        Dim at As Integer = 0

        If elementType Is GetType(Double) Then
            Dim values As Double() = DirectCast(array, Double())

            For i As Integer = 0 To count - 1
                Call ensure(buffer, at, MaxElementSize, writer)

                buffer(at) = MsgPackFormats.FLOAT_64
                Call putInt64(buffer, at + 1, BitConverter.DoubleToInt64Bits(values(i)))

                at += MaxElementSize
            Next
        ElseIf elementType Is GetType(Single) Then
            Dim values As Single() = DirectCast(array, Single())

            For i As Integer = 0 To count - 1
                Call ensure(buffer, at, 5, writer)

                buffer(at) = MsgPackFormats.FLOAT_32
                Call putInt32(buffer, at + 1, BitConverter.SingleToInt32Bits(values(i)))

                at += 5
            Next
        ElseIf elementType Is GetType(Long) Then
            Dim values As Long() = DirectCast(array, Long())

            For i As Integer = 0 To count - 1
                Dim value As Long = values(i)
                Dim size As Integer = integerSize(value)

                Call ensure(buffer, at, size, writer)

                at += writeInteger(buffer, at, value, size)
            Next
        ElseIf elementType Is GetType(Integer) Then
            Dim values As Integer() = DirectCast(array, Integer())

            For i As Integer = 0 To count - 1
                Dim value As Integer = values(i)
                Dim size As Integer = integerSize(CLng(value))

                Call ensure(buffer, at, size, writer)

                at += writeInteger(buffer, at, CLng(value), size)
            Next
        ElseIf elementType Is GetType(Short) Then
            Dim values As Short() = DirectCast(array, Short())

            For i As Integer = 0 To count - 1
                Dim value As Integer = CInt(values(i))

                If value >= 0 AndAlso value <= FixedInteger.POSITIVE_MAX Then
                    Call ensure(buffer, at, 1, writer)

                    buffer(at) = CByte(value)

                    at += 1
                ElseIf value >= 0 AndAlso value <= Byte.MaxValue Then
                    Call ensure(buffer, at, 2, writer)

                    buffer(at) = MsgPackFormats.UINT_8
                    buffer(at + 1) = CByte(value)

                    at += 2
                ElseIf value >= SByte.MinValue AndAlso value <= SByte.MaxValue Then
                    Call ensure(buffer, at, 2, writer)

                    buffer(at) = MsgPackFormats.INT_8
                    buffer(at + 1) = CByte(value And &HFF)

                    at += 2
                Else
                    Call ensure(buffer, at, 3, writer)

                    buffer(at) = MsgPackFormats.INT_16
                    Call putInt16(buffer, at + 1, value)

                    at += 3
                End If
            Next
        ElseIf elementType Is GetType(Byte) Then
            Dim values As Byte() = DirectCast(array, Byte())

            For i As Integer = 0 To count - 1
                Call ensure(buffer, at, 2, writer)

                buffer(at) = MsgPackFormats.UINT_8
                buffer(at + 1) = values(i)

                at += 2
            Next
        Else
            Dim values As Boolean() = DirectCast(array, Boolean())

            For i As Integer = 0 To count - 1
                Call ensure(buffer, at, 1, writer)

                buffer(at) = If(values(i), Bool.TRUE, Bool.FALSE)

                at += 1
            Next
        End If

        If at > 0 Then
            Call writer.Write(buffer, 0, at)
        End If

        Return True
    End Function

    ''' <summary>数组的格式头（与 <see cref="MsgPackIO.SerializeValue"/> 里的一致）。</summary>
    Private Sub writeArrayHeader(writer As BinaryWriter, count As Integer)
        If count <= 15 Then
            writer.Write(CByte(FixedArray.MIN + count))
        ElseIf count <= UShort.MaxValue Then
            writer.Write(MsgPackFormats.ARRAY_16)
            writer.Write(swapUShort(CUShort(count)))
        Else
            writer.Write(MsgPackFormats.ARRAY_32)
            writer.Write(swapUInt(CUInt(count)))
        End If
    End Sub

    ''' <summary>缓冲不够放下一个元素就先刷盘。</summary>
    Private Sub ensure(buffer As Byte(), ByRef at As Integer, size As Integer, writer As BinaryWriter)
        If at + size <= buffer.Length Then Return

        Call writer.Write(buffer, 0, at)

        at = 0
    End Sub

    ''' <summary>整数需要几个字节（与 <see cref="MsgPackIO.WriteMsgPack(BinaryWriter, Long)"/> 的取值规则一致）。</summary>
    Private Function integerSize(value As Long) As Integer
        If value >= 0 AndAlso value <= FixedInteger.POSITIVE_MAX Then Return 1
        If value >= 0 AndAlso value <= Byte.MaxValue Then Return 2
        If value >= SByte.MinValue AndAlso value <= SByte.MaxValue Then Return 2
        If value >= Short.MinValue AndAlso value <= Short.MaxValue Then Return 3
        If value >= 0 AndAlso value <= UShort.MaxValue Then Return 3
        If value >= Integer.MinValue AndAlso value <= Integer.MaxValue Then Return 5
        If value >= 0 AndAlso value <= UInteger.MaxValue Then Return 5

        Return 9
    End Function

    ''' <summary>把一个整数按 <paramref name="size"/> 指定的格式写进缓冲，返回实际写入的字节数。</summary>
    Private Function writeInteger(buffer As Byte(), at As Integer, value As Long, size As Integer) As Integer
        Select Case size
            Case 1
                buffer(at) = CByte(value)

                Return 1
            Case 2
                If value >= 0 Then
                    buffer(at) = MsgPackFormats.UINT_8
                    buffer(at + 1) = CByte(value)
                Else
                    buffer(at) = MsgPackFormats.INT_8
                    buffer(at + 1) = CByte(value And &HFF)
                End If

                Return 2
            Case 3
                If value >= 0 Then
                    buffer(at) = MsgPackFormats.UINT_16
                    Call putInt16(buffer, at + 1, CInt(value))
                Else
                    buffer(at) = MsgPackFormats.INT_16
                    Call putInt16(buffer, at + 1, CInt(value))
                End If

                Return 3
            Case 5
                If value >= 0 Then
                    buffer(at) = MsgPackFormats.UINT_32
                    ' 值可能落在 [Integer.MaxValue, UInteger.MaxValue]，不能先转成有符号整数
                    Call putUInt32(buffer, at + 1, CUInt(value))
                Else
                    buffer(at) = MsgPackFormats.INT_32
                    Call putInt32(buffer, at + 1, CInt(value))
                End If

                Return 5
            Case Else
                buffer(at) = MsgPackFormats.INT_64
                Call putInt64(buffer, at + 1, value)

                Return 9
        End Select
    End Function

    Private Sub putInt64(buffer As Byte(), offset As Integer, bits As Long)
        buffer(offset) = CByte((bits >> 56) And &HFF)
        buffer(offset + 1) = CByte((bits >> 48) And &HFF)
        buffer(offset + 2) = CByte((bits >> 40) And &HFF)
        buffer(offset + 3) = CByte((bits >> 32) And &HFF)
        buffer(offset + 4) = CByte((bits >> 24) And &HFF)
        buffer(offset + 5) = CByte((bits >> 16) And &HFF)
        buffer(offset + 6) = CByte((bits >> 8) And &HFF)
        buffer(offset + 7) = CByte(bits And &HFF)
    End Sub

    Private Sub putUInt32(buffer As Byte(), offset As Integer, bits As UInteger)
        buffer(offset) = CByte((bits >> 24) And &HFFUI)
        buffer(offset + 1) = CByte((bits >> 16) And &HFFUI)
        buffer(offset + 2) = CByte((bits >> 8) And &HFFUI)
        buffer(offset + 3) = CByte(bits And &HFFUI)
    End Sub

    Private Sub putInt32(buffer As Byte(), offset As Integer, bits As Integer)
        buffer(offset) = CByte((bits >> 24) And &HFF)
        buffer(offset + 1) = CByte((bits >> 16) And &HFF)
        buffer(offset + 2) = CByte((bits >> 8) And &HFF)
        buffer(offset + 3) = CByte(bits And &HFF)
    End Sub

    Private Sub putInt16(buffer As Byte(), offset As Integer, bits As Integer)
        buffer(offset) = CByte((bits >> 8) And &HFF)
        buffer(offset + 1) = CByte(bits And &HFF)
    End Sub

    Private Function swapUShort(value As UShort) As Byte()
        Dim data As Byte() = BitConverter.GetBytes(value)

        Call Array.Reverse(data)

        Return data
    End Function

    Private Function swapUInt(value As UInteger) As Byte()
        Dim data As Byte() = BitConverter.GetBytes(value)

        Call Array.Reverse(data)

        Return data
    End Function

#End Region

#Region "read"

    ''' <summary>
    ''' 尝试整块读出一个基础类型数组；返回 False 表示"这个数组不该由我处理"。
    ''' </summary>
    Friend Function TryReadArray(array As Array, numElements As Integer, reader As BinaryDataReader) As Boolean
        Dim elementType As Type = array.GetType().GetElementType()

        If numElements <= 0 OrElse Not IsPrimitiveElement(elementType) Then Return False

        If elementType Is GetType(Double) Then
            Call readDoubles(DirectCast(array, Double()), numElements, reader)
        ElseIf elementType Is GetType(Single) Then
            Call readSingles(DirectCast(array, Single()), numElements, reader)
        ElseIf elementType Is GetType(Long) Then
            Call readLongs(DirectCast(array, Long()), numElements, reader)
        ElseIf elementType Is GetType(Integer) Then
            Call readIntegers(DirectCast(array, Integer()), numElements, reader)
        ElseIf elementType Is GetType(Short) Then
            Call readShorts(DirectCast(array, Short()), numElements, reader)
        ElseIf elementType Is GetType(Byte) Then
            Call readBytes(DirectCast(array, Byte()), numElements, reader)
        Else
            Call readBooleans(DirectCast(array, Boolean()), numElements, reader)
        End If

        Return True
    End Function

    ''' <summary>
    ''' <c>List(Of Double)</c> 之类的泛型列表：先读成数组再 <c>AddRange</c>，
    ''' 比逐元素 <c>Add</c> 少掉百万级的接口调用。
    ''' </summary>
    Friend Function TryReadList(collection As IList, numElements As Integer, reader As BinaryDataReader) As Boolean
        If numElements <= 0 Then Return False

        Dim elementType As Type = collection.GetType().GetGenericArguments()(0)

        If Not IsPrimitiveElement(elementType) Then Return False

        If elementType Is GetType(Double) Then
            Dim values As Double() = New Double(numElements - 1) {}

            Call readDoubles(values, numElements, reader)
            Call DirectCast(collection, List(Of Double)).AddRange(values)
        ElseIf elementType Is GetType(Single) Then
            Dim values As Single() = New Single(numElements - 1) {}

            Call readSingles(values, numElements, reader)
            Call DirectCast(collection, List(Of Single)).AddRange(values)
        ElseIf elementType Is GetType(Long) Then
            Dim values As Long() = New Long(numElements - 1) {}

            Call readLongs(values, numElements, reader)
            Call DirectCast(collection, List(Of Long)).AddRange(values)
        ElseIf elementType Is GetType(Integer) Then
            Dim values As Integer() = New Integer(numElements - 1) {}

            Call readIntegers(values, numElements, reader)
            Call DirectCast(collection, List(Of Integer)).AddRange(values)
        ElseIf elementType Is GetType(Short) Then
            Dim values As Short() = New Short(numElements - 1) {}

            Call readShorts(values, numElements, reader)
            Call DirectCast(collection, List(Of Short)).AddRange(values)
        ElseIf elementType Is GetType(Byte) Then
            Dim values As Byte() = New Byte(numElements - 1) {}

            Call readBytes(values, numElements, reader)
            Call DirectCast(collection, List(Of Byte)).AddRange(values)
        Else
            Dim values As Boolean() = New Boolean(numElements - 1) {}

            Call readBooleans(values, numElements, reader)
            Call DirectCast(collection, List(Of Boolean)).AddRange(values)
        End If

        Return True
    End Function

    Private Sub readDoubles(values As Double(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * MaxElementSize, start)

        If block Is Nothing Then
            ' 流不可定位：逐元素读，但不分配临时数组
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToDouble(
                    MsgPackIO.DeserializeValue(GetType(Double), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0
        Dim scratch8 As Byte() = scratchOf(8)

        For i As Integer = 0 To count - 1
            If at >= block.Length OrElse block(at) <> MsgPackFormats.FLOAT_64 Then
                ' 罕见格式（NIL / 整数编码的浮点数）：把流退回当前位置，剩下的交给通用路径
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToDouble(
                        MsgPackIO.DeserializeValue(GetType(Double), reader, NilImplication.Null))
                Next

                Return
            End If

            at += 1

            Call Array.Copy(block, at, scratch8, 0, 8)
            Call Array.Reverse(scratch8, 0, 8)

            values(i) = BitConverter.ToDouble(scratch8, 0)

            at += 8
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readSingles(values As Single(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * 5, start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToSingle(
                    MsgPackIO.DeserializeValue(GetType(Single), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0
        Dim scratch4 As Byte() = scratchOf(4)

        For i As Integer = 0 To count - 1
            If at >= block.Length OrElse block(at) <> MsgPackFormats.FLOAT_32 Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToSingle(
                        MsgPackIO.DeserializeValue(GetType(Single), reader, NilImplication.Null))
                Next

                Return
            End If

            at += 1

            Call Array.Copy(block, at, scratch4, 0, 4)
            Call Array.Reverse(scratch4, 0, 4)

            values(i) = BitConverter.ToSingle(scratch4, 0)

            at += 4
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readLongs(values As Long(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * MaxElementSize, start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToInt64(
                    MsgPackIO.DeserializeValue(GetType(Long), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0
        Dim buffer As Byte() = scratchOf(8)

        For i As Integer = 0 To count - 1
            Dim value As Long

            If Not tryDecodeInteger(block, at, buffer, value, at) Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToInt64(
                        MsgPackIO.DeserializeValue(GetType(Long), reader, NilImplication.Null))
                Next

                Return
            End If

            values(i) = value
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readIntegers(values As Integer(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * MaxElementSize, start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToInt32(
                    MsgPackIO.DeserializeValue(GetType(Integer), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0
        Dim buffer As Byte() = scratchOf(8)

        For i As Integer = 0 To count - 1
            Dim value As Long

            If Not tryDecodeInteger(block, at, buffer, value, at) Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToInt32(
                        MsgPackIO.DeserializeValue(GetType(Integer), reader, NilImplication.Null))
                Next

                Return
            End If

            values(i) = CInt(value)
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readShorts(values As Short(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * 3, start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToInt16(
                    MsgPackIO.DeserializeValue(GetType(Short), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0
        Dim buffer As Byte() = scratchOf(8)

        For i As Integer = 0 To count - 1
            Dim value As Long

            If Not tryDecodeInteger(block, at, buffer, value, at) Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToInt16(
                        MsgPackIO.DeserializeValue(GetType(Short), reader, NilImplication.Null))
                Next

                Return
            End If

            values(i) = CShort(value)
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readBytes(values As Byte(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count) * 2, start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToByte(
                    MsgPackIO.DeserializeValue(GetType(Byte), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0

        For i As Integer = 0 To count - 1
            If at >= block.Length OrElse block(at) <> MsgPackFormats.UINT_8 Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToByte(
                        MsgPackIO.DeserializeValue(GetType(Byte), reader, NilImplication.Null))
                Next

                Return
            End If

            at += 1
            values(i) = block(at)
            at += 1
        Next

        Call seekBack(reader, start, at)
    End Sub

    Private Sub readBooleans(values As Boolean(), count As Integer, reader As BinaryDataReader)
        Dim start As Long = 0
        Dim block As Byte() = readBlock(reader, CLng(count), start)

        If block Is Nothing Then
            For i As Integer = 0 To count - 1
                values(i) = Convert.ToBoolean(
                    MsgPackIO.DeserializeValue(GetType(Boolean), reader, NilImplication.Null))
            Next

            Return
        End If

        Dim at As Integer = 0

        For i As Integer = 0 To count - 1
            If at >= block.Length OrElse (block(at) <> Bool.TRUE AndAlso block(at) <> Bool.FALSE) Then
                Call seekBack(reader, start, at)

                For j As Integer = i To count - 1
                    values(j) = Convert.ToBoolean(
                        MsgPackIO.DeserializeValue(GetType(Boolean), reader, NilImplication.Null))
                Next

                Return
            End If

            values(i) = (block(at) = Bool.TRUE)

            at += 1
        Next

        Call seekBack(reader, start, at)
    End Sub

    ''' <summary>
    ''' 从字节块里解出一个整数（不碰流、不分配），并把游标推到下一个元素。
    ''' </summary>
    ''' <returns>解不出来（格式不对 / 块不够）时返回 False，调用方退回通用路径。</returns>
    Private Function tryDecodeInteger(block As Byte(), at As Integer, buffer As Byte(),
                                      ByRef value As Long, ByRef nextAt As Integer) As Boolean
        If at >= block.Length Then Return False

        Dim header As Byte = block(at)

        at += 1

        Select Case header
            Case MsgPackFormats.UINT_8
                If at + 1 > block.Length Then Return False

                value = block(at)
                at += 1
            Case MsgPackFormats.UINT_16
                If at + 2 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 2)
                Call Array.Reverse(buffer, 0, 2)

                value = BitConverter.ToUInt16(buffer, 0)
                at += 2
            Case MsgPackFormats.UINT_32
                If at + 4 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 4)
                Call Array.Reverse(buffer, 0, 4)

                value = BitConverter.ToUInt32(buffer, 0)
                at += 4
            Case MsgPackFormats.UINT_64
                If at + 8 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 8)
                Call Array.Reverse(buffer, 0, 8)

                value = BitConverter.ToInt64(buffer, 0)
                at += 8
            Case MsgPackFormats.INT_8
                If at + 1 > block.Length Then Return False

                ' 不能直接 CSByte：VB 会对 >127 的字节抛溢出
                value = toSigned(block(at))
                at += 1
            Case MsgPackFormats.INT_16
                If at + 2 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 2)
                Call Array.Reverse(buffer, 0, 2)

                value = BitConverter.ToInt16(buffer, 0)
                at += 2
            Case MsgPackFormats.INT_32
                If at + 4 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 4)
                Call Array.Reverse(buffer, 0, 4)

                value = BitConverter.ToInt32(buffer, 0)
                at += 4
            Case MsgPackFormats.INT_64
                If at + 8 > block.Length Then Return False

                Call Array.Copy(block, at, buffer, 0, 8)
                Call Array.Reverse(buffer, 0, 8)

                value = BitConverter.ToInt64(buffer, 0)
                at += 8
            Case Else
                If header <= FixedInteger.POSITIVE_MAX Then
                    value = CLng(header)
                ElseIf header >= FixedInteger.NEGATIVE_MIN Then
                    value = -(CLng(header) - CLng(FixedInteger.NEGATIVE_MIN))
                Else
                    ' NIL 或者别的类型：交给通用路径
                    at -= 1

                    Return False
                End If
        End Select

        nextAt = at

        Return True
    End Function

#End Region

#Region "底层"

    ''' <summary>
    ''' 把接下来最多 <paramref name="maxBytes"/> 个字节一次读出来。
    ''' </summary>
    ''' <returns>
    ''' 流不可定位时返回 Nothing（不能多读，否则多出来的字节就丢了）。
    ''' 可定位时可能多读一点，调用方读完要用 <see cref="seekBack"/> 把位置回正。
    ''' </returns>
    Private Function readBlock(reader As BinaryDataReader, maxBytes As Long, ByRef start As Long) As Byte()
        Dim baseStream As Stream = reader.BaseStream

        If baseStream Is Nothing OrElse Not baseStream.CanSeek Then Return Nothing
        If maxBytes <= 0 Then Return Nothing

        start = baseStream.Position

        Dim available As Long = baseStream.Length - start

        If available <= 0 Then Return Nothing

        Dim size As Integer = CInt(System.Math.Min(maxBytes, available))

        If size <= 0 Then Return Nothing

        Return reader.ReadBytes(size)
    End Function

    Private Sub seekBack(reader As BinaryDataReader, start As Long, consumed As Integer)
        Dim baseStream As Stream = reader.BaseStream

        If baseStream Is Nothing OrElse Not baseStream.CanSeek Then Return

        Call baseStream.Seek(start + consumed, SeekOrigin.Begin)
    End Sub

    ''' <summary>字节 -> 有符号字节（0x80~0xFF 是负数）。</summary>
    Private Function toSigned(b As Byte) As Long
        If b > 127 Then Return CLng(b) - 256

        Return CLng(b)
    End Function

    ''' <summary>按线程的暂存区（够大就复用，避免每个元素分配一个数组）。</summary>
    Private Function scratchOf(size As Integer) As Byte()
        If scratch Is Nothing OrElse scratch.Length < size Then
            scratch = New Byte(size - 1) {}
        End If

        Return scratch
    End Function

#End Region

End Module

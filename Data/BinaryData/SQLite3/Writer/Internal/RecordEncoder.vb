Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace Writer.Internal

    ''' <summary>
    ''' SQLite 记录体(record)编码器。
    ''' 
    ''' 与读取侧 <c>Sqlite3Table.ReadValue</c> 严格对称:
    ''' 记录体 = <c>varint(headerSize) + 各列 serial type(varint) + 各列数据</c>。
    ''' 
    ''' > https://www.sqlite.org/fileformat2.html#record_format
    ''' </summary>
    Friend Module RecordEncoder

        ''' <summary>
        ''' 计算 SQLite 变长整数(VarInt)编码之后所占的字节数。
        ''' </summary>
        Public Function GetVarIntLength(value As Long) As Integer
            If value < 0 Then
                ' 负数需要写满 9 个字节(包含符号扩展的最高位)
                Return 9
            End If

            Dim v As ULong = CULng(value)
            Dim length As Integer = 1

            While v >= &H80UL
                length += 1
                v >>= 7
            End While

            Return length
        End Function

        ''' <summary>
        ''' 将变长整数写入流(大端, 每个字节低 7 位为数据, 最高位为后续标志)
        ''' </summary>
        Public Sub WriteVarInt(stream As Stream, value As Long)
            If value < 0 OrElse GetVarIntLength(value) > 8 Then
                ' 9 字节形式: 前 8 个字节各携带 7 位(最高位恒为 1), 最后 1 个字节携带低 8 位
                Dim buffer(8) As Byte
                Dim u As ULong = CULng(value)

                buffer(8) = CByte(u And &HFFUL)
                u >>= 8

                For i As Integer = 7 To 0 Step -1
                    buffer(i) = CByte((u And &H7FUL) Or &H80UL)
                    u >>= 7
                Next

                stream.Write(buffer, 0, 9)
                Return
            End If

            Dim tmp(7) As Byte
            Dim n As Integer = 0
            Dim val As ULong = CULng(value)

            Do
                tmp(n) = CByte(val And &H7FUL)
                val >>= 7
                n += 1
            Loop While val <> 0UL

            For i As Integer = n - 1 To 0 Step -1
                Dim b As Byte = tmp(i)

                If i <> 0 Then
                    b = b Or &H80
                End If

                stream.WriteByte(b)
            Next
        End Sub

        ''' <summary>
        ''' 依据 CLR 值计算其 serial type, 并输出对应的数据字节。
        ''' 
        ''' | serial type | 含义 |
        ''' | --- | --- |
        ''' | 0 | NULL |
        ''' | 1..6 | 1/2/3/4/6/8 字节整数 |
        ''' | 7 | 8 字节 IEEE 浮点数 |
        ''' | 8 / 9 | 整数 0 / 1 |
        ''' | 12 + 2n | n 字节 BLOB |
        ''' | 13 + 2n | n 字节 TEXT |
        ''' </summary>
        Public Function GetSerialType(value As Object, ByRef data As Byte()) As Long
            data = Nothing

            If value Is Nothing Then
                Return 0L
            End If

            If TypeOf value Is Boolean Then
                Return If(CBool(value), 9L, 8L)
            End If

            If TypeOf value Is DateTime Then
                data = Encoding.UTF8.GetBytes(DirectCast(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF"))
                Return 13L + 2L * data.Length
            End If

            If TypeOf value Is Guid Then
                data = Encoding.UTF8.GetBytes(DirectCast(value, Guid).ToString())
                Return 13L + 2L * data.Length
            End If

            If TypeOf value Is String Then
                data = Encoding.UTF8.GetBytes(DirectCast(value, String))
                Return 13L + 2L * data.Length
            End If

            If TypeOf value Is Byte() Then
                data = DirectCast(value, Byte())
                If data Is Nothing Then
                    data = New Byte() {}
                End If
                Return 12L + 2L * data.Length
            End If

            If TypeOf value Is Double OrElse TypeOf value Is Single OrElse TypeOf value Is Decimal Then
                data = BitConverter.GetBytes(Convert.ToDouble(value))
                ' BitConverter 使用主机字节序(小端), 这里反转为大端以匹配读取侧的 ReadInteger
                Array.Reverse(data)
                Return 7L
            End If

            Dim integerValue As Long = Convert.ToInt64(value)
            Dim width As Integer = GetMinimalIntegerWidth(integerValue)

            data = New Byte(width - 1) {}
            Dim u As ULong = CULng(integerValue)

            For i As Integer = width - 1 To 0 Step -1
                data(i) = CByte(u And &HFFUL)
                u >>= 8
            Next

            Return CLng(width)
        End Function

        ''' <summary>
        ''' 计算存储指定整数所需的最小字节宽度(1/2/3/4/6/8)
        ''' </summary>
        Public Function GetMinimalIntegerWidth(value As Long) As Integer
            If value >= -128L AndAlso value <= 127L Then Return 1
            If value >= -32768L AndAlso value <= 32767L Then Return 2
            If value >= -8388608L AndAlso value <= 8388607L Then Return 3
            If value >= -2147483648L AndAlso value <= 2147483647L Then Return 4
            If value >= -140737488355328L AndAlso value <= 140737488355327L Then Return 6
            Return 8
        End Function

        ''' <summary>
        ''' 将一行数据编码为 SQLite 记录体。
        ''' </summary>
        ''' <param name="values">按列顺序排列的值</param>
        ''' <param name="rowIdAliasOrdinal">
        ''' rowid 别名列(INTEGER PRIMARY KEY)的序号, 该列在记录体之中写 NULL, 实际取值由 rowid 承载; 不存在时为 -1。
        ''' </param>
        Public Function EncodeRecord(values As Object(), rowIdAliasOrdinal As Integer) As Byte()
            Dim serialTypes As New List(Of Long)
            Dim datas As New List(Of Byte())

            For i As Integer = 0 To values.Length - 1
                If i = rowIdAliasOrdinal Then
                    ' INTEGER PRIMARY KEY 是 rowid 的别名, 记录体之中以 NULL 存储
                    serialTypes.Add(0L)
                    datas.Add(Nothing)
                    Continue For
                End If

                Dim data As Byte() = Nothing
                Dim serialType As Long = GetSerialType(values(i), data)

                serialTypes.Add(serialType)
                datas.Add(data)
            Next

            Dim bodySize As Long = 0
            For Each st As Long In serialTypes
                bodySize += GetVarIntLength(st)
            Next

            ' headerSize 自身也占用一个 varint, 需要迭代到稳定值
            Dim headerSize As Long = bodySize + GetVarIntLength(bodySize)

            While headerSize <> bodySize + GetVarIntLength(headerSize)
                headerSize = bodySize + GetVarIntLength(headerSize)
            End While

            Using ms As New MemoryStream()
                Call WriteVarInt(ms, headerSize)

                For Each st As Long In serialTypes
                    Call WriteVarInt(ms, st)
                Next

                For Each data As Byte() In datas
                    If data IsNot Nothing AndAlso data.Length > 0 Then
                        ms.Write(data, 0, data.Length)
                    End If
                Next

                Return ms.ToArray()
            End Using
        End Function

    End Module

End Namespace

' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\PT\PtDataType.vb
' /********************************************************************************/

Imports System.Text
Imports std = System.Math

Namespace PT

    ''' <summary>
    ''' PyTorch 张量存储块（storage）的元素类型。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 框架内部的张量一律以双精度 <see cref="Double"/> 存放，因此本枚举只用于**描述落盘格式**：
    ''' 读取时按枚举把原始字节提升为双精度，写出时再按枚举把双精度收窄回目标精度。
    ''' </para>
    ''' <para>
    ''' PyTorch 在 pickle 流中出现两种等价的类型记号：新式 dtype 记号（<c>torch.float32</c>）与
    ''' 旧式 storage 类名（<c>torch.FloatStorage</c>）。两种记号都由
    ''' <see cref="PtDataType.Parse(String)"/> 归一化到本枚举。
    ''' </para>
    ''' </remarks>
    Public Enum PtElementType

        ''' <summary>单精度浮点（<c>torch.float32</c> / <c>torch.FloatStorage</c>），PyTorch 默认权重精度。</summary>
        Float32 = 0

        ''' <summary>双精度浮点（<c>torch.float64</c> / <c>torch.DoubleStorage</c>）。</summary>
        Float64 = 1

        ''' <summary>半精度浮点（<c>torch.float16</c> / <c>torch.HalfStorage</c>）。</summary>
        Float16 = 2

        ''' <summary>BFloat16（<c>torch.bfloat16</c> / <c>torch.BFloat16Storage</c>）。</summary>
        BFloat16 = 3

        ''' <summary>64 位有符号整数（<c>torch.int64</c> / <c>torch.LongStorage</c>）。</summary>
        Int64 = 4

        ''' <summary>32 位有符号整数（<c>torch.int32</c> / <c>torch.IntStorage</c>）。</summary>
        Int32 = 5

        ''' <summary>16 位有符号整数（<c>torch.int16</c> / <c>torch.ShortStorage</c>）。</summary>
        Int16 = 6

        ''' <summary>8 位有符号整数（<c>torch.int8</c> / <c>torch.CharStorage</c>）。</summary>
        Int8 = 7

        ''' <summary>8 位无符号整数（<c>torch.uint8</c> / <c>torch.ByteStorage</c>）。</summary>
        UInt8 = 8

        ''' <summary>布尔（<c>torch.bool</c> / <c>torch.BoolStorage</c>），落盘为 0/1 单字节。</summary>
        [Bool] = 9

    End Enum

    ''' <summary>
    ''' <see cref="PtElementType"/> 的元数据与字节编解码。
    ''' </summary>
    ''' <remarks>
    ''' PyTorch 的归档文件只在小端序平台上产生（<see cref="BitConverter.IsLittleEndian"/> 为 False 的平台
    ''' 需要做一次字节翻转），本模块对两种平台都做了处理。
    ''' </remarks>
    Public Module PtDataType

        ''' <summary>单个元素的字节数。</summary>
        ''' <param name="type">元素类型</param>
        Public Function ElementSize(type As PtElementType) As Integer
            Select Case type
                Case PtElementType.Float64, PtElementType.Int64
                    Return 8
                Case PtElementType.Float32, PtElementType.Int32
                    Return 4
                Case PtElementType.Float16, PtElementType.BFloat16, PtElementType.Int16
                    Return 2
                Case PtElementType.Int8, PtElementType.UInt8, PtElementType.Bool
                    Return 1
                Case Else
                    Throw New NotSupportedException($"不支持的 PyTorch 元素类型：{type}")
            End Select
        End Function

        ''' <summary>
        ''' 新式 dtype 记号，例如 <c>torch.float32</c>。
        ''' </summary>
        ''' <param name="type">元素类型</param>
        Public Function DTypeName(type As PtElementType) As String
            Select Case type
                Case PtElementType.Float32 : Return "torch.float32"
                Case PtElementType.Float64 : Return "torch.float64"
                Case PtElementType.Float16 : Return "torch.float16"
                Case PtElementType.BFloat16 : Return "torch.bfloat16"
                Case PtElementType.Int64 : Return "torch.int64"
                Case PtElementType.Int32 : Return "torch.int32"
                Case PtElementType.Int16 : Return "torch.int16"
                Case PtElementType.Int8 : Return "torch.int8"
                Case PtElementType.UInt8 : Return "torch.uint8"
                Case PtElementType.Bool : Return "torch.bool"
                Case Else
                    Throw New NotSupportedException($"不支持的 PyTorch 元素类型：{type}")
            End Select
        End Function

        ''' <summary>
        ''' 旧式 storage 类名，例如 <c>torch.FloatStorage</c>（PyTorch 1.5 及更早版本的写法）。
        ''' </summary>
        ''' <param name="type">元素类型</param>
        Public Function StorageName(type As PtElementType) As String
            Select Case type
                Case PtElementType.Float32 : Return "torch.FloatStorage"
                Case PtElementType.Float64 : Return "torch.DoubleStorage"
                Case PtElementType.Float16 : Return "torch.HalfStorage"
                Case PtElementType.BFloat16 : Return "torch.BFloat16Storage"
                Case PtElementType.Int64 : Return "torch.LongStorage"
                Case PtElementType.Int32 : Return "torch.IntStorage"
                Case PtElementType.Int16 : Return "torch.ShortStorage"
                Case PtElementType.Int8 : Return "torch.CharStorage"
                Case PtElementType.UInt8 : Return "torch.ByteStorage"
                Case PtElementType.Bool : Return "torch.BoolStorage"
                Case Else
                    Throw New NotSupportedException($"不支持的 PyTorch 元素类型：{type}")
            End Select
        End Function

        ''' <summary>
        ''' 解析 dtype 记号（<c>torch.float32</c>、<c>FloatStorage</c>、<c>torch.FloatStorage</c>、
        ''' 以及 <c>float</c>/<c>double</c>/<c>half</c> 等别名）。
        ''' </summary>
        ''' <param name="token">记号文本</param>
        ''' <returns>归一化后的元素类型</returns>
        ''' <exception cref="NotSupportedException">无法识别的记号</exception>
        Public Function Parse(token As String) As PtElementType
            Dim result As PtElementType

            If TryParse(token, result) Then
                Return result
            Else
                Throw New NotSupportedException($"无法识别的 PyTorch 元素类型记号：'{token}'")
            End If
        End Function

        ''' <summary>
        ''' 尝试解析 dtype 记号。
        ''' </summary>
        ''' <param name="token">记号文本；允许带 <c>torch.</c> 前缀或 storage 后缀</param>
        ''' <param name="result">解析结果</param>
        ''' <returns>是否识别成功</returns>
        Public Function TryParse(token As String, ByRef result As PtElementType) As Boolean
            If String.IsNullOrEmpty(token) Then
                Return False
            End If

            Dim name As String = token.Trim()

            If name.StartsWith("torch.", StringComparison.OrdinalIgnoreCase) Then
                name = name.Substring(6)
            End If

            ' 去掉旧式 storage 类名的 Storage 后缀：FloatStorage -> Float
            If name.EndsWith("Storage", StringComparison.OrdinalIgnoreCase) Then
                name = name.Substring(0, name.Length - 7)
            End If

            Select Case name.ToLowerInvariant()
                Case "float32", "float", "single"
                    result = PtElementType.Float32 : Return True
                Case "float64", "double"
                    result = PtElementType.Float64 : Return True
                Case "float16", "half"
                    result = PtElementType.Float16 : Return True
                Case "bfloat16"
                    result = PtElementType.BFloat16 : Return True
                Case "int64", "long"
                    result = PtElementType.Int64 : Return True
                Case "int32", "int"
                    result = PtElementType.Int32 : Return True
                Case "int16", "short"
                    result = PtElementType.Int16 : Return True
                Case "int8", "char"
                    result = PtElementType.Int8 : Return True
                Case "uint8", "byte"
                    result = PtElementType.UInt8 : Return True
                Case "bool"
                    result = PtElementType.Bool : Return True
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' 把 <paramref name="count"/> 个元素从原始字节解码为双精度数组。
        ''' </summary>
        ''' <param name="bytes">原始字节</param>
        ''' <param name="offset">起始字节偏移</param>
        ''' <param name="count">元素个数</param>
        ''' <param name="type">元素类型</param>
        Public Function Decode(bytes As Byte(), offset As Integer, count As Integer, type As PtElementType) As Double()
            Dim size As Integer = ElementSize(type)
            Dim length As Long = CLng(offset) + CLng(count) * size

            If bytes Is Nothing Then
                Throw New ArgumentNullException(NameOf(bytes))
            End If

            If count < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count), $"元素个数不能为负数：{count}")
            End If

            If length > bytes.Length Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(count),
                    $"存储块字节数不足：需要 {length} 字节（offset={offset}, count={count}, itemsize={size}），实际只有 {bytes.Length} 字节")
            End If

            Dim result As Double() = New Double(count - 1) {}

            For i As Integer = 0 To count - 1
                Dim at As Integer = offset + i * size

                Select Case type
                    Case PtElementType.Float64
                        result(i) = BitConverter.ToDouble(bytes, at)
                    Case PtElementType.Float32
                        result(i) = BitConverter.ToSingle(bytes, at)
                    Case PtElementType.Float16
                        result(i) = HalfToDouble(BitConverter.ToUInt16(bytes, at))
                    Case PtElementType.BFloat16
                        result(i) = BFloat16ToDouble(BitConverter.ToUInt16(bytes, at))
                    Case PtElementType.Int64
                        result(i) = BitConverter.ToInt64(bytes, at)
                    Case PtElementType.Int32
                        result(i) = BitConverter.ToInt32(bytes, at)
                    Case PtElementType.Int16
                        result(i) = BitConverter.ToInt16(bytes, at)
                    Case PtElementType.Int8
                        result(i) = CSByte(bytes(at))
                    Case PtElementType.UInt8
                        result(i) = bytes(at)
                    Case PtElementType.Bool
                        result(i) = If(bytes(at) = 0, 0.0, 1.0)
                    Case Else
                        Throw New NotSupportedException($"不支持的 PyTorch 元素类型：{type}")
                End Select
            Next

            Return result
        End Function

        ''' <summary>
        ''' 把双精度数组编码为指定元素类型的原始字节。
        ''' </summary>
        ''' <param name="values">要编码的数值</param>
        ''' <param name="type">元素类型</param>
        ''' <returns>与 <paramref name="type"/> 对应的原始字节（小端序）</returns>
        Public Function Encode(values As Double(), type As PtElementType) As Byte()
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim size As Integer = ElementSize(type)
            Dim bytes As Byte() = New Byte(values.Length * size - 1) {}

            Call Encode(values, bytes, 0, type)

            Return bytes
        End Function

        ''' <summary>
        ''' 把双精度数组编码进给定的字节缓冲区。
        ''' </summary>
        ''' <param name="values">要编码的数值</param>
        ''' <param name="bytes">目标缓冲区</param>
        ''' <param name="offset">写入的起始字节偏移</param>
        ''' <param name="type">元素类型</param>
        Public Sub Encode(values As Double(), bytes As Byte(), offset As Integer, type As PtElementType)
            Dim size As Integer = ElementSize(type)
            Dim length As Long = CLng(offset) + CLng(values.Length) * size

            If bytes Is Nothing Then
                Throw New ArgumentNullException(NameOf(bytes))
            End If

            If length > bytes.Length Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(bytes),
                    $"目标缓冲区不足：需要 {length} 字节（offset={offset}, count={values.Length}, itemsize={size}），实际只有 {bytes.Length} 字节")
            End If

            Dim buffer As Byte() = If(BitConverter.IsLittleEndian, Nothing, New Byte(7) {})

            For i As Integer = 0 To values.Length - 1
                Dim at As Integer = offset + i * size
                Dim value As Double = values(i)

                Select Case type
                    Case PtElementType.Float64
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(value))
                    Case PtElementType.Float32
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(CSng(value)))
                    Case PtElementType.Float16
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(DoubleToHalf(value)))
                    Case PtElementType.BFloat16
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(DoubleToBFloat16(value)))
                    Case PtElementType.Int64
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(CLng(std.Round(value))))
                    Case PtElementType.Int32
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(CInt(std.Round(value))))
                    Case PtElementType.Int16
                        Call WriteLittleEndian(buffer, bytes, at, BitConverter.GetBytes(CShort(std.Round(value))))
                    Case PtElementType.Int8
                        ' 负数需要用掩码写回无符号字节，直接 CByte 会溢出
                        bytes(at) = CByte(CInt(CSByte(std.Round(value))) And &HFF)
                    Case PtElementType.UInt8
                        bytes(at) = CByte(CInt(std.Round(value)) And &HFF)
                    Case PtElementType.Bool
                        bytes(at) = If(value = 0.0, CByte(0), CByte(1))
                    Case Else
                        Throw New NotSupportedException($"不支持的 PyTorch 元素类型：{type}")
                End Select
            Next
        End Sub

        ''' <summary>
        ''' 写入一个小端序的定长值（在大端序平台上翻转字节）。
        ''' </summary>
        Private Sub WriteLittleEndian(scratch As Byte(), target As Byte(), offset As Integer, value As Byte())
            If scratch Is Nothing Then
                Call Array.Copy(value, 0, target, offset, value.Length)
            Else
                Call Array.Copy(value, scratch, value.Length)
                Call Array.Reverse(scratch, 0, value.Length)
                Call Array.Copy(scratch, 0, target, offset, value.Length)
            End If
        End Sub

        ''' <summary>IEEE 754 半精度（binary16）位模式 → 双精度。</summary>
        Public Function HalfToDouble(bits As UShort) As Double
            Return HalfToSingle(bits)
        End Function

        ''' <summary>IEEE 754 半精度（binary16）位模式 → 单精度。</summary>
        Public Function HalfToSingle(bits As UShort) As Single
            Dim sign As Integer = (CInt(bits) And &H8000) << 16
            Dim exponent As Integer = (CInt(bits) >> 10) And &H1F
            Dim mantissa As Integer = CInt(bits) And &H3FF

            If exponent = 0 Then
                If mantissa = 0 Then
                    Return BitConverter.Int32BitsToSingle(sign)
                End If

                ' 非规约数：规格化到最小指数
                Do While (mantissa And &H400) = 0
                    mantissa <<= 1
                    exponent -= 1
                Loop

                exponent += 1
                mantissa = mantissa And &H3FF

                Return BitConverter.Int32BitsToSingle(sign Or ((exponent + 127 - 15) << 23) Or (mantissa << 13))
            ElseIf exponent = &H1F Then
                ' Inf / NaN
                Return BitConverter.Int32BitsToSingle(sign Or &H7F800000 Or (mantissa << 13))
            Else
                Return BitConverter.Int32BitsToSingle(sign Or ((exponent + 127 - 15) << 23) Or (mantissa << 13))
            End If
        End Function

        ''' <summary>BFloat16 位模式 → 双精度。</summary>
        Public Function BFloat16ToDouble(bits As UShort) As Double
            Return BitConverter.Int32BitsToSingle(CInt(bits) << 16)
        End Function

        ''' <summary>双精度 → IEEE 754 半精度位模式（就近舍入到偶数）。</summary>
        Public Function DoubleToHalf(value As Double) As UShort
            Dim single_ As Single = CSng(value)
            Dim bits As Integer = BitConverter.SingleToInt32Bits(single_)
            Dim sign As Integer = (bits >> 16) And &H8000
            Dim exponent As Integer = (bits >> 23) And &HFF
            Dim mantissa As Integer = bits And &H7FFFFF

            If exponent = &HFF Then
                ' Inf / NaN
                If mantissa <> 0 Then
                    Return CUShort(sign Or &H7E00)
                Else
                    Return CUShort(sign Or &H7C00)
                End If
            End If

            Dim e As Integer = exponent - 127 + 15

            If e >= &H1F Then
                ' 上溢为无穷大
                Return CUShort(sign Or &H7C00)
            ElseIf e <= 0 Then
                ' 下溢到非规约数（或 0）
                If e < -10 Then
                    Return CUShort(sign)
                End If

                mantissa = mantissa Or &H800000

                Dim shift As Integer = 14 - e
                Dim kept As Integer = mantissa >> shift
                Dim remainder As Integer = mantissa And ((1 << shift) - 1)
                Dim halfway As Integer = 1 << (shift - 1)

                If remainder > halfway OrElse (remainder = halfway AndAlso (kept And 1) = 1) Then
                    kept += 1
                End If

                Return CUShort(sign Or kept)
            Else
                Dim kept As Integer = mantissa >> 13
                Dim remainder As Integer = mantissa And &H1FFF

                If remainder > &H1000 OrElse (remainder = &H1000 AndAlso (kept And 1) = 1) Then
                    kept += 1

                    If kept = &H400 Then
                        kept = 0
                        e += 1

                        If e >= &H1F Then
                            Return CUShort(sign Or &H7C00)
                        End If
                    End If
                End If

                Return CUShort(sign Or (e << 10) Or kept)
            End If
        End Function

        ''' <summary>双精度 → BFloat16 位模式（就近舍入到偶数）。</summary>
        Public Function DoubleToBFloat16(value As Double) As UShort
            Dim bits As Integer = BitConverter.SingleToInt32Bits(CSng(value))

            If Single.IsNaN(CSng(value)) Then
                Return CUShort((bits >> 16) Or &H40)
            End If

            Dim lsb As Integer = (bits >> 16) And 1
            Dim rounding As Integer = &H7FFF + lsb

            Return CUShort((bits + rounding) >> 16)
        End Function

        ''' <summary>
        ''' 生成用于错误信息的人类可读描述。
        ''' </summary>
        Public Function Describe(type As PtElementType) As String
            Return $"{DTypeName(type)} ({ElementSize(type)} bytes)"
        End Function

    End Module

End Namespace

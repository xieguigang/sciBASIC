Imports System.Buffers.Binary
Imports System.IO
Imports System.Text

Namespace Numpy

    ''' <summary>
    ''' npy 文件读写所用的低层字节流辅助函数。
    ''' </summary>
    Friend Module NpyStream

        ''' <summary>
        ''' 精确读取指定字节数，读不满则抛出异常。
        ''' </summary>
        Friend Sub ReadExactly(stream As Stream, buffer As Byte(), offset As Integer, count As Integer)
            Dim readCount As Integer = 0

            While readCount < count
                Dim n As Integer = stream.Read(buffer, offset + readCount, count - readCount)

                If n <= 0 Then
                    Throw New InvalidDataException($"unexpected end of numpy data stream: expected {count} bytes but only {readCount} bytes available!")
                End If

                readCount += n
            End While
        End Sub

        ''' <summary>
        ''' 读取 <paramref name="count"/> 个字节。
        ''' </summary>
        Friend Function ReadBytes(stream As Stream, count As Long) As Byte()
            If count < 0 Then
                Throw New InvalidDataException($"invalid numpy data length: {count}")
            End If
            If count > Integer.MaxValue Then
                Throw New NotSupportedException($"numpy array is too large for the current runtime: {count} bytes")
            End If

            Dim buffer As Byte() = New Byte(CInt(count) - 1) {}

            If count > 0 Then
                Call ReadExactly(stream, buffer, 0, CInt(count))
            End If

            Return buffer
        End Function

        ''' <summary>
        ''' 读取 2 字节小端无符号整数（npy v1 头部长度字段）。
        ''' </summary>
        Friend Function ReadUInt16(stream As Stream) As Integer
            Dim buffer As Byte() = ReadBytes(stream, 2)

            Return BinaryPrimitives.ReadUInt16LittleEndian(buffer)
        End Function

        ''' <summary>
        ''' 读取 4 字节小端无符号整数（npy v2/v3 头部长度字段）。
        ''' </summary>
        Friend Function ReadUInt32(stream As Stream) As Long
            Dim buffer As Byte() = ReadBytes(stream, 4)

            Return BinaryPrimitives.ReadUInt32LittleEndian(buffer)
        End Function

        ''' <summary>
        ''' 读取单个字节，结束时抛出异常。
        ''' </summary>
        Friend Function ReadByte(stream As Stream) As Byte
            Dim value As Integer = stream.ReadByte()

            If value < 0 Then
                Throw New InvalidDataException("unexpected end of numpy data stream!")
            End If

            Return CByte(value)
        End Function

    End Module

    ''' <summary>
    ''' numpy 的 <c>.npy</c> 文件头部：魔数、格式版本与头部字典（<c>descr</c>、
    ''' <c>fortran_order</c>、<c>shape</c>）。
    ''' </summary>
    ''' <remarks>
    ''' 文件布局：<c>\x93NUMPY</c>（6 字节） + 版本（主/次各 1 字节） + 头部长度（v1 为 2 字节小端，
    ''' v2/v3 为 4 字节小端） + 头部字典文本（空格填充到 64 字节对齐，以换行结尾） + 数据区。
    ''' </remarks>
    Public Class NpyHeader

        ''' <summary>
        ''' 魔数首字节（<c>0x93</c>）。
        ''' </summary>
        Public Const MagicFirstByte As Byte = &H93

        ''' <summary>
        ''' 魔数文本部分。
        ''' </summary>
        Public Const MagicText As String = "NUMPY"

        ''' <summary>
        ''' 头部对齐长度（numpy 约定 64 字节）。
        ''' </summary>
        Public Const Alignment As Integer = 64

        ''' <summary>
        ''' 格式主版本号（1、2 或 3）。
        ''' </summary>
        Public Property MajorVersion As Byte = 1

        ''' <summary>
        ''' 格式次版本号。
        ''' </summary>
        Public Property MinorVersion As Byte = 0

        ''' <summary>
        ''' dtype 描述文本（<c>descr</c> 字段）。
        ''' </summary>
        Public Property Descr As String

        ''' <summary>
        ''' 数据是否以 Fortran 序（列优先）存放。
        ''' </summary>
        Public Property FortranOrder As Boolean

        ''' <summary>
        ''' 数组形状；0 维数组为空数组。
        ''' </summary>
        Public Property Shape As Integer()

        ''' <summary>
        ''' 头部字典文本的字节长度。
        ''' </summary>
        Public Property HeaderSize As Integer

        ''' <summary>
        ''' 解析后的数据类型。
        ''' </summary>
        Public Property DType As NpyDType

        ''' <summary>
        ''' 数据区在文件中的起始偏移量。
        ''' </summary>
        Public ReadOnly Property DataOffset As Long
            Get
                Return If(MajorVersion = 1, 10, 12) + HeaderSize
            End Get
        End Property

        ''' <summary>
        ''' 元素总数（0 维数组为 1）。
        ''' </summary>
        Public ReadOnly Property Length As Long
            Get
                If Shape Is Nothing OrElse Shape.Length = 0 Then
                    Return 1
                End If

                Dim n As Long = 1

                For Each size As Integer In Shape
                    n *= size
                Next

                Return n
            End Get
        End Property

        ''' <summary>
        ''' 数组维数。
        ''' </summary>
        Public ReadOnly Property Rank As Integer
            Get
                If Shape Is Nothing Then
                    Return 0
                Else
                    Return Shape.Length
                End If
            End Get
        End Property

        ''' <summary>
        ''' 格式版本文本，例如 <c>1.0</c>。
        ''' </summary>
        Public ReadOnly Property Version As String
            Get
                Return $"{MajorVersion}.{MinorVersion}"
            End Get
        End Property

        ''' <summary>
        ''' 从流中读取并解析 npy 头部（流位置须位于文件起始处，读取后停留在数据区起点）。
        ''' </summary>
        ''' <param name="stream">npy 数据流</param>
        Public Shared Function Read(stream As Stream) As NpyHeader
            Dim magic As Byte() = NpyStream.ReadBytes(stream, 6)

            If magic(0) <> MagicFirstByte OrElse Encoding.ASCII.GetString(magic, 1, 5) <> MagicText Then
                Throw New InvalidDataException("invalid numpy data file: magic header mismatch!")
            End If

            Dim major As Byte = NpyStream.ReadByte(stream)
            Dim minor As Byte = NpyStream.ReadByte(stream)

            If major < 1 OrElse major > 3 Then
                Throw New NotSupportedException($"unsupported numpy file format version: {major}.{minor}")
            End If

            Dim headerLength As Long = If(major = 1, NpyStream.ReadUInt16(stream), NpyStream.ReadUInt32(stream))

            If headerLength <= 0 OrElse headerLength > 1048576 Then
                Throw New InvalidDataException($"invalid numpy header length: {headerLength}")
            End If

            Dim headerBytes As Byte() = NpyStream.ReadBytes(stream, headerLength)
            Dim headerText As String = Encoding.UTF8.GetString(headerBytes)
            Dim dict As Object = PyLiteral.Parse(headerText.Trim())

            Dim table As Dictionary(Of Object, Object) = TryCast(dict, Dictionary(Of Object, Object))

            If table Is Nothing Then
                Throw New InvalidDataException("invalid numpy header: a python dict literal is expected!")
            End If

            Dim result As New NpyHeader With {
                .MajorVersion = major,
                .MinorVersion = minor,
                .HeaderSize = CInt(headerLength)
            }

            If Not table.ContainsKey("descr") Then
                Throw New InvalidDataException("invalid numpy header: missing 'descr' field!")
            End If

            result.DType = NpyDType.FromLiteral(table("descr"))
            result.Descr = result.DType.Descriptor

            If table.ContainsKey("fortran_order") Then
                result.FortranOrder = CBool(table("fortran_order"))
            End If

            result.Shape = ParseShape(table)

            Return result
        End Function

        ''' <summary>
        ''' 读取指定 npy 文件的头部信息（不读取数据区，适合快速探测大文件）。
        ''' </summary>
        ''' <param name="path">npy 文件路径</param>
        Public Shared Function Read(path As String) As NpyHeader
            Using stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return Read(stream)
            End Using
        End Function

        Private Shared Function ParseShape(table As Dictionary(Of Object, Object)) As Integer()
            If Not table.ContainsKey("shape") Then
                Throw New InvalidDataException("invalid numpy header: missing 'shape' field!")
            End If

            Dim raw As Object() = TryCast(table("shape"), Object())

            If raw Is Nothing Then
                Throw New InvalidDataException("invalid numpy header: 'shape' must be a tuple of integers!")
            End If

            Dim shape As Integer() = New Integer(raw.Length - 1) {}

            For i As Integer = 0 To raw.Length - 1
                Dim n As Long = CLng(raw(i))

                If n < 0 OrElse n > Integer.MaxValue Then
                    Throw New NotSupportedException($"invalid numpy array dimension: {n}")
                End If

                shape(i) = CInt(n)
            Next

            Return shape
        End Function

        ''' <summary>
        ''' 依据 dtype、形状与内存布局创建头部。
        ''' </summary>
        ''' <param name="dtype">元素类型</param>
        ''' <param name="shape">数组形状</param>
        ''' <param name="fortranOrder">是否 Fortran 序</param>
        ''' <param name="major">格式主版本号，1 表示 2 字节长度字段</param>
        Public Shared Function Create(dtype As NpyDType, shape As Integer(), fortranOrder As Boolean, Optional major As Byte = 1) As NpyHeader
            If dtype Is Nothing Then
                Throw New ArgumentNullException(NameOf(dtype))
            End If
            If shape Is Nothing Then
                shape = New Integer() {}
            End If

            Return New NpyHeader With {
                .MajorVersion = major,
                .MinorVersion = 0,
                .Descr = dtype.Descriptor,
                .DType = dtype,
                .FortranOrder = fortranOrder,
                .Shape = shape
            }
        End Function

        ''' <summary>
        ''' 生成头部字典文本（不含填充）。
        ''' </summary>
        Public Function ToDictText() As String
            Dim descriptor As String = Descr

            If descriptor Is Nothing Then
                descriptor = DType?.Descriptor
            End If

            If descriptor Is Nothing Then
                Throw New InvalidOperationException("numpy dtype descriptor is not initialized!")
            End If

            Dim descrText As String = If(descriptor.StartsWith("[", StringComparison.Ordinal), descriptor, "'" & descriptor & "'")

            Return "{'descr': " & descrText &
                ", 'fortran_order': " & If(FortranOrder, "True", "False") &
                ", 'shape': " & FormatShape() & ", }"
        End Function

        Private Function FormatShape() As String
            If Shape Is Nothing OrElse Shape.Length = 0 Then
                Return "()"
            End If

            Dim parts As String() = Shape.Select(Function(i) i.ToString()).ToArray()

            If Shape.Length = 1 Then
                Return "(" & parts(0) & ",)"
            Else
                Return "(" & String.Join(", ", parts) & ")"
            End If
        End Function

        ''' <summary>
        ''' 生成完整的文件前导（魔数 + 版本 + 长度字段 + 补齐到 64 字节的头部文本）。
        ''' </summary>
        Public Function ToPreamble() As Byte()
            Dim text As String = ToDictText()
            Dim prefixSize As Integer = If(MajorVersion = 1, 10, 12)
            Dim baseLength As Integer = prefixSize + text.Length + 1
            Dim padding As Integer = (Alignment - (baseLength Mod Alignment)) Mod Alignment
            Dim header As String = text & New String(" "c, padding) & vbLf
            Dim headerBytes As Byte() = Encoding.ASCII.GetBytes(header)

            HeaderSize = headerBytes.Length

            Dim buffer As New List(Of Byte)()

            buffer.Add(MagicFirstByte)
            buffer.AddRange(Encoding.ASCII.GetBytes(MagicText))
            buffer.Add(MajorVersion)
            buffer.Add(MinorVersion)

            If MajorVersion = 1 Then
                If headerBytes.Length > UShort.MaxValue Then
                    Throw New InvalidOperationException($"numpy header is too large for format version 1: {headerBytes.Length} bytes")
                End If

                Dim lengthBuffer As Byte() = New Byte(1) {}

                BinaryPrimitives.WriteUInt16LittleEndian(lengthBuffer, CUShort(headerBytes.Length))
                buffer.AddRange(lengthBuffer)
            Else
                Dim lengthBuffer As Byte() = New Byte(3) {}

                BinaryPrimitives.WriteUInt32LittleEndian(lengthBuffer, CUInt(headerBytes.Length))
                buffer.AddRange(lengthBuffer)
            End If

            buffer.AddRange(headerBytes)

            Return buffer.ToArray()
        End Function

        ''' <summary>
        ''' 返回 <c>npy 1.0 dtype='&lt;f8' shape=(3, 160) fortran=False offset=128</c> 形式的摘要文本。
        ''' </summary>
        Public Overrides Function ToString() As String
            Return $"npy {Version} dtype='{Descr}' shape={FormatShape()} fortran={FortranOrder} offset={DataOffset}"
        End Function

    End Class

End Namespace

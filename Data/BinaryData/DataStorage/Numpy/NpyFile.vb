Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Numpy

    ''' <summary>
    ''' numpy 的 <c>.npy</c> 单数组文件读写门面。
    ''' </summary>
    ''' <remarks>
    ''' 读取：<see cref="Read(String)"/> 自动识别数值、布尔、字符串与对象数组；
    ''' 写入：按元素类型选择 dtype 重载，默认写出小端序与 C 序（可选 Fortran 序）的 64 字节对齐头部。
    ''' 
    ''' 关于对象数组（<c>dtype='O'</c>）：读取时数据区是一个 Python pickle 流，本模块会借助
    ''' <see cref="Pickle.MinimalPickleUnpickler"/> 与 <see cref="NumpyPickleFactory"/> 还原内嵌对象；
    ''' 写入对象数组需要自行生成 numpy 兼容的 pickle 流，目前不支持并会抛出
    ''' <see cref="NotSupportedException"/>。
    ''' </remarks>
    Public Module NpyFile

        ''' <summary>
        ''' 读取 npy 文件。
        ''' </summary>
        ''' <param name="path">npy 文件路径</param>
        Public Function Read(path As String) As NpyArray
            Using stream As Stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return Read(stream)
            End Using
        End Function

        ''' <summary>
        ''' 从流中读取 npy 数组（流位置须位于文件起始处）。
        ''' </summary>
        ''' <param name="stream">npy 数据流</param>
        Public Function Read(stream As Stream) As NpyArray
            Dim header As NpyHeader = NpyHeader.Read(stream)

            If header.DType.Kind = NpyTypeKind.PyObject Then
                Return ReadObjectArray(stream)
            End If

            Dim expected As Long = header.Length * header.DType.ItemSize
            Dim data As Byte() = NpyStream.ReadBytes(stream, expected)

            Return New NpyArray With {
                .DType = header.DType,
                .Shape = header.Shape,
                .FortranOrder = header.FortranOrder,
                .RawData = data
            }
        End Function

        Private Function ReadObjectArray(stream As Stream) As NpyArray
            Dim pickleData As Byte() = ReadRemaining(stream)
            Dim obj As Object = Pickle.MinimalPickleUnpickler.UnpickleFromBytes(pickleData, New NumpyPickleFactory())
            Dim result As NpyArray = TryCast(obj, NpyArray)

            If result Is Nothing Then
                Throw New InvalidDataException($"numpy object array can not be restored, the pickle stream returns a '{If(obj Is Nothing, "null", obj.GetType().FullName)}' object!")
            End If

            Return result
        End Function

        Private Function ReadRemaining(stream As Stream) As Byte()
            If stream.CanSeek Then
                Return NpyStream.ReadBytes(stream, stream.Length - stream.Position)
            End If

            Using buffer As New MemoryStream()
                Call stream.CopyTo(buffer)
                Return buffer.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' 只读取 npy 文件的头部信息（不加载数据区）。
        ''' </summary>
        ''' <param name="path">npy 文件路径</param>
        Public Function ReadHeader(path As String) As NpyHeader
            Return NpyHeader.Read(path)
        End Function

        ''' <summary>
        ''' 只读取 npy 流中的头部信息（读取后流位置位于数据区起点）。
        ''' </summary>
        ''' <param name="stream">npy 数据流</param>
        Public Function ReadHeader(stream As Stream) As NpyHeader
            Return NpyHeader.Read(stream)
        End Function

        ''' <summary>
        ''' 把任意已构造好的数组写出为 npy 文件。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="array">待写出的数组</param>
        Public Sub Save(path As String, array As NpyArray)
            Using stream As Stream = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
                Call WriteTo(stream, array)
            End Using
        End Sub

        ''' <summary>
        ''' 把数组写入一个已有的流（npz 归档逐个条目写出时复用）。
        ''' </summary>
        ''' <param name="stream">目标流</param>
        ''' <param name="array">待写出的数组</param>
        Friend Sub WriteTo(stream As Stream, array As NpyArray)
            If array Is Nothing Then
                Throw New ArgumentNullException(NameOf(array))
            End If
            If array.DType Is Nothing Then
                Throw New ArgumentException("numpy array dtype is not initialized!", NameOf(array))
            End If
            If array.DType.Kind = NpyTypeKind.PyObject OrElse array.IsObjectArray Then
                Throw New NotSupportedException(
                    "writing numpy object arrays (dtype='O') is not supported: " &
                    "numpy stores such arrays as python pickle streams. " &
                    "Please write a numeric, boolean, string or byte array instead.")
            End If

            Dim header As NpyHeader = NpyHeader.Create(array.DType, array.Shape, array.FortranOrder)
            Dim preamble As Byte() = header.ToPreamble()
            Dim data As Byte() = If(array.RawData, New Byte() {})

            Call stream.Write(preamble, 0, preamble.Length)

            If data.Length > 0 Then
                Call stream.Write(data, 0, data.Length)
            End If
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Double"/> 数组（dtype <c>&lt;f8</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Double(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Call SaveNumeric(path, data, NpyDType.Parse("<f8"), shape, fortranOrder, 8)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Single"/> 数组（dtype <c>&lt;f4</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Single(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Call SaveNumeric(path, data, NpyDType.Parse("<f4"), shape, fortranOrder, 4)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Integer"/> 数组（dtype <c>&lt;i4</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Integer(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Call SaveNumeric(path, data, NpyDType.Parse("<i4"), shape, fortranOrder, 4)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Long"/> 数组（dtype <c>&lt;i8</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Long(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Call SaveNumeric(path, data, NpyDType.Parse("<i8"), shape, fortranOrder, 8)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Boolean"/> 数组（dtype <c>|b1</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Boolean(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Dim bytes As Byte() = New Byte(data.Length - 1) {}

            For i As Integer = 0 To data.Length - 1
                bytes(i) = If(data(i), CByte(1), CByte(0))
            Next

            Call SaveBuffer(path, bytes, NpyDType.Parse("|b1"), shape, fortranOrder, 1, data.Length)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="Byte"/> 数组（dtype <c>|u1</c>）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As Byte(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Call SaveBuffer(path, data, NpyDType.Parse("|u1"), shape, fortranOrder, 1, data.Length)
        End Sub

        ''' <summary>
        ''' 写出 <see cref="String"/> 数组（dtype <c>&lt;U[n]</c>，按最长字符串容量定长，零填充）。
        ''' </summary>
        ''' <param name="path">输出文件路径</param>
        ''' <param name="data">按 C 序排列的元素</param>
        ''' <param name="shape">数组形状；省略时视为一维数组</param>
        ''' <param name="fortranOrder">是否以 Fortran 序（列优先）落盘</param>
        Public Sub Write(path As String, data As String(), Optional shape As Integer() = Nothing, Optional fortranOrder As Boolean = False)
            Dim capacity As Integer = 1

            For Each s As String In data
                Dim n As Integer = CodePointCount(s)

                If n > capacity Then
                    capacity = n
                End If
            Next

            Dim itemSize As Integer = capacity * 4
            Dim bytes As Byte() = New Byte(data.Length * itemSize - 1) {}

            For i As Integer = 0 To data.Length - 1
                Call EncodeUtf32(data(i), bytes, i * itemSize, capacity)
            Next

            Call SaveBuffer(path, bytes, NpyDType.Parse("<U" & capacity), shape, fortranOrder, itemSize, data.Length)
        End Sub

        Private Sub SaveNumeric(Of T)(path As String, data As T(), dtype As NpyDType, shape As Integer(), fortranOrder As Boolean, itemSize As Integer)
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.Length = 0 Then
                Throw New ArgumentException("can not write an empty numpy array: at least one element is required!", NameOf(data))
            End If

            Call SaveBuffer(path, Serialize(data, itemSize), dtype, shape, fortranOrder, itemSize, data.Length)
        End Sub

        ''' <summary>
        ''' 按小端序、C 序的原始字节写出数组（字符串类型等自定义宽度的元素也走这个入口）。
        ''' </summary>
        Private Sub SaveBuffer(path As String, raw As Byte(), dtype As NpyDType, shape As Integer(), fortranOrder As Boolean, itemSize As Integer, elementCount As Integer)
            If raw Is Nothing Then
                Throw New ArgumentNullException(NameOf(raw))
            End If
            If elementCount <= 0 Then
                Throw New ArgumentException("can not write an empty numpy array: at least one element is required!")
            End If
            If raw.Length <> elementCount * itemSize Then
                Throw New ArgumentException($"numpy array buffer size mismatch: {raw.Length} bytes for {elementCount} elements of {itemSize} bytes!")
            End If

            Dim resolved As Integer() = ResolveShape(shape, elementCount)

            If fortranOrder AndAlso resolved.Length > 1 Then
                raw = NpyArray.ReorderToFortran(raw, elementCount, itemSize, resolved)
            End If

            Call Save(path, New NpyArray With {
                .DType = dtype,
                .Shape = resolved,
                .FortranOrder = fortranOrder,
                .RawData = raw
            })
        End Sub

        Private Function ResolveShape(shape As Integer(), size As Integer) As Integer()
            If shape Is Nothing OrElse shape.Length = 0 Then
                Return New Integer() {size}
            End If

            Dim n As Long = 1

            For Each axis As Integer In shape
                If axis <= 0 Then
                    Throw New ArgumentException($"invalid numpy array dimension: {axis}", NameOf(shape))
                End If

                n *= axis
            Next

            If n <> size Then
                Throw New ArgumentException($"numpy array shape does not match the data size: shape={String.Join(", ", shape)} but {size} elements were given!", NameOf(shape))
            End If

            Return DirectCast(shape.Clone(), Integer())
        End Function

        <System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)>
        Private Function Serialize(Of T)(data As T(), itemSize As Integer) As Byte()
            Dim bytes As Byte() = New Byte(data.Length * itemSize - 1) {}

            Call System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length)

            If Not BitConverter.IsLittleEndian AndAlso itemSize > 1 Then
                ' 大端机器：把数据翻转成小端序落盘
                Dim half As Integer = itemSize \ 2

                For offset As Integer = 0 To bytes.Length - 1 Step itemSize
                    For i As Integer = 0 To half - 1
                        Dim a As Integer = offset + i
                        Dim b As Integer = offset + itemSize - 1 - i
                        Dim tmp As Byte = bytes(a)

                        bytes(a) = bytes(b)
                        bytes(b) = tmp
                    Next
                Next
            End If

            Return bytes
        End Function

        Private Function CodePointCount(s As String) As Integer
            If String.IsNullOrEmpty(s) Then
                Return 0
            End If

            Dim n As Integer = 0
            Dim i As Integer = 0

            While i < s.Length
                If Char.IsHighSurrogate(s(i)) AndAlso i + 1 < s.Length AndAlso Char.IsLowSurrogate(s(i + 1)) Then
                    i += 2
                Else
                    i += 1
                End If

                n += 1
            End While

            Return n
        End Function

        Private Sub EncodeUtf32(s As String, buffer As Byte(), offset As Integer, capacity As Integer)
            If String.IsNullOrEmpty(s) Then
                Return
            End If

            Dim i As Integer = 0
            Dim [index] As Integer = 0

            While i < s.Length AndAlso [index] < capacity
                Dim codePoint As Integer

                If Char.IsHighSurrogate(s(i)) AndAlso i + 1 < s.Length AndAlso Char.IsLowSurrogate(s(i + 1)) Then
                    codePoint = Char.ConvertToUtf32(s(i), s(i + 1))
                    i += 2
                Else
                    codePoint = AscW(s(i))

                    If codePoint < 0 Then
                        codePoint += 65536
                    End If

                    i += 1
                End If

                Dim chunk As Byte() = BitConverter.GetBytes(codePoint)

                Call Array.Copy(chunk, 0, buffer, offset + [index] * 4, 4)

                [index] += 1
            End While
        End Sub

        ''' <summary>
        ''' 读取 npy 文件并直接转换为矩阵（仅一维或二维数值数组可用）。
        ''' </summary>
        ''' <param name="path">npy 文件路径</param>
        Public Function ReadMatrix(path As String) As NumericMatrix
            Return Read(path).AsMatrix()
        End Function

    End Module

End Namespace

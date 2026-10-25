Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace Numpy

    ''' <summary>
    ''' numpy 数组的内存表示：形状、数据类型、内存布局与数据区。
    ''' </summary>
    ''' <remarks>
    ''' 数值、布尔、字符串与原始字节类型的数据保存在 <see cref="RawData"/> 中（与文件中的数据区逐字节一致），
    ''' Python 对象数组（<c>dtype='O'</c>）的元素保存在 <see cref="Objects"/> 中。
    ''' 
    ''' 所有访问器返回的数据都遵循 numpy 的 <c>ravel(order='C')</c> 语义：Fortran 序存储的数组会在首次访问时
    ''' 重排为行优先，字节序也会归一化到本机字节序，重排结果会被缓存。
    ''' </remarks>
    Public Class NpyArray

        ''' <summary>
        ''' 数组名称（来自 npz 容器的条目名；npy 文件为 <c>Nothing</c>）。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 元素数据类型。
        ''' </summary>
        Public Property DType As NpyDType

        ''' <summary>
        ''' 数组形状；0 维数组为空数组。
        ''' </summary>
        Public Property Shape As Integer()

        ''' <summary>
        ''' 数据区是否以 Fortran 序（列优先）存放。
        ''' </summary>
        Public Property FortranOrder As Boolean

        ''' <summary>
        ''' 数值/布尔/字符串/原始字节类型的数据区（逐字节对应文件内容）。
        ''' </summary>
        Public Property RawData As Byte()

        ''' <summary>
        ''' Python 对象数组的元素（仅 <c>dtype='O'</c> 时非空）。
        ''' </summary>
        Public Property Objects As Object()

        Private _normalized As Byte()
        Private _doubles As Double()
        Private _strings As String()
        Private _objects As Object()

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
        ''' 单个元素的字节宽度。
        ''' </summary>
        Public ReadOnly Property ItemSize As Integer
            Get
                Return DType.ItemSize
            End Get
        End Property

        ''' <summary>
        ''' 是否为 Python 对象数组。
        ''' </summary>
        Public ReadOnly Property IsObjectArray As Boolean
            Get
                Return DType IsNot Nothing AndAlso DType.Kind = NpyTypeKind.PyObject
            End Get
        End Property

        ''' <summary>
        ''' 是否为二维及以上的数值数组（可以直接转换为矩阵）。
        ''' </summary>
        Public ReadOnly Property IsMatrixLike As Boolean
            Get
                Return Rank = 2 AndAlso DType.IsNumeric AndAlso Not IsObjectArray
            End Get
        End Property

        ''' <summary>
        ''' 按 C 序线性下标读取一个元素。
        ''' </summary>
        ''' <param name="index">C 序线性下标</param>
        ''' <returns>布尔 / 整数 / 浮点 / 字符串 / 字节或内嵌的 <see cref="NpyArray"/></returns>
        Public Function GetValue(index As Long) As Object
            If index < 0 OrElse index >= Length Then
                Throw New IndexOutOfRangeException($"numpy array index out of range: {index} (size={Length})")
            End If

            If IsObjectArray Then
                Return AsObjectArray()(CInt(index))
            End If

            Dim buffer As Byte() = NormalizedData()
            Dim offset As Integer = CInt(index) * ItemSize

            Select Case DType.Kind
                Case NpyTypeKind.Bool
                    Return buffer(offset) <> 0
                Case NpyTypeKind.Int, NpyTypeKind.Datetime, NpyTypeKind.Timedelta
                    Select Case DType.ItemSize
                        Case 1
                            Dim b As Byte = buffer(offset)

                            If b > 127 Then
                                Return CLng(b) - 256
                            Else
                                Return CLng(b)
                            End If
                        Case 2
                            Return CLng(BitConverter.ToInt16(buffer, offset))
                        Case 4
                            Return CLng(BitConverter.ToInt32(buffer, offset))
                        Case Else
                            Return BitConverter.ToInt64(buffer, offset)
                    End Select
                Case NpyTypeKind.UInt
                    Select Case DType.ItemSize
                        Case 1
                            Return CULng(buffer(offset))
                        Case 2
                            Return CULng(BitConverter.ToUInt16(buffer, offset))
                        Case 4
                            Return CULng(BitConverter.ToUInt32(buffer, offset))
                        Case Else
                            Return BitConverter.ToUInt64(buffer, offset)
                    End Select
                Case NpyTypeKind.Float
                    Select Case DType.ItemSize
                        Case 2
                            Return CDbl(BitConverter.ToHalf(buffer, offset))
                        Case 4
                            Return CDbl(BitConverter.ToSingle(buffer, offset))
                        Case Else
                            Return BitConverter.ToDouble(buffer, offset)
                    End Select
                Case NpyTypeKind.Complex
                    Return ReadComplex(buffer, offset)
                Case NpyTypeKind.Bytes, NpyTypeKind.Unicode
                    Return DecodeString(buffer, offset)
                Case NpyTypeKind.Void
                    Dim chunk As Byte() = New Byte(ItemSize - 1) {}

                    Call Array.Copy(buffer, offset, chunk, 0, ItemSize)
                    Return chunk
                Case Else
                    Throw New NotSupportedException($"unsupported numpy dtype: {DType}")
            End Select
        End Function

        ''' <summary>
        ''' 按多维下标读取一个元素。
        ''' </summary>
        ''' <param name="indices">从最外层到最内层的下标，个数必须等于数组维数</param>
        Public Function Item(ParamArray indices As Integer()) As Object
            Dim linear As Long = 0

            If indices Is Nothing OrElse indices.Length = 0 Then
                Return GetValue(0)
            End If
            If indices.Length <> Rank Then
                Throw New ArgumentException($"numpy array expects {Rank} indices but {indices.Length} were given!", NameOf(indices))
            End If

            Dim strides As Integer() = CStrides()

            For i As Integer = 0 To indices.Length - 1
                If indices(i) < 0 OrElse indices(i) >= Shape(i) Then
                    Throw New IndexOutOfRangeException($"numpy array index out of range at axis {i}: {indices(i)} (size={Shape(i)})")
                End If

                linear += indices(i) * strides(i)
            Next

            Return GetValue(linear)
        End Function

        ''' <summary>
        ''' 以 <see cref="Double"/> 数组返回所有元素（C 序线性展开）。
        ''' </summary>
        ''' <remarks>整数与布尔会被提升为浮点；复数只返回实部。</remarks>
        Public Function AsDouble() As Double()
            If _doubles IsNot Nothing Then
                Return _doubles
            End If
            If IsObjectArray Then
                Throw New InvalidOperationException("can not convert a numpy object array to a double array!")
            End If

            Dim buffer As Byte() = NormalizedData()
            Dim n As Integer = ElementCount()
            Dim values As Double() = New Double(n - 1) {}

            Select Case DType.Kind
                Case NpyTypeKind.Float
                    Select Case DType.ItemSize
                        Case 2
                            For i As Integer = 0 To n - 1
                                values(i) = CDbl(BitConverter.ToHalf(buffer, i * 2))
                            Next
                        Case 4
                            Dim singles As Single() = New Single(n - 1) {}

                            Call Buffer.BlockCopy(buffer, 0, singles, 0, n * 4)

                            For i As Integer = 0 To n - 1
                                values(i) = CDbl(singles(i))
                            Next
                        Case 8
                            ' 数据区已经归一化成 C 序与本机字节序，可以直接整体拷贝
                            Call Buffer.BlockCopy(buffer, 0, values, 0, n * 8)
                        Case Else
                            Throw New NotSupportedException($"unsupported numpy float width: {DType.ItemSize} bytes")
                    End Select
                Case NpyTypeKind.Int
                    For i As Integer = 0 To n - 1
                        values(i) = CDbl(GetValue(i))
                    Next
                Case NpyTypeKind.UInt
                    For i As Integer = 0 To n - 1
                        values(i) = CDbl(GetValue(i))
                    Next
                Case NpyTypeKind.Bool
                    For i As Integer = 0 To n - 1
                        values(i) = If(buffer(i) <> 0, 1.0, 0.0)
                    Next
                Case NpyTypeKind.Complex
                    Dim size As Integer = DType.ItemSize

                    For i As Integer = 0 To n - 1
                        values(i) = If(size = 8,
                            CDbl(BitConverter.ToSingle(buffer, i * size)),
                            BitConverter.ToDouble(buffer, i * size))
                    Next
                Case Else
                    Throw New NotSupportedException($"numpy dtype '{DType}' can not be converted to a double array!")
            End Select

            _doubles = values

            Return values
        End Function

        ''' <summary>
        ''' 以 <see cref="Single"/> 数组返回所有元素（C 序线性展开）。
        ''' </summary>
        Public Function AsSingle() As Single()
            Dim values As Double() = AsDouble()
            Dim result As Single() = New Single(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                result(i) = CSng(values(i))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 以 <see cref="Long"/> 数组返回所有元素（C 序线性展开）。
        ''' </summary>
        Public Function AsInt64() As Long()
            Dim values As Double() = AsDouble()
            Dim result As Long() = New Long(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                result(i) = CLng(values(i))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 以 <see cref="Integer"/> 数组返回所有元素（C 序线性展开）。
        ''' </summary>
        Public Function AsInt32() As Integer()
            Dim values As Double() = AsDouble()
            Dim result As Integer() = New Integer(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                result(i) = CInt(values(i))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 以 <see cref="Boolean"/> 数组返回所有元素（0 视为 <c>False</c>）。
        ''' </summary>
        Public Function AsBoolean() As Boolean()
            Dim values As Double() = AsDouble()
            Dim result As Boolean() = New Boolean(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                result(i) = values(i) <> 0
            Next

            Return result
        End Function

        ''' <summary>
        ''' 以字符串数组返回所有元素（仅 <c>S</c> 与 <c>U</c> 类型可用）。
        ''' </summary>
        Public Function AsString() As String()
            If _strings IsNot Nothing Then
                Return _strings
            End If
            If Not DType.IsStringType Then
                Throw New InvalidOperationException($"numpy dtype '{DType}' is not a string type!")
            End If

            Dim n As Integer = ElementCount()
            Dim buffer As Byte() = NormalizedData()
            Dim result As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                result(i) = DecodeString(buffer, i * ItemSize)
            Next

            _strings = result

            Return result
        End Function

        ''' <summary>
        ''' 以对象数组返回所有元素（对象数组返回内嵌元素本身）。
        ''' </summary>
        Public Function AsObjectArray() As Object()
            If _objects IsNot Nothing Then
                Return _objects
            End If

            If IsObjectArray Then
                If Objects Is Nothing Then
                    _objects = New Object() {}
                Else
                    _objects = DirectCast(Objects.Clone(), Object())
                End If

                Return _objects
            End If

            Dim n As Integer = ElementCount()
            Dim result As Object() = New Object(n - 1) {}

            For i As Integer = 0 To n - 1
                result(i) = GetValue(i)
            Next

            _objects = result

            Return result
        End Function

        ''' <summary>
        ''' 以行优先的二维数组返回数据（仅一维或二维数值数组可用）。
        ''' </summary>
        Public Function As2D() As Double(,)
            Dim rows As Integer = 0
            Dim cols As Integer = 0

            Select Case Rank
                Case 1
                    rows = 1
                    cols = Shape(0)
                Case 2
                    rows = Shape(0)
                    cols = Shape(1)
                Case Else
                    Throw New InvalidOperationException($"numpy array with rank {Rank} can not be converted to a 2D matrix!")
            End Select

            Dim values As Double() = AsDouble()
            Dim result As Double(,) = New Double(rows - 1, cols - 1) {}

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    result(r, c) = values(r * cols + c)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 转换为 sciBASIC# 的矩阵对象（仅一维或二维数值数组可用）。
        ''' </summary>
        Public Function AsMatrix() As NumericMatrix
            Dim rows As Integer = 0
            Dim cols As Integer = 0

            Select Case Rank
                Case 1
                    rows = 1
                    cols = Shape(0)
                Case 2
                    rows = Shape(0)
                    cols = Shape(1)
                Case Else
                    Throw New InvalidOperationException($"numpy array with rank {Rank} can not be converted to a matrix!")
            End Select

            Dim values As Double() = AsDouble()
            Dim buffer As Double()() = New Double(rows - 1)() {}

            For r As Integer = 0 To rows - 1
                Dim line As Double() = New Double(cols - 1) {}

                For c As Integer = 0 To cols - 1
                    line(c) = values(r * cols + c)
                Next

                buffer(r) = line
            Next

            Return New NumericMatrix(buffer)
        End Function

        ''' <summary>
        ''' 返回数组的 C 序行优先步长。
        ''' </summary>
        Public Function CStrides() As Integer()
            If Shape Is Nothing OrElse Shape.Length = 0 Then
                Return New Integer() {}
            End If

            Dim strides As Integer() = New Integer(Shape.Length - 1) {}
            Dim acc As Integer = 1

            For i As Integer = Shape.Length - 1 To 0 Step -1
                strides(i) = acc
                acc *= Shape(i)
            Next

            Return strides
        End Function

        ''' <summary>
        ''' 返回 C 序、本机字节序的数据区副本（首次访问时计算并缓存）。
        ''' </summary>
        Private Function NormalizedData() As Byte()
            If _normalized IsNot Nothing Then
                Return _normalized
            End If
            If RawData Is Nothing Then
                Throw New InvalidOperationException("numpy array has no raw data buffer!")
            End If

            Dim buffer As Byte() = RawData

            If FortranOrder AndAlso Rank > 1 Then
                buffer = ReorderToC(buffer, ElementCount(), ItemSize, Shape)
            End If

            If DType.NeedsByteSwap Then
                buffer = DirectCast(buffer.Clone(), Byte())
                SwapByteOrder(buffer, DType, ElementCount())
            End If

            _normalized = buffer

            Return buffer
        End Function

        ''' <summary>
        ''' 把 Fortran 序（列优先）的数据区重排成 C 序（行优先）。
        ''' </summary>
        Private Shared Function ReorderToC(raw As Byte(), n As Integer, itemSize As Integer, shape As Integer()) As Byte()
            Dim rank As Integer = shape.Length
            Dim stridesC As Integer() = New Integer(rank - 1) {}
            Dim stridesF As Integer() = New Integer(rank - 1) {}

            Dim acc As Integer = 1

            For i As Integer = rank - 1 To 0 Step -1
                stridesC(i) = acc
                acc *= shape(i)
            Next

            acc = 1

            For i As Integer = 0 To rank - 1
                stridesF(i) = acc
                acc *= shape(i)
            Next

            Dim result As Byte() = New Byte(n * itemSize - 1) {}
            Dim indices As Integer() = New Integer(rank - 1) {}
            Dim dst As Integer = 0

            For k As Integer = 0 To n - 1
                Dim src As Integer = 0

                For j As Integer = 0 To rank - 1
                    src += indices(j) * stridesF(j)
                Next

                Call Buffer.BlockCopy(raw, src * itemSize, result, dst, itemSize)

                dst += itemSize

                For j As Integer = rank - 1 To 0 Step -1
                    indices(j) += 1

                    If indices(j) < shape(j) Then
                        Exit For
                    End If

                    indices(j) = 0
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 就地反转数据区中每个数值分量的字节序。
        ''' </summary>
        Private Shared Sub SwapByteOrder(buffer As Byte(), dtype As NpyDType, n As Integer)
            Dim component As Integer = dtype.ItemSize

            Select Case dtype.Kind
                Case NpyTypeKind.Complex
                    component = dtype.ItemSize \ 2
                Case NpyTypeKind.Unicode
                    component = 4
                Case NpyTypeKind.Bool, NpyTypeKind.Bytes, NpyTypeKind.Void, NpyTypeKind.PyObject
                    Return
            End Select

            If component <= 1 Then
                Return
            End If

            Dim half As Integer = component \ 2
            Dim total As Integer = n * dtype.ItemSize

            For offset As Integer = 0 To total - 1 Step component
                For i As Integer = 0 To half - 1
                    Dim a As Integer = offset + i
                    Dim b As Integer = offset + component - 1 - i
                    Dim t As Byte = buffer(a)

                    buffer(a) = buffer(b)
                    buffer(b) = t
                Next
            Next
        End Sub

        Private Function ReadComplex(buffer As Byte(), offset As Integer) As System.Numerics.Complex
            If DType.ItemSize = 8 Then
                Return New System.Numerics.Complex(
                    CDbl(BitConverter.ToSingle(buffer, offset)),
                    CDbl(BitConverter.ToSingle(buffer, offset + 4))
                )
            Else
                Return New System.Numerics.Complex(
                    BitConverter.ToDouble(buffer, offset),
                    BitConverter.ToDouble(buffer, offset + 8)
                )
            End If
        End Function

        Private Function DecodeString(buffer As Byte(), offset As Integer) As String
            Select Case DType.Kind
                Case NpyTypeKind.Bytes
                    Dim bytes As Byte() = New Byte(ItemSize - 1) {}

                    Call Array.Copy(buffer, offset, bytes, 0, ItemSize)

                    Return Encoding.Latin1.GetString(bytes).TrimEnd(vbNullChar)
                Case NpyTypeKind.Unicode
                    Dim sb As New StringBuilder()
                    Dim count As Integer = DType.CharCapacity

                    For i As Integer = 0 To count - 1
                        Dim cp As Integer = BitConverter.ToInt32(buffer, offset + i * 4)

                        If cp = 0 Then
                            Exit For
                        End If

                        sb.Append(Char.ConvertFromUtf32(cp))
                    Next

                    Return sb.ToString()
                Case Else
                    Throw New InvalidOperationException($"numpy dtype '{DType}' is not a string type!")
            End Select
        End Function

        Private Function ElementCount() As Integer
            If Length > Integer.MaxValue Then
                Throw New NotSupportedException($"numpy array is too large: {Length} elements")
            End If

            Return CInt(Length)
        End Function

        ''' <summary>
        ''' 返回 <c>npy_array dtype='&lt;f8' shape=(14464, 160) elements=2314240</c> 形式的摘要文本。
        ''' </summary>
        Public Overrides Function ToString() As String
            Dim shapeText As String

            If Shape Is Nothing OrElse Shape.Length = 0 Then
                shapeText = "()"
            Else
                shapeText = "(" & String.Join(", ", Shape) & ")"
            End If

            Dim nameText As String = If(Name Is Nothing, "", $" name='{Name}'")

            Return $"npy_array{nameText} dtype='{DType?.Descriptor}' shape={shapeText} elements={Length} fortran={FortranOrder}"
        End Function

    End Class

End Namespace

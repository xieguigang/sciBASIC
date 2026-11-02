' /********************************************************************************/
'  Microsoft.VisualBasic.MachineLearning.DataStorage
'  Data_science\MachineLearning\MLDataStorage\ONNX\OnnxTensor.vb
' /********************************************************************************/

Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX

    ''' <summary>
    ''' 运行时的稠密张量：形状 + 行主序（row-major）的 <see cref="Double"/> 数值。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 框架内部（<c>DataBlock</c> / <c>Tensor</c>）的数值一律是双精度，因此 ONNX 侧的所有数据类型在读取时
    ''' 都被统一提升为 <see cref="Double"/>；只有在**写出**时才根据 <see cref="TensorDataType"/> 决定落盘精度
    ''' （默认单精度，可选双精度）。
    ''' </para>
    ''' <para>
    ''' 行主序意味着最后一个维度的下标变化最快，这与 ONNX 的张量语义一致，也与
    ''' <c>DataBlock</c> 的 channel-last 内存布局同序（后者可以零拷贝地重解释为 <c>[1, SY, SX, Depth]</c>）。
    ''' </para>
    ''' </remarks>
    Public Class OnnxTensor

        ''' <summary>各维度的长度。</summary>
        Public ReadOnly Property Dims As Integer()

        ''' <summary>行主序的数值。</summary>
        Public ReadOnly Property Data As Double()

        ''' <summary>张量的秩（维度个数）。</summary>
        Public ReadOnly Property Rank As Integer
            Get
                Return If(Dims Is Nothing, 0, Dims.Length)
            End Get
        End Property

        ''' <summary>元素总个数。</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return Data.Length
            End Get
        End Property

        ''' <summary>
        ''' 创建一个张量。
        ''' </summary>
        ''' <param name="dims">形状；Nothing 或空数组表示标量。</param>
        ''' <param name="data">行主序数值，长度必须与形状一致。</param>
        Public Sub New(dims As Integer(), data As Double())
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Me.Dims = If(dims Is Nothing, New Integer() {}, dims)
            Me.Data = data

            Dim expected As Integer = 1

            For Each d As Integer In Me.Dims
                If d < 0 Then
                    Throw New ArgumentException($"张量形状中存在非法维度：{d}", NameOf(dims))
                End If

                expected *= d
            Next

            If expected <> data.Length Then
                Throw New ArgumentException($"张量形状 [{String.Join(", ", Me.Dims)}] 需要 {expected} 个元素，但实际提供了 {data.Length} 个")
            End If
        End Sub

        ''' <summary>以指定形状包装现有数组。</summary>
        ''' <param name="data">行主序数值。</param>
        ''' <param name="dims">形状。</param>
        ''' <returns>新的张量视图（不会复制数据）。</returns>
        Public Shared Function FromArray(data As Double(), ParamArray dims As Integer()) As OnnxTensor
            Return New OnnxTensor(dims, data)
        End Function

        ''' <summary>创建一个标量张量。</summary>
        ''' <param name="value">标量值。</param>
        ''' <returns>0 阶张量。</returns>
        Public Shared Function Scalar(value As Double) As OnnxTensor
            Return New OnnxTensor(New Integer() {}, New Double() {value})
        End Function

        ''' <summary>创建一个指定形状的零张量。</summary>
        ''' <param name="dims">形状。</param>
        ''' <returns>全零张量。</returns>
        Public Shared Function Zeros(ParamArray dims As Integer()) As OnnxTensor
            Dim n As Integer = 1

            For Each d As Integer In dims
                n *= d
            Next

            Return New OnnxTensor(dims, New Double(n - 1) {})
        End Function

        ''' <summary>创建一个指定形状的全一张量。</summary>
        ''' <param name="dims">形状。</param>
        ''' <returns>全一张量。</returns>
        Public Shared Function Ones(ParamArray dims As Integer()) As OnnxTensor
            Dim t As OnnxTensor = Zeros(dims)

            For i As Integer = 0 To t.Data.Length - 1
                t.Data(i) = 1.0
            Next

            Return t
        End Function

        ''' <summary>返回 <c>[2, 3]</c> 形式的形状文本。</summary>
        Public ReadOnly Property ShapeText As String
            Get
                Return "[" & String.Join(", ", Dims) & "]"
            End Get
        End Property

        ''' <summary>
        ''' 在不改变数据的前提下改变形状。
        ''' </summary>
        ''' <param name="dims">新的形状；其中最多允许出现一个 -1，由方法自动推断。</param>
        ''' <returns>新的张量视图。</returns>
        Public Function Reshape(ParamArray dims As Integer()) As OnnxTensor
            Dim resolved As Integer() = ResolveDims(Dims, Data.Length, dims)

            Return New OnnxTensor(resolved, Data)
        End Function

        ''' <summary>
        ''' 按 ONNX 的 <c>Reshape</c> 规则解析目标形状：支持 0（沿用对应输入维度）与 -1（自动推断）。
        ''' </summary>
        ''' <param name="sourceDims">输入张量的形状。</param>
        ''' <param name="length">元素总个数。</param>
        ''' <param name="target">目标形状（可能含 0 / -1）。</param>
        ''' <returns>解析后的形状。</returns>
        Public Shared Function ResolveDims(sourceDims As Integer(), length As Integer, target As Integer()) As Integer()
            If target Is Nothing OrElse target.Length = 0 Then
                Return New Integer() {}
            End If

            Dim resolved As Integer() = New Integer(target.Length - 1) {}
            Dim known As Integer = 1
            Dim inferred As Integer = -1

            For i As Integer = 0 To target.Length - 1
                Dim d As Integer = target(i)

                If d = -1 Then
                    If inferred >= 0 Then
                        Throw New ArgumentException("目标形状中最多只能出现一个 -1")
                    End If

                    inferred = i
                    resolved(i) = 1
                ElseIf d = 0 Then
                    If i >= sourceDims.Length Then
                        Throw New ArgumentException($"目标形状第 {i} 维为 0，但输入张量只有 {sourceDims.Length} 维")
                    End If

                    resolved(i) = sourceDims(i)
                    known *= If(resolved(i) > 0, resolved(i), 1)
                ElseIf d > 0 Then
                    resolved(i) = d
                    known *= d
                Else
                    Throw New ArgumentException($"非法的目标维度：{d}")
                End If
            Next

            If inferred >= 0 Then
                If known = 0 OrElse length Mod known <> 0 Then
                    Throw New ArgumentException($"无法把 {length} 个元素重排为 [{String.Join(", ", target)}]")
                End If

                resolved(inferred) = length \ known
            Else
                Dim total As Integer = 1

                For Each d As Integer In resolved
                    total *= d
                Next

                If total <> length Then
                    Throw New ArgumentException($"重排后的形状 [{String.Join(", ", resolved)}] 需要 {total} 个元素，但实际有 {length} 个")
                End If
            End If

            Return resolved
        End Function

        ''' <summary>
        ''' 交换两个维度的位置（用于 NCHW ↔ NHWC 之类的视图变换）。
        ''' </summary>
        ''' <param name="axisA">第一个维度下标。</param>
        ''' <param name="axisB">第二个维度下标。</param>
        ''' <returns>重排后的新张量。</returns>
        Public Function SwapAxis(axisA As Integer, axisB As Integer) As OnnxTensor
            Dim permutation As Integer() = Enumerable.Range(0, Rank).ToArray()

            permutation(axisA) = axisB
            permutation(axisB) = axisA

            Return Transpose(permutation)
        End Function

        ''' <summary>
        ''' 按给定的维度顺序重排张量。
        ''' </summary>
        ''' <param name="permutation">新的维度顺序，例如 NCHW → NHWC 为 <c>{0, 2, 3, 1}</c>。</param>
        ''' <returns>重排后的新张量。</returns>
        Public Function Transpose(permutation As Integer()) As OnnxTensor
            Dim outDims As Integer() = permutation.Select(Function(i) Dims(i)).ToArray()
            Dim outData As Double() = New Double(Data.Length - 1) {}

            ' 行主序的通用重排：逐元素计算源/目标的下标
            Dim strides As Integer() = GetStrides(Dims)
            Dim outStrides As Integer() = GetStrides(outDims)
            Dim index As Integer() = New Integer(Rank - 1) {}

            For i As Integer = 0 To outData.Length - 1
                Dim remain As Integer = i
                Dim src As Integer = 0

                For k As Integer = 0 To outDims.Length - 1
                    Dim coord As Integer = remain \ outStrides(k)

                    remain = remain Mod outStrides(k)
                    index(k) = coord
                Next

                For k As Integer = 0 To permutation.Length - 1
                    src += index(k) * strides(permutation(k))
                Next

                outData(i) = Data(src)
            Next

            Return New OnnxTensor(outDims, outData)
        End Function

        ''' <summary>
        ''' 取得按行主序展开时每个维度的步长。
        ''' </summary>
        ''' <param name="dims">形状。</param>
        ''' <returns>步长数组。</returns>
        Public Shared Function GetStrides(dims As Integer()) As Integer()
            Dim strides As Integer() = New Integer(dims.Length - 1) {}
            Dim acc As Integer = 1

            For i As Integer = dims.Length - 1 To 0 Step -1
                strides(i) = acc
                acc *= dims(i)
            Next

            Return strides
        End Function

        ''' <summary>
        ''' 从 <see cref="TensorProto"/> 读取张量，统一转换为 double 数值视图。
        ''' </summary>
        ''' <param name="tensor">ONNX 张量原型。</param>
        ''' <returns>运行时张量。</returns>
        Public Shared Function FromTensorProto(tensor As TensorProto) As OnnxTensor
            If tensor Is Nothing Then
                Throw New ArgumentNullException(NameOf(tensor))
            End If

            If tensor.DataLocation = DataLocation.External Then
                Throw New NotSupportedException(
                    $"张量 {tensor.Name} 的数据存放在外部文件中（external_data），当前实现只支持内嵌数据")
            End If

            Dim dims As Integer() = tensor.GetShape()
            Dim n As Integer = 1

            For Each d As Integer In dims
                n *= d
            Next

            If dims.Length = 0 Then
                n = 1
            End If

            Dim data As Double()

            If tensor.RawData IsNot Nothing AndAlso tensor.RawData.Length > 0 Then
                data = DecodeRaw(tensor, dims, n)
            Else
                data = DecodeTyped(tensor, n)
            End If

            Return New OnnxTensor(dims, data)
        End Function

        Private Shared Function DecodeRaw(tensor As TensorProto, dims As Integer(), n As Integer) As Double()
            Dim bytes As Byte() = tensor.RawData
            Dim type As TensorDataType = tensor.DataType
            Dim size As Integer = TensorDataTypeHelper.GetElementSize(type)

            If size <= 0 Then
                Throw New NotSupportedException($"张量 {tensor.Name} 的数据类型 {type} 不支持从 raw_data 读取")
            End If

            Dim expected As Integer = n * size

            If bytes.Length <> expected Then
                Throw New InvalidOperationException(
                    $"张量 {tensor.Name} 的 raw_data 长度为 {bytes.Length} 字节，但形状 [{String.Join(", ", dims)}] " &
                    $"与数据类型 {type} 要求 {expected} 字节")
            End If

            Dim result As Double() = New Double(n - 1) {}

            Select Case type
                Case TensorDataType.Float
                    Dim tmp As Single() = ToArray(Of Single)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.Float64
                    result = ToArray(Of Double)(bytes)

                Case TensorDataType.Int64
                    Dim tmp As Long() = ToArray(Of Long)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.Int32
                    Dim tmp As Integer() = ToArray(Of Integer)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.UInt32
                    Dim tmp As UInteger() = ToArray(Of UInteger)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.UInt64
                    Dim tmp As ULong() = ToArray(Of ULong)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.UInt16
                    Dim tmp As UShort() = ToArray(Of UShort)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.Int16
                    Dim tmp As Short() = ToArray(Of Short)(bytes)

                    For i As Integer = 0 To n - 1
                        result(i) = tmp(i)
                    Next

                Case TensorDataType.UInt8
                    For i As Integer = 0 To n - 1
                        result(i) = bytes(i)
                    Next

                Case TensorDataType.Int8
                    For i As Integer = 0 To n - 1
                        result(i) = CByte(bytes(i)) - If(bytes(i) > 127, 256, 0)
                    Next

                Case TensorDataType.Bool
                    For i As Integer = 0 To n - 1
                        result(i) = If(bytes(i) <> 0, 1.0, 0.0)
                    Next

                Case Else
                    Throw New NotSupportedException($"张量 {tensor.Name} 的数据类型 {type} 暂不支持")
            End Select

            Return result
        End Function

        Private Shared Function DecodeTyped(tensor As TensorProto, n As Integer) As Double()
            Dim type As TensorDataType = tensor.DataType
            Dim result As Double() = New Double(n - 1) {}

            Select Case type
                Case TensorDataType.Float
                    If tensor.FloatData Is Nothing OrElse tensor.FloatData.Count = 0 Then
                        Return result
                    End If

                    For i As Integer = 0 To n - 1
                        result(i) = tensor.FloatData(i)
                    Next

                Case TensorDataType.Float64
                    If tensor.DoubleData Is Nothing OrElse tensor.DoubleData.Count = 0 Then
                        Return result
                    End If

                    Return tensor.DoubleData.Take(n).ToArray()

                Case TensorDataType.Int64
                    If tensor.Int64Data Is Nothing OrElse tensor.Int64Data.Count = 0 Then
                        Return result
                    End If

                    For i As Integer = 0 To n - 1
                        result(i) = tensor.Int64Data(i)
                    Next

                Case TensorDataType.Int32, TensorDataType.UInt8, TensorDataType.Int8,
                     TensorDataType.UInt16, TensorDataType.Int16, TensorDataType.Bool
                    If tensor.Int32Data Is Nothing OrElse tensor.Int32Data.Count = 0 Then
                        Return result
                    End If

                    For i As Integer = 0 To n - 1
                        result(i) = tensor.Int32Data(i)
                    Next

                Case TensorDataType.UInt64
                    If tensor.UInt64Data Is Nothing OrElse tensor.UInt64Data.Count = 0 Then
                        Return result
                    End If

                    For i As Integer = 0 To n - 1
                        result(i) = tensor.UInt64Data(i)
                    Next

                Case TensorDataType.Undefined
                    Throw New InvalidOperationException($"张量 {tensor.Name} 没有声明数据类型（data_type = UNDEFINED）")

                Case Else
                    Throw New NotSupportedException($"张量 {tensor.Name} 的数据类型 {type} 暂不支持")
            End Select

            Return result
        End Function

        Private Shared Function ToArray(Of T As Structure)(bytes As Byte()) As T()
            Dim size As Integer = System.Runtime.InteropServices.Marshal.SizeOf(GetType(T))
            Dim result As T() = New T(bytes.Length \ size - 1) {}

            Buffer.BlockCopy(bytes, 0, result, 0, result.Length * size)

            Return result
        End Function

        ''' <summary>
        ''' 转换为 ONNX 张量原型。
        ''' </summary>
        ''' <param name="name">张量名称。</param>
        ''' <param name="precision">
        ''' 落盘精度：<see cref="TensorDataType.Float"/>（默认，单精度）或 <see cref="TensorDataType.Float64"/>。
        ''' </param>
        ''' <param name="useRawData">是否使用紧凑的 <c>raw_data</c> 字节块（默认 True）。</param>
        ''' <returns>ONNX 张量原型。</returns>
        Public Function ToTensorProto(name As String,
                                      Optional precision As TensorDataType = TensorDataType.Float,
                                      Optional useRawData As Boolean = True) As TensorProto

            Dim proto As New TensorProto With {.Name = name}

            For Each d As Integer In Dims
                Call proto.Dims.Add(CLng(d))
            Next

            Select Case precision
                Case TensorDataType.Float
                    proto.DataType = TensorDataType.Float

                    Dim singles As Single() = New Single(Data.Length - 1) {}

                    For i As Integer = 0 To Data.Length - 1
                        singles(i) = CSng(Data(i))
                    Next

                    If useRawData Then
                        proto.RawData = ToBytes(singles)
                    Else
                        proto.FloatData = New List(Of Single)(singles)
                    End If

                Case TensorDataType.Float64
                    proto.DataType = TensorDataType.Float64

                    If useRawData Then
                        proto.RawData = ToBytes(Data)
                    Else
                        proto.DoubleData = New List(Of Double)(Data)
                    End If

                Case Else
                    Throw New NotSupportedException($"不支持写出 {precision} 精度的张量；只能使用 Float 或 Float64")
            End Select

            Return proto
        End Function

        Private Shared Function ToBytes(Of T As Structure)(values As T()) As Byte()
            Dim size As Integer = System.Runtime.InteropServices.Marshal.SizeOf(GetType(T))
            Dim bytes As Byte() = New Byte(values.Length * size - 1) {}

            Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length)

            Return bytes
        End Function

        ''' <summary>返回 <c>shape [dims], first=...</c> 形式的文本。</summary>
        Public Overrides Function ToString() As String
            Dim preview As String = ""

            If Data.Length > 0 Then
                preview = $", first={Data(0):G6}"
            End If

            Return $"tensor{ShapeText} ({Data.Length} values{preview})"
        End Function

    End Class

End Namespace

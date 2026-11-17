' ------------------------------------------------------------------------
' 张量描述（形状 / 数据类型 / 步长）
'
' 这里描述的是"内存里的一个多维数组"，本身不持有显存；
' 显存由 DmlMemoryPool / DmlAllocation 负责，绑定关系由 DmlGraph 维护。
' ------------------------------------------------------------------------

Imports System.Text
Imports Microsoft.VisualBasic.Computing.DirectML.Interop

Namespace Runtime

    ''' <summary>
    ''' DirectML 缓冲区张量的形状描述。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 元素按行主序（最后一个维度变化最快）紧密排列；也可以显式给出 <see cref="Strides"/>
    ''' 实现"零拷贝的形状变换"（例如转置、切片视图）。
    ''' </para>
    ''' <para>
    ''' 形状最多 8 维（DirectML 1.12 之后的缓冲区张量上限）；更常见的用法是 4 维 NCHW。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class DmlTensor

        ''' <summary>最大维度数。</summary>
        Public Const MaxDimensions As Integer = 8

        ''' <summary>各维长度。</summary>
        Public ReadOnly Property Dims As UInteger()

        ''' <summary>元素数据类型。</summary>
        Public ReadOnly Property DataType As DML_TENSOR_DATA_TYPE

        ''' <summary>步长（以元素为单位）；Nothing 表示紧密排列。</summary>
        Public ReadOnly Property Strides As UInteger()

        ''' <summary>张量标志。</summary>
        Public ReadOnly Property Flags As DML_TENSOR_FLAGS

        ''' <summary>
        ''' 创建一个张量描述。
        ''' </summary>
        ''' <param name="dims">各维长度（至少一维）。</param>
        ''' <param name="dataType">元素类型，默认 FP32。</param>
        ''' <param name="strides">可选的显式步长。</param>
        ''' <param name="flags">张量标志。</param>
        Public Sub New(dims As UInteger(),
                       Optional dataType As DML_TENSOR_DATA_TYPE = DML_TENSOR_DATA_TYPE.FLOAT32,
                       Optional strides As UInteger() = Nothing,
                       Optional flags As DML_TENSOR_FLAGS = DML_TENSOR_FLAGS.NONE)

            If dims Is Nothing OrElse dims.Length = 0 Then
                Throw New ArgumentException("张量至少需要一个维度", NameOf(dims))
            End If

            If dims.Length > MaxDimensions Then
                Throw New ArgumentException($"张量维度数 {dims.Length} 超过 DirectML 支持的上限 {MaxDimensions}", NameOf(dims))
            End If

            For Each d As UInteger In dims
                If d = 0UI Then
                    Throw New ArgumentException("张量维度的长度不能为 0", NameOf(dims))
                End If
            Next

            If DmlTensorDataTypeHelper.GetElementSize(dataType) <= 0 Then
                Throw New ArgumentException($"不支持的数据类型：{dataType}", NameOf(dataType))
            End If

            Me.Dims = CType(dims.Clone(), UInteger())
            Me.DataType = dataType
            Me.Strides = If(strides Is Nothing, Nothing, CType(strides.Clone(), UInteger()))
            Me.Flags = flags

            If Me.Strides IsNot Nothing AndAlso Me.Strides.Length <> dims.Length Then
                Throw New ArgumentException("步长数组的长度必须与维度数一致", NameOf(strides))
            End If
        End Sub

        ''' <summary>张量的秩（维度数）。</summary>
        Public ReadOnly Property Rank As Integer
            Get
                Return Dims.Length
            End Get
        End Property

        ''' <summary>元素字节数。</summary>
        Public ReadOnly Property ElementSize As Integer
            Get
                Return DmlTensorDataTypeHelper.GetElementSize(DataType)
            End Get
        End Property

        ''' <summary>元素总个数。</summary>
        Public ReadOnly Property ElementCount As ULong
            Get
                Dim n As ULong = 1UL

                For Each d As UInteger In Dims
                    n *= d
                Next

                Return n
            End Get
        End Property

        ''' <summary>张量数据需要的字节数。</summary>
        Public ReadOnly Property TotalSizeInBytes As ULong
            Get
                If Strides Is Nothing Then
                    Return ElementCount * CULng(ElementSize)
                End If

                ' 显式步长：最大偏移 + 1 个元素
                Dim last As ULong = 0UL

                For i As Integer = 0 To Strides.Length - 1
                    last += CULng(Strides(i)) * CULng(Dims(i) - 1UI)
                Next

                Return (last + 1UL) * CULng(ElementSize)
            End Get
        End Property

        ''' <summary>取得按行主序紧密排列的默认步长（以元素为单位）。</summary>
        Public Shared Function GetDefaultStrides(dims As UInteger()) As UInteger()
            Dim strides As UInteger() = New UInteger(dims.Length - 1) {}
            Dim acc As UInteger = 1UI

            For i As Integer = dims.Length - 1 To 0 Step -1
                strides(i) = acc
                acc *= dims(i)
            Next

            Return strides
        End Function

        ''' <summary>返回一个形状不同、数据相同的新描述（用于 Reshape）。</summary>
        Public Function WithShape(ParamArray dims As UInteger()) As DmlTensor
            Return New DmlTensor(dims, DataType, Nothing, Flags)
        End Function

        ''' <summary>返回一个数据类型不同的新描述（用于 Cast）。</summary>
        Public Function WithType(dataType As DML_TENSOR_DATA_TYPE) As DmlTensor
            Return New DmlTensor(Dims, dataType, Strides, Flags)
        End Function

        ''' <summary>返回一个带显式步长的视图（用于转置等零拷贝变换）。</summary>
        Public Function WithStrides(strides As UInteger()) As DmlTensor
            Return New DmlTensor(Dims, DataType, strides, Flags)
        End Function

        ''' <summary>判断两个张量的形状与数据类型是否一致。</summary>
        Public Function IsCompatibleWith(other As DmlTensor) As Boolean
            If other Is Nothing OrElse other.DataType <> DataType OrElse other.Dims.Length <> Dims.Length Then
                Return False
            End If

            For i As Integer = 0 To Dims.Length - 1
                If other.Dims(i) <> Dims(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' 把本张量写成 <c>DML_TENSOR_DESC</c>，返回其非托管指针。
        ''' </summary>
        ''' <param name="builder">持有内存的构建器。</param>
        Public Function WriteDescriptor(builder As DmlOperatorBuilder) As IntPtr
            Dim sizesPtr As IntPtr = builder.UIntArray(Dims)
            Dim stridesPtr As IntPtr

            If Strides Is Nothing Then
                stridesPtr = IntPtr.Zero
            Else
                stridesPtr = builder.UIntArray(Strides)
            End If

            Dim buffer As New DML_BUFFER_TENSOR_DESC With {
                .DataType = DataType,
                .Flags = Flags,
                .DimensionCount = CUInt(Dims.Length),
                .Sizes = sizesPtr,
                .Strides = stridesPtr,
                .TotalTensorSizeInBytes = TotalSizeInBytes,
                .GuaranteedBaseOffsetAlignment = 0UI
            }

            Dim bufferPtr As IntPtr = builder.StructPtr(buffer)
            Dim desc As New DML_TENSOR_DESC(DML_TENSOR_TYPE.BUFFER, bufferPtr)

            Return builder.StructPtr(desc)
        End Function

        ''' <summary>返回 <c>[1, 3, 224, 224]</c> 形式的形状文本。</summary>
        Public ReadOnly Property ShapeText As String
            Get
                Dim sb As New StringBuilder("[")

                For i As Integer = 0 To Dims.Length - 1
                    If i > 0 Then
                        Call sb.Append(", ")
                    End If

                    Call sb.Append(Dims(i))
                Next

                Call sb.Append("]")

                Return sb.ToString()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"tensor{ShapeText} {DataType}"
        End Function

    End Class

End Namespace

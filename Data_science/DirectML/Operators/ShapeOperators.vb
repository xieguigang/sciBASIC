' ------------------------------------------------------------------------
' 形状 / 布局类算子
'
' 注意：Reshape、Squeeze、Unsqueeze、Flatten 在 DirectML 里没有对应算子，
' 它们只是"同一个缓冲区、不同的形状描述"，桥接层直接复用同一段显存即可。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>形状类算子工厂。</summary>
    Public Module ShapeOperators

        ''' <summary>原样复制（用于类型/形状无关的拷贝与占位）。</summary>
        Public Function Identity(input As DmlTensor, output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_IDENTITY, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ELEMENT_WISE_UNARY_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b)
                             })
                         End Function)
        End Function

        ''' <summary>类型转换。</summary>
        Public Function Cast(input As DmlTensor, output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.CAST, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_CAST_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b)
                             })
                         End Function)
        End Function

        ''' <summary>切片。</summary>
        Public Function Slice(input As DmlTensor, output As DmlTensor,
                              offsets As UInteger(), sizes As UInteger(), strides As UInteger(),
                              Optional name As String = Nothing) As DmlOperatorDefinition

            Dim count As Integer = input.Rank

            Return Build(DML_OPERATOR_TYPE.SLICE, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_SLICE_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .DimensionCount = CUInt(count),
                                 .Offsets = b.UIntArray(offsets),
                                 .Sizes = b.UIntArray(sizes),
                                 .Strides = b.UIntArray(strides)
                             })
                         End Function)
        End Function

        ''' <summary>按轴拼接。</summary>
        Public Function Join(inputs As DmlTensor(), output As DmlTensor, axis As UInteger,
                             Optional name As String = Nothing) As DmlOperatorDefinition

            If inputs Is Nothing OrElse inputs.Length = 0 Then
                Throw New ArgumentNullException(NameOf(inputs))
            End If

            Return Build(DML_OPERATOR_TYPE.JOIN, inputs, {output}, name,
                         Function(b)
                             Dim descriptors As IntPtr() = New IntPtr(inputs.Length - 1) {}

                             For i As Integer = 0 To inputs.Length - 1
                                 descriptors(i) = Desc(inputs(i), b)
                             Next

                             Return b.StructPtr(New DML_JOIN_OPERATOR_DESC With {
                                 .InputCount = CUInt(inputs.Length),
                                 .InputTensors = b.PointerArray(descriptors),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis
                             })
                         End Function)
        End Function

        ''' <summary>按轴切分（输出个数与尺寸由 DML 按输出描述符推导）。</summary>
        Public Function Split(input As DmlTensor, outputs As DmlTensor(), axis As UInteger,
                              Optional name As String = Nothing) As DmlOperatorDefinition

            If outputs Is Nothing OrElse outputs.Length = 0 Then
                Throw New ArgumentNullException(NameOf(outputs))
            End If

            Return Build(DML_OPERATOR_TYPE.SPLIT, {input}, outputs, name,
                         Function(b)
                             Dim descriptors As IntPtr() = New IntPtr(outputs.Length - 1) {}

                             For i As Integer = 0 To outputs.Length - 1
                                 descriptors(i) = Desc(outputs(i), b)
                             Next

                             Return b.StructPtr(New DML_SPLIT_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputCount = CUInt(outputs.Length),
                                 .OutputTensors = b.PointerArray(descriptors),
                                 .Axis = axis
                             })
                         End Function)
        End Function

        ''' <summary>填充。</summary>
        Public Function Pad(input As DmlTensor, output As DmlTensor,
                            mode As DML_PADDING_MODE,
                            startPadding As UInteger(), endPadding As UInteger(),
                            Optional value As Single = 0.0F,
                            Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.PADDING, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_PADDING_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .PaddingMode = mode,
                                 .PaddingValue = value,
                                 .DimensionCount = CUInt(input.Rank),
                                 .StartPadding = b.UIntArray(startPadding),
                                 .EndPadding = b.UIntArray(endPadding)
                             })
                         End Function)
        End Function

        ''' <summary>按轴重复（ONNX Tile）。</summary>
        Public Function Tile(input As DmlTensor, output As DmlTensor, repeats As UInteger(),
                             Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.TILE, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_TILE_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .RepeatsCount = CUInt(If(repeats Is Nothing, 0, repeats.Length)),
                                 .Repeats = b.UIntArray(repeats)
                             })
                         End Function)
        End Function

        ''' <summary>深度到空间（ONNX DepthToSpace）。</summary>
        Public Function DepthToSpace(input As DmlTensor, output As DmlTensor, blockSize As UInteger,
                                     Optional order As DML_DEPTH_SPACE_ORDER = DML_DEPTH_SPACE_ORDER.DEPTH_COLUMN_ROW,
                                     Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.DEPTH_TO_SPACE1, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_DEPTH_TO_SPACE1_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .BlockSize = blockSize,
                                 .Order = order
                             })
                         End Function)
        End Function

        ''' <summary>空间到深度（ONNX SpaceToDepth）。</summary>
        Public Function SpaceToDepth(input As DmlTensor, output As DmlTensor, blockSize As UInteger,
                                     Optional order As DML_DEPTH_SPACE_ORDER = DML_DEPTH_SPACE_ORDER.DEPTH_COLUMN_ROW,
                                     Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.SPACE_TO_DEPTH1, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_DEPTH_TO_SPACE1_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .BlockSize = blockSize,
                                 .Order = order
                             })
                         End Function)
        End Function

        ''' <summary>重采样（缩放因子）。</summary>
        Public Function Resample(input As DmlTensor, output As DmlTensor,
                                 mode As DML_INTERPOLATION_MODE,
                                 scales As Single(), inputOffsets As Single(), outputOffsets As Single(),
                                 Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.RESAMPLE1, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_RESAMPLE1_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .InterpolationMode = mode,
                                 .DimensionCount = CUInt(input.Rank),
                                 .Scales = b.SingleArray(scales),
                                 .InputPixelOffsets = b.SingleArray(inputOffsets),
                                 .OutputPixelOffsets = b.SingleArray(outputOffsets)
                             })
                         End Function)
        End Function

    End Module

End Namespace

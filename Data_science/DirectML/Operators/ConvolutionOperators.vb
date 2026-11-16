' ------------------------------------------------------------------------
' 卷积、反卷积与池化算子
'
' ONNX 的 Conv 是互相关（cross-correlation），因此 Mode 固定为 CROSS_CORRELATION；
' ONNX 的 ConvTranspose 对应 Direction = BACKWARD。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>卷积与池化算子工厂。</summary>
    Public Module ConvolutionOperators

        ''' <summary>
        ''' 卷积 / 反卷积。
        ''' </summary>
        ''' <param name="input">输入（NCHW 或 NCDHW）。</param>
        ''' <param name="filter">卷积核。</param>
        ''' <param name="bias">偏置（可为 Nothing）。</param>
        ''' <param name="output">输出。</param>
        ''' <param name="strides">每个空间维度的步长。</param>
        ''' <param name="dilations">每个空间维度的膨胀。</param>
        ''' <param name="startPadding">起点填充。</param>
        ''' <param name="endPadding">终点填充。</param>
        ''' <param name="outputPadding">输出填充（仅反卷积）。</param>
        ''' <param name="group">分组数。</param>
        ''' <param name="direction">FORWARD 或 BACKWARD。</param>
        ''' <param name="name">调试名称。</param>
        Public Function Convolution(input As DmlTensor,
                                    filter As DmlTensor,
                                    bias As DmlTensor,
                                    output As DmlTensor,
                                    strides As UInteger(),
                                    dilations As UInteger(),
                                    startPadding As UInteger(),
                                    endPadding As UInteger(),
                                    Optional outputPadding As UInteger() = Nothing,
                                    Optional group As UInteger = 1UI,
                                    Optional direction As DML_CONVOLUTION_DIRECTION = DML_CONVOLUTION_DIRECTION.FORWARD,
                                    Optional name As String = Nothing) As DmlOperatorDefinition

            If input Is Nothing OrElse filter Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(input))
            End If

            Dim spatial As Integer = input.Rank - 2

            If spatial < 1 Then
                Throw New ArgumentException("卷积输入至少需要 3 维（N, C, spatial）", NameOf(input))
            End If

            Return Build(DML_OPERATOR_TYPE.CONVOLUTION,
                         If(bias Is Nothing, {input, filter}, {input, filter, bias}),
                         {output},
                         name,
                         Function(b)
                             Dim biasPtr As IntPtr = IntPtr.Zero

                             If bias IsNot Nothing Then
                                 biasPtr = Desc(bias, b)
                             End If

                             Return b.StructPtr(New DML_CONVOLUTION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .FilterTensor = Desc(filter, b),
                                 .BiasTensor = biasPtr,
                                 .OutputTensor = Desc(output, b),
                                 .Mode = DML_CONVOLUTION_MODE.CROSS_CORRELATION,
                                 .Direction = direction,
                                 .DimensionCount = CUInt(spatial),
                                 .Strides = b.UIntArray(strides),
                                 .Dilations = b.UIntArray(dilations),
                                 .StartPadding = b.UIntArray(startPadding),
                                 .EndPadding = b.UIntArray(endPadding),
                                 .OutputPadding = b.UIntArray(outputPadding),
                                 .GroupCount = group,
                                 .FusedActivation = IntPtr.Zero
                             })
                         End Function)
        End Function

        ''' <summary>平均池化。</summary>
        ''' <param name="includePadding">是否把 padding 计入分母（ONNX 的 count_include_pad）。</param>
        Public Function AveragePool(input As DmlTensor, output As DmlTensor,
                                    windowSize As UInteger(), strides As UInteger(),
                                    startPadding As UInteger(), endPadding As UInteger(),
                                    Optional includePadding As Boolean = False,
                                    Optional name As String = Nothing) As DmlOperatorDefinition

            Dim spatial As Integer = input.Rank - 2

            Return Build(DML_OPERATOR_TYPE.AVERAGE_POOLING, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_AVERAGE_POOLING_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .DimensionCount = CUInt(spatial),
                                 .Strides = b.UIntArray(strides),
                                 .WindowSize = b.UIntArray(windowSize),
                                 .StartPadding = b.UIntArray(startPadding),
                                 .EndPadding = b.UIntArray(endPadding),
                                 .IncludePadding = BoolToInt(includePadding)
                             })
                         End Function)
        End Function

        ''' <summary>最大池化。</summary>
        Public Function MaxPool(input As DmlTensor, output As DmlTensor,
                                windowSize As UInteger(), strides As UInteger(),
                                startPadding As UInteger(), endPadding As UInteger(),
                                Optional dilations As UInteger() = Nothing,
                                Optional name As String = Nothing) As DmlOperatorDefinition

            Dim spatial As Integer = input.Rank - 2
            Dim hasDilation As Boolean = dilations IsNot Nothing AndAlso dilations.Length > 0

            If hasDilation Then
                Return Build(DML_OPERATOR_TYPE.MAX_POOLING2, {input}, {output}, name,
                             Function(b)
                                 Return b.StructPtr(New DML_MAX_POOLING2_OPERATOR_DESC With {
                                     .InputTensor = Desc(input, b),
                                     .OutputTensor = Desc(output, b),
                                     .OutputIndicesTensor = IntPtr.Zero,
                                     .DimensionCount = CUInt(spatial),
                                     .Strides = b.UIntArray(strides),
                                     .WindowSize = b.UIntArray(windowSize),
                                     .StartPadding = b.UIntArray(startPadding),
                                     .EndPadding = b.UIntArray(endPadding),
                                     .Dilations = b.UIntArray(dilations)
                                 })
                             End Function)
            End If

            Return Build(DML_OPERATOR_TYPE.MAX_POOLING, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_MAX_POOLING_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .DimensionCount = CUInt(spatial),
                                 .Strides = b.UIntArray(strides),
                                 .WindowSize = b.UIntArray(windowSize),
                                 .StartPadding = b.UIntArray(startPadding),
                                 .EndPadding = b.UIntArray(endPadding)
                             })
                         End Function)
        End Function

        ''' <summary>Lp 池化（ONNX 的 LpPool）。</summary>
        Public Function LpPool(input As DmlTensor, output As DmlTensor,
                               windowSize As UInteger(), strides As UInteger(),
                               startPadding As UInteger(), endPadding As UInteger(),
                               Optional p As UInteger = 2UI,
                               Optional name As String = Nothing) As DmlOperatorDefinition

            Dim spatial As Integer = input.Rank - 2

            Return Build(DML_OPERATOR_TYPE.LP_POOLING, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_LP_POOLING_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .DimensionCount = CUInt(spatial),
                                 .Strides = b.UIntArray(strides),
                                 .WindowSize = b.UIntArray(windowSize),
                                 .StartPadding = b.UIntArray(startPadding),
                                 .EndPadding = b.UIntArray(endPadding),
                                 .P = p
                             })
                         End Function)
        End Function

        ''' <summary>最大反池化（ONNX 的 MaxUnpool）。</summary>
        Public Function MaxUnpool(input As DmlTensor, indices As DmlTensor, output As DmlTensor,
                                  Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.MAX_UNPOOLING, {input, indices}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_MAX_UNPOOLING_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .IndicesTensor = Desc(indices, b),
                                 .OutputTensor = Desc(output, b)
                             })
                         End Function)
        End Function

    End Module

End Namespace

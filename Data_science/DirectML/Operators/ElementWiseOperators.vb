' ------------------------------------------------------------------------
' 逐元素算子
'
' Unary / Binary 用 DML_OPERATOR_TYPE 作为判别式，桥接层只需要给出算子类型，
' 不需要知道每个 DML 描述符的具体结构。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>逐元素算子工厂。</summary>
    Public Module ElementWiseOperators

        ''' <summary>
        ''' 一元逐元素算子。
        ''' </summary>
        ''' <param name="kind">支持 ABS/EXP/LOG/SQRT/RECIP/SIN/COS/TAN/FLOOR/CEIL/ERF/SIGN/NEGATE/LOGICAL_NOT/IDENTITY。</param>
        ''' <param name="input">输入。</param>
        ''' <param name="output">输出。</param>
        ''' <param name="scaleBias">可选的缩放/偏移（仅部分算子支持）。</param>
        ''' <param name="name">调试名称。</param>
        Public Function Unary(kind As DML_OPERATOR_TYPE,
                              input As DmlTensor,
                              output As DmlTensor,
                              Optional scaleBias As DML_SCALE_BIAS? = Nothing,
                              Optional name As String = Nothing) As DmlOperatorDefinition

            If input Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(input))
            End If

            Select Case kind
                Case DML_OPERATOR_TYPE.ELEMENT_WISE_SIGN, DML_OPERATOR_TYPE.ELEMENT_WISE_NEGATE,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_NOT, DML_OPERATOR_TYPE.ELEMENT_WISE_IDENTITY

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ELEMENT_WISE_UNARY_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b)
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ELEMENT_WISE_ABS, DML_OPERATOR_TYPE.ELEMENT_WISE_EXP,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOG, DML_OPERATOR_TYPE.ELEMENT_WISE_SQRT,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_RECIP, DML_OPERATOR_TYPE.ELEMENT_WISE_SIN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_COS, DML_OPERATOR_TYPE.ELEMENT_WISE_TAN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_FLOOR, DML_OPERATOR_TYPE.ELEMENT_WISE_CEIL,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_ERF

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Dim sb As IntPtr = IntPtr.Zero

                                     If scaleBias.HasValue Then
                                         sb = b.StructPtr(scaleBias.Value)
                                     End If

                                     Return b.StructPtr(New DML_ELEMENT_WISE_UNARY_SCALE_BIAS_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .ScaleBias = sb
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ELEMENT_WISE_ROUND

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ELEMENT_WISE_ROUND_OPERATOR_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .RoundingMode = DML_ROUNDING_MODE.HALVES_TO_NEAREST_EVEN
                                     })
                                 End Function)

                Case Else
                    Throw New NotSupportedException($"不支持的一元逐元素算子：{kind}")
            End Select
        End Function

        ''' <summary>
        ''' 二元逐元素算子。
        ''' </summary>
        ''' <param name="kind">
        ''' 支持 ADD/SUBTRACT/MULTIPLY/DIVIDE/MIN/MAX/MEAN/LOGICAL_AND/LOGICAL_OR/LOGICAL_XOR/
        ''' LOGICAL_EQUALS/LOGICAL_GREATER_THAN/LOGICAL_GREATER_THAN_OR_EQUAL/
        ''' LOGICAL_LESS_THAN/LOGICAL_LESS_THAN_OR_EQUAL/POW。
        ''' </param>
        Public Function Binary(kind As DML_OPERATOR_TYPE,
                               a As DmlTensor,
                               b As DmlTensor,
                               output As DmlTensor,
                               Optional name As String = Nothing) As DmlOperatorDefinition

            If a Is Nothing OrElse b Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(a))
            End If

            If kind = DML_OPERATOR_TYPE.ELEMENT_WISE_POW Then
                Return Build(kind, {a, b}, {output}, name,
                             Function(bb)
                                 Return bb.StructPtr(New DML_ELEMENT_WISE_POW_OPERATOR_DESC With {
                                     .InputTensor = Desc(a, bb),
                                     .ExponentTensor = Desc(b, bb),
                                     .OutputTensor = Desc(output, bb),
                                     .ScaleBias = IntPtr.Zero
                                 })
                             End Function)
            End If

            Select Case kind
                Case DML_OPERATOR_TYPE.ELEMENT_WISE_ADD,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_SUBTRACT,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_MULTIPLY,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_DIVIDE,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_MIN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_MAX,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_MEAN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_AND,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_OR,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_XOR,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_EQUALS,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_GREATER_THAN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_GREATER_THAN_OR_EQUAL,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_LESS_THAN,
                     DML_OPERATOR_TYPE.ELEMENT_WISE_LOGICAL_LESS_THAN_OR_EQUAL

                    Return Build(kind, {a, b}, {output}, name,
                                 Function(bb)
                                     Return bb.StructPtr(New DML_ELEMENT_WISE_BINARY_DESC With {
                                         .ATensor = Desc(a, bb),
                                         .BTensor = Desc(b, bb),
                                         .OutputTensor = Desc(output, bb)
                                     })
                                 End Function)

                Case Else
                    Throw New NotSupportedException($"不支持的二元逐元素算子：{kind}")
            End Select
        End Function

        ''' <summary>DML_ELEMENT_WISE_CLIP：把数值限制在 [min, max]。</summary>
        Public Function Clip(input As DmlTensor, output As DmlTensor, min As Single, max As Single,
                             Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_CLIP, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ELEMENT_WISE_CLIP_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .ScaleBias = IntPtr.Zero,
                                 .Min = min,
                                 .Max = max
                             })
                         End Function)
        End Function

        ''' <summary>DML_ELEMENT_WISE_THRESHOLD：小于阈值置零。</summary>
        Public Function Threshold(input As DmlTensor, output As DmlTensor, value As Single,
                                  Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_THRESHOLD, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ELEMENT_WISE_THRESHOLD_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .ScaleBias = IntPtr.Zero,
                                 .Min = value
                             })
                         End Function)
        End Function

        ''' <summary>DML_ELEMENT_WISE_IF：按条件在 a/b 之间选择。</summary>
        Public Function [If](condition As DmlTensor, a As DmlTensor, b As DmlTensor, output As DmlTensor,
                             Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_IF, {condition, a, b}, {output}, name,
                         Function(builder)
                             Return builder.StructPtr(New DML_ELEMENT_WISE_IF_OPERATOR_DESC With {
                                 .ConditionTensor = Desc(condition, builder),
                                 .ATensor = Desc(a, builder),
                                 .BTensor = Desc(b, builder),
                                 .OutputTensor = Desc(output, builder)
                             })
                         End Function)
        End Function

        ''' <summary>DML_ELEMENT_WISE_QUANTIZE_LINEAR：y = round(x / scale) + zeroPoint。</summary>
        Public Function QuantizeLinear(input As DmlTensor, scale As DmlTensor, zeroPoint As DmlTensor,
                                       output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_QUANTIZE_LINEAR, {input, scale, zeroPoint}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ELEMENT_WISE_QUANTIZE_LINEAR_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .ScaleTensor = Desc(scale, b),
                                 .ZeroPointTensor = Desc(zeroPoint, b),
                                 .OutputTensor = Desc(output, b)
                             })
                         End Function)
        End Function

        ''' <summary>DML_ELEMENT_WISE_DEQUANTIZE_LINEAR：y = (x - zeroPoint) * scale。</summary>
        Public Function DequantizeLinear(input As DmlTensor, scale As DmlTensor, zeroPoint As DmlTensor,
                                         output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Build(DML_OPERATOR_TYPE.ELEMENT_WISE_DEQUANTIZE_LINEAR, {input, scale, zeroPoint}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ELEMENT_WISE_QUANTIZE_LINEAR_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .ScaleTensor = Desc(scale, b),
                                 .ZeroPointTensor = Desc(zeroPoint, b),
                                 .OutputTensor = Desc(output, b)
                             })
                         End Function)
        End Function

    End Module

End Namespace

' ------------------------------------------------------------------------
' 激活算子
'
' 统一入口 Activation(kind, ...)：
'   桥梁层只需要把 ONNX 算子名映射成 DML_OPERATOR_TYPE 与几个标量参数。
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>激活算子工厂。</summary>
    Public Module ActivationOperators

        ''' <summary>
        ''' 创建一个激活算子。
        ''' </summary>
        ''' <param name="kind">激活算子类型。</param>
        ''' <param name="input">输入。</param>
        ''' <param name="output">输出。</param>
        ''' <param name="alpha">alpha（LeakyRelu/Elu/Celu/ThresholdedRelu/HardSigmoid/...）。</param>
        ''' <param name="beta">beta（HardSigmoid/HardSwish/...）。</param>
        ''' <param name="gamma">gamma（Selu）。</param>
        ''' <param name="axis">归一化轴（Softmax/LogSoftmax/Hardmax，-1 表示最后一维）。</param>
        ''' <param name="slope">PReLU 的斜率张量。</param>
        ''' <param name="name">调试名称。</param>
        Public Function Activation(kind As DML_OPERATOR_TYPE,
                                   input As DmlTensor,
                                   output As DmlTensor,
                                   Optional alpha As Single = 0.0F,
                                   Optional beta As Single = 0.0F,
                                   Optional gamma As Single = 1.0F,
                                   Optional axis As Integer = -1,
                                   Optional slope As DmlTensor = Nothing,
                                   Optional name As String = Nothing) As DmlOperatorDefinition

            If input Is Nothing OrElse output Is Nothing Then
                Throw New ArgumentNullException(NameOf(input))
            End If

            Select Case kind
                Case DML_OPERATOR_TYPE.ACTIVATION_RELU,
                     DML_OPERATOR_TYPE.ACTIVATION_SIGMOID,
                     DML_OPERATOR_TYPE.ACTIVATION_TANH,
                     DML_OPERATOR_TYPE.ACTIVATION_SOFTSIGN,
                     DML_OPERATOR_TYPE.ACTIVATION_GELU

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_SIMPLE_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b)
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_LEAKY_RELU,
                     DML_OPERATOR_TYPE.ACTIVATION_ELU,
                     DML_OPERATOR_TYPE.ACTIVATION_CELU,
                     DML_OPERATOR_TYPE.ACTIVATION_THRESHOLDED_RELU

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_ALPHA_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .Alpha = alpha
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_SCALED_ELU

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_SCALED_ELU_OPERATOR_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .Alpha = alpha,
                                         .Gamma = gamma
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_HARD_SIGMOID,
                     DML_OPERATOR_TYPE.ACTIVATION_HARD_SWISH,
                     DML_OPERATOR_TYPE.ACTIVATION_LINEAR,
                     DML_OPERATOR_TYPE.ACTIVATION_SCALED_TANH

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_ALPHA_BETA_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .Alpha = alpha,
                                         .Beta = beta
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_SOFTPLUS

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_SOFTPLUS_OPERATOR_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .Steepness = If(alpha = 0.0F, 1.0F, alpha)
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_SHRINK

                    Return Build(kind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_SHRINK_OPERATOR_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .Bias = alpha,
                                         .Threshold = beta
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_PARAMETERIZED_RELU

                    If slope Is Nothing Then
                        Throw New ArgumentNullException(NameOf(slope), "PReLU 需要斜率张量")
                    End If

                    Return Build(kind, {input, slope}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_PARAMETERIZED_RELU_OPERATOR_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .SlopeTensor = Desc(slope, b),
                                         .OutputTensor = Desc(output, b)
                                     })
                                 End Function)

                Case DML_OPERATOR_TYPE.ACTIVATION_SOFTMAX, DML_OPERATOR_TYPE.ACTIVATION_LOG_SOFTMAX,
                     DML_OPERATOR_TYPE.ACTIVATION_HARDMAX

                    ' ONNX 的 Softmax 轴是任意轴：只有"最后一维"能走 1.0 的旧算子，
                    ' 其它情况使用带 Axes 的 1.5 版本。
                    Dim lastAxis As Integer = input.Rank - 1

                    If axis < 0 Then
                        axis = lastAxis
                    End If

                    If axis = lastAxis Then
                        If kind = DML_OPERATOR_TYPE.ACTIVATION_SOFTMAX Then
                            Return Build(kind, {input}, {output}, name,
                                         Function(b)
                                             Return b.StructPtr(New DML_ACTIVATION_SIMPLE_DESC With {
                                                 .InputTensor = Desc(input, b),
                                                 .OutputTensor = Desc(output, b)
                                             })
                                         End Function)
                        ElseIf kind = DML_OPERATOR_TYPE.ACTIVATION_LOG_SOFTMAX Then
                            Return Build(kind, {input}, {output}, name,
                                         Function(b)
                                             Return b.StructPtr(New DML_ACTIVATION_SIMPLE_DESC With {
                                                 .InputTensor = Desc(input, b),
                                                 .OutputTensor = Desc(output, b)
                                             })
                                         End Function)
                        Else
                            Return Build(kind, {input}, {output}, name,
                                         Function(b)
                                             Return b.StructPtr(New DML_ACTIVATION_SIMPLE_DESC With {
                                                 .InputTensor = Desc(input, b),
                                                 .OutputTensor = Desc(output, b)
                                             })
                                         End Function)
                        End If
                    End If

                    Dim axesKind As DML_OPERATOR_TYPE

                    Select Case kind
                        Case DML_OPERATOR_TYPE.ACTIVATION_LOG_SOFTMAX
                            axesKind = DML_OPERATOR_TYPE.ACTIVATION_LOG_SOFTMAX1
                        Case DML_OPERATOR_TYPE.ACTIVATION_HARDMAX
                            axesKind = DML_OPERATOR_TYPE.ACTIVATION_HARDMAX1
                        Case Else
                            axesKind = DML_OPERATOR_TYPE.ACTIVATION_SOFTMAX1
                    End Select

                    Return Build(axesKind, {input}, {output}, name,
                                 Function(b)
                                     Return b.StructPtr(New DML_ACTIVATION_AXES_DESC With {
                                         .InputTensor = Desc(input, b),
                                         .OutputTensor = Desc(output, b),
                                         .AxisCount = 1UI,
                                         .Axes = b.UIntArray(New UInteger() {CUInt(axis)})
                                     })
                                 End Function)

                Case Else
                    Throw New NotSupportedException($"不支持的激活算子：{kind}")
            End Select
        End Function

        ''' <summary>ReLU。</summary>
        Public Function Relu(input As DmlTensor, output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Activation(DML_OPERATOR_TYPE.ACTIVATION_RELU, input, output, name:=name)
        End Function

        ''' <summary>Sigmoid。</summary>
        Public Function Sigmoid(input As DmlTensor, output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Activation(DML_OPERATOR_TYPE.ACTIVATION_SIGMOID, input, output, name:=name)
        End Function

        ''' <summary>Tanh。</summary>
        Public Function Tanh(input As DmlTensor, output As DmlTensor, Optional name As String = Nothing) As DmlOperatorDefinition
            Return Activation(DML_OPERATOR_TYPE.ACTIVATION_TANH, input, output, name:=name)
        End Function

        ''' <summary>Softmax（按 ONNX 的 axis 语义）。</summary>
        Public Function Softmax(input As DmlTensor, output As DmlTensor, Optional axis As Integer = -1,
                                Optional name As String = Nothing) As DmlOperatorDefinition
            Return Activation(DML_OPERATOR_TYPE.ACTIVATION_SOFTMAX, input, output, axis:=axis, name:=name)
        End Function

        ''' <summary>LeakyReLU。</summary>
        Public Function LeakyRelu(input As DmlTensor, output As DmlTensor, Optional alpha As Single = 0.01F,
                                  Optional name As String = Nothing) As DmlOperatorDefinition
            Return Activation(DML_OPERATOR_TYPE.ACTIVATION_LEAKY_RELU, input, output, alpha:=alpha, name:=name)
        End Function

    End Module

End Namespace

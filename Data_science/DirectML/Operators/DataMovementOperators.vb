' ------------------------------------------------------------------------
' 数据搬运类算子：Gather / Scatter / TopK / OneHot / 累积 / 反转
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>数据搬运算子工厂。</summary>
    Public Module DataMovementOperators

        ''' <summary>Gather（按轴取下标）。</summary>
        Public Function Gather(input As DmlTensor, indices As DmlTensor, output As DmlTensor,
                               Optional axis As UInteger = 0UI,
                               Optional indexDimensions As UInteger = 1UI,
                               Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.GATHER, {input, indices}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_GATHER_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .IndicesTensor = Desc(indices, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis,
                                 .IndexDimensions = indexDimensions
                             })
                         End Function)
        End Function

        ''' <summary>GatherElements（逐元素取下标，输出形状与 indices 一致）。</summary>
        Public Function GatherElements(input As DmlTensor, indices As DmlTensor, output As DmlTensor,
                                       Optional axis As UInteger = 0UI,
                                       Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.GATHER_ELEMENTS, {input, indices}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_GATHER_ELEMENTS_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .IndicesTensor = Desc(indices, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis
                             })
                         End Function)
        End Function

        ''' <summary>GatherND（多维下标）。</summary>
        Public Function GatherNd(input As DmlTensor, indices As DmlTensor, output As DmlTensor,
                                 Optional inputDimensionCount As UInteger = 1UI,
                                 Optional indicesDimensionCount As UInteger = 1UI,
                                 Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.GATHER_ND, {input, indices}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_GATHER_ND_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .IndicesTensor = Desc(indices, b),
                                 .OutputTensor = Desc(output, b),
                                 .InputDimensionCount = inputDimensionCount,
                                 .IndicesDimensionCount = indicesDimensionCount
                             })
                         End Function)
        End Function

        ''' <summary>ScatterND。</summary>
        Public Function ScatterNd(input As DmlTensor, indices As DmlTensor, updates As DmlTensor,
                                  output As DmlTensor,
                                  Optional inputDimensionCount As UInteger = 1UI,
                                  Optional indicesDimensionCount As UInteger = 1UI,
                                  Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.SCATTER_ND, {input, indices, updates}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_SCATTER_ND_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .IndicesTensor = Desc(indices, b),
                                 .UpdatesTensor = Desc(updates, b),
                                 .OutputTensor = Desc(output, b),
                                 .InputDimensionCount = inputDimensionCount,
                                 .IndicesDimensionCount = indicesDimensionCount
                             })
                         End Function)
        End Function

        ''' <summary>TopK。</summary>
        Public Function TopK(input As DmlTensor, values As DmlTensor, indices As DmlTensor,
                             k As UInteger, axis As UInteger,
                             Optional largest As Boolean = True,
                             Optional name As String = Nothing) As DmlOperatorDefinition

            Dim direction As DML_AXIS_DIRECTION = DML_AXIS_DIRECTION.DECREASING

            If Not largest Then
                direction = DML_AXIS_DIRECTION.INCREASING
            End If

            Return Build(DML_OPERATOR_TYPE.TOP_K1, {input}, {values, indices}, name,
                         Function(b)
                             Return b.StructPtr(New DML_TOP_K1_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputValueTensor = Desc(values, b),
                                 .OutputIndexTensor = Desc(indices, b),
                                 .Axis = axis,
                                 .K = k,
                                 .AxisDirection = direction
                             })
                         End Function)
        End Function

        ''' <summary>OneHot。</summary>
        Public Function OneHot(indices As DmlTensor, values As DmlTensor, output As DmlTensor,
                               Optional axis As UInteger = UInteger.MaxValue,
                               Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.ONE_HOT, {indices, values}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ONE_HOT_OPERATOR_DESC With {
                                 .IndicesTensor = Desc(indices, b),
                                 .ValuesTensor = Desc(values, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis
                             })
                         End Function)
        End Function

        ''' <summary>累积求和（ONNX CumSum）。</summary>
        Public Function CumulativeSummation(input As DmlTensor, output As DmlTensor, axis As UInteger,
                                            Optional reverse As Boolean = False,
                                            Optional exclusive As Boolean = False,
                                            Optional name As String = Nothing) As DmlOperatorDefinition

            Dim direction As DML_AXIS_DIRECTION = DML_AXIS_DIRECTION.INCREASING

            If reverse Then
                direction = DML_AXIS_DIRECTION.DECREASING
            End If

            Return Build(DML_OPERATOR_TYPE.CUMULATIVE_SUMMATION, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_CUMULATIVE_SUMMATION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis,
                                 .AxisDirection = direction,
                                 .HasExclusiveSum = BoolToInt(exclusive)
                             })
                         End Function)
        End Function

        ''' <summary>反转子序列（ONNX ReverseSequence）。</summary>
        Public Function ReverseSubsequences(input As DmlTensor, sequenceLengths As DmlTensor,
                                            output As DmlTensor, axis As UInteger,
                                            Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.REVERSE_SUBSEQUENCES, {input, sequenceLengths}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_REVERSE_SUBSEQUENCES_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .SequenceLengthsTensor = Desc(sequenceLengths, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis
                             })
                         End Function)
        End Function

        ''' <summary>对角矩阵（ONNX EyeLike / 对角矩阵构造）。</summary>
        Public Function DiagonalMatrix(input As DmlTensor, output As DmlTensor,
                                       Optional offset As Integer = 0,
                                       Optional value As Single = 1.0F,
                                       Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.DIAGONAL_MATRIX1, If(input Is Nothing, New DmlTensor() {}, {input}), {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_DIAGONAL_MATRIX1_OPERATOR_DESC With {
                                 .InputTensor = If(input Is Nothing, IntPtr.Zero, Desc(input, b)),
                                 .OutputTensor = Desc(output, b),
                                 .Offset = offset,
                                 .Value = value
                             })
                         End Function)
        End Function

    End Module

End Namespace

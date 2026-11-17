' ------------------------------------------------------------------------
' 归约、归一化与 LRN
' ------------------------------------------------------------------------

Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime

Namespace Operators

    ''' <summary>归约与归一化算子工厂。</summary>
    Public Module ReduceOperators

        ''' <summary>
        ''' 通用归约。
        ''' </summary>
        ''' <param name="reduceFunction">归约函数。</param>
        ''' <param name="input">输入。</param>
        ''' <param name="output">输出。</param>
        ''' <param name="axes">归约轴（空表示全部轴）。</param>
        ''' <param name="name">调试名称。</param>
        Public Function Reduce(reduceFunction As DML_REDUCE_FUNCTION,
                               input As DmlTensor,
                               output As DmlTensor,
                               axes As UInteger(),
                               Optional name As String = Nothing) As DmlOperatorDefinition

            If axes Is Nothing OrElse axes.Length = 0 Then
                axes = GetAllAxes(input.Rank)
            End If

            Return Build(DML_OPERATOR_TYPE.REDUCE, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_REDUCE_OPERATOR_DESC With {
                                 .[Function] = reduceFunction,
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .AxisCount = CUInt(axes.Length),
                                 .Axes = b.UIntArray(axes)
                             })
                         End Function)
        End Function

        ''' <summary>全部轴的索引。</summary>
        Public Function GetAllAxes(rank As Integer) As UInteger()
            Dim axes As UInteger() = New UInteger(rank - 1) {}

            For i As Integer = 0 To rank - 1
                axes(i) = CUInt(i)
            Next

            Return axes
        End Function

        ''' <summary>替换轴上的长度（keepdims=False 时用于推导输出形状）。</summary>
        Public Function ReduceShape(dims As UInteger(), axes As UInteger(), keepDims As Boolean) As UInteger()
            Dim removed As New HashSet(Of UInteger)

            If axes IsNot Nothing Then
                For Each a As UInteger In axes
                    Call removed.Add(a)
                Next
            End If

            Dim result As New List(Of UInteger)

            For i As Integer = 0 To dims.Length - 1
                If removed.Contains(CUInt(i)) Then
                    If keepDims Then
                        Call result.Add(1UI)
                    End If
                Else
                    Call result.Add(dims(i))
                End If
            Next

            If result.Count = 0 Then
                Return New UInteger() {1UI}
            End If

            Return result.ToArray()
        End Function

        ''' <summary>ArgMax / ArgMin。</summary>
        Public Function ArgMinMax(isArgMax As Boolean, input As DmlTensor, output As DmlTensor,
                                  axes As UInteger(), Optional keepDims As Boolean = True,
                                  Optional name As String = Nothing) As DmlOperatorDefinition

            If axes Is Nothing OrElse axes.Length = 0 Then
                axes = GetAllAxes(input.Rank)
            End If

            Return Build(If(isArgMax, DML_OPERATOR_TYPE.ARGMAX, DML_OPERATOR_TYPE.ARGMIN),
                         {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_ARGMIN_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .AxisCount = CUInt(axes.Length),
                                 .Axes = b.UIntArray(axes),
                                 .AxisDirection = DML_AXIS_DIRECTION.INCREASING
                             })
                         End Function)
        End Function

        ''' <summary>
        ''' 批量归一化（推理：均值/方差为常量输入）。
        ''' </summary>
        Public Function BatchNormalization(input As DmlTensor,
                                           mean As DmlTensor,
                                           variance As DmlTensor,
                                           scale As DmlTensor,
                                           bias As DmlTensor,
                                           output As DmlTensor,
                                           Optional epsilon As Single = 0.00001F,
                                           Optional spatial As Boolean = True,
                                           Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.BATCH_NORMALIZATION,
                         {input, mean, variance, scale, bias}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_BATCH_NORMALIZATION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .MeanTensor = Desc(mean, b),
                                 .VarianceTensor = Desc(variance, b),
                                 .ScaleTensor = Desc(scale, b),
                                 .BiasTensor = Desc(bias, b),
                                 .OutputTensor = Desc(output, b),
                                 .Spatial = BoolToInt(spatial),
                                 .Epsilon = epsilon,
                                 .FusedActivation = IntPtr.Zero
                             })
                         End Function)
        End Function

        ''' <summary>
        ''' 层归一化（对 <paramref name="axes"/> 指定的轴做均值/方差归一化）。
        ''' </summary>
        Public Function LayerNormalization(input As DmlTensor,
                                           scale As DmlTensor,
                                           bias As DmlTensor,
                                           output As DmlTensor,
                                           axes As UInteger(),
                                           Optional epsilon As Single = 0.00001F,
                                           Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.MEAN_VARIANCE_NORMALIZATION1,
                         If(scale Is Nothing, {input}, {input, scale, bias}), {output}, name,
                         Function(b)
                             Dim scalePtr As IntPtr = IntPtr.Zero
                             Dim biasPtr As IntPtr = IntPtr.Zero

                             If scale IsNot Nothing Then
                                 scalePtr = Desc(scale, b)
                             End If

                             If bias IsNot Nothing Then
                                 biasPtr = Desc(bias, b)
                             End If

                             Return b.StructPtr(New DML_MEAN_VARIANCE_NORMALIZATION1_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .ScaleTensor = scalePtr,
                                 .BiasTensor = biasPtr,
                                 .OutputTensor = Desc(output, b),
                                 .AxisCount = CUInt(If(axes Is Nothing, 0, axes.Length)),
                                 .Axes = b.UIntArray(axes),
                                 .NormalizeVariance = 1,
                                 .Epsilon = epsilon,
                                 .FusedActivation = IntPtr.Zero
                             })
                         End Function)
        End Function

        ''' <summary>实例归一化（跨空间维度归一化，每个通道独立）。</summary>
        Public Function InstanceNormalization(input As DmlTensor,
                                              scale As DmlTensor,
                                              bias As DmlTensor,
                                              output As DmlTensor,
                                              Optional epsilon As Single = 0.00001F,
                                              Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.MEAN_VARIANCE_NORMALIZATION,
                         If(scale Is Nothing, {input}, {input, scale, bias}), {output}, name,
                         Function(b)
                             Dim scalePtr As IntPtr = IntPtr.Zero
                             Dim biasPtr As IntPtr = IntPtr.Zero

                             If scale IsNot Nothing Then
                                 scalePtr = Desc(scale, b)
                             End If

                             If bias IsNot Nothing Then
                                 biasPtr = Desc(bias, b)
                             End If

                             Return b.StructPtr(New DML_MEAN_VARIANCE_NORMALIZATION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .ScaleTensor = scalePtr,
                                 .BiasTensor = biasPtr,
                                 .OutputTensor = Desc(output, b),
                                 .CrossChannel = 0,
                                 .NormalizeVariance = 1,
                                 .Epsilon = epsilon,
                                 .FusedActivation = IntPtr.Zero
                             })
                         End Function)
        End Function

        ''' <summary>局部响应归一化（ONNX LRN）。</summary>
        Public Function LocalResponseNormalization(input As DmlTensor,
                                                   output As DmlTensor,
                                                   Optional size As UInteger = 5UI,
                                                   Optional alpha As Single = 0.0001F,
                                                   Optional beta As Single = 0.75F,
                                                   Optional bias As Single = 1.0F,
                                                   Optional crossChannel As Boolean = True,
                                                   Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.LOCAL_RESPONSE_NORMALIZATION, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_LOCAL_RESPONSE_NORMALIZATION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .CrossChannel = BoolToInt(crossChannel),
                                 .LocalSize = size,
                                 .Alpha = alpha,
                                 .Beta = beta,
                                 .Bias = bias
                             })
                         End Function)
        End Function

        ''' <summary>Lp 归一化。</summary>
        Public Function LpNormalization(input As DmlTensor, output As DmlTensor,
                                        Optional axis As UInteger = 0UI,
                                        Optional p As UInteger = 2UI,
                                        Optional epsilon As Single = 0.00001F,
                                        Optional name As String = Nothing) As DmlOperatorDefinition

            Return Build(DML_OPERATOR_TYPE.LP_NORMALIZATION, {input}, {output}, name,
                         Function(b)
                             Return b.StructPtr(New DML_LP_NORMALIZATION_OPERATOR_DESC With {
                                 .InputTensor = Desc(input, b),
                                 .OutputTensor = Desc(output, b),
                                 .Axis = axis,
                                 .Epsilon = epsilon,
                                 .P = p
                             })
                         End Function)
        End Function

    End Module

End Namespace

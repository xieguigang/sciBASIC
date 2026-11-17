Imports Microsoft.VisualBasic.Computing.DirectML.Interop
Imports Microsoft.VisualBasic.Computing.DirectML.Operators
Imports Microsoft.VisualBasic.Computing.DirectML.Runtime
Imports Microsoft.VisualBasic.MachineLearning.DataStorage.ONNX.Proto

Namespace ONNX.DirectML

    ''' <summary>
    ''' 当 DirectML 后端无法执行某个算子时抛出。
    ''' </summary>
    Public Class OnnxDmlUnsupportedOperatorException
        Inherits OnnxUnsupportedOperatorException

        ''' <summary>出错的节点名称。</summary>
        Public ReadOnly Property Node As String

        ''' <summary>当前设备的 DirectML 特性级别。</summary>
        Public ReadOnly Property FeatureLevel As String

        Public Sub New(node As NodeProto, featureLevel As String, reason As String)
            MyBase.New(node, reason)
            Me.Node = If(node Is Nothing, "", node.Name)
            Me.FeatureLevel = featureLevel
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Message}（DirectML 特性级别 {FeatureLevel}）"
        End Function

    End Class

    ''' <summary>
    ''' 当前设备对 ONNX 算子的支持矩阵。
    ''' </summary>
    Public NotInheritable Class DmlOnnxCapabilities

        Friend Sub New(device As DmlDevice)
            Me.FeatureLevel = If(device Is Nothing, "<none>", device.FeatureLevelText)

            If device IsNot Nothing Then
                Me.SupportsFloat16 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.FLOAT16)
                Me.SupportsInt8 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.INT8)
                Me.SupportsInt32 = device.IsDataTypeSupported(DML_TENSOR_DATA_TYPE.INT32)
            End If
        End Sub

        ''' <summary>设备支持的 DirectML 特性级别文本。</summary>
        Public ReadOnly Property FeatureLevel As String

        ''' <summary>是否支持 FP16 张量。</summary>
        Public ReadOnly Property SupportsFloat16 As Boolean

        ''' <summary>是否支持 INT8 张量。</summary>
        Public ReadOnly Property SupportsInt8 As Boolean

        ''' <summary>是否支持 INT32 张量。</summary>
        Public ReadOnly Property SupportsInt32 As Boolean

        ''' <summary>判断某个 ONNX 算子是否有映射。</summary>
        Public Function IsSupported(opType As String) As Boolean
            Return OperatorCatalog.IsSupported(opType)
        End Function

        ''' <summary>列出模型中使用到、但当前框架没有映射的算子。</summary>
        Public Shared Function FindUnsupported(model As OnnxModel) As String()
            Dim result As New List(Of String)

            If model Is Nothing OrElse model.Graph Is Nothing Then
                Return result.ToArray()
            End If

            For Each opType As String In model.GetOpTypes()
                If Not OperatorCatalog.IsSupported(opType) Then
                    Call result.Add(opType)
                End If
            Next

            Return result.ToArray()
        End Function

        ''' <summary>生成能力矩阵文本。</summary>
        Public Function Report() As String
            Return OperatorCatalog.Report()
        End Function

        Public Overrides Function ToString() As String
            Return $"DirectML level {FeatureLevel}: fp16={SupportsFloat16}, int8={SupportsInt8}, int32={SupportsInt32}"
        End Function

    End Class

End Namespace

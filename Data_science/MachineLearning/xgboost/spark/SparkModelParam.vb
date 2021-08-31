Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace spark

    <Serializable>
    Public Class SparkModelParam

        Public Const MODEL_TYPE_CLS As String = "_cls_"
        Public Const MODEL_TYPE_REG As String = "_reg_"

        Public Overridable ReadOnly Property modelType As String
        Public Overridable ReadOnly Property featureCol As String
        Public Overridable ReadOnly Property labelCol As String
        Public Overridable ReadOnly Property predictionCol As String

        ''' <summary>
        ''' classification model only
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property rawPredictionCol As String
        Public Overridable ReadOnly Property thresholds As Double()

        Public Sub New(modelType As String, featureCol As String, reader As ModelReader)
            _modelType = modelType
            _featureCol = featureCol
            _labelCol = reader.readUTF()
            _predictionCol = reader.readUTF()

            If MODEL_TYPE_CLS.Equals(modelType) Then
                _rawPredictionCol = reader.readUTF()
                Dim thresholdLength As Integer = reader.readIntBE()
                _thresholds = If(thresholdLength > 0, reader.readDoubleArrayBE(thresholdLength), Nothing)
            ElseIf MODEL_TYPE_REG.Equals(modelType) Then
                _rawPredictionCol = Nothing
                _thresholds = Nothing
            Else
                Throw New NotSupportedException("Unknown modelType: " & modelType)
            End If
        End Sub
    End Class
End Namespace

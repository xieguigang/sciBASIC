Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util

Namespace spark

    <Serializable>
    Public Class SparkModelParam
        Public Const MODEL_TYPE_CLS As String = "_cls_"
        Public Const MODEL_TYPE_REG As String = "_reg_"

        Friend ReadOnly modelType_Renamed As String
        Friend ReadOnly featureCol_Renamed As String

        Friend ReadOnly labelCol_Renamed As String
        Friend ReadOnly predictionCol_Renamed As String

        ' classification model only
        Friend ReadOnly rawPredictionCol_Renamed As String
        Friend ReadOnly thresholds_Renamed As Double()

        Public Sub New(modelType As String, featureCol As String, reader As ModelReader)
            modelType_Renamed = modelType
            featureCol_Renamed = featureCol
            labelCol_Renamed = reader.readUTF()
            predictionCol_Renamed = reader.readUTF()

            If MODEL_TYPE_CLS.Equals(modelType) Then
                rawPredictionCol_Renamed = reader.readUTF()
                Dim thresholdLength As Integer = reader.readIntBE()
                thresholds_Renamed = If(thresholdLength > 0, reader.readDoubleArrayBE(thresholdLength), Nothing)
            ElseIf MODEL_TYPE_REG.Equals(modelType) Then
                rawPredictionCol_Renamed = Nothing
                thresholds_Renamed = Nothing
            Else
                Throw New NotSupportedException("Unknown modelType: " & modelType)
            End If
        End Sub

        Public Overridable ReadOnly Property modelType As String
            Get
                Return modelType_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property featureCol As String
            Get
                Return featureCol_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property labelCol As String
            Get
                Return labelCol_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property predictionCol As String
            Get
                Return predictionCol_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property rawPredictionCol As String
            Get
                Return rawPredictionCol_Renamed
            End Get
        End Property

        Public Overridable ReadOnly Property thresholds As Double()
            Get
                Return thresholds_Renamed
            End Get
        End Property
    End Class
End Namespace

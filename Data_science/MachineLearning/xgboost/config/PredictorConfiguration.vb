Imports Microsoft.VisualBasic.MachineLearning.XGBoost.learner

Namespace config
    Public Class PredictorConfiguration
        Public Class BuilderType
            Friend predictorConfiguration As PredictorConfiguration

            Friend Sub New()
                predictorConfiguration = New PredictorConfiguration()
            End Sub

            Public Overridable Function objFunction(objFunc As ObjFunction) As BuilderType
                predictorConfiguration._objFunction = objFunc
                Return Me
            End Function

            Public Overridable Function build() As PredictorConfiguration
                Dim result = predictorConfiguration
                predictorConfiguration = Nothing
                Return result
            End Function
        End Class

        Public Shared ReadOnly [DEFAULT] As New PredictorConfiguration()

        Public Overridable ReadOnly Property objFunction As ObjFunction

        Public Shared Function builder() As BuilderType
            Return New BuilderType()
        End Function
    End Class
End Namespace

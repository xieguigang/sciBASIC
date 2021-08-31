Imports ObjFunction = biz.k11i.xgboost.learner.ObjFunction

Namespace biz.k11i.xgboost.config
    Public Class PredictorConfiguration
        Public Class BuilderType
            Friend predictorConfiguration As PredictorConfiguration

            Friend Sub New()
                predictorConfiguration = New PredictorConfiguration()
            End Sub

            Public Overridable Function objFunction(ByVal objFunction As ObjFunction) As BuilderType
                predictorConfiguration.objFunction_Renamed = objFunction
                Return Me
            End Function

            Public Overridable Function build() As PredictorConfiguration
                Dim result = predictorConfiguration
                predictorConfiguration = Nothing
                Return result
            End Function
        End Class

        Public Shared ReadOnly [DEFAULT] As PredictorConfiguration = New PredictorConfiguration()

        'JAVA TO C# CONVERTER CRACKED BY X-CRACKER NOTE: Fields cannot have the same name as methods:
        Private objFunction_Renamed As ObjFunction

        Public Overridable ReadOnly Property objFunction As ObjFunction
            Get
                Return objFunction_Renamed
            End Get
        End Property

        Public Shared Function builder() As BuilderType
            Return New BuilderType()
        End Function
    End Class
End Namespace

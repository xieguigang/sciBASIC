Imports Microsoft.VisualBasic.Serialization.JSON

Namespace LP

    Public Class ObjectiveFunction

        Public Property Type As OptimizationType
        Public Property xyz As Double()

        Public Overrides Function ToString() As String
            Return $"{Type.ToString}({xyz.GetJson})"
        End Function
    End Class
End Namespace
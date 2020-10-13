Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    Public Structure SVMPrediction

        Public Property [class] As Integer
        Public Property score As Double
        Public Property unifyValue As Double

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Structure
End Namespace


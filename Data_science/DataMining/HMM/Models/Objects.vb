Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Namespace Models

    Public Class StatesObject

        Public Property state As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{state}: {prob.Select(Function(d) stdNum.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class

    Public Class Observable

        Public Property obs As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{obs}: {prob.Select(Function(d) stdNum.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class
End Namespace
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace EmGaussian

    ''' <summary>
    ''' A possible signle peak
    ''' </summary>
    Public Class Variable

        Public Property weight As Double
        Public Property mean As Double
        Public Property variance As Double

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace
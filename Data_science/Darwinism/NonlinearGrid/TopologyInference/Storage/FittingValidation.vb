Public Class FittingValidation

    Public Property sampleID As String
    Public Property actual As Double
    Public Property predicts As Double

    Public ReadOnly Property errors As Double
        Get
            Return Math.Abs(actual - predicts)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"[{errors}] {sampleID}"
    End Function
End Class

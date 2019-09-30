Namespace Math.Statistics

    Public Class Min : Inherits SampleObservation

        Public ReadOnly Property MinValue As Double

        Sub New(value As Double)
            Call MyBase.New({value})
            ' intialize value is the min value
            MinValue = value
        End Sub

        Protected Overrides Sub addObservation(observation As Double)
            If observation < MinValue Then
                _MinValue = observation
            End If
        End Sub

        Protected Overrides Function getEigenvalue() As Double
            Return MinValue
        End Function

        Public Overrides Function ToString() As String
            Return MinValue
        End Function
    End Class
End Namespace
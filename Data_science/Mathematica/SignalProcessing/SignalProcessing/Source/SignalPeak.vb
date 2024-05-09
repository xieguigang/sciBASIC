Imports Microsoft.VisualBasic.Math.Distributions
Imports signalData = Microsoft.VisualBasic.Math.SignalProcessing.Signal

Namespace Source

    Public Class SignalPeak

        Public Property offset As Double
        Public Property max_intensity As Double
        Public Property width As Double

        Public Function GetSignalData(dt As Double, Optional scale As Double = 1.5) As signalData
            Dim tick As New List(Of (time As Double, value As Double))
            Dim right As Double = width * scale
            Dim xi As Double = -right
            Dim center As Double = width / 2

            Do While xi < right
                tick.Add((xi + offset, Gaussian.Gaussian(xi, max_intensity, center, width)))
                xi += dt
            Loop

            Return New signalData(
                (From ti In tick Select ti.time),
                (From ti In tick Select ti.value)
            )
        End Function

    End Class
End Namespace
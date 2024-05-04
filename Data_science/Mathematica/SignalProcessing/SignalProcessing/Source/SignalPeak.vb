Imports signalData = Microsoft.VisualBasic.Math.SignalProcessing.Signal

Namespace Source

    Public Class SignalPeak

        Public Property offset As Double
        Public Property max_intensity As Double
        Public Property width As Double

        Public Function GetSignalData(left As Double, right As Double, dt As Double) As signalData
            Dim tick As New List(Of (time As Double, value As Double))

            Return New signalData(
                (From xi In tick Select xi.time),
                (From xi In tick Select xi.value)
            )
        End Function

    End Class
End Namespace
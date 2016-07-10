Imports System
Imports System.Runtime.CompilerServices

Namespace Quantile

    Public Module Extensions

        Const epsilon As Double = 0.001

        <Extension>
        Public Function GKQuantile(source As IEnumerable(Of Long),
                                   Optional epsilon As Double = Extensions.epsilon,
                                   Optional compact_size As Integer = 1000) As QuantileEstimationGK

            Dim estimator As New QuantileEstimationGK(epsilon, compact_size)

            For Each x As Long In source
                Call estimator.insert(x)
            Next

            Return estimator
        End Function

        Const window_size As Integer = 10000

        Private Sub Test()
            Dim shuffle As Long() = New Long(window_size - 1) {}

            For i As Integer = 0 To shuffle.Length - 1
                shuffle(i) = i
            Next

            shuffle = shuffle.Shuffles

            Dim estimator As QuantileEstimationGK = shuffle.GKQuantile
            Dim quantiles As Double() = {0.5, 0.9, 0.95, 0.99, 1.0}

            For Each q As Double In quantiles
                Dim estimate As Long = estimator.query(q)
                Dim actual As Long = shuffle.actually(q)
                Console.WriteLine(String.Format("Estimated {0:F2} quantile as {1:D} (actually {2:D})", q, estimate, actual))
            Next
        End Sub

        <Extension>
        Public Function actually(source As Long(), q As Double) As Long
            Dim actual As Long = CLng(Fix((q) * (source.Length - 1)))
            Return actual
        End Function
    End Module
End Namespace
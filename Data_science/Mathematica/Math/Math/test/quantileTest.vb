Imports Microsoft.VisualBasic.Math.Quantile

Module quantileTest

    Sub Main()
        Dim q = New Double() {5, 100, 200, 2000, 300, 20, 20, 20, 20, 3000, 9999999, 1, 1, 1, 1, 1, 99}.GKQuantile

        For Each l In {0, 0.25, 0.5, 0.75, 1}
            Call q.Query(l).__DEBUG_ECHO
        Next
    End Sub
End Module

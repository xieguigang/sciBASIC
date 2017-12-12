Public Module TestProvider

    ' Inspired by Lee Byron's test data generator.
    Public Function bumps(n%, m%)
        Dim a#() = New Double(n - 1) {}
        Dim seed As New Random

        For i As Integer = 0 To m - 1
            Call bump(a, n, seed)
        Next

        Return a
    End Function

    Public Sub bump(a#(), n%, seed As Random)
        Dim x = 1 / (0.1 + seed.NextDouble),
            y = 2 * seed.NextDouble - 0.5,
            Z = 10 / (0.1 + seed.NextDouble)

        For i As Integer = 0 To n - 1
            Dim w = (i / n - y) * Z
            a(i) += x * Math.Exp(-w * w)
        Next
    End Sub
End Module

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers

Module gaussTest

    Sub Main()
        Dim a As New NumericMatrix({
            {2, 1, -1},
            {-3, -1, 2},
            {-2, 1, 2}
        })
        Dim b As Vector = {8, -11, -3}
        Dim x As Vector = GaussianElimination.Solve(a, b)

        Console.WriteLine(x)

        Pause()
    End Sub
End Module

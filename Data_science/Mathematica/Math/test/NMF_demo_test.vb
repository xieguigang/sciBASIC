Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module NMF_demo_test

    Sub Main()
        Dim a As New NumericMatrix({{423, 4, 23}, {2342, 3, 4}, {2, 4, 23}, {2, 423, 4}, {24, 22, 2}, {24, 2, 4.0}})
        Dim result = NMF.Factorisation(a)

        Pause()
    End Sub
End Module

Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Matrix

Module Matrix
    Sub Test()
        Dim m As Microsoft.VisualBasic.Math.Matrix.GeneralMatrix = {
            {1, 8, 0.0},
            {2, 8, 0},
            {3, 8, 0},
            {4, 8, 1},
            {5, 9, 5}
        }

        Dim result = m.SVD()

        Call result.S.Print
        Call Console.WriteLine()
        Call result.U.Print
        Call Console.WriteLine()
        Call result.V.Print

        Pause()
    End Sub
End Module

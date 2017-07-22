Imports Microsoft.VisualBasic.Math

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

        Pause()
    End Sub
End Module

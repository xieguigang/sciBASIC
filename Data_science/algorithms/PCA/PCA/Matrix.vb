Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Module Matrix
    Sub Test()
        Dim m As GeneralMatrix = {
            {1, 8, 0.0},
            {2, 8, 0},
            {3, 8, 0},
            {4, 8, 1},
            {5, 9, 5}
        }

        '   Call test3(m)

        Dim result = m.SVD()

        Call result.S.Print
        Call Console.WriteLine()
        Call result.U.Print
        Call Console.WriteLine()
        Call result.V.Print

        Pause()
    End Sub

    Sub fffff()

        Dim C As GeneralMatrix = {{0.716, 0.615}, {0.615, 0.616}}
        Dim SVD = C.SVD
        Dim V = SVD.V
        Dim L = V(Which.IsTrue(SVD.SingularValues.Top(2)))
        Dim PCA = L * C

        Pause()
    End Sub

    Sub Ei()
        Dim m As GeneralMatrix = {{0.716, 0.615}, {0.615, 0.616}}
        Dim e = m.Eigen
        Dim d = e.D

        Dim svd = m.SVD

        Dim svd2 = m.Covariance.SVD
        Dim eeee = m.Covariance.Eigen

        Pause()
    End Sub

    'Sub test3(m As GeneralMatrix)
    '    Dim SVD As Double()()() = Microsoft.VisualBasic.DataMining.PCA.Matrix.singularValueDecomposition(m.Array)
    '    Dim U As GeneralMatrix = SVD(0)
    '    Dim S As GeneralMatrix = SVD(1)
    '    Dim V As GeneralMatrix = SVD(2)


    '    Call U.Print
    '    Call Console.WriteLine()
    '    Call V.Print
    '    Call Console.WriteLine()
    '    Call V.Print


    '    Pause()
    'End Sub
End Module

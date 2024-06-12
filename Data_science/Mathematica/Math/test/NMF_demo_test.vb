Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module NMF_demo_test

    Sub dot_test()
        ' A <- matrix(c(1, 2, 3, 4,5,6), nrow = 2, ncol = 3)
        ' B <- matrix(c(5, 6, 7, 8,9,0), nrow = 3, ncol = 2)
        '
        ' A;
        '      [,1] [,2] [,3]
        ' [1,]    1    3    5
        ' [2,]    2    4    6
        '
        ' B;
        '      [,1] [,2]
        ' [1,]    5    8
        ' [2,]    6    9
        ' [3,]    7    0
        '
        ' A %*% B;
        '      [,1] [,2]
        ' [1,]   58   35
        ' [2,]   76   52
        Dim A As New NumericMatrix({{1, 3, 5}, {2, 4, 6}})
        Dim B As New NumericMatrix({{5, 8}, {6, 9}, {7, 0}})
        Dim d = A.DotProduct(B)

        Pause()
    End Sub

    Sub Main()
        ' Call dot_test()

        Dim a As New NumericMatrix({{423, 4, 23}, {2342, 3, 4}, {2, 4, 23}, {2, 423, 4}, {24, 22, 2}, {24, 2, 4.0}})
        Dim result = NMF.Factorisation(a)

        Pause()
    End Sub
End Module

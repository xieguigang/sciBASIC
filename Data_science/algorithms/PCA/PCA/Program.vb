Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Module Program

    Sub Main()
        ' Call Matrix.Test()
        Dim input As GeneralMatrix = File.Load("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\flower.csv").AsMatrix
        Dim B As GeneralMatrix = input.Transpose.CenterNormalize
        Dim C = DataMining.PCA.Data.covarianceMatrix(B)
        Dim SVD = C.SVD

        '  Call B.Print
        Call C.Print
        Call Console.WriteLine()
        Call Console.WriteLine(SVD.SingularValues)
        Call Console.WriteLine()
        Call SVD.V.Print
        Call Console.WriteLine()
        Dim n = 2
        Dim columns = Which.IsTrue(SVD.SingularValues.Top(n))

        Dim M = SVD.V(columns)

        Call M.Print
        Call Console.WriteLine()
        Dim PCA = input * M

        Call PCA.Print(out:="./PCA_result.txt".OpenWriter)

        Call Test()
    End Sub

    ''' <summary>
    ''' http://www.cnblogs.com/bigshuai/archive/2012/06/18/2553808.html
    ''' </summary>
    Private Sub Test()
        Dim data()() = {{-1, -1, 0.0, 2, 0}, {-2, 0R, 0, 1, 1}}.RowIterator.ToArray
        Microsoft.VisualBasic.DataMining.PCA.Data.principalComponentAnalysis(data, 2).Print

        Call Console.WriteLine()
        ' Call New GeneralMatrix({New Double() {-3 / Math.Sqrt(2), -1 / Math.Sqrt(2), 0, 3 / Math.Sqrt(2), -1 / Math.Sqrt(2)}}).Print

        Dim scores As GeneralMatrix = DataMining.PCA.Data.PCANIPALS(data, 2)
        Call scores.Print

        Pause()
    End Sub
End Module

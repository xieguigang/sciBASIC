Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Matrix

Module Program

    Sub Main()
        Matrix.fffff()
        Matrix.Ei()
        Call TestPCA_scores()

        ' Call Matrix.Test()
        Dim input As GeneralMatrix = File.Load("D:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\flower.csv").AsMatrix
        Dim PCA = DataMining.PCA.PrincipalComponentAnalysis(input, nPC:=2)

        Call PCA.Print(out:="./PCA_result.txt".OpenWriter)

        Call Test()
    End Sub

    '''' <summary>
    '''' http://www.cnblogs.com/bigshuai/archive/2012/06/18/2553808.html
    '''' </summary>
    'Private Sub Test()
    '    Dim data()() = {{-1, -1, 0.0, 2, 0}, {-2, 0R, 0, 1, 1}}.RowIterator.ToArray
    '    Microsoft.VisualBasic.DataMining.PCA.Data.principalComponentAnalysis(data, 2).Print

    '    Call Console.WriteLine()
    '    ' Call New GeneralMatrix({New Double() {-3 / Math.Sqrt(2), -1 / Math.Sqrt(2), 0, 3 / Math.Sqrt(2), -1 / Math.Sqrt(2)}}).Print

    '    Dim scores As GeneralMatrix = DataMining.PCA.Data.PCANIPALS(data, 2)
    '    Call scores.Print

    '    Pause()
    'End Sub

    Sub TestPCA_scores()
        Dim input As GeneralMatrix = File.Load("G:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\scores.csv").AsMatrix(True, True)
        Dim PCA = DataMining.PCA.PrincipalComponentAnalysis(input, nPC:=2)

        Call PCA.Print

        Pause()
    End Sub
End Module

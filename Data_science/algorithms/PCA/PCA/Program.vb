Imports Microsoft.VisualBasic.Math.Matrix

Module Program

    Sub Main()
        ' Call Matrix.Test()

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

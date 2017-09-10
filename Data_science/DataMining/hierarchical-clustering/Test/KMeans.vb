Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans

Module KMeans

    Sub Main()

        Dim data = DataSet.LoadDataSet("C:\Users\xieguigang\Desktop\8.27\8.27\2. 17-92 vs ctrl\3. DEPs\data.csv").ToKMeansModels
        data = data.Kmeans(15)
        data.SaveTo("C:\Users\xieguigang\Desktop\8.27\8.27\2. 17-92 vs ctrl\3. DEPs\heatmap.kmeans.csv")
    End Sub
End Module

Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.PCA

Module Module1

    Sub Main()
        Call methodTest()

        Pause()
    End Sub

    Sub methodTest()
        Dim data = DataSet.LoadDataSet("C:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\PCA\iris.csv", uidMap:="class")

        Dim pca As New PCA(data)



        Pause()
    End Sub
End Module

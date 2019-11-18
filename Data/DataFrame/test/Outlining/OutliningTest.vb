Imports Microsoft.VisualBasic.Data.csv.Outlining

Module OutliningTest

    Sub Main()
        Dim test = "D:\GCModeller\src\runtime\sciBASIC#\Data\data\outlining.csv".LoadOutlining(Of compound).ToArray


        Pause()
    End Sub
End Module

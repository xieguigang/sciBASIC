Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Data.csv

Module KmedoidsTest

    Sub Main()

        Dim data = DataSet.LoadDataSet("E:\GCModeller\src\runtime\sciBASIC#\Data_science\algorithms\bezdekIris.csv", uidMap:="class").ToArray
        Dim names = data.PropertyNames
        Dim points = data.Select(Function(d)
                                     Return New ClusterEntity With {.Properties = d(names), .Extension = New ExtendedProps(New [Property](Of Object)("class", d.ID))}
                                 End Function).ToArray

        Dim result = points.DoKMedoids(3, 1000)
        Dim table = result.Select(Function(e)
                                      Dim properties = names.SeqIterator.ToDictionary(Function(i) i.value, Function(i) e.Properties(i).ToString)

                                      properties.Add("cluster", e.cluster)

                                      Return New EntityObject With {
                                        .ID = e.Extension.DynamicHashTable!class,
                                        .Properties = properties
                                      }
                                  End Function).ToArray

        Call table.SaveTo("X:/test.csv")

        Pause()

    End Sub
End Module

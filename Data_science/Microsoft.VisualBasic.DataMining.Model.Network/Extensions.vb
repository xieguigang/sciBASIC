Imports System.IO
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language

Public Module Extensions

    ''' <summary>
    ''' 假若有很多个节点的话，则进行聚类会得到很多的属性，但是想要加载的数据
    ''' 只有ID和cluster结果等非附加属性部分，则这个时候可以使用这个函数进行快速加载
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <returns></returns>
    Public Iterator Function ClusterResultFastLoad(path$) As IEnumerable(Of EntityLDM)
        Using reader As StreamReader = path.OpenReader
            Dim header As New RowObject(reader.ReadLine)
            Dim cluster% = header.IndexOf(NameOf(EntityLDM.Cluster))
            Dim name% = header.IndexOf(NameOf(EntityLDM.Name))
            Dim row As New Value(Of RowObject)

            Do While Not reader.EndOfStream
                Yield New EntityLDM With {
                    .Name = (row = New RowObject(reader.ReadLine))(name),
                    .Cluster = (+row)(cluster)
                }
            Loop
        End Using
    End Function
End Module

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module Extensions

        ''' <summary>
        ''' Grouping the numeric values by using the kmeans cluserting operations.
        ''' (对一组数字进行聚类操作，其实在这里就是将这组数值生成Entity数据对象，然后将数值本身作为自动生成的Entity对象的一个唯一属性)
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="nd"></param>
        ''' <returns></returns>
        <Extension> Public Function ValueGroups(array As IEnumerable(Of Double), nd As Integer) As List(Of EntityLDM)
            Dim entities As EntityLDM() = array.ToArray(
                Function(x, i) New EntityLDM With {
                    .Name = i & ":" & x,
                    .Properties = New Dictionary(Of String, Double) From {{"val", x}}
                })
            Return entities.Kmeans(nd)
        End Function

        ''' <summary>
        ''' Performance the clustering operation on the entity data model.
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension> Public Function Kmeans(source As IEnumerable(Of EntityLDM), n As Integer) As List(Of EntityLDM)
            Dim maps As String() = source.First.Properties.Keys.ToArray
            Dim clusters As ClusterCollection(Of Entity) = n.ClusterDataSet(source.ToArray(Function(x) x.ToModel))
            Dim result As New List(Of EntityLDM)

            n = 1

            For Each cluster As Cluster(Of Entity) In clusters
                Dim values As EntityLDM() = cluster.ToArray(Function(x) x.ToLDM(maps))

                For Each x In values
                    x.Cluster = n
                Next

                result += values
                n += 1
            Next

            Return result
        End Function
    End Module
End Namespace
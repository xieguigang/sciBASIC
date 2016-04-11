Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace KMeans

    Public Module Extensions

        <Extension> Public Function ValueGroups(array As IEnumerable(Of Double), nd As Integer) As List(Of EntityLDM)
            Dim entities As EntityLDM() = array.ToArray(
                Function(x, i) New EntityLDM With {
                    .Name = i & ":" & x,
                    .Properties = New Dictionary(Of String, Double) From {{"val", x}}
                })
            Return entities.Kmeans(nd)
        End Function

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
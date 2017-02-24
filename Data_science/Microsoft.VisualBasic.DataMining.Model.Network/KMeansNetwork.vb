Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Language

Namespace KMeans

    Public Module KMeansNetwork

        <Extension>
        Public Function ToNetwork(kmeans As IEnumerable(Of EntityLDM)) As Network
            Dim data As EntityLDM() = kmeans.ToArray
            Dim nodes As Dictionary(Of Node) = data _
                .Select(Function(n) New Node With {
                    .ID = n.Name,
                    .NodeType = n.Cluster
                }).ToDictionary
            Dim edges As New List(Of NetworkEdge)

            Return New Network With {
                .Edges = edges,
                .Nodes = nodes
            }
        End Function
    End Module
End Namespace
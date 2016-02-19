Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace TreeAPI

    <PackageNamespace("TREE.Cluster")>
    Public Module Operations

        Public Const PATH As String = "Path"
        Public Const LEAF As String = "Leaf"
        Public Const LEAF_X As String = "Leaf-X"

        Public Const ROOT As String = "ROOT"

        <ExportAPI("Cluster.Parts")>
        Public Function ClusterParts(net As IEnumerable(Of FileStream.NetworkEdge)) As Dictionary(Of String, Edge())
            Dim ROOTs = net.GetConnections(ROOT)

        End Function

        Private Function __addCluster() As Dictionary(Of String, Edge())


        End Function
    End Module
End Namespace
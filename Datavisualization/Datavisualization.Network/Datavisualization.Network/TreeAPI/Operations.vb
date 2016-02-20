Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

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
            Dim tree As New BinaryTree(Of NodeTypes)(ROOT, NodeTypes.ROOT)
            Dim netList = net.ToList

            For Each node In ROOTs
                Call netList.Remove(node)
                Call __buildTREE(tree, node.GetConnectedNode(ROOT), netList)
            Next
        End Function

        Private Sub __buildTREE(ByRef tree As BinaryTree(Of NodeTypes), node As String, ByRef netList As List(Of FileStream.NetworkEdge))
            Dim nexts = netList.GetNextConnects(node)
            Dim type = __getTypes(nexts.First.InteractionType)

            If type = NodeTypes.Leaf Then

            ElseIf type = NodeTypes.LeafX Then
                Dim Xnode As New LeafX(node) With {.LeafX = nexts}

            Else ' 这个是Path，则继续建树
                For Each nxode In nexts
                    Call netList.Remove(nxode)
                    Call tree.insert(nxode.FromNode, __getTypes(nxode.InteractionType))
                    Call __buildTREE(tree, nxode.ToNode, netList)
                Next
            End If
        End Sub

        Private ReadOnly __getTypes As Dictionary(Of String, NodeTypes) =
            New Dictionary(Of String, NodeTypes) From {
 _
            {PATH, NodeTypes.Path},
            {LEAF, NodeTypes.Leaf},
            {LEAF_X, NodeTypes.LeafX},
            {ROOT, NodeTypes.ROOT}
        }

        Public Function [GetType](name As String) As NodeTypes
            If __getTypes.ContainsKey(name) Then
                Return __getTypes(name)
            Else
                Return NodeTypes.ROOT
            End If
        End Function

        Private Function __addCluster() As Dictionary(Of String, Edge())


        End Function
    End Module
End Namespace
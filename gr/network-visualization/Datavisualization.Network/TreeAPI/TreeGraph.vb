Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public NotInheritable Class TreeGraph(Of K, V)

    Public Shared Function CreateGraph(tree As BinaryTree(Of K, V), uniqueId As Func(Of V, String), keyId As Func(Of K, String), Optional connectLeft As Boolean = False) As NetworkGraph
        Dim g As New NetworkGraph
        Dim root As Node = g.CreateNode(keyId(tree.Key), New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, keyId(tree.Key)}}})

        Call appendGraph(g, root, tree, uniqueId, keyId, connectLeft)
        Return g
    End Function

    Private Shared Sub appendGraph(g As NetworkGraph, center As Node, tree As BinaryTree(Of K, V), uniqueId As Func(Of V, String), keyId As Func(Of K, String), connectLeft As Boolean)
        Dim clusterId = keyId(tree.Key)
        Dim v As Node
        Dim guid As String

        For Each member In tree.Members
            guid = uniqueId(member)

            If g.GetElementByID(guid) Is Nothing Then
                v = g.CreateNode(uniqueId(member), New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})
            Else
                v = g.GetElementByID(guid)
            End If

            g.CreateEdge(center, v)
        Next

        If Not tree.Left Is Nothing Then
            clusterId = keyId(tree.Left.Key)
            v = g.CreateNode(clusterId, New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})

            If connectLeft Then
                g.CreateEdge(v, center)
            End If

            appendGraph(g, v, tree.Left, uniqueId, keyId, connectLeft)
        End If

        If Not tree.Right Is Nothing Then
            clusterId = keyId(tree.Right.Key)
            v = g.CreateNode(clusterId, New NodeData With {.Properties = New Dictionary(Of String, String) From {{NamesOf.REFLECTION_ID_MAPPING_NODETYPE, clusterId}}})
            g.CreateEdge(v, center)
            appendGraph(g, v, tree.Right, uniqueId, keyId, connectLeft)
        End If
    End Sub
End Class

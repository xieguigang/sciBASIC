Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public Module ClusterViz

    <Extension>
    Public Function MakeTreeGraph(tree As BTreeCluster, Optional metadata As Func(Of String, Dictionary(Of String, String)) = Nothing) As NetworkGraph
        Dim g As New NetworkGraph
        Call tree.PullTreeGraph(g, If(metadata, EmptyMetadata()))
        Return g
    End Function

    Private Function EmptyMetadata() As Func(Of String, Dictionary(Of String, String))
        Return Function(any) New Dictionary(Of String, String)
    End Function

    <Extension>
    Private Sub PullTreeGraph(tree As BTreeCluster, g As NetworkGraph, metadata As Func(Of String, Dictionary(Of String, String)))
        Dim root As Node = g.CreateNode(tree.uuid)
        Call root.data.Add(metadata(root.label))
        For Each id As String In tree.members
            Dim v As Node = g.CreateNode(id)
            Call v.data.Add(metadata(id))
            Call g.CreateEdge(root, v)
        Next

        Call tree.left.PullTreeGraph(g, metadata)
        Call tree.right.PullTreeGraph(g, metadata)

        Call g.CreateEdge(root, g.GetElementByID(tree.left.uuid))
        Call g.CreateEdge(root, g.GetElementByID(tree.right.uuid))
    End Sub
End Module

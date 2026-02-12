Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Public Module ClusterViz

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="metadata">
    ''' there are some special metadata key:
    ''' 
    ''' 1. text - for node data label
    ''' 2. value - for node data mass weight
    ''' </param>
    ''' <returns></returns>
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

        root.data.Add(metadata(root.label))
        root.data.Add(NamesOf.REFLECTION_ID_MAPPING_NODETYPE, root.label)
        root.data.label = root.data.Properties.Popout("text")
        root.data.mass = Val(root.data.Properties.Popout("value"))

        For Each id As String In tree.members
            If id <> root.label Then
                Dim v As Node = g.CreateNode(id)

                v.data.Add(metadata(id))
                v.data.Add(NamesOf.REFLECTION_ID_MAPPING_NODETYPE, root.label)
                v.data.label = v.data.Properties.Popout("text")
                v.data.mass = Val(v.data.Properties.Popout("value"))

                g.CreateEdge(root, v)
            End If
        Next

        If tree.left IsNot Nothing Then
            Call tree.left.PullTreeGraph(g, metadata)
            Call g.CreateEdge(root, g.GetElementByID(tree.left.uuid))
        End If
        If tree.right IsNot Nothing Then
            Call tree.right.PullTreeGraph(g, metadata)
            Call g.CreateEdge(root, g.GetElementByID(tree.right.uuid))
        End If
    End Sub
End Module

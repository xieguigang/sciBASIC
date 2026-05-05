Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging

Public Module Serialization

    Public Sub Dump(g As NetworkGraph, s As Stream)

    End Sub

    Public Function Load(s As Stream) As NetworkGraph
        Dim g As New NetworkGraph

        Using file As New BinaryDataReader(s)
            g.id = file.ReadString
            g.name = file.ReadString

            For Each v As Graph.Node In NodeFile.ReadNode(file)
                Call g.AddNode(v, assignId:=False)
            Next
            For Each e As NetworkEdge In EdgeFile.ReadNetwork(file)
                Dim u = g.GetElementByID(e.fromNode)
                Dim v = g.GetElementByID(e.toNode)

                Call g.CreateEdge(u, v, e.value, New EdgeData With {
                    .length = Val(e!length),
                    .label = e!label,
                    .style = New Pen(e!color.ToColor, Val(e!width)),
                    .Properties = e.Properties
                })
            Next
        End Using

        Return g
    End Function

End Module

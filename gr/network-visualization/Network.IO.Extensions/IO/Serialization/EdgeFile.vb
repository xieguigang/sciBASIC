Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON

Module EdgeFile

    <Extension>
    Public Sub SaveOneEdge(edge As Edge, s As BinaryDataWriter)
        Dim table As New NetworkEdge(edge)

        Call s.Write(table.fromNode)
        Call s.Write(table.toNode)
        Call s.Write(table.value)
        Call s.Write(If(table.interaction, ""))
        Call s.Write(table.Properties.GetJson)
    End Sub

    Public Iterator Function ReadNetwork(file As BinaryDataReader, count As Integer) As IEnumerable(Of NetworkEdge)
        For i As Integer = 0 To count - 1
            Yield New NetworkEdge With {
                .fromNode = file.ReadString,
                .toNode = file.ReadString,
                .value = file.ReadDouble,
                .interaction = file.ReadString,
                .Properties = file.ReadString.LoadJSON(Of Dictionary(Of String, String))
            }
        Next
    End Function

End Module

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module EdgeFile

    <Extension>
    Public Sub SaveOneEdge(edge As Edge, s As BinaryDataWriter)

    End Sub

    Public Iterator Function ReadNetwork(file As BinaryDataReader, count As Integer) As IEnumerable(Of NetworkEdge)

    End Function

End Module

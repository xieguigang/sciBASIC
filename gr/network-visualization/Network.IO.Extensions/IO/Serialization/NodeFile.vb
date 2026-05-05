Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON

Module NodeFile

    <Extension>
    Public Sub SaveOneNode(node As Node, s As BinaryDataWriter)
        Dim data As NodeData = node.data

        Call s.Write(node.ID)
        Call s.Write(node.label)
        Call s.Write(node.pinned)
        Call s.Write(node.visited)
        Call s.Write(node.degree.In)
        Call s.Write(node.degree.Out)

        Call s.Write(data.betweennessCentrality)
        Call s.Write(data.force.X)
        Call s.Write(data.force.Y)
        Call s.Write(If(TypeOf data.initialPostion Is FDGVector2, 2, 3))
        Call s.Write(data.initialPostion.ToArray)
        Call s.Write(Brush.SolidColor(data.color).ToHtmlColor)
        Call s.Write(data.label)
        Call s.Write(data.mass)
        Call s.Write(data.neighborhoods)
        Call s.Write(data.neighbours)
        Call s.Write(data.origID)
        Call s.Write(data.size.TryCount)
        Call s.Write(data.size)
        Call s.Write(data.weights.TryCount)
        Call s.Write(data.weights)
        Call s.Write(data.Properties.GetJson)
    End Sub

    Public Iterator Function ReadNode(file As BinaryDataReader, count As Integer) As IEnumerable(Of Node)
        For i As Integer = 0 To count - 1
            Dim node As New Node With {
               .ID = file.ReadInt32,
               .label = file.ReadString,
               .pinned = file.ReadBoolean,
               .visited = file.ReadBoolean,
               .degree = New NodeDegree(file.ReadInt32, file.ReadInt32)
            }
            Dim data As New NodeData With {
                .betweennessCentrality = file.ReadDouble,
                .force = New Point(file.ReadInt32, file.ReadInt32),
                .initialPostion = AbstractVector.FromVector(file.ReadDoubles(file.ReadInt32)),
                .color = New SolidBrush(file.ReadString.ToColor),
                .label = file.ReadString,
                .mass = file.ReadDouble,
                .neighbours = file.ReadInt32s(file.ReadInt32),
                .origID = file.ReadString,
                .size = file.ReadDoubles(file.ReadInt32),
                .weights = file.ReadDoubles(file.ReadInt32),
                .Properties = file.ReadString.LoadJSON(Of Dictionary(Of String, String))
            }

            node.data = data

            Yield node
        Next
    End Function

End Module

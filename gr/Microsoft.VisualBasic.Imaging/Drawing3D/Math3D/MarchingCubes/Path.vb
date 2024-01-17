Imports System.Numerics

Namespace Drawing3D.Math3D.MarchingCubes

    Public Class Path
        Public CurrentRadius As Single = PathNode.DefaultRadius
        Public CurrentColor As Drawing.Color = Drawing.Color.White

        Private Sub AddNode(pos As Vector3, startPoint As Boolean)
            Dim node = New GameObject("Node " & transform.childCount.ToString(), GetType(PathNode)).GetComponent(Of PathNode)()

            node.StartPoint = startPoint
            node.Radius = CurrentRadius
            node.Color = CurrentColor
            node.transform.SetParent(transform, False)
            node.transform.localPosition = pos
        End Sub

        Public Sub Clear()
            For Each node In transform.GetComponentsInChildren(Of PathNode)()
                DestroyImmediate(node.gameObject)
            Next
        End Sub

        Public Sub MoveTo(x As Single, y As Single, z As Single)
            AddNode(New Vector3(x, y, z), True)
        End Sub

        Public Sub LineTo(x As Single, y As Single, z As Single)
            AddNode(New Vector3(x, y, z), False)
        End Sub

        Public Sub GetVertices(dest As List(Of DistanceFieldSampler.Vertex))
            Dim pathNodes = GetComponentsInChildren(Of PathNode)()
            For Each pathNode In pathNodes
                dest.Add(New DistanceFieldSampler.Vertex(pathNode))
            Next
        End Sub

        <UsedImplicitly>
        Private Sub OnDrawGizmos()
            Dim nodes = transform.GetComponentsInChildren(Of PathNode)()
            If nodes.Length < 2 Then Return

            Dim prev = nodes(nodes.Length - 1)
            For Each [next] In nodes
                If Not [next].StartPoint Then
                    Gizmos.color = Drawing.Color.Lerp(prev.Color, [next].Color, 0.5F)
                    Gizmos.DrawLine(prev.transform.position, [next].transform.position)
                End If

                prev = [next]
            Next
        End Sub
    End Class
End Namespace
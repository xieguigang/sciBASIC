Namespace KdTree

    Public Class KdTree

        Dim dimensions As Integer()
        Dim points As Object()
        Dim metric As Object

        Sub New(points As Object(), metric As Object, dimensions As Integer())
            Me.points = points
            Me.metric = metric
            Me.dimensions = dimensions
        End Sub

        Private Function buildTree(points As Object(), depth As Integer, parent As Node) As Node
            Dim [dim] = depth Mod dimensions.Length
            Dim median As Double
            Dim node As Node

            If points.Length = 0 Then
                Return Nothing
            ElseIf points.Length = 1 Then
                Return New Node(points(Scan0), [dim], parent)
            End If

            'points.Sort(Function(a, b)
            '                Return a(dimensions([dim])) - b(dimensions([dim]))
            '            End Function)
        End Function
    End Class
End Namespace
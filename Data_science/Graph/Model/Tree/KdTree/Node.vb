Namespace KdTree

    Public Class Node : Inherits Vertex

        Public Property obj As Object
        Public Property left As Node
        Public Property right As Node
        Public Property parent As Node
        Public Property dimension As Integer

        Sub New(obj As Object, dimension%, parent As Node)
            Me.obj = obj
            Me.dimension = dimension
            Me.parent = parent
        End Sub

    End Class
End Namespace
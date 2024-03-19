Namespace AffinityPropagation

    Public Class Graph

        Public ReadOnly Property VerticesCount As Integer

        Public SimMatrixElementsCount As Integer

        Public outEdges As Edge()()
        Public inEdges As Edge()()
        Public Edges As Edge()

        Public Sub New(vertices As Integer)
            VerticesCount = If(vertices < 0, 0, vertices)
            SimMatrixElementsCount = (VerticesCount - 1) * VerticesCount + VerticesCount

            outEdges = New Edge(VerticesCount - 1)() {}
            inEdges = New Edge(VerticesCount - 1)() {}
            Edges = New Edge(SimMatrixElementsCount - 1) {}

            Dim i = 0

            While i < VerticesCount
                outEdges(i) = New Edge(VerticesCount - 1) {}
                inEdges(i) = New Edge(VerticesCount - 1) {}
                i += 1
            End While
        End Sub

    End Class
End Namespace

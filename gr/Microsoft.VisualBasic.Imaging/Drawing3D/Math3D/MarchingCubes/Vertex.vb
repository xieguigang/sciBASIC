Namespace Drawing3D.Math3D.MarchingCubes

    Friend Structure Vertex
        Implements IEquatable(Of Vertex)
        Public ReadOnly A As Integer
        Public ReadOnly B As Integer

        Public Sub New(a As Integer, b As Integer)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{{{0}, {1}}}", VertexIndexToString(A), VertexIndexToString(B))
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            Return TypeOf obj Is Vertex AndAlso Equals(CType(obj, Vertex))
        End Function

        Public Overloads Function Equals(other As Vertex) As Boolean Implements IEquatable(Of Vertex).Equals
            Return A = other.A AndAlso B = other.B
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return A * 397 Xor B
        End Function
    End Structure
End Namespace
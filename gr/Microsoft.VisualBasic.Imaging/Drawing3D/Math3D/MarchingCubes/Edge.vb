Namespace Drawing3D.Math3D.MarchingCubes

    Friend Structure Edge
        Implements IEquatable(Of Edge)
        Public ReadOnly A As Vertex
        Public ReadOnly B As Vertex

        Public ReadOnly Property IsValid As Boolean
            Get
                Return Not A.Equals(B)
            End Get
        End Property
        Public ReadOnly Property Reverse As Edge
            Get
                Return New Edge(B, A)
            End Get
        End Property

        Public Sub New(a As Vertex, b As Vertex)
            Me.A = a
            Me.B = b
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("({0}, {1})", A, B)
        End Function

        Public Overloads Function Equals(other As Edge) As Boolean Implements IEquatable(Of Edge).Equals
            Return A.Equals(other.A) AndAlso B.Equals(other.B)
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If ReferenceEquals(Nothing, obj) Then Return False
            Return TypeOf obj Is Edge AndAlso Equals(CType(obj, Edge))
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return (A.GetHashCode() * 397) Xor B.GetHashCode()
        End Function
    End Structure

End Namespace
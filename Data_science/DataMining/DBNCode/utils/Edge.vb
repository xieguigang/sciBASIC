Namespace utils

    Public Class Edge : Implements IComparable(Of Edge)

        Public Overridable ReadOnly Property Tail As Integer
        Public Overridable ReadOnly Property Head As Integer
        Public Overridable ReadOnly Property OriginalWeight As Double

        Public Overridable Property Weight As Double
            Get
                Return updatedWeight
            End Get
            Set(value As Double)
                updatedWeight = value
            End Set
        End Property

        Dim updatedWeight As Double

        Public Sub New(tail As Integer, head As Integer, weight As Double)
            _Tail = tail
            _Head = head
            updatedWeight = weight
            OriginalWeight = weight
        End Sub

        Public Sub New(tail As Integer, head As Integer)
            Me.New(tail, head, 0)
        End Sub

        Public Overridable Function CompareTo(anotherEdge As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return -1 * updatedWeight.CompareTo(anotherEdge.updatedWeight)
        End Function

        Public Overrides Function ToString() As String
            '		DecimalFormat df = new DecimalFormat("0.00"); 
            '		return "("+ tail + ", "+ head +") "+ df.format(updatedWeight);
            Return "(" & Tail.ToString() & ", " & Head.ToString() & ")"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Const prime = 31
            Dim result = 1
            result = prime * result + Head
            result = prime * result + Tail
            Return result
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If Me Is obj Then
                Return True
            End If
            If obj Is Nothing Then
                Return False
            End If
            If Not (TypeOf obj Is Edge) Then
                Return False
            End If
            Dim other As Edge = CType(obj, Edge)
            If Head <> other.Head Then
                Return False
            End If
            If Tail <> other.Tail Then
                Return False
            End If
            Return True
        End Function
    End Class

End Namespace

Imports Microsoft.VisualBasic.DataVisualization.Network.Layouts

Public Class Edge

    Dim _v As Node
    Dim _u As Node

    Protected Friend _weight As Double = 1.1

    Public ReadOnly Property V As Node
        Get
            Return _v
        End Get
    End Property

    Public ReadOnly Property U As Node
        Get
            Return _u
        End Get
    End Property

    Sub New(v As Node, u As Node)
        Me._v = v
        Me._u = u
    End Sub

    Public Overridable Overloads Function Equals(o As Object) As Boolean
        If TypeOf o Is Edge Then
            Dim other As Edge = DirectCast(o, Edge)
            Return (_v.Equals(other._v) AndAlso _u.Equals(other._u)) OrElse (_u.Equals(other._v) AndAlso _v.Equals(other._u))
        End If
        Return False
    End Function

    Public ReadOnly Property Length As Double
        Get
            Return LayoutCreator.EuclideanDistance(_v, _u)
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{0} <--> {1}", _v.DispName, _u.DispName)
    End Function
End Class
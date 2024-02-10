
Namespace AffinityPropagation

    Public Class Edge : Implements IComparable(Of Edge)
        Public Property Source As Integer
        Public Property Destination As Integer
        Public Property Similarity As Single
        Public Property Responsability As Double
        Public Property Availability As Single

        Public Sub New()
            Destination = 0
            Source = 0
            Availability = 0.0F
            Responsability = 0
            Similarity = 0.0F
        End Sub
        Public Sub New(Source As Integer, Destination As Integer, Similarity As Single)
            Me.Source = Source
            Me.Destination = Destination
            Me.Similarity = Similarity
            Responsability = 0
            Availability = 0
        End Sub
        Public Function CompareTo(obj As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return Similarity.CompareTo(obj.Similarity)
        End Function
    End Class
End Namespace
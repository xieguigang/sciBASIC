
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace AffinityPropagation

    Public Class Edge : Implements IComparable(Of Edge)

        Public Property Source As Integer
        Public Property Destination As Integer
        Public Property Similarity As Double
        Public Property Responsability As Double
        Public Property Availability As Double

        Public Sub New(Source As Integer, Destination As Integer, Similarity As Single)
            Me.Source = Source
            Me.Destination = Destination
            Me.Similarity = Similarity
            Responsability = 0
            Availability = 0
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Source} -> {Destination}] {New Dictionary(Of String, Double) From {
                {"similarity", Similarity},
                {"responsability", Responsability},
                {"availability", Availability}
            }.GetJson }"
        End Function

        Public Function CompareTo(obj As Edge) As Integer Implements IComparable(Of Edge).CompareTo
            Return Similarity.CompareTo(obj.Similarity)
        End Function
    End Class
End Namespace
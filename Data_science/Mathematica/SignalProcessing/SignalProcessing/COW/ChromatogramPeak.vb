Namespace COW

    Public Class ChromatogramPeak

        Public ReadOnly Property ID As Integer
        Public Property Mass As Double
        Public Property Intensity As Double
        Public Property time As Double

        Public Sub New(ByVal id As Integer, ByVal mass As Double, ByVal intensity As Double, ByVal time As Double)
            Me.ID = id
            Me.Mass = mass
            Me.Intensity = intensity
            Me.time = time
        End Sub
    End Class
End Namespace
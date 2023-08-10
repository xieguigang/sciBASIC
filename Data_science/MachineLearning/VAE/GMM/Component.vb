Namespace GMM

    Public Class Component

        Public Overridable Property Weight As Double
        Public Overridable Property Mean As Double
        Public Overridable Property Stdev As Double

        Public Property vector As Double()

        Public Sub New(weight As Double, mean As Double, stdev As Double)
            _Weight = weight
            _Mean = mean
            _Stdev = stdev
        End Sub

    End Class
End Namespace
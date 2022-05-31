Namespace Distributions.LinearMoments

    Public Class FDistribution : Inherits Distributions.ContinuousDistribution

        ReadOnly freedom1 As Integer
        ReadOnly freedom2 As Integer

        Sub New(freedom1 As Integer, freedom2 As Integer)
            Me.freedom1 = freedom1
            Me.freedom2 = freedom2
        End Sub

        Public Overrides Function GetInvCDF(probability As Double) As Double
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetCDF(value As Double) As Double
            Return Distribution.FDistribution(value, freedom1, freedom2)
        End Function

        Public Overrides Function GetPDF(value As Double) As Double
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Validate() As IEnumerable(Of Exception)
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
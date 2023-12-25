Namespace EmGaussian

    Public Class Opts

        ''' <summary>
        ''' time points that possible has a peak
        ''' </summary>
        ''' <returns></returns>
        Public Property components As Double()()
        Public Property maxNumber As Integer = 100
        Public Property maxIterations As Integer = 100
        Public Property tolerance As Double = 0.00001

        Public Shared Function GetDefault() As Opts
            Return New Opts
        End Function
    End Class
End Namespace
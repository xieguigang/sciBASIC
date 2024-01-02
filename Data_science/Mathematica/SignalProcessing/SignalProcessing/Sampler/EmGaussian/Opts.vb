Namespace EmGaussian

    Public Class Opts

        ''' <summary>
        ''' time points that possible has a peak
        ''' </summary>
        ''' <returns></returns>
        Public Property components As Double()()
        ''' <summary>
        ''' max number of components in case of auto-detection
        ''' </summary>
        ''' <returns></returns>
        Public Property maxNumber As Integer = 100
        ''' <summary>
        ''' max number of iterations
        ''' </summary>
        ''' <returns></returns>
        Public Property maxIterations As Integer = 100
        ''' <summary>
        ''' min difference of likelihood
        ''' </summary>
        ''' <returns></returns>
        Public Property tolerance As Double = 0.00001

        Public Property eps As Double = 0.0000000001

        Public Shared Function GetDefault() As Opts
            Return New Opts
        End Function
    End Class
End Namespace
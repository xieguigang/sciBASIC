
Namespace NDtw.Preprocessing

    ''' <summary>
    ''' signal data processor that do nothing
    ''' 
    ''' f(x) = x
    ''' </summary>
    Public Class NonePreprocessor : Implements IPreprocessor

        ''' <summary>
        ''' do nothing at here
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Function Preprocess(data As Double()) As Double() Implements IPreprocessor.Preprocess
            Return data
        End Function

        Public Overrides Function ToString() As String Implements IPreprocessor.ToString
            Return "None"
        End Function
    End Class
End Namespace

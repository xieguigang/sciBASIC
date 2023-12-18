
Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' signal data processor that do nothing
    ''' 
    ''' f(x) = x
    ''' </summary>
    Public Class NonePreprocessor : Inherits IPreprocessor

        ''' <summary>
        ''' do nothing at here
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Preprocess(data As Double()) As Double()
            Return data
        End Function

        Public Overrides Function ToString() As String
            Return "None"
        End Function
    End Class
End Namespace

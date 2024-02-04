Namespace Scripting.Runtime

    Public Interface ICTypeVector

        Function ToNumeric() As Double()
        Function ToFloat() As Single()
        Function ToFactors() As String()
        Function ToInteger() As Integer()
        Function ToLong() As Long()

    End Interface
End Namespace
Namespace Language.Perl

    Public Structure ExceptionHandler

        Dim Message$
        Dim failure As Func(Of Object, Boolean)

        ''' <summary>
        ''' Perl like exception handler syntax for testing the result is failure or not?
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="h"></param>
        ''' <returns></returns>
        Public Shared Operator Or(result As Object, h As ExceptionHandler) As Object
            If h.failure(result) Then
                Throw New Exception(h.Message)
            Else
                Return result
            End If
        End Operator
    End Structure
End Namespace
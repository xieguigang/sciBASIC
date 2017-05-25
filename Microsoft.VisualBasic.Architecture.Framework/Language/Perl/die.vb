Namespace Language.Perl

    Public Structure ExceptionHandler
        Dim Message$

        ''' <summary>
        ''' Perl like exception handler syntax
        ''' </summary>
        ''' <param name="result"></param>
        ''' <param name="h"></param>
        ''' <returns></returns>
        Public Shared Operator Or(result As Boolean, h As ExceptionHandler) As Boolean
            If Not result Then
                Throw New Exception(h.Message)
            Else
                Return True
            End If
        End Operator
    End Structure
End Namespace
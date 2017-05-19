Namespace Language.Perl

    Public Structure ExceptionHandler
        Dim Message$

        Public Shared Operator Or(result As Boolean, h As ExceptionHandler) As Boolean
            If Not result Then
                Throw New Exception(h.Message)
            Else
                Return True
            End If
        End Operator
    End Structure
End Namespace
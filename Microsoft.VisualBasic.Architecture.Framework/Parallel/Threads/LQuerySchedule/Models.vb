Namespace Parallel.Linq

    Public Structure TimeoutModel(Of out)

        Dim timeout As Double
        Dim task As Func(Of out())

        Public Function Invoke() As out()
            Dim result As out() = Nothing

            If OperationTimeOut(task, result, timeout) Then
                Return Nothing
            Else
                Return result
            End If
        End Function
    End Structure
End Namespace
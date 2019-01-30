Namespace Language.JavaScript

    Public Class UnionType(Of T)

        Public value As T
        Public lambda As Func(Of T)

        Public Overloads Function [GetType]() As Type
            If lambda Is Nothing Then
                Return value.GetType()
            Else
                Return GetType(Func(Of T))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetType().ToString
        End Function
    End Class
End Namespace
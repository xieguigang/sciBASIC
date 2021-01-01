Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ObjectNotFoundException : Inherits Exception

        Sub New(message As String)
            Call MyBase.New(message)
        End Sub
    End Class
End Namespace
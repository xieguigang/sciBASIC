Namespace ApplicationServices.Debugging.Diagnostics

    Public Class ExceptionData

        Public Property TypeFullName As String
        Public Property Message As String
        Public Property StackTrace As StackFrame()

        Public Overrides Function ToString() As String
            Return $"{TypeFullName}::{Message}"
        End Function
    End Class

    Public Class StackFrame
        Public Property Method As String
        Public Property File As String
        Public Property Line As String
    End Class
End Namespace
Namespace CommandLine.InteropService.SharedORM

    Public Class R : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
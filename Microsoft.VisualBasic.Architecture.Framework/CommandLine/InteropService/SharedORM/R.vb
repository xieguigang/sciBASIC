Namespace CommandLine.InteropService.SharedORM

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>因为会和Unix bash之中的``ls``命令之中的``-r``参数冲突，所以在这里命名为``Rlanguage``</remarks>
    Public Class RLanguage : Inherits CodeGenerator

        Public Sub New(CLI As Type)
            MyBase.New(CLI)
        End Sub

        Public Overrides Function GetSourceCode() As String
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
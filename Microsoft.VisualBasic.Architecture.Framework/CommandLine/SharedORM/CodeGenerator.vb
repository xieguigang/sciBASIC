Namespace CommandLine.SharedORM

    Public MustInherit Class CodeGenerator

        Protected ReadOnly App As Interpreter
        Protected ReadOnly exe$

        Sub New(CLI As Type)
            App = New Interpreter(type:=CLI)
            exe = CLI.Assembly.CodeBase.BaseName
        End Sub

        Public MustOverride Function GetSourceCode() As String

    End Class
End Namespace
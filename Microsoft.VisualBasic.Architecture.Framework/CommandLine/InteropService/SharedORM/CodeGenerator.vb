Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.InteropService.SharedORM

    Public MustInherit Class CodeGenerator

        Protected ReadOnly App As Interpreter
        Protected ReadOnly exe$

        Sub New(CLI As Type)
            App = New Interpreter(type:=CLI)
            exe = CLI.Assembly.CodeBase.BaseName
        End Sub

        Public MustOverride Function GetSourceCode() As String

        Public Iterator Function EnumeratesAPI() As IEnumerable(Of NamedValue(Of CommandLine))
            For Each api As APIEntryPoint In App.APIList
                Try
                    Yield New NamedValue(Of CommandLine) With {
                        .Name = api.Name,
                        .Description = api.Info,
                        .Value = api.Usage.CommandLineModel
                    }
                Catch ex As Exception
                    ex = New Exception(api.EntryPointFullName(False))
                    Throw ex
                End Try
            Next
        End Function
    End Class
End Namespace
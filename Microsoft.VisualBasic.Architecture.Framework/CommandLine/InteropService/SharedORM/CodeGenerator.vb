Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.InteropService.SharedORM

    Public MustInherit Class CodeGenerator

        Protected ReadOnly App As Interpreter
        ''' <summary>
        ''' 目标应用程序模块的文件名，不包含有文件拓展名
        ''' </summary>
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
                        .Name = api.EntryPoint.Name,
                        .Description = $"```
                        {api.Usage.Replace("<", "&lt;")}
                        ```" & vbCrLf & api.Info,
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
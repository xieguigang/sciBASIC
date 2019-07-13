#Region "Microsoft.VisualBasic::dee18c385910bba9c50b797ed61de515, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\CodeGenerator.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CodeGenerator
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: EnumeratesAPI, GetManualPage
    ' 
    '     Class APITuple
    ' 
    '         Properties: API, CLI
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.InteropService.SharedORM

    Public MustInherit Class CodeGenerator

        Protected ReadOnly App As Interpreter
        ''' <summary>
        ''' 目标应用程序模块的文件名，不包含有文件拓展名
        ''' </summary>
        Protected ReadOnly exe$

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(CLI As Type)
            Call Me.New(New Interpreter(type:=CLI))
        End Sub

        Sub New(App As Interpreter)
            Me.App = App
            Me.exe = App.Type _
                .Assembly _
                .CodeBase _
                .BaseName
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetManualPage() As String
            Return App.HelpSummary(markdown:=False)
        End Function

        Public MustOverride Function GetSourceCode() As String

        Public Iterator Function EnumeratesAPI() As IEnumerable(Of APITuple)
            Dim help$
            Dim CLI As NamedValue(Of CommandLine)

            For Each api As APIEntryPoint In App.APIList
                ' 2018-11-02 usage是空的，说明可能没有额外的参数，只需要调用命令即可
                ' 在这里Usage可能是空值
                Dim apiUsage$ = api.Usage Or EmptyString
                Dim usageCommand As CommandLine

                If apiUsage.StringEmpty Then
                    usageCommand = New CommandLine With {
                        .Name = api.Name,
                        .BoolFlags = {},
                        .cliCommandArgvs = api.Name,
                        .SingleValue = api.Name,
                        .Tokens = {api.Name},
                        .__arguments = New List(Of NamedValue(Of String))
                    }
                    Call $"{api.EntryPointFullName(relativePath:=True)} is nothing!".Warning
                Else
                    usageCommand = apiUsage.CommandLineModel
                End If

                Try
                    help =
$"```
{apiUsage.Replace("<", "&lt;")}
```" & vbCrLf & api.Info

                    CLI = New NamedValue(Of CommandLine) With {
                        .Name = api.EntryPoint.Name,
                        .Description = help,
                        .Value = usageCommand
                    }

                    Yield New APITuple With {
                        .CLI = CLI,
                        .API = api.EntryPoint
                    }
                Catch ex As Exception
                    ex = New Exception(api.EntryPointFullName(False), ex)
                    Throw ex
                End Try
            Next
        End Function
    End Class

    Public Class APITuple

        Public Property CLI As NamedValue(Of CommandLine)
        Public Property API As MethodInfo

    End Class
End Namespace

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.SoftwareToolkits

Namespace CommandLine.Reflection

    Module SDKManual

        Public ReadOnly Property DocPath As String = $"{App.ExecutablePath.TrimFileExt}.md"

        ''' <summary>
        ''' 这个是用于在终端上面显示的无格式的文本输出
        ''' </summary>
        ''' <param name="CLI"></param>
        ''' <returns></returns>
        <Extension>
        Public Function LaunchManual(CLI As Interpreter) As Integer
            Dim assm As New ApplicationDetails
            Dim title As String = $"{Application.ProductName} [version {Application.ProductVersion}]" & vbCrLf &
                assm.ProductTitle & vbCrLf &
                assm.ProductDescription & vbCrLf &
                assm.CompanyName & vbCrLf &
                assm.CopyRightsDetail

            Dim sb As New StringBuilder

            Call sb.AppendLine($"Module AssemblyName: {App.ExecutablePath.ToFileURL}")
            Call sb.AppendLine($"Root namespace: " & CLI.ToString)
            Call sb.AppendLine(vbCrLf & vbCrLf & CLI.HelpSummary(False))

            Dim firstPage As String = sb.ToString
            Dim pages As String() =
                DebuggerArgs.DebuggerHelps +
               (LinqAPI.MakeList(Of String) <= From api As SeqValue(Of APIEntryPoint)
                                               In CLI.Values.SeqIterator(offset:=1)
                                               Let index As String = api.i & ".   "
                                               Select index & api.obj.HelpInformation)
            Dim manual As New Terminal.Utility.IndexedManual(pages, title)

            Call manual.ShowManual()

            Return 0
        End Function

        ''' <summary>
        ''' 这个是用于保存于文件之中的markdown格式的有格式标记的文本输出
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function MarkdownDoc(App As Interpreter) As String
            Dim sb As New StringBuilder($"# {Application.ProductName} [version {Application.ProductVersion}]")
            Dim type As Type = App.Type

            Call sb.AppendLine()
            Call sb.AppendLine($"**Module AssemblyName**: {type.Assembly.Location.ToFileURL}")
            Call sb.AppendLine("**Root namespace**: " & App.Type.FullName)
            Call sb.AppendLine(vbCrLf & vbCrLf & App.HelpSummary(True))
            Call sb.AppendLine("## Commands")
            Call sb.AppendLine("--------------------------")

            For Each CmdlEntry As APIEntryPoint In App.Values
                sb.AppendLine(CmdlEntry.HelpInformation(md:=True))

                If CmdlEntry.ParameterInfo.Count > 0 Then
                    Call sb.AppendLine("#### Accepted Types")

                    For Each param As NamedValue(Of ParameterInfo) In CmdlEntry.ParameterInfo
                        Call sb.AppendLine("##### " & param.Name)

                        For Each pType As Type In param.x.AcceptTypes.SafeQuery
                            Call sb.AppendLine(Actives.DisplType(pType))
                        Next
                    Next
                End If
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Returns the summary brief help information of all of the commands in current cli interpreter.
        ''' (枚举出本CLI解释器之中的所有的命令的帮助的摘要信息)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' <param name="markdown">Output in markdown format?</param>
        <Extension>
        Public Function HelpSummary(App As Interpreter, markdown As Boolean) As String
            Dim sb As New StringBuilder(1024)
            Dim nameMaxLen As Integer =
                App.Values.Select(Function(x) Len(x.Name)).Max

            Call sb.AppendLine(ListAllCommandsPrompt)
            Call sb.AppendLine()

            If markdown Then
                Call sb.AppendLine("|Function API|Info|")
                Call sb.AppendLine("|------------|----|")
            End If

            For Each commandInfo As APIEntryPoint In App.Values
                If Not markdown Then
                    Dim blank As String =
                        New String(c:=" "c, count:=nameMaxLen - Len(commandInfo.Name))
                    Dim line As String = $" {commandInfo.Name}:  {blank}{commandInfo.Info}"

                    Call sb.AppendLine(line)
                Else
                    Call sb.AppendLine($"|{commandInfo.Name}|{commandInfo.Info}|")
                End If
            Next

            Return sb.ToString
        End Function

        Public Const ListAllCommandsPrompt As String = "All of the command that available in this program has been list below:"
    End Module
End Namespace
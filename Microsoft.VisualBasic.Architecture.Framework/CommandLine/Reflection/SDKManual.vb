Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.Debugging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.SoftwareToolkits

Namespace CommandLine.Reflection

    Module SDKManual

        Public ReadOnly Property DocPath As String = $"{App.ExecutablePath.TrimFileExt}.txt"

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
            Call sb.AppendLine(vbCrLf & vbCrLf & CLI.HelpSummary())

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
            Dim sb As New StringBuilder($"{Application.ProductName} [version {Application.ProductVersion}]")
            Dim Index As Integer = 1
            Dim type As Type = App.Type

            Call sb.AppendLine()
            Call sb.AppendLine($"Module AssemblyName: {type.Assembly.Location.ToFileURL}")
            Call sb.AppendLine("Root namespace: " & App.Type.FullName)
            Call sb.AppendLine(vbCrLf & vbCrLf & App.HelpSummary())
            Call sb.AppendLine("Commands")
            Call sb.AppendLine("--------------------------------------------------------------------------------")

            For Each CmdlEntry As APIEntryPoint In App.Values
                sb.AppendLine(Index & ".  " & CmdlEntry.HelpInformation)
                Index += 1
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace
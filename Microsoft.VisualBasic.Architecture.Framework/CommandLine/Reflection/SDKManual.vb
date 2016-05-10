Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.SoftwareToolkits

Namespace CommandLine.Reflection

    Module SDKManual

        Public ReadOnly Property DocPath As String = $"{App.ExecutablePath.TrimFileExt}.txt"

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
    End Module
End Namespace
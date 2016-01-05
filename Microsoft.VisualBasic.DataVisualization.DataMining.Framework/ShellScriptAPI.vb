Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions

<[Namespace]("Tools.DataMining")> Public Module ShellScriptAPI

    <ExportAPI("Write.Csv.Rule")> Public Function SaveAprioRule(data As Generic.IEnumerable(Of AprioriAlgorithm.Entities.Rule), saveto As String) As Boolean
        Return data.SaveTo(saveto, False)
    End Function

End Module

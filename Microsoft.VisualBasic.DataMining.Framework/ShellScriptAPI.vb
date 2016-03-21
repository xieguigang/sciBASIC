Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

<[PackageNamespace]("Tools.DataMining")>
Public Module ShellScriptAPI

    <ExportAPI("Write.Csv.Rule")> Public Function SaveAprioRule(data As IEnumerable(Of AprioriAlgorithm.Entities.Rule), saveTo As String) As Boolean
        Return data.SaveTo(saveTo, False)
    End Function
End Module

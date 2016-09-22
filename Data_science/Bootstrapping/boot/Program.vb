Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Bootstrapping.EigenvectorBootstrapping
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.diffEq

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/Build.Zone", Usage:="/Build.Zone /imports <odes_out.DIR> [/part.N 10 /cluster.N 10 /out <outDIR>]")>
    Public Function BootstrappingExport(args As CommandLine) As Integer
        Dim [in] As String = args("/imports")
        Dim partN As Integer = args.GetValue("/part.N", 10)
        Dim clusterN As Integer = args.GetValue("/cluster.N", 10)
        Dim vec = DefaultEigenvector([in])
        Dim out = [in].LoadData(vec, partN).KMeans(clusterN)
        Dim EXPORT As String = args.GetValue("/out", [in].TrimDIR & $".partN={partN},.clusterN={clusterN}/")
        Dim uid As New Uid(False)

        For Each cluster In out
            Dim Eigenvector As ODEsOut = cluster.Key.GetSample(vec, partN)
            Dim DIR As String = EXPORT & "/" & FormatZero(uid.Plus, "0000")

            Call Eigenvector.DataFrame.Save(DIR & "/Eigenvector.Sample.csv", Encodings.ASCII)
            Call cluster.Value.Select(Function(x) New With {.params = x}).ToArray.SaveTo(DIR & "/Eigenvector.paramZone.csv")
        Next

        Return 0
    End Function
End Module

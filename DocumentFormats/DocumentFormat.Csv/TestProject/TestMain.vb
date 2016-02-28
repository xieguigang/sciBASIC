Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Module TestMain

    Sub Main()
        Dim ssss As String = "ssssss,""ss,sssss"",""ssss""""sss"",sss   sss"
        Dim ttttt = Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.CharsParser(ssss)


        Dim typeschema = Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject(Of SchemaParsingTest)(False)
        Dim data = {New SchemaParsingTest, New SchemaParsingTest}
        Call data.SaveTo("./fgfgfgf.csv")

        data = Nothing
        data = "./fgfgfgf.csv".LoadCsv(Of SchemaParsingTest).ToArray


        Dim nn = "E:\Desktop\DESeq\colR\diffexpr-results.csv".LoadCsv(Of RTools.ResultData)(False)

        Call nn.SaveTo(FileIO.FileSystem.GetTempFileName & ".csv", False)


        Call fdff.d.SaveTo(".\ddd", False)

        Dim nnn = ".\ddd".LoadCsv(Of fdff)(False)

        Dim DataCollection As ExampleExperimentData() = New ExampleExperimentData() {
            New ExampleExperimentData With {.Id = "GeneId_0001", .ExpressionRPKM = 0, .Tags = New String() {"Up", "Regulator"}},
            New ExampleExperimentData With {.Id = "GeneId_0002", .ExpressionRPKM = 1, .Tags = New String() {"Up", "PathwayA"}},
            New ExampleExperimentData With {.Id = "GeneId_0003", .ExpressionRPKM = 2, .Tags = New String() {"Down", "Virulence"}}}

        Dim CsvPath As String = "./TestData.csv"

        Call DataCollection.SaveTo(CsvPath, explicit:=False)

        DataCollection = Nothing
        DataCollection = CsvPath.LoadCsv(Of ExampleExperimentData)(explicit:=False).ToArray

        Dim File = Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.Reflector.Save(Of ExampleExperimentData)(DataCollection, Explicit:=False)
        For Each row In File
            Call Console.WriteLine(row.ToString)
        Next

        Console.WriteLine("Press any key to continute...")
        Console.Read()
    End Sub

    Public Class fdff
        Public Enum ffff
            dd = -100
            f = 2
            ssfd = -10
            sfdff
            aaa = 10
        End Enum

        <Column("gggg")> Public Property fffff As ffff
        Public Property df As ffff

        Public Shared Function d() As fdff()
            Return New fdff() {New fdff With {.fffff = ffff.f, .df = ffff.sfdff}}
        End Function
    End Class

    Public Class ExampleExperimentData
        Public Property Id As String
        <Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.ColumnAttribute("RPKM")>
        Public Property ExpressionRPKM As Double
        <Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.Collection("tags")>
        Public Property Tags As String()
    End Class
End Module

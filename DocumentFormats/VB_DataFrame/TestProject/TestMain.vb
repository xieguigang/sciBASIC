Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.Serialization

Module TestMain

    ReadOnly sss As String = <cc>ssssss,"ss,sssss","ssss""sss","sss  """" sss",""""""""""""</cc>

    Sub Main()



        Dim ddddddddd = CharsParser(<s>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</s>)

        Dim ssssssssss As String = <s>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</s>

        Dim rp As New List(Of Long)
        Dim cp As New List(Of Long)

        VBDebugger.Mute = True

        For i As Integer = 0 To 30000
            rp += Time(Sub() RegexTokenizer(ssssssssss))
            cp += Time(Sub() CharsParser(ssssssssss))
        Next

        VBDebugger.Mute = False

        Call $"regex={rp.Average}ms,    chars={cp.Average}ms".__DEBUG_ECHO

        Dim firstddd As String = "F:\VisualBasic_AppFramework\DocumentFormats\DocumentFormat.Csv\TestProject\parser_TEST.csv".ReadAllLines()(2)
        Dim row = Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.CharsParser(firstddd)

        Dim stream As New DocumentFormat.Csv.DocumentStream.Linq.DataStream("G:\3.29\CP000050\CP000050-SiteMASTScan-Motif_PWM-RegPrecise-Pfam-A.Pfam-String_vs_xcb.PfamA.Pfam-String-virtualFootprints.Csv")

        Call stream.ForEachBlock(Of [Property](Of String))(Sub(array)
                                                               For Each x In array
                                                                   Call x.source.GetJson.__DEBUG_ECHO
                                                               Next
                                                           End Sub)


        Dim ssss As String = sss
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

        Dim File = Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.Reflector.Save(Of ExampleExperimentData)(DataCollection, explicit:=False)
        For Each rowD As RowObject In File
            Call Console.WriteLine(rowD.ToString)
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

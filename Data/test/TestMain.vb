#Region "Microsoft.VisualBasic::49f2fd481e87924b541d0bff0f5fa9eb, sciBASIC#\Data\test\TestMain.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 175
    '    Code Lines: 0
    ' Comment Lines: 124
    '   Blank Lines: 51
    '     File Size: 7.32 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::9105999c0ecfed6688c2add48c06d3b6, ..\visualbasic_App\Data\TestProject\TestMain.vb"

'    ' Author:
'    ' 
'    '       asuka (amethyst.asuka@gcmodeller.org)
'    '       xieguigang (xie.guigang@live.com)
'    '       xie (genetics@smrucc.org)
'    ' 
'    ' Copyright (c) 2016 GPL3 Licensed
'    ' 
'    ' 
'    ' GNU GENERAL PUBLIC LICENSE (GPL3)
'    ' 
'    ' This program is free software: you can redistribute it and/or modify
'    ' it under the terms of the GNU General Public License as published by
'    ' the Free Software Foundation, either version 3 of the License, or
'    ' (at your option) any later version.
'    ' 
'    ' This program is distributed in the hope that it will be useful,
'    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
'    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    ' GNU General Public License for more details.
'    ' 
'    ' You should have received a copy of the GNU General Public License
'    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

'#End Region

'Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
'Imports Microsoft.VisualBasic.Data.csv.Extensions
'Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
'Imports Microsoft.VisualBasic
'Imports Microsoft.VisualBasic.Data.csv.DocumentStream
'Imports Microsoft.VisualBasic.Serialization.JSON
'Imports Microsoft.VisualBasic.Data.csv
'Imports Microsoft.VisualBasic.Language

'Public Class p1
'    Public Property a As String = Now.ToString
'    Public Property b As String = Rnd()
'    Public Property c As String = RandomDouble()
'End Class

'Public Class pp
'    Public Property x As p1
'    Public Property y As p1
'    Public Property z As Double = RandomDouble()
'End Class


'Module TestMain

'    ReadOnly sss As String = <cc>ssssss,"ss,sssss","ssss""sss","sss  """" sss",""""""""""""</cc>

'    Sub Main()

'        Dim uuuuuuuuuuuuu As New Uid(50)


'        Dim fsfsfsd = [Class].GetSchema(Of pp)


'        Dim sssis As New List(Of pp)

'        For i As Integer = 0 To 2000
'            sssis += {New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}, New pp With {.x = New p1, .y = New p1}}

'        Next


'        Call sssis.SaveData("x:\test2")

'        '  Dim schdddema = Microsoft.VisualBasic.Data.csv.Schema.GetSchema(Of SchemaParsingTest)

'        '  Call schema.GetJson.__DEBUG_ECHO
'        '  Call schema.GetJson.SaveTo("./test.json")
'        Pause()

'        Dim ddddddddd = CharsParser(<s>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</s>)

'        Dim ssssssssss As String = <s>"Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"</s>

'        Dim rp As New List(Of Long)
'        Dim cp As New List(Of Long)

'        VBDebugger.Mute = True

'        For i As Integer = 0 To 30000
'            rp += Time(Sub() RegexTokenizer(ssssssssss))
'            cp += Time(Sub() CharsParser(ssssssssss))
'        Next

'        VBDebugger.Mute = False

'        Call $"regex={rp.Average}ms,    chars={cp.Average}ms".__DEBUG_ECHO

'        Dim firstddd As String = "F:\VisualBasic_AppFramework\DocumentFormats\DocumentFormat.Csv\TestProject\parser_TEST.csv".ReadAllLines()(2)
'        Dim row = Microsoft.VisualBasic.Data.csv.DocumentStream.CharsParser(firstddd)

'        Dim stream As New DocumentStream.Linq.DataStream("G:\3.29\CP000050\CP000050-SiteMASTScan-Motif_PWM-RegPrecise-Pfam-A.Pfam-String_vs_xcb.PfamA.Pfam-String-virtualFootprints.Csv")

'        Call stream.ForEachBlock(Of [Property](Of String))(Sub(array)
'                                                               For Each x In array
'                                                                   Call x.source.GetJson.__DEBUG_ECHO
'                                                               Next
'                                                           End Sub)


'        Dim ssss As String = sss
'        Dim ttttt = Microsoft.VisualBasic.Data.csv.DocumentStream.CharsParser(ssss)


'        Dim typeschema = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider.CreateObject(Of SchemaParsingTest)(False)
'        Dim data = {New SchemaParsingTest, New SchemaParsingTest}
'        Call data.SaveTo("./fgfgfgf.csv")

'        data = Nothing
'        data = "./fgfgfgf.csv".LoadCsv(Of SchemaParsingTest).ToArray


'        Dim nn = "E:\Desktop\DESeq\colR\diffexpr-results.csv".LoadCsv(Of RTools.DESeq.ResultData)(False)

'        Call nn.SaveTo(FileIO.FileSystem.GetTempFileName & ".csv", False)


'        Call fdff.d.SaveTo(".\ddd", False)

'        Dim nnn = ".\ddd".LoadCsv(Of fdff)(False)

'        Dim DataCollection As ExampleExperimentData() = New ExampleExperimentData() {
'            New ExampleExperimentData With {.Id = "GeneId_0001", .ExpressionRPKM = 0, .Tags = New String() {"Up", "Regulator"}},
'            New ExampleExperimentData With {.Id = "GeneId_0002", .ExpressionRPKM = 1, .Tags = New String() {"Up", "PathwayA"}},
'            New ExampleExperimentData With {.Id = "GeneId_0003", .ExpressionRPKM = 2, .Tags = New String() {"Down", "Virulence"}}}

'        Dim CsvPath As String = "./TestData.csv"

'        Call DataCollection.SaveTo(CsvPath, explicit:=False)

'        DataCollection = Nothing
'        DataCollection = CsvPath.LoadCsv(Of ExampleExperimentData)(explicit:=False).ToArray

'        Dim File = Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Reflector.Save(Of ExampleExperimentData)(DataCollection, explicit:=False)
'        For Each rowD As RowObject In File
'            Call Console.WriteLine(rowD.ToString)
'        Next

'        Console.WriteLine("Press any key to continute...")
'        Console.Read()
'    End Sub

'    Public Class fdff
'        Public Enum ffff
'            dd = -100
'            f = 2
'            ssfd = -10
'            sfdff
'            aaa = 10
'        End Enum

'        <Column("gggg")> Public Property fffff As ffff
'        Public Property df As ffff

'        Public Shared Function d() As fdff()
'            Return New fdff() {New fdff With {.fffff = ffff.f, .df = ffff.sfdff}}
'        End Function
'    End Class

'    Public Class ExampleExperimentData
'        Public Property Id As String
'        <Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.ColumnAttribute("RPKM")>
'        Public Property ExpressionRPKM As Double
'        <Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection.Collection("tags")>
'        Public Property Tags As String()
'    End Class
'End Module

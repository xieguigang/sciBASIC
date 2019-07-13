#Region "Microsoft.VisualBasic::513aef6cc871fc481f0692cbf45b335f, Data\TestProject\SchemaParsingTest.vb"

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

    ' Class SchemaParsingTest
    ' 
    '     Properties: a, b, c, d, e
    '                 f, ffssdfsd, g, h, i
    '                 KV, LINQAlias
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports TestProject.RTools

Public Class SchemaParsingTest

    Public Property a As CommandLine.CommandLine = "1 2 3 4 5"
    Public Property b As String = Guid.NewGuid.ToString
    Public Property c As Double = Rnd()
    Public Property d As Integer = Rnd()
    Public Property e As Long = Now.ToBinary
    Public Property f As Date
    Public Property g As String() = {"35345", "645646", Rnd()}
    Public Property h As Boolean
    <Column("abcc")> Public Property i As GZip.ArchiveAction
    Public Property KV As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(Rnd, "1234")

    <Linq.Mapping.Column(Name:="alias.linq")> Public ReadOnly Property LINQAlias As Date = Now

    Public Property ffssdfsd As NamedValue(Of DESeq)

End Class

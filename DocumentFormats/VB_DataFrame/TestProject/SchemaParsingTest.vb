Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection

Public Class SchemaParsingTest

    Public Property a As CommandLine.CommandLine = "1 2 3 4 5"
    Public Property b As String = Guid.NewGuid.ToString
    Public Property c As Double = RandomDouble()
    Public Property d As Integer = RandomDouble()
    Public Property e As Long = Now.ToBinary
    Public Property f As Date
    Public Property g As String() = {"35345", "645646", RandomDouble()}
    Public Property h As Boolean
    <Column("abcc")> Public Property i As GZip.ArchiveAction
    Public Property KV As KeyValuePair(Of String, String) = New KeyValuePair(Of String, String)(RandomDouble, "1234")

    <Linq.Mapping.Column(Name:="alias.linq")> Public ReadOnly Property LINQAlias As Date = Now

End Class

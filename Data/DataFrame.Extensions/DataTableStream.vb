Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq
Imports TableSchema = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider

Public Module DataTableStream

    <Extension>
    Public Sub StreamTo(Of T As Class)(list As IEnumerable(Of T), table As DataTable,
                                       Optional strict As Boolean = False,
                                       Optional metaBlank As String = "",
                                       Optional nonParallel As Boolean = False,
                                       Optional maps As Dictionary(Of String, String) = Nothing,
                                       Optional reorderKeys As Integer = 0,
                                       Optional layout As Dictionary(Of String, Integer) = Nothing,
                                       Optional tsv As Boolean = False,
                                       Optional transpose As Boolean = False,
                                       Optional silent As Boolean = False)

        Dim argv As New Arguments With {
            .layout = layout,
            .maps = maps,
            .metaBlank = metaBlank,
            .nonParallel = nonParallel,
            .reorderKeys = reorderKeys,
            .silent = silent,
            .strict = strict,
            .transpose = transpose,
            .tsv = tsv
        }
        Dim source As IEnumerable(Of Object) = list.Select(Function(a) CObj(a)).ToArray
        Dim typeDef As Type = GetType(T)
        Dim schema As TableSchema = TableSchema.CreateObjectInternal(typeDef, strict).CopyReadDataFromObject
        Dim rowWriter As RowWriter = New RowWriter(schema, metaBlank, layout) _
            .CacheIndex(source, reorderKeys)
        Dim fieldNames As String() = rowWriter.GetRowNames(maps).ToArray
        Dim metaNames As String() = rowWriter.GetMetaTitles
        Dim hasMetadata As Boolean = metaNames.Any

        For Each name As String In fieldNames.JoinIterates(metaNames)
            Call table.Columns.Add(name, rowWriter.GetColumnType(name, maps))
        Next

        For Each item As Object In source
            Dim row As Object() = New Object(fieldNames.Length + metaNames.Length - 1) {}
            Dim meta As IDictionary = Nothing

            If hasMetadata Then
                rowWriter.metaRow.BindProperty.GetValue(item)
            End If

            For i As Integer = 0 To fieldNames.Length - 1
                row(i) = rowWriter.columns(i).GetValue(item)
            Next
            For i As Integer = 0 To metaNames.Length - 1
                row(i + fieldNames.Length) = If(meta.Contains(key:=metaNames(i)), meta(metaNames(i)), Nothing)
            Next

            Call table.Rows.Add(row)
        Next
    End Sub
End Module

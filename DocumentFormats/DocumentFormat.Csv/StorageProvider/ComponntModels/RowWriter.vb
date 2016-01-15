Namespace StorageProvider.ComponentModels

    Public Class RowWriter

        Public ReadOnly Property Columns As ComponentModels.StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider
        Public ReadOnly Property MetaRow As MetaAttribute

        Sub New(SchemaProvider As SchemaProvider)
            Me.SchemaProvider = SchemaProvider
            Me.Columns = {
                SchemaProvider.Columns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.EnumColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.KeyValuePairColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.CollectionColumns.ToArray(Function(field) DirectCast(field, StorageProvider))
            }.MatrixToVector
            Me.Columns = (From field As StorageProvider In Me.Columns
                          Where Not field Is Nothing
                          Select field).ToArray
            Me.MetaRow = SchemaProvider.MetaAttributes
        End Sub

        Public Function GetRowNames() As DocumentStream.RowObject
            Return New DocumentStream.RowObject(Columns.ToArray(Function(field) field.Name))
        End Function

        Public Function ToRow(Of T As Class)(obj As T) As DocumentStream.RowObject
            Dim row As List(Of String) = (From colum As StorageProvider
                                          In Columns
                                          Let value As Object = colum.BindProperty.GetValue(obj)
                                          Let strData As String = colum.ToString(value)
                                          Select strData).ToList
            Dim metas As String() = __meta(obj)
            Call row.Add(metas)
            Return New DocumentStream.RowObject(row)
        End Function

        Public Function GetMetaTitles(obj As Object) As String()
            Dim hash As IDictionary = DirectCast(MetaRow.BindProperty.GetValue(obj), IDictionary)
            Dim keys As String() = (From x In hash.Keys Select Scripting.ToString(x)).ToArray
            Return keys
        End Function

        Private Function __meta(obj As Object) As String()
            Dim hash As IDictionary = DirectCast(MetaRow.BindProperty.GetValue(obj), IDictionary)
            Dim values As String() = (From x In hash.Values Select Scripting.ToString(x)).ToArray
            Return values
        End Function

        Public Overrides Function ToString() As String
            Return GetRowNames.ToString
        End Function
    End Class
End Namespace
Namespace StorageProvider.ComponentModels

    Public Class RowWriter

        Public ReadOnly Property Columns As ComponentModels.StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider

        Sub New(SchemaProvider As SchemaProvider)
            Me.SchemaProvider = SchemaProvider
            Me.Columns = ({
                SchemaProvider.Columns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.EnumColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.KeyValuePairColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
                SchemaProvider.CollectionColumns.ToArray(Function(field) DirectCast(field, ComponentModels.StorageProvider)),
                New StorageProvider() {DirectCast(SchemaProvider.MetaAttributes, StorageProvider)}}).MatrixToVector
            Me.Columns = (From field As StorageProvider In Me.Columns
                          Where Not field Is Nothing
                          Select field).ToArray
        End Sub

        Public Function GetRowNames() As DocumentStream.RowObject
            Return New DocumentStream.RowObject(Columns.ToArray(Function(field) field.Name))
        End Function

        Public Function ToRow(Of T As Class)(obj As T) As DocumentStream.RowObject
            Return New DocumentStream.RowObject((From colum As StorageProvider In Columns
                                                 Let value As Object = colum.BindProperty.GetValue(obj)
                                                 Let strData = colum.ToString(value)
                                                 Select strData).ToList)
        End Function

        Public Overrides Function ToString() As String
            Return GetRowNames.ToString
        End Function
    End Class
End Namespace
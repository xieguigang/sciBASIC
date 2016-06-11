Option Strict Off

Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language

Namespace StorageProvider.ComponentModels

    Public Class RowWriter

        Public ReadOnly Property Columns As ComponentModels.StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider
        Public ReadOnly Property MetaRow As MetaAttribute

        Sub New(SchemaProvider As SchemaProvider, metaBlank As String)
            Me.SchemaProvider = SchemaProvider
            Me.Columns =
                SchemaProvider.Columns.ToList(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.EnumColumns.ToArray(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.KeyValuePairColumns.ToArray(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.CollectionColumns.ToArray(Function(field) DirectCast(field, StorageProvider))
            Me.Columns =
                LinqAPI.Exec(Of StorageProvider) <= From field As StorageProvider
                                                    In Me.Columns
                                                    Where Not field Is Nothing
                                                    Select field
            Me.MetaRow = SchemaProvider.MetaAttributes
            Me._metaBlank = metaBlank

            If Me.MetaRow Is Nothing Then
                __buildRow = AddressOf __buildRowNullMeta
            Else
                __buildRow = AddressOf __buildRowMeta
            End If
        End Sub

        Public Function GetRowNames() As DocumentStream.RowObject
            Return New DocumentStream.RowObject(Columns.ToArray(Function(field) field.Name))
        End Function

        ReadOnly __buildRow As IRowBuilder
        ''' <summary>
        ''' 填充不存在的动态属性的默认字符串
        ''' </summary>
        ReadOnly _metaBlank As String

        Public Function ToRow(obj As Object) As DocumentStream.RowObject
            Dim row As DocumentStream.RowObject = __buildRow(obj)
            Return row
        End Function

#Region "IRowBuilder"

        ''' <summary>
        ''' 将实体对象映射为一个数据行
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Delegate Function IRowBuilder(obj As Object) As DocumentStream.RowObject

        ''' <summary>
        ''' 这里是没有动态属性的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function __buildRowNullMeta(obj As Object) As DocumentStream.RowObject
            Dim row As List(Of String) = (From colum As StorageProvider
                                          In Columns
                                          Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                                          Let strData As String = colum.ToString(value)
                                          Select strData).ToList
            Return New DocumentStream.RowObject(row)
        End Function

        Dim __cachedIndex As String()

        Public Function CacheIndex(source As IEnumerable(Of Object)) As RowWriter
            If MetaRow Is Nothing Then
                Return Me
            End If

            Dim hashMetas As IDictionary() =
                LinqAPI.Exec(Of IDictionary) <= From obj As Object
                                                In source.AsParallel
                                                Let x As Object = MetaRow.BindProperty.GetValue(obj, Nothing)
                                                Where Not x Is Nothing
                                                Let hash As IDictionary = DirectCast(x, IDictionary)
                                                Select hash

            Dim indexs As IEnumerable(Of String) = (From x As IDictionary
                                                    In hashMetas.AsParallel
                                                    Select From o As Object
                                                           In x.Keys
                                                           Select Scripting.ToString(o)).MatrixAsIterator
            __cachedIndex = indexs.Distinct.ToArray

            Return Me
        End Function

        ''' <summary>
        ''' 这里是含有动态属性的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function __buildRowMeta(obj As Object) As DocumentStream.RowObject
            Dim row As List(Of String) = (From colum As StorageProvider
                                          In Columns
                                          Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                                          Let strData As String = colum.ToString(value)
                                          Select strData).ToList
            Dim metas As String() = __meta(obj)
            Call row.AddRange(metas)
            Return New DocumentStream.RowObject(row)
        End Function
#End Region

        Public Function GetMetaTitles(obj As Object) As String()
            If MetaRow Is Nothing OrElse MetaRow.BindProperty Is Nothing Then
                Return New String() {}
            Else
                Return __cachedIndex
            End If
        End Function

        Private Function __meta(obj As Object) As String()
            Dim source As Object = ' 得到实体之中的字典类型的属性值
                MetaRow.BindProperty.GetValue(obj, Nothing)

            If source Is Nothing Then
                Return _metaBlank.CopyVector(__cachedIndex.Length)
            End If

            Dim values As String() = New String(Me.__cachedIndex.Length - 1) {}
            Dim hash As IDictionary = DirectCast(source, IDictionary)

            For i As Integer = 0 To __cachedIndex.Length - 1
                Dim tag As String = __cachedIndex(i)

                If hash.Contains(tag) Then
                    Dim value As Object = hash(key:=tag)
                    values(i) = Scripting.ToString(value)
                Else
                    values(i) = _metaBlank  ' 假若不存在，则使用默认字符串进行替换，默认是空白字符串
                End If
            Next

            Return values
        End Function

        Public Overrides Function ToString() As String
            Return GetRowNames.ToString
        End Function
    End Class
End Namespace
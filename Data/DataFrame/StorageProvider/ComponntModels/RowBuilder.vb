#Region "Microsoft.VisualBasic::ac98d82a1ababb387feb45e34a9ee396, Data\DataFrame\StorageProvider\ComponntModels\RowBuilder.vb"

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

    '   Total Lines: 264
    '    Code Lines: 167 (63.26%)
    ' Comment Lines: 51 (19.32%)
    '    - Xml Docs: 92.16%
    ' 
    '   Blank Lines: 46 (17.42%)
    '     File Size: 10.61 KB


    '     Interface ISchema
    ' 
    '         Properties: SchemaOridinal
    ' 
    '         Function: GetOrdinal
    ' 
    '     Class RowBuilder
    ' 
    '         Properties: ColumnIndex, Columns, Defaults, HaveMetaAttribute, IndexedFields
    '                     MissingFields, NonIndexed, SchemaProvider
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: doColumnFill, (+2 Overloads) FillData, ToString
    ' 
    '         Sub: IndexOf, SolveReadOnlyMetaConflicts
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    Public Interface ISchema

        ''' <summary>
        ''' 从数据源之中解析出来得到的域列表
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer)
        Function GetOrdinal(name As String) As Integer
    End Interface

    ''' <summary>
    ''' 这个是用于将Csv文件之中的行数据转换为.NET对象的
    ''' </summary>
    Public Class RowBuilder

        ''' <summary>
        ''' 总的列表：出现在csv文件之中的列以及未出现在csv文件之中的列
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Columns As StorageProvider()
        Public ReadOnly Property ColumnIndex As Index(Of String)

        Public ReadOnly Property SchemaProvider As SchemaProvider

        ''' <summary>
        ''' 出现在csv文件之中的列的列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IndexedFields As StorageProvider()
        ''' <summary>
        ''' 未出现在csv文件之中的列的字段的集合，请注意，字典属性不会出现在这个集合之中
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MissingFields As StorageProvider()

        ''' <summary>
        ''' 在csv文件之中未被索引的列的名称和其顺序索引编号
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NonIndexed As Dictionary(Of String, Integer)
        Public ReadOnly Property HaveMetaAttribute As Boolean

        ''' <summary>
        ''' ``{propertyName, defaultValue}``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Defaults As Dictionary(Of String, Object)

        Sub New(schemaProvider As SchemaProvider)
            Dim M = {
                schemaProvider.Columns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                schemaProvider.EnumColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                schemaProvider.KeyValuePairColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                schemaProvider.CollectionColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray
            }

            Me.SchemaProvider = schemaProvider
            Me.Columns = M.IteratesALL _
                .Join(DirectCast(schemaProvider.MetaAttributes, StorageProvider)) _
                .ToArray
            Me.Columns = LinqAPI.Exec(Of StorageProvider) _
 _
                () <= From field As StorageProvider
                      In Me.Columns
                      Where Not field Is Nothing
                      Select field

            HaveMetaAttribute = Not schemaProvider.MetaAttributes Is Nothing
            Defaults = DefaultAttribute.GetDefaultValues(schemaProvider.DeclaringType)
            ColumnIndex = Columns.Select(Function(c) c.Name).ToArray
        End Sub

        ''' <summary>
        ''' 从外部源之中获取本数据集的Schema的信息
        ''' </summary>
        ''' <param name="schema"></param>
        Public Sub IndexOf(schema As ISchema)
            Dim setValue = New SetValue(Of StorageProvider)().GetSet(NameOf(StorageProvider.Ordinal))
            Dim LQuery() = LinqAPI.Exec(Of StorageProvider) _
 _
                () <= From field As StorageProvider
                      In Columns
                      Let ordinal = schema.GetOrdinal(field.Name)
                      Select setValue(field, ordinal)

            _IndexedFields = LQuery _
                .Where(Function(field) field.Ordinal > -1) _
                .ToArray

            Dim Indexed As String() = IndexedFields _
                .Select(Function(field) field.Name.ToLower) _
                .ToArray

            ' 没有被建立索引的都可能会当作为字典数据
            With From colum As KeyValuePair(Of String, Integer)
                 In schema.SchemaOridinal
                 Let key = colum.Key.ToLower
                 Where Array.IndexOf(Indexed, key) = -1
                 Select colum

                _NonIndexed = .ToDictionary(Function(field) field.Key,
                                            Function(field) field.Value)
            End With

            With IndexedFields _
                .Select(Function(i) i.BindProperty.Name) _
                .Indexing

                _MissingFields = Columns _
                    .Where(Function(field)
                               Return Not field.BindProperty.Name Like .ByRef AndAlso Not field.IsMetaField
                           End Function) _
                    .ToArray
            End With
        End Sub

        ''' <summary>
        ''' 对于只读属性而言，由于没有写入的过程，所以在从文件加在csv数据到.NET对象的时候会被放进字典属性里面，从而会导致输出的时候出现重复的域的BUG
        ''' 故而需要在这里将字典属性之中的只读属性的名称移除掉
        ''' </summary>
        Public Sub SolveReadOnlyMetaConflicts(silent As Boolean)
            ' 假若存在字典属性的话，则需要进行额外的处理
            If HaveMetaAttribute Then
                ' why two reference that have the effects????
                Dim schema As SchemaProvider = SchemaProvider.Raw.Raw

                Call "Schema has meta dictionary property...".__DEBUG_ECHO(mute:=silent)

                For Each name In NonIndexed.Keys.ToArray
                    ' 在原始的数据之中可以找得到这个域，则说明是只读属性，移除他
                    If Not schema.GetField(name) Is Nothing Then
                        Call NonIndexed.Remove(name)
#If DEBUG Then
                        Call $"{name} was removed!".__DEBUG_ECHO
#End If
                    End If
                Next
            End If
        End Sub

        Public Function FillData(row As RowObject, obj As Object, metaBlank$) As Object
            obj = doColumnFill(row, obj)

            If HaveMetaAttribute Then
                Dim values = From field As KeyValuePair(Of String, Integer)
                             In NonIndexed
                             Let s = row(field.Value)
                             Let str = If(s Is Nothing OrElse s.Length = 0, metaBlank, s)
                             Let value = SchemaProvider.MetaAttributes.LoadMethod(str)
                             Select name = field.Key, value

                Dim meta As IDictionary = SchemaProvider.MetaAttributes.CreateDictionary

                For Each x In values
                    Call meta.Add(x.name, x.value)
                Next

                Call SchemaProvider _
                    .MetaAttributes _
                    .BindProperty _
                    .SetValue(obj, meta, Nothing)
            End If

            Return obj
        End Function

        ''' <summary>
        ''' 这个函数主要是应用于例如sqlite3, netcdf, hdf5等数据文件之中的所存储的对象的批量的反序列化操作
        ''' </summary>
        ''' <param name="row">从数据文件之中所读取出来的一帧数据</param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function FillData(row As IEnumerable(Of NamedValue(Of Object)), obj As Object) As Object
            Dim missing As New List(Of NamedValue(Of Object))
            Dim propValue As Object
            Dim column As StorageProvider
            Dim i As i32 = Scan0

            For Each field As NamedValue(Of Object) In row
                If (i = ColumnIndex.IndexOf(field.Name)) = -1 Then
                    missing += field

                    Continue For
                Else
                    column = _Columns(i)
                    propValue = field.Value
                End If

                If propValue Is Nothing Then
                    propValue = Defaults(column.BindProperty.Name)
                End If

                If Not propValue Is Nothing AndAlso GetType(Byte()) Is propValue.GetType Then

                End If

                Call column.SetValue(obj, propValue)
            Next

            If HaveMetaAttribute Then
                Dim meta As IDictionary = SchemaProvider.MetaAttributes.CreateDictionary

                For Each x In missing
                    Call meta.Add(x.Name, x.Value)
                Next

                Call SchemaProvider _
                    .MetaAttributes _
                    .BindProperty _
                    .SetValue(obj, meta, Nothing)
            End If

            Return obj
        End Function

        ''' <summary>
        ''' Cast <see cref="RowObject"/> to a specific .NET <see cref="Type"/> object.
        ''' </summary>
        ''' <param name="row"></param>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function doColumnFill(row As RowObject, obj As Object) As Object
            Dim column As StorageProvider
            Dim value$
            Dim propValue As Object

            For i As Integer = 0 To IndexedFields.Length - 1
                column = IndexedFields(i)
                value = row.Column(column.Ordinal)

                If String.IsNullOrEmpty(value) Then
                    propValue = Defaults(column.BindProperty.Name)
                Else
                    propValue = column.LoadMethod()(value)
                End If

                Call column.SetValue(obj, propValue)
            Next

            For Each missing In MissingFields
                Call missing.SetValue(obj, Defaults(missing.BindProperty.Name))
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return SchemaProvider.ToString
        End Function
    End Class
End Namespace

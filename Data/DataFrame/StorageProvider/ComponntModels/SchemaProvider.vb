#Region "Microsoft.VisualBasic::d7ccc6002ad634e43309946a8097731c, ..\sciBASIC#\Data\DataFrame\StorageProvider\ComponntModels\SchemaProvider.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.IO.Linq
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' 从目标对象解析出来的Csv文件的结构组织数据
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SchemaProvider : Implements IEnumerable(Of StorageProvider)

        ''' <summary>
        ''' 基本数据类型的列
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Columns As Column()
            Get
                Return _columns
            End Get
            Set(value As Column())
                _columns = value

                If value.IsNullOrEmpty Then
                    _dictColumns = New Dictionary(Of String, Column)
                Else
                    _dictColumns = value _
                        .ToDictionary(Function(x) x.BindProperty.Name)

                    For Each x In value
                        If Not _dictColumns.ContainsKey(x.Name) Then
                            Call _dictColumns.Add(x.Name, x)
                        End If
                    Next
                End If
            End Set
        End Property

        ''' <summary>
        ''' 基本数据类型的数组形式的列
        ''' </summary>
        ''' <remarks></remarks>
        Public Property CollectionColumns As CollectionColumn()
            Get
                Return _collectionColumns
            End Get
            Set(value As CollectionColumn())
                _collectionColumns = value

                If _collectionColumns.IsNullOrEmpty Then
                    _dictCollectionColumns = New Dictionary(Of String, CollectionColumn)
                Else
                    _dictCollectionColumns = value _
                        .ToDictionary(Function(x) x.BindProperty.Name)

                    For Each x In value
                        If Not _dictCollectionColumns.ContainsKey(x.Name) Then
                            Call _dictCollectionColumns.Add(x.Name, x)
                        End If
                    Next
                End If
            End Set
        End Property

        Public Property EnumColumns As [Enum]()
            Get
                Return _enumColumns
            End Get
            Set(value As [Enum]())
                _enumColumns = value
                If _enumColumns.IsNullOrEmpty Then
                    _dictEnumColumns = New Dictionary(Of String, [Enum])
                Else
                    _dictEnumColumns = value _
                        .ToDictionary(Function(x) x.BindProperty.Name)

                    For Each x In value
                        If Not _dictEnumColumns.ContainsKey(x.Name) Then
                            Call _dictEnumColumns.Add(x.Name, x)
                        End If
                    Next
                End If
            End Set
        End Property

        Public Property KeyValuePairColumns As KeyValuePair()
            Get
                Return _keyMeta
            End Get
            Set(value As KeyValuePair())
                _keyMeta = value
                If value.IsNullOrEmpty Then
                    _dictKeyMeta = New Dictionary(Of String, KeyValuePair)
                Else
                    _dictKeyMeta = value _
                        .ToDictionary(Function(x) x.BindProperty.Name)

                    For Each x In value
                        If Not _dictKeyMeta.ContainsKey(x.Name) Then
                            Call _dictKeyMeta.Add(x.Name, x)
                        End If
                    Next
                End If
            End Set
        End Property

        ''' <summary>
        ''' 一个类型之中只可以定义一个元数据存储对象
        ''' </summary>
        ''' <returns></returns>
        Public Property MetaAttributes As MetaAttribute

        ''' <summary>
        ''' 提供当前的schema数据的原始数据
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Raw As SchemaProvider

        Dim _columns As Column()
        Dim _collectionColumns As CollectionColumn()
        Dim _enumColumns As [Enum]()
        Dim _keyMeta As KeyValuePair()

#Region "按照实际的属性名称进行映射"

        Dim _dictColumns As Dictionary(Of String, Column)
        Dim _dictCollectionColumns As Dictionary(Of String, CollectionColumn)
        Dim _dictEnumColumns As Dictionary(Of String, [Enum])
        Dim _dictKeyMeta As Dictionary(Of String, KeyValuePair)
#End Region

        ''' <summary>
        ''' 从Schema之中移除一个绑定的域
        ''' </summary>
        ''' <param name="name$"></param>
        Public Sub Remove(name$)
            If _dictCollectionColumns.ContainsKey(name) Then
                _dictCollectionColumns.Remove(name)
                _collectionColumns = _dictCollectionColumns.Values.ToArray
            ElseIf _dictColumns.ContainsKey(name) Then
                _dictColumns.Remove(name)
                _columns = _dictColumns.Values.ToArray
            ElseIf _dictEnumColumns.ContainsKey(name) Then
                _dictEnumColumns.Remove(name)
                _enumColumns = _dictEnumColumns.Values.ToArray
            ElseIf _dictKeyMeta.ContainsKey(name) Then
                _dictKeyMeta.Remove(name)
                _keyMeta = _dictKeyMeta.Values.ToArray
            Else
                ' 没有找到相应的键名，则不做进一步的处理了
            End If
        End Sub

        Public ReadOnly Property HasMetaAttributes As Boolean
            Get
                Return Not MetaAttributes Is Nothing
            End Get
        End Property

        Public ReadOnly Property DeclaringType As Type

        Public ReadOnly Iterator Property Properties As IEnumerable(Of StorageProvider)
            Get
                For Each p As CollectionColumn In CollectionColumns.SafeQuery
                    Yield p
                Next
                For Each p As Column In Me.Columns.SafeQuery
                    Yield p
                Next
                For Each p As [Enum] In Me.EnumColumns
                    Yield p
                Next
                For Each p As KeyValuePair In Me.KeyValuePairColumns
                    Yield p
                Next
                If Not Me.MetaAttributes Is Nothing Then
                    Yield MetaAttributes
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DeclaringType.FullName
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Name">支持属性名称或者域名称</param>
        ''' <returns></returns>
        Public Function GetField(Name As String) As StorageProvider
            If _dictColumns.ContainsKey(Name) Then
                Return _dictColumns(Name)
            End If
            If _dictCollectionColumns.ContainsKey(Name) Then
                Return _dictCollectionColumns(Name)
            End If
            If _dictEnumColumns.ContainsKey(Name) Then
                Return _dictEnumColumns(Name)
            End If
            If _dictKeyMeta.ContainsKey(Name) Then
                Return _dictKeyMeta(Name)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' For write csv data file.(从目标类型对象之中可以读取这个属性的值将数据写入到文件之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyReadDataFromObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = (From p In CollectionColumns Where p.CanReadDataFromObject Select p).ToArray,
                .Columns = (From p In Columns Where p.CanReadDataFromObject Select p).ToArray,
                .EnumColumns = (From p In EnumColumns Where p.CanReadDataFromObject Select p).ToArray,
                .KeyValuePairColumns = (From p In KeyValuePairColumns Where p.CanReadDataFromObject Select p).ToArray,
                ._Raw = Me,
                .MetaAttributes =
                    If(MetaAttributes IsNot Nothing AndAlso
                       MetaAttributes.CanReadDataFromObject,
                       MetaAttributes,
                       Nothing)
            }
        End Function

        ''' <summary>
        ''' For create object instance.(可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyWriteDataToObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = (From p In CollectionColumns Where p.CanWriteDataToObject Select p).ToArray,
                .Columns = (From p In Columns Where p.CanWriteDataToObject Select p).ToArray,
                .EnumColumns = (From p In EnumColumns Where p.CanWriteDataToObject Select p).ToArray,
                .KeyValuePairColumns = KeyValuePairColumns.Where(Function(p) p.CanWriteDataToObject).ToArray,
                ._Raw = Me,
                .MetaAttributes =
                    If(MetaAttributes IsNot Nothing AndAlso
                       MetaAttributes.CanWriteDataToObject,
                       MetaAttributes,
                       Nothing)
            }
        End Function

        Public Function CacheOrdinal(Df As DataFrame) As SchemaProvider
            Return CacheOrdinal(AddressOf Df.GetOrdinal)
        End Function

        Public Function CacheOrdinal(GetOrdinal As GetOrdinal) As SchemaProvider
            For Each Column As StorageProvider In Columns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In CollectionColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In EnumColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In KeyValuePairColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next

            Return Me
        End Function

        ''' <summary>
        ''' 从域名称来判断
        ''' </summary>
        ''' <param name="Name">从csv文件的header行数据之中所得到的列名称</param>
        ''' <returns></returns>
        Public Function ContainsField(Name As String) As Boolean
            Dim LQuery As StorageProvider =
                LinqAPI.DefaultFirst(Of Column) <=
 _
                From p As Column
                In Columns
                Where String.Equals(Name, p.Name)
                Select p

            If Not LQuery Is Nothing Then
                Return True
            End If

            LQuery = LinqAPI.DefaultFirst(Of CollectionColumn) <=
 _
                From p As CollectionColumn
                In Me.CollectionColumns
                Where String.Equals(Name, p.Name)
                Select p

            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="row">The csv header row.</param>
        ''' <returns></returns>
        Public Function CheckFieldConsistent(row As RowObject) As String
            Dim sb As New StringBuilder

            For Each field As String In row
                If Not ContainsField(field) Then
                    If HasMetaAttributes Then
                        Call sb.AppendLine($"Field: `{field}` probably exists in meta field data.")
                    Else
                        Call sb.AppendLine($"Field: `{field}` can not be found!")
                    End If
                End If
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 从所绑定的属性来判断
        ''' </summary>
        ''' <param name="[Property]"></param>
        ''' <returns></returns>
        ''' <remarks>这个函数还需要进行一些绑定的映射</remarks>
        Public Function ContainsProperty([Property] As PropertyInfo) As Boolean
            Dim LQuery As StorageProvider =
                LinqAPI.DefaultFirst(Of Column) <= From p As Column
                                                   In Columns
                                                   Where [Property].Equals(p.BindProperty)
                                                   Select p
            If Not LQuery Is Nothing Then
                Return True
            End If

            LQuery = LinqAPI.DefaultFirst(Of CollectionColumn) <=
 _
                From p As CollectionColumn
                In Me.CollectionColumns
                Where [Property].Equals(p.BindProperty)
                Select p

            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' Creates the data frame schema for the specific object type.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Shared Function CreateObject(type As Type, Optional strict As Boolean = False) As SchemaProvider
            Dim Properties As Dictionary(Of PropertyInfo, StorageProvider) =
                TypeSchemaProvider.GetProperties(type, strict)

            Dim Schema As New SchemaProvider With {
                .Columns = GetColumns(Properties),
                .CollectionColumns = GetCollectionColumns(Properties),
                .EnumColumns = GetEnumColumns(Properties),
                .MetaAttributes = GetMetaAttributeColumn(Properties, strict),
                .KeyValuePairColumns = GetKeyValuePairColumn(Properties),
                ._DeclaringType = type
            }
            Schema._Raw = Schema

            Return Schema
        End Function

        ''' <summary>
        ''' ``CreateObject(GetType(T), Explicit)``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="strict">是否严格解析？严格的意思就是说只解析出经过自定义属性所定义的属性为列</param>
        ''' <returns></returns>
        Public Shared Function CreateObject(Of T As Class)(strict As Boolean) As SchemaProvider
            Return CreateObject(GetType(T), strict)
        End Function

        Private Shared Function GetKeyValuePairColumn(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As KeyValuePair()
            Return __gets(Of KeyValuePair)(Properties, Function(type) type = ProviderIds.KeyValuePair)
        End Function

        Private Shared Function __gets(Of T As StorageProvider)(
                                Properties As Dictionary(Of PropertyInfo, StorageProvider),
                                ProviderId As Func(Of ProviderIds, Boolean)) As T()

            Dim LQuery As T() = LinqAPI.Exec(Of T) <=
 _
                From [Property] As StorageProvider
                In Properties.Values.AsParallel
                Where ProviderId([Property].ProviderId) = True
                Select DirectCast([Property], T)

            Return LQuery
        End Function

        Const DynamicsNotFound As String = "Explicit option is set TRUE, but could not found Meta attribute for the dynamics property!"

        Public Sub New()
        End Sub

        ''' <summary>
        ''' 对于<see cref="DynamicPropertyBase"/>的继承对象类型，也会自动解析出来的，假若<see cref="MetaAttribute"/>没有被定义的话
        ''' </summary>
        ''' <param name="Properties"></param>
        ''' <returns></returns>
        Private Shared Function GetMetaAttributeColumn(Properties As Dictionary(Of PropertyInfo, StorageProvider), strict As Boolean) As MetaAttribute
            Dim MetaAttributes As MetaAttribute =
                __gets(Of MetaAttribute)(Properties, Function(type) type = ProviderIds.MetaAttribute).FirstOrDefault

            If MetaAttributes Is Nothing Then
                Dim prop As PropertyInfo = Properties.Keys.FirstOrDefault

                If prop Is Nothing Then
                    If strict Then
                        Throw New Exception(DynamicsNotFound)
                    Else
                        Return Nothing
                    End If
                End If

                Dim type As New Value(Of Type)(prop.DeclaringType)

                If (+type).IsInheritsFrom(GetType(DynamicPropertyBase(Of ))) Then
                    Dim metaProp As PropertyInfo =
                        (type = (+type).BaseType).GetProperty(
                        NameOf(DynamicPropertyBase(Of Double).Properties),
                        BindingFlags.Public Or BindingFlags.Instance)
                    type = (+type).GetGenericArguments.First
                    MetaAttributes = New MetaAttribute(
                        New Reflection.MetaAttribute(+type),
                        metaProp)
                End If
            End If

            Return MetaAttributes
        End Function

        Private Shared Function GetEnumColumns(Properties As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider)) As [Enum]()
            Return __gets(Of [Enum])(Properties, Function(type) type = ProviderIds.Enum)
        End Function

        Private Shared Function GetCollectionColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As CollectionColumn()
            Return __gets(Of CollectionColumn)(Properties, Function(type) type = ProviderIds.CollectionColumn)
        End Function

        Private Shared Function GetColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As Column()
            Return __gets(Of Column)(Properties, AddressOf __columnType)
        End Function

        Private Shared Function __columnType(type As ProviderIds) As Boolean
            Return type = Reflection.ProviderIds.Column OrElse
                type = Reflection.ProviderIds.NullMask
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of StorageProvider) Implements IEnumerable(Of StorageProvider).GetEnumerator
            For Each field As StorageProvider In Properties
                Yield field
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

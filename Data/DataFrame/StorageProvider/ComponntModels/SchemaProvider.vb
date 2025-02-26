#Region "Microsoft.VisualBasic::3cac95546523c6cb8e5f4a7c89ab7023, Data\DataFrame\StorageProvider\ComponntModels\SchemaProvider.vb"

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

    '   Total Lines: 505
    '    Code Lines: 360 (71.29%)
    ' Comment Lines: 81 (16.04%)
    '    - Xml Docs: 96.30%
    ' 
    '   Blank Lines: 64 (12.67%)
    '     File Size: 21.24 KB


    '     Class SchemaProvider
    ' 
    '         Properties: CollectionColumns, Columns, DeclaringType, EnumColumns, HasMetaAttributes
    '                     KeyValuePairColumns, MetaAttributes, Raw
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __columnType, (+2 Overloads) CacheOrdinal, CheckFieldConsistent, ContainsField, ContainsProperty
    '                   CopyReadDataFromObject, CopyWriteDataToObject, (+2 Overloads) CreateObject, CreateObjectInternal, GetCollectionColumns
    '                   GetColumns, GetEnumColumns, GetEnumerator, GetField, GetKeyValuePairColumn
    '                   getMeta, GetMetaAttributeColumn, gets, getWriteProvider, IEnumerable_GetEnumerator
    '                   ToString
    ' 
    '         Sub: Remove
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Data.Framework.IO.Linq
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My

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
                        .ToDictionary(Function(x)
                                          Return x.BindProperty.Name
                                      End Function)

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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not MetaAttributes Is Nothing
            End Get
        End Property

        Dim rawType As Type

        ''' <summary>
        ''' The object <see cref="Type"/> that will be convert to csv row or convert from the csv row.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DeclaringType As Type
            Get
                Return rawType
            End Get
        End Property

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return DeclaringType.FullName
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name">支持属性名称或者域名称</param>
        ''' <returns></returns>
        Public Function GetField(name As String) As StorageProvider
            If _dictColumns.ContainsKey(name) Then
                Return _dictColumns(name)
            End If
            If _dictCollectionColumns.ContainsKey(name) Then
                Return _dictCollectionColumns(name)
            End If
            If _dictEnumColumns.ContainsKey(name) Then
                Return _dictEnumColumns(name)
            End If
            If _dictKeyMeta.ContainsKey(name) Then
                Return _dictKeyMeta(name)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' For write csv data file.(从目标类型对象之中可以读取这个属性的值将数据写入到文件之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyReadDataFromObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = (From p As CollectionColumn In CollectionColumns Where p.CanReadDataFromObject Select p).ToArray,
                .Columns = (From p As Column In Columns Where p.CanReadDataFromObject Select p).ToArray,
                .EnumColumns = (From p As [Enum] In EnumColumns Where p.CanReadDataFromObject Select p).ToArray,
                .KeyValuePairColumns = (From p As KeyValuePair In KeyValuePairColumns Where p.CanReadDataFromObject Select p).ToArray,
                .rawType = rawType,
                ._Raw = Me,
                .MetaAttributes = getMeta(writeDataToObject:=False)
            }
        End Function

        ''' <summary>
        ''' For create object instance.
        ''' (可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyWriteDataToObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = getWriteProvider(CollectionColumns).ToArray,
                .Columns = getWriteProvider(Columns).ToArray,
                .EnumColumns = getWriteProvider(EnumColumns).ToArray,
                .KeyValuePairColumns = getWriteProvider(KeyValuePairColumns).ToArray,
                ._Raw = Me,
                .MetaAttributes = getMeta(writeDataToObject:=True),
                .rawType = DeclaringType
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function getWriteProvider(Of T As StorageProvider)(source As IEnumerable(Of T)) As IEnumerable(Of T)
            Return From provider As StorageProvider
                   In source
                   Where provider.CanWriteDataToObject
                   Select DirectCast(provider, T)
        End Function

        Private Function getMeta(writeDataToObject As Boolean) As MetaAttribute
            If MetaAttributes IsNot Nothing Then
                If writeDataToObject AndAlso MetaAttributes.CanWriteDataToObject Then
                    Return MetaAttributes
                ElseIf (Not writeDataToObject) AndAlso MetaAttributes.CanReadDataFromObject Then
                    Return MetaAttributes
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CacheOrdinal(df As DataFrame) As SchemaProvider
            Return CacheOrdinal(AddressOf df.GetOrdinal)
        End Function

        Public Function CacheOrdinal(GetOrdinal As GetOrdinal) As SchemaProvider
            For Each column As StorageProvider In Columns
                column.Ordinal = GetOrdinal(column.Name)
            Next
            For Each column As StorageProvider In CollectionColumns
                column.Ordinal = GetOrdinal(column.Name)
            Next
            For Each column As StorageProvider In EnumColumns
                column.Ordinal = GetOrdinal(column.Name)
            Next
            For Each column As StorageProvider In KeyValuePairColumns
                column.Ordinal = GetOrdinal(column.Name)
            Next

            Return Me
        End Function

        ''' <summary>
        ''' 从域名称来判断
        ''' </summary>
        ''' <param name="name">从csv文件的header行数据之中所得到的列名称</param>
        ''' <returns></returns>
        Public Function ContainsField(name As String) As Boolean
            Dim LQuery As StorageProvider =
                LinqAPI.DefaultFirst(Of Column) <=
 _
                From p As Column
                In Columns
                Where String.Equals(name, p.Name)
                Select p

            If Not LQuery Is Nothing Then
                Return True
            End If

            LQuery = LinqAPI.DefaultFirst(Of CollectionColumn) <=
 _
                From p As CollectionColumn
                In Me.CollectionColumns
                Where String.Equals(name, p.Name)
                Select p

            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="headers">The csv header row.</param>
        ''' <returns>
        ''' 这个函数会输出警告信息,如果没有问题,则返回空字符串
        ''' </returns>
        Public Function CheckFieldConsistent(headers As RowObject) As String
            Dim sb As New StringBuilder

            For Each field As String In headers
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
        ''' <remarks>
        ''' 因为在这里使用了缓存,所以为了防止外部使用的时候意外修改缓存,在这里将这个函数的访问权限修改为仅内部使用
        ''' </remarks>
        Public Shared Function CreateObjectInternal(type As Type, Optional strict As Boolean = False) As SchemaProvider
            Dim staticCache = SingletonHolder(Of Dictionary(Of Type, SchemaProvider)).Instance

            If Not staticCache.ContainsKey(type) Then
                staticCache(type) = CreateObject(type, strict)
            End If

            Return staticCache(type)
        End Function

        Public Shared Function CreateObject(type As Type, Optional strict As Boolean = False) As SchemaProvider
            Dim properties = TypeSchemaProvider.GetProperties(type, strict)
            Dim schema As New SchemaProvider With {
                .Columns = GetColumns(properties),
                .CollectionColumns = GetCollectionColumns(properties),
                .EnumColumns = GetEnumColumns(properties),
                .MetaAttributes = GetMetaAttributeColumn(properties, strict),
                .KeyValuePairColumns = GetKeyValuePairColumn(properties),
                .rawType = type
            }
            schema._Raw = schema

            Return schema
        End Function

        ''' <summary>
        ''' ``CreateObject(GetType(T), Explicit)``
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="strict">是否严格解析？严格的意思就是说只解析出经过自定义属性所定义的属性为列</param>
        ''' <returns></returns>
        Public Shared Function CreateObject(Of T As Class)(strict As Boolean) As SchemaProvider
            Return CreateObjectInternal(GetType(T), strict)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetKeyValuePairColumn(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As KeyValuePair()
            Return gets(Of KeyValuePair)(Properties, Function(type) type = ProviderIds.KeyValuePair).ToArray
        End Function

        Private Shared Function gets(Of T As StorageProvider)(properties As Dictionary(Of PropertyInfo, StorageProvider), providerId As Func(Of ProviderIds, Boolean)) As IEnumerable(Of T)
            Return From [Property] As StorageProvider
                   In properties.Values.AsParallel
                   Where providerId([Property].ProviderId) = True
                   Select DirectCast([Property], T)
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
            Dim MetaAttributes As MetaAttribute = gets(Of MetaAttribute)(Properties, Function(type) type = ProviderIds.MetaAttribute).FirstOrDefault

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
                    Dim metaProp As PropertyInfo = (type = (+type).BaseType).GetProperty(NameOf(DynamicPropertyBase(Of Double).Properties), PublicProperty)
                    type = (+type).GetGenericArguments.First
                    MetaAttributes = New MetaAttribute(New Reflection.MetaAttribute(+type), metaProp)
                End If
            End If

            Return MetaAttributes
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetEnumColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As [Enum]()
            Return gets(Of [Enum])(Properties, Function(type) type = ProviderIds.Enum).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetCollectionColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As CollectionColumn()
            Return gets(Of CollectionColumn)(Properties, Function(type) type = ProviderIds.CollectionColumn).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function GetColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As Column()
            Return gets(Of Column)(Properties, AddressOf __columnType).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function __columnType(type As ProviderIds) As Boolean
            Return type = Reflection.ProviderIds.Column OrElse type = Reflection.ProviderIds.NullMask
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

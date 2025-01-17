#Region "Microsoft.VisualBasic::dca41c84d8592361c10560dbe89167b7, Data\DataFrame\StorageProvider\ComponntModels\RowWriter.vb"

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

    '   Total Lines: 323
    '    Code Lines: 228 (70.59%)
    ' Comment Lines: 50 (15.48%)
    '    - Xml Docs: 82.00%
    ' 
    '   Blank Lines: 45 (13.93%)
    '     File Size: 13.04 KB


    '     Class RowWriter
    ' 
    '         Properties: columns, metaRow, schemaProvider
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetColumnType, GetRowNames, ToRow
    '         Delegate Function
    ' 
    '             Properties: hasMeta, isMetaIndexed
    ' 
    '             Function: __buildRowMeta, __buildRowNullMeta, __meta, CacheIndex, castStr
    '                       extractAllKeys, getAllMetadata, GetMetaTitles, ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Option Strict Off

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Linq.JoinExtensions
Imports any = Microsoft.VisualBasic.Scripting

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' 从.NET对象转换为Csv文件之中的行数据
    ''' </summary>
    Public Class RowWriter

        Public ReadOnly Property columns As StorageProvider()
        Public ReadOnly Property schemaProvider As SchemaProvider
        Public ReadOnly Property metaRow As MetaAttribute

        ''' <summary>
        ''' 由于集合类型的数据会比较长，所以一般是将集合类型放在最后面
        ''' 故而这里只对单个的column类型做原始排序
        ''' </summary>
        ''' <param name="schemaProvider"></param>
        ''' <param name="metaBlank"></param>
        Sub New(schemaProvider As SchemaProvider, metaBlank As String, layoutOrder As Dictionary(Of String, Integer))
            Me.schemaProvider = schemaProvider
            Me.columns =
                schemaProvider.Columns _
                    .ToList(Function(field)
                                Return DirectCast(field, StorageProvider)
                            End Function) +
                schemaProvider.EnumColumns _
                    .Select(Function(field)
                                Return DirectCast(field, StorageProvider)
                            End Function) +
                schemaProvider.KeyValuePairColumns _
                    .Select(Function(field)
                                Return DirectCast(field, StorageProvider)
                            End Function) +
                schemaProvider.CollectionColumns _
                    .Select(Function(field)
                                Return DirectCast(field, StorageProvider)
                            End Function).ToArray
            Me.columns = LinqAPI.Exec(Of StorageProvider) _
                                                          _
                () <= From field As StorageProvider
                      In Me.columns
                      Where Not field Is Nothing
                      Select field

            Me.metaRow = schemaProvider.MetaAttributes
            Me._metaBlank = metaBlank
            Me.hasMeta = Not metaRow Is Nothing

            If Me.metaRow Is Nothing Then
                __buildRow = AddressOf __buildRowNullMeta
            Else
                __buildRow = AddressOf __buildRowMeta
            End If

            Dim properties$() = schemaProvider _
                .DeclaringType _
                .GetProperties(BindingFlags.Public Or BindingFlags.Instance) _
                .Where(Function(d) d.GetIndexParameters.IsNullOrEmpty) _
                .Select(Function(d) d.Name) _
                .ToArray
            Dim rawOrders As New Index(Of String)(properties)
            Dim ordered As StorageProvider() = New StorageProvider(rawOrders.Count - 1) {}

            ' 只对column类型进行原始排序
            With columns _
                .Where(Function(c) c.ProviderId = ProviderIds.Column) _
                .ToDictionary(Function(c) c.BindProperty.Name)

                For Each c As KeyValuePair(Of String, StorageProvider) In .AsEnumerable
                    ordered(rawOrders(c.Key)) = c.Value
                Next
            End With

            ordered = ordered.Where(Function(c) Not c Is Nothing).ToArray
            columns = ordered.AsList +
            columns.Where(Function(c)
                              For Each rc In ordered
                                  If rc Is c Then
                                      ' 是前面经过重新排序的column，则不考虑了      
                                      Return False
                                  End If
                              Next

                              Return True
                          End Function)

            If Not layoutOrder Is Nothing Then
                ' 列的顺序需要通过layoutOrder进行重排序
                columns = columns _
                    .OrderBy(Function(c)
                                 Return If(layoutOrder.ContainsKey(c.Name), layoutOrder(c.Name), 1000)
                             End Function) _
                    .ToArray
            End If
        End Sub

        Public Function GetColumnType(name As String, Optional maps As Dictionary(Of String, String) = Nothing) As Type
            Dim field As StorageProvider

            If Not maps Is Nothing Then
                For Each tuple As KeyValuePair(Of String, String) In maps
                    If tuple.Value = name Then
                        name = tuple.Key
                        Exit For
                    End If
                Next
            End If

            field = columns.Where(Function(c) c.Name = name).FirstOrDefault

            If Not field Is Nothing Then
                Return field.BindProperty.PropertyType
            ElseIf Not metaRow Is Nothing Then
                ' probabily is the metadata field
                Return metaRow.Dictionary
            Else
                Return GetType(Object)
            End If
        End Function

        Public Function GetRowNames(Optional maps As Dictionary(Of String, String) = Nothing) As RowObject
            If maps Is Nothing Then
                Return New RowObject(columns.Select(Function(field) field.Name))
            Else
                Dim __get As Func(Of StorageProvider, String) =
                    Function(x)
                        Return If(maps.ContainsKey(x.Name), maps(x.Name), x.Name)
                    End Function

                Return New RowObject(columns.Select(__get))
            End If
        End Function

        ReadOnly __buildRow As IRowBuilder
        ''' <summary>
        ''' 填充不存在的动态属性的默认字符串
        ''' </summary>
        ReadOnly _metaBlank As String

        Public Function ToRow(obj As Object, numFormat As String) As RowObject
            Dim row As RowObject = __buildRow(obj, numFormat)
            Return row
        End Function

#Region "IRowBuilder"

        ''' <summary>
        ''' 将实体对象映射为一个数据行
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Delegate Function IRowBuilder(obj As Object, numFormat As String) As RowObject

        ''' <summary>
        ''' 这里是没有动态属性的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function __buildRowNullMeta(obj As Object, numFormat As String) As RowObject
            Dim row = LinqAPI.MakeList(Of String) _
                                                  _
            () <= From colum As StorageProvider
                  In columns
                  Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                  Let strData As String = castStr(value, colum, numFormat)
                  Select strData

            Return New RowObject(row)
        End Function

        Private Shared Function castStr(value As Object, colum As StorageProvider, numFormat As String) As String
            If TypeOf value Is Double Then
                If String.IsNullOrEmpty(numFormat) Then
                    Return CDbl(value).ToString
                Else
                    Return CDbl(value).ToString(numFormat)
                End If
            Else
                Return colum.ToString(value)
            End If
        End Function

        Friend __cachedIndex As String()

        Public ReadOnly Property hasMeta As Boolean

        ''' <summary>
        ''' Has the meta field indexed?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isMetaIndexed As Boolean
            Get
                If Not hasMeta Then
                    Return True
                Else
                    Return Not __cachedIndex Is Nothing
                End If
            End Get
        End Property

        Private Shared Function getAllMetadata(source As IEnumerable(Of Object), metaRow As MetaAttribute) As IEnumerable(Of IDictionary)
            ' 获取每一个实体对象的字典属性的值
            Return From obj As Object
                   In source.AsParallel
                   Where Not obj Is Nothing
                   Let x As Object = metaRow.BindProperty.GetValue(obj, Nothing)
                   Where Not x Is Nothing
                   Let hash As IDictionary = DirectCast(x, IDictionary)
                   Select hash
        End Function

        Private Shared Function extractAllKeys(metadatas As IDictionary()) As String()
            Dim q_extract = From x As IDictionary
                            In metadatas.AsParallel
                            Select From o As Object
                                   In x.Keys
                                   Select any.ToString(o)

            Return q_extract.ToArray.IteratesALL.ToArray
        End Function

        ''' <summary>
        ''' 在这个函数之中生成字典动态属性的表头
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="reorderKeys">
        ''' options for sorts the metadata index keys?
        ''' 
        ''' + 0: 不排序
        ''' + 1: 升序排序
        ''' +-1: 降序排序
        ''' </param>
        ''' <returns></returns>
        Public Function CacheIndex(source As IEnumerable(Of Object), Optional reorderKeys As Integer = 0) As RowWriter
            If metaRow Is Nothing Then
                Return Me
            End If

            ' 获取每一个实体对象的字典属性的值
            Dim metadatas = getAllMetadata(source, metaRow).ToArray
            ' 得到所有的键名Keys
            Dim indexs As String() = extractAllKeys(metadatas)

            If reorderKeys > 0 Then
                __cachedIndex = indexs _
                    .Distinct _
                    .OrderBy(Function(name) name) _
                    .ToArray
            ElseIf reorderKeys < 0 Then
                __cachedIndex = indexs _
                    .Distinct _
                    .OrderByDescending(Function(name) name) _
                    .ToArray
            Else
                __cachedIndex = indexs.Distinct.ToArray
            End If

            Return Me
        End Function

        ''' <summary>
        ''' 这里是含有动态属性的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function __buildRowMeta(obj As Object, numFormat As String) As RowObject
            Dim row As List(Of String) = (From colum As StorageProvider
                                          In columns
                                          Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                                          Let strData As String = castStr(value, colum, numFormat)
                                          Select strData).AsList
            Dim metas As String() = __meta(obj)
            Call row.AddRange(metas)
            Return New RowObject(row)
        End Function
#End Region

        Public Function GetMetaTitles() As String()
            If metaRow Is Nothing OrElse metaRow.BindProperty Is Nothing Then
                Return New String() {}
            Else
                Return __cachedIndex
            End If
        End Function

        Private Function __meta(obj As Object) As String()
            ' 得到实体之中的字典类型的属性值
            Dim source As Object = metaRow.BindProperty.GetValue(obj, Nothing)

            If __cachedIndex Is Nothing Then
                Throw New NullReferenceException($"the cache index is nothing, you should run the method '{NameOf(CacheIndex)}' for index cached at first, and then write the data!")
            End If
            If source Is Nothing Then
                Return _metaBlank.Repeats(__cachedIndex.Length)
            End If

            Dim metaSize As Integer = __cachedIndex.Length
            Dim values As String() = New String(metaSize - 1) {}
            Dim hash As IDictionary = DirectCast(source, IDictionary)

            For i As Integer = 0 To __cachedIndex.Length - 1
                Dim tag As String = __cachedIndex(i)

                If hash.Contains(tag) Then
                    values(i) = any.ToString(hash(key:=tag))
                Else
                    ' 假若不存在，则使用默认字符串进行替换，默认是空白字符串
                    values(i) = _metaBlank
                End If
            Next

            Return values
        End Function

        Public Overrides Function ToString() As String
            Return GetRowNames.ToString
        End Function
    End Class
End Namespace

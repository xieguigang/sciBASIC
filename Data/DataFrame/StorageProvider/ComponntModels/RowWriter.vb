#Region "Microsoft.VisualBasic::31175fbb7610f46cb46a5ad290952c37, ..\sciBASIC#\Data\DataFrame\StorageProvider\ComponntModels\RowWriter.vb"

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

Option Strict Off

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' 从.NET对象转换为Csv文件之中的行数据
    ''' </summary>
    Public Class RowWriter

        Public ReadOnly Property Columns As StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider
        Public ReadOnly Property MetaRow As MetaAttribute

        Sub New(SchemaProvider As SchemaProvider, metaBlank As String)
            Me.SchemaProvider = SchemaProvider
            Me.Columns =
                SchemaProvider.Columns _
                    .ToList(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.EnumColumns _
                    .ToArray(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.KeyValuePairColumns _
                    .ToArray(Function(field) DirectCast(field, StorageProvider)) +
                SchemaProvider.CollectionColumns _
                    .ToArray(Function(field) DirectCast(field, StorageProvider))
            Me.Columns =
                LinqAPI.Exec(Of StorageProvider) <= From field As StorageProvider
                                                    In Me.Columns
                                                    Where Not field Is Nothing
                                                    Select field
            Me.MetaRow = SchemaProvider.MetaAttributes
            Me._metaBlank = metaBlank
            Me.HaveMeta = Not MetaRow Is Nothing

            If Me.MetaRow Is Nothing Then
                __buildRow = AddressOf __buildRowNullMeta
            Else
                __buildRow = AddressOf __buildRowMeta
            End If
        End Sub

        Public Function GetRowNames(Optional maps As Dictionary(Of String, String) = Nothing) As RowObject
            If maps Is Nothing Then
                Return New RowObject(Columns.Select(Function(field) field.Name))
            Else
                Dim __get As Func(Of StorageProvider, String) =
                    Function(x) If(maps.ContainsKey(x.Name), maps(x.Name), x.Name)
                Return New RowObject(Columns.Select(__get))
            End If
        End Function

        ReadOnly __buildRow As IRowBuilder
        ''' <summary>
        ''' 填充不存在的动态属性的默认字符串
        ''' </summary>
        ReadOnly _metaBlank As String

        Public Function ToRow(obj As Object) As RowObject
            Dim row As RowObject = __buildRow(obj)
            Return row
        End Function

#Region "IRowBuilder"

        ''' <summary>
        ''' 将实体对象映射为一个数据行
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Delegate Function IRowBuilder(obj As Object) As RowObject

        ''' <summary>
        ''' 这里是没有动态属性的
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Private Function __buildRowNullMeta(obj As Object) As RowObject
            Dim row As List(Of String) = LinqAPI.MakeList(Of String) <=
 _
                From colum As StorageProvider
                In Columns
                Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                Let strData As String = colum.ToString(value)
                Select strData

            Return New RowObject(row)
        End Function

        Friend __cachedIndex As String()

        Public ReadOnly Property HaveMeta As Boolean

        ''' <summary>
        ''' Has the meta field indexed?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsMetaIndexed As Boolean
            Get
                If Not HaveMeta Then
                    Return True
                Else
                    Return Not __cachedIndex Is Nothing
                End If
            End Get
        End Property

        ''' <summary>
        ''' 在这个函数之中生成字典动态属性的表头
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="reorderKeys">
        ''' + 0: 不排序
        ''' + 1: 升序排序
        ''' +-1: 降序排序
        ''' </param>
        ''' <returns></returns>
        Public Function CacheIndex(source As IEnumerable(Of Object), reorderKeys As Integer) As RowWriter
            If MetaRow Is Nothing Then
                Return Me
            End If

            Dim hashMetas As IDictionary() =
                LinqAPI.Exec(Of IDictionary) <= From obj As Object
                                                In source.AsParallel
                                                Let x As Object = MetaRow.BindProperty.GetValue(obj, Nothing)
                                                Where Not x Is Nothing
                                                Let hash As IDictionary = DirectCast(x, IDictionary)
                                                Select hash  ' 获取每一个实体对象的字典属性的值
            ' 得到所有的键名Keys
            Dim indexs As IEnumerable(Of String) = (From x As IDictionary
                                                    In hashMetas.AsParallel
                                                    Select From o As Object
                                                           In x.Keys
                                                           Select Scripting.ToString(o)).IteratesALL
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
        Private Function __buildRowMeta(obj As Object) As RowObject
            Dim row As List(Of String) = (From colum As StorageProvider
                                          In Columns
                                          Let value As Object = colum.BindProperty.GetValue(obj, Nothing)
                                          Let strData As String = colum.ToString(value)
                                          Select strData).ToList
            Dim metas As String() = __meta(obj)
            Call row.AddRange(metas)
            Return New RowObject(row)
        End Function
#End Region

        Public Function GetMetaTitles() As String()
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

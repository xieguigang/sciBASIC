#Region "Microsoft.VisualBasic::f40e4dd9890cba570666cb8d1d5444af, ..\sciBASIC#\Data\DataFrame\StorageProvider\ComponntModels\RowBuilder.vb"

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

Imports Microsoft.VisualBasic.Data.csv.IO
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
        ''' 总的列表
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Columns As StorageProvider()
        Public ReadOnly Property SchemaProvider As SchemaProvider

        Public ReadOnly Property IndexedFields As StorageProvider()
        Public ReadOnly Property NonIndexed As Dictionary(Of String, Integer)
        Public ReadOnly Property HaveMetaAttribute As Boolean

        Sub New(SchemaProvider As SchemaProvider)
            Dim M = {
                SchemaProvider.Columns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                SchemaProvider.EnumColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                SchemaProvider.KeyValuePairColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray,
                SchemaProvider.CollectionColumns _
                    .Select(Function(field) DirectCast(field, StorageProvider)).ToArray
            }

            Me.SchemaProvider = SchemaProvider
            Me.Columns = M.IteratesALL _
                .Join(DirectCast(SchemaProvider.MetaAttributes, StorageProvider)) _
                .ToArray
            Me.Columns = LinqAPI.Exec(Of StorageProvider) <=
 _
                From field As StorageProvider
                In Me.Columns
                Where Not field Is Nothing
                Select field

            HaveMetaAttribute = Not SchemaProvider.MetaAttributes Is Nothing
        End Sub

        ''' <summary>
        ''' 从外部源之中获取本数据集的Schema的信息
        ''' </summary>
        ''' <param name="schema"></param>
        Public Sub Indexof(schema As ISchema)
            Dim setValue = New SetValue(Of StorageProvider)() _
                .GetSet(NameOf(StorageProvider.Ordinal))
            Dim LQuery As StorageProvider() =
                LinqAPI.Exec(Of StorageProvider) <= From field As StorageProvider
                                                    In Columns
                                                    Let ordinal As Integer =
                                                        schema.GetOrdinal(field.Name)
                                                    Select setValue(field, ordinal)
            _IndexedFields = LQuery _
                .Where(Function(field) field.Ordinal > -1) _
                .ToArray

            Dim Indexed As String() = IndexedFields _
                .Select(Function(field) field.Name.ToLower) _
                .ToArray

            '没有被建立索引的都可能会当作为字典数据
            _NonIndexed = (From colum As KeyValuePair(Of String, Integer)
                           In schema.SchemaOridinal
                           Where Array.IndexOf(Indexed, colum.Key.ToLower) = -1
                           Select colum).ToDictionary(Function(field) field.Key,
                                                      Function(field) field.Value)
        End Sub

        ''' <summary>
        ''' 对于只读属性而言，由于没有写入的过程，所以在从文件加在csv数据到.NET对象的时候会被放进字典属性里面，从而会导致输出的时候出现重复的域的BUG
        ''' 故而需要在这里将字典属性之中的只读属性的名称移除掉
        ''' </summary>
        Public Sub SolveReadOnlyMetaConflicts()
            If HaveMetaAttribute Then ' 假若存在字典属性的话，则需要进行额外的处理
                Dim schema As SchemaProvider = SchemaProvider.Raw.Raw ' why two reference that have the effects????

                Call "Schema has meta dictionary property...".__DEBUG_ECHO

                For Each name In NonIndexed.Keys.ToArray
                    If Not schema.GetField(name) Is Nothing Then  ' 在原始的数据之中可以找得到这个域，则说明是只读属性，移除他
                        Call NonIndexed.Remove(name)
#If DEBUG Then
                        Call $"{name} was removed!".__DEBUG_ECHO
#End If
                    End If
                Next
            End If
        End Sub

        Public Function FillData(row As RowObject, obj As Object) As Object
            obj = __tryFill(row, obj)

            If HaveMetaAttribute Then
                Dim values = From field As KeyValuePair(Of String, Integer)
                             In NonIndexed
                             Let s = row(field.Value)
                             Select name = field.Key,
                                  value = SchemaProvider.MetaAttributes.LoadMethod(s)
                Dim meta As IDictionary = SchemaProvider.MetaAttributes.CreateDictionary

                For Each x In values
                    Call meta.Add(x.name, x.value)
                Next

                Call SchemaProvider.MetaAttributes.BindProperty.SetValue(obj, meta, Nothing)
            End If

            Return obj
        End Function

        Private Function __tryFill(row As RowObject, obj As Object) As Object
            Dim i As Integer, column As StorageProvider = Nothing

            For i = 0 To IndexedFields.Length - 1
                column = IndexedFields(i)

                Dim value As String = row.Column(column.Ordinal)
                Dim propValue As Object = column.LoadMethod()(value)

                Call column.BindProperty.SetValue(obj, propValue, Nothing)
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return SchemaProvider.ToString
        End Function
    End Class
End Namespace

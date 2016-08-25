#Region "Microsoft.VisualBasic::15346632f1a7b798cfa1984b9e8c723a, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\StorageProvider\ComponntModels\RowBuilder.vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace StorageProvider.ComponentModels

    Public Interface ISchema
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
            Me.SchemaProvider = SchemaProvider
            Me.Columns = ({
            SchemaProvider.Columns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.EnumColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.KeyValuePairColumns.ToArray(Function(field) DirectCast(field, StorageProvider)),
            SchemaProvider.CollectionColumns.ToArray(Function(field) DirectCast(field, ComponentModels.StorageProvider)),
            New StorageProvider() {DirectCast(SchemaProvider.MetaAttributes, StorageProvider)}}).MatrixToVector
            Me.Columns = (From field As StorageProvider
                          In Me.Columns
                          Where Not field Is Nothing
                          Select field).ToArray
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
            _IndexedFields = LQuery.Where(Function(field) field.Ordinal > -1).ToArray
            Dim Indexed As String() = IndexedFields.ToArray(Function(field) field.Name.ToLower)
            '没有被建立索引的都可能会当作为字典数据
            _NonIndexed = (From colum As KeyValuePair(Of String, Integer)
                           In schema.SchemaOridinal
                           Where Array.IndexOf(Indexed, colum.Key.ToLower) = -1
                           Select colum).ToDictionary(Function(field) field.Key,
                                                      Function(field) field.Value)
        End Sub

        Public Function FillData(Of T As Class)(row As DocumentStream.RowObject, obj As T) As T
            obj = __tryFill(Of T)(row, obj)

            If HaveMetaAttribute Then
                Dim values = (From field As KeyValuePair(Of String, Integer)
                              In NonIndexed
                              Select name = field.Key,
                                  value = SchemaProvider.MetaAttributes.LoadMethod(row.DirectGet(field.Value))).ToArray
                Dim meta As IDictionary = SchemaProvider.MetaAttributes.CreateDictionary

                For Each x In values
                    Call meta.Add(x.name, x.value)
                Next

                Call SchemaProvider.MetaAttributes.BindProperty.SetValue(obj, meta, Nothing)
            End If

            Return obj
        End Function

        Private Function __tryFill(Of T As Class)(row As DocumentStream.RowObject, obj As T) As T
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

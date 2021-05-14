#Region "Microsoft.VisualBasic::e7e860d24dde996ecca0d9317368a9e8, Data\DataFrame\IO\DataFrame\HeaderSchema.vb"

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

    '     Class HeaderSchema
    ' 
    '         Properties: Count, Headers, SchemaOridinal
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: AddAttribute, createSchemaOridinal, GetName, GetOrdinal
    ' 
    '         Sub: ChangeMapping
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace IO

    ''' <summary>
    ''' Create table column schema based on the dataframe title headers
    ''' </summary>
    Public Class HeaderSchema : Implements ISchema

        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal

        Public ReadOnly Property Count As Integer
            Get
                Return SchemaOridinal.Count
            End Get
        End Property

        Public ReadOnly Property Headers As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return SchemaOridinal _
                    .OrderBy(Function(p) p.Value) _
                    .Select(Function(p) p.Key) _
                    .ToArray
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(headers As IEnumerable(Of String))
            SchemaOridinal = createSchemaOridinal(headers)
        End Sub

        Public Function GetName(i As Integer) As String
            If i < 0 Then
                i = SchemaOridinal.Count + i
            End If

            If i >= SchemaOridinal.Count Then
                Throw New IndexOutOfRangeException(i.ToString)
            Else
                Return SchemaOridinal _
                    .First(Function(name) name.Value = i) _
                    .Key
            End If
        End Function

        ''' <summary>
        ''' 这个函数会返回目标列在schema之中的下标序列号
        ''' 如果已经存在了，则会直接返回下标号
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function AddAttribute(Name As String) As Integer
            If SchemaOridinal.ContainsKey(Name) Then
                Return SchemaOridinal(Name)
            Else
                Dim p As Integer = SchemaOridinal.Count
                Call _SchemaOridinal.Add(Name, p)
                Return p
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function GetOrdinal(name As String) As Integer Implements ISchema.GetOrdinal
            If SchemaOridinal.ContainsKey(name) Then
                Return SchemaOridinal(name)
            Else
                Return -1
            End If
        End Function

        Const FieldExists$ = "Required change column name mapping from `{0}` to `{1}`, but the column ``{1}`` is already exists in your file data!"

        ''' <summary>
        ''' ``Csv.Field -> <see cref="PropertyInfo.Name"/>``
        ''' </summary>
        ''' <param name="mapping">``{oldFieldName, newFieldName}``</param>
        ''' <remarks></remarks>
        Public Sub ChangeMapping(mapping As Dictionary(Of String, String))
            Dim p As i32 = 0
            Dim oridinal = SchemaOridinal

            For Each map As NamedValue(Of String) In mapping.IterateNameValues
                ' 由于只是改变映射的名称，并不要添加新的列，所以在这里忽略掉不存在的列
                If (p = GetOrdinal(map.Name)) = -1 Then
                    Continue For
                End If

                If oridinal.ContainsKey(map.Value) AndAlso map.Name <> map.Value Then
                    Dim msg = String.Format(FieldExists, map.Name, map.Value)
                    Dim ex As New Exception(msg)

                    ' 2017-11-4 假设在原来的文件之中存在一个名字叫做ID的列
                    ' 但是在这里进行名称映射的变化的结果也是ID名字的话，
                    ' 则在这里会出现重复键名称的错误
                    Throw ex
                End If

                Call oridinal.Remove(map.Name)
                Call oridinal.Add(map.Value, p)
            Next
        End Sub

        ''' <summary>
        ''' There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!
        ''' </summary>
        Const DuplicatedKeys As String = "There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!"

        ''' <summary>
        ''' Indexing the column headers
        ''' </summary>
        ''' <returns></returns>
        Friend Shared Function createSchemaOridinal(headers As IEnumerable(Of String)) As Dictionary(Of String, Integer)
            Dim arrayCache$() = headers.ToArray
            Dim duplicates$() = arrayCache _
                .GroupBy(Function(s) s) _
                .Where(Function(g) g.Count > 1) _
                .Select(Function(g) g.Key) _
                .ToArray

            If Not duplicates.IsNullOrEmpty Then
                Dim sb As New StringBuilder(DuplicatedKeys)

                Call sb.AppendLine()
                Call sb.AppendLine("Duplicated headers: " & duplicates.GetJson)
                Call sb.AppendLine()

                Call sb.AppendLine("Here is the column header keys in you data: ")
                Call sb.AppendLine()
                Call sb.AppendLine("  " & arrayCache.GetJson)

                Throw New DuplicateNameException(sb.ToString)
            End If

            Return arrayCache _
                .SeqIterator _
                .ToDictionary(Function(i) i.value,
                              Function(i)
                                  Return i.i
                              End Function)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::c4fb8cf71b662cab31109ad7ac8db3a7, Data\DataFrame.Extensions\DataTableStream.vb"

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

    '   Total Lines: 62
    '    Code Lines: 54
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 2.91 KB


    ' Module DataTableStream
    ' 
    '     Sub: StreamTo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq
Imports TableSchema = Microsoft.VisualBasic.Data.csv.StorageProvider.ComponentModels.SchemaProvider

Public Module DataTableStream

    <Extension>
    Public Sub StreamTo(Of T As Class)(list As IEnumerable(Of T), table As DataTable,
                                       Optional strict As Boolean = False,
                                       Optional metaBlank As String = "",
                                       Optional nonParallel As Boolean = False,
                                       Optional maps As Dictionary(Of String, String) = Nothing,
                                       Optional reorderKeys As Integer = 0,
                                       Optional layout As Dictionary(Of String, Integer) = Nothing,
                                       Optional tsv As Boolean = False,
                                       Optional transpose As Boolean = False,
                                       Optional silent As Boolean = False)

        Dim argv As New Arguments With {
            .layout = layout,
            .maps = maps,
            .metaBlank = metaBlank,
            .nonParallel = nonParallel,
            .reorderKeys = reorderKeys,
            .silent = silent,
            .strict = strict,
            .transpose = transpose,
            .tsv = tsv
        }
        Dim source As IEnumerable(Of Object) = list.Select(Function(a) CObj(a)).ToArray
        Dim typeDef As Type = GetType(T)
        Dim schema As TableSchema = TableSchema.CreateObjectInternal(typeDef, strict).CopyReadDataFromObject
        Dim rowWriter As RowWriter = New RowWriter(schema, metaBlank, layout) _
            .CacheIndex(source, reorderKeys)
        Dim fieldNames As String() = rowWriter.GetRowNames(maps).ToArray
        Dim metaNames As String() = rowWriter.GetMetaTitles
        Dim hasMetadata As Boolean = metaNames.Any

        For Each name As String In fieldNames.JoinIterates(metaNames)
            Call table.Columns.Add(name, rowWriter.GetColumnType(name, maps))
        Next

        For Each item As Object In source
            Dim row As Object() = New Object(fieldNames.Length + metaNames.Length - 1) {}
            Dim meta As IDictionary = Nothing

            If hasMetadata Then
                rowWriter.metaRow.BindProperty.GetValue(item)
            End If

            For i As Integer = 0 To fieldNames.Length - 1
                row(i) = rowWriter.columns(i).GetValue(item)
            Next
            For i As Integer = 0 To metaNames.Length - 1
                row(i + fieldNames.Length) = If(meta.Contains(key:=metaNames(i)), meta(metaNames(i)), Nothing)
            Next

            Call table.Rows.Add(row)
        Next
    End Sub
End Module

﻿#Region "Microsoft.VisualBasic::6e5c09a849f0a85f5cc54c060cfeeea9, Data\DataFrame.Extensions\DataTableStream.vb"

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

    '   Total Lines: 209
    '    Code Lines: 134 (64.11%)
    ' Comment Lines: 54 (25.84%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 21 (10.05%)
    '     File Size: 9.65 KB


    ' Module DataTableStream
    ' 
    '     Function: (+2 Overloads) StreamToFrame
    ' 
    '     Sub: StreamTo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.TypeCast
Imports Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports TableSchema = Microsoft.VisualBasic.Data.Framework.StorageProvider.ComponentModels.SchemaProvider

Public Module DataTableStream

    ''' <summary>
    ''' a helper function for load a clr obejct collection into a given datatable
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <param name="table"></param>
    ''' <param name="strict"></param>
    ''' <param name="metaBlank"></param>
    ''' <param name="nonParallel"></param>
    ''' <param name="maps"></param>
    ''' <param name="reorderKeys"></param>
    ''' <param name="layout"></param>
    ''' <param name="transpose"></param>
    ''' <param name="silent"></param>
    <Extension>
    Public Sub StreamTo(Of T As Class)(list As IEnumerable(Of T), table As DataTable,
                                       Optional strict As Boolean = False,
                                       Optional metaBlank As String = "",
                                       Optional nonParallel As Boolean = False,
                                       Optional maps As Dictionary(Of String, String) = Nothing,
                                       Optional reorderKeys As Integer = 0,
                                       Optional layout As Dictionary(Of String, Integer) = Nothing,
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
            .transpose = transpose
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
                meta = rowWriter.metaRow.BindProperty.GetValue(item)
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

    ''' <summary>
    ''' cast the clr object collection as the dataframe
    ''' </summary>
    ''' <remarks>
    ''' this function will create a new dataframe object, and the dataframe object will be
    ''' created by the given clr object collection
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim df As DataFrame = list.StreamToFrame()
    ''' </code>
    ''' </example>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list">a generic clr object collection for make data cast</param>
    ''' <param name="strict">only extract the property/field value which has column attribute tagged if this parameter value is config as TRUE.</param>
    ''' <param name="metaBlank"></param>
    ''' <param name="nonParallel"></param>
    ''' <param name="maps"></param>
    ''' <param name="reorderKeys"></param>
    ''' <param name="layout"></param>
    ''' <param name="transpose"></param>
    ''' <param name="silent"></param>
    ''' <returns></returns>
    <Extension>
    Public Function StreamToFrame(Of T As Class)(list As IEnumerable(Of T),
                                                 Optional strict As Boolean = False,
                                                 Optional metaBlank As String = "",
                                                 Optional nonParallel As Boolean = False,
                                                 Optional maps As Dictionary(Of String, String) = Nothing,
                                                 Optional reorderKeys As Integer = 0,
                                                 Optional layout As Dictionary(Of String, Integer) = Nothing,
                                                 Optional transpose As Boolean = False,
                                                 Optional silent As Boolean = False) As DataFrame

        Dim source As Object() = list.Select(Function(a) CObj(a)).ToArray
        Dim typeDef As Type = GetType(T)
        Dim dataframe As DataFrame = source.StreamToFrame(
            typeDef:=typeDef,
            strict:=strict,
            metaBlank:=metaBlank,
            nonParallel:=nonParallel,
            maps:=maps,
            reorderKeys:=reorderKeys,
            layout:=layout,
            transpose:=transpose,
            silent:=silent)

        Return dataframe
    End Function

    ''' <summary>
    ''' cast the clr object collection as the dataframe
    ''' </summary>
    ''' <remarks>
    ''' this function will create a new dataframe object, and the dataframe object will be
    ''' created by the given clr object collection
    ''' </remarks>
    ''' <param name="source">a generic clr object collection for make data cast</param>
    ''' <param name="strict">only extract the property/field value which has column attribute tagged if this parameter value is config as TRUE.</param>
    ''' <param name="metaBlank"></param>
    ''' <param name="nonParallel"></param>
    ''' <param name="maps"></param>
    ''' <param name="reorderKeys"></param>
    ''' <param name="layout"></param>
    ''' <param name="transpose"></param>
    ''' <param name="silent"></param>
    ''' <returns></returns>
    <Extension>
    Public Function StreamToFrame(source As Object(), typeDef As Type,
                                  Optional strict As Boolean = False,
                                  Optional metaBlank As String = "",
                                  Optional nonParallel As Boolean = False,
                                  Optional maps As Dictionary(Of String, String) = Nothing,
                                  Optional reorderKeys As Integer = 0,
                                  Optional layout As Dictionary(Of String, Integer) = Nothing,
                                  Optional transpose As Boolean = False,
                                  Optional silent As Boolean = False) As DataFrame

        Dim argv As New Arguments With {
            .layout = layout,
            .maps = maps,
            .metaBlank = metaBlank,
            .nonParallel = nonParallel,
            .reorderKeys = reorderKeys,
            .silent = silent,
            .strict = strict,
            .transpose = transpose
        }
        Dim schema As TableSchema = TableSchema.CreateObjectInternal(typeDef, strict).CopyReadDataFromObject
        Dim rowWriter As RowWriter = New RowWriter(schema, metaBlank, layout).CacheIndex(source, reorderKeys)
        Dim fieldNames As String() = rowWriter.GetRowNames(maps).ToArray
        Dim metaNames As String() = rowWriter.GetMetaTitles
        Dim hasMetadata As Boolean = metaNames.Any
        Dim columns As New Dictionary(Of String, List(Of Object))

        For Each name As String In fieldNames.JoinIterates(metaNames)
            Call columns.Add(name, New List(Of Object))
        Next

        For Each item As Object In source
            Dim meta As IDictionary = Nothing

            If hasMetadata Then
                meta = rowWriter.metaRow.BindProperty.GetValue(item)
            End If

            For i As Integer = 0 To fieldNames.Length - 1
                columns(fieldNames(i)).Add(rowWriter.columns(i).GetValue(item))
            Next
            For i As Integer = 0 To metaNames.Length - 1
                columns(metaNames(i)).Add(If(meta.Contains(key:=metaNames(i)), meta(metaNames(i)), Nothing))
            Next
        Next

        Dim df As New DataFrame With {
            .rownames = Enumerable _
                .Range(1, source.Length) _
                .Select(Function(i) $"#{i}") _
                .ToArray,
            .features = New Dictionary(Of String, FeatureVector)
        }

        For Each col As KeyValuePair(Of String, List(Of Object)) In columns
            Dim type As Type = rowWriter.GetColumnType(col.Key)
            Dim pull_vec As Array = VectorCast.CType(col.Value, type.PrimitiveTypeCode)
            Dim v As FeatureVector = FeatureVector.FromGeneral(col.Key, pull_vec)

            Call df.add(v)
        Next

        Return df
    End Function
End Module

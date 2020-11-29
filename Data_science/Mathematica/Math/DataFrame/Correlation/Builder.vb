#Region "Microsoft.VisualBasic::7ed1dbad2842c97d451647ac114d5308, Data_science\Mathematica\Math\DataFrame\Correlation\Builder.vb"

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

' Module Builder
' 
'     Function: FromTabular, MatrixBuilder
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module Builder

    ''' <summary>
    ''' 一个通用的距离矩阵创建函数
    ''' </summary>
    ''' <typeparam name="DataSet"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="eval"></param>
    ''' <param name="isDistance"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatrixBuilder(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), eval As Func(Of Double(), Double(), Double), isDistance As Boolean) As DistanceMatrix
        Dim allData As DataSet() = data.ToArray
        Dim names As String() = allData.PropertyNames
        Dim keys As String() = allData.Keys
        Dim matrix As Double()() = allData _
            .SeqIterator _
            .AsParallel _
            .Select(Function(d)
                        Dim vec As New List(Of Double)
                        Dim sample = d.value.Properties
                        Dim x As Double() = sample.Takes(names).ToArray
                        Dim y As Double()

                        For Each row As DataSet In allData
                            y = names.Select(Function(key) row(key)).ToArray
                            vec += eval(x, y)
                        Next

                        Return (d.i, vec.ToArray)
                    End Function) _
            .OrderBy(Function(d) d.i) _
            .Select(Function(d) d.Item2) _
            .ToArray

        Return New DistanceMatrix(keys.Indexing, matrix, isDistance)
    End Function

    <Extension>
    Private Function loadAsMatrix(Of DataSet As {INamedValue, DynamicPropertyBase(Of String)})(allData As IEnumerable(Of DataSet), item1$, item2$, correlation$, ByRef names As String()) As Dictionary(Of String, Dictionary(Of String, Double))
        Dim hash As New Dictionary(Of String, Dictionary(Of String, Double))
        Dim a, b As String
        Dim c As Double
        Dim nameList As New List(Of String)

        For Each row As DataSet In allData
            a = row.Properties(item1)
            b = row.Properties(item2)
            c = row.Properties(correlation)

            Call nameList.Add(a)
            Call nameList.Add(b)

            If Not hash.ContainsKey(a) Then
                Call hash.Add(a, New Dictionary(Of String, Double))
            End If
            If Not hash.ContainsKey(b) Then
                Call hash.Add(b, New Dictionary(Of String, Double))
            End If
            If Not hash(a).ContainsKey(b) Then
                Call hash(a).Add(b, c)
            End If
            If Not hash(b).ContainsKey(a) Then
                Call hash(b).Add(a, c)
            End If
        Next

        names = nameList

        Return hash
    End Function

    <Extension>
    Public Function FromTabular(Of DataSet As {INamedValue, DynamicPropertyBase(Of String)})(data As IEnumerable(Of DataSet),
                                                                                             Optional item1$ = "A",
                                                                                             Optional item2$ = "B",
                                                                                             Optional correlation$ = "correlation",
                                                                                             Optional isDistance As Boolean = False) As DistanceMatrix
        Dim names As String() = Nothing
        Dim hash As Dictionary(Of String, Dictionary(Of String, Double)) = data.loadAsMatrix(
            item1:=item1,
            item2:=item2,
            correlation:=correlation,
            names:=names
        )

        With names.Distinct.Indexing
            Return .DoCall(Function(index)
                               Dim matrix As Double()() = index.Objects _
                                  .Select(Function(name)
                                              Return index.Objects _
                                                  .Select(Function(name2)
                                                              If hash(name).ContainsKey(name2) Then
                                                                  Return hash(name)(name2)
                                                              Else
                                                                  Return 0
                                                              End If
                                                          End Function) _
                                                  .ToArray
                                          End Function) _
                                  .ToArray

                               Return New DistanceMatrix(index, matrix, isDistance)
                           End Function)
        End With
    End Function
End Module

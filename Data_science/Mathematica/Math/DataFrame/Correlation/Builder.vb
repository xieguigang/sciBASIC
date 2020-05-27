Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module Builder

    <Extension>
    Public Function MatrixBuilder(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet), eval As Func(Of Double(), Double(), Double), isDistance As Boolean) As DistanceMatrix
        Dim allData = data.ToArray
        Dim names = allData.PropertyNames
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
    Public Function FromTabular(Of DataSet As {INamedValue, DynamicPropertyBase(Of String)})(data As IEnumerable(Of DataSet),
                                                                                             Optional item1$ = "A",
                                                                                             Optional item2$ = "B",
                                                                                             Optional correlation$ = "correlation",
                                                                                             Optional isDistance As Boolean = False) As DistanceMatrix
        Dim allData As DataSet() = data.ToArray
        Dim names As New List(Of String)
        Dim hash As New Dictionary(Of String, Dictionary(Of String, Double))
        Dim a, b As String
        Dim c As Double

        For Each row As DataSet In allData
            a = row.Properties(item1)
            b = row.Properties(item2)
            c = row.Properties(correlation)

            Call names.Add(a)
            Call names.Add(b)

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

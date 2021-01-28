Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module LoadDataMatrix

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
        Dim hash As Dictionary(Of String, Dictionary(Of String, Double)) = data _
            .loadAsMatrix(
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

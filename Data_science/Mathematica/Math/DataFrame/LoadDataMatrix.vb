#Region "Microsoft.VisualBasic::c6ace6f916b84f94c734ad76c5f54991, Data_science\Mathematica\Math\DataFrame\LoadDataMatrix.vb"

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

    '   Total Lines: 94
    '    Code Lines: 78
    ' Comment Lines: 5
    '   Blank Lines: 11
    '     File Size: 4.14 KB


    ' Module LoadDataMatrix
    ' 
    '     Properties: Names
    ' 
    '     Function: FromTabular, loadAsMatrix
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module LoadDataMatrix

    ''' <summary>
    ''' get/set matrix row names
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <returns></returns>
    Public Property Names(matrix As DataMatrix) As String()
        Get
            Return matrix.keys
        End Get
        Set(value As String())
            matrix.names.Clear()
            matrix.names.Add(value)
        End Set
    End Property

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

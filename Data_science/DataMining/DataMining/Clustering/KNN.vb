﻿#Region "Microsoft.VisualBasic::5060f9db75dd7c6da450342ec0a2b3d3, Data_science\DataMining\DataMining\Clustering\KNN.vb"

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

    '   Total Lines: 104
    '    Code Lines: 78
    ' Comment Lines: 11
    '   Blank Lines: 15
    '     File Size: 4.24 KB


    '     Class KNN
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Classify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Parallels = System.Threading.Tasks.Parallel

Namespace Clustering

    Public Class KNN

        ''' <summary>
        ''' this holds the values of the training data
        ''' </summary>
        ReadOnly trainingSet As ClusterEntity()
        ''' <summary>
        ''' this holds the class associated with the values
        ''' </summary>
        ReadOnly clusterNames As Dictionary(Of Integer, String)

        Sub New(trainingSet As IEnumerable(Of ClusterEntity), clusterNames As Dictionary(Of Integer, String))
            Me.trainingSet = trainingSet.ToArray
            Me.clusterNames = clusterNames
        End Sub

        Sub New(trainingSet As IEnumerable(Of EntityClusterModel))
            Dim raw = trainingSet.ToArray
            Dim allNames As String() = raw _
                .Select(Function(d) d.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            Me.trainingSet = raw.Select(Function(r) r.ToModel(allNames)).ToArray
            Me.clusterNames = raw.Select(Function(i) i.Cluster) _
                .Distinct _
                .SeqIterator _
                .ToDictionary(Function(i) i + 1,
                              Function(i)
                                  Return i.value
                              End Function)

            Dim clusterIndex As Dictionary(Of String, Integer) = clusterNames _
                .ToDictionary(Function(d) d.Value,
                              Function(d)
                                  Return d.Key
                              End Function)

            For i As Integer = 0 To Me.trainingSet.Length - 1
                Me.trainingSet(i).cluster = clusterIndex(raw(i).Cluster)
            Next
        End Sub

        Public Iterator Function Classify(testSet As NamedCollection(Of Double)(), K As Integer) As IEnumerable(Of NamedCollection(Of NamedValue(Of Integer)))
            ' create an array where we store the distance from our test data and the training data -> [0]
            ' plus the index of the training data element -> [1]
            Dim distances = New Double(trainingSet.Length - 1)() {}

            For i As Integer = 0 To trainingSet.Length - 1
                distances(i) = New Double(1) {}
            Next

            ' start computing
            For test As Integer = 0 To testSet.Length - 1
                Call Parallels.For(
                    fromInclusive:=0,
                    toExclusive:=trainingSet.Length,
                    body:=Sub(index)
#Disable Warning
                              Dim dist = DistanceMethods.EuclideanDistance(
                                  X:=testSet(test).value,
                                  Y:=trainingSet(index).entityVector
                              )
#Enable Warning
                              distances(index)(0) = dist
                              distances(index)(1) = index
                          End Sub)

                ' sort and select first K of them
                Dim sortedDistances = distances _
                    .AsParallel() _
                    .OrderBy(Function(t) t(0)) _
                    .Take(K)
                Dim result As New List(Of NamedValue(Of Integer))

                ' print and check the result
                For Each d As Double() In sortedDistances
                    Dim predictedClass = clusterNames(CInt(d(1)))
                    Dim output As New NamedValue(Of Integer) With {
                        .Name = testSet(test).name,
                        .Value = d(1),
                        .Description = predictedClass
                    }

                    result.Add(output)
                Next

                Yield New NamedCollection(Of NamedValue(Of Integer)) With {
                    .name = testSet(test).name,
                    .value = result.ToArray
                }
            Next
        End Function
    End Class
End Namespace

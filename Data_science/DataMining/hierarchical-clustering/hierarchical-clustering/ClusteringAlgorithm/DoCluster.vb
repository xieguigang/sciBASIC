#Region "Microsoft.VisualBasic::ce6f7b34380f447520c9583135c9dc30, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\DoCluster.vb"

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

    '   Total Lines: 101
    '    Code Lines: 71
    ' Comment Lines: 11
    '   Blank Lines: 19
    '     File Size: 4.49 KB


    ' Module DoCluster
    ' 
    '     Properties: DefaultClusteringAlgorithm, DefaultLinkageStrategy
    ' 
    '     Function: CreateDistanceMatrix, RunCluster, RunVectorCluster
    '     Class EuclideanTask
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: Solve
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Parallel

Public Module DoCluster

    Public ReadOnly Property DefaultClusteringAlgorithm As [Default](Of ClusteringAlgorithm) = New DefaultClusteringAlgorithm
    Public ReadOnly Property DefaultLinkageStrategy As [Default](Of LinkageStrategy) = New AverageLinkageStrategy()

    <Extension>
    Public Function RunVectorCluster(Of DataSet As {INamedValue, IVector})(objects As IEnumerable(Of DataSet),
                                                                           Optional algorithm As ClusteringAlgorithm = Nothing,
                                                                           Optional linkageStrategy As LinkageStrategy = Nothing) As Cluster
        Dim rawdata = objects.ToArray
        Dim distances As Double()() = rawdata.CreateDistanceMatrix(Function(r) r.Data)
        Dim keys As String() = rawdata.Keys

        linkageStrategy = linkageStrategy Or DefaultLinkageStrategy

        ' with (algorithm or new DefaultClusteringAlgorithm as default) if algorithm is nothing
        With algorithm Or DefaultClusteringAlgorithm  ' (Function(alg) alg Is Nothing)
            ' using (linkageStrategy or new AverageLinkageStrategy as default) if linkageStrategy is nothing
            Dim cluster As Cluster = .performClustering(distances, keys, linkageStrategy)
            Return cluster
        End With
    End Function

    ''' <summary>
    ''' Run hierarchical clustering
    ''' </summary>
    ''' <param name="objects"></param>
    ''' <param name="algorithm">Default is <see cref="DefaultClusteringAlgorithm"/></param>
    ''' <param name="linkageStrategy">Default is <see cref="AverageLinkageStrategy"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function RunCluster(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(objects As IEnumerable(Of DataSet),
                               Optional algorithm As ClusteringAlgorithm = Nothing,
                               Optional linkageStrategy As LinkageStrategy = Nothing) As Cluster

        Dim rawdata = objects.ToArray
        Dim features As String() = objects _
            .Select(Function(a) a.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray
        Dim distances As Double()() = rawdata.CreateDistanceMatrix(Function(r) r(features))
        Dim keys As String() = rawdata.Keys

        linkageStrategy = linkageStrategy Or DefaultLinkageStrategy

        ' with (algorithm or new DefaultClusteringAlgorithm as default) if algorithm is nothing
        With algorithm Or DefaultClusteringAlgorithm  ' (Function(alg) alg Is Nothing)
            ' using (linkageStrategy or new AverageLinkageStrategy as default) if linkageStrategy is nothing
            Dim cluster As Cluster = .performClustering(distances, keys, linkageStrategy)
            Return cluster
        End With
    End Function

    <Extension>
    Private Function CreateDistanceMatrix(Of T)(objects As T(), getVector As Func(Of T, Double())) As Double()()
        Dim rawdata As New List(Of Double())

        For Each r As T In objects
            Call rawdata.Add(getVector(r))
        Next

        Dim par As New EuclideanTask(rawdata.ToArray)

        Return DirectCast(par.Run, EuclideanTask).dist
    End Function

    Private Class EuclideanTask : Inherits VectorTask

        Public dist As Double()()
        Public rawdata As Double()()

        Sub New(rawdata As Double()())
            Call MyBase.New(rawdata.Length)

            Me.dist = New Double(rawdata.Length - 1)() {}
            Me.rawdata = rawdata
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            Dim row As Double()

            For i As Integer = start To ends
                row = rawdata(i)
                dist(i) = rawdata _
                    .Select(Function(r) row.EuclideanDistance(r)) _
                    .ToArray
            Next
        End Sub
    End Class
End Module

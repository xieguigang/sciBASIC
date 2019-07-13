#Region "Microsoft.VisualBasic::afe70abdb84d742297b6a4cdd2057bb0, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\DoCluster.vb"

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

    ' Module DoCluster
    ' 
    '     Function: DistanceMatrix, RunCluster
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module DoCluster

    <Extension>
    Public Function DistanceMatrix(objects As IEnumerable(Of DataSet)) As Double()()
        Dim list = objects.ToArray
        Dim keys = list _
            .Select(Function(obj) obj.Properties.Keys) _
            .IteratesALL _
            .Distinct _
            .ToArray

        Return list _
            .Select(Function(x)
                        Return list _
                            .Select(Function(y) x.EuclideanDistance(y, keys)) _
                            .ToArray
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' Run hierarchical clustering
    ''' </summary>
    ''' <param name="objects"></param>
    ''' <param name="algorithm">Default is <see cref="DefaultClusteringAlgorithm"/></param>
    ''' <param name="linkageStrategy">Default is <see cref="AverageLinkageStrategy"/></param>
    ''' <returns></returns>
    <Extension>
    Public Function RunCluster(objects As IEnumerable(Of DataSet),
                               Optional algorithm As ClusteringAlgorithm = Nothing,
                               Optional linkageStrategy As LinkageStrategy = Nothing) As Cluster

        Dim list = objects.ToArray
        Dim distances = list.DistanceMatrix
        Dim names$() = list.Keys

        ' with (algorithm or new DefaultClusteringAlgorithm as default) if algorithm is nothing
        With algorithm Or New DefaultClusteringAlgorithm().AsDefault ' (Function(alg) alg Is Nothing)

            ' using (linkageStrategy or new AverageLinkageStrategy as default) if linkageStrategy is nothing
            Dim cluster As Cluster = .performClustering(
                distances,
                names,
                linkageStrategy Or New AverageLinkageStrategy().AsDefault)

            Return cluster
        End With
    End Function
End Module

#Region "Microsoft.VisualBasic::bf1eb00ddc581674928d35393cd1e3e8, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanAlgorithm.vb"

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

    '     Class DbscanAlgorithm
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ComputeClusterDBSCAN, RegionQuery
    ' 
    '         Sub: ExpandCluster
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace DBSCAN

    ''' <summary>
    ''' DBSCAN algorithm class, Density-based spatial clustering of applications with noise (DBSCAN) 
    ''' </summary>
    ''' <typeparam name="T">Takes dataset item row (features, preferences, vector) type</typeparam>
    ''' <remarks>
    ''' ###### DBSCAN Clustering Algorithm C# Implementation
    ''' 
    ''' > https://github.com/yusufuzun/dbscan
    ''' </remarks>
    Public Class DbscanAlgorithm(Of T)

        ReadOnly _metricFunc As Func(Of T, T, Double)
        ReadOnly _full As Boolean

        ''' <summary>
        ''' Takes metric function to compute distances between dataset items T
        ''' </summary>
        ''' <param name="metricFunc"></param>
        ''' <param name="full">
        ''' A logical option for indicates that evaluate all neighbor points or not for test and create cluster members
        ''' </param>
        Public Sub New(metricFunc As Func(Of T, T, Double), Optional full As Boolean = True)
            _metricFunc = metricFunc
            _full = full
        End Sub

        ''' <summary>
        ''' Performs the DBSCAN clustering algorithm.
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="epsilon">Desired region ball radius</param>
        ''' <param name="minPts">Minimum number of points to be in a region</param>
        ''' <returns>sets of clusters, renew the parameter</returns>
        Public Function ComputeClusterDBSCAN(allPoints As T(),
                                             epsilon As Double,
                                             minPts As Integer,
                                             Optional ByRef is_seed As Integer() = Nothing,
                                             Optional filterNoise As Boolean = True) As NamedCollection(Of T)()

            Dim allPointsDbscan As DbscanPoint(Of T)() = allPoints _
                .Select(Function(x) New DbscanPoint(Of T)(x)) _
                .ToArray()
            Dim clusterId As Integer = 0
            Dim seeds As New List(Of Integer)

            For i As Integer = 0 To allPointsDbscan.Length - 1
                Dim p As DbscanPoint(Of T) = allPointsDbscan(i)

                If p.IsVisited Then
                    Continue For
                Else
                    p.IsVisited = True
                End If

                Dim neighborPts As DbscanPoint(Of T)() = RegionQuery(allPointsDbscan, p.ClusterPoint, epsilon)

                If neighborPts.Length < minPts Then
                    p.ClusterId = ClusterIDs.Noise
                Else
                    clusterId += 1
                    ' point to be in a cluster
                    p.ClusterId = clusterId
                    ExpandCluster(allPointsDbscan, neighborPts, clusterId, epsilon, minPts)
                    seeds.Add(i)
                End If
            Next

            With allPointsDbscan _
                .Where(Function(x)
                           If Not filterNoise Then
                               Return True
                           Else
                               Return x.ClusterId > 0
                           End If
                       End Function) _
                .GroupBy(Function(x)
                             Return x.ClusterId
                         End Function)

                is_seed = seeds.ToArray

                Return .Select(Function(x)
                                   Return New NamedCollection(Of T) With {
                                       .name = x.Key,
                                       .value = x _
                                           .Select(Function(y) y.ClusterPoint) _
                                           .ToArray()
                                   }
                               End Function) _
                       .ToArray
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        ''' <param name="epsilon">Desired region ball range</param>
        ''' <param name="minPts">Minimum number of points to be in a region</param>
        Private Sub ExpandCluster(allPoints As DbscanPoint(Of T)(),
                                  neighborPts As DbscanPoint(Of T)(),
                                  clusterId As Integer,
                                  epsilon As Double,
                                  minPts As Integer)

            Dim neighborPts2 As DbscanPoint(Of T)() = Nothing

            For Each pn As DbscanPoint(Of T) In neighborPts.Where(Function(p) Not p.IsVisited)
                If _full OrElse Not pn.IsVisited Then
                    pn.IsVisited = True
                    neighborPts2 = RegionQuery(allPoints, pn.ClusterPoint, epsilon)

                    If neighborPts2.Length >= minPts Then
                        neighborPts = neighborPts.Union(neighborPts2).ToArray()
                        ExpandCluster(allPoints, neighborPts2, clusterId, epsilon, minPts)
                    End If
                End If

                If pn.ClusterId = ClusterIDs.Unclassified Then
                    pn.ClusterId = clusterId
                End If
            Next
        End Sub

        ''' <summary>
        ''' Checks and searchs neighbor points for given point
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="point">centered point to be searched neighbors</param>
        ''' <param name="epsilon">radius of center point</param>
        ''' <returns>result neighbors</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function RegionQuery(allPoints As DbscanPoint(Of T)(), point As T, epsilon As Double) As DbscanPoint(Of T)()
            Return allPoints _
                .AsParallel _
                .Where(Function(x) _metricFunc(point, x.ClusterPoint) <= epsilon) _
                .ToArray()
        End Function
    End Class
End Namespace

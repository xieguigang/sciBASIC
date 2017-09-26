#Region "Microsoft.VisualBasic::d470f62e78fe7930cb5faf08c3ba4eae, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\DBSCAN\DbscanAlgorithm.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.InteropServices

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
    Public Class DbscanAlgorithm(Of T As DatasetItemBase)

        ReadOnly _metricFunc As Func(Of T, T, Double)

        ''' <summary>
        ''' Takes metric function to compute distances between dataset items T
        ''' </summary>
        ''' <param name="metricFunc"></param>
        Public Sub New(metricFunc As Func(Of T, T, Double))
            _metricFunc = metricFunc
        End Sub

        ''' <summary>
        ''' Performs the DBSCAN clustering algorithm.
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="epsilon">Desired region ball radius</param>
        ''' <param name="minPts">Minimum number of points to be in a region</param>
        ''' <param name="clusters">returns sets of clusters, renew the parameter</param>
        Public Sub ComputeClusterDbscan(allPoints As T(), epsilon As Double, minPts As Integer, <Out()> ByRef clusters As HashSet(Of T()))
            Dim allPointsDbscan As DbscanPoint(Of T)() = allPoints.[Select](Function(x) New DbscanPoint(Of T)(x)).ToArray()
            Dim clusterId As Integer = 0
            For i As Integer = 0 To allPointsDbscan.Length - 1
                Dim p As DbscanPoint(Of T) = allPointsDbscan(i)
                If p.IsVisited Then
                    Continue For
                End If
                p.IsVisited = True

                Dim neighborPts As DbscanPoint(Of T)() = Nothing
                RegionQuery(allPointsDbscan, p.ClusterPoint, epsilon, neighborPts)
                If neighborPts.Length < minPts Then
                    p.ClusterId = CInt(ClusterIds.Noise)
                Else
                    clusterId += 1
                    ExpandCluster(allPointsDbscan, p, neighborPts, clusterId, epsilon, minPts)
                End If
            Next
            clusters = New HashSet(Of T())(allPointsDbscan.Where(Function(x) x.ClusterId > 0).GroupBy(Function(x) x.ClusterId).[Select](Function(x) x.[Select](Function(y) y.ClusterPoint).ToArray()))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="allPoints">Dataset</param>
        ''' <param name="point">point to be in a cluster</param>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        ''' <param name="epsilon">Desired region ball range</param>
        ''' <param name="minPts">Minimum number of points to be in a region</param>
        Private Sub ExpandCluster(allPoints As DbscanPoint(Of T)(), point As DbscanPoint(Of T), neighborPts As DbscanPoint(Of T)(), clusterId As Integer, epsilon As Double, minPts As Integer)
            point.ClusterId = clusterId
            For i As Integer = 0 To neighborPts.Length - 1
                Dim pn As DbscanPoint(Of T) = neighborPts(i)
                If Not pn.IsVisited Then
                    pn.IsVisited = True
                    Dim neighborPts2 As DbscanPoint(Of T)() = Nothing
                    RegionQuery(allPoints, pn.ClusterPoint, epsilon, neighborPts2)
                    If neighborPts2.Length >= minPts Then
                        neighborPts = neighborPts.Union(neighborPts2).ToArray()
                    End If
                End If
                If pn.ClusterId = CInt(ClusterIds.Unclassified) Then
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
        ''' <param name="neighborPts">result neighbors</param>
        Private Sub RegionQuery(allPoints As DbscanPoint(Of T)(), point As T, epsilon As Double, ByRef neighborPts As DbscanPoint(Of T)())
            neighborPts = allPoints.Where(Function(x) _metricFunc(point, x.ClusterPoint) <= epsilon).ToArray()
        End Sub
    End Class
End Namespace

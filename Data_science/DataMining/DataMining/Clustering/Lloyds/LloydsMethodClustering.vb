#Region "Microsoft.VisualBasic::e55debf2bc7f254b12d3da7bcab5f482, Data_science\DataMining\DataMining\Clustering\Lloyds\LloydsMethodClustering.vb"

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

    '   Total Lines: 108
    '    Code Lines: 74
    ' Comment Lines: 11
    '   Blank Lines: 23
    '     File Size: 4.20 KB


    '     Class LloydsMethodClustering
    ' 
    '         Properties: Points
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Loop], Clustering
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Math.Correlations
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Lloyds

    ''' <summary>
    ''' Voronoi
    ''' </summary>
    Public Class LloydsMethodClustering : Inherits Clustering

        Friend _lloydsPoints As List(Of Point)
        Friend _kmeansClusters As List(Of KMeansCluster(Of Point))
        Friend _minKMeansClusters As List(Of KMeansCluster(Of Point))

        Public Overrides ReadOnly Property Points As List(Of Point)
            Get
                Return _lloydsPoints
            End Get
        End Property

        Public Sub New(source As IEnumerable(Of Point), k As Integer)
            Call MyBase.New(source, k)

            _kmeansClusters = New List(Of KMeansCluster(Of Point))
            _lloydsPoints = New List(Of Point)(_source)
        End Sub

        Public Overrides Function Clustering() As List(Of Point)
            Dim minKMeansCost As Double = Double.MaxValue

            For runningTimes As Integer = 0 To 99
                ' for grabbing random points from input file
                If _kmeansClusters.Count > 0 Then
                    _kmeansClusters.Clear()
                End If

                Dim currentKMeansCost As Double = [Loop]()

                If currentKMeansCost < minKMeansCost Then
                    minKMeansCost = currentKMeansCost
                    _minKMeansClusters = New List(Of KMeansCluster(Of Point))(_kmeansClusters)
                End If
            Next

            Return Points
        End Function

        Private Function [Loop]() As Double
            Dim oldKmeansCost As Double = 0
            Dim currentKMeansCost As Double = 0
            Dim randomIndex As Integer

            ' First, create number of desired clusters
            ' Pick K (mNumDesiredRandomPoints)
            For i As Integer = 0 To mNumDesiredClusters - 1
                _kmeansClusters.Add(New KMeansCluster(Of Point))
                randomIndex = randf.Next(_source.Count)
                _kmeansClusters(i).Center = _source(randomIndex)
            Next

            ' Until convergence:
            Do
                For i As Integer = 0 To _kmeansClusters.Count - 1
                    _kmeansClusters(i).refresh()
                Next

                oldKmeansCost = currentKMeansCost

                Dim closestClusterToPoint As KMeansCluster(Of Point) = Nothing
                Dim minClusterIndex As Integer = Integer.MaxValue

                ' find the closest cluster to each point
                For i As Integer = 0 To _lloydsPoints.Count - 1
                    Dim minDistanceToCluster As Double = Double.MaxValue
                    For j As Integer = 0 To _kmeansClusters.Count - 1
                        Dim distanceToCluster As Double = _lloydsPoints(i).DistanceTo(_kmeansClusters(j).Center)

                        If distanceToCluster < minDistanceToCluster Then
                            closestClusterToPoint = _kmeansClusters(j)
                            minDistanceToCluster = distanceToCluster
                            minClusterIndex = j
                        End If
                    Next

                    ' Indexing by one with naming, so add one
                    _lloydsPoints(i).SetKMeansCluster(minClusterIndex + 1)
                    _kmeansClusters(minClusterIndex).Add(_lloydsPoints(i))
                Next

                ' reset kmeans cost
                currentKMeansCost = 0

                ' calculate the new center
                For i As Integer = 0 To _kmeansClusters.Count - 1
                    _kmeansClusters(i).Center = New Point With {
                        .entityVector = _kmeansClusters(i).CalculateClusterMean
                    }
                    currentKMeansCost += _kmeansClusters(i).CalculateKMeansCost()
                Next
            Loop While std.Abs(oldKmeansCost - currentKMeansCost) > 1

            Return currentKMeansCost
        End Function
    End Class

End Namespace

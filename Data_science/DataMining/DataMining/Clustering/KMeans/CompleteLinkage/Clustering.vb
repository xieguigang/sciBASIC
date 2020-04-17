#Region "Microsoft.VisualBasic::6d42023f1d6515710acda3073db81fd3, Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\Clustering.vb"

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

'     Class Clustering
' 
'         Properties: Points
' 
'         Constructor: (+1 Overloads) Sub New
'         Sub: __writeCluster
' 
'     Class LloydsMethodClustering
' 
'         Properties: Points
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Clustering
' 
'     Class CompleteLinkageClustering
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Clustering
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace KMeans.CompleteLinkage

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/halfjew22/Clustering/blob/master/src/com/lustig/model/Clustering.java
    ''' </remarks>
    Public MustInherit Class Clustering

        Friend _source As List(Of Point)
        Friend mNumDesiredClusters As Integer

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            _source = source.AsList
            mNumDesiredClusters = numClusters
        End Sub

        Public MustOverride Function Clustering() As List(Of Point)

        Public Overridable ReadOnly Property Points As List(Of Point)
            Get
                Return _source
            End Get
        End Property

        Protected Shared Sub __writeCluster(source As IEnumerable(Of Cluster(Of Point)))
            For Each c In source.SeqIterator
                For Each x As Point In c.value._innerList
                    Call x.CompleteLinkageCluster(c.i)
                Next
            Next
        End Sub
    End Class

    Public Class LloydsMethodClustering : Inherits Clustering

        Friend _lloydsPoints As List(Of Point)
        Friend mKMeansClusters As List(Of KMeansCluster(Of Point))
        Friend mMinKMeansClusters As List(Of KMeansCluster(Of Point))

        Public Overrides ReadOnly Property Points As List(Of Point)
            Get
                Return _lloydsPoints
            End Get
        End Property

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            Call MyBase.New(source, numClusters)

            mKMeansClusters = New List(Of KMeansCluster(Of Point))
            _lloydsPoints = New List(Of Point)(_source)
        End Sub

        Public Overrides Function Clustering() As List(Of Point)
            Dim minKMeansCost As Double = Double.MaxValue

            For runningTimes As Integer = 0 To 99
                ' for grabbing random points from input file
                Dim random As New Random

                If mKMeansClusters.Count > 0 Then mKMeansClusters.Clear()

                ' First, create number of desired clusters
                ' Pick K (mNumDesiredRandomPoints)
                For i As Integer = 0 To mNumDesiredClusters - 1
                    mKMeansClusters.Add(New KMeansCluster(Of Point))
                    Dim randomIndex As Integer = random.Next(_source.Count)
                    mKMeansClusters(i).Center = _source(randomIndex)
                Next

                Dim oldKmeansCost As Double = 0
                Dim currentKMeansCost As Double = 0

                ' Until convergence:
                Do
                    For i As Integer = 0 To mKMeansClusters.Count - 1
                        mKMeansClusters(i).refresh()
                    Next
                    oldKmeansCost = currentKMeansCost

                    Dim closestClusterToPoint As KMeansCluster(Of Point) = Nothing

                    Dim minClusterIndex As Integer = Integer.MaxValue

                    ' find the closest cluster to each point
                    For i As Integer = 0 To _lloydsPoints.Count - 1
                        Dim minDistanceToCluster As Double = Double.MaxValue
                        For j As Integer = 0 To mKMeansClusters.Count - 1

                            Dim distanceToCluster As Double = _lloydsPoints(i).distanceToOtherPoint(mKMeansClusters(j).Center)

                            If distanceToCluster < minDistanceToCluster Then
                                closestClusterToPoint = mKMeansClusters(j)
                                minDistanceToCluster = distanceToCluster
                                minClusterIndex = j
                            End If
                        Next

                        ' Indexing by one with naming, so add one
                        _lloydsPoints(i).SetKMeansCluster(minClusterIndex + 1)

                        mKMeansClusters(minClusterIndex).Add(_lloydsPoints(i))
                    Next

                    ' reset kmeans cost
                    currentKMeansCost = 0

                    ' calculate the new center
                    For i As Integer = 0 To mKMeansClusters.Count - 1
                        mKMeansClusters(i).Center = mKMeansClusters(i).CalculateCenter()
                        currentKMeansCost += mKMeansClusters(i).CalculateKMeansCost()
                    Next i
                Loop While stdNum.Abs(oldKmeansCost - currentKMeansCost) > 1

                If currentKMeansCost < minKMeansCost Then
                    minKMeansCost = currentKMeansCost
                    mMinKMeansClusters = New List(Of KMeansCluster(Of Point))(mKMeansClusters)
                End If
            Next

            Return Points
        End Function
    End Class

    Public Class CompleteLinkageClustering : Inherits Clustering

        Friend _completeLinkageClusters As List(Of Cluster(Of Point))

        Public Sub New(source As IEnumerable(Of Point), numClusters As Integer)
            Call MyBase.New(source, numClusters)
            _completeLinkageClusters = New List(Of Cluster(Of Point))
        End Sub

        Public Overrides Function Clustering() As List(Of Point)
            ' Start by placing each point in its own cluster
            For Each p As Point In _source
                _completeLinkageClusters.Add(New Cluster(Of Point)(p))
            Next p

            Dim minDistance As Double = Double.MaxValue
            Dim currentDistance As Double

            ' Array to hold pair of closest clusters
            Dim twoClosestClusters As Cluster(Of Point)() = New Cluster(Of Point)(1) {}

            ' while there are more than k clusters
            Do While _completeLinkageClusters.Count > mNumDesiredClusters

                ' Calculate and store the distance between each pair of clusters, finding the minimum distance between clusters
                For i As Integer = 0 To _completeLinkageClusters.Count - 1
                    For j As Integer = i + 1 To _completeLinkageClusters.Count - 1
                        Dim c1 As Cluster(Of Point) = _completeLinkageClusters(i)
                        Dim c2 As Cluster(Of Point) = _completeLinkageClusters(j)
                        currentDistance = ClusterAPI.completeLinkageDistance(c1, c2)
                        If currentDistance < minDistance Then
                            twoClosestClusters(0) = c1
                            twoClosestClusters(1) = c2
                            minDistance = currentDistance
                        End If
                    Next
                Next

                Dim mergedCluster As Cluster(Of Point) = ClusterAPI.mergeClusters(twoClosestClusters(0), twoClosestClusters(1))
                _completeLinkageClusters.Add(mergedCluster)
                _completeLinkageClusters.Remove(twoClosestClusters(0))
                _completeLinkageClusters.Remove(twoClosestClusters(1))

                ' reset min distance so Array is overwritten
                minDistance = Double.MaxValue

                ' just to be safe, nullify array as well
                twoClosestClusters(0) = Nothing
                twoClosestClusters(1) = Nothing
            Loop

            Call __writeCluster(_completeLinkageClusters)

            Return Points
        End Function
    End Class
End Namespace

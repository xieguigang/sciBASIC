#Region "Microsoft.VisualBasic::6aeb10d74871ccf8b42d2a9ebc993c12, sciBASIC#\Data_science\DataMining\DataMining\Clustering\KMeans\CompleteLinkage\CompleteLinkageClustering.vb"

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

    '   Total Lines: 58
    '    Code Lines: 40
    ' Comment Lines: 6
    '   Blank Lines: 12
    '     File Size: 2.66 KB


    '     Class CompleteLinkageClustering
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clustering
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace KMeans.CompleteLinkage
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

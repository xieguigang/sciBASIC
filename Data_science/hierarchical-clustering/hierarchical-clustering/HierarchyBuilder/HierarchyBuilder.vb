Imports System
Imports System.Collections.Generic

'
'*****************************************************************************
' Copyright 2013 Lars Behnke
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'

Namespace Hierarchy

    Public Class HierarchyBuilder

        Public ReadOnly Property Distances As DistanceMap
        Public ReadOnly Property Clusters As IList(Of Cluster)

        Public ReadOnly Property TreeComplete As Boolean
            Get
                Return Clusters.Count = 1
            End Get
        End Property

        Public ReadOnly Property RootCluster As Cluster
            Get
                If Not TreeComplete Then Throw New Exception("No root available")
                Return Clusters(0)
            End Get
        End Property

        Public Sub New(clusters As IList(Of Cluster), distances As DistanceMap)
            Me.Clusters = clusters
            Me.Distances = distances
        End Sub

        ''' <summary>
        ''' Returns Flattened clusters, i.e. clusters that are at least apart by a given threshold </summary>
        ''' <param name="linkageStrategy"> </param>
        ''' <param name="threshold">
        ''' @return </param>
        Public Function flatAgg(linkageStrategy As LinkageStrategy, threshold As Double) As IList(Of Cluster)
            Do While ((Not TreeComplete)) AndAlso (Distances.minDist() <= threshold)
                'System.out.println("Cluster Distances: " + distances.toString());
                'System.out.println("Cluster Size: " + clusters.size());
                agglomerate(linkageStrategy)
            Loop

            'System.out.println("Final MinDistance: " + distances.minDist());
            'System.out.println("Tree complete: " + isTreeComplete());
            Return Clusters
        End Function

        Public Sub agglomerate(linkageStrategy As LinkageStrategy)
            Dim minDistLink As HierarchyTreeNode = Distances.removeFirst()
            If minDistLink IsNot Nothing Then
                Clusters.Remove(minDistLink.rCluster())
                Clusters.Remove(minDistLink.lCluster())

                Dim oldClusterL As Cluster = minDistLink.lCluster()
                Dim oldClusterR As Cluster = minDistLink.rCluster()
                Dim newCluster As Cluster = minDistLink.Agglomerate(Nothing)

                For Each iClust As Cluster In Clusters
                    Dim link1 As HierarchyTreeNode = findByClusters(iClust, oldClusterL)
                    Dim link2 As HierarchyTreeNode = findByClusters(iClust, oldClusterR)
                    Dim newLinkage As New HierarchyTreeNode
                    newLinkage.lCluster = iClust
                    newLinkage.rCluster = newCluster
                    Dim distanceValues As ICollection(Of Distance) = New List(Of Distance)

                    If link1 IsNot Nothing Then
                        Dim distVal As Double = link1.LinkageDistance
                        Dim weightVal As Double = link1.getOtherCluster(iClust).WeightValue
                        distanceValues.Add(New Distance(distVal, weightVal))
                        Distances.remove(link1)
                    End If
                    If link2 IsNot Nothing Then
                        Dim distVal As Double = link2.LinkageDistance
                        Dim weightVal As Double = link2.getOtherCluster(iClust).WeightValue
                        distanceValues.Add(New Distance(distVal, weightVal))
                        Distances.remove(link2)
                    End If

                    Dim newDistance As Distance = linkageStrategy.CalculateDistance(distanceValues)

                    newLinkage.LinkageDistance = newDistance.Distance
                    Distances.add(newLinkage)

                Next iClust
                Clusters.Add(newCluster)
            End If
        End Sub

        Private Function findByClusters(c1 As Cluster, c2 As Cluster) As HierarchyTreeNode
            Return Distances.findByCodePair(c1, c2)
        End Function
    End Class

End Namespace
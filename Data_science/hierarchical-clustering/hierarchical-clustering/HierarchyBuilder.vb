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

Namespace com.apporiented.algorithm.clustering


	Public Class HierarchyBuilder

        Public Overridable ReadOnly Property Distances As DistanceMap
        Public Overridable ReadOnly Property Clusters As IList(Of Cluster)


        Public Sub New( clusters As IList(Of Cluster), distances As DistanceMap)
			Me.clusters = clusters
			Me.distances = distances
		End Sub

		''' <summary>
		''' Returns Flattened clusters, i.e. clusters that are at least apart by a given threshold </summary>
		''' <param name="linkageStrategy"> </param>
		''' <param name="threshold">
		''' @return </param>
		Public Overridable Function flatAgg( linkageStrategy As LinkageStrategy, threshold As Double ) As IList(Of Cluster)
            Do While ((Not TreeComplete)) AndAlso (Distances.minDist() <= threshold)
                'System.out.println("Cluster Distances: " + distances.toString());
                'System.out.println("Cluster Size: " + clusters.size());
                agglomerate(linkageStrategy)
            Loop

            'System.out.println("Final MinDistance: " + distances.minDist());
            'System.out.println("Tree complete: " + isTreeComplete());
            Return clusters
		End Function

		Public Overridable Sub agglomerate( linkageStrategy As LinkageStrategy)
			Dim minDistLink As ClusterPair = distances.removeFirst()
			If minDistLink IsNot Nothing Then
				clusters.Remove(minDistLink.getrCluster())
				clusters.Remove(minDistLink.getlCluster())

				Dim oldClusterL As Cluster = minDistLink.getlCluster()
				Dim oldClusterR As Cluster = minDistLink.getrCluster()
				Dim newCluster As Cluster = minDistLink.agglomerate(Nothing)

				For Each iClust As Cluster In clusters
					Dim link1 As ClusterPair = findByClusters(iClust, oldClusterL)
					Dim link2 As ClusterPair = findByClusters(iClust, oldClusterR)
					Dim newLinkage As New ClusterPair
					newLinkage.setlCluster(iClust)
					newLinkage.setrCluster(newCluster)
					Dim distanceValues As ICollection(Of Distance) = New List(Of Distance)

					If link1 IsNot Nothing Then
						Dim distVal As Double  = link1.LinkageDistance
						Dim weightVal As Double  = link1.getOtherCluster(iClust).WeightValue
						distanceValues.Add(New Distance(distVal, weightVal))
						distances.remove(link1)
					End If
					If link2 IsNot Nothing Then
						Dim distVal As Double  = link2.LinkageDistance
						Dim weightVal As Double  = link2.getOtherCluster(iClust).WeightValue
						distanceValues.Add(New Distance(distVal, weightVal))
						distances.remove(link2)
					End If

					Dim newDistance As Distance = linkageStrategy.calculateDistance(distanceValues)

					newLinkage.LinkageDistance = newDistance.Distance
					distances.add(newLinkage)

				Next iClust
				clusters.Add(newCluster)
			End If
		End Sub

		Private Function findByClusters( c1 As Cluster, c2 As Cluster) As ClusterPair
			Return distances.findByCodePair(c1, c2)
		End Function

        Public Overridable ReadOnly Property TreeComplete As Boolean
            Get
                Return Clusters.Count = 1
            End Get
        End Property

        Public Overridable ReadOnly Property RootCluster As Cluster
            Get
                If Not TreeComplete Then Throw New Exception("No root available")
                Return Clusters(0)
            End Get
        End Property

    End Class

End Namespace
Imports System
Imports System.Text

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

	Public Class ClusterPair
		Implements IComparable(Of ClusterPair)

		Private Shared globalIndex As Long = 0

		Private lCluster As Cluster
		Private rCluster As Cluster

        Public Sub New()
		End Sub

        Public Sub New( left As Cluster, right As Cluster, distance As Double)
            lCluster = left
            rCluster = right
            LinkageDistance = distance
        End Sub

        Public Overridable Function getOtherCluster( c As Cluster) As Cluster
            Return If(lCluster Is c, rCluster, lCluster)
        End Function

        Public Overridable Function getlCluster() As Cluster
            Return lCluster
        End Function

        Public Overridable Sub setlCluster( lCluster As Cluster)
            Me.lCluster = lCluster
        End Sub

        Public Overridable Function getrCluster() As Cluster
            Return rCluster
        End Function

        Public Overridable Sub setrCluster( rCluster As Cluster)
            Me.rCluster = rCluster
        End Sub

        Public Overridable Property LinkageDistance As Double

        ''' <returns> a new ClusterPair with the two left/right inverted </returns>
        Public Overridable Function reverse() As ClusterPair
            Return New ClusterPair(getrCluster(), getlCluster(), LinkageDistance)
        End Function



        Public Function compareTo( o As ClusterPair) As Integer Implements IComparable(Of ClusterPair).CompareTo
            Dim result As Integer
            If o Is Nothing OrElse o.LinkageDistance = 0 Then
                result = -1
            ElseIf LinkageDistance = 0 Then
                result = 1
            Else
                result = LinkageDistance.CompareTo(o.LinkageDistance)
            End If

            Return result
        End Function


        Public Overridable Function agglomerate( name As String) As Cluster
            If name Is Nothing Then
                globalIndex += 1
                name = "clstr#" & (globalIndex)

                '            
                '            StringBuilder sb = new StringBuilder();
                '            if (lCluster != null) {
                '                sb.append(lCluster.getName());
                '            }
                '            if (rCluster != null) {
                '                if (sb.length() > 0) {
                '                    sb.append("&");
                '                }
                '                sb.append(rCluster.getName());
                '            }
                '            name = sb.toString();
                '            
            End If
            Dim cluster As New Cluster(name)
            cluster.Distance = New Distance(LinkageDistance)
            'New clusters will track their children's leaf names; i.e. each cluster knows what part of the original data it contains
            cluster.appendLeafNames(lCluster.LeafNames)
            cluster.appendLeafNames(rCluster.LeafNames)
            cluster.addChild(lCluster)
            cluster.addChild(rCluster)
            lCluster.Parent = cluster
            rCluster.Parent = cluster

            Dim lWeight As Double = lCluster.WeightValue
            Dim rWeight As Double = rCluster.WeightValue
            Dim weight As Double = lWeight + rWeight
            cluster.Distance.Weight = weight

            Return cluster
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder
            If lCluster IsNot Nothing Then sb.Append(lCluster.Name)
            If rCluster IsNot Nothing Then
                If sb.Length > 0 Then sb.Append(" + ")
                sb.Append(rCluster.Name)
            End If
            sb.Append(" : ").Append(LinkageDistance)
            Return sb.ToString()
        End Function

    End Class

End Namespace
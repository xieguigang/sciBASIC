Imports System
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

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

    Public Class HierarchyTreeNode : Implements IComparable(Of HierarchyTreeNode)

        Private Shared globalIndex As Long = 0

        Public Sub New()
        End Sub

        Public Sub New(left As Cluster, right As Cluster, distance As Double)
            lCluster = left
            rCluster = right
            LinkageDistance = distance
        End Sub

        Public Function GetOtherCluster(c As Cluster) As Cluster
            Return If(lCluster Is c, rCluster, lCluster)
        End Function

        Public Property lCluster As Cluster
        Public Property rCluster As Cluster
        Public Property LinkageDistance As Double

        ''' <returns> 
        ''' a new ClusterPair with the two left/right inverted
        ''' </returns>
        Public Function Reverse() As HierarchyTreeNode
            Return New HierarchyTreeNode(rCluster(), lCluster(), LinkageDistance)
        End Function

        Public Function compareTo(o As HierarchyTreeNode) As Integer Implements IComparable(Of HierarchyTreeNode).CompareTo
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

        Public Function Agglomerate(name As String) As Cluster
            If name Is Nothing Then
                globalIndex += 1
                name = "clstr#" & (globalIndex)
            End If

            Dim cluster As New Cluster(name) With {
            .Distance = New Distance(LinkageDistance)
        }
            ' New clusters will track their children's leaf names; i.e. each cluster knows what part of the original data it contains
            cluster.AppendLeafNames(lCluster.LeafNames)
            cluster.AppendLeafNames(rCluster.LeafNames)
            cluster.AddChild(lCluster)
            cluster.AddChild(rCluster)
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
                If sb.Length > 0 Then
                    sb.Append(" + ")
                End If
                sb.Append(rCluster.Name)
            End If
            sb.Append(" : ").Append(LinkageDistance)
            Return sb.ToString()
        End Function
    End Class
End Namespace
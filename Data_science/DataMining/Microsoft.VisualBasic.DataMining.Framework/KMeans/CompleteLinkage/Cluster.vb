#Region "Microsoft.VisualBasic::23ffcde17117b0a595d56129e5df025b, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\KMeans\CompleteLinkage\Cluster.vb"

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

Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.ComponentModel

Namespace KMeans.CompleteLinkage

    Public Class Cluster(Of T As EntityBase(Of Double))

        Protected Friend ReadOnly _innerList As New List(Of T)

        Public Sub New(points As List(Of T))
            _innerList = points
        End Sub

        Public Sub New()
            _innerList = New List(Of T)
        End Sub

        Public Sub New(p As T)
            _innerList = New List(Of T)
            Call Add(p)
        End Sub

        Public Overridable Sub Add(p As T)
            Call _innerList.Add(p)
        End Sub
    End Class

    Public Module ClusterAPI

        Public Function completeLinkageDistance(Of T As Point)(c1 As Cluster(Of T), c2 As Cluster(Of T)) As Double

            Dim points1 As List(Of T) = c1._innerList
            Dim points2 As List(Of T) = c2._innerList

            Dim numPointsInC1 As Integer = points1.Count
            Dim numPointsInC2 As Integer = points2.Count

            Dim maxDistance As Double = Double.MinValue

            For i1 As Integer = 0 To numPointsInC1 - 1
                For i2 As Integer = 0 To numPointsInC2 - 1
                    maxDistance = Math.Max(points1(i1).distanceToOtherPoint(points2(i2)), maxDistance)
                Next i2
            Next i1

            Return maxDistance
        End Function

        Public Function mergeClusters(Of T As Point)(c1 As Cluster(Of T), c2 As Cluster(Of T)) As Cluster(Of T)
            Dim mergedCluster As New Cluster(Of T)(New List(Of T))
            Dim pointsC1 As List(Of T) = c1._innerList

            For i As Integer = 0 To pointsC1.Count - 1
                mergedCluster.Add(pointsC1(i))
            Next i

            Dim pointsC2 As List(Of T) = c2._innerList

            For i As Integer = 0 To pointsC2.Count - 1
                mergedCluster.Add(pointsC2(i))
            Next i

            Return mergedCluster
        End Function

        Public Function HammingDistance(completeLinkageClusteredPoints As List(Of Point), lloydsMethodClusteredPoints As List(Of Point)) As Double
            If completeLinkageClusteredPoints.Count <> lloydsMethodClusteredPoints.Count Then
                Throw New ArgumentException("Lists of different sizes cannot be passed here")
            End If

            Dim numDisagreements As Integer = 0
            Dim ___hammingDistance As Double

            ' variables to hold whether or not a pair of points is in the
            ' same cluster as grouped by complete linkage and lloyds method
            Dim sameClusterByCompleteLinkage As Boolean = False
            Dim sameClusterByLloyds As Boolean = False

            Dim n As Double = completeLinkageClusteredPoints.Count

            ' Loop over both lists and find clustering disagreements
            ' I don't think you have to loop through all elements, maybe only half,
            ' but I'll just be redundant to make sure
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1

                    sameClusterByCompleteLinkage =
                        completeLinkageClusteredPoints(i).CompleteLinkageResultCluster = completeLinkageClusteredPoints(j).CompleteLinkageResultCluster
                    sameClusterByLloyds =
                        lloydsMethodClusteredPoints(i).LloydsResultCluster = lloydsMethodClusteredPoints(j).LloydsResultCluster

                    ' Two points have different clusters, increment disagreement count
                    If sameClusterByCompleteLinkage <> sameClusterByLloyds Then
                        numDisagreements += 1
                    End If
                Next
            Next

            Dim nChoose2 As Double = 0.5 * (n - 1) * n
            ___hammingDistance = numDisagreements / nChoose2
            Return ___hammingDistance
        End Function

        <Extension>
        Public Function DistanceBetweenPoints(Of T As EntityBase(Of Double))(c1 As T, c2 As T) As Double
            If c1.Length <> c2.Length Then
                Throw New ArgumentException("Dimension sizes MUST be equivalent")
            End If

            Dim distanceSquared As Double = 0

            ' At this point, we're guaranteed to have identical dimensions
            Dim ___dimension As Integer = c1.Length
            Dim c1Coordinates As Double() = c1.Properties
            Dim c2Coordinates As Double() = c2.Properties

            For dimenIndex As Integer = 0 To ___dimension - 1
                distanceSquared += Math.Pow(((c1Coordinates(dimenIndex)) - c2Coordinates(dimenIndex)), 2)
            Next

            Return Math.Sqrt(distanceSquared)
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::96fe311aef4364c9bd7f386cfd146dc0, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Algorithm\FuzzyCMeans.vb"

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

    '     Class FuzzyCMeans
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CMeansLoop, RunCMeans
    ' 
    '         Sub: addTrace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace FuzzyCMeans

    Friend Class FuzzyCMeans

        Dim coordinates As List(Of FuzzyCMeansEntity)
        Dim clusterCenters As List(Of FuzzyCMeansEntity)
        Dim threshold As Double
        Dim membershipMatrix As New MembershipMatrix
        Dim fuzzificationParameter As Double

        Friend trace As Dictionary(Of Integer, FuzzyCMeansEntity())

        Sub New(data As IEnumerable(Of FuzzyCMeansEntity), numberOfClusters%, thresholdVal#, fuzzificationParam As Double)
            coordinates = New List(Of FuzzyCMeansEntity)(data)
            clusterCenters = AlgorithmsUtils.MakeInitialSeeds(coordinates, numberOfClusters)
            threshold = thresholdVal
            fuzzificationParameter = fuzzificationParam
        End Sub

        Private Sub addTrace(iteration As Integer, centers As IEnumerable(Of FuzzyCMeansEntity))
            If Not trace Is Nothing Then
                trace.Add(iteration, centers.ToArray)
            End If
        End Sub

        Public Function RunCMeans(maxIterates As Integer) As IEnumerable(Of FuzzyCMeansEntity)
            Dim iteration As i32 = 0

            While ++iteration <= maxIterates
                If CMeansLoop(iteration) Then
                    Exit While
                End If
            End While

            Return clusterCenters
        End Function

        Private Function CMeansLoop(iteration As i32) As Boolean
            Dim clusters As Dictionary(Of FuzzyCMeansEntity, FuzzyCMeansEntity) = MakeFuzzyClusters(coordinates, clusterCenters, fuzzificationParameter, membershipMatrix)

            For Each pair In membershipMatrix.GetMemberships
                For Each annotation As FuzzyCMeansEntity In coordinates
                    If VectorEqualityComparer.VectorEqualsToAnother(annotation.entityVector, pair.entity.entityVector) Then
                        Dim tooltip As New Dictionary(Of Integer, Double)

                        For i As Integer = 0 To pair.membership.Count - 1
                            Call tooltip.Add(i, stdNum.Round(pair.membership(i), 2))
                        Next

                        annotation.memberships = tooltip
                    End If
                Next
            Next

            Dim oldClusterCenters As List(Of FuzzyCMeansEntity) = clusterCenters

            clusterCenters = RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters, membershipMatrix, fuzzificationParameter)

            Call addTrace(iteration, clusterCenters)

            Dim distancesToClusterCenters As MembershipMatrix = membershipMatrix.DistanceToClusterCenters(coordinates, clusterCenters)
            Dim newMembershipMatrix As MembershipMatrix = membershipMatrix.CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)
            Dim differences As List(Of List(Of Double)) = newMembershipMatrix.GetMembershipMatrix.DifferenceMatrix(membershipMatrix.GetMembershipMatrix.ToArray())
            Dim maxElement As Double = differences.IteratesALL.Max

            Call $"Max element: {maxElement}".__DEBUG_ECHO

            If maxElement <= threshold Then
                Return True
            End If

            For Each oldClusterCenter As FuzzyCMeansEntity In oldClusterCenters
                Dim isClusterCenterDataPoint As Boolean = False

                For Each coordinate As FuzzyCMeansEntity In coordinates
                    If VectorEqualityComparer.VectorEqualsToAnother(oldClusterCenter.entityVector, coordinate.entityVector) Then
                        isClusterCenterDataPoint = True
                        Exit For
                    End If
                Next

                If Not isClusterCenterDataPoint Then
                    For Each annotation As FuzzyCMeansEntity In coordinates
                        If VectorEqualityComparer.VectorEqualsToAnother(annotation.entityVector, oldClusterCenter.entityVector) Then
                            coordinates.Remove(annotation)
                            Exit For
                        End If
                    Next
                End If
            Next

            For i As Integer = 0 To clusterCenters.Count - 1
                Dim clusterCenter As FuzzyCMeansEntity = clusterCenters(i)
                Dim isExists As Boolean = False

                For Each annotation As FuzzyCMeansEntity In coordinates
                    If VectorEqualityComparer.VectorEqualsToAnother(annotation.entityVector, clusterCenter.entityVector) Then
                        isExists = True
                        Exit For
                    End If
                Next

                If Not isExists Then
                    coordinates += New FuzzyCMeansEntity With {
                        .entityVector = clusterCenter.entityVector.Clone,
                        .uid = clusterCenter.uid
                    }
                End If
            Next

            Return False
        End Function
    End Class
End Namespace

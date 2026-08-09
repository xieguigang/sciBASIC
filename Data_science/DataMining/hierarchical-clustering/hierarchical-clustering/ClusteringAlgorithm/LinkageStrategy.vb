#Region "Microsoft.VisualBasic::53081fa964bc49c7c0daaf5b257f7b33, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\ClusteringAlgorithm\LinkageStrategy.vb"

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

    '   Total Lines: 256
    '    Code Lines: 101 (39.45%)
    ' Comment Lines: 126 (49.22%)
    '    - Xml Docs: 86.51%
    ' 
    '   Blank Lines: 29 (11.33%)
    '     File Size: 11.85 KB


    ' Interface LinkageStrategy
    ' 
    '     Function: (+2 Overloads) CalculateDistance
    ' 
    ' Class SingleLinkageStrategy
    ' 
    '     Function: (+2 Overloads) CalculateDistance
    ' 
    ' Class WeightedLinkageStrategy
    ' 
    '     Function: (+2 Overloads) CalculateDistance
    ' 
    ' Class CompleteLinkageStrategy
    ' 
    '     Function: (+2 Overloads) CalculateDistance
    ' 
    ' Class AverageLinkageStrategy
    ' 
    '     Function: (+2 Overloads) CalculateDistance
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy

'
'*****************************************************************************
' Copyright2013 Lars Behnke
' 
' Licensed under the Apache License, Version2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
' http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************
'
''' <summary>
''' Defines a strategy for calculating the distance between clusters in hierarchical clustering.
''' This is the core abstraction that allows different linkage criteria (single, complete, average, weighted)
''' to be plugged into the clustering algorithm.
''' </summary>
Public Interface LinkageStrategy

    ''' <summary>
    ''' Calculates the distance between two clusters based on a collection of pairwise distances
    ''' between the elements of the clusters.
    ''' </summary>
    ''' <param name="distances">A collection of <see cref="Distance"/>  objects representing the pairwise
    ''' distances between elements of the two clusters being merged.</param>
    ''' <returns>A <see cref="Distance"/>  object representing the computed linkage distance between
    ''' the two clusters.</returns>
    Function CalculateDistance(distances As ICollection(Of Distance)) As Distance

    ''' <summary>
    ''' Calculates a combined distance from two individual <see cref="Distance"/>  values according
    ''' to the specific linkage strategy.
    ''' </summary>
    ''' <param name="a">The first <see cref="Distance"/>  value to combine.</param>
    ''' <param name="b">The second <see cref="Distance"/>  value to combine.</param>
    ''' <returns>A <see cref="Double"/>  value representing the combined distance.</returns>
    Function CalculateDistance(a As Distance, b As Distance) As Double

End Interface

''' <summary>
''' Implements the single-linkage (nearest neighbor) clustering strategy.
''' The distance between two clusters is defined as the minimum distance between any
''' single element from the first cluster and any single element from the second cluster.
''' This strategy tends to produce long, chain-like clusters.
''' </summary>
Public Class SingleLinkageStrategy : Implements LinkageStrategy

    ''' <summary>
    ''' Calculates the single-linkage distance from a collection of pairwise distances
    ''' by selecting the minimum distance value.
    ''' </summary>
    ''' <param name="distances">A collection of <see cref="Distance"/>  objects containing the pairwise
    ''' distances between elements of two clusters.</param>
    ''' <returns>A <see cref="Distance"/>  object whose value is the smallest distance found
    ''' in the collection.</returns>
    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim min As Double = Double.MaxValue
        For Each dist As Distance In distances
            If dist.Distance < min Then
                min = dist.Distance
            End If
        Next

        Return New Distance(min)
    End Function

    ''' <summary>
    ''' Combines two distances using the single-linkage (minimum) criterion.
    ''' Returns the smaller of the two distance values.
    ''' </summary>
    ''' <param name="a">The first <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <param name="b">The second <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <returns>The minimum distance value between <paramref name="a"/>  and <paramref name="b"/> ;
    ''' returns <see cref="Double.MaxValue"/>  if both are <c>Nothing</c>.</returns>
    Public Function CalculateDistance(a As Distance, b As Distance) As Double Implements LinkageStrategy.CalculateDistance
        Dim min As Double = Double.MaxValue
        If a IsNot Nothing AndAlso a.Distance < min Then
            min = a.Distance
        End If

        If b IsNot Nothing AndAlso b.Distance < min Then
            min = b.Distance
        End If

        Return min
    End Function
End Class

''' <summary>
''' Implements the weighted-linkage clustering strategy.
''' The distance between two clusters is computed as the weighted average of the distances
''' between all inter-cluster element pairs, where each distance is weighted by its associated weight.
''' This strategy is also known as the "weighted pair-group method" (WPGMA).
''' </summary>
Public Class WeightedLinkageStrategy : Implements LinkageStrategy

    ''' <summary>
    ''' Calculates the weighted-linkage distance from a collection of pairwise distances
    ''' by computing the weighted arithmetic mean, where each distance is multiplied by its
    ''' corresponding weight.
    ''' </summary>
    ''' <param name="distances">A collection of <see cref="Distance"/>  objects containing the pairwise
    ''' distances and their associated weights.</param>
    ''' <returns>A <see cref="Distance"/>  object representing the weighted average distance; the
    ''' returned object also carries the total weight.</returns>
    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim sum As Double = 0
        Dim weightTotal As Double = 0
        For Each distance As Distance In distances
            weightTotal += distance.Weight
            sum += distance.Distance * distance.Weight
        Next

        Return New Distance(sum / weightTotal, weightTotal)
    End Function

    ''' <summary>
    ''' Combines two distances using the weighted-linkage criterion.
    ''' Returns the weighted arithmetic mean of the two distance values,
    ''' using each distance's weight as the weighting factor.
    ''' </summary>
    ''' <param name="a">The first <see cref="Distance"/>  value with its associated weight. May be <c>Nothing</c>.</param>
    ''' <param name="b">The second <see cref="Distance"/>  value with its associated weight. May be <c>Nothing</c>.</param>
    ''' <returns>The weighted average of the two distance values.</returns>
    Public Function CalculateDistance(a As Distance, b As Distance) As Double Implements LinkageStrategy.CalculateDistance
        Dim sum As Double = 0
        Dim weightTotal As Double = 0
        If Not a Is Nothing Then
            weightTotal += a.Weight
            sum += a.Distance * a.Weight
        End If

        If Not b Is Nothing Then
            weightTotal += b.Weight
            sum += b.Distance * b.Weight
        End If

        Return sum / weightTotal
    End Function
End Class

''' <summary>
''' Implements the complete-linkage (farthest neighbor) clustering strategy.
''' The distance between two clusters is defined as the maximum distance between any
''' single element from the first cluster and any single element from the second cluster.
''' This strategy tends to produce compact, spherical clusters.
''' </summary>
Public Class CompleteLinkageStrategy : Implements LinkageStrategy

    ''' <summary>
    ''' Calculates the complete-linkage distance from a collection of pairwise distances
    ''' by selecting the maximum distance value.
    ''' </summary>
    ''' <param name="distances">A collection of <see cref="Distance"/>  objects containing the pairwise
    ''' distances between elements of two clusters.</param>
    ''' <returns>A <see cref="Distance"/>  object whose value is the largest distance found
    ''' in the collection.</returns>
    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim max As Double = Double.MinValue
        For Each dist As Distance In distances
            If dist.Distance > max Then max = dist.Distance
        Next

        Return New Distance(max)
    End Function

    ''' <summary>
    ''' Combines two distances using the complete-linkage (maximum) criterion.
    ''' Returns the larger of the two distance values.
    ''' </summary>
    ''' <param name="a">The first <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <param name="b">The second <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <returns>The maximum distance value between <paramref name="a"/>  and <paramref name="b"/> ;
    ''' returns <see cref="Double.MinValue"/>  if both are <c>Nothing</c>.</returns>
    Public Function CalculateDistance(a As Distance, b As Distance) As Double Implements LinkageStrategy.CalculateDistance
        Dim max As Double = Double.MinValue
        If a IsNot Nothing AndAlso a.Distance > max Then
            max = a.Distance
        End If

        If b IsNot Nothing AndAlso b.Distance > max Then
            max = b.Distance
        End If

        Return max
    End Function
End Class

''' <summary>
''' Implements the average-linkage (unweighted pair-group method using arithmetic averages, UPGMA)
''' clustering strategy.
''' The distance between two clusters is defined as the arithmetic mean of all pairwise distances
''' between elements of the two clusters. This strategy represents a compromise between
''' single-linkage and complete-linkage.
''' </summary>
Public Class AverageLinkageStrategy : Implements LinkageStrategy

    ''' <summary>
    ''' Calculates the average-linkage distance from a collection of pairwise distances
    ''' by computing the unweighted arithmetic mean of all distance values.
    ''' </summary>
    ''' <param name="distances">A collection of <see cref="Distance"/>  objects containing the pairwise
    ''' distances between elements of two clusters.</param>
    ''' <returns>A <see cref="Distance"/>  object representing the arithmetic mean of the input distances;
    ''' returns zero if the collection is empty.</returns>
    Public Function CalculateDistance(distances As ICollection(Of Distance)) As Distance Implements LinkageStrategy.CalculateDistance
        Dim sum As Double = 0
        Dim result As Double
        For Each dist As Distance In distances
            sum += dist.Distance
        Next

        If distances.Count > 0 Then
            result = sum / distances.Count
        Else
            result = 0.0
        End If

        Return New Distance(result)
    End Function

    ''' <summary>
    ''' Combines two distances using the average-linkage criterion.
    ''' Returns the simple arithmetic mean of the two distance values.
    ''' </summary>
    ''' <param name="a">The first <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <param name="b">The second <see cref="Distance"/>  value. May be <c>Nothing</c>.</param>
    ''' <returns>The arithmetic mean of the two distance values; returns0 if both are <c>Nothing</c>.</returns>
    Public Function CalculateDistance(a As Distance, b As Distance) As Double Implements LinkageStrategy.CalculateDistance
        Dim sum As Double
        Dim n As Integer = 0
        If Not a Is Nothing Then
            n += 1
            sum += a.Distance
        End If

        If Not b Is Nothing Then
            n += 1
            sum += b.Distance
        End If

        If n > 0 Then
            Return sum / n
        Else
            Return0
        End If
    End Function
End Class

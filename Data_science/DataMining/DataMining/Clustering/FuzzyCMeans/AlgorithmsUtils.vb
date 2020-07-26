#Region "Microsoft.VisualBasic::1f7cdd555d5acf3389fb130bd353a060, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\AlgorithmsUtils.vb"

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

    '     Module AlgorithmsUtils
    ' 
    '         Function: DifferenceMatrix, DistanceToClusterCenters, GenerateDataPoints, GetElementIndex, GetMaxElement
    '                   MakeInitialSeeds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace FuzzyCMeans

    Public Module AlgorithmsUtils

        Public Function GetMaxElement(values As List(Of List(Of Double))) As Double
            Dim max As Double = Double.MinValue
            For i As Integer = 0 To values.Count - 1
                For j As Integer = 0 To values(0).Count - 1
                    If values(i)(j) > max Then
                        max = values(i)(j)
                    End If

                Next
            Next

            Return max
        End Function

        ''' <summary>
        ''' </summary>
        ''' <param name="matrix1"></param>
        ''' <param name="matrix2"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DifferenceMatrix(matrix1 As IEnumerable(Of List(Of Double)), matrix2 As List(Of List(Of Double))) As List(Of List(Of Double))
            Dim d As New List(Of List(Of Double))()
            Dim l% = matrix1.First.Count
            Dim line As List(Of Double)

            For Each row As SeqValue(Of List(Of Double)) In matrix1.SeqIterator
                Dim rowDifferences As New List(Of Double)()

                line = (+row)

                For j As Integer = 0 To l - 1
                    Dim result As Double = stdNum.Abs(line(j) - matrix2(row.i)(j))
                    rowDifferences.Add(result)
                Next

                d.Add(rowDifferences)
            Next

            Return d
        End Function

        Public Function GetElementIndex(list As List(Of List(Of Double)), element As List(Of Double)) As Integer
            For i As Integer = 0 To list.Count - 1
                If VectorEqualityComparer.VectorEqualsToAnother(list(i), element) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        <Extension>
        Public Function DistanceToClusterCenters(ls As List(Of FuzzyCMeansEntity), clusterCenters As List(Of FuzzyCMeansEntity)) As Dictionary(Of FuzzyCMeansEntity, List(Of Double))
            Dim map As New Dictionary(Of FuzzyCMeansEntity, List(Of Double))()

            For Each x As FuzzyCMeansEntity In ls
                Dim distancesToCenters As New List(Of Double)()

                For Each c As FuzzyCMeansEntity In clusterCenters
                    Dim distance As Double

                    For i As Integer = 0 To x.Length - 1
                        distance += stdNum.Pow(x(i) - c(i), 2)
                    Next

                    distance = stdNum.Sqrt(distance)
                    distancesToCenters.Add(distance)
                Next

                Call map.Add(x, distancesToCenters)
            Next

            Return map
        End Function

        Public Function GenerateDataPoints(dimension As Integer) As HashSet(Of Vector)
            Dim coordinates As New HashSet(Of Vector)(New VectorEqualityComparer())
            Dim random As New Random()

            While coordinates.Count < 50
                Dim list As New List(Of Double)()
                For j As Integer = 0 To dimension - 1
                    list.Add(random.[Next](0, 100))
                Next

                Dim b As Boolean = coordinates.Add(New Vector(list))
#Region "DEBUG"
#If DEBUG Then
                If Not b Then _
                    Call "Duplicate detected while generating data points".Warning
#End If
#End Region
            End While

            Return coordinates
        End Function

        Public Function MakeInitialSeeds(coordinates As List(Of FuzzyCMeansEntity), numberOfClusters As Integer) As List(Of FuzzyCMeansEntity)
            Dim random As New Random()
            Dim coordinatesCopy As List(Of FuzzyCMeansEntity) = coordinates.AsList()
            Dim initialClusterCenters As New List(Of FuzzyCMeansEntity)()
            For i As Integer = 0 To numberOfClusters - 1
                Dim clusterCenterPointNumber As Integer = random.[Next](0, coordinatesCopy.Count)
                initialClusterCenters.Add(coordinatesCopy(clusterCenterPointNumber))
                coordinatesCopy.RemoveAt(clusterCenterPointNumber)
            Next

            Return initialClusterCenters
        End Function
    End Module
End Namespace

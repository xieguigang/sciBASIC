#Region "Microsoft.VisualBasic::b540de8a717ff9fbd9b35c81052ae1a6, Data_science\DataMining\DataMining\Clustering\KMeans\Kmedoids.vb"

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

    '     Module Kmedoids
    ' 
    '         Function: doKMedoids, DoKMedoids
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace KMeans

    ''' <summary>
    ''' Partitioning around medoids(PAM)
    ''' </summary>
    Public Module Kmedoids

        ''' <summary>
        ''' Partitioning around medoids(PAM)
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="k"></param>
        ''' <param name="maxSteps"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DoKMedoids(source As IEnumerable(Of ClusterEntity), k As Integer, Optional maxSteps% = 1000) As ClusterEntity()
            Dim points = source.ToArray

            If k > points.Length OrElse k < 1 Then
                Throw New Exception("K must be between 0 and set size")
            Else
                Return doKMedoids(points, k, maxSteps)
            End If
        End Function

        Private Function doKMedoids(points As ClusterEntity(), k As Integer, Optional maxSteps% = 1000) As ClusterEntity()
            Dim medoids As ClusterEntity() = New ClusterEntity(k - 1) {}
            ' Dim resultClusterPoints As String() = New String(k - 1) {}
            Dim resultPoints As ClusterEntity() = New ClusterEntity(points.Length - 1) {}
            Dim stepmedoids As ClusterEntity() = New ClusterEntity(k - 1) {}
            Dim medoidsIndexes As New List(Of Integer)()
            Dim randIndex As Integer = 0

            ' initilizing random different medoids
            For i As Integer = 0 To k - 1
                randIndex = randf.NextInteger(points.Length)

                If Not medoidsIndexes.Contains(randIndex) Then
                    stepmedoids(i) = points(randIndex)
                    medoidsIndexes.Add(randIndex)
                Else
                    i -= 1
                End If
            Next

            Dim resultSumFunc As Double = Double.MaxValue
            Dim stepSumFunc As Double = 0
            Dim dist As Double = 0
            Dim minDist As Double
            Dim randomValue As Integer = 0

            For [step] As Integer = 0 To maxSteps
                ' initial clustering to medoids
                Dim clusterSumFunc As Double() = New Double(k - 1) {}
                ' Dim clusterPoints As String() = New String(k - 1) {}

                For i As Integer = 0 To points.Length - 1
                    minDist = Double.MaxValue
                    dist = 0

                    For c As Integer = 0 To k - 1
                        dist = points(i).entityVector.EuclideanDistance(stepmedoids(c).entityVector)

                        If dist < minDist Then
                            points(i).cluster = c
                            minDist = dist
                        End If
                    Next

                    ' getting sumFunc result for all clusters
                    clusterSumFunc(points(i).cluster) += minDist
                    stepSumFunc += minDist

                    'If clusterPoints(points(i).cluster) Is Nothing Then
                    '    clusterPoints(points(i).cluster) = ""
                    'End If

                    'clusterPoints(points(i).cluster) &= " " & points(i).ToString()
                Next

                ' if result of sumFinc is better than previous, save the configuration
                If stepSumFunc < resultSumFunc Then
                    resultSumFunc = stepSumFunc

                    For i As Integer = 0 To k - 1
                        medoids(i) = stepmedoids(i)
                        ' resultClusterPoints(i) = clusterPoints(i)
                    Next

                    Call Array.ConstrainedCopy(points, Scan0, resultPoints, Scan0, points.Length)
                End If

                stepSumFunc = 0

                ' random swapping medoids with nonmedoids
                Dim clusterSwapRandomCost As Integer() = New Integer(k - 1) {}
                Dim indexOfSwapCandidate As Integer() = New Integer(k - 1) {}

                For i As Integer = 0 To points.Length - 1
                    randomValue = randf.NextInteger(Integer.MaxValue)

                    If clusterSwapRandomCost(points(i).cluster) < randomValue AndAlso stepmedoids(points(i).cluster) IsNot points(i) Then
                        indexOfSwapCandidate(points(i).cluster) = i
                        clusterSwapRandomCost(points(i).cluster) = randomValue
                    End If
                Next

                For i As Integer = 0 To k - 1
                    stepmedoids(i) = points(indexOfSwapCandidate(i))
                Next
            Next

            Return resultPoints
        End Function
    End Module
End Namespace

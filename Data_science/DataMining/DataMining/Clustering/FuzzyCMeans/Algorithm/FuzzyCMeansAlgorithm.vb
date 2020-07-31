#Region "Microsoft.VisualBasic::8b6a1dbab239818337dfb44b47bcea7d, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Algorithm\FuzzyCMeansAlgorithm.vb"

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

    '     Module FuzzyCMeansAlgorithm
    ' 
    '         Function: FuzzyCMeans
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace FuzzyCMeans

    ''' <summary>
    ''' **Fuzzy clustering** (also referred to as **soft clustering**) is a form of clustering in which 
    ''' each data point can belong to more than one cluster.
    '''
    ''' Clustering Or cluster analysis involves assigning data points to clusters (also called buckets, 
    ''' bins, Or classes), Or homogeneous classes, such that items in the same class Or cluster are as 
    ''' similar as possible, while items belonging to different classes are as dissimilar as possible. 
    ''' Clusters are identified via similarity measures. These similarity measures include distance, 
    ''' connectivity, And intensity. Different similarity measures may be chosen based on the data Or 
    ''' the application.
    ''' 
    ''' > https://en.wikipedia.org/wiki/Fuzzy_clustering
    ''' </summary>
    ''' <remarks>
    ''' Clustering problems have applications in **biology**, medicine, psychology, economics, and many other disciplines.
    '''
    ''' ##### Bioinformatics
    ''' 
    ''' In the field of bioinformatics, clustering Is used for a number of applications. One use Is as 
    ''' a pattern recognition technique to analyze gene expression data from microarrays Or other 
    ''' technology. In this case, genes with similar expression patterns are grouped into the same cluster, 
    ''' And different clusters display distinct, well-separated patterns of expression. Use of clustering 
    ''' can provide insight into gene function And regulation. Because fuzzy clustering allows genes 
    ''' to belong to more than one cluster, it allows for the identification of genes that are conditionally 
    ''' co-regulated Or co-expressed. For example, one gene may be acted on by more than one Transcription 
    ''' factor, And one gene may encode a protein that has more than one function. Thus, fuzzy clustering 
    ''' Is more appropriate than hard clustering.
    ''' </remarks>
    Public Module FuzzyCMeansAlgorithm

        ''' <summary>
        ''' **Fuzzy clustering** (also referred to as **soft clustering**) is a form of clustering in which 
        ''' each data point can belong to more than one cluster.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="numberOfClusters%"></param>
        ''' <param name="fuzzificationParameter#">This parameter value should greater than **1.0**</param>
        ''' <param name="maxIterates%"></param>
        ''' <param name="threshold#"></param>
        ''' <param name="trace"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FuzzyCMeans(data As IEnumerable(Of FuzzyCMeansEntity),
                                    numberOfClusters%,
                                    Optional fuzzificationParameter# = 2,
                                    Optional maxIterates% = Short.MaxValue,
                                    Optional threshold# = 0.001,
                                    Optional ByRef trace As Dictionary(Of Integer, List(Of FuzzyCMeansEntity)) = Nothing) As FuzzyCMeansEntity()

            If Not trace Is Nothing Then
                Call trace.Clear()
            End If

            Return New FuzzyCMeans(data, numberOfClusters, threshold, fuzzificationParameter).RunCMeans(maxIterates).ToArray
        End Function
    End Module

    Friend Class FuzzyCMeans

        Dim coordinates As List(Of FuzzyCMeansEntity)
        Dim clusterCenters As List(Of FuzzyCMeansEntity)
        Dim trace As Dictionary(Of Integer, FuzzyCMeansEntity())
        Dim threshold As Double
        Dim membershipMatrix As New Dictionary(Of FuzzyCMeansEntity, List(Of Double))
        Dim fuzzificationParameter As Double

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
#If DEBUG Then
                Call $"Iteration = {iteration}".__DEBUG_ECHO
#End If
                If CMeansLoop(iteration) Then
                    Exit While
                End If
            End While

            Return clusterCenters
        End Function

        Private Function CMeansLoop(iteration As i32) As Boolean
            Dim ptClone As New Value(Of FuzzyCMeansEntity)
            Dim clusters As Dictionary(Of FuzzyCMeansEntity, FuzzyCMeansEntity) = MakeFuzzyClusters(coordinates, clusterCenters, fuzzificationParameter, membershipMatrix)

            'For Each pair As KeyValuePair(Of Entity, Entity) In clusters
            '    For Each annotation As Entity In coordinates

            '        If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, pair.Key.Properties) Then
            '            annotation.MarkClusterCenter(clusterColors(pair.Value))
            '        End If
            '    Next
            'Next

            For Each pair As KeyValuePair(Of FuzzyCMeansEntity, List(Of Double)) In membershipMatrix
                For Each annotation As FuzzyCMeansEntity In coordinates

                    If VectorEqualityComparer.VectorEqualsToAnother(annotation.entityVector, pair.Key.entityVector) Then
                        Dim tooltip As New Dictionary(Of Integer, Double)

                        For i As Integer = 0 To pair.Value.Count - 1
                            Dim value As Double = pair.Value(i)
                            Call tooltip.Add(i, stdNum.Round(value, 2))
                        Next

                        annotation.Memberships = tooltip
                    End If
                Next
            Next

            Dim oldClusterCenters As List(Of FuzzyCMeansEntity) = clusterCenters

            clusterCenters = RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters, membershipMatrix, fuzzificationParameter)
            If Not Trace Is Nothing Then
                Call Trace.Add(iteration, clusterCenters)
            End If

            Dim distancesToClusterCenters As Dictionary(Of FuzzyCMeansEntity, List(Of Double)) = coordinates.DistanceToClusterCenters(clusterCenters)
            Dim newMembershipMatrix As Dictionary(Of FuzzyCMeansEntity, List(Of Double)) = CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)

            Dim differences As List(Of List(Of Double)) = newMembershipMatrix.Values.DifferenceMatrix(membershipMatrix.Values.AsList())
            Dim maxElement As Double = GetMaxElement(differences)

            Call $"Max element: {maxElement}".__DEBUG_ECHO

            If maxElement <= threshold Then
                Return True
            End If

            'Dim colorValues As New List(Of Color)(clusterColors.Values)

            'Call clusterColors.Clear()

            'For i As Integer = 0 To clusterCenters.Count - 1
            '    clusterColors.Add(clusterCenters(i), colorValues(i))
            'Next

            For Each oldClusterCenter As FuzzyCMeansEntity In oldClusterCenters
                Dim isClusterCenterDataPoint As Boolean = False

                For Each coordinate As FuzzyCMeansEntity In coordinates

                    If VectorEqualityComparer.VectorEqualsToAnother(oldClusterCenter.entityVector, coordinate.entityVector) Then
#If DEBUG Then
                            Call $"ex-center {oldClusterCenter.uid}".__DEBUG_ECHO
#End If
                        isClusterCenterDataPoint = True
                        Exit For
                    End If
                Next

                If Not isClusterCenterDataPoint Then
                    For Each annotation As FuzzyCMeansEntity In coordinates

                        If VectorEqualityComparer.VectorEqualsToAnother(annotation.entityVector, oldClusterCenter.entityVector) Then
#If DEBUG Then
                                Call $"remove center with coordinate {annotation.uid}".__DEBUG_ECHO
#End If
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
                        '  MarkClusterCenter(annotation, colorValues(i))
                        isExists = True
                        Exit For
                    End If
                Next

                If Not isExists Then

                    '  Call MarkClusterCenter(ptClone, colorValues(i))
                    coordinates += ptClone = New FuzzyCMeansEntity With {
                        .entityVector = clusterCenter.entityVector.Clone,
                        .uid = clusterCenter.uid
                    }
#If DEBUG Then
                        Call $"add center with coordinate {(+ptClone).uid}".__DEBUG_ECHO
#End If
                End If
            Next

            Return False
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::ce3384ba888bc4cc558b5c9a7e01b655, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Algorithm\FuzzyCMeansAlgorithm.vb"

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
    '         Function: FuzzyCMeans, MakeFuzzyClusters, RecalculateCoordinateOfFuzzyClusterCenters
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
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
                                    Optional ByRef trace As Dictionary(Of Integer, FuzzyCMeansEntity()) = Nothing) As FuzzyCMeansEntity()

            If Not trace Is Nothing Then
                Call trace.Clear()
            End If

            Return New FuzzyCMeans(data, numberOfClusters, threshold, fuzzificationParameter) With {
                .trace = trace
            }.RunCMeans(maxIterates).ToArray
        End Function

        Friend Function MakeFuzzyClusters(points As List(Of FuzzyCMeansEntity),
                                clusterCenters As List(Of FuzzyCMeansEntity),
                        fuzzificationParameter As Double,
                        ByRef membershipMatrix As MembershipMatrix) As Dictionary(Of FuzzyCMeansEntity, FuzzyCMeansEntity)

            Dim distancesToClusterCenters As MembershipMatrix = MembershipMatrix.DistanceToClusterCenters(points, clusterCenters)
            Dim clusters As New Dictionary(Of FuzzyCMeansEntity, FuzzyCMeansEntity)()
            Dim clusterNumber As Integer

            membershipMatrix = MembershipMatrix.CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)

            For Each value In membershipMatrix.GetMemberships
                clusterNumber = Which.Max(value.membership)
                clusters.Add(value.entity, clusterCenters(clusterNumber))
            Next

            Return clusters
        End Function

        Friend Function RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters As List(Of FuzzyCMeansEntity),
                                                                 membershipMatrix As MembershipMatrix,
                                                           fuzzificationParameter As Double) As List(Of FuzzyCMeansEntity)

            Dim clusterMembershipValuesSums As New List(Of Double)()

            For Each clusterCenter As FuzzyCMeansEntity In clusterCenters
                clusterMembershipValuesSums.Add(0)
            Next

            Dim weightedPointCoordinateSums As New List(Of List(Of Double))()
            Dim memberRelationships = membershipMatrix.GetMemberships.ToArray

            For i As Integer = 0 To clusterCenters.Count - 1
                Dim clusterCoordinatesSum As New List(Of Double)()

                For Each coordinate As Double In clusterCenters(0).entityVector
                    clusterCoordinatesSum.Add(0)
                Next

                For Each member In memberRelationships
                    Dim pointCoordinates As FuzzyCMeansEntity = member.entity
                    Dim membershipValues As List(Of Double) = member.membership

                    clusterMembershipValuesSums(i) += stdNum.Pow(membershipValues(i), fuzzificationParameter)

                    For j As Integer = 0 To pointCoordinates.Length - 1
                        clusterCoordinatesSum(j) += pointCoordinates(j) * stdNum.Pow(membershipValues(i), fuzzificationParameter)
                    Next
                Next

                weightedPointCoordinateSums.Add(clusterCoordinatesSum)
            Next

            Dim newClusterCenters As New List(Of FuzzyCMeansEntity)()

            For i As Integer = 0 To clusterCenters.Count - 1
                Dim coordinatesSums As List(Of Double) = weightedPointCoordinateSums(i)
                Dim newCoordinates As New FuzzyCMeansEntity() With {
                    .entityVector = New Double(coordinatesSums.Count - 1) {},
                    .uid = clusterCenters(i).uid
                }

                For j As Integer = 0 To coordinatesSums.Count - 1
                    newCoordinates(j) = coordinatesSums(j) / clusterMembershipValuesSums(i)
                Next

                newClusterCenters.Add(newCoordinates)
            Next

            Return newClusterCenters
        End Function
    End Module
End Namespace

Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Namespace FuzzyCMeans

    Public Module FuzzyCMeansAlgorithm

        Public Sub DoClusteringByFuzzyCMeans(data As IEnumerable(Of Entity), numberOfClusters As Integer, fuzzificationParameter As Double, Optional maxIterates As Integer = Short.MaxValue)
            Dim coordinates As New List(Of Entity)(data)
            Dim random As New Random()
            Dim bgrColorComponents As Byte() = New Byte(2) {}
            Dim clusterCenters As List(Of Entity) = AlgorithmsUtils.MakeInitialSeeds(coordinates, numberOfClusters)
            Dim [stop] As Boolean = False
            Dim clusters As Dictionary(Of Entity, Entity)
            Dim membershipMatrix As Dictionary(Of Entity, List(Of Double)) = Nothing
            Dim iteration As int = 0
            Dim clusterColors As New Dictionary(Of Entity, Color)()

            For Each clusterCenter As Entity In clusterCenters
                For Each annotation As Entity In coordinates

                    If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, clusterCenter.Properties) Then
                        random.NextBytes(bgrColorComponents)

                        MarkClusterCenter(annotation, Color.FromArgb(bgrColorComponents(0), bgrColorComponents(1), bgrColorComponents(2)))
                        clusterColors.Add(clusterCenter, annotation.Fill)

                        Call $"Inital cluster center {annotation.uid }".__DEBUG_ECHO
                    End If
                Next
            Next

            While ++iteration <= maxIterates AndAlso Not [stop]
#If DEBUG Then
                Call $"Iteration = {iteration}".__DEBUG_ECHO
#End If
                clusters = MakeFuzzyClusters(coordinates, clusterCenters, fuzzificationParameter, membershipMatrix)

                For Each pair As KeyValuePair(Of Entity, Entity) In clusters
                    For Each annotation As Entity In coordinates

                        If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, pair.Key.Properties) Then
                            annotation.MarkClusterCenter(clusterColors(pair.Value))
                        End If
                    Next
                Next

                For Each pair As KeyValuePair(Of Entity, List(Of Double)) In membershipMatrix
                    For Each annotation As Entity In coordinates

                        If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, pair.Key.Properties) Then
                            Dim tooltip As String = ""

                            For i As Integer = 0 To pair.Value.Count - 1
                                Dim value As Double = pair.Value(i)
                                tooltip &= "Cluster " & i & ": Value: " & Math.Round(value, 2) & vbLf
                            Next

                            annotation.Extension.DynamicHash.Value(NameOf(tooltip)) = tooltip
                        End If
                    Next
                Next

                Dim oldClusterCenters As List(Of Entity) = clusterCenters

                clusterCenters = RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters, membershipMatrix, fuzzificationParameter)

                Dim distancesToClusterCenters As Dictionary(Of Entity, List(Of Double)) = AlgorithmsUtils.CalculateDistancesToClusterCenters(coordinates, clusterCenters)
                Dim newMembershipMatrix As Dictionary(Of Entity, List(Of Double)) = CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)

                Dim differences As List(Of List(Of Double)) = ListUtils.CreateDifferencesMatrix(newMembershipMatrix.Values.ToList(), membershipMatrix.Values.ToList())
                Dim maxElement As Double = ListUtils.GetMaxElement(differences)

#If DEBUG Then
                Call $"Max element: {maxElement}".__DEBUG_ECHO
#End If

                If maxElement < 0.001 Then
                    [stop] = True
                Else
                    Dim colorValues As List(Of Color) = clusterColors.Values.ToList()
                    clusterColors.Clear()
                    For i As Integer = 0 To clusterCenters.Count - 1
                        clusterColors.Add(clusterCenters(i), colorValues(i))
                    Next

                    For Each oldClusterCenter As Entity In oldClusterCenters
                        Dim isClusterCenterDataPoint As Boolean = False
                        For Each coordinate As Entity In coordinates

                            If oldClusterCenter(0) = coordinate(0) AndAlso oldClusterCenter(1) = coordinate(1) Then
                                '#Region "DEBUG"
#If DEBUG Then
                                Console.WriteLine("ex-center x = {0}, y = {1}", oldClusterCenter(0), oldClusterCenter(1))
#End If
                                '#End Region

                                isClusterCenterDataPoint = True
                                Exit For
                            End If
                        Next

                        If Not isClusterCenterDataPoint Then
                            For Each annotation As Entity In coordinates

                                If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, oldClusterCenter.Properties) Then
                                    '#Region "DEBUG"
#If DEBUG Then
                                    Call $"remove center with coordinate {annotation.uid}".__DEBUG_ECHO
#End If
                                    '#End Region

                                    coordinates.Remove(annotation)
                                    Exit For
                                End If
                            Next
                        End If
                    Next

                    For i As Integer = 0 To clusterCenters.Count - 1
                        Dim clusterCenter As Entity = clusterCenters(i)
                        Dim isExists As Boolean = False
                        For Each annotation As Entity In coordinates

                            If VectorEqualityComparer.VectorEqualsToAnother(annotation.Properties, clusterCenter.Properties) Then
                                MarkClusterCenter(annotation, colorValues(i))
                                isExists = True
                                Exit For
                            End If
                        Next

                        If Not isExists Then
                            Dim pointAnnotation As New Entity With {
                            .Properties = clusterCenter.Properties.Clone,
                            .uid = clusterCenter.uid
                        }
                            MarkClusterCenter(pointAnnotation, colorValues(i))
                            coordinates.Add(pointAnnotation)

                            Call $"add center with coordinate { pointAnnotation.uid }".__DEBUG_ECHO
                        End If
                    Next
                End If
            End While
        End Sub
    End Module
End Namespace
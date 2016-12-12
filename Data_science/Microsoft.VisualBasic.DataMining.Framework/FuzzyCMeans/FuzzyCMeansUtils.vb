Imports System.Collections.Generic

Namespace FuzzyCMeans

    ''' <summary>
    ''' Шаги алгоритма C-Means
    ''' </summary>
    Partial Public Class FuzzyCMeansAlgorithm

        Private Shared Function MakeFuzzyClusters(points As List(Of List(Of Double)), clusterCenters As List(Of List(Of Double)), fuzzificationParameter As Double, ByRef membershipMatrix As Dictionary(Of List(Of Double), List(Of Double))) As Dictionary(Of List(Of Double), List(Of Double))
            'расстояния от точек до центров всех кластеров
            Dim distancesToClusterCenters As Dictionary(Of List(Of Double), List(Of Double)) = AlgorithmsUtils.CalculateDistancesToClusterCenters(points, clusterCenters)
            Dim clusters As New Dictionary(Of List(Of Double), List(Of Double))()
            'матрица членства - Отображение "координаты точки" -> "значения функции членства точки во всех кластерах"
            membershipMatrix = CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)

            For Each value As KeyValuePair(Of List(Of Double), List(Of Double)) In membershipMatrix
                'получение номера кластера с ближайшим центром
                Dim clusterNumber As Integer = ListUtils.GetMaxIndex(value.Value)
                clusters.Add(value.Key, clusterCenters(clusterNumber))
            Next

            Return clusters
        End Function

        ''' <summary>
        ''' Этот метод создаёт матрицу членства
        ''' </summary>
        ''' <param name="distancesToClusterCenters">расстояния от точек до всех центров кластеров отображение "координаты точки" -> "расстояния до всех 
        ''' центров кластеров"</param>
        ''' <param name="fuzzificationParameter">параметр фаззификации</param>
        ''' <returns>Отображение "координаты точки" -> "значения функции членства точки во всех кластерах"</returns> 
        Private Shared Function CreateMembershipMatrix(distancesToClusterCenters As Dictionary(Of List(Of Double), List(Of Double)), fuzzificationParameter As Double) As Dictionary(Of List(Of Double), List(Of Double))
            Dim map As New Dictionary(Of List(Of Double), List(Of Double))()

            For Each pair As KeyValuePair(Of List(Of Double), List(Of Double)) In distancesToClusterCenters
                Dim unNormaizedMembershipValues As New List(Of Double)()
                Dim sum As Double = 0

                For i As Integer = 0 To pair.Value.Count - 1
                    Dim distance As Double = pair.Value(i)
                    If distance = 0 Then
                        distance = 0.0000001
                    End If

                    Dim membershipValue As Double = Math.Pow(1 / distance, (1 / (fuzzificationParameter - 1)))
                    sum += membershipValue
                    unNormaizedMembershipValues.Add(membershipValue)
                Next

                Dim membershipValues As New List(Of Double)()
                For Each membershipValue As Double In unNormaizedMembershipValues
                    membershipValues.Add((membershipValue / sum))
                Next
                map.Add(pair.Key, membershipValues)
            Next

            Return map
        End Function

        ''' <summary>
        ''' Расчёт новых центров кластеров
        ''' </summary>
        ''' <param name="clusterCenters">текущие координаты центров кластеров</param>
        ''' <param name="membershipMatrix">матрица членства - отображение "координаты точки" -> "значения функции членства точки во всех кластерах"</param>
        ''' <param name="fuzzificationParameter">параметр фаззификации</param>
        ''' <returns>Новые координаты центров кластеров</returns>
        Private Shared Function RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters As List(Of List(Of Double)), membershipMatrix As Dictionary(Of List(Of Double), List(Of Double)), fuzzificationParameter As Double) As List(Of List(Of Double))
            Dim clusterMembershipValuesSums As New List(Of Double)()
            For Each clusterCenter As List(Of Double) In clusterCenters
                clusterMembershipValuesSums.Add(0)
            Next

            Dim weightedPointCoordinateSums As New List(Of List(Of Double))()
            For i As Integer = 0 To clusterCenters.Count - 1
                Dim clusterCoordinatesSum As New List(Of Double)()
                For Each coordinate As Double In clusterCenters(0)
                    clusterCoordinatesSum.Add(0)
                Next

                For Each pair As KeyValuePair(Of List(Of Double), List(Of Double)) In membershipMatrix
                    Dim pointCoordinates As List(Of Double) = pair.Key
                    Dim membershipValues As List(Of Double) = pair.Value

                    clusterMembershipValuesSums(i) += Math.Pow(membershipValues(i), fuzzificationParameter)

                    For j As Integer = 0 To pointCoordinates.Count - 1
                        clusterCoordinatesSum(j) += pointCoordinates(j) * Math.Pow(membershipValues(i), fuzzificationParameter)
                    Next
                Next

                weightedPointCoordinateSums.Add(clusterCoordinatesSum)
            Next

            Dim newClusterCenters As New List(Of List(Of Double))()
            For i As Integer = 0 To clusterCenters.Count - 1
                Dim newCoordinates As New List(Of Double)()
                Dim coordinatesSums As List(Of Double) = weightedPointCoordinateSums(i)
                For j As Integer = 0 To coordinatesSums.Count - 1
                    newCoordinates.Add(coordinatesSums(j) / clusterMembershipValuesSums(i))
                Next

                newClusterCenters.Add(newCoordinates)
            Next

            Return newClusterCenters
        End Function
    End Class
End Namespace
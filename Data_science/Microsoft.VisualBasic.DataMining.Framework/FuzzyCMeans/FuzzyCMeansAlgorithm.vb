Imports Microsoft.VisualBasic.DataMining.KMeans

Namespace FuzzyCMeans

    Partial Public Class FuzzyCMeansAlgorithm

        Public Shared Sub DoClusteringByFuzzyCMeans(data As IEnumerable(Of Entity), numberOfClusters As Integer, fuzzificationParameter As Double)
            Dim coordinates As New List(Of Entity)(data)
            Dim random As New Random()
            Dim bgrColorComponents As Byte() = New Byte(2) {}
            Dim clusterCenters As List(Of Entity) = AlgorithmsUtils.MakeInitialSeeds(coordinates, numberOfClusters)
            Dim [stop] As Boolean = False
            Dim clusters As Dictionary(Of Entity, Entity)
            Dim membershipMatrix As Dictionary(Of Entity, List(Of Double)) = Nothing

            Dim iteration As Integer = 0

            While Not [stop]
#If DEBUG Then
                Call $"Iteration = {iteration}".__DEBUG_ECHO
#End If
                clusters = MakeFuzzyClusters(coordinates, clusterCenters, fuzzificationParameter, membershipMatrix)
                For Each pair As KeyValuePair(Of Entity, Entity) In clusters
                    For Each annotation As Entity In coordinates

                        If annotation.X = pair.Key(0) AndAlso annotation.Y = pair.Key(1) Then
                            'закрашиваем точку цветом кластера
                            annotation.Fil = clusterColors(pair.Value)
                        End If
                    Next
                Next

                'отображение значений матрицы членства для каждой точки на графике
                For Each pair As KeyValuePair(Of Entity, List(Of Double)) In membershipMatrix
                    For Each annotation As PointAnnotation In plot.Model.Annotations
                        ' todo: разобраться со сравнением флоат-пойнт чисел
                        If annotation.X = pair.Key(0) AndAlso annotation.Y = pair.Key(1) Then
                            Dim tooltip As [String] = ""

                            For i As Integer = 0 To pair.Value.Count - 1
                                Dim value As Double = pair.Value(i)
                                tooltip += "Cluster " & i & ": Value: " & Math.Round(value, 2) & vbLf
                            Next

                            annotation.ToolTip = tooltip
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
                    'если координаты центров кластеров поменялись изменились, пересчитываем кластеры
                    Dim colorValues As List(Of OxyColor) = clusterColors.Values.ToList()
                    clusterColors.Clear()
                    For i As Integer = 0 To clusterCenters.Count - 1
                        clusterColors.Add(clusterCenters(i), colorValues(i))
                    Next

                    'удаление отображения всех центров кластеров
                    For Each annotation As PointAnnotation In plot.Model.Annotations
                        annotation.Shape = MarkerType.Circle
                        annotation.Size = 4
                    Next

                    'проверка на потенциально не существующие центры кластеров
                    For Each oldClusterCenter As Entity In oldClusterCenters
                        Dim isClusterCenterDataPoint As Boolean = False
                        For Each coordinate As Entity In coordinates
                            ' todo: разобраться со сравнением флоат-пойнт чисел
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
                        'если центр кластера не является точкой данных
                        If Not isClusterCenterDataPoint Then
                            For Each annotation As PointAnnotation In plot.Model.Annotations
                                ' todo: разобраться со сравнением флоат-пойнт чисел
                                If annotation.X = oldClusterCenter(0) AndAlso annotation.Y = oldClusterCenter(1) Then
                                    '#Region "DEBUG"
#If DEBUG Then
                                    Console.WriteLine("remove center with coordinate x = {0}, y = {1}", annotation.X, annotation.Y)
#End If
                                    '#End Region

                                    'удаление центра кластера
                                    plot.Model.Annotations.Remove(annotation)
                                    Exit For
                                End If
                            Next
                        End If
                    Next

                    'Отмечаем новые кластеры на графике
                    For i As Integer = 0 To clusterCenters.Count - 1
                        Dim clusterCenter As Entity = clusterCenters(i)
                        Dim isExists As Boolean = False
                        For Each annotation As PointAnnotation In plot.Model.Annotations
                            ' todo: разобраться со сравнением флоат-пойнт чисел
                            If annotation.X = clusterCenter(0) AndAlso annotation.Y = clusterCenter(1) Then
                                'если центр кластера с такими координатами существует, помечаем его на графике как центр кластера
                                UIUtils.MarkClusterCenter(annotation, colorValues(i))
                                isExists = True
                                Exit For
                            End If
                        Next
                        'если центр кластера с такими координатами не существует, создаём на графике новую точку и помечаем её как центр кластера
                        If Not isExists Then
                            Dim pointAnnotation As New PointAnnotation() With {
                            .X = clusterCenter(0),
                            .Y = clusterCenter(1)
                        }
                            UIUtils.MarkClusterCenter(pointAnnotation, colorValues(i))
                            plot.Model.Annotations.Add(pointAnnotation)

                            Console.WriteLine("add center with coordinate x = {0}, y = {1}", pointAnnotation.X, pointAnnotation.Y)
                        End If
                    Next
                End If

                iteration += 1
            End While
        End Sub
    End Class
End Namespace
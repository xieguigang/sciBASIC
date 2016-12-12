Namespace FuzzyCMeans

    Partial Public Class FuzzyCMeansAlgorithm

        Public Shared Sub DoClusteringByFuzzyCMeans(plot As PlotView, label As Label, numberOfClusters As Integer, fuzzificationParameter As Double)
            If plot.Model Is Nothing Then
                Return
            End If

            Dim coordinates As New List(Of List(Of Double))()
            For Each annotation As PointAnnotation In plot.Model.Annotations
                Dim pointCoordinates As New List(Of Double)() From {
                annotation.X,
                annotation.Y
            }
                coordinates.Add(pointCoordinates)
            Next

            Dim random As New Random()
            Dim bgrColorComponents As Byte() = New Byte(2) {}
            'цвета кластеров, выбираются случайно
            Dim clusterColors As New Dictionary(Of List(Of Double), OxyColor)()

            'первоначальная генерация центров кластеров
            Dim clusterCenters As List(Of List(Of Double)) = AlgorithmsUtils.MakeInitialSeeds(coordinates, numberOfClusters)

            For Each clusterCenter As List(Of Double) In clusterCenters
                For Each annotation As PointAnnotation In plot.Model.Annotations
                    ' todo: разобраться со сравнением флоат-пойнт чисел
                    If annotation.X = clusterCenter(0) AndAlso annotation.Y = clusterCenter(1) Then
                        random.NextBytes(bgrColorComponents)
                        'отметим на графике центры кластеров
                        UIUtils.MarkClusterCenter(annotation, OxyColor.FromRgb(bgrColorComponents(0), bgrColorComponents(1), bgrColorComponents(2)))
                        clusterColors.Add(clusterCenter, annotation.Fill)

                        '#Region "DEBUG"
#If DEBUG Then
#End If
                        '#End Region
                        Console.WriteLine("Inital cluster center x = {0}, y = {1}", annotation.X, annotation.Y)
                    End If
                Next
            Next

            Dim [stop] As Boolean = False
            Dim clusters As Dictionary(Of List(Of Double), List(Of Double)) = Nothing

            'матрица членства - Отображение "координаты точки" -> "значения функции членства точки во всех кластерах"
            Dim membershipMatrix As Dictionary(Of List(Of Double), List(Of Double)) = Nothing

            Dim iteration As Integer = 0

            'цикл продолжается пока меняются координаты центров кластеров
            While Not [stop]
                '#Region "DEBUG"
#If DEBUG Then
                Console.WriteLine("Iteration = {0}", iteration)
#End If
                '#End Region

                label.Content = "Iteration " & iteration

                'отображение из координат точки в координаты центра кластера
                clusters = MakeFuzzyClusters(coordinates, clusterCenters, fuzzificationParameter, membershipMatrix)
                For Each pair As KeyValuePair(Of List(Of Double), List(Of Double)) In clusters
                    For Each annotation As PointAnnotation In plot.Model.Annotations
                        ' todo: разобраться со сравнением флоат-пойнт чисел
                        If annotation.X = pair.Key(0) AndAlso annotation.Y = pair.Key(1) Then
                            'закрашиваем точку цветом кластера
                            annotation.Fill = clusterColors(pair.Value)
                        End If
                    Next
                Next

                'отображение значений матрицы членства для каждой точки на графике
                For Each pair As KeyValuePair(Of List(Of Double), List(Of Double)) In membershipMatrix
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

                Dim oldClusterCenters As List(Of List(Of Double)) = clusterCenters
                'пересчёт центров кластеров 
                clusterCenters = RecalculateCoordinateOfFuzzyClusterCenters(clusterCenters, membershipMatrix, fuzzificationParameter)

                Dim distancesToClusterCenters As Dictionary(Of List(Of Double), List(Of Double)) = AlgorithmsUtils.CalculateDistancesToClusterCenters(coordinates, clusterCenters)
                Dim newMembershipMatrix As Dictionary(Of List(Of Double), List(Of Double)) = CreateMembershipMatrix(distancesToClusterCenters, fuzzificationParameter)

                Dim differences As List(Of List(Of Double)) = ListUtils.CreateDifferencesMatrix(newMembershipMatrix.Values.ToList(), membershipMatrix.Values.ToList())
                'если координаты центров кластеров не изменились, выходим из цикла
                Dim maxElement As Double = ListUtils.GetMaxElement(differences)

                '#Region "DEBUG"
#If DEBUG Then
                Console.WriteLine("Max element: {0}", maxElement)
#End If
                '#End Region

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
                    For Each oldClusterCenter As List(Of Double) In oldClusterCenters
                        Dim isClusterCenterDataPoint As Boolean = False
                        For Each coordinate As List(Of Double) In coordinates
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
                        Dim clusterCenter As List(Of Double) = clusterCenters(i)
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

                            '#Region "DEBUG"
#If DEBUG Then
#End If
                            '#End Region
                            Console.WriteLine("add center with coordinate x = {0}, y = {1}", pointAnnotation.X, pointAnnotation.Y)
                        End If
                    Next
                End If

                plot.InvalidatePlot()
                iteration += 1
            End While
        End Sub
    End Class
End Namespace
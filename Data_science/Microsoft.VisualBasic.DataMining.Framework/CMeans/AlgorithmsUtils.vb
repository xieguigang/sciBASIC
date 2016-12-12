Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' Общие шаги алгоритма
''' </summary>
Public Class AlgorithmsUtils
	''' <summary>
	''' Этот метод подсчитывает евклидово расстояние от точки до всех центров кластеров
	''' </summary>
	''' <param name="points">координаты точек</param>
	''' <param name="clusterCenters">координаты центров кластеров</param>
	''' <returns>Отображение "координаты точки" -> "расстояния до всех центров кластеров"</returns> 
	Public Shared Function CalculateDistancesToClusterCenters(points As List(Of List(Of Double)), clusterCenters As List(Of List(Of Double))) As Dictionary(Of List(Of Double), List(Of Double))
		Dim map As New Dictionary(Of List(Of Double), List(Of Double))()

		For Each pointCoordinates As List(Of Double) In points
			Dim distancesToCenters As New List(Of Double)()
			For Each clusterCenter As List(Of Double) In clusterCenters
				'расчёт евклидового расстояния
				Dim distance As Double = 0
				For i As Integer = 0 To pointCoordinates.Count - 1
					distance += Math.Pow(pointCoordinates(i) - clusterCenter(i), 2)
				Next
				distance = Math.Sqrt(distance)
				distancesToCenters.Add(distance)
			Next
			map.Add(pointCoordinates, distancesToCenters)
		Next

		Return map
	End Function

	''' <summary>
	''' Генерирует случайные точки
	''' </summary>
	''' <param name="dimension">Размерность данных</param>
	''' <returns>Координаты точек</returns> 
	Public Shared Function GenerateDataPoints(dimension As Integer) As HashSet(Of List(Of Double))
		Dim coordinates As New HashSet(Of List(Of Double))(New ListOfDoubleEqualityComparer())
		Dim random As New Random()

		While coordinates.Count < 50
			Dim list As New List(Of Double)()
			For j As Integer = 0 To dimension - 1
				list.Add(random.[Next](0, 100))
			Next

			Dim b As Boolean = coordinates.Add(list)

			'#Region "DEBUG"
			#If DEBUG Then
			If Not b Then
				Console.WriteLine("Duplicate detected while generating data points")
				#End If
				'#End Region
			End If
		End While

		Return coordinates
	End Function

	''' <summary>
	''' Первоначальная случайная генерация центров кластеров
	''' </summary>
	''' <param name="coordinates">координаты точек</param>
	''' <param name="numberOfClusters">количество кластеров</param>
	''' <returns>координаты центров кластеров</returns>
	Public Shared Function MakeInitialSeeds(coordinates As List(Of List(Of Double)), numberOfClusters As Integer) As List(Of List(Of Double))
		Dim random As New Random()
		Dim coordinatesCopy As List(Of List(Of Double)) = coordinates.ToList()
		Dim initialClusterCenters As New List(Of List(Of Double))()
		For i As Integer = 0 To numberOfClusters - 1
			Dim clusterCenterPointNumber As Integer = random.[Next](0, coordinatesCopy.Count)
			initialClusterCenters.Add(coordinatesCopy(clusterCenterPointNumber))
			coordinatesCopy.RemoveAt(clusterCenterPointNumber)
		Next

		Return initialClusterCenters
	End Function
End Class

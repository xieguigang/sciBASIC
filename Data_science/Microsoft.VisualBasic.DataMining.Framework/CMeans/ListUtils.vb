Imports System.Collections.Generic

''' <summary>
''' Класс утилит для работы с листами
''' </summary>
Public Class ListUtils
	''' <summary>
	''' Сравнивает листы поэлементно
	''' </summary>
	''' <param name="list1">Первый аргумент для сравнения</param>
	''' <param name="list2">Второй аргумент для сравнения</param>
	''' <returns>Результат поэлементного сравнения листов</returns>
	Public Shared Function IsListEqualsToAnother(list1 As List(Of Double), list2 As List(Of Double)) As Boolean
		If list1.Count <> list2.Count Then
			Return False
		End If

		For i As Integer = 0 To list1.Count - 1
			' todo: разобраться со сравнением флоат-пойнт чисел                
			If list1(i) <> list2(i) Then
				Return False
			End If
		Next

		Return True
	End Function

	''' <summary>
	''' Сравнивает листы поэлементно
	''' </summary>
	''' <param name="list1">Первый аргумент для сравнения</param>
	''' <param name="list2">Второй аргумент для сравнения</param>
	''' <returns>Результат поэлементного сравнения листов</returns>
	Public Shared Function IsListEqualsToAnother(list1 As List(Of List(Of Double)), list2 As List(Of List(Of Double))) As Boolean
		If list1.Count <> list2.Count Then
			Return False
		End If

		For i As Integer = 0 To list1.Count - 1
			If list1(i).Count <> list2(i).Count Then
				Return False
			End If
		Next

		For i As Integer = 0 To list1.Count - 1
			For j As Integer = 0 To list1(i).Count - 1
				' todo: разобраться со сравнением флоат-пойнт чисел
				If list1(i)(j) <> list2(i)(j) Then
					Return False
				End If
			Next
		Next

		Return True
	End Function

	''' <summary>
	''' Получение индекса элемента с минимальным значением в листе
	''' </summary>
	''' <param name="values">лист значений</param>
	''' <returns>индекс элемента с минимальным значением в листе</returns> 
	Public Shared Function GetMinIndex(values As List(Of Double)) As Integer
		Dim min As Double = Double.MaxValue
		Dim minIndex As Integer = 0
		For i As Integer = 0 To values.Count - 1
			If values(i) < min Then
				min = values(i)
				minIndex = i
			End If
		Next

		Return minIndex
	End Function

	''' <summary>
	''' Получение индекса элемента с максимальным значением в листе
	''' </summary>
	''' <param name="values">лист значений</param>
	''' <returns>индекс элемента с максимальным значением в листе</returns> 
	Public Shared Function GetMaxIndex(values As List(Of Double)) As Integer
		Dim max As Double = Double.MinValue
		Dim maxIndex As Integer = 0
		For i As Integer = 0 To values.Count - 1
			If values(i) > max Then
				max = values(i)
				maxIndex = i
			End If
		Next

		Return maxIndex
	End Function

	''' <summary>
	''' Получение элемента с максимальным значением в листе
	''' </summary>
	''' <param name="values">лист значений</param>
	''' <returns>элемент с максимальным значением в листе</returns> 
	Public Shared Function GetMaxElement(values As List(Of List(Of Double))) As Double
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
	''' Создание матрицы различий между двумя матрицами
	''' </summary>
	''' <param name="matrix1">Уменьшаемая матрица</param>
	''' <param name="matrix2">Вычитаемая матрица</param>
	''' <returns>Матрица-разность</returns>
	Public Shared Function CreateDifferencesMatrix(matrix1 As List(Of List(Of Double)), matrix2 As List(Of List(Of Double))) As List(Of List(Of Double))
		Dim differences As New List(Of List(Of Double))()
		For i As Integer = 0 To matrix1.Count - 1
			Dim rowDifferences As New List(Of Double)()
			For j As Integer = 0 To matrix1(0).Count - 1
				Dim result As Double = Math.Abs(matrix1(i)(j) - matrix2(i)(j))
				rowDifferences.Add(result)
			Next

			differences.Add(rowDifferences)
		Next

		Return differences
	End Function


	''' <summary>
	'''Получение индекса указанного элемента
	''' </summary>
	''' <param name="list">Лист</param>
	''' <param name="element">Элемент</param>
	''' <returns>Индекс указанного элемента</returns>
	Public Shared Function GetElementIndex(list As List(Of List(Of Double)), element As List(Of Double)) As Integer
		For i As Integer = 0 To list.Count - 1
			If IsListEqualsToAnother(list(i), element) Then
				Return i
			End If
		Next

		Return -1
	End Function
End Class

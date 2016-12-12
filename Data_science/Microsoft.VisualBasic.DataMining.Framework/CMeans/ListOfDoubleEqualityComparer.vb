Imports System.Collections.Generic

''' <summary>
''' Сравнение List 'double'
''' </summary>
Public Class ListOfDoubleEqualityComparer
	Implements IEqualityComparer(Of List(Of Double))
    ''' <summary>
    ''' Переопределение метода Equals для List 'double'
    ''' </summary>
    ''' <param name="x">Первый лист</param>
    ''' <param name="y">Второй лист</param>
    ''' <returns></returns>
    Public Overloads Function Equals(x As List(Of Double), y As List(Of Double)) As Boolean Implements IEqualityComparer(Of List(Of Double)).Equals
        Return ListUtils.IsListEqualsToAnother(x, y)
    End Function

    ''' <summary>
    ''' Получение хэш-кода для List 'double'
    ''' </summary>
    ''' <param name="obj">объект хэширования</param>
    ''' <returns></returns>
    Public Overloads Function GetHashCode(obj As List(Of Double)) As Integer Implements IEqualityComparer(Of List(Of Double)).GetHashCode
        Dim sum As Integer = obj.Sum(Function(x) x.GetHashCode)
        Return sum
    End Function
End Class

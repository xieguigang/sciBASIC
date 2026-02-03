Namespace utils

    ''' <summary>
    ''' Implements an array that is also indexed by the values, which means that it
    ''' takes O(1) time to access an item both by its index and by its value.
    ''' Removing items is not possible. Adapted from
    ''' http://stackoverflow.com/a/7834138
    ''' </summary>
    ''' @param <T>
    '''            array type </param>
    Public Class BidirectionalArray(Of T)
        Private indexToValueMap As IList(Of T) = New List(Of T)()
        Private valueToIndexMap As IDictionary(Of T, Integer) = New Dictionary(Of T, Integer)()

        ''' <returns> false if the value is already present </returns>
        Public Overridable Function add(value As T) As Boolean
            If containsValue(value) Then
                Return False
            End If
            Dim size = indexToValueMap.Count
            indexToValueMap.Insert(size, value)
            valueToIndexMap(value) = size
            Return True
        End Function

        Public Overridable Function containsValue(value As T) As Boolean
            Return valueToIndexMap.ContainsKey(value)
        End Function

        Public Overridable Function getIndex(value As T) As Integer
            Return valueToIndexMap(value)
        End Function

        Public Overridable Function [get](index As Integer) As T
            Return indexToValueMap(index)
        End Function

        Public Overridable Function size() As Integer
            Return indexToValueMap.Count
        End Function

        Public Overrides Function ToString() As String
            Return "" & indexToValueMap.ToString()
        End Function
    End Class
End Namespace

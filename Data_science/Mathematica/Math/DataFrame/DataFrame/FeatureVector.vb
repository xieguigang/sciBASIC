
Public Class FeatureVector

    Public ReadOnly Property vector As Array
    Public ReadOnly Property type As Type

    Public ReadOnly Property isScalar As Boolean
        Get
            Return vector Is Nothing OrElse vector.Length = 0 OrElse vector.Length = 1
        End Get
    End Property

    Sub New(ints As IEnumerable(Of Integer))
        vector = ints.ToArray
        type = GetType(Integer)
    End Sub

    Sub New(factors As IEnumerable(Of String))
        vector = factors.ToArray
        type = GetType(String)
    End Sub

    Sub New(floats As IEnumerable(Of Single))
        vector = floats.ToArray
        type = GetType(Single)
    End Sub

    Sub New(doubles As IEnumerable(Of Double))
        vector = doubles.ToArray
        type = GetType(Double)
    End Sub

    Sub New(int16 As IEnumerable(Of Short))
        vector = int16.ToArray
        type = GetType(Short)
    End Sub

    Sub New(int64 As IEnumerable(Of Long))
        vector = int64.ToArray
        type = GetType(Long)
    End Sub

    Sub New(logicals As IEnumerable(Of Boolean))
        vector = logicals.ToArray
        type = GetType(Boolean)
    End Sub

    Public Function [TryCast](Of T)() As T()
        Throw New NotImplementedException
    End Function

    Public Overrides Function ToString() As String
        Return $"[{type.Name}] {vector.Length} elements"
    End Function

    Public Shared Function FromGeneral(vec As Array) As FeatureVector
        Select Case vec.GetType.GetElementType
            Case GetType(Integer) : Return New FeatureVector(DirectCast(vec, Integer()))
            Case GetType(Short) : Return New FeatureVector(DirectCast(vec, Short()))
            Case GetType(Long) : Return New FeatureVector(DirectCast(vec, Long()))
            Case GetType(Single) : Return New FeatureVector(DirectCast(vec, Single()))
            Case GetType(Double) : Return New FeatureVector(DirectCast(vec, Double()))
            Case GetType(Boolean) : Return New FeatureVector(DirectCast(vec, Boolean()))
            Case GetType(String) : Return New FeatureVector(DirectCast(vec, String()))
            Case Else
                Throw New NotImplementedException(vec.GetType.FullName)
        End Select
    End Function

End Class
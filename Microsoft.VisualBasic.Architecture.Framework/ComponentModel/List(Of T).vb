Public Class List(Of T) : Inherits Generic.List(Of T)

    Sub New(source As IEnumerable(Of T))
        Call MyBase.New(source.ToArray)
    End Sub

    Sub New(ParamArray x As T())
        Call MyBase.New(x)
    End Sub

    Sub New()
    End Sub

    Public Shared Operator +(list As List(Of T), x As T) As List(Of T)
        Call list.Add(x)
        Return list
    End Operator

    Public Shared Operator -(list As List(Of T), x As T) As List(Of T)
        Call list.Remove(x)
        Return list
    End Operator

    Public Shared Operator +(list As List(Of T), vals As IEnumerable(Of T)) As List(Of T)
        Call list.AddRange(vals.ToArray)
        Return list
    End Operator

    Public Shared Operator -(list As List(Of T), vals As IEnumerable(Of T)) As List(Of T)
        If Not vals Is Nothing Then
            For Each x As T In vals
                Call list.Remove(x)
            Next
        End If
        Return list
    End Operator

    Public Shared Operator -(list As List(Of T), index As Integer) As List(Of T)
        Call list.RemoveAt(index)
        Return list
    End Operator

    Public Shared Narrowing Operator CType(list As List(Of T)) As T()
        Return list.ToArray
    End Operator
End Class

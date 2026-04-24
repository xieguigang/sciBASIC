Namespace Pickle

    ''' <summary>
    ''' 表示 Python 的 set 类型。Python 的 set 是无序且元素唯一的集合，
    ''' 类似于 .NET 的 HashSet，但提供与 Python 互操作所需的类型标识。
    ''' 支持 frozenset 和 set 两种语义（此类型不区分可变/不可变）。
    ''' </summary>
    Public Class PythonSet
        Implements IEnumerable(Of Object)

        Private ReadOnly _items As New HashSet(Of Object)

        Public Sub New()
        End Sub

        Public Sub Add(item As Object)
            _items.Add(item)
        End Sub

        Public Function Contains(item As Object) As Boolean
            Return _items.Contains(item)
        End Function

        Public ReadOnly Property Count As Integer
            Get
                Return _items.Count
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of Object) Implements IEnumerable(Of Object).GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Overrides Function ToString() As String
            Return "{" & String.Join(", ", _items.Select(Function(o) If(o Is Nothing, "None", o.ToString()))) & "}"
        End Function
    End Class

End Namespace
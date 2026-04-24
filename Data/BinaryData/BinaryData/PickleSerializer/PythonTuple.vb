Namespace Pickle


    ''' <summary>
    ''' 表示 Python 的 tuple 类型。.NET 没有原生的不可变元组类型，
    ''' 因此使用此包装类来保持 Python 语义上的不可变性和有序性。
    ''' 元组在 Python 中常用于函数返回多值、字典键等场景。
    ''' </summary>
    Public Class PythonTuple
        Implements IEnumerable(Of Object)
        Implements IEquatable(Of PythonTuple)

        Private ReadOnly _items As Object()

        Public Sub New(items As Object())
            _items = If(items, Array.Empty(Of Object)())
        End Sub

        ''' <summary>按索引访问元组元素</summary>
        Default Public ReadOnly Property Item(index As Integer) As Object
            Get
                Return _items(index)
            End Get
        End Property

        ''' <summary>元组元素数量</summary>
        Public ReadOnly Property Length As Integer
            Get
                Return _items.Length
            End Get
        End Property

        ''' <summary>获取元素数组的副本（防止外部修改）</summary>
        Public ReadOnly Property Items As Object()
            Get
                Return DirectCast(_items.Clone(), Object())
            End Get
        End Property

        Public Function GetEnumerator() As IEnumerator(Of Object) Implements IEnumerable(Of Object).GetEnumerator
            Return DirectCast(_items, IEnumerable(Of Object)).GetEnumerator()
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _items.GetEnumerator()
        End Function

        Public Overrides Function ToString() As String
            Return "(" & String.Join(", ", _items.Select(Function(o) If(o Is Nothing, "None", o.ToString()))) & ")"
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Return Equals(TryCast(obj, PythonTuple))
        End Function

        Public Overloads Function Equals(other As PythonTuple) As Boolean Implements IEquatable(Of PythonTuple).Equals
            If other Is Nothing Then Return False
            If _items.Length <> other._items.Length Then Return False
            For i = 0 To _items.Length - 1
                If Not Object.Equals(_items(i), other._items(i)) Then Return False
            Next
            Return True
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim hash = 17
            For Each item As Object In _items
                hash = hash * 31 + If(item?.GetHashCode(), 0)
            Next
            Return hash
        End Function
    End Class

End Namespace
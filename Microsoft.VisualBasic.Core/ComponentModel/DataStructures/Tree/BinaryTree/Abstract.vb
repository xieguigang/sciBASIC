Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DataStructures.BinaryTree

    ''' <summary>
    ''' The generic object comparer model:
    ''' 
    ''' + ``> 0`` means ``a > b``
    ''' + ``= 0`` means ``a = 0``
    ''' + ``&lt;0`` means ``a &lt; b``
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Delegate Function CompareOf(Of T)(a As T, b As T) As Integer

    Public MustInherit Class TreeMap(Of K, V)
        Implements IKeyedEntity(Of K)
        Implements Value(Of V).IValueOf
        Implements IComparable(Of K)

        Public Property Key As K Implements IKeyedEntity(Of K).Key
        Public Property value As V Implements Value(Of V).IValueOf.Value

        Public MustOverride Function CompareTo(other As K) As Integer Implements IComparable(Of K).CompareTo

    End Class
End Namespace
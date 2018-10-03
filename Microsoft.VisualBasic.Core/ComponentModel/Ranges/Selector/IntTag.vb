Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    Public Structure IntTag(Of T)
        Implements IComparable

        Public ReadOnly n As Integer
        Public ReadOnly x As T

        Sub New(x As T, getInt As Func(Of T, Integer))
            Me.x = x
            Me.n = getInt(x)
        End Sub

        Sub New(n As Integer)
            Me.n = n
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            If obj Is Nothing Then
                Return 1
            Else
                If TypeOf obj Is Integer Then
                    Return n.CompareTo(DirectCast(obj, Integer))
                ElseIf TypeOf obj Is IntTag(Of T) Then
                    Return n.CompareTo(DirectCast(obj, IntTag(Of T)).n)
                Else
                    Return 0
                End If
            End If
        End Function

        Public Shared Function OrderSelector(source As IEnumerable(Of T),
                                             getInt As Func(Of T, Integer),
                                             Optional asc As Boolean = True) As OrderSelector(Of IntTag(Of T))
            Dim array As IEnumerable(Of IntTag(Of T)) = source.Select(Function(x) New IntTag(Of T)(x, getInt))
            Dim selects As New OrderSelector(Of IntTag(Of T))(array, asc)
            Return selects
        End Function

        Public Shared Widening Operator CType(n As Integer) As IntTag(Of T)
            Return New IntTag(Of T)(n)
        End Operator
    End Structure
End Namespace
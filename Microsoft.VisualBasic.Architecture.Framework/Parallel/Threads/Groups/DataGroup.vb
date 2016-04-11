Namespace Parallel

    Public MustInherit Class TaggedGroupData(Of T_TAG)
        Public Overridable Property TAG As T_TAG

        Public Overrides Function ToString() As String
            Return TAG.ToString
        End Function
    End Class

    Public Class GroupListNode(Of T, T_TAG) : Inherits TaggedGroupData(Of T_TAG)
        Implements IEnumerable(Of T)

        Dim _Group As List(Of T)

        Public Property Group As List(Of T)
            Get
                Return _Group
            End Get
            Set(value As List(Of T))
                _Group = value
                If value.IsNullOrEmpty Then
                    _InitReads = 0
                Else
                    _InitReads = value.Count
                End If
            End Set
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return Group.Count
            End Get
        End Property

        ''' <summary>
        ''' 由于<see cref="Group"/>在分组之后的后续的操作的过程之中元素会发生改变，
        ''' 所以在这个属性之中存储了在初始化<see cref="Group"/>列表的时候的原始的列表之中的元素的个数以满足一些其他的算法操作
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property InitReads As Integer

        Public Overrides Function ToString() As String
            Return MyBase.ToString & $" // {NameOf(InitReads)}:={InitReads},  current:={Count}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each obj In Group
                Yield obj
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class

    Public Class GroupResult(Of T, Itag) : Inherits TaggedGroupData(Of Itag)
        Implements IEnumerable(Of T)
        Implements IGrouping(Of Itag, T)

        Public Overrides Property TAG As Itag Implements IGrouping(Of Itag, T).Key
        Public Property Group As T()

        Public ReadOnly Property Count As Integer
            Get
                Return Group.Length
            End Get
        End Property

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For i As Integer = 0 To Group.Length - 1
                Yield Group(i)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

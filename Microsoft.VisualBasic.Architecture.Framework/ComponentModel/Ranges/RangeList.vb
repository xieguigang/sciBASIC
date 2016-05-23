Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

Namespace ComponentModel.Ranges

    Public Class RangeList(Of T As IComparable, V) : Inherits List(Of RangeTagValue(Of T, V))

        Sub New()
            MyBase.New(128)
        End Sub

        Public Function [Select](x As T) As RangeTagValue(Of T, V)
            Dim LQuery As RangeTagValue(Of T, V) =
                LinqAPI.DefaultFirst(Of RangeTagValue(Of T, V)) <=
                From r As RangeTagValue(Of T, V)
                In Me.AsQueryable
                Where r.IsInside(x)
                Select r

            Return LQuery
        End Function

        Public Function SelectValue(x As T, Optional [throw] As Boolean = True, Optional ByRef success As Boolean = False) As V
            Dim n As RangeTagValue(Of T, V) = [Select](x)
            If n Is Nothing Then
                If [throw] Then
                    Throw New DataException($"{x.GetJson} is not in any ranges!")
                Else
                    Return Nothing
                End If
            Else
                success = True
                Return n.Value
            End If
        End Function

        Public Function SelectValue(x As T, [default] As Func(Of T, V)) As V
            Dim success As Boolean = False
            Dim v As V = SelectValue(x, [throw]:=False, success:=success)

            If success Then
                Return v
            Else
                Return [default](x)
            End If
        End Function

        Public ReadOnly Iterator Property Values As IEnumerable(Of V)
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x.Value
                Next
            End Get
        End Property

        Public ReadOnly Iterator Property Keys As IEnumerable(Of Range(Of T))
            Get
                For Each x As RangeTagValue(Of T, V) In Me
                    Yield x
                Next
            End Get
        End Property
    End Class

    Public Class OrderSelector(Of T As IComparable)

        ReadOnly _source As T()

        Sub New(source As IEnumerable(Of T), Optional asc As Boolean = True)
            If asc Then
                _source = source.OrderBy(Function(x) x).ToArray
            Else
                _source = source.OrderByDescending(Function(x) x).ToArray
            End If
        End Sub

        ''' <summary>
        ''' 直到当前元素大于指定值
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Iterator Function SelectUntilGreaterThan(n As T) As IEnumerable(Of T)
            For Each x In _source
                If Language.LessThanOrEquals(x, n) Then
                    Yield x
                Else
                    Exit For   ' 由于是经过排序了的，所以在这里不再小于的话，则后面的元素都不会再比他小了
                End If
            Next
        End Function
    End Class
End Namespace
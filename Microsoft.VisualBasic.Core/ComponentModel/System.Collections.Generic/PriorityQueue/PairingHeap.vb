Imports Microsoft.VisualBasic.Language.JavaScript

Namespace ComponentModel.Collection

    Public Class PairingHeap(Of T)
        Private subheaps As Stack(Of PairingHeap(Of T))
        Public elem As T
        ' from: https://gist.github.com/nervoussystem
        '{elem:object, subheaps:[array of heaps]}
        Public Sub New(elem As T)
            Me.subheaps = New Stack(Of PairingHeap(Of T))
            Me.elem = elem
        End Sub

        Public Overloads Function toString(selector As Func(Of T, String)) As String
            Dim str = ""
            Dim needComma = False
            For i As Integer = 0 To Me.subheaps.Count - 1
                Dim subheap As PairingHeap(Of T) = Me.subheaps(i)
                If Not subheap.elem Is Nothing Then
                    needComma = False
                    Continue For
                End If
                If needComma Then
                    str = str & ","
                End If
                str = str & subheap.toString(selector)
                needComma = True
            Next
            If str <> "" Then
                str = "(" & str & ")"
            End If
            Return (If(Me.elem Is Nothing, selector(Me.elem), "")) & str
        End Function

        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            If Not Me.empty() Then
                f(Me.elem, Me)
                Me.subheaps.DoEach(Sub(s) s.forEach(f))
            End If
        End Sub

        Public Function count() As Double
            If Me.empty Then
                Return 0
            Else
                Return 1 + subheaps _
                    .Reduce(Function(n As Double, h As PairingHeap(Of T))
                                Return n + h.count()
                            End Function, 0)
            End If
        End Function

        Public Function min() As T
            Return Me.elem
        End Function

        Public Function empty() As Boolean
            Return Me.elem Is Nothing
        End Function

        Public Function contains(h As PairingHeap(Of T)) As Boolean
            If Me Is h Then
                Return True
            End If
            For i As Integer = 0 To Me.subheaps.Count - 1
                If Me.subheaps(i).contains(h) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function isHeap(lessThan As Func(Of T, T, Boolean)) As Boolean
            Return Me.subheaps.All(Function(h) lessThan(Me.elem, h.elem) AndAlso h.isHeap(lessThan))
        End Function

        Public Function insert(obj As T, lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Return Me.merge(New PairingHeap(Of T)(obj), lessThan)
        End Function

        Public Function merge(heap2 As PairingHeap(Of T), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.empty() Then
                Return heap2
            ElseIf heap2.empty() Then
                Return Me
            ElseIf lessThan(Me.elem, heap2.elem) Then
                Me.subheaps.Push(heap2)
                Return Me
            Else
                heap2.subheaps.Push(Me)
                Return heap2
            End If
        End Function

        Public Function removeMin(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.empty() Then
                Return Nothing
            Else
                Return Me.mergePairs(lessThan)
            End If
        End Function

        Public Function mergePairs(lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            If Me.subheaps.Count = 0 Then
                Return New PairingHeap(Of T)(Nothing)
            ElseIf Me.subheaps.Count = 1 Then
                Return Me.subheaps(0)
            Else
                Dim firstPair = Me.subheaps.pop().merge(Me.subheaps.pop(), lessThan)
                Dim remaining = Me.mergePairs(lessThan)
                Return firstPair.merge(remaining, lessThan)
            End If
        End Function
        Public Function decreaseKey(subheap As PairingHeap(Of T), newValue As T, setHeapNode As Action(Of T, PairingHeap(Of T)), lessThan As Func(Of T, T, Boolean)) As PairingHeap(Of T)
            Dim newHeap = subheap.removeMin(lessThan)
            'reassign subheap values to preserve tree
            subheap.elem = newHeap.elem
            subheap.subheaps = newHeap.subheaps
            If setHeapNode IsNot Nothing AndAlso newHeap.elem IsNot Nothing Then
                setHeapNode(subheap.elem, subheap)
            End If
            Dim pairingNode = New PairingHeap(Of T)(newValue)
            Call setHeapNode(newValue, pairingNode)
            Return Me.merge(pairingNode, lessThan)
        End Function
    End Class

End Namespace
Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    Class PairingHeap(Of T)
        Private subheaps As PairingHeap(Of T)()
        Public elem As T
        ' from: https://gist.github.com/nervoussystem
        '{elem:object, subheaps:[array of heaps]}
        Public Sub New(elem As T)
            Me.subheaps = New any() {}
            Me.elem = elem
        End Sub

        Public Overloads Function toString(selector As any) As String
            Dim str = ""
            Dim needComma = False
            For i As var = 0 To Me.subheaps.length - 1
                Dim subheap As PairingHeap(Of T) = Me.subheaps(i)
                If Not subheap.elem Then
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
            Return (If(Me.elem, selector(Me.elem), "")) & str
        End Function

        Public Sub forEach(f As Action(Of T, PairingHeap(Of T)))
            If Not Me.empty() Then
                f(Me.elem, Me)
                Me.subheaps.forEach(Function(s) s.forEach(f))
            End If
        End Sub

        Public Function count() As Double
            Return If(Me.empty(), 0, 1 + Me.subheaps.reduce(Function(n As Double, h As PairingHeap(Of T))
                                                                Return n + h.count()

                                                            End Function, 0))
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
            For i As var = 0 To Me.subheaps.length - 1
                If Me.subheaps(i).contains(h) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Function isHeap(lessThan As Func(Of T, T, Boolean)) As Boolean
            Return Me.subheaps.every(Function(h) lessThan(Me.elem, h.elem) AndAlso h.isHeap(lessThan))
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
                Me.subheaps.push(heap2)
                Return Me
            Else
                heap2.subheaps.push(Me)
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
            If Me.subheaps.length = 0 Then
                Return New PairingHeap(Of T)(Nothing)
            ElseIf Me.subheaps.length = 1 Then
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
            Dim pairingNode = New PairingHeap(newValue)
            RaiseEvent setHeapNode(newValue, pairingNode)
            Return Me.merge(pairingNode, lessThan)
        End Function
    End Class

    '*
    ' * @class PriorityQueue a min priority queue backed by a pairing heap
    ' 

    Class PriorityQueue(Of T)
        Private root As PairingHeap(Of T)
        Private lessThan As Func(Of T, T, Boolean)
        Public Sub New(lessThan As Func(Of T, T, Boolean))
            Me.lessThan = lessThan
        End Sub
        '*
        '     * @method top
        '     * @return the top element (the min element as defined by lessThan)
        '     

        Public Function top() As T
            If Me.empty() Then
                Return Nothing
            End If
            Return Me.root.elem
        End Function
        '*
        '     * @method push
        '     * put things on the heap
        '     

        Public Function push(args As T()) As PairingHeap(Of T)
            Dim pairingNode As any

            Dim i As Integer = 0
            While i > -1
                Dim arg As T = args(i - 1)
                pairingNode = New PairingHeap(arg)
                Me.root = If(Me.empty(), pairingNode, Me.root.merge(pairingNode, Me.lessThan))
                i += 1
            End While
            Return pairingNode
        End Function
        '*
        '     * @method empty
        '     * @return true if no more elements in queue
        '     

        Public Function empty() As Boolean
            Return Not Me.root OrElse Not Me.root.elem
        End Function
        '*
        '     * @method isHeap check heap condition (for testing)
        '     * @return true if queue is in valid state
        '     

        Public Function isHeap() As Boolean
            Return Me.root.isHeap(Me.lessThan)
        End Function
        '*
        '     * @method forEach apply f to each element of the queue
        '     * @param f function to apply
        '     

        Public Sub forEach(f As any)
            Me.root.forEach(f)
        End Sub
        '*
        '     * @method pop remove and return the min element from the queue
        '     

        Public Function pop() As T
            If Me.empty() Then
                Return Nothing
            End If
            Dim obj = Me.root.min()
            Me.root = Me.root.removeMin(Me.lessThan)
            Return obj
        End Function
        '*
        '     * @method reduceKey reduce the key value of the specified heap node
        '     

        Public Sub reduceKey(heapNode As PairingHeap(Of T), newKey As T, setHeapNode As Action(Of T, PairingHeap(Of T)))
            Me.root = Me.root.decreaseKey(heapNode, newKey, setHeapNode, Me.lessThan)
        End Sub
        Public Overloads Function toString(selector As any) As String
            Return Me.root.toString(selector)
        End Function
        '*
        '     * @method count
        '     * @return number of elements in queue
        '     

        Public Function count() As Double
            Return Me.root.count()
        End Function
    End Class
End Namespace
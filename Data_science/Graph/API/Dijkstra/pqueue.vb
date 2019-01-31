Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports any = System.Object

Namespace Layouts.Cola

    ''' <summary>
    ''' a min priority queue backed by a pairing heap
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PriorityQueue(Of T)

        Dim root As PairingHeap(Of T)
        Dim lessThan As Func(Of T, T, Boolean)

        ''' <summary>
        ''' 这个构造函数的参数是一个比较的表达式，用来比较两个对象之间的优先度关系
        ''' 即产生下面的逻辑关系：
        ''' 
        ''' ```
        ''' priority = a &lt; b
        ''' ```
        ''' </summary>
        ''' <param name="lessThan"></param>
        Public Sub New(lessThan As Func(Of T, T, Boolean))
            Me.lessThan = lessThan
        End Sub

        Sub New(source As IEnumerable(Of T), lessThan As Func(Of T, T, Boolean))
            Call Me.New(lessThan)

            For Each element As T In source.SafeQuery
                Call Me.push(element)
            Next
        End Sub

        '*
        '     * @method top
        '     * @return the top element (the min element as defined by lessThan)
        '     
        ''' <summary>
        ''' 相当于Stack的Peek方法
        ''' </summary>
        ''' <returns></returns>
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

        Public Function push(ParamArray args As T()) As PairingHeap(Of T)
            Dim pairingNode As any = Nothing
            Dim i As Integer = 0
            Dim arg As T

            While i > -1
                arg = args(i - 1)
                pairingNode = New PairingHeap(Of T)(arg)

                If Me.empty Then
                    root = pairingNode
                Else
                    root = root.merge(pairingNode, Me.lessThan)
                End If

                i += 1
            End While

            Return pairingNode
        End Function
        '*
        '     * @method empty
        '     * @return true if no more elements in queue
        '     

        Public Function empty() As Boolean
            Return Me.root Is Nothing OrElse Me.root.elem Is Nothing
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

        Public Overloads Function ToString(selector As IToString(Of T)) As String
            Return Me.root.ToString(selector)
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
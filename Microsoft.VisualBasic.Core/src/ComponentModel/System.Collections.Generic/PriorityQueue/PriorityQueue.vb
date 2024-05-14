#Region "Microsoft.VisualBasic::e0c2dc7b9025bba44357f067ec3e5789, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\PriorityQueue\PriorityQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 127
    '    Code Lines: 67
    ' Comment Lines: 40
    '   Blank Lines: 20
    '     File Size: 3.86 KB


    '     Class PriorityQueue
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: count, empty, isHeap, pop, push
    '                   top, ToString
    ' 
    '         Sub: forEach, reduceKey
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports any = System.Object

Namespace ComponentModel.Collection

    ''' <summary>
    ''' a min priority queue backed by a pairing heap
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PriorityQueue(Of T)

        Dim root As PairingHeap(Of T)
        Dim lessThan As Func(Of T, T, Boolean)

        ''' <summary>
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

        ''' <summary>
        ''' the top element (the min element as defined by lessThan)
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

#Region "Microsoft.VisualBasic::3ae888f626a3b818319faa9b836de1a0, Microsoft.VisualBasic.Core\src\ComponentModel\System.Collections.Generic\BinaryHeap.vb"

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

    '   Total Lines: 178
    '    Code Lines: 118
    ' Comment Lines: 32
    '   Blank Lines: 28
    '     File Size: 6.05 KB


    '     Class BinaryHeap
    ' 
    '         Properties: peek, size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: pop, ToString
    ' 
    '         Sub: bubbleUp, push, remove, sinkDown
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports stdNum = System.Math

Namespace ComponentModel.Collection

    ''' <summary>
    ''' Binary heap implementation from:
    ''' 
    ''' > http://eloquentjavascript.net/appendix2.html
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class BinaryHeap(Of T As Class)

        ReadOnly content As New List(Of T)
        ReadOnly scoreFunction As Func(Of T, Double)

        Public ReadOnly Property size As Integer
            Get
                Return content.Count
            End Get
        End Property

        Default Public Property Item(i As Integer) As T
            Get
                If i >= content.Count Then
                    Return Nothing
                Else
                    Return content(i)
                End If
            End Get
            Set(value As T)
                content(i) = value
            End Set
        End Property

        Public ReadOnly Property peek As T
            Get
                If content.Count > 0 Then
                    Return content(Scan0)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New(scoreFunction As Func(Of T, Double))
            Me.scoreFunction = scoreFunction
        End Sub

        Public Overrides Function ToString() As String
            Return $"{size} items in current heap"
        End Function

        Public Sub push(element As T)
            ' Add the new element to the end of the array.
            ' Then allow it to bubble up.
            content.Add(element)
            bubbleUp(content.Count - 1)
        End Sub

        Public Function pop() As T
            ' Store the first element so we can return it later.
            ' Get the element at the end of the array.
            Dim result = content(Scan0)
            Dim [end] = content.Pop

            ' If there are any elements left, put the end element at the
            ' start, And let it sink down.
            If content > 0 Then
                content(Scan0) = [end]
                sinkDown(Scan0)
            End If

            Return result
        End Function

        Public Sub remove(node As T)
            Dim len = content.Count
            Dim [end] As T

            ' To remove a value, we must search through the array to find
            ' it.
            For i As Integer = 0 To len - 1
                If content(i) Is node Then
                    ' When it is found, the process seen in 'pop' is repeated
                    ' to fill up the hole.
                    [end] = content.Pop

                    If i <> len - 1 Then
                        content(i) = [end]

                        If scoreFunction([end]) < scoreFunction(node) Then
                            bubbleUp(i)
                        Else
                            sinkDown(i)
                        End If
                    End If

                    Return
                End If
            Next

            Throw New Exception("Node not found.")
        End Sub

        Private Sub bubbleUp(n As Integer)
            ' Fetch the element that has to be moved.
            Dim element = content(n)

            ' When at 0, an element can not go up any further.
            Do While n > 0
                ' Compute the parent element's index, and fetch it.
                Dim parentN% = stdNum.Floor((n + 1) / 2) - 1
                Dim parent = content(parentN)

                ' Swap the elements if the parent is greater.
                If scoreFunction(element) < scoreFunction(parent) Then
                    content(parentN) = element
                    content(n) = parent
                    ' Update 'n' to continue at the new position.
                    n = parentN
                Else
                    ' Found a parent that is less, no need to move it further.
                    Exit Do
                End If
            Loop
        End Sub

        Private Sub sinkDown(n As Integer)
            ' Look up the target element and its score.
            Dim length = content.Count
            Dim element = content(n)
            Dim elemScore = scoreFunction(element)
            Dim child1Score As Double
            Dim child1 As T

            Do While True
                ' Compute the indices of the child elements.
                Dim child2N = (n + 1) * 2, child1N = child2N - 1
                ' This is used to store the new position of the element,
                ' if any.
                Dim swap As Integer?

                ' If the first child exists (is inside the array)...
                If (child1N < length) Then
                    ' Look it up And compute its score.
                    child1 = content(child1N)
                    child1Score = scoreFunction(child1)

                    ' If the score Is less than our element's, we need to swap.
                    If child1Score < elemScore Then
                        swap = child1N
                    End If
                End If

                ' Do the same checks for the other child.
                If child2N < length Then
                    Dim child2 = content(child2N)
                    Dim child2Score = scoreFunction(child2)

                    If (child2Score < If(swap Is Nothing, elemScore, child1Score)) Then
                        swap = child2N
                    End If
                End If

                ' If the element needs to be moved, swap it, and continue.
                If Not swap Is Nothing Then
                    content(n) = content(swap)
                    content(swap) = element
                    n = swap
                Else
                    ' Otherwise, we are done.
                    Exit Do
                End If
            Loop
        End Sub
    End Class
End Namespace

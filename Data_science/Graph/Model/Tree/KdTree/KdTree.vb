#Region "Microsoft.VisualBasic::fe5b663293de45d49b865eb35839577e, Data_science\Graph\Model\Tree\KdTree\KdTree.vb"

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

    '     Class KdTree
    ' 
    '         Properties: balanceFactor
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: buildTree, count, findMax, findMin, height
    '                   innerSearch, insert, nearest, nodeSearch, remove
    ' 
    '         Sub: nearestSearch, removeNode, saveNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports stdNum = System.Math

Namespace KdTree

    Public Class KdTree

        Dim dimensions As Integer()
        Dim points As Object()
        Dim metric As Object

        Dim root As Node

        Public ReadOnly Property balanceFactor() As Double
            Get
                Return height(root) / (stdNum.Log(count(root)) / stdNum.Log(2))
            End Get
        End Property

        Sub New(points As Object(), metric As Object, dimensions As Integer())
            Me.points = points
            Me.metric = metric
            Me.dimensions = dimensions
            Me.root = buildTree(points, Scan0, Nothing)
        End Sub

        Private Function buildTree(points As Object(), depth As Integer, parent As Node) As Node
            Dim [dim] = depth Mod dimensions.Length
            Dim median As Integer
            Dim node As Node

            If points.Length = 0 Then
                Return Nothing
            ElseIf points.Length = 1 Then
                Return New Node(points(Scan0), [dim], parent)
            End If

            points.Sort(Function(a, b) As Integer
                            Return a(dimensions([dim])) - b(dimensions([dim]))
                        End Function)

            median = stdNum.Floor(points.Length / 2)
            node = New Node(points(median), [dim], parent)
            node.left = buildTree(points.slice(0, median), depth + 1, node)
            node.right = buildTree(points.slice(median + 1), depth + 1, node)

            Return node
        End Function

        Public Function insert(point) As Node
            Dim insertPosition = innerSearch(point, root, Nothing),
                newNode As Node,
                dimension As Object

            If insertPosition Is Nothing Then
                root = New Node(point, 0, Nothing)
                Return root
            Else
                newNode = New Node(point, (insertPosition.dimension + 1) Mod dimensions.Length, insertPosition)
                dimension = dimensions(insertPosition.dimension)
            End If

            If point(dimension) < insertPosition.obj(dimension) Then
                insertPosition.left = newNode
            Else
                insertPosition.right = newNode
            End If

            Return newNode
        End Function

        Private Function nodeSearch(point As Object, node As Node) As Node
            If node Is Nothing Then
                Return Nothing
            ElseIf node.obj Is point Then
                Return node
            End If

            Dim dimension = dimensions(node.dimension)

            If point(dimension) < node.obj(dimension) Then
                Return nodeSearch(node.left, node)
            Else
                Return nodeSearch(node.right, node)
            End If
        End Function

        Private Function innerSearch(point As Object, node As Node, parent As Node)
            If node Is Nothing Then
                Return parent
            End If

            Dim dimension = dimensions(node.dimension)

            If point(dimension) < node.obj(dimension) Then
                Return innerSearch(point, node.left, node)
            Else
                Return innerSearch(point, node.right, node)
            End If
        End Function

        Public Function remove(point) As Node
            Dim node = nodeSearch(point, root)

            If Not node Is Nothing Then
                Call removeNode(node)
            End If

            Return node
        End Function

        Private Sub removeNode(node As Node)
            Dim nextNode As Node
            Dim nextObj
            Dim pDimension

            If node.left Is Nothing AndAlso node.right Is Nothing Then
                If node.parent Is Nothing Then
                    root = Nothing
                    Return
                End If

                pDimension = dimensions(node.parent.dimension)

                If node.obj(pDimension) < node.parent.obj(pDimension) Then
                    node.parent.left = Nothing
                Else
                    node.parent.right = Nothing
                End If

                Return
            End If

            If Not node.left Is Nothing Then
                nextNode = findMax(node.left, node.dimension)
            Else
                nextNode = findMin(node.right, node.dimension)
            End If

            nextObj = nextNode.obj
            removeNode(nextNode)
            node.obj = nextObj
        End Sub

        Private Function findMax(node As Node, [dim] As Integer)
            Dim dimension As Integer
            Dim own
            Dim Left, Right, max As Node

            If node Is Nothing Then
                Return Nothing
            Else
                dimension = dimensions([dim])
            End If

            If node.dimension = [dim] Then
                If Not node.right Is Nothing Then
                    Return findMax(node.right, [dim])
                Else
                    Return node
                End If
            End If

            own = node.obj(dimension)
            Left = findMax(node.left, [dim])
            Right = findMax(node.right, [dim])
            max = node

            If Not Left Is Nothing AndAlso Left.obj(dimension) > own Then
                max = Left
            End If
            If Not Right Is Nothing AndAlso Right.obj(dimension) > max.obj(dimension) Then
                max = Right
            End If

            Return max
        End Function

        Private Function findMin(node As Node, [dim] As Integer)
            Dim dimension As Integer
            Dim own
            Dim Left,
            Right,
            min As Node

            If node Is Nothing Then
                Return Nothing
            End If

            dimension = dimensions([dim])

            If node.dimension = [dim] Then
                If Not node.left Is Nothing Then
                    Return findMin(node.left, [dim])
                Else
                    Return node
                End If
            End If

            own = node.obj(dimension)
            Left = findMin(node.left, [dim])
            Right = findMin(node.right, [dim])
            min = node

            If Not Left Is Nothing AndAlso Left.obj(dimension) < own Then
                min = Left
            End If

            If Not Right Is Nothing AndAlso Right.obj(dimension) < min.obj(dimension) Then
                min = Right
            End If

            Return min
        End Function

        Public Function nearest(point As Object, maxNodes As Integer, maxDistance As Double)
            Dim i%
            Dim result As New List(Of Object)
            Dim bestNodes As New BinaryHeap(Of Tuple(Of Node, Double))(Function(e) -e.Item2)

            If maxDistance Then
                For i = 0 To maxNodes - 1
                    bestNodes.push(New Tuple(Of Node, Double)(Nothing, maxDistance))
                Next
            End If

            nearestSearch(point, root, bestNodes, maxNodes)

            For i = 0 To maxNodes - 1
                If Not bestNodes(i) Is Nothing Then
                    result.Add(New Tuple(Of Node, Double)(bestNodes(i).Item1.obj, bestNodes(i).Item2))
                End If
            Next

            Return result
        End Function

        Private Sub nearestSearch(point As Object, node As Node, bestNodes As BinaryHeap(Of Tuple(Of Node, Double)), maxNodes%)
            Dim bestChild
            Dim dimension = dimensions(node.dimension),
          ownDistance = metric(point, node.obj),
          linearPoint = New Object,
          linearDistance,
          otherChild,
          i

            For i = 0 To dimensions.Length - 1
                If i = node.dimension Then
                    linearPoint(dimensions(i)) = point(dimensions(i))
                Else
                    linearPoint(dimensions(i)) = node.obj(dimensions(i))
                End If
            Next

            linearDistance = metric(linearPoint, node.obj)

            If node.right Is Nothing AndAlso node.left Is Nothing Then
                If bestNodes.size < maxNodes OrElse ownDistance < bestNodes.peek().Item2 Then
                    saveNode(bestNodes, node, ownDistance, maxNodes)
                End If
                Return
            End If

            If node.right Is Nothing Then
                bestChild = node.left
            ElseIf node.left Is Nothing Then
                bestChild = node.right
            Else
                If point(dimension) < node.obj(dimension) Then
                    bestChild = node.left
                Else
                    bestChild = node.right
                End If
            End If

            nearestSearch(point, bestChild, bestNodes, maxNodes)

            If bestNodes.size() < maxNodes OrElse ownDistance < bestNodes.peek.Item2 Then
                saveNode(bestNodes, node, ownDistance, maxNodes)
            End If

            If bestNodes.size < maxNodes OrElse stdNum.Abs(linearDistance) < bestNodes.peek.Item2 Then
                If bestChild Is node.left Then
                    otherChild = node.right
                Else
                    otherChild = node.left
                End If

                If Not otherChild Is Nothing Then
                    nearestSearch(point, otherChild, bestNodes, maxNodes)
                End If
            End If
        End Sub

        Private Sub saveNode(bestNodes As BinaryHeap(Of Tuple(Of Node, Double)), node As Node, distance#, maxNodes%)
            bestNodes.push(New Tuple(Of Node, Double)(node, distance))
            If (bestNodes.size > maxNodes) Then
                bestNodes.pop()
            End If
        End Sub

        Private Function height(node As Node) As Integer
            If node Is Nothing Then
                Return 0
            Else
                Return stdNum.Max(height(node.left), height(node.right)) + 1
            End If
        End Function

        Private Function count(node As Node) As Integer
            If node Is Nothing Then
                Return 0
            Else
                Return count(node.left) + count(node.right) + 1
            End If
        End Function
    End Class
End Namespace

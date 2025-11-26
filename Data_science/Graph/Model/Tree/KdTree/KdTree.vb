#Region "Microsoft.VisualBasic::1ed8b10e1738631acb9020993683b87b, Data_science\Graph\Model\Tree\KdTree\KdTree.vb"

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

    '   Total Lines: 399
    '    Code Lines: 266 (66.67%)
    ' Comment Lines: 68 (17.04%)
    '    - Xml Docs: 72.06%
    ' 
    '   Blank Lines: 65 (16.29%)
    '     File Size: 14.90 KB


    '     Class KdTree
    ' 
    '         Properties: balanceFactor, counts, dimSize, rootNode
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: buildTree, count, findMax, findMin, GetPoints
    '                   GetPointSample, height, innerSearch, insert, nearest
    '                   nodeSearch, remove
    ' 
    '         Sub: nearestSearch, removeNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Python
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace KdTree

    ''' <summary>
    ''' KDTree is a class supporting KD-tree insertion, deletion, equality search,
    ''' range search, and nearest neighbor(s) using double-precision floating-point
    ''' keys. Splitting dimension is chosen naively, by depth modulo K. Semantics are
    ''' as follows:
    ''' 
    ''' + Two different keys containing identical numbers should retrieve the same
    ''' value from a given KD-tree. Therefore keys are cloned when a node is
    ''' inserted. 
    ''' + As with Hashtables, values inserted into a KD-tree are <I>not</I> cloned.
    ''' Modifying a value between insertion and retrieval will therefore modify the
    ''' value stored in the tree.
    ''' 
    ''' @author Simon Levy, Bjoern Heckel
    ''' @version %I%, %G%
    ''' @since JDK1.2
    ''' </summary>
    Public Class KdTree(Of T As New)

        ''' <summary>
        ''' 
        ''' </summary>
        Dim dimensions As String()
        Dim access As KdNodeAccessor(Of T)
        Dim root As KdTreeNode(Of T)
        Dim m_points As New List(Of T)

        Public ReadOnly Property balanceFactor() As Double
            Get
                Return height(root) / (std.Log(count(root)) / std.Log(2))
            End Get
        End Property

        ''' <summary>
        ''' how many dimensions we're working with here
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dimSize As Integer
            Get
                Return dimensions.Length
            End Get
        End Property

        ''' <summary>
        ''' total number of nodes 
        ''' </summary>
        ''' <returns>
        ''' count nodes number from the <see cref="root"/>
        ''' </returns>
        Public ReadOnly Property counts As Integer
            Get
                Return count(root)
            End Get
        End Property

        Public ReadOnly Property rootNode As KdTreeNode(Of T)
            Get
                Return root
            End Get
        End Property

        Sub New(target As IEnumerable(Of T), metric As KdNodeAccessor(Of T))
            ' 20220129 due to the reason of build tree processing
            ' will re-order the input array, so this may caused the 
            ' algorithm bugs when the analysis algorithm is related
            ' to the input sequence order.
            ' so we should break the array reference at here by
            ' calling toarray etxension method.
            Dim points As T() = target.ToArray

            Me.access = metric
            Me.dimensions = metric.GetDimensions
            Me.root = buildTree(points, Scan0, Nothing)
            Me.m_points.AddRange(points)
        End Sub

        Public Function GetPoints() As IEnumerable(Of T)
            Return m_points.AsEnumerable
        End Function

        Public Iterator Function GetPointSample(n As Integer) As IEnumerable(Of T)
            Dim i As Integer() = Enumerable _
                .Range(0, counts) _
                .Shuffles _
                .Take(n) _
                .ToArray

            For Each idx As Integer In i
                Yield m_points(idx)
            Next
        End Function

        Private Function buildTree(points As T(), depth As Integer, parent As KdTreeNode(Of T)) As KdTreeNode(Of T)
            Dim axis = depth Mod dimensions.Length
            Dim median As Integer
            Dim node As KdTreeNode(Of T)

            If points.Length = 0 Then
                Return Nothing
            ElseIf points.Length = 1 Then
                Return New KdTreeNode(Of T)(points(Scan0), axis, parent)
            Else
                ' sort by the axis dimensions
                points.Sort(Function(a, b) access(a, dimensions(axis)).CompareTo(access(b, dimensions(axis))))
                median = std.Floor(points.Length / 2)
            End If

            Dim left = points.slice(0, median).ToArray
            Dim right = points.slice(median + 1).ToArray

            node = New KdTreeNode(Of T)(points(median), axis, parent)
            node.left = buildTree(left, depth + 1, node)
            node.right = buildTree(right, depth + 1, node)

            Return node
        End Function

        Public Function insert(point As T) As KdTreeNode(Of T)
            Dim insertPosition = innerSearch(point, root, Nothing),
                newNode As KdTreeNode(Of T),
                dimension As String

            If insertPosition Is Nothing Then
                root = New KdTreeNode(Of T)(point, 0, Nothing)
                Return root
            Else
                newNode = New KdTreeNode(Of T)(point, (insertPosition.dimension + 1) Mod dimensions.Length, insertPosition)
                dimension = dimensions(insertPosition.dimension)
            End If

            If access.getByDimension(point, dimension) < access.getByDimension(insertPosition.data, dimension) Then
                insertPosition.left = newNode
            Else
                insertPosition.right = newNode
            End If

            Return newNode
        End Function

        Private Function nodeSearch(point As T, node As KdTreeNode(Of T)) As KdTreeNode(Of T)
            If node Is Nothing Then
                Return Nothing
            ElseIf access.nodeIs(node.data, point) Then
                Return node
            End If

            Dim dimension = dimensions(node.dimension)

            If access.getByDimension(point, dimension) < access.getByDimension(node.data, dimension) Then
                Return nodeSearch(node.left, node)
            Else
                Return nodeSearch(node.right, node)
            End If
        End Function

        Private Function innerSearch(point As T, node As KdTreeNode(Of T), parent As KdTreeNode(Of T)) As KdTreeNode(Of T)
            If node Is Nothing Then
                Return parent
            End If

            Dim dimension = dimensions(node.dimension)

            If access.getByDimension(point, dimension) < access.getByDimension(node.data, dimension) Then
                Return innerSearch(point, node.left, node)
            Else
                Return innerSearch(point, node.right, node)
            End If
        End Function

        Public Function remove(point As T) As KdTreeNode(Of T)
            Dim node = nodeSearch(point, root)

            If Not node Is Nothing Then
                Call removeNode(node)
            End If

            Return node
        End Function

        Private Sub removeNode(node As KdTreeNode(Of T))
            Dim nextNode As KdTreeNode(Of T)
            Dim nextObj As T
            Dim pDimension As String

            If node.left Is Nothing AndAlso node.right Is Nothing Then
                If node.parent Is Nothing Then
                    root = Nothing
                    Return
                End If

                pDimension = dimensions(node.parent.dimension)

                If access.getByDimension(node.data, pDimension) < access.getByDimension(node.parent.data, pDimension) Then
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

            nextObj = nextNode.data
            Call removeNode(nextNode)
            node.data = nextObj
            node.dimension = (node.parent.dimension + 1) Mod dimensions.Length  ' 更新维度
        End Sub

        Private Function findMax(node As KdTreeNode(Of T), [dim] As Integer) As KdTreeNode(Of T)
            Dim dimension As String
            Dim own As Double
            Dim Left, Right, max As KdTreeNode(Of T)

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

            own = access.getByDimension(node.data, dimension)
            Left = findMax(node.left, [dim])
            Right = findMax(node.right, [dim])
            max = node

            If Not Left Is Nothing AndAlso access.getByDimension(Left.data, dimension) > own Then
                max = Left
            End If
            If Not Right Is Nothing AndAlso access.getByDimension(Right.data, dimension) > access.getByDimension(max.data, dimension) Then
                max = Right
            End If

            Return max
        End Function

        Private Function findMin(node As KdTreeNode(Of T), [dim] As Integer) As KdTreeNode(Of T)
            Dim dimension As String
            Dim own As Double
            Dim left, Right, min As KdTreeNode(Of T)

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

            own = access.getByDimension(node.data, dimension)
            left = findMin(node.left, [dim])
            Right = findMin(node.right, [dim])
            min = node

            If Not left Is Nothing AndAlso access.getByDimension(left.data, dimension) < own Then
                min = left
            End If

            If Not Right Is Nothing AndAlso access.getByDimension(Right.data, dimension) < access.getByDimension(min.data, dimension) Then
                min = Right
            End If

            Return min
        End Function

        ''' <summary>
        ''' Find KD-tree node whose key is identical to key. Uses algorithm
        ''' translated from 352.srch.c of Gonnet &amp; Baeza-Yates.
        ''' </summary>
        ''' <param name="point">
        ''' key for KD-tree node
        ''' </param>
        ''' <param name="maxDistance"></param>
        ''' <param name="maxNodes">
        ''' k
        ''' </param>
        ''' <returns>KNN search result</returns>
        Public Iterator Function nearest(point As T, maxNodes As Integer, Optional maxDistance As Double? = Nothing) As IEnumerable(Of KdNodeHeapItem(Of T))
            Dim bestNodes As New List(Of KdNodeHeapItem(Of T))
            Dim query As New KdTreeNode(Of T)(point, 0, Nothing)

            ' 20210920 似乎在这里必须要保证足够大的采样集大小才可以找到正确的解
            Call nearestSearch(query, root, 0, bestNodes, maxNodes)

            For Each node As KdNodeHeapItem(Of T) In bestNodes
                If Not maxDistance Is Nothing Then
                    If node.distance <= maxDistance Then
                        Yield New KdNodeHeapItem(Of T)(node.node, node.distance)
                    End If
                Else
                    Yield New KdNodeHeapItem(Of T)(node.node, node.distance)
                End If
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="point">
        ''' the user query target point
        ''' </param>
        ''' <param name="node">
        ''' the parent node for search
        ''' </param>
        ''' <param name="result"></param>
        ''' <param name="maxNodes"></param>
        Private Sub nearestSearch(point As KdTreeNode(Of T),
                                  node As KdTreeNode(Of T),
                                  depth As Integer,
                                  result As List(Of KdNodeHeapItem(Of T)),
                                  maxNodes As Integer)

            Dim dimension As Integer = depth Mod dimensions.Length
            Dim axis As String = dimensions(dimension)
            Dim distance As Double = access.metric(point.data, node.data)
            Dim i As Integer
            Dim addNode As Boolean = False

            If result = 0 Then
                result.Add(New KdNodeHeapItem(Of T)(node, distance))
                addNode = True
            End If

            For i = 0 To result.Count - 1
                If distance < result(i).distance Then
                    Exit For
                End If
            Next

            ' splice in our result
            If i >= 0 AndAlso i <= maxNodes AndAlso Not addNode Then
                result.Insert(i, New KdNodeHeapItem(Of T)(node, distance))
            End If

            ' get rid of any extra results
            Do While result > maxNodes
                result.Pop()
            Loop

            ' whats got the got best _search result? left or right?
            Dim goLeft = access(point.data, axis) < access(node.data, axis)
            Dim target = If(goLeft, node.left, node.right)
            Dim opposite = If(goLeft, node.right, node.left)

            ' target has our most likely nearest point, we go down that side of the
            ' tree first
            If Not target Is Nothing Then
                Call nearestSearch(point, target, depth + 1, result, maxNodes)
            End If

            ' _search the opposite direction, only if there is potentially a better
            ' value than the longest distance we already have in our _search results
            If Not opposite Is Nothing AndAlso opposite.distanceSquared(point.data, access) <= result(result.Count - 1).distance Then
                Call nearestSearch(point, opposite, depth + 1, result, maxNodes)
            End If
        End Sub

        Private Function height(node As KdTreeNode(Of T)) As Integer
            If node Is Nothing Then
                Return 0
            Else
                Return std.Max(height(node.left), height(node.right)) + 1
            End If
        End Function

        Private Function count(node As KdTreeNode(Of T)) As Integer
            If node Is Nothing Then
                Return 0
            Else
                Return count(node.left) + count(node.right) + 1
            End If
        End Function
    End Class
End Namespace

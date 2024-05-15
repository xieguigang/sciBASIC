#Region "Microsoft.VisualBasic::f511fd972828adb3075654c4ee2e030f, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\RBTree\RBTree.vb"

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

    '   Total Lines: 382
    '    Code Lines: 242
    ' Comment Lines: 72
    '   Blank Lines: 68
    '     File Size: 13.14 KB


    '     Class RBTree
    ' 
    '         Properties: Iterator, max, min
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _bound, doubleRotate, find, findIter, Insert
    '                   lowerBound, Remove, singleRotate, upperBound
    ' 
    '         Sub: [each], reach
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Algorithm.BinaryTree

    Public Class RBTree(Of K, V) : Inherits TreeBase(Of K, V)

        ''' <summary>
        ''' returns a null iterator call <see cref="Iterator(Of K, V).next"/> or 
        ''' <see cref="Iterator(Of K, V).prev()"/> to point to an element
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Iterator As Iterator(Of K, V)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Iterator(Of K, V)(Me)
            End Get
        End Property

        ''' <summary>
        ''' returns null if tree is empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property min() As V
            Get
                Dim res = Me._root
                If res Is Nothing Then
                    Return Nothing
                End If

                While res.Left IsNot Nothing
                    res = res.Left
                End While

                Return res.Value
            End Get
        End Property

        ''' <summary>
        ''' returns null if tree is empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property max() As V
            Get
                Dim res As RBNode(Of K, V) = Me._root
                If res Is Nothing Then
                    Return Nothing
                End If

                While res.Right IsNot Nothing
                    res = res.Right
                End While

                Return res.Value
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            MyBase.New(compares, views)
        End Sub

        ''' <summary>
        ''' returns true if inserted, false if duplicate
        ''' </summary>
        ''' <param name="key"></param>
        ''' <param name="data"></param>
        Public Function Insert(key As K, data As V) As Boolean
            Dim ret As Boolean = False

            If _root Is Nothing Then
                _root = New RBNode(Of K, V)(key, data,, views)
                ret = True
                stack.Add(_root)
            Else
                Dim head As New RBNode(Of K, V)(Nothing, Nothing)
                Dim dir As Boolean = False
                Dim last As Boolean = False
                Dim gp As RBNode(Of K, V) = Nothing
                Dim ggp = head
                Dim p As RBNode(Of K, V) = Nothing
                Dim node As RBNode(Of K, V) = root

                ggp.Right = root

                ' search down
                While True
                    If node Is Nothing Then
                        ' insert new node at the bottom
                        node = New RBNode(Of K, V)(key, data,, views)
                        p.Child(dir) = node
                        ret = True
                        Call stack.Add(node)
                    ElseIf RBNode(Of K, V).IsRed(node.Left) AndAlso RBNode(Of K, V).IsRed(node.Right) Then
                        ' color flip
                        node.Red = True
                        DirectCast(node.Left, RBNode(Of K, V)).Red = False
                        DirectCast(node.Right, RBNode(Of K, V)).Red = False
                    End If

                    ' fix red violation
                    If RBNode(Of K, V).IsRed(node) AndAlso RBNode(Of K, V).IsRed(p) Then
                        Dim dir2 = ggp.Right Is gp

                        If node Is p.Child(last) Then
                            ggp.Child(dir2) = singleRotate(gp, Not last)
                        Else
                            ggp.Child(dir2) = doubleRotate(gp, Not last)
                        End If
                    End If

                    Dim cmp = compares(node.Key, key)

                    ' stop if found
                    If cmp = 0 Then
                        Exit While
                    Else
                        last = dir
                        dir = cmp < 0
                    End If

                    If Not gp Is Nothing Then
                        ggp = gp
                    End If

                    gp = p
                    p = node
                    node = node.Child(dir)
                End While

                ' update root
                _root = head.Right
            End If

            ' make root black
            DirectCast(_root, RBNode(Of K, V)).Red = False

            Return ret
        End Function

        ''' <summary>
        ''' returns true if removed, false if not found
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function Remove(key As K) As Boolean
            If root Is Nothing Then
                Return False
            End If

            Dim head = New RBNode(Of K, V)(Nothing, Nothing,, views)
            Dim node = head
            node.Right = root

            Dim p As RBNode(Of K, V) = Nothing
            Dim gp As RBNode(Of K, V)
            Dim found As RBNode(Of K, V) = Nothing
            Dim dir As Boolean = True

            While Not node.Child(dir) Is Nothing
                Dim last = dir

                gp = p
                p = node
                node = node.Child(dir)

                Dim cmp = compares(key, node.Key)
                dir = cmp > 0

                ' save found node
                If cmp = 0 Then
                    found = node
                End If

                ' push the red node down
                If Not RBNode(Of K, V).IsRed(node) AndAlso Not RBNode(Of K, V).IsRed(node.Child(dir)) Then
                    If RBNode(Of K, V).IsRed(node.Child(Not dir)) Then
                        Dim sr = singleRotate(node, dir)
                        p.Child(last) = sr
                        p = sr
                    ElseIf Not RBNode(Of K, V).IsRed(node.Child(Not dir)) Then
                        Dim sibling = p.Child(Not last)

                        If Not sibling Is Nothing Then
                            If Not RBNode(Of K, V).IsRed(sibling.Child(Not last)) AndAlso Not RBNode(Of K, V).IsRed(sibling.Child(last)) Then
                                ' color flip
                                p.Red = False
                                sibling.Red = True
                                node.Red = True
                            Else
                                Dim dir2 = gp.Right Is p

                                If RBNode(Of K, V).IsRed(sibling.Child(last)) Then
                                    gp.Child(dir2) = doubleRotate(p, last)
                                ElseIf RBNode(Of K, V).IsRed(sibling.Child(Not last)) Then
                                    gp.Child(dir2) = singleRotate(p, last)
                                End If

                                ' ensure correct coloring
                                Dim gpc = gp.Child(dir2)
                                gpc.Red = True
                                node.Red = True
                                DirectCast(gpc.Left, RBNode(Of K, V)).Red = False
                                DirectCast(gpc.Right, RBNode(Of K, V)).Red = False
                            End If
                        End If
                    End If
                End If
            End While

            ' replace and remove if found
            If Not found Is Nothing Then
                found.Value = node.Value
                found.Key = node.Key
                p.Child(p.Right Is node) = node.Child(node.Left Is Nothing)
            End If

            ' update root and make it black
            _root = head.Right

            If Not _root Is Nothing Then
                DirectCast(_root, RBNode(Of K, V)).Red = False
            End If

            Return Not found Is Nothing
        End Function

        ''' <summary>
        ''' returns node data if found, null otherwise
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function find(key As K) As V
            Dim res As RBNode(Of K, V) = Me._root

            While res IsNot Nothing
                Dim c As Integer = Me.compares(key, res.Key)

                If c = 0 Then
                    Return res.Value
                Else
                    res = res.Child(c > 0)
                End If
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' returns iterator to node if found, null otherwise
        ''' </summary>
        ''' <param name="key"></param>
        ''' <returns></returns>
        Public Function findIter(key As K) As Iterator(Of K, V)
            Dim res As RBNode(Of K, V) = Me._root
            Dim iter = Me.Iterator

            While res IsNot Nothing
                Dim c = Me.compares(key, res.Key)

                If c = 0 Then
                    iter._cursor = res
                    Return iter
                Else
                    iter._ancestors.Push(res)
                    res = res.Child(c > 0)
                End If
            End While

            Return Nothing
        End Function

        ''' <summary>
        ''' Returns an interator to the tree node immediately before (or at) the element
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function lowerBound(data As K) As Iterator(Of K, V)
            Return Me._bound(data, Me.compares)
        End Function

        ''' <summary>
        ''' Returns an interator to the tree node immediately after (or at) the element
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function upperBound(data As Object) As Iterator(Of K, V)
            Dim cmp = Me.compares
            Dim reverse_cmp = Function(a, b) cmp(b, a)

            Return Me._bound(data, reverse_cmp)
        End Function

        ''' <summary>
        ''' calls cb on each node's data, in order
        ''' </summary>
        ''' <param name="cb"></param>
        Public Sub [each](cb As Action(Of V))
            Dim it = Me.Iterator()
            Dim data As New Value(Of V)

            While (data = it.[next]()) IsNot Nothing
                cb(data)
            End While
        End Sub

        ''' <summary>
        ''' calls cb on each node's data, in reverse order
        ''' </summary>
        ''' <param name="cb"></param>
        Public Sub reach(cb As Action(Of V))
            Dim it = Me.Iterator()
            Dim data As New Value(Of V)

            While (data = it.prev()) IsNot Nothing
                cb(data)
            End While
        End Sub

        ''' <summary>
        ''' used for lowerBound and upperBound
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="cmp"></param>
        ''' <returns></returns>
        Public Function _bound(data As K, cmp As Comparison(Of K)) As Iterator(Of K, V)
            Dim cur As RBNode(Of K, V) = Me._root
            Dim iter = Me.Iterator()

            While cur IsNot Nothing
                Dim c = Me.compares(data, cur.Key)

                If c = 0 Then
                    iter._cursor = cur
                    Return iter
                End If

                iter._ancestors.Push(cur)
                cur = cur.Child(c > 0)
            End While

            For i As Integer = iter._ancestors.Count - 1 To 0 Step -1
                cur = iter._ancestors(i)
                If cmp(data, cur.Key) > 0 Then
                    iter._cursor = cur

                    Do While iter._ancestors.Count > i
                        iter._ancestors.Pop()
                    Loop

                    Return iter
                End If
            Next

            iter._ancestors.Clear()

            Return iter
        End Function

        Private Shared Function singleRotate(root As RBNode(Of K, V), dir As Boolean) As RBNode(Of K, V)
            Dim save = root.Child(Not dir)

            root.Child(Not dir) = save.Child(dir)
            save.Child(dir) = root

            root.Red = True
            save.Red = False

            Return save
        End Function

        Private Shared Function doubleRotate(root As RBNode(Of K, V), dir As Boolean) As RBNode(Of K, V)
            root.Child(Not dir) = singleRotate(root.Child(Not dir), Not dir)
            Return singleRotate(root, dir)
        End Function
    End Class

End Namespace

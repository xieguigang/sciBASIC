#Region "Microsoft.VisualBasic::b34924e5c49c5cc1ad7b0d6159e96dd4, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\RBTree.vb"

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

    '     Class RBTree
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: doubleRotate, Insert, Remove, singleRotate
    ' 
    '     Class RBNode
    ' 
    '         Properties: Child, Red
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IsRed, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Class RBTree(Of K, V) : Inherits TreeBase(Of K, V)

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
                    ElseIf Not RBNode(Of K, v).IsRed(node.Child(Not dir)) Then
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

    Public Class RBNode(Of K, V) : Inherits BinaryTree(Of K, V)

        Public Property Red As Boolean

        Public Property Child(dir As Boolean) As RBNode(Of K, V)
            Get
                If dir Then
                    Return Right
                Else
                    Return Left
                End If
            End Get
            Set(value As RBNode(Of K, V))
                If dir Then
                    Right = value
                Else
                    Left = value
                End If
            End Set
        End Property

        Public Sub New(key As K, value As V, Optional parent As BinaryTree(Of K, V) = Nothing, Optional toString As Func(Of K, String) = Nothing)
            MyBase.New(key, value, parent, toString)
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{If(Red, "Red", "Black")}] {MyBase.ToString}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function IsRed(node As RBNode(Of K, V)) As Boolean
            Return Not node Is Nothing AndAlso node.Red
        End Function
    End Class
End Namespace

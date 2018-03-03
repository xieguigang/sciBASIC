#Region "Microsoft.VisualBasic::d2f914a165c75d52d6bf1be61d4f3402, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\AVLTree.vb"

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

    '     Class AVLTree
    ' 
    '         Properties: root
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Add, Remove
    ' 
    '         Sub: Add, appendLeft, appendRight, Remove, removeCurrent
    '              removeLeft, removeRight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="K"></typeparam>
    ''' <typeparam name="V"></typeparam>
    ''' <remarks>
    ''' http://www.cnblogs.com/huangxincheng/archive/2012/07/22/2603956.html
    ''' </remarks>
    Public Class AVLTree(Of K, V)

        ''' <summary>
        ''' The root node of this binary tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property root As BinaryTree(Of K, V)

        ReadOnly compares As Comparison(Of K)
        ReadOnly views As Func(Of K, String)

        Sub New(compares As Comparison(Of K), Optional views As Func(Of K, String) = Nothing)
            Me.compares = compares
            Me.views = views
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(key As K, value As V)
            _root = Add(key, value, _root)
        End Sub

        Public Function Add(key As K, value As V, tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            If tree Is Nothing Then
                tree = New BinaryTree(Of K, V)(key, value, Nothing, views)
            End If

            Select Case compares(key, tree.Key)
                Case < 0 : Call appendLeft(tree, key, value)
                Case > 0 : Call appendRight(tree, key, value)
                Case = 0

                    ' 将value追加到附加值中（也可对应重复元素）
                    tree.Value = value

                Case Else
                    ' This will never happend!
                    Throw New Exception("????")
            End Select

            tree.PutHeight

            Return tree
        End Function

        Private Sub appendRight(ByRef tree As BinaryTree(Of K, V), key As K, value As V)
            tree.Right = Add(key, value, tree.Right)

            If tree.Right.height - tree.Left.height = 2 Then
                If compares(key, tree.Right.Key) > 0 Then
                    tree = tree.RotateRR
                Else
                    tree = tree.RotateRL
                End If
            End If
        End Sub

        Private Sub appendLeft(ByRef tree As BinaryTree(Of K, V), key As K, value As V)
            tree.Left = Add(key, value, tree.Left)

            If tree.Left.height - tree.Right.height = 2 Then
                If compares(key, tree.Left.Key) < 0 Then
                    tree = tree.RotateLL
                Else
                    tree = tree.RotateLR
                End If
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Remove(key As K)
            _root = Remove(key, _root)
        End Sub

        Public Function Remove(key As K, tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            If tree Is Nothing Then
                Return Nothing
            End If

            Select Case compares(key, tree.Key)
                Case < 0 : Call removeLeft(tree, key)
                Case > 0 : Call removeRight(tree, key)
                Case = 0 : Call removeCurrent(tree)
                Case Else
                    Throw New Exception
            End Select

            If Not tree Is Nothing Then
                Call tree.PutHeight
            End If

            Return tree
        End Function

        Private Sub removeLeft(ByRef tree As BinaryTree(Of K, V), key As K)
            tree.Left = Remove(key, tree.Left)

            If tree.Left.height - tree.Right.height = 2 Then
                If compares(key, tree.Left.Key) < 0 Then
                    tree = tree.RotateLL
                Else
                    tree = tree.RotateLR
                End If
            End If
        End Sub

        Private Sub removeRight(ByRef tree As BinaryTree(Of K, V), key As K)
            tree.Right = Remove(key, tree.Right)

            If tree.Right.height - tree.Left.height = 2 Then
                If compares(key, tree.Right.Key) > 0 Then
                    tree = tree.RotateRR
                Else
                    tree = tree.RotateRL
                End If
            End If
        End Sub

        Private Sub removeCurrent(ByRef tree As BinaryTree(Of K, V))
            If Not tree.Left Is Nothing AndAlso Not tree.Right Is Nothing Then

                tree = New BinaryTree(Of K, V)(tree.Right.MinKey, tree.Value) With {
                    .Left = tree.Left,
                    .Right = tree.Right
                }
                tree.Right = Remove(tree.Key, tree.Right)

            Else
                tree = If(tree.Left Is Nothing, tree.Right, tree.Left)

                If tree Is Nothing Then
                    tree = Nothing
                End If
            End If
        End Sub
    End Class
End Namespace

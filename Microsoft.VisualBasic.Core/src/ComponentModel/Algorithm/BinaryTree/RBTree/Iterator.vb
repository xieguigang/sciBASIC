#Region "Microsoft.VisualBasic::b5d416823ceb889f45ddda51bb3399da, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\RBTree\Iterator.vb"

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

    '   Total Lines: 115
    '    Code Lines: 93
    ' Comment Lines: 13
    '   Blank Lines: 9
    '     File Size: 4.14 KB


    '     Class Iterator
    ' 
    '         Properties: data
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [next], prev
    ' 
    '         Sub: _maxNode, _minNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.BinaryTree

    Public Class Iterator(Of K, V)

        Public _tree As RBTree(Of K, V)
        Public _ancestors As Stack(Of RBNode(Of K, V))
        Public _cursor As RBNode(Of K, V)

        Public ReadOnly Property data() As V
            Get
                Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
            End Get
        End Property

        Default Public ReadOnly Property GetDirectionValue(dir As String) As Func(Of V)
            Get
                Select Case dir
                    Case NameOf(prev)
                        Return AddressOf prev
                    Case NameOf([next])
                        Return AddressOf [next]
                    Case Else
                        Throw New NotImplementedException
                End Select
            End Get
        End Property

        Public Sub New(tree As RBTree(Of K, V))
            Me._tree = tree
            Me._ancestors = New Stack(Of RBNode(Of K, V))
            Me._cursor = Nothing
        End Sub

        ''' <summary>
        ''' if null-iterator, returns first node
        ''' otherwise, returns next node
        ''' </summary>
        ''' <returns></returns>
        Public Function [next]() As V
            If Me._cursor Is Nothing Then
                Dim root = Me._tree.root
                If root IsNot Nothing Then
                    Me._minNode(root)
                End If
            Else
                If Me._cursor.Right Is Nothing Then
                    ' no greater node in subtree, go up to parent
                    ' if coming from a right child, continue up the stack
                    Dim save
                    Do
                        save = Me._cursor
                        If Me._ancestors.Count > 0 Then
                            Me._cursor = Me._ancestors.Pop()
                        Else
                            Me._cursor = Nothing
                            Exit Do
                        End If
                    Loop While Me._cursor.Right = save
                Else
                    ' get the next node from the subtree
                    Me._ancestors.Push(Me._cursor)
                    Me._minNode(Me._cursor.Right)
                End If
            End If
            Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
        End Function

        ''' <summary>
        ''' if null-iterator, returns last node
        ''' otherwise, returns previous node
        ''' </summary>
        ''' <returns></returns>
        Public Function prev() As V
            If Me._cursor Is Nothing Then
                Dim root = Me._tree.root
                If root IsNot Nothing Then
                    Me._maxNode(root)
                End If
            Else
                If Me._cursor.Left Is Nothing Then
                    Dim save
                    Do
                        save = Me._cursor
                        If Me._ancestors.Count > 0 Then
                            Me._cursor = Me._ancestors.Pop()
                        Else
                            Me._cursor = Nothing
                            Exit Do
                        End If
                    Loop While Me._cursor.Left = save
                Else
                    Me._ancestors.Push(Me._cursor)
                    Me._maxNode(Me._cursor.Left)
                End If
            End If
            Return If(Me._cursor IsNot Nothing, Me._cursor.Value, Nothing)
        End Function

        Public Sub _minNode(start As RBNode(Of K, V))
            While start.Left IsNot Nothing
                Me._ancestors.Push(start)
                start = start.Left
            End While
            Me._cursor = start
        End Sub

        Public Sub _maxNode(start As RBNode(Of K, V))
            While start.Right IsNot Nothing
                Me._ancestors.Push(start)
                start = start.Right
            End While
            Me._cursor = start
        End Sub
    End Class
End Namespace

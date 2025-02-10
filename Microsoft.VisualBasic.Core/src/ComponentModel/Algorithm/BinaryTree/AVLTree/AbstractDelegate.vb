#Region "Microsoft.VisualBasic::456722e6dfc9b2eb38a5f33de95e4b50, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\AVLTree\AbstractDelegate.vb"

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

    '   Total Lines: 58
    '    Code Lines: 37 (63.79%)
    ' Comment Lines: 7 (12.07%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 14 (24.14%)
    '     File Size: 2.10 KB


    '     Delegate Sub
    ' 
    ' 
    '     Class TreeInsertCallback
    ' 
    ' 
    ' 
    '     Class DelegateTreeInsertCallback
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: InsertDuplicated, InsertLeft, InsertRight
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.BinaryTree

    Public Delegate Sub TreeKeyInsertHandler(Of K, V)(keyNode As BinaryTree(Of K, V), newValue As V)

    Public MustInherit Class TreeInsertCallback(Of K, V)

        Public MustOverride Sub InsertDuplicated(keyNode As BinaryTree(Of K, V), newValue As V)
        Public MustOverride Sub InsertRight(keyNode As BinaryTree(Of K, V), newValue As V)
        Public MustOverride Sub InsertLeft(keyNode As BinaryTree(Of K, V), newValue As V)

    End Class

    Public Class DelegateTreeInsertCallback(Of K, V) : Inherits TreeInsertCallback(Of K, V)

        ' default nothing means do nothing

        ''' <summary>
        ''' usually for add cluster member into cluster
        ''' </summary>
        Public m_duplicated As TreeKeyInsertHandler(Of K, V)
        Public m_right As TreeKeyInsertHandler(Of K, V)
        Public m_left As TreeKeyInsertHandler(Of K, V)

        ''' <summary>
        ''' default is do nothing
        ''' </summary>
        Sub New()
        End Sub

        Sub New(Optional left As TreeKeyInsertHandler(Of K, V) = Nothing,
                Optional right As TreeKeyInsertHandler(Of K, V) = Nothing,
                Optional duplicated As TreeKeyInsertHandler(Of K, V) = Nothing)

            m_left = left
            m_right = right
            m_duplicated = duplicated
        End Sub

        Public Overrides Sub InsertDuplicated(keyNode As BinaryTree(Of K, V), newValue As V)
            If Not m_duplicated Is Nothing Then
                Call m_duplicated(keyNode, newValue)
            End If
        End Sub

        Public Overrides Sub InsertRight(keyNode As BinaryTree(Of K, V), newValue As V)
            If Not m_right Is Nothing Then
                Call m_right(keyNode, newValue)
            End If
        End Sub

        Public Overrides Sub InsertLeft(keyNode As BinaryTree(Of K, V), newValue As V)
            If Not m_left Is Nothing Then
                Call m_left(keyNode, newValue)
            End If
        End Sub
    End Class

End Namespace

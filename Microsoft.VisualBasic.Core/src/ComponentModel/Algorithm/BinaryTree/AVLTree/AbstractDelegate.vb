#Region "Microsoft.VisualBasic::5e05928f728e7b6abbf93b2478140100, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\AVLTree\AbstractDelegate.vb"

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

    '   Total Lines: 25
    '    Code Lines: 14 (56.00%)
    ' Comment Lines: 6 (24.00%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 5 (20.00%)
    '     File Size: 776 B


    '     Delegate Sub
    ' 
    ' 
    '     Class DelegateTreeInsertCallback
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Algorithm.BinaryTree

    Public Delegate Sub TreeKeyInsertHandler(Of K, V)(keyNode As BinaryTree(Of K, V), newValue As V)

    Public Class DelegateTreeInsertCallback(Of K, V)

        ''' <summary>
        ''' usually for add cluster member into cluster
        ''' </summary>
        Public insertDuplicated As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing 
            End Sub
        Public insertRight As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing
            End Sub
        Public insertLeft As TreeKeyInsertHandler(Of K, V) =
            Sub(tree, key)
                ' do nothing
            End Sub

    End Class

End Namespace

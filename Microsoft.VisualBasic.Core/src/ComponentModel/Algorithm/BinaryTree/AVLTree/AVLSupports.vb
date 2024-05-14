#Region "Microsoft.VisualBasic::227c0a8571cc6741b26b6a4117c59ea2, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\AVLTree\AVLSupports.vb"

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

    '   Total Lines: 61
    '    Code Lines: 44
    ' Comment Lines: 3
    '   Blank Lines: 14
    '     File Size: 1.86 KB


    '     Module AVLSupports
    ' 
    '         Function: height, RotateLL, RotateLR, RotateRL, RotateRR
    ' 
    '         Sub: PutHeight
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace ComponentModel.Algorithm.BinaryTree

    ''' <summary>
    ''' Binary tree balance helper
    ''' </summary>
    Public Module AVLSupports

        <Extension>
        Public Function RotateLL(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Dim top = node.Left

            node.Left = top.Right
            top.Right = node

            node.PutHeight
            top.PutHeight

            Return top
        End Function

        <Extension>
        Public Function RotateRR(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Dim top = node.Right

            node.Right = top.Left
            top.Left = node

            Call node.PutHeight
            Call top.PutHeight

            Return top
        End Function

        <Extension>
        Public Function RotateLR(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            node.Left = node.Left.RotateRR
            Return node.RotateLL
        End Function

        <Extension>
        Public Function RotateRL(Of K, V)(node As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            node.Right = node.Right.RotateLL
            Return node.RotateRR
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function height(Of K, V)(node As BinaryTree(Of K, V)) As Double
            Return If(node Is Nothing, -1, CDbl(node!height))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub PutHeight(Of K, V)(node As BinaryTree(Of K, V))
            node.SetValue("height", stdNum.Max(node.Left.height, node.Right.height) + 1)
        End Sub
    End Module
End Namespace

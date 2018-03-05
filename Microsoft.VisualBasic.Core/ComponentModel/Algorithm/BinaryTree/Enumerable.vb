#Region "Microsoft.VisualBasic::6ae578dfd9270db35bb59126fc69449c, Microsoft.VisualBasic.Core\ComponentModel\Algorithm\BinaryTree\Enumerable.vb"

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

    '     Module Enumerable
    ' 
    '         Function: PopulateSequence
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    Public Module Enumerable

        ''' <summary>
        ''' Populate an ASC sortted sequence from this binary tree 
        ''' 
        ''' ```
        ''' left -> me -> right
        ''' ```
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function PopulateSequence(Of K, V)(tree As BinaryTree(Of K, V)) As IEnumerable(Of Map(Of K, V))
            If Not tree.Left Is Nothing Then
                For Each node In tree.Left.PopulateSequence
                    Yield node
                Next
            End If

            Yield New Map(Of K, V)(tree.Key, tree.Value)

            If Not tree.Right Is Nothing Then
                For Each node In tree.Right.PopulateSequence
                    Yield node
                Next
            End If
        End Function
    End Module
End Namespace

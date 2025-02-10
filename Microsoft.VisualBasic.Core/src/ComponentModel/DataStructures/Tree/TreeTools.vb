#Region "Microsoft.VisualBasic::fd86041715fd3129ba4017a33d407855, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Tree\TreeTools.vb"

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

    '   Total Lines: 31
    '    Code Lines: 17 (54.84%)
    ' Comment Lines: 9 (29.03%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (16.13%)
    '     File Size: 965 B


    '     Module TreeTools
    ' 
    '         Function: EnumerateAllChilds
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.DataStructures.Tree

    ''' <summary>
    ''' extension method tools for abstract tree model
    ''' </summary>
    Public Module TreeTools

        ''' <summary>
        ''' enumerates all childs in current tree node
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="tree"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function EnumerateAllChilds(Of T As ITreeNodeData(Of T))(tree As ITreeNodeData(Of T)) As IEnumerable(Of T)
            If tree.ChildNodes Is Nothing Then
                Return
            End If

            For Each child As T In tree.ChildNodes
                Yield child

                For Each subchild As T In child.EnumerateAllChilds
                    Yield subchild
                Next
            Next
        End Function
    End Module
End Namespace

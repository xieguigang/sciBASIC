#Region "Microsoft.VisualBasic::6728f763b2c263e93848803114f1e89b, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Tree\ITreeNode.vb"

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

    '   Total Lines: 33
    '    Code Lines: 14 (42.42%)
    ' Comment Lines: 11 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (24.24%)
    '     File Size: 1020 B


    '     Interface ITreeNodeData
    ' 
    '         Properties: ChildNodes, FullyQualifiedName, IsLeaf, IsRoot, Parent
    ' 
    '     Interface ITreeNode
    ' 
    '         Function: GetRootNode
    ' 
    '         Sub: Add, ChildCountsTravel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.DataStructures.Tree

    Public Interface ITreeNodeData(Of T As ITreeNodeData(Of T))

        Property Parent() As T

        ''' <summary>
        ''' Children
        ''' </summary>
        ReadOnly Property ChildNodes() As IReadOnlyCollection(Of T)

        ''' <summary>
        ''' Is this node have no childs
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsLeaf() As Boolean
        ''' <summary>
        ''' I this node have no parents
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property IsRoot() As Boolean
        ReadOnly Property FullyQualifiedName() As String

    End Interface

    Public Interface ITreeNode(Of T As ITreeNode(Of T)) : Inherits ITreeNodeData(Of T)

        Function GetRootNode() As T
        Sub ChildCountsTravel(distribute As Dictionary(Of String, Double), Optional getID As Func(Of T, String) = Nothing)
        Sub Add(child As T)

    End Interface
End Namespace

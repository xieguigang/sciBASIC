#Region "Microsoft.VisualBasic::05cdec2923b16e095487d27a7a2cf5c1, Data_science\DataMining\UMAP\Components\Tree\RandomProjectionTreeNode.vb"

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

    '   Total Lines: 11
    '    Code Lines: 10
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 410 B


    '     Class RandomProjectionTreeNode
    ' 
    '         Properties: Hyperplane, Indices, IsLeaf, LeftChild, Offset
    '                     RightChild
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Tree

    Public NotInheritable Class RandomProjectionTreeNode
        Public Property IsLeaf As Boolean
        Public Property Indices As Integer()
        Public Property LeftChild As RandomProjectionTreeNode
        Public Property RightChild As RandomProjectionTreeNode
        Public Property Hyperplane As Double()
        Public Property Offset As Double
    End Class
End Namespace

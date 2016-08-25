#Region "Microsoft.VisualBasic::9c906dfaf2d8141be306e0fdc8b3ef92, ..\visualbasic_App\Datavisualization\Datavisualization.Network\Datavisualization.Network\TreeAPI\TREE.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree
Imports Microsoft.VisualBasic.Linq

Namespace TreeAPI

    Public Enum NodeTypes
        Path
        Leaf
        LeafX
        ROOT
    End Enum

    Public MustInherit Class TreeNode : Inherits TreeNode(Of NodeTypes)

        Public MustOverride Function GetEntities() As String()

        Sub New(parent As String, myType As NodeTypes)
            Call MyBase.New(parent, myType)
        End Sub
    End Class

    Public Class LeafX : Inherits TreeNode

        Public Property LeafX As FileStream.NetworkEdge()

        Sub New(parent As String)
            Call MyBase.New(parent & "-LeafX", NodeTypes.LeafX)
        End Sub

        Public Overrides Function GetEntities() As String()
            Return LeafX.ToArray(Function(x) x.ToNode)
        End Function
    End Class

    Public Class Leaf : Inherits TreeNode

        Sub New(parent As String)
            Call MyBase.New(parent & "-Leaf", NodeTypes.Leaf)
        End Sub

        Public Overrides Function GetEntities() As String()
            Return Me.GetEnumerator.ToArray(Function(x) x.Name)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::c82a487fd0c72e57d7159c1c08801ee2, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\LDM\FileStream\Node.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Abstract

Namespace FileStream

    ''' <summary>
    ''' An node entity in the target network.(这个对象里面包含了网络之中的节点的实体的最基本的定义：节点的标识符以及节点的类型)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Node : Inherits INetComponent
        Implements INamedValue
        Implements INode

        ''' <summary>
        ''' 这个节点的标识符
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Identifier As String Implements INamedValue.Key, INode.Identifier
        ''' <summary>
        ''' Node data groups identifier.(这个节点的分组类型的定义)
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property NodeType As String Implements INode.NodeType

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Sub New()
        End Sub

        Sub New(sId As String)
            Identifier = sId
        End Sub

        Sub New(sid As String, type As String)
            Call Me.New(sid)
            NodeType = type
        End Sub

        Public Function CopyTo(Of T As Node)() As T
            Dim NewNode As T = Activator.CreateInstance(Of T)()
            NewNode.Identifier = Identifier
            NewNode.NodeType = NodeType

            Return NewNode
        End Function
    End Class
End Namespace

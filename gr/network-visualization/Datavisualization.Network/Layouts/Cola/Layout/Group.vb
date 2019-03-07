#Region "Microsoft.VisualBasic::8ecaedc75669e69c04dd1c30460935a1, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout\Group.vb"

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

    '     Interface Indexed
    ' 
    '         Properties: id
    ' 
    '     Interface IGroup
    ' 
    '         Properties: groups, leaves
    ' 
    '     Class IndexGroup
    ' 
    '         Properties: groups, id, index, leaves, padding
    ' 
    '     Class Group
    ' 
    '         Properties: groups, leaves, padding
    ' 
    '         Function: isGroup
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    Public Interface Indexed
        Property id As Integer
    End Interface

    Public Interface IGroup(Of TGroups, TLeaves) : Inherits Indexed
        Property groups As List(Of TGroups)
        Property leaves As List(Of TLeaves)
    End Interface

    Public Class IndexGroup : Inherits JavaScriptObject
        Implements IGroup(Of Integer, Integer)

        Public Property leaves As List(Of Integer) Implements IGroup(Of Integer, Integer).leaves
        Public Property groups As List(Of Integer) Implements IGroup(Of Integer, Integer).groups
        Public Property id As Integer Implements IGroup(Of Integer, Integer).id
        Public Property padding As Double
        Public Property index As Integer
    End Class

    Public Class Group : Inherits Node
        Implements IGroup(Of Group, Node)

        Public Property groups As List(Of Group) Implements IGroup(Of Group, Node).groups
        Public Property leaves As List(Of Node) Implements IGroup(Of Group, Node).leaves
        Public Property padding As Double?

        Public Shared Function isGroup(g As Group) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function
    End Class

End Namespace

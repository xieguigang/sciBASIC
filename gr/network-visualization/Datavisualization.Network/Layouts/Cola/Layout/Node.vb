#Region "Microsoft.VisualBasic::064f88a98661b0cfface4cdf684fb6d3, gr\network-visualization\Datavisualization.Network\Layouts\Cola\Layout\Node.vb"

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

    '     Class InputNode
    ' 
    ' 
    ' 
    '     Class Node
    ' 
    '         Function: makeRBTree
    ' 
    '     Class Group
    ' 
    '         Function: isGroup
    ' 
    '     Class Link
    ' 
    '         Properties: length, source, target, weight
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class LinkLengthTypeAccessor
    ' 
    '         Function: [getType]
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.GridRouter
Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts.Cola

    Public Class InputNode

        ''' <summary>
        ''' index in nodes array, this is initialized by Layout.start()
        ''' </summary>
        Public index As Integer

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public x As Double

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public y As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public width As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public height As Double

        ''' <summary>
        ''' if fixed, layout will not move the node from its specified starting position
        ''' selective bit mask.  !=0 means layout will not move.
        ''' </summary>
        Public fixed As Double
    End Class

    ''' <summary>
    ''' Client-passed node may be missing these properties, which will be set
    ''' upon ingestion
    ''' </summary>
    Public Class Node : Inherits InputNode

        Public id As Integer
        Public name As String
        Public routerNode As Node
        Public prev As RBNode(Of Node, Object)
        Public [next] As RBNode(Of Node, Object)

        Public r As Rectangle2D
        Public bounds As Rectangle2D
        Public pos As Double
        Public _dragGroupOffsetY As Double
        Public _dragGroupOffsetX As Double
        Public px As Double?
        Public py As Double?
        Public parent As Group

        Public Shared Function makeRBTree() As RBNode(Of Node, Object)
            Return New RBNode(Of Node, Object)(Nothing, Nothing)
        End Function

    End Class

    Public Class Group

        Public routerNode As Group
        Public id As Integer
        Public bounds As Rectangle2D
        Public leaves As List(Of Node)
        Public groups As List(Of Group)
        Public padding As Double?
        Public parent As Group
        Public index As Integer

        Public ReadOnly Iterator Property keys As IEnumerable(Of String)
            Get

            End Get
        End Property

        Default Public Property PropertyValue(name As String) As Object
            Get

            End Get
            Set(value As Object)

            End Set
        End Property

        Public Shared Function isGroup(g As Group) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function
    End Class


    Public Class Link(Of NodeRefType)

        Public Property source() As NodeRefType
        Public Property target() As NodeRefType

        ''' <summary>
        ''' ideal length the layout should try to achieve for this link 
        ''' </summary>
        ''' <returns></returns>
        Public Property length() As Double

        ''' <summary>
        ''' how hard we should try to satisfy this link's ideal length
        ''' must be in the range: ``0 &lt; weight &lt;= 1``
        ''' if unspecified 1 is the default
        ''' </summary>
        ''' <returns></returns>
        Public Property weight() As Double
    End Class

    Public Delegate Function LinkNumericPropertyAccessor(t As Link(Of Node)) As Double

    Public Class LinkLengthTypeAccessor
        Inherits LinkLengthAccessor(Of Link(Of Node))
        Overloads Function [getType]() As LinkNumericPropertyAccessor

        End Function
    End Class
End Namespace

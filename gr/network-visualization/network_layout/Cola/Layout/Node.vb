#Region "Microsoft.VisualBasic::6fa8ef5db24de3b2524b5cd8175b9a02, gr\network-visualization\network_layout\Cola\Layout\Node.vb"

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

    '   Total Lines: 107
    '    Code Lines: 67 (62.62%)
    ' Comment Lines: 23 (21.50%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (15.89%)
    '     File Size: 4.18 KB


    '     Class InputNode
    ' 
    '         Properties: fixed, height, index, width, x
    '                     y
    ' 
    '     Class Node
    ' 
    '         Properties: bounds, fixed, fixedWeight, groups, height
    '                     id, leaves, padding, px, py
    '                     variable, width, x, y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: isGroup, makeRBTree, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Cola

    Public Class InputNode : Inherits JavaScriptObject

        ''' <summary>
        ''' index in nodes array, this is initialized by Layout.start()
        ''' </summary>
        Public Property index As Integer

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public Overridable Property x As Double

        ''' <summary>
        ''' x and y will be computed by layout as the Node's centroid
        ''' </summary>
        Public Overridable Property y As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public Overridable Property width As Double

        ''' <summary>
        ''' specify a width and height of the node's bounding box if you turn on avoidOverlaps
        ''' </summary>
        Public Overridable Property height As Double

        ''' <summary>
        ''' if fixed, layout will not move the node from its specified starting position
        ''' selective bit mask.  !=0 means layout will not move.
        ''' </summary>
        Public Overridable Property fixed As Double
    End Class

    ''' <summary>
    ''' Client-passed node may be missing these properties, which will be set
    ''' upon ingestion
    ''' </summary>
    Public Class Node : Inherits InputNode
        Implements Indexed
        Implements IGraphNode

        Public Overridable Property id As Integer Implements Indexed.id
        Public name As String
        Public routerNode As Node
        Public prev As RBTree(Of Integer, Node)
        Public [next] As RBTree(Of Integer, Node)
        Public children As Integer()
        Public r As Rectangle2D
        Public v As Variable
        Public pos As Double
        Public _dragGroupOffsetY As Double
        Public _dragGroupOffsetX As Double
        Public Property bounds As Rectangle2D Implements IGraphNode.bounds
        Public Property variable As Variable Implements IGraphNode.variable
        Public Overloads Property width As Double Implements IGraphNode.width
        Public Overloads Property height As Double Implements IGraphNode.height
        Public Property px As Double? Implements IGraphNode.px
        Public Property py As Double? Implements IGraphNode.py
        Public Overloads Property x As Double Implements IGraphNode.x
        Public Overloads Property y As Double Implements IGraphNode.y
        Public Overloads Property fixed As Boolean Implements IGraphNode.fixed
        Public Property fixedWeight As Double? Implements IGraphNode.fixedWeight
        Public parent As Node
        Public Property groups As List(Of [Variant](Of Integer, Node))
        Public Property leaves As List(Of [Variant](Of Integer, Node))
        Public Property padding As Double?

        Public Shared Function isGroup(g As Node) As Boolean
            Return g.leaves IsNot Nothing OrElse g.groups IsNot Nothing
        End Function

        Sub New()
        End Sub

        Sub New(v As Variable, r As Rectangle2D, pos As Double)
            Me.v = v
            Me.r = r
            Me.pos = pos

            prev = makeRBTree()
            [next] = makeRBTree()
        End Sub

        Public Overrides Function ToString() As String
            Return $"#{name}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function makeRBTree() As RBTree(Of Integer, Node)
            Return New RBTree(Of Integer, Node)(Function(x, y) x - y, Function(i) i.ToString)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(node As Node) As Point2D
            Return New Point2D(node.x, node.y)
        End Operator
    End Class
End Namespace

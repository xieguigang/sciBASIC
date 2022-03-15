#Region "Microsoft.VisualBasic::bd75460b1dc737cf591d83641a9a60ce, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\OrganicLayout\CellWrapper.vb"

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

    '   Total Lines: 71
    '    Code Lines: 18
    ' Comment Lines: 39
    '   Blank Lines: 14
    '     File Size: 2.54 KB


    '     Class CellWrapper
    ' 
    '         Properties: Cell, ConnectedEdges, HeightSquared, RadiusSquared, RelevantEdges
    '                     Source, Target, X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Layouts

    ''' <summary>
    ''' Internal representation of a node or edge that holds cached information
    ''' to enable the layout to perform more quickly and to simplify the code
    ''' </summary>
    Public Class CellWrapper

        Private ReadOnly outerInstance As mxOrganicLayout

        ''' <summary>
        ''' Constructs a new CellWrapper </summary>
        ''' <param name="cell"> the graph cell this wrapper represents </param>
        Public Sub New(ByVal outerInstance As mxOrganicLayout, ByVal cell As Object)
            Me.outerInstance = outerInstance
            Me.Cell = cell
        End Sub

        ''' <summary>
        ''' All edge that repel this cell, only used for nodes. This array
        ''' is equivalent to all edges unconnected to this node
        ''' </summary>
        Public Overridable Property RelevantEdges As Integer()

        ''' <summary>
        ''' the index of all connected edges in the <code>e</code> array
        ''' to this node. This is only used for nodes.
        ''' </summary>
        Public Overridable Property ConnectedEdges As Integer()

        ''' <summary>
        ''' The x-coordinate position of this cell, nodes only
        ''' </summary>
        Public Overridable Property X As Double

        ''' <summary>
        ''' The y-coordinate position of this cell, nodes only
        ''' </summary>
        Public Overridable Property Y As Double

        ''' <summary>
        ''' The approximate radius squared of this cell, nodes only. If
        ''' approxNodeDimensions is true on the layout this value holds the
        ''' width of the node squared
        ''' </summary>
        Public Overridable Property RadiusSquared As Double

        ''' <summary>
        ''' The height of the node squared, only used if approxNodeDimensions
        ''' is set to true.
        ''' </summary>
        Public Overridable Property HeightSquared As Double

        ''' <summary>
        ''' The index of the node attached to this edge as source, edges only
        ''' </summary>
        Public Overridable Property Source As Integer

        ''' <summary>
        ''' The index of the node attached to this edge as target, edges only
        ''' </summary>
        Public Overridable Property Target As Integer

        ''' <summary>
        ''' The actual graph cell this wrapper represents
        ''' </summary>
        Public Overridable Property Cell As Object

    End Class

End Namespace

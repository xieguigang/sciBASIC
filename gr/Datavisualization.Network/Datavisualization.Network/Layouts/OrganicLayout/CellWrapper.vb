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
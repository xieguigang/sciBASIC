Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts

    ''' <summary>
    ''' Abstract bass class for layouts
    ''' </summary>
    Public MustInherit Class mxGraphLayout

        ''' <summary>
        ''' The parent cell of the layout, if any
        ''' </summary>
        Protected Friend parent As Object

        ''' <summary>
        ''' Boolean indicating if the bounding box of the label should be used if
        ''' its available. Default is true.
        ''' </summary>
        Public Property useBoundingBox As Boolean = True

        ''' <summary>
        ''' Constructs a new fast organic layout for the specified graph.
        ''' </summary>
        Public Sub New(ByVal graph As NetworkGraph)
            Me.Graph = graph
        End Sub

        Public Sub execute(ByVal parent As Object)
            Me.parent = parent
        End Sub

        '	 (non-Javadoc)
        '	 * @see com.mxgraph.layout.mxIGraphLayout#move(java.lang.Object, double, double)
        '	 
        Public Sub moveCell(ByVal cell As Object, ByVal x As Double, ByVal y As Double)
            ' TODO: Map the position to a child index for
            ' the cell to be placed closest to the position
        End Sub

        ''' <summary>
        ''' Returns the associated graph.
        ''' </summary>
        Public Overridable Property Graph As NetworkGraph

        ''' <summary>
        ''' Returns the constraint for the given key and cell. This implementation
        ''' always returns the value for the given key in the style of the given
        ''' cell.
        ''' </summary>
        ''' <param name="key"> Key of the constraint to be returned. </param>
        ''' <param name="cell"> Cell whose constraint should be returned. </param>
        Public Overridable Function getConstraint(ByVal key As Object, ByVal cell As Object) As Object
            Return getConstraint(key, cell, Nothing, False)
        End Function

        ''' <summary>
        ''' Returns the constraint for the given key and cell. The optional edge and
        ''' source arguments are used to return inbound and outgoing routing-
        ''' constraints for the given edge and vertex. This implementation always
        ''' returns the value for the given key in the style of the given cell.
        ''' </summary>
        ''' <param name="key"> Key of the constraint to be returned. </param>
        ''' <param name="cell"> Cell whose constraint should be returned. </param>
        ''' <param name="edge"> Optional cell that represents the connection whose constraint
        ''' should be returned. Default is null. </param>
        ''' <param name="source"> Optional boolean that specifies if the connection is incoming
        ''' or outgoing. Default is false. </param>
        Public Overridable Function getConstraint(ByVal key As Object, ByVal cell As Object, ByVal edge As Object, ByVal source As Boolean) As Object
            Dim state As com.mxgraph.view.mxCellState = Graph.View.getState(cell)
            Dim style As IDictionary(Of String, Object) = If(state IsNot Nothing, state.Style, Graph.getCellStyle(cell))

            Return If(style IsNot Nothing, style(key), Nothing)
        End Function


        ''' <summary>
        ''' Returns true if the given vertex may be moved by the layout.
        ''' </summary>
        ''' <param name="vertex"> Object that represents the vertex to be tested. </param>
        ''' <returns> Returns true if the vertex can be moved. </returns>
        Public Overridable Function isVertexMovable(ByVal vertex As Object) As Boolean
            Return Graph.isCellMovable(vertex)
        End Function

        ''' <summary>
        ''' Returns true if the given vertex has no connected edges.
        ''' </summary>
        ''' <param name="vertex"> Object that represents the vertex to be tested. </param>
        ''' <returns> Returns true if the vertex should be ignored. </returns>
        Public Overridable Function isVertexIgnored(ByVal vertex As Object) As Boolean
            Return (Not Graph.Model.isVertex(vertex)) OrElse Not Graph.isCellVisible(vertex)
        End Function

        ''' <summary>
        ''' Returns true if the given edge has no source or target terminal.
        ''' </summary>
        ''' <param name="edge"> Object that represents the edge to be tested. </param>
        ''' <returns> Returns true if the edge should be ignored. </returns>
        Public Overridable Function isEdgeIgnored(ByVal edge As Object) As Boolean
            Dim model As com.mxgraph.model.mxIGraphModel = Graph.Model

            Return (Not model.isEdge(edge)) OrElse (Not Graph.isCellVisible(edge)) OrElse model.getTerminal(edge, True) Is Nothing OrElse model.getTerminal(edge, False) Is Nothing
        End Function

        ''' <summary>
        ''' Disables or enables the edge style of the given edge.
        ''' </summary>
        Public Overridable Sub setEdgeStyleEnabled(ByVal edge As Object, ByVal value As Boolean)
            Graph.setCellStyles(com.mxgraph.util.mxConstants.STYLE_NOEDGESTYLE, If(value, "0", "1"), New Object() {edge})
        End Sub

        ''' <summary>
        ''' Disables or enables orthogonal end segments of the given edge
        ''' </summary>
        Public Overridable Sub setOrthogonalEdge(ByVal edge As Object, ByVal value As Boolean)
            Graph.setCellStyles(com.mxgraph.util.mxConstants.STYLE_ORTHOGONAL, If(value, "1", "0"), New Object() {edge})
        End Sub

        Public Overridable Function getParentOffset(ByVal parent As Object) As com.mxgraph.util.mxPoint
            Dim result As New com.mxgraph.util.mxPoint

            If parent IsNot Nothing AndAlso parent IsNot Me.parent Then
                Dim model As com.mxgraph.model.mxIGraphModel = Graph.Model

                If model.isAncestor(Me.parent, parent) Then
                    Dim parentGeo As com.mxgraph.model.mxGeometry = model.getGeometry(parent)

                    Do While parent IsNot Me.parent
                        result.X = result.X + parentGeo.X
                        result.Y = result.Y + parentGeo.Y

                        parent = model.getParent(parent)
                        parentGeo = model.getGeometry(parent)
                    Loop
                End If
            End If

            Return result
        End Function

        ''' <summary>
        ''' Sets the control points of the given edge to the given
        ''' list of mxPoints. Set the points to null to remove all
        ''' existing points for an edge.
        ''' </summary>
        Public Overridable Sub setEdgePoints(ByVal edge As Object, ByVal points As IList(Of PointF))
            Dim model As com.mxgraph.model.mxIGraphModel = Graph.Model
            Dim geometry As com.mxgraph.model.mxGeometry = model.getGeometry(edge)

            If geometry Is Nothing Then
                geometry = New com.mxgraph.model.mxGeometry
                geometry.Relative = True
            Else
                geometry = CType(geometry.clone(), com.mxgraph.model.mxGeometry)
            End If

            If Me.parent IsNot Nothing AndAlso points IsNot Nothing Then
                Dim parent As Object = Graph.Model.getParent(edge)

                Dim ___parentOffset As com.mxgraph.util.mxPoint = getParentOffset(parent)

                For Each point As com.mxgraph.util.mxPoint In points
                    point.X = point.X - ___parentOffset.X
                    point.Y = point.Y - ___parentOffset.Y
                Next

            End If

            geometry.Points = points
            model.setGeometry(edge, geometry)
        End Sub

        ''' <summary>
        ''' Returns an <mxRectangle> that defines the bounds of the given cell
        ''' or the bounding box if <useBoundingBox> is true.
        ''' </summary>
        Public Overridable Function getVertexBounds(ByVal vertex As Object) As Rectangle
            Dim geo As com.mxgraph.util.mxRectangle = Graph.Model.getGeometry(vertex)

            ' Checks for oversize label bounding box and corrects
            ' the return value accordingly
            If useBoundingBox Then
                Dim state As com.mxgraph.view.mxCellState = Graph.View.getState(vertex)

                If state IsNot Nothing Then
                    Dim scale As Double = Graph.View.Scale
                    Dim tmp As com.mxgraph.util.mxRectangle = state.BoundingBox

                    Dim dx0 As Double = (tmp.X - state.X) / scale
                    Dim dy0 As Double = (tmp.Y - state.Y) / scale
                    Dim dx1 As Double = (tmp.X + tmp.Width - state.X - state.Width) / scale
                    Dim dy1 As Double = (tmp.Y + tmp.Height - state.Y - state.Height) / scale

                    geo = New com.mxgraph.util.mxRectangle(geo.X + dx0, geo.Y + dy0, geo.Width - dx0 + dx1, geo.Height + -dy0 + dy1)
                End If
            End If

            If Me.parent IsNot Nothing Then
                Dim parent As Object = Graph.Model.getParent(vertex)
                geo = CType(geo.clone(), com.mxgraph.util.mxRectangle)

                If parent IsNot Nothing AndAlso parent IsNot Me.parent Then
                    Dim ___parentOffset As com.mxgraph.util.mxPoint = getParentOffset(parent)
                    geo.X = geo.X + ___parentOffset.X
                    geo.Y = geo.Y + ___parentOffset.Y
                End If
            End If

            Return New com.mxgraph.util.mxRectangle(geo)
        End Function

        ''' <summary>
        ''' Sets the new position of the given cell taking into account the size of
        ''' the bounding box if <useBoundingBox> is true. The change is only carried
        ''' out if the new location is not equal to the existing location, otherwise
        ''' the geometry is not replaced with an updated instance. The new or old
        ''' bounds are returned (including overlapping labels).
        ''' 
        ''' Parameters:
        ''' 
        ''' cell - <mxCell> whose geometry is to be set.
        ''' x - Integer that defines the x-coordinate of the new location.
        ''' y - Integer that defines the y-coordinate of the new location.
        ''' </summary>
        Public Overridable Function setVertexLocation(ByVal vertex As Object, ByVal x As Double, ByVal y As Double) As Rectangle
            Dim model As com.mxgraph.model.mxIGraphModel = Graph.Model
            Dim geometry As com.mxgraph.model.mxGeometry = model.getGeometry(vertex)
            Dim result As com.mxgraph.util.mxRectangle = Nothing

            If geometry IsNot Nothing Then
                result = New com.mxgraph.util.mxRectangle(x, y, geometry.Width, geometry.Height)

                Dim graphView As NetworkGraphView = Graph.View

                ' Checks for oversize labels and offset the result
                If useBoundingBox Then
                    Dim state As com.mxgraph.view.mxCellState = graphView.getState(vertex)

                    If state IsNot Nothing Then
                        Dim scale As Double = Graph.View.Scale
                        Dim box As com.mxgraph.util.mxRectangle = state.BoundingBox

                        If state.BoundingBox.X < state.X Then
                            x += (state.X - box.X) / scale
                            result.Width = box.Width
                        End If
                        If state.BoundingBox.Y < state.Y Then
                            y += (state.Y - box.Y) / scale
                            result.Height = box.Height
                        End If
                    End If
                End If

                If Me.parent IsNot Nothing Then
                    Dim parent As Object = model.getParent(vertex)

                    If parent IsNot Nothing AndAlso parent IsNot Me.parent Then
                        Dim ___parentOffset As com.mxgraph.util.mxPoint = getParentOffset(parent)

                        x = x - ___parentOffset.X
                        y = y - ___parentOffset.Y
                    End If
                End If

                If geometry.X <> x OrElse geometry.Y <> y Then
                    geometry = CType(geometry.clone(), com.mxgraph.model.mxGeometry)
                    geometry.X = x
                    geometry.Y = y

                    model.setGeometry(vertex, geometry)
                End If
            End If

            Return result
        End Function

        ''' <summary>
        ''' Updates the bounds of the given groups to include all children. Call
        ''' this with the groups in parent to child order, top-most group first, eg.
        ''' 
        ''' arrangeGroups(graph, mxUtils.sortCells(Arrays.asList(
        '''   new Object[] { v1, v3 }), true).toArray(), 10); </summary>
        ''' <param name="groups"> the groups to adjust </param>
        ''' <param name="border"> the border applied to the adjusted groups </param>
        Public Overridable Sub arrangeGroups(ByVal groups As Object(), ByVal border As Integer)
            Graph.Model.beginUpdate()
            Try
                For i As Integer = groups.Length - 1 To 0 Step -1
                    Dim group As Object = groups(i)
                    Dim children As Object() = Graph.getChildVertices(group)
                    Dim bounds As com.mxgraph.util.mxRectangle = Graph.getBoundingBoxFromGeometry(children)

                    Dim geometry As com.mxgraph.model.mxGeometry = Graph.getCellGeometry(group)
                    Dim left As Double = 0
                    Dim top As Double = 0

                    ' Adds the size of the title area for swimlanes
                    If Me.Graph.isSwimlane(group) Then
                        Dim size As com.mxgraph.util.mxRectangle = Graph.getStartSize(group)
                        left = size.Width
                        top = size.Height
                    End If

                    If bounds IsNot Nothing AndAlso geometry IsNot Nothing Then
                        geometry = CType(geometry.clone(), com.mxgraph.model.mxGeometry)
                        geometry.X = geometry.X + bounds.X - border - left
                        geometry.Y = geometry.Y + bounds.Y - border - top
                        geometry.Width = bounds.Width + 2 * border + left
                        geometry.Height = bounds.Height + 2 * border + top
                        Graph.Model.setGeometry(group, geometry)
                        Graph.moveCells(children, border + left - bounds.X, border + top - bounds.Y)
                    End If
                Next
            Finally
                Graph.Model.endUpdate()
            End Try
        End Sub
    End Class

End Namespace
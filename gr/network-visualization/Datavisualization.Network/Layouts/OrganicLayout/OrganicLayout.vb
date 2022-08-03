#Region "Microsoft.VisualBasic::abe553f7dd21d4c40a98e949b07d1c8a, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\OrganicLayout\OrganicLayout.vb"

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

    '   Total Lines: 1110
    '    Code Lines: 478
    ' Comment Lines: 489
    '   Blank Lines: 143
    '     File Size: 55.13 KB


    '     Class mxOrganicLayout
    ' 
    '         Properties: ApproxNodeDimensions, AverageNodeArea, BorderLineCostFactor, DisableEdgeStyle, EdgeCrossingCostFactor
    '                     EdgeDistanceCostFactor, EdgeLengthCostFactor, FineTuning, FineTuningRadius, InitialMoveRadius
    '                     MaxDistanceLimit, MaxIterations, MinDistanceLimit, MinMoveRadius, NodeDistributionCostFactor
    '                     OptimizeBorderLine, OptimizeEdgeCrossing, OptimizeEdgeDistance, OptimizeEdgeLength, OptimizeNodeDistribution
    '                     RadiusScaleFactor, ResetEdges, TriesPerCell, UnchangedEnergyRoundTermination
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: calcEnergyDelta, getAdditionFactorsEnergy, getBorderline, getConnectedEdges, getEdgeCrossing
    '                   getEdgeCrossingAffectedEdges, getEdgeDistanceAffectedNodes, getEdgeDistanceFromEdge, getEdgeDistanceFromNode, getEdgeLength
    '                   getEdgeLengthAffectedEdges, getEdges, getNodeDistribution, getRelevantEdges, ToString
    ' 
    '         Sub: execute, performRound
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language.Java

' $Id: mxOrganicLayout.java,v 1.2 2012/12/22 22:39:40 david Exp $
' Copyright (c) 2007-2013, JGraph Ltd

Namespace Layouts

    ''' <summary>
    ''' An implementation of a simulated annealing layout, based on "Drawing Graphs
    ''' Nicely Using Simulated Annealing" by Davidson and Harel (1996). This
    ''' paper describes these criteria as being favourable in a graph layout: (1)
    ''' distributing nodes evenly, (2) making edge-lengths uniform, (3)
    ''' minimizing cross-crossings, and (4) keeping nodes from coming too close
    ''' to edges. These criteria are translated into energy cost functions in the
    ''' layout. Nodes or edges breaking these criteria create a larger cost function
    ''' , the total cost they contribute related to the extent that they break it.
    ''' The idea of the algorithm is to minimise the total system energy. Factors
    ''' are assigned to each of the criteria describing how important that
    ''' criteria is. Higher factors mean that those criteria are deemed to be
    ''' relatively preferable in the final layout. Most of  the criteria conflict
    ''' with the others to some extent and so the setting of the factors determines
    ''' the general look of the resulting graph.
    ''' 
    ''' In addition to the four aesthetic criteria the concept of a border line
    ''' which induces an energy cost to nodes in proximity to the graph bounds is
    ''' introduced to attempt to restrain the graph. All of the 5 factors can be
    ''' switched on or off using the <code>isOptimize...</code> variables.
    ''' 
    ''' Simulated Annealing is a force-directed layout and is one of the more
    ''' expensive, but generally effective layouts of this type. Layouts like
    ''' the spring layout only really factor in edge length and inter-node
    ''' distance being the lowest CPU intensive for the most aesthetic gain. The
    ''' additional factors are more expensive but can have very attractive results.
    ''' 
    ''' The main loop of the algorithm consist of processing the nodes in a 
    ''' deterministic order. During the processing of each node a circle of radius
    ''' <code>moveRadius</code> is made around the node and split into
    ''' <code>triesPerCell</code> equal segments. Each point between neighbour
    ''' segments is determined and the new energy of the system if the node were
    ''' moved to that position calculated. Only the necessary nodes and edges are
    ''' processed new energy values resulting in quadratic performance, O(VE),
    ''' whereas calculating the total system energy would be cubic. The default
    ''' implementation only checks 8 points around the radius of the circle, as
    ''' opposed to the suggested 30 in the paper. Doubling the number of points
    ''' double the CPU load and 8 works almost as well as 30.
    ''' 
    ''' The <code>moveRadius</code> replaces the temperature as the influencing
    ''' factor in the way the graph settles in later iterations. If the user does
    ''' not set the initial move radius it is set to half the maximum dimension
    ''' of the graph. Thus, in 2 iterations a node may traverse the entire graph,
    ''' and it is more sensible to find minima this way that uphill moves, which
    ''' are little more than an expensive 'tilt' method. The factor by which
    ''' the radius is multiplied by after each iteration is important, lowering
    ''' it improves performance but raising it towards 1.0 can improve the
    ''' resulting graph aesthetics. When the radius hits the minimum move radius
    ''' defined, the layout terminates. The minimum move radius should be set
    ''' a value where the move distance is too minor to be of interest.
    ''' 
    ''' Also, the idea of a fine tuning phase is used, as described in the paper.
    ''' This involves only calculating the edge to node distance energy cost
    ''' at the end of the algorithm since it is an expensive calculation and
    ''' it really an 'optimizating' function. <code>fineTuningRadius</code>
    ''' defines the radius value that, when reached, causes the edge to node
    ''' distance to be calculated.
    ''' 
    ''' There are other special cases that are processed after each iteration.
    ''' <code>unchangedEnergyRoundTermination</code> defines the number of
    ''' iterations, after which the layout terminates. If nothing is being moved
    ''' it is assumed a good layout has been found. In addition to this if
    ''' no nodes are moved during an iteration the move radius is halved, presuming
    ''' that a finer granularity is required.
    ''' 
    ''' </summary>
    Public Class mxOrganicLayout
        Inherits mxGraphLayout

        ''' <summary>
        ''' Whether or not the distance between edge and nodes will be calculated
        ''' as an energy cost function. This function is CPU intensive and is best
        ''' only used in the fine tuning phase.
        ''' </summary>
        Protected Friend ___isOptimizeEdgeDistance As Boolean = True

        ''' <summary>
        ''' Whether or not edges crosses will be calculated as an energy cost
        ''' function. This function is CPU intensive, though if some iterations
        ''' without it are required, it is best to have a few cycles at the start
        ''' of the algorithm using it, then use it intermittantly through the rest
        ''' of the layout.
        ''' </summary>
        Protected Friend ___isOptimizeEdgeCrossing As Boolean = True

        ''' <summary>
        ''' Whether or not edge lengths will be calculated as an energy cost
        ''' function. This function not CPU intensive.
        ''' </summary>
        Protected Friend ___isOptimizeEdgeLength As Boolean = True

        ''' <summary>
        ''' Whether or not nodes will contribute an energy cost as they approach
        ''' the bound of the graph. The cost increases to a limit close to the
        ''' border and stays constant outside the bounds of the graph. This function
        ''' is not CPU intensive
        ''' </summary>
        Protected Friend ___isOptimizeBorderLine As Boolean = True

        ''' <summary>
        ''' Whether or not node distribute will contribute an energy cost where
        ''' nodes are close together. The function is moderately CPU intensive.
        ''' </summary>
        Protected Friend ___isOptimizeNodeDistribution As Boolean = True

        ''' <summary>
        ''' The current radius around each node where the next position energy
        ''' values will be calculated for a possible move
        ''' </summary>
        Protected Friend moveRadius As Double

        ''' <summary>
        ''' The average amount of area allocated per node. If <code> bounds</code>
        ''' is not set this value mutiplied by the number of nodes to find
        ''' the total graph area. The graph is assumed square.
        ''' </summary>
        Public Overridable Property AverageNodeArea As Double = 160000

        ''' <summary>
        ''' Cost factor applied to energy calculations for node promixity to the
        ''' notional border of the graph. Increasing this value results in
        ''' nodes tending towards the centre of the drawing space, at the
        ''' partial cost of other graph aesthetics.
        ''' <code>isOptimizeBorderLine</code> must be true for border
        ''' repulsion to be applied.
        ''' </summary>
        Public Overridable Property BorderLineCostFactor As Double = 5

        ''' <summary>
        ''' Cost factor applied to energy calculations involving edges that cross
        ''' over one another. Increasing this value tends to result in fewer edge
        ''' crossings, at the partial cost of other graph aesthetics.
        ''' <code>isOptimizeEdgeCrossing</code> must be true for edge crossings
        ''' to be taken into account.
        ''' </summary>
        Public Overridable Property EdgeCrossingCostFactor As Double = 6000

        ''' <summary>
        ''' Cost factor applied to energy calculations involving the distance
        ''' nodes and edges. Increasing this value tends to cause nodes to move away
        ''' from edges, at the partial cost of other graph aesthetics.
        ''' <code>isOptimizeEdgeDistance</code> must be true for edge to nodes
        ''' distances to be taken into account.
        ''' </summary>
        Public Overridable Property EdgeDistanceCostFactor As Double = 3000

        ''' <summary>
        ''' Cost factor applied to energy calculations for the edge lengths.
        ''' Increasing this value results in the layout attempting to shorten all
        ''' edges to the minimum edge length, at the partial cost of other graph
        ''' aesthetics.
        ''' <code>isOptimizeEdgeLength</code> must be true for edge length
        ''' shortening to be applied.
        ''' </summary>
        Public Overridable Property EdgeLengthCostFactor As Double = 0.02

        ''' <summary>
        ''' The radius below which fine tuning of the layout should start
        ''' This involves allowing the distance between nodes and edges to be
        ''' taken into account in the total energy calculation. If this is set to
        ''' zero, the layout will automatically determine a suitable value
        ''' </summary>
        Public Overridable Property FineTuningRadius As Double = 40

        ''' <summary>
        ''' The initial value of <code>moveRadius</code>. If this is set to zero
        ''' the layout will automatically determine a suitable value.
        ''' </summary>
        Public Overridable Property InitialMoveRadius As Double

        ''' <returns> Returns the isFineTuning. </returns>
        Public Overridable Property FineTuning As Boolean
            Get
                Return ___isFineTuning
            End Get
            Set(ByVal isFineTuning As Boolean)
                Me.___isFineTuning = isFineTuning
            End Set
        End Property


        ''' <returns> Returns the isOptimizeBorderLine. </returns>
        Public Overridable Property OptimizeBorderLine As Boolean
            Get
                Return ___isOptimizeBorderLine
            End Get
            Set(ByVal isOptimizeBorderLine As Boolean)
                Me.___isOptimizeBorderLine = isOptimizeBorderLine
            End Set
        End Property


        ''' <returns> Returns the isOptimizeEdgeCrossing. </returns>
        Public Overridable Property OptimizeEdgeCrossing As Boolean
            Get
                Return ___isOptimizeEdgeCrossing
            End Get
            Set(ByVal isOptimizeEdgeCrossing As Boolean)
                Me.___isOptimizeEdgeCrossing = isOptimizeEdgeCrossing
            End Set
        End Property


        ''' <returns> Returns the isOptimizeEdgeDistance. </returns>
        Public Overridable Property OptimizeEdgeDistance As Boolean
            Get
                Return ___isOptimizeEdgeDistance
            End Get
            Set(ByVal isOptimizeEdgeDistance As Boolean)
                Me.___isOptimizeEdgeDistance = isOptimizeEdgeDistance
            End Set
        End Property


        ''' <returns> Returns the isOptimizeEdgeLength. </returns>
        Public Overridable Property OptimizeEdgeLength As Boolean
            Get
                Return ___isOptimizeEdgeLength
            End Get
            Set(ByVal isOptimizeEdgeLength As Boolean)
                Me.___isOptimizeEdgeLength = isOptimizeEdgeLength
            End Set
        End Property


        ''' <returns> Returns the isOptimizeNodeDistribution. </returns>
        Public Overridable Property OptimizeNodeDistribution As Boolean
            Get
                Return ___isOptimizeNodeDistribution
            End Get
            Set(ByVal isOptimizeNodeDistribution As Boolean)
                Me.___isOptimizeNodeDistribution = isOptimizeNodeDistribution
            End Set
        End Property

        ''' <summary>
        ''' Limit to the number of iterations that may take place. This is only
        ''' reached if one of the termination conditions does not occur first.
        ''' </summary>
        Public Overridable Property MaxIterations As Integer = 1000

        ''' <summary>
        ''' prevents from dividing with zero and from creating excessive energy
        ''' values
        ''' </summary>
        Public Overridable Property MinDistanceLimit As Double = 2

        ''' <summary>
        ''' when <seealso cref="moveRadius"/>reaches this value, the algorithm is terminated
        ''' </summary>
        Public Overridable Property MinMoveRadius As Double = 2

        ''' <summary>
        ''' Cost factor applied to energy calculations involving the general node
        ''' distribution of the graph. Increasing this value tends to result in
        ''' a better distribution of nodes across the available space, at the
        ''' partial cost of other graph aesthetics.
        ''' <code>isOptimizeNodeDistribution</code> must be true for this general
        ''' distribution to be applied.
        ''' </summary>
        Public Overridable Property NodeDistributionCostFactor As Double = 30000

        ''' <summary>
        ''' The factor by which the <code>moveRadius</code> is multiplied by after
        ''' every iteration. A value of 0.75 is a good balance between performance
        ''' and aesthetics. Increasing the value provides more chances to find
        ''' minimum energy positions and decreasing it causes the minimum radius
        ''' termination condition to occur more quickly.
        ''' </summary>
        Public Overridable Property RadiusScaleFactor As Double = 0.75

        ''' <summary>
        ''' determines, in how many segments the circle around cells is divided, to
        ''' find a new position for the cell. Doubling this value doubles the CPU
        ''' load. Increasing it beyond 16 might mean a change to the
        ''' <code>performRound</code> method might further improve accuracy for a
        ''' small performance hit. The change is described in the method comment.
        ''' </summary>
        Public Overridable Property TriesPerCell As Integer = 8

        ''' <summary>
        ''' The number of round of no node moves taking placed that the layout
        ''' terminates
        ''' </summary>
        Public Overridable Property UnchangedEnergyRoundTermination As Integer = 5

        ''' <summary>
        ''' distance limit beyond which energy costs due to object repulsive is
        ''' not calculated as it would be too insignificant
        ''' </summary>
        Public Overridable Property MaxDistanceLimit As Double = 100

        ''' <summary>
        ''' Whether or not to use approximate node dimensions or not. Set to true
        ''' the radius squared of the smaller dimension is used. Set to false the
        ''' radiusSquared variable of the CellWrapper contains the width squared
        ''' and heightSquared is used in the obvious manner.
        ''' </summary>
        Public Overridable Property ApproxNodeDimensions As Boolean = True

        ''' <summary>
        '''  Specifies if the STYLE_NOEDGESTYLE flag should be set on edges that are
        ''' modified by the result. Default is true.
        ''' </summary>
        Public Overridable Property DisableEdgeStyle As Boolean = True

        ''' <summary>
        ''' Specifies if all edge points of traversed edges should be removed.
        ''' Default is true.
        ''' </summary>
        Public Overridable Property ResetEdges As Boolean = True


        ''' <summary>
        ''' The x coordinate of the final graph
        ''' </summary>
        Protected Friend boundsX As Double = 0.0

        ''' <summary>
        ''' The y coordinate of the final graph
        ''' </summary>
        Protected Friend boundsY As Double = 0.0

        ''' <summary>
        ''' The width coordinate of the final graph
        ''' </summary>
        Protected Friend boundsWidth As Double = 0.0

        ''' <summary>
        ''' The height coordinate of the final graph
        ''' </summary>
        Protected Friend boundsHeight As Double = 0.0

        ''' <summary>
        ''' current iteration number of the layout
        ''' </summary>
        Protected Friend iteration As Integer

        ''' <summary>
        ''' cached version of <code>minDistanceLimit</code> squared
        ''' </summary>
        Protected Friend minDistanceLimitSquared As Double


        ''' <summary>
        ''' cached version of <code>maxDistanceLimit</code> squared
        ''' </summary>
        Protected Friend maxDistanceLimitSquared As Double

        ''' <summary>
        ''' Keeps track of how many consecutive round have passed without any energy
        ''' changes 
        ''' </summary>
        Protected Friend unchangedEnergyRoundCount As Integer

        ''' <summary>
        ''' Internal models collection of nodes ( vertices ) to be laid out
        ''' </summary>
        Protected Friend v As CellWrapper()

        ''' <summary>
        ''' Internal models collection of edges to be laid out
        ''' </summary>
        Protected Friend e As CellWrapper()

        ''' <summary>
        ''' Array of the x portion of the normalised test vectors that 
        ''' are tested for a lower energy around each vertex. The vector 
        ''' of the combined x and y normals are multipled by the current 
        ''' radius to obtain test points for each vector in the array.
        ''' </summary>
        Protected Friend xNormTry As Double()

        ''' <summary>
        ''' Array of the y portion of the normalised test vectors that 
        ''' are tested for a lower energy around each vertex. The vector 
        ''' of the combined x and y normals are multipled by the current 
        ''' radius to obtain test points for each vector in the array.
        ''' </summary>
        Protected Friend yNormTry As Double()

        ''' <summary>
        ''' Whether or not fine tuning is on. The determines whether or not
        ''' node to edge distances are calculated in the total system energy.
        ''' This cost function , besides detecting line intersection, is a
        ''' performance intensive component of this algorithm and best left
        ''' to optimization phase. <code>isFineTuning</code> is switched to
        ''' <code>true</code> if and when the <code>fineTuningRadius</code>
        ''' radius is reached. Switching this variable to <code>true</code>
        ''' before the algorithm runs mean the node to edge cost function
        ''' is always calculated.
        ''' </summary>
        Protected Friend ___isFineTuning As Boolean = True

        ''' <summary>
        ''' Constructor for mxOrganicLayout.
        ''' </summary>
        Public Sub New(ByVal graph As NetworkGraph)
            MyBase.New(graph)
        End Sub

        ''' <summary>
        ''' Constructor for mxOrganicLayout.
        ''' </summary>
        Public Sub New(ByVal graph As NetworkGraph, ByVal bounds As mxRectangle)
            MyBase.New(graph)
            boundsX = bounds.X
            boundsY = bounds.Y
            boundsWidth = bounds.Width
            boundsHeight = bounds.Height
        End Sub

        ''' <summary>
        ''' Returns all distinct edges connected to this cell.
        ''' </summary>
        ''' <param name="model"> Model that contains the connection information. </param>
        ''' <param name="cell"> Cell whose connections should be returned. </param>
        ''' <param name="incoming"> Specifies if incoming edges should be returned. </param>
        ''' <param name="outgoing"> Specifies if outgoing edges should be returned. </param>
        ''' <param name="includeLoops"> Specifies if loops should be returned. </param>
        ''' <returns> Returns the array of connected edges for the given cell. </returns>
        Public Shared Function getEdges(ByVal model As mxIGraphModel, ByVal cell As Object, ByVal incoming As Boolean, ByVal outgoing As Boolean, ByVal includeLoops As Boolean) As Object()
            Dim ___edgeCount As Integer = model.getEdgeCount(cell)
            Dim result As IList(Of Object) = New List(Of Object)(___edgeCount)

            For i As Integer = 0 To ___edgeCount - 1
                Dim ___edge As Object = model.getEdgeAt(cell, i)
                Dim source As Object = model.getTerminal(___edge, True)
                Dim target As Object = model.getTerminal(___edge, False)

                If (includeLoops AndAlso source Is target) OrElse ((source IsNot target) AndAlso ((incoming AndAlso target Is cell) OrElse (outgoing AndAlso source Is cell))) Then result.Add(___edge)
            Next

            Return result.ToArray()
        End Function

        Public Overrides Sub execute(ByVal parent As Object)
            Dim model As com.mxgraph.model.mxIGraphModel = Graph.Model
            Dim view As NetworkGraphView = Graph.View
            Dim vertices As Object() = Graph.getChildVertices(parent)
            Dim vertexSet As New HashSet(Of Object)(vertices)

            Dim validEdges As New HashSet(Of Object)
            Dim edges As Object()

            ' Remove edges that do not have both source and target terminals visible
            For i As Integer = 0 To vertices.Length - 1
                edges = getEdges(model, vertices(i), False, True, False)

                For j As Integer = 0 To edges.Length - 1
                    ' Only deal with sources. To be valid in the layout, each edge must be attached
                    ' at both source and target to a vertex in the layout. Doing this avoids processing
                    ' each edge twice.
                    If view.getVisibleTerminal(edges(j), True) Is vertices(i) AndAlso vertexSet.Contains(view.getVisibleTerminal(edges(j), False)) Then validEdges.Add(edges(j))
                Next

            Next

            edges = validEdges.ToArray()

            ' If the bounds dimensions have not been set see if the average area
            ' per node has been
            Dim totalBounds As mxRectangle = Nothing
            Dim bounds As mxRectangle = Nothing

            ' Form internal model of nodes
            Dim vertexMap As IDictionary(Of Object, Integer?) = New Dictionary(Of Object, Integer?)
            v = New CellWrapper(vertices.Length - 1) {}
            For i As Integer = 0 To vertices.Length - 1
                v(i) = New CellWrapper(Me, vertices(i))
                vertexMap(vertices(i)) = New Integer?(i)
                bounds = getVertexBounds(vertices(i))

                If totalBounds Is Nothing Then
                    totalBounds = bounds
                Else
                    totalBounds.add(bounds)
                End If

                ' Set the X,Y value of the internal version of the cell to
                ' the center point of the vertex for better positioning
                Dim width As Double = bounds.Width
                Dim height As Double = bounds.Height
                v(i).X = bounds.X + width / 2.0
                v(i).Y = bounds.Y + height / 2.0
                If ApproxNodeDimensions Then
                    v(i).RadiusSquared = Math.Min(width, height)
                    v(i).RadiusSquared *= v(i).RadiusSquared
                Else
                    v(i).RadiusSquared = width * width
                    v(i).HeightSquared = height * height
                End If
            Next

            If AverageNodeArea = 0.0 Then
                If boundsWidth = 0.0 AndAlso Not totalBounds Is Nothing Then
                    ' Just use current bounds of graph
                    boundsX = totalBounds.X
                    boundsY = totalBounds.Y
                    boundsWidth = totalBounds.Width
                    boundsHeight = totalBounds.Height
                End If
            Else
                ' find the center point of the current graph
                ' based the new graph bounds on the average node area set
                Dim newArea As Double = AverageNodeArea * vertices.Length
                Dim squareLength As Double = Math.Sqrt(newArea)
                If Not bounds Is Nothing Then
                    Dim centreX As Double = totalBounds.X + totalBounds.Width / 2.0
                    Dim centreY As Double = totalBounds.Y + totalBounds.Height / 2.0
                    boundsX = centreX - squareLength / 2.0
                    boundsY = centreY - squareLength / 2.0
                Else
                    boundsX = 0
                    boundsY = 0
                End If
                boundsWidth = squareLength
                boundsHeight = squareLength
                ' Ensure x and y are 0 or positive
                If boundsX < 0.0 OrElse boundsY < 0.0 Then
                    Dim maxNegativeAxis As Double = Math.Min(boundsX, boundsY)
                    Dim axisOffset As Double = -maxNegativeAxis
                    boundsX += axisOffset
                    boundsY += axisOffset
                End If
            End If

            ' If the initial move radius has not been set find a suitable value.
            ' A good value is half the maximum dimension of the final graph area
            If InitialMoveRadius = 0.0 Then InitialMoveRadius = Math.Max(boundsWidth, boundsHeight) / 2.0

            moveRadius = InitialMoveRadius

            minDistanceLimitSquared = MinDistanceLimit * MinDistanceLimit
            maxDistanceLimitSquared = MaxDistanceLimit * MaxDistanceLimit

            unchangedEnergyRoundCount = 0


            ' Form internal model of edges
            e = New CellWrapper(edges.Length - 1) {}

            For i As Integer = 0 To e.Length - 1
                e(i) = New CellWrapper(Me, edges(i))

                Dim sourceCell As Object = model.getTerminal(edges(i), True)
                Dim targetCell As Object = model.getTerminal(edges(i), False)
                Dim source As Integer? = Nothing
                Dim target As Integer? = Nothing
                ' Check if either end of the edge is not connected
                If sourceCell IsNot Nothing Then source = vertexMap(sourceCell)
                If targetCell IsNot Nothing Then target = vertexMap(targetCell)
                If source IsNot Nothing Then
                    e(i).Source = source
                Else
                    ' source end is not connected
                    e(i).Source = -1
                End If
                If target IsNot Nothing Then
                    e(i).Target = target
                Else
                    ' target end is not connected
                    e(i).Target = -1
                End If
            Next

            ' Set up internal nodes with information about whether edges
            ' are connected to them or not
            For i As Integer = 0 To v.Length - 1
                v(i).RelevantEdges = getRelevantEdges(i)
                v(i).ConnectedEdges = getConnectedEdges(i)
            Next

            ' Setup the normal vectors for the test points to move each vertex to
            xNormTry = New Double(TriesPerCell - 1) {}
            yNormTry = New Double(TriesPerCell - 1) {}

            For i As Integer = 0 To TriesPerCell - 1
                Dim angle As Double = i * ((2.0 * Math.PI) / TriesPerCell)
                xNormTry(i) = Math.Cos(angle)
                yNormTry(i) = Math.Sin(angle)
            Next


            Dim childCount As Integer = model.getChildCount(parent)

            For i As Integer = 0 To childCount - 1
                Dim cell As Object = model.getChildAt(parent, i)

                If Not isEdgeIgnored(cell) Then
                    If ResetEdges Then Graph.resetEdge(cell)

                    If DisableEdgeStyle Then setEdgeStyleEnabled(cell, False)
                End If
            Next

            ' The main layout loop
            For iteration = 0 To MaxIterations - 1
                performRound()
            Next

            ' Obtain the final positions
            Dim result As Double()() = MAT(Of Double)(v.Length, 2)
            For i As Integer = 0 To v.Length - 1
                vertices(i) = v(i).Cell
                bounds = getVertexBounds(vertices(i))

                result(i)(0) = v(i).X - bounds.Width / 2
                result(i)(1) = v(i).Y - bounds.Height / 2
            Next

            model.beginUpdate()
            Try
                For i As Integer = 0 To vertices.Length - 1
                    setVertexLocation(vertices(i), result(i)(0), result(i)(1))
                Next
            Finally
                model.endUpdate()
            End Try
        End Sub

        ''' <summary>
        ''' The main round of the algorithm. Firstly, a permutation of nodes
        ''' is created and worked through in that random order. Then, for each node
        ''' a number of point of a circle of radius <code>moveRadius</code> are
        ''' selected and the total energy of the system calculated if that node
        ''' were moved to that new position. If a lower energy position is found
        ''' this is accepted and the algorithm moves onto the next node. There
        ''' may be a slightly lower energy value yet to be found, but forcing
        ''' the loop to check all possible positions adds nearly the current
        ''' processing time again, and for little benefit. Another possible
        ''' strategy would be to take account of the fact that the energy values
        ''' around the circle decrease for half the loop and increase for the
        ''' other, as a general rule. If part of the decrease were seen, then
        ''' when the energy of a node increased, the previous node position was
        ''' almost always the lowest energy position. This adds about two loop
        ''' iterations to the inner loop and only makes sense with 16 tries or more.
        ''' </summary>
        Protected Friend Overridable Sub performRound()
            ' sequential order cells are computed (every round the same order)

            ' boolean to keep track of whether any moves were made in this round
            Dim energyHasChanged As Boolean = False
            For i As Integer = 0 To v.Length - 1
                Dim index As Integer = i

                ' Obtain the energies for the node is its current position
                ' TODO The energy could be stored from the last iteration
                ' and used again, rather than re-calculate
                Dim oldNodeDistribution As Double = getNodeDistribution(index)
                Dim oldEdgeDistance As Double = getEdgeDistanceFromNode(index)
                oldEdgeDistance += getEdgeDistanceAffectedNodes(index)
                Dim oldEdgeCrossing As Double = getEdgeCrossingAffectedEdges(index)
                Dim oldBorderLine As Double = getBorderline(index)
                Dim oldEdgeLength As Double = getEdgeLengthAffectedEdges(index)
                Dim oldAdditionFactors As Double = getAdditionFactorsEnergy(index)

                For j As Integer = 0 To TriesPerCell - 1
                    Dim movex As Double = moveRadius * xNormTry(j)
                    Dim movey As Double = moveRadius * yNormTry(j)

                    ' applying new move
                    Dim oldx As Double = v(index).X
                    Dim oldy As Double = v(index).Y
                    v(index).X = v(index).X + movex
                    v(index).Y = v(index).Y + movey

                    ' calculate the energy delta from this move
                    Dim energyDelta As Double = calcEnergyDelta(index, oldNodeDistribution, oldEdgeDistance, oldEdgeCrossing, oldBorderLine, oldEdgeLength, oldAdditionFactors)

                    If energyDelta < 0 Then
                        ' energy of moved node is lower, finish tries for this
                        ' node
                        energyHasChanged = True
                        Exit For ' exits loop
                    Else
                        ' Revert node coordinates
                        v(index).X = oldx
                        v(index).Y = oldy
                    End If
                Next
            Next
            ' Check if we've hit the limit number of unchanged rounds that cause
            ' a termination condition
            If energyHasChanged Then
                unchangedEnergyRoundCount = 0
            Else
                unchangedEnergyRoundCount += 1
                ' Half the move radius in case assuming it's set too high for
                ' what might be an optimisation case
                moveRadius /= 2.0
            End If
            If unchangedEnergyRoundCount >= UnchangedEnergyRoundTermination Then iteration = MaxIterations

            ' decrement radius in controlled manner
            Dim newMoveRadius As Double = moveRadius * RadiusScaleFactor
            ' Don't waste time on tiny decrements, if the final pixel resolution
            ' is 50 then there's no point doing 55,54.1, 53.2 etc
            If moveRadius - newMoveRadius < MinMoveRadius Then newMoveRadius = moveRadius - MinMoveRadius
            ' If the temperature reaches its minimum temperature then finish
            If newMoveRadius <= MinMoveRadius Then iteration = MaxIterations
            ' Switch on fine tuning below the specified temperature
            If newMoveRadius < FineTuningRadius Then ___isFineTuning = True

            moveRadius = newMoveRadius

        End Sub

        ''' <summary>
        ''' Calculates the change in energy for the specified node. The new energy is
        ''' calculated from the cost function methods and the old energy values for
        ''' each cost function are passed in as parameters
        ''' </summary>
        ''' <param name="index">
        '''            The index of the node in the <code>vertices</code> array </param>
        ''' <param name="oldNodeDistribution">
        '''            The previous node distribution energy cost of this node </param>
        ''' <param name="oldEdgeDistance">
        '''            The previous edge distance energy cost of this node </param>
        ''' <param name="oldEdgeCrossing">
        '''            The previous edge crossing energy cost for edges connected to
        '''            this node </param>
        ''' <param name="oldBorderLine">
        '''            The previous border line energy cost for this node </param>
        ''' <param name="oldEdgeLength">
        '''            The previous edge length energy cost for edges connected to
        '''            this node </param>
        ''' <param name="oldAdditionalFactorsEnergy">
        '''            The previous energy cost for additional factors from
        '''            sub-classes
        ''' </param>
        ''' <returns> the delta of the new energy cost to the old energy cost
        '''  </returns>
        Protected Friend Overridable Function calcEnergyDelta(ByVal index As Integer, ByVal oldNodeDistribution As Double, ByVal oldEdgeDistance As Double, ByVal oldEdgeCrossing As Double, ByVal oldBorderLine As Double, ByVal oldEdgeLength As Double, ByVal oldAdditionalFactorsEnergy As Double) As Double
            Dim energyDelta As Double = 0.0
            energyDelta += getNodeDistribution(index) * 2.0
            energyDelta -= oldNodeDistribution * 2.0

            energyDelta += getBorderline(index)
            energyDelta -= oldBorderLine

            energyDelta += getEdgeDistanceFromNode(index)
            energyDelta += getEdgeDistanceAffectedNodes(index)
            energyDelta -= oldEdgeDistance

            energyDelta -= oldEdgeLength
            energyDelta += getEdgeLengthAffectedEdges(index)

            energyDelta -= oldEdgeCrossing
            energyDelta += getEdgeCrossingAffectedEdges(index)

            energyDelta -= oldAdditionalFactorsEnergy
            energyDelta += getAdditionFactorsEnergy(index)

            Return energyDelta
        End Function

        ''' <summary>
        ''' Calculates the energy cost of the specified node relative to all other
        ''' nodes. Basically produces a higher energy the closer nodes are together.
        ''' </summary>
        ''' <param name="i"> the index of the node in the array <code>v</code> </param>
        ''' <returns> the total node distribution energy of the specified node  </returns>
        Protected Friend Overridable Function getNodeDistribution(ByVal i As Integer) As Double
            Dim energy As Double = 0.0

            ' This check is placed outside of the inner loop for speed, even
            ' though the code then has to be duplicated
            If ___isOptimizeNodeDistribution = True Then
                If ApproxNodeDimensions Then
                    For j As Integer = 0 To v.Length - 1
                        If i <> j Then
                            Dim vx As Double = v(i).X - v(j).X
                            Dim vy As Double = v(i).Y - v(j).Y
                            Dim distanceSquared As Double = vx * vx + vy * vy
                            distanceSquared -= v(i).RadiusSquared
                            distanceSquared -= v(j).RadiusSquared

                            ' prevents from dividing with Zero.
                            If distanceSquared < minDistanceLimitSquared Then distanceSquared = minDistanceLimitSquared

                            energy += NodeDistributionCostFactor / distanceSquared
                        End If
                    Next
                Else
                    For j As Integer = 0 To v.Length - 1
                        If i <> j Then
                            Dim vx As Double = v(i).X - v(j).X
                            Dim vy As Double = v(i).Y - v(j).Y
                            Dim distanceSquared As Double = vx * vx + vy * vy
                            distanceSquared -= v(i).RadiusSquared
                            distanceSquared -= v(j).RadiusSquared
                            ' If the height separation indicates overlap, subtract
                            ' the widths from the distance. Same for width overlap
                            ' TODO						if ()

                            ' prevents from dividing with Zero.
                            If distanceSquared < minDistanceLimitSquared Then distanceSquared = minDistanceLimitSquared

                            energy += NodeDistributionCostFactor / distanceSquared
                        End If
                    Next
                End If
            End If
            Return energy
        End Function

        ''' <summary>
        ''' This method calculates the energy of the distance of the specified
        ''' node to the notional border of the graph. The energy increases up to
        ''' a limited maximum close to the border and stays at that maximum
        ''' up to and over the border.
        ''' </summary>
        ''' <param name="i"> the index of the node in the array <code>v</code> </param>
        ''' <returns> the total border line energy of the specified node  </returns>
        Protected Friend Overridable Function getBorderline(ByVal i As Integer) As Double
            Dim energy As Double = 0.0
            If ___isOptimizeBorderLine Then
                ' Avoid very small distances and convert negative distance (i.e
                ' outside the border to small positive ones )
                Dim l As Double = v(i).X - boundsX
                If l < MinDistanceLimit Then l = MinDistanceLimit
                Dim t As Double = v(i).Y - boundsY
                If t < MinDistanceLimit Then t = MinDistanceLimit
                Dim r As Double = boundsX + boundsWidth - v(i).X
                If r < MinDistanceLimit Then r = MinDistanceLimit
                Dim b As Double = boundsY + boundsHeight - v(i).Y
                If b < MinDistanceLimit Then b = MinDistanceLimit
                energy += BorderLineCostFactor * ((1000000.0 / (t * t)) + (1000000.0 / (l * l)) + (1000000.0 / (b * b)) + (1000000.0 / (r * r)))
            End If
            Return energy
        End Function

        ''' <summary>
        ''' Obtains the energy cost function for the specified node being moved.
        ''' This involves calling <code>getEdgeLength</code> for all
        ''' edges connected to the specified node </summary>
        ''' <param name="node">
        ''' 				the node whose connected edges cost functions are to be
        ''' 				calculated </param>
        ''' <returns> the total edge length energy of the connected edges  </returns>
        Protected Friend Overridable Function getEdgeLengthAffectedEdges(ByVal node As Integer) As Double
            Dim energy As Double = 0.0
            For i As Integer = 0 To v(node).ConnectedEdges.Length - 1
                energy += getEdgeLength(v(node).ConnectedEdges(i))
            Next
            Return energy
        End Function

        ''' <summary>
        ''' This method calculates the energy due to the length of the specified
        ''' edge. The energy is proportional to the length of the edge, making
        ''' shorter edges preferable in the layout.
        ''' </summary>
        ''' <param name="i"> the index of the edge in the array <code>e</code> </param>
        ''' <returns> the total edge length energy of the specified edge  </returns>
        Protected Friend Overridable Function getEdgeLength(ByVal i As Integer) As Double
            If ___isOptimizeEdgeLength Then
                Dim ___edgeLength As Double = Imaging.Distance(v(e(i).Source).X, v(e(i).Source).Y, v(e(i).Target).X, v(e(i).Target).Y)
                Return (EdgeLengthCostFactor * ___edgeLength * ___edgeLength)
            Else
                Return 0.0
            End If
        End Function

        ''' <summary>
        ''' Obtains the energy cost function for the specified node being moved.
        ''' This involves calling <code>getEdgeCrossing</code> for all
        ''' edges connected to the specified node </summary>
        ''' <param name="node">
        ''' 				the node whose connected edges cost functions are to be
        ''' 				calculated </param>
        ''' <returns> the total edge crossing energy of the connected edges  </returns>
        Protected Friend Overridable Function getEdgeCrossingAffectedEdges(ByVal node As Integer) As Double
            Dim energy As Double = 0.0
            For i As Integer = 0 To v(node).ConnectedEdges.Length - 1
                energy += getEdgeCrossing(v(node).ConnectedEdges(i))
            Next

            Return energy
        End Function

        ''' <summary>
        ''' This method calculates the energy of the distance from the specified
        ''' edge crossing any other edges. Each crossing add a constant factor
        ''' to the total energy
        ''' </summary>
        ''' <param name="i"> the index of the edge in the array <code>e</code> </param>
        ''' <returns> the total edge crossing energy of the specified edge  </returns>
        Protected Friend Overridable Function getEdgeCrossing(ByVal i As Integer) As Double
            ' TODO Could have a cost function per edge
            Dim n As Integer = 0 ' counts energy of edgecrossings through edge i

            ' max and min variable for minimum bounding rectangles overlapping
            ' checks
            Dim minjX, minjY, miniX, miniY, maxjX, maxjY, maxiX, maxiY As Double

            If ___isOptimizeEdgeCrossing Then
                Dim iP1X As Double = v(e(i).Source).X
                Dim iP1Y As Double = v(e(i).Source).Y
                Dim iP2X As Double = v(e(i).Target).X
                Dim iP2Y As Double = v(e(i).Target).Y

                For j As Integer = 0 To e.Length - 1
                    Dim jP1X As Double = v(e(j).Source).X
                    Dim jP1Y As Double = v(e(j).Source).Y
                    Dim jP2X As Double = v(e(j).Target).X
                    Dim jP2Y As Double = v(e(j).Target).Y
                    If j <> i Then
                        ' First check is to see if the minimum bounding rectangles
                        ' of the edges overlap at all. Since the layout tries
                        ' to separate nodes and shorten edges, the majority do not
                        ' overlap and this is a cheap way to avoid most of the
                        ' processing
                        ' Some long code to avoid a Math.max call...
                        If iP1X < iP2X Then
                            miniX = iP1X
                            maxiX = iP2X
                        Else
                            miniX = iP2X
                            maxiX = iP1X
                        End If
                        If jP1X < jP2X Then
                            minjX = jP1X
                            maxjX = jP2X
                        Else
                            minjX = jP2X
                            maxjX = jP1X
                        End If
                        If maxiX < minjX OrElse miniX > maxjX Then Continue For

                        If iP1Y < iP2Y Then
                            miniY = iP1Y
                            maxiY = iP2Y
                        Else
                            miniY = iP2Y
                            maxiY = iP1Y
                        End If
                        If jP1Y < jP2Y Then
                            minjY = jP1Y
                            maxjY = jP2Y
                        Else
                            minjY = jP2Y
                            maxjY = jP1Y
                        End If
                        If maxiY < minjY OrElse miniY > maxjY Then Continue For

                        ' Ignore if any end points are coincident
                        If ((iP1X <> jP1X) AndAlso (iP1Y <> jP1Y)) AndAlso ((iP1X <> jP2X) AndAlso (iP1Y <> jP2Y)) AndAlso ((iP2X <> jP1X) AndAlso (iP2Y <> jP1Y)) AndAlso ((iP2X <> jP2X) AndAlso (iP2Y <> jP2Y)) Then
                            ' Values of zero returned from Line2D.relativeCCW are
                            ' ignored because the point being exactly on the line
                            ' is very rare for double and we've already checked if
                            ' any end point share the same vertex. Should zero
                            ' ever be returned, it would be the vertex connected
                            ' to the edge that's actually on the edge and this is
                            ' dealt with by the node to edge distance cost
                            ' function. The worst case is that the vertex is
                            ' pushed off the edge faster than it would be
                            ' otherwise. Because of ignoring the zero this code
                            ' below can behave like only a 1 or -1 will be
                            ' returned. See Lines2D.linesIntersects().
                            Dim intersects As Boolean = ((Line2D.relativeCCW(iP1X, iP1Y, iP2X, iP2Y, jP1X, jP1Y) <> Line2D.relativeCCW(iP1X, iP1Y, iP2X, iP2Y, jP2X, jP2Y)) AndAlso (Line2D.relativeCCW(jP1X, jP1Y, jP2X, jP2Y, iP1X, iP1Y) <> Line2D.relativeCCW(jP1X, jP1Y, jP2X, jP2Y, iP2X, iP2Y)))

                            If intersects Then n += 1
                        End If
                    End If
                Next
            End If
            Return EdgeCrossingCostFactor * n
        End Function

        ''' <summary>
        ''' This method calculates the energy of the distance between Cells and
        ''' Edges. This version of the edge distance cost calculates the energy
        ''' cost from a specified <strong>node</strong>. The distance cost to all
        ''' unconnected edges is calculated and the total returned.
        ''' </summary>
        ''' <param name="i"> the index of the node in the array <code>v</code> </param>
        ''' <returns> the total edge distance energy of the node </returns>
        Protected Friend Overridable Function getEdgeDistanceFromNode(ByVal i As Integer) As Double
            Dim energy As Double = 0.0
            ' This function is only performed during fine tuning for performance
            If ___isOptimizeEdgeDistance AndAlso ___isFineTuning Then
                Dim edges As Integer() = v(i).RelevantEdges
                For j As Integer = 0 To edges.Length - 1
                    ' Note that the distance value is squared
                    Dim distSquare As Double = Line2D.ptSegDistSq(v(e(edges(j)).Source).X, v(e(edges(j)).Source).Y, v(e(edges(j)).Target).X, v(e(edges(j)).Target).Y, v(i).X, v(i).Y)

                    distSquare -= v(i).RadiusSquared

                    ' prevents from dividing with Zero. No Math.abs() call
                    ' for performance
                    If distSquare < minDistanceLimitSquared Then distSquare = minDistanceLimitSquared

                    ' Only bother with the divide if the node and edge are
                    ' fairly close together
                    If distSquare < maxDistanceLimitSquared Then energy += EdgeDistanceCostFactor / distSquare
                Next
            End If
            Return energy
        End Function

        ''' <summary>
        ''' Obtains the energy cost function for the specified node being moved.
        ''' This involves calling <code>getEdgeDistanceFromEdge</code> for all
        ''' edges connected to the specified node </summary>
        ''' <param name="node">
        ''' 				the node whose connected edges cost functions are to be
        ''' 				calculated </param>
        ''' <returns> the total edge distance energy of the connected edges  </returns>
        Protected Friend Overridable Function getEdgeDistanceAffectedNodes(ByVal node As Integer) As Double
            Dim energy As Double = 0.0
            For i As Integer = 0 To (v(node).ConnectedEdges.Length) - 1
                energy += getEdgeDistanceFromEdge(v(node).ConnectedEdges(i))
            Next

            Return energy
        End Function

        ''' <summary>
        ''' This method calculates the energy of the distance between Cells and
        ''' Edges. This version of the edge distance cost calculates the energy
        ''' cost from a specified <strong>edge</strong>. The distance cost to all
        ''' unconnected nodes is calculated and the total returned.
        ''' </summary>
        ''' <param name="i"> the index of the edge in the array <code>e</code> </param>
        ''' <returns> the total edge distance energy of the edge </returns>
        Protected Friend Overridable Function getEdgeDistanceFromEdge(ByVal i As Integer) As Double
            Dim energy As Double = 0.0
            ' This function is only performed during fine tuning for performance
            If ___isOptimizeEdgeDistance AndAlso ___isFineTuning Then
                For j As Integer = 0 To v.Length - 1
                    ' Don't calculate for connected nodes
                    If e(i).Source <> j AndAlso e(i).Target <> j Then
                        Dim distSquare As Double = Line2D.ptSegDistSq(v(e(i).Source).X, v(e(i).Source).Y, v(e(i).Target).X, v(e(i).Target).Y, v(j).X, v(j).Y)

                        distSquare -= v(j).RadiusSquared

                        ' prevents from dividing with Zero. No Math.abs() call
                        ' for performance
                        If distSquare < minDistanceLimitSquared Then distSquare = minDistanceLimitSquared

                        ' Only bother with the divide if the node and edge are
                        ' fairly close together
                        If distSquare < maxDistanceLimitSquared Then energy += EdgeDistanceCostFactor / distSquare
                    End If
                Next
            End If
            Return energy
        End Function

        ''' <summary>
        ''' Hook method to adding additional energy factors into the layout.
        ''' Calculates the energy just for the specified node. </summary>
        ''' <param name="i"> the nodes whose energy is being calculated </param>
        ''' <returns> the energy of this node caused by the additional factors </returns>
        Protected Friend Overridable Function getAdditionFactorsEnergy(ByVal i As Integer) As Double
            Return 0.0
        End Function

        ''' <summary>
        ''' Returns all Edges that are not connected to the specified cell
        ''' </summary>
        ''' <param name="cellIndex">
        '''            the cell index to which the edges are not connected </param>
        ''' <returns> Array of all interesting Edges </returns>
        Protected Friend Overridable Function getRelevantEdges(ByVal cellIndex As Integer) As Integer()
            Dim relevantEdgeList As New List(Of Integer)(e.Length)

            For i As Integer = 0 To e.Length - 1
                If e(i).Source <> cellIndex AndAlso e(i).Target <> cellIndex Then relevantEdgeList.Add(i)
            Next

            Return relevantEdgeList.ToArray
        End Function

        ''' <summary>
        ''' Returns all Edges that are connected with the specified cell
        ''' </summary>
        ''' <param name="cellIndex">
        '''            the cell index to which the edges are connected </param>
        ''' <returns> Array of all connected Edges </returns>
        Protected Friend Overridable Function getConnectedEdges(ByVal cellIndex As Integer) As Integer()
            Dim connectedEdgeList As New List(Of Integer)(e.Length)

            For i As Integer = 0 To e.Length - 1
                If e(i).Source = cellIndex OrElse e(i).Target = cellIndex Then connectedEdgeList.Add(i)
            Next

            Return connectedEdgeList.ToArray
        End Function

        ''' <summary>
        ''' Returns <code>Organic</code>, the name of this algorithm.
        ''' </summary>
        Public Overrides Function ToString() As String
            Return "Organic"
        End Function
    End Class
End Namespace

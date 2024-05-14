#Region "Microsoft.VisualBasic::f93d9e071d4bb5d6aaa2052ea53e6c5d, gr\network-visualization\network_layout\Cola\Layout\layout.vb"

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

    '   Total Lines: 1020
    '    Code Lines: 647
    ' Comment Lines: 237
    '   Blank Lines: 136
    '     File Size: 45.30 KB


    '     Class Layout
    ' 
    '         Function: [on], [resume], [stop], (+2 Overloads) alpha, (+2 Overloads) avoidOverlaps
    '                   (+2 Overloads) constraints, (+2 Overloads) convergenceThreshold, (+2 Overloads) defaultNodeSize, (+2 Overloads) distanceMatrix, dragOrigin
    '                   (+2 Overloads) flowLayout, getLinkLength, getLinkType, (+2 Overloads) getSourceIndex, (+2 Overloads) getTargetIndex
    '                   (+2 Overloads) groupCompactness, (+2 Overloads) groups, (+2 Overloads) handleDisconnected, jaccardLinkLengths, (+3 Overloads) linkDistance
    '                   linkId, (+2 Overloads) links, (+2 Overloads) linkType, (+2 Overloads) nodes, powerGraphGroups
    '                   routeEdge, (+2 Overloads) size, start, symmetricDiffLinkLengths, tick
    ' 
    '         Sub: drag, dragEnd, dragStart, initialLayout, kick
    '              mouseOut, mouseOver, prepareEdgeRouting, separateOverlappingComponents, stopNode
    '              storeOffset, trigger, updateNodePositions
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports any = System.Object
Imports number = System.Double

Namespace Cola

    ''' <summary>
    ''' Main interface to cola layout.
    ''' </summary>
    Public Class Layout

        Private _canvasSize As Integer() = {1, 1}
        Private _linkDistance As [Variant](Of Double, Func(Of any, Double)) = 20
        Private _defaultNodeSize As Double = 10
        Private _linkLengthCalculator As Action = Nothing
        Private _linkType As New [Variant](Of Integer, Func(Of Link(Of Node), Integer))
        Private _avoidOverlaps As Boolean = False
        Private _handleDisconnected As Boolean = True
        Private _alpha As Double
        Private _lastStress As Double?
        Private _running As Boolean = False
        Private _nodes As Node() = {}
        Private _groups As Node() = {}
        Private _rootGroup As Node

        ''' <summary>
        ''' 与<see cref="_indexLinks"/>是一一对应的
        ''' </summary>
        Private _links As Link(Of Node)() = {}
        ''' <summary>
        ''' 因为在typescript里面，类型是可变的，所以在这里会需要这个额外的index对象来保持兼容
        ''' </summary>
        Private _indexLinks As Link(Of Integer)() = {}

        Private _constraints As Constraint(Of Integer)() = {}
        Private _distanceMatrix As Integer()() = Nothing
        Private _descent As Descent = Nothing
        Private _directedLinkConstraints As LinkSepAccessor(Of Link(Of Node)) = Nothing
        Private _threshold As [Variant](Of Double, Func(Of Double)) = 0.01
        Private _visibilityGraph As any = Nothing
        Private _groupCompactness As Double = 0.000001

        ''' <summary>
        ''' sub-class and override this property to replace with a more sophisticated eventing mechanism
        ''' </summary>
        Protected [event] As [Event] = Nothing

        ' subscribe a listener to an event
        ' sub-class and override this method to replace with a more sophisticated eventing mechanism
        Public Function [on](e As EventType, listener As Action(Of [Event])) As Layout
            ' override me!
            If Me.[event] Is Nothing Then
                Me.[event] = New [Event]
            End If

            Me.[event](e.ToString) = listener

            Return Me
        End Function

        ' a function that is notified of events like "tick"
        ' sub-classes can override this method to replace with a more sophisticated eventing mechanism
        Protected Sub trigger(e As [Event])
            If Not Me.[event] Is Nothing AndAlso Me.[event](e.type) IsNot Nothing Then
                Dim action As Action(Of [Event]) = Me.event(e.type)
                Call action(e)
            End If
        End Sub

        ' a function that kicks off the iteration tick loop
        ' it calls tick() repeatedly until tick returns true (is converged)
        ' subclass and override it with something fancier (e.g. dispatch tick on a timer)
        Protected Sub kick()
            While Not Me.tick()
            End While
        End Sub

        '*
        '     * iterate the layout.  Returns true when layout converged.
        '     

        Protected Function tick() As Boolean
            If Me._alpha < Me._threshold Then
                Me._running = False
                Me._alpha = 0
                Me.trigger(New [Event] With {
                 .type = EventType.[end],
                 .alpha = 0,
                 .stress = Me._lastStress
            })
                Return True
            End If
            Dim n = Me._nodes.Length
            Dim m = Me._links.Length
            Dim o As Node

            Me._descent.locks.clear()
            For i As Integer = 0 To n - 1
                o = Me._nodes(i)
                If o.fixed Then
                    If o.px Is Nothing OrElse o.py Is Nothing Then
                        o.px = o.x
                        o.py = o.y
                    End If
                    Dim p As Double() = {o.px, o.py}
                    Me._descent.locks.add(i, p)
                End If
            Next

            Dim s1 = Me._descent.rungeKutta()
            'var s1 = descent.reduceStress();
            If s1 = 0 Then
                Me._alpha = 0
            ElseIf Me._lastStress IsNot Nothing Then
                'std.Abs(std.Abs(this._lastStress / s1) - 1);
                Me._alpha = s1
            End If
            Me._lastStress = s1

            Me.updateNodePositions()

            Me.trigger(New [Event] With {
            .type = EventType.tick,
            .alpha = Me._alpha,
            .stress = Me._lastStress
        })
            Return False
        End Function

        ' copy positions out of descent instance into each of the nodes' center coords
        Private Sub updateNodePositions()
            Dim x = Me._descent.x(0)
            Dim y = Me._descent.x(1)
            Dim o As Node
            Dim i As Integer = Me._nodes.Length

            While System.Math.Max(Interlocked.Decrement(i), i + 1)
                o = Me._nodes(i)
                o.x = x(i)
                o.y = y(i)
            End While
        End Sub

        '*
        '     * the list of nodes.
        '     * If nodes has not been set, but links has, then we instantiate a nodes list here, of the correct size,
        '     * before returning it.
        '     * @property nodes {Array}
        '     * @default empty list
        '     

        Public Function nodes() As Node()
            If Me._nodes.Length = 0 AndAlso Me._links.Length > 0 Then
                ' if we have links but no nodes, create the nodes array now with empty objects for the links to point at.
                ' in this case the links are expected to be numeric indices for nodes in the range 0..n-1 where n is the number of nodes
                Dim n = 0
                Me._links.DoEach(Sub(l) n = Math.Max(n, l.source.id, l.target.id))
                Me._nodes = New Node(Interlocked.Increment(n)) {}
                For i As Integer = 0 To n - 1
                    Me._nodes(i) = New Node
                Next
            End If
            Return Me._nodes
        End Function

        Public Function nodes(v As IEnumerable(Of Node)) As Layout
            Me._nodes = v.ToArray
            Return Me
        End Function

        '*
        '     * a list of hierarchical groups defined over nodes
        '     * @property groups {Array}
        '     * @default empty list
        '     

        Public Function groups() As Node()
            Return Me._groups
        End Function

        Public Function groups(x As IEnumerable(Of Node)) As Layout
            Me._groups = x.ToArray
            Me._rootGroup = New Node
            Me._groups.DoEach(Sub(g)
                                  If g.padding Is Nothing Then
                                      g.padding = 1
                                  End If
                                  If g.leaves IsNot Nothing Then
                                      g.leaves.ForEach(Sub(v, i)
                                                           g.leaves(i) = v
                                                           CType(g.leaves(i), Node).parent = g
                                                       End Sub)
                                  End If
                                  If g.groups IsNot Nothing Then
                                      g.groups.ForEach(Sub(gi, i)
                                                           g.groups(i) = gi
                                                           CType(g.groups(i), Node).parent = g
                                                       End Sub)
                                  End If
                              End Sub)
            Me._rootGroup.leaves = Me._nodes.Where(Function(v) v.parent Is Nothing).Select(Function(n) New [Variant](Of Integer, Node)(n)).ToList
            Me._rootGroup.groups = Me._groups.Where(Function(g) g.parent Is Nothing).Select(Function(n) New [Variant](Of Integer, Node)(n)).ToList

            Return Me
        End Function

        Public Function powerGraphGroups(f As Action(Of PowerGraph)) As Layout
            Dim g = powergraphExtensions.getGroups(Of Link(Of Node))(Me._nodes, Me._links, Me.linkAccessor, Me._rootGroup)
            Me.groups(g.groups)
            Call f(g)
            Return Me
        End Function

        '*
        '     * if true, the layout will not permit overlaps of the node bounding boxes (defined by the width and height properties on nodes)
        '     * @property avoidOverlaps
        '     * @type bool
        '     * @default false
        '     

        Public Function avoidOverlaps() As Boolean
            Return Me._avoidOverlaps
        End Function

        Public Function avoidOverlaps(v As [Boolean]) As Layout
            Me._avoidOverlaps = v
            Return Me
        End Function

        '*
        '     * if true, the final step of the start method will be to nicely pack connected components of the graph.
        '     * works best if start() is called with a reasonable number of iterations specified and
        '     * each node has a bounding box (defined by the width and height properties on nodes).
        '     * @property handleDisconnected
        '     * @type bool
        '     * @default true
        '     

        Public Function handleDisconnected() As Boolean
            Return Me._handleDisconnected
        End Function

        Public Function handleDisconnected(v As Boolean) As Layout
            Me._handleDisconnected = v
            Return Me
        End Function

        '*
        '     * causes constraints to be generated such that directed graphs are laid out either from left-to-right or top-to-bottom.
        '     * a separation constraint is generated in the selected axis for each edge that is not involved in a cycle (part of a strongly connected component)
        '     * @param axis {string} 'x' for left-to-right, 'y' for top-to-bottom
        '     * @param minSeparation {number|link=>number} either a number specifying a minimum spacing required across all links or a function to return the minimum spacing for each link
        '     

        Public Function flowLayout(Optional axis As String = "y", Optional minSeparation As Double = 0) As Layout
            Me._directedLinkConstraints = New LinkSepAccessor(Of Link(Of Node)) With {
            .axis = axis,
            .getMinSeparation = minSeparation
        }
            Return Me
        End Function

        Public Function flowLayout(Optional axis As String = "y", Optional minSeparation As Func(Of Link(Of Node), Double) = Nothing) As Layout
            Me._directedLinkConstraints = New LinkSepAccessor(Of Link(Of Node)) With {
                .axis = axis,
               .getMinSeparation = minSeparation
            }
            Return Me
        End Function

        '*
        '     * links defined as source, target pairs over nodes
        '     * @property links {array}
        '     * @default empty list
        '     
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function links() As Link(Of Node)()
            Return Me._links
        End Function

        Public Function links(x As IEnumerable(Of Link(Of Node))) As Layout
            Me._links = x.ToArray
            Return Me
        End Function

        '*
        '     * list of constraints of various types
        '     * @property constraints
        '     * @type {array}
        '     * @default empty list
        '     

        Public Function constraints() As Constraint(Of Integer)()
            Return Me._constraints
        End Function

        Public Function constraints(c As Constraint(Of Integer)()) As Layout
            Me._constraints = c
            Return Me
        End Function

        '*
        '     * Matrix of ideal distances between all pairs of nodes.
        '     * If unspecified, the ideal distances for pairs of nodes will be based on the shortest path distance between them.
        '     * @property distanceMatrix
        '     * @type {Array of Array of Number}
        '     * @default null
        '     

        Public Function distanceMatrix() As Integer()()
            Return Me._distanceMatrix
        End Function

        Public Function distanceMatrix(d As Integer()()) As Layout
            Me._distanceMatrix = d
            Return Me
        End Function

        ''' <summary>
        ''' Size of the layout canvas dimensions [x,y]. Currently only used to determine the midpoint which is taken as the starting position
        ''' for nodes with no preassigned x and y.
        ''' </summary>
        ''' <returns></returns>
        Public Function size() As Integer()
            Return Me._canvasSize
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function size(x As Integer()) As Layout
            Me._canvasSize = x
            Return Me
        End Function

        '*
        '     * Default size (assume nodes are square so both width and height) to use in packing if node width/height are not specified.
        '     * @property defaultNodeSize
        '     * @type {Number}
        '     

        Public Function defaultNodeSize() As Double
            Return Me._defaultNodeSize
        End Function

        Public Function defaultNodeSize(x As Double) As Layout
            Me._defaultNodeSize = x
            Return Me
        End Function

        '*
        '     * The strength of attraction between the group boundaries to each other.
        '     * @property defaultNodeSize
        '     * @type {Number}
        '     

        Public Function groupCompactness() As Double
            Return Me._groupCompactness
        End Function

        Public Function groupCompactness(x As Double) As Layout
            Me._groupCompactness = x
            Return Me
        End Function

        '*
        '     * links have an ideal distance, The automatic layout will compute layout that tries to keep links (AKA edges) as close as possible to this length.
        '     

        Public Function linkDistance() As Double
            Return Me._linkDistance
        End Function

        Public Function linkDistance(x As [Variant](Of Double, Func(Of any, Double))) As Layout
            Me._linkDistance = x
            Me._linkLengthCalculator = Nothing
            Return Me
        End Function

        Public Function linkDistance(x As Func(Of Link(Of Node), Double)) As Layout
            Me._linkDistance = New Func(Of any, Double)(Function(any) x(any))
            Me._linkLengthCalculator = Nothing

            Return Me
        End Function

        Public Function linkType(f As Func(Of Link(Of Node), Integer)) As Layout
            Me._linkType = f
            Return Me
        End Function

        Public Function linkType(f As Integer) As Layout
            Me._linkType = f
            Return Me
        End Function

        Public Function convergenceThreshold() As Double
            Return Me._threshold
        End Function

        Public Function convergenceThreshold(x As [Variant](Of Double, Func(Of Double))) As Layout
            Me._threshold = x
            Return Me
        End Function

        Public Function alpha() As Double
            Return Me._alpha
        End Function

        Public Function alpha(x As Double) As Layout

            x = +x
            If Me._alpha Then
                ' if we're already running
                If x > 0 Then
                    Me._alpha = x
                Else
                    ' we might keep it hot
                    Me._alpha = 0
                    ' or, next tick will dispatch "end"
                End If
            ElseIf x > 0 Then
                ' otherwise, fire it up!
                If Not Me._running Then
                    Me._running = True
                    Me._alpha = x
                    Me.trigger(New [Event] With {
                    .type = EventType.start,
                    .alpha = x
                })
                    Me.kick()
                End If
            End If
            Return Me

        End Function

        Public Function getLinkLength(link As Link(Of Node)) As Double
            If Not _linkDistance Like GetType(Double) Then
                Return _linkDistance(link)
            Else
                Return _linkDistance
            End If
        End Function

        Private Function getLinkType(link As Link(Of Node)) As Double
            Return If(_linkType Like GetType(Integer), 0, _linkType(link))
        End Function

        Private linkAccessor As New LinkTypeAccessor(Of Link(Of Node)) With {
            .getSourceIndex = AddressOf Layout.getSourceIndex,
            .getTargetIndex = AddressOf Layout.getTargetIndex,
            .setLength = Sub(l, len) l.length = len,
            .GetLinkType = Function(l)
                               Return If(_linkType Like GetType(Integer), 0, Me._linkType(l))
                           End Function
        }

        '*
        '     * compute an ideal length for each link based on the graph structure around that link.
        '     * you can use this (for example) to create extra space around hub-nodes in dense graphs.
        '     * In particular this calculation is based on the "symmetric difference" in the neighbour sets of the source and target:
        '     * i.e. if neighbours of source is a and neighbours of target are b then calculation is: sqrt(|a union b| - |a intersection b|)
        '     * Actual computation based on inspection of link structure occurs in start(), so links themselves
        '     * don't have to have been assigned before invoking this function.
        '     * @param {number} [idealLength] the base length for an edge when its source and start have no other common neighbours (e.g. 40)
        '     * @param {number} [w] a multiplier for the effect of the length adjustment (e.g. 0.7)
        '     

        Public Function symmetricDiffLinkLengths(idealLength As Double, Optional w As Double = 1) As Layout
            Me.linkDistance(Function(l) idealLength * l.length)
            Me._linkLengthCalculator = Sub() linkLengthExtensions.symmetricDiffLinkLengths(Me._links, Me.linkAccessor, w)
            Return Me
        End Function

        '*
        '     * compute an ideal length for each link based on the graph structure around that link.
        '     * you can use this (for example) to create extra space around hub-nodes in dense graphs.
        '     * In particular this calculation is based on the "symmetric difference" in the neighbour sets of the source and target:
        '     * i.e. if neighbours of source is a and neighbours of target are b then calculation is: |a intersection b|/|a union b|
        '     * Actual computation based on inspection of link structure occurs in start(), so links themselves
        '     * don't have to have been assigned before invoking this function.
        '     * @param {number} [idealLength] the base length for an edge when its source and start have no other common neighbours (e.g. 40)
        '     * @param {number} [w] a multiplier for the effect of the length adjustment (e.g. 0.7)
        '     

        Public Function jaccardLinkLengths(idealLength As Double, Optional w As Double = 1) As Layout
            Me.linkDistance(Function(l) idealLength * l.length)
            Me._linkLengthCalculator = Sub() linkLengthExtensions.jaccardLinkLengths(Me._links, Me.linkAccessor, w)
            Return Me
        End Function

        ''' <summary>
        ''' start the layout process
        ''' </summary>
        ''' <param name="initialUnconstrainedIterations">unconstrained initial layout iterations</param>
        ''' <param name="initialUserConstraintIterations">initial layout iterations with user-specified constraints</param>
        ''' <param name="initialAllConstraintsIterations">initial layout iterations with all constraints including non-overlap</param>
        ''' <param name="gridSnapIterations">iterations of "grid snap", which pulls nodes towards grid cell centers - grid of size node[0].width - only really makes sense if all nodes have the same width and height</param>
        ''' <param name="keepRunning">keep iterating asynchronously via the tick method</param>
        ''' <param name="centerGraph">Center graph on restart</param>
        ''' <returns></returns>
        Public Function start(Optional initialUnconstrainedIterations As Double = 0,
                              Optional initialUserConstraintIterations As Double = 0,
                              Optional initialAllConstraintsIterations As Double = 0,
                              Optional gridSnapIterations As Double = 0,
                              Optional keepRunning As Boolean = True,
                              Optional centerGraph As Boolean = True) As Layout

            Dim n__1 = Me.nodes().Length
            Dim N__2 = n__1 + 2 * Me._groups.Length
            Dim m = Me._links.Length
            Dim w = Me._canvasSize(0)
            Dim h = Me._canvasSize(1)

            Dim x = New Double(N__2) {}
            Dim y = New Double(N__2) {}

            Dim G__3 As Double()() = Nothing

            Dim ao = Me._avoidOverlaps

            Me._nodes.ForEach(Sub(v, i)
                                  v.index = i
                                  If v.x = 0 Then
                                      v.x = w / 2
                                      v.y = h / 2
                                  End If
                                  x(i) = v.x
                                  y(i) = v.y
                              End Sub)

            If Not Me._linkLengthCalculator Is Nothing Then
                Me._linkLengthCalculator()
            End If

            'should we do this to clearly label groups?
            'this._groups.forEach((g, i) => g.groupIndex = i);

            Dim distances As Integer()()

            If Not Me._distanceMatrix Is Nothing Then
                ' use the user specified distanceMatrix
                distances = Me._distanceMatrix
            Else
                ' construct an n X n distance matrix based on shortest paths through graph (with respect to edge.length).
                distances = (New Dijkstra.Calculator(Of Link(Of Node))(N__2, Me._links, AddressOf Layout.getSourceIndex, AddressOf Layout.getTargetIndex, Function(l) Me.getLinkLength(l))).DistanceMatrix()

                ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
                ' otherwise 2. (
                G__3 = Descent.createSquareMatrix(N__2, Function() 2)
                Me._links.ForEach(Sub(l, i)
                                      Dim index = _indexLinks(i)

                                      l.source = Me._nodes(index.source)
                                      l.target = Me._nodes(index.target)
                                  End Sub)
                Me._links.DoEach(Sub(e)
                                     Dim u = Layout.getSourceIndex(e)
                                     Dim v = Layout.getTargetIndex(e)

                                     If e.weight Then
                                         G__3(v)(u) = e.weight
                                         G__3(u)(v) = e.weight
                                     Else
                                         G__3(v)(u) = 1
                                         G__3(u)(v) = 1
                                     End If
                                 End Sub)
            End If

            Dim D = Descent.createSquareMatrix(N__2, Function(i, j) distances(i)(j))

            If Not Me._rootGroup Is Nothing AndAlso Me._rootGroup.groups IsNot Nothing Then
                Dim i = n__1
                Dim addAttraction = Sub(ii As Integer, j As Integer, strength As Double, idealDistance As Double)
                                        G__3(j)(ii) = strength
                                        G__3(ii)(j) = strength
                                        D(j)(ii) = idealDistance
                                        D(ii)(j) = idealDistance
                                    End Sub
                Me._groups.DoEach(Sub(g__4)
                                      addAttraction(i, i + 1, Me._groupCompactness, 0.1)

                                      ' todo: add terms here attracting children of the group to the group dummy nodes
                                      'if (typeof g.leaves !== 'undefined')
                                      '    g.leaves.forEach(l => {
                                      '        addAttraction(l.index, i, 1e-4, 0.1);
                                      '        addAttraction(l.index, i + 1, 1e-4, 0.1);
                                      '    });
                                      'if (typeof g.groups !== 'undefined')
                                      '    g.groups.forEach(g => {
                                      '        var gid = n + g.groupIndex * 2;
                                      '        addAttraction(gid, i, 0.1, 0.1);
                                      '        addAttraction(gid + 1, i, 0.1, 0.1);
                                      '        addAttraction(gid, i + 1, 0.1, 0.1);
                                      '        addAttraction(gid + 1, i + 1, 0.1, 0.1);
                                      '    });

                                      x(i) = 0
                                      y(System.Math.Max(Interlocked.Increment(i), i - 1)) = 0
                                      x(i) = 0
                                      y(System.Math.Max(Interlocked.Increment(i), i - 1)) = 0
                                  End Sub)
            Else
                Me._rootGroup = New Node With {
                    .leaves = Me._nodes.Select(Function(n) New [Variant](Of Integer, Node)(n)).ToList,
                    .groups = New List(Of [Variant](Of Integer, Node))
                }
            End If

            Dim curConstraints As Constraint(Of Integer)() = If(Me._constraints Is Nothing, Me._constraints, {})

            If Me._directedLinkConstraints IsNot Nothing Then
                Me.linkAccessor.getMinSeparation = Me._directedLinkConstraints.getMinSeparation

                ' todo: add containment constraints between group dummy nodes and their children
                curConstraints = curConstraints.Concat(generateDirectedEdgeConstraints(Of Link(Of Node))(n__1, Me._links, Me._directedLinkConstraints.axis, Me.linkAccessor))
            End If

            Me.avoidOverlaps(False)
            Me._descent = New Descent(New number()() {x, y}, D)
            Me._descent.locks.clear()

            For i As Integer = 0 To n__1 - 1
                Dim o = Me._nodes(i)
                If o.fixed Then
                    o.px = o.x
                    o.py = o.y
                    Dim p = New number() {o.x, o.y}
                    Me._descent.locks.add(i, p)
                End If
            Next

            Me._descent.threshold = Me._threshold

            ' apply initialIterations without user constraints or nonoverlap constraints
            ' if groups are specified, dummy nodes and edges will be added to untangle
            ' with respect to group connectivity
            Me.initialLayout(initialUnconstrainedIterations, x, y)

            ' apply initialIterations with user constraints but no nonoverlap constraints
            If curConstraints.Length > 0 Then
                Me._descent.project = New Projection(Of Node)(Me._nodes, Me._groups, Me._rootGroup, curConstraints).projectFunctions()
            End If
            Me._descent.run(initialUserConstraintIterations)
            Me.separateOverlappingComponents(w, h, centerGraph)

            ' subsequent iterations will apply all constraints
            Me.avoidOverlaps(ao)
            If ao Then
                Me._nodes.ForEach(Sub(v, i)
                                      v.x = x(i)
                                      v.y = y(i)
                                  End Sub)
                Me._descent.project = New Projection(Of Node)(Me._nodes, Me._groups, Me._rootGroup, curConstraints, True).projectFunctions()
                Me._nodes.ForEach(Sub(v, i)
                                      x(i) = v.x
                                      y(i) = v.y
                                  End Sub)
            End If

            ' allow not immediately connected nodes to relax apart (p-stress)
            Me._descent.g = G__3
            Me._descent.run(initialAllConstraintsIterations)

            If gridSnapIterations Then
                Me._descent.snapStrength = 1000
                Me._descent.snapGridSize = Me._nodes(0).width
                Me._descent.numGridSnapNodes = n__1
                Me._descent.scaleSnapByMaxH = n__1 <> N__2
                ' if we have groups then need to scale hessian so grid forces still apply
                Dim G0 = Descent.createSquareMatrix(
                    n:=N__2,
                    f:=Function(i, j)
                           If i >= n__1 OrElse j >= n__1 Then
                               Return G__3(i)(j)
                           End If

                           Return 0
                       End Function)

                Me._descent.g = G0
                Me._descent.run(gridSnapIterations)
            End If

            Me.updateNodePositions()
            Me.separateOverlappingComponents(w, h, centerGraph)
            Return If(keepRunning, Me.[resume](), Me)
        End Function

        Private Sub initialLayout(iterations As Double, x As Double(), y As Double())
            If Me._groups.Length > 0 AndAlso iterations > 0 Then
                ' construct a flat graph with dummy nodes for the groups and edges connecting group dummy nodes to their children
                ' todo: edges attached to groups are replaced with edges connected to the corresponding group dummy node
                Dim n = Me._nodes.Length
                Dim edges = Me._links.Select(Function(e) New PowerEdge(Of Integer) With {
                .source = DirectCast(e.source, Node).index,
                .target = DirectCast(e.target, Node).index
            }).ToArray
                Dim vs = Me._nodes.Select(Function(v) New Node With {
                .index = v.index
            }).ToList
                Me._groups.ForEach(Sub(g, i)
                                       g.index = n + i
                                       vs.Add(New Node With {
                                       .index = g.index
                                   })
                                   End Sub)
                Me._groups.ForEach(Sub(g, i)
                                       If g.leaves IsNot Nothing Then
                                           g.leaves.DoEach(Sub(v)
                                                               edges.Add(New PowerEdge(Of Integer) With {
                                           .source = g.index,
                                           .target = CType(v, Node).index
                                       })
                                                           End Sub)
                                       End If
                                       If g.groups IsNot Nothing Then
                                           g.groups.DoEach(Sub(gg)
                                                               edges.Add(New PowerEdge(Of Integer) With {
                                           .source = g.index,
                                           .target = CType(gg, Node).index
                                       })
                                                           End Sub)
                                       End If
                                   End Sub)

                ' layout the flat graph with dummy nodes and edges
                Call New Layout().size(Me.size()) _
                    .nodes(vs) _
                    .links(edges) _
                    .avoidOverlaps(False) _
                    .linkDistance(Me.linkDistance()) _
                    .symmetricDiffLinkLengths(5) _
                    .convergenceThreshold(0.0001) _
                    .start(iterations, 0, 0, 0, False)

                Me._nodes.DoEach(Sub(v)
                                     x(v.index) = vs(v.index).x
                                     y(v.index) = vs(v.index).y
                                 End Sub)
            Else
                Me._descent.run(iterations)
            End If
        End Sub

        ' recalculate nodes position for disconnected graphs
        Private Sub separateOverlappingComponents(width As Double, height As Double, Optional centerGraph As Boolean = True)
            ' recalculate nodes position for disconnected graphs
            If Me._distanceMatrix Is Nothing AndAlso Me._handleDisconnected Then
                Dim x = Me._descent.x(0)
                Dim y = Me._descent.x(1)
                Me._nodes.ForEach(Sub(v, i)
                                      v.x = x(i)
                                      v.y = y(i)
                                  End Sub)
                Dim graphs = Cola.handleDisconnected.separateGraphs(Me._nodes, Me._links)

                Call New handleDisconnected().applyPacking(graphs, width, height, Me._defaultNodeSize, 1, centerGraph)

                Me._nodes.ForEach(Sub(v, i)
                                      Me._descent.x(0)(i) = v.x
                                      Me._descent.x(1)(i) = v.y
                                      If v.bounds IsNot Nothing Then
                                          v.bounds.setXCentre(v.x)
                                          v.bounds.setYCentre(v.y)
                                      End If
                                  End Sub)
            End If
        End Sub

        Public Function [resume]() As Layout
            Return Me.alpha(0.1)
        End Function

        Public Function [stop]() As Layout
            Return Me.alpha(0)
        End Function

        ''' <summary>
        ''' find a visibility graph over the set of nodes.  assumes all nodes have a
        ''' bounds property (a rectangle) and that no pair of bounds overlaps.
        ''' </summary>
        ''' <param name="nodeMargin"></param>
        Private Sub prepareEdgeRouting(Optional nodeMargin As Double = 0)
            Me._visibilityGraph = New TangentVisibilityGraph(
                Me._nodes _
                  .Select(Function(v)
                              Return v.bounds.inflate(-nodeMargin).Vertices()
                          End Function))
        End Sub

        '*
        '     * find a route avoiding node bounds for the given edge.
        '     * assumes the visibility graph has been created (by prepareEdgeRouting method)
        '     * and also assumes that nodes have an index property giving their position in the
        '     * node array.  This index property is created by the start() method.
        '     * @param [edge] The edge to generate a route for.
        '     * @param {number} [ah] The size of the arrow head, a distance to shorten the end
        '     *                      of the edge by.  Defaults to 5.
        '     

        Private Function routeEdge(edge As any, Optional ah As Double = 5, Optional draw As Action(Of TangentVisibilityGraph) = Nothing) As List(Of Point2D)
            Dim lineData As List(Of Point2D)
            'if (d.source.id === 10 && d.target.id === 11) {
            '    debugger;
            '}
            Dim vg2 = New TangentVisibilityGraph(Me._visibilityGraph.P, New With {
             .V = Me._visibilityGraph.V,
             .E = Me._visibilityGraph.E
        })
            Dim port1 = New TVGPoint() With {
            .X = edge.source.x,
            .Y = edge.source.y
        }
            Dim port2 = New TVGPoint() With {
            .X = edge.target.x,
            .Y = edge.target.y
        }
            Dim start = vg2.addPoint(port1, edge.source.index)
            Dim [end] = vg2.addPoint(port2, edge.target.index)

            vg2.addEdgeIfVisible(port1, port2, edge.source.index, edge.target.index)
            Call draw(vg2)

            Dim sourceInd = Function(e As VisibilityEdge) e.source.id
            Dim targetInd = Function(e As VisibilityEdge) e.target.id
            Dim length = Function(e As VisibilityEdge) e.length
            Dim spCalc = New Dijkstra.Calculator(Of VisibilityEdge)(vg2.V.Count, vg2.E, sourceInd, targetInd, length)
            Dim shortestPath = spCalc.PathFromNodeToNode(start.id, [end].id)

            If shortestPath.Count = 1 OrElse shortestPath.Count = vg2.V.Count Then
                Dim route = makeEdgeBetween(edge.source.innerBounds, edge.target.innerBounds, ah)
                lineData = New List(Of Point2D) From {route.sourceIntersection, route.arrowStart}
            Else
                Dim n = shortestPath.Count - 2
                Dim p = vg2.V(shortestPath(n)).p
                Dim q = vg2.V(shortestPath(0)).p
                lineData = New List(Of Point2D) From {edge.source.innerBounds.rayIntersection(p.X, p.Y)}

                For i As Integer = n To 0 Step -1
                    lineData.Add(vg2.V(shortestPath(i)).p)
                Next
                lineData.Add(makeEdgeTo(q, edge.target.innerBounds, ah))
            End If
            'lineData.forEach((v, i) => {
            '    if (i > 0) {
            '        var u = lineData[i - 1];
            '        this._nodes.forEach(function (node) {
            '            if (node.id === getSourceIndex(d) || node.id === getTargetIndex(d)) return;
            '            var ints = node.innerBounds.lineIntersections(u.x, u.y, v.x, v.y);
            '            if (ints.length > 0) {
            '                debugger;
            '            }
            '        })
            '    }
            '})
            Return lineData
        End Function

        Private Shared Function getSourceIndex(e As Link(Of Integer)) As Integer
            Return e.source
        End Function

        'The link source and target may be just a node index, or they may be references to nodes themselves.
        Private Shared Function getSourceIndex(e As Link(Of Node)) As Integer
            Return e.source.index
        End Function

        'The link source and target may be just a node index, or they may be references to nodes themselves.
        Private Shared Function getTargetIndex(e As Link(Of Integer)) As Integer
            Return e.target
        End Function

        Private Shared Function getTargetIndex(e As Link(Of Node)) As Integer
            Return e.target.index
        End Function

        ' Get a string ID for a given link.
        Private Shared Function linkId(e As Link(Of Node)) As String
            Return Layout.getSourceIndex(e) & "-" & Layout.getTargetIndex(e)
        End Function

        ' The fixed property has three bits:
        ' Bit 1 can be set externally (e.g., d.fixed = true) and show persist.
        ' Bit 2 stores the dragging state, from mousedown to mouseup.
        ' Bit 3 stores the hover state, from mouseover to mouseout.
        Private Shared Sub dragStart(d As Node)
            If Node.isGroup(d) Then
                Layout.storeOffset(d, Layout.dragOrigin(d))
            Else
                Layout.stopNode(d)
                ' set bit 2
                d.fixed = d.fixed Or 2
            End If
        End Sub

        ' we clobber any existing desired positions for nodes
        ' in case another tick event occurs before the drag
        Private Shared Sub stopNode(v As Node)
            DirectCast(v, any).px = v.x
            DirectCast(v, any).py = v.y
        End Sub

        ''' <summary>
        ''' we store offsets for each node relative to the centre of the ancestor group
        ''' being dragged in a pair of properties on the node
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="origin"></param>
        Private Shared Sub storeOffset(d As Node, origin As Point2D)
            If d.leaves IsNot Nothing Then
                d.leaves.DoEach(Sub(v)
                                    With v.VB
                                        .fixed = .fixed Or 2

                                        Call Layout.stopNode(v)

                                        ._dragGroupOffsetX = .x - origin.X
                                        ._dragGroupOffsetY = .y - origin.Y
                                    End With
                                End Sub)
            End If

            If d.groups IsNot Nothing Then
                d.groups.DoEach(Sub(g) Call Layout.storeOffset(g, origin))
            End If
        End Sub

        ''' <summary>
        ''' the drag origin is taken as the centre of the node or group
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function dragOrigin(d As Node) As Point2D
            If Node.isGroup(d) Then
                Return New Point2D() With {
                   .X = d.bounds.CenterX(),
                   .Y = d.bounds.CenterY()
               }
            Else
                Return d
            End If
        End Function

        ''' <summary>
        ''' for groups, the drag translation is propagated down to all of the children of
        ''' the group.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="position"></param>
        Private Shared Sub drag(d As Node, position As Point2D)
            If Node.isGroup(d) Then
                If d.leaves IsNot Nothing Then
                    d.leaves.DoEach(Sub(v)
                                        d.bounds.setXCentre(position.X)
                                        d.bounds.setYCentre(position.Y)

                                        With v.VB
                                            .px = ._dragGroupOffsetX + position.X
                                            .py = ._dragGroupOffsetY + position.Y
                                        End With
                                    End Sub)
                End If
                If d.groups IsNot Nothing Then
                    d.groups.DoEach(Sub(g) Call Layout.drag(g, position))
                End If
            Else
                d.px = position.X
                d.py = position.Y
            End If
        End Sub

        ''' <summary>
        ''' we unset only bits 2 and 3 so that the user can fix nodes with another a different
        ''' bit such that the lock persists between drags
        ''' </summary>
        ''' <param name="d"></param>
        Private Shared Sub dragEnd(d As Node)
            If Node.isGroup(d) Then
                If d.leaves IsNot Nothing Then
                    d.leaves.DoEach(Sub(v) Call Layout.dragEnd(v))
                End If
                If d.groups IsNot Nothing Then
                    d.groups.DoEach(AddressOf Layout.dragEnd)
                End If
            Else
                ' unset bits 2 and 3
                'd.fixed = 0;
                d.fixed = d.fixed And Not 6
            End If
        End Sub

        ' in d3 hover temporarily locks nodes, currently not used in cola
        Private Shared Sub mouseOver(d As Node)
            d.fixed = d.fixed Or 4
            ' set bit 3
            d.px = d.x
            d.py = d.y
            ' set velocity to zero
        End Sub

        ' in d3 hover temporarily locks nodes, currently not used in cola
        Private Shared Sub mouseOut(d As Node)
            d.fixed = d.fixed And Not 4
            ' unset bit 3
        End Sub

    End Class
End Namespace

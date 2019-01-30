Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports any = System.Object
Imports number = System.Double

Namespace Layouts.Cola

    ''' <summary>
    ''' Main interface to cola layout.
    ''' </summary>
    Public Class Layout
        Private _canvasSize As Double() = {1, 1}
        Private _linkDistance As Double = 20
        Private _defaultNodeSize As Double = 10
        Private _linkLengthCalculator As any = Nothing
        Private _linkType As any = Nothing
        Private _avoidOverlaps As Boolean = False
        Private _handleDisconnected As Boolean = True
        Private _alpha As Double
        Private _lastStress As any
        Private _running As Boolean = False
        Private _nodes As any() = {}
        Private _groups As any() = {}
        Private _rootGroup As any = Nothing
        Private _links As Link(Of Node)() = {}
        Private _constraints As any() = {}
        Private _distanceMatrix As Double()() = Nothing
        Private _descent As Descent = Nothing
        Private _directedLinkConstraints As any = Nothing
        Private _threshold As Double = 0.01
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
                Me.[event] = New any() {}
            End If
            If e.GetType() Is GetType(String) Then
                Me.[event](EventType(e)) = listener
            Else
                Me.[event](e) = listener
            End If
            Return Me
        End Function

        ' a function that is notified of events like "tick"
        ' sub-classes can override this method to replace with a more sophisticated eventing mechanism
        Protected Sub trigger(e As [Event])
            If Me.[event] AndAlso Me.[event](e.type) IsNot Nothing Then
                Me.[event](e.type)(e)
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
                Me.trigger(New With {
                Key .type = EventType.[end],
                Key .alpha = InlineAssignHelper(Me._alpha, 0),
                Key .stress = Me._lastStress
            })
                Return True
            End If
            Dim n = Me._nodes.Length
            Dim m = Me._links.Length
            Dim o As any
            Dim i As Integer

            Me._descent.locks.clear()
            For i = 0 To n - 1
                o = Me._nodes(i)
                If o.fixed Then
                    If o.px Is Nothing OrElse o.py Is Nothing Then
                        o.px = o.x
                        o.py = o.y
                    End If
                    Dim p = New From { _
					o.px, _
					o.py _
				}
				Me._descent.locks.add(i, p)
                End If
            Next

            Dim s1 = Me._descent.rungeKutta()
            'var s1 = descent.reduceStress();
            If s1 = 0 Then
                Me._alpha = 0
            ElseIf Me._lastStress IsNot Nothing Then
                'Math.abs(Math.abs(this._lastStress / s1) - 1);
                Me._alpha = s1
            End If
            Me._lastStress = s1

            Me.updateNodePositions()

            Me.trigger(New With {
            Key .type = EventType.tick,
            Key .alpha = Me._alpha,
            Key .stress = Me._lastStress
        })
            Return False
        End Function

        ' copy positions out of descent instance into each of the nodes' center coords
        Private Sub updateNodePositions()
            Dim x = Me._descent.x(0)
            Dim y = Me._descent.x(1)
            Dim o As any
            Dim i As Integer = Me._nodes.Length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
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

        Public Function nodes() As Array(Of Node)
            If Me._nodes.Length = 0 AndAlso Me._links.Length > 0 Then
                ' if we have links but no nodes, create the nodes array now with empty objects for the links to point at.
                ' in this case the links are expected to be numeric indices for nodes in the range 0..n-1 where n is the number of nodes
                Dim n = 0
                Me._links.ForEach(Function(l)
                                      n = Math.Max(n, CType(l.source, number), CType(l.target, number))

                                  End Function)
                Me._nodes = New Array(System.Threading.Interlocked.Increment(n))
                For i As var = 0 To n - 1
                    Me._nodes(i) = New any() {}
                Next
            End If
            Return Me._nodes
        End Function
        Private Function nodes(v As Array(Of InputNode)) As Layout

            Me._nodes = v
            Return Me
        End Function

        '*
        '     * a list of hierarchical groups defined over nodes
        '     * @property groups {Array}
        '     * @default empty list
        '     

        Private Function groups() As Array(Of Group)
            Return Me._groups
        End Function
        Private Function groups(x As Array(Of Group)) As Layout

            Me._groups = x
            Me._rootGroup = New any() {}
            Me._groups.ForEach(Function(g)
                                   If g.padding Is Nothing Then
                                       g.padding = 1
                                   End If
                                   If g.leaves IsNot Nothing Then
                                       g.leaves.forEach(Function(v, i)
                                                            If GetType(v) Is Double Then
                       				(InlineAssignHelper(g.leaves(i), Me._nodes(v))).parent = g
			End If

                                                        End Function)
                                   End If
                                   If g.groups IsNot Nothing Then
                                       g.groups.forEach(Function(gi, i)
                                                            If GetType(gi) Is Double Then
                       				(InlineAssignHelper(g.groups(i), Me._groups(gi))).parent = g
			End If

                                                        End Function)
                                   End If

                               End Function)
            Me._rootGroup.leaves = Me._nodes.filter(Function(v) v.parent Is Nothing)
            Me._rootGroup.groups = Me._groups.filter(Function(g) g.parent Is Nothing)
            Return Me
        End Function

        Private Function powerGraphGroups(f As Action(Of any)) As Layout
            Dim g = powergraph.getGroups(Me._nodes, Me._links, Me.linkAccessor, Me._rootGroup)
            Me.groups(g.groups)
            f(g)
            Return Me
        End Function

        '*
        '     * if true, the layout will not permit overlaps of the node bounding boxes (defined by the width and height properties on nodes)
        '     * @property avoidOverlaps
        '     * @type bool
        '     * @default false
        '     

        Private Function avoidOverlaps() As Boolean
            Return Me._avoidOverlaps
        End Function
        Private Function avoidOverlaps(v As [Boolean]) As Layout

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

        Private Function handleDisconnected() As Boolean
            Return Me._handleDisconnected
        End Function
        Private Function handleDisconnected(v As Boolean) As Layout

            Me._handleDisconnected = v
            Return Me
        End Function

        '*
        '     * causes constraints to be generated such that directed graphs are laid out either from left-to-right or top-to-bottom.
        '     * a separation constraint is generated in the selected axis for each edge that is not involved in a cycle (part of a strongly connected component)
        '     * @param axis {string} 'x' for left-to-right, 'y' for top-to-bottom
        '     * @param minSeparation {number|link=>number} either a number specifying a minimum spacing required across all links or a function to return the minimum spacing for each link
        '     

        Private Function flowLayout(Optional axis As String = "y", Optional minSeparation As Double = 0) As Layout

            Me._directedLinkConstraints = New With {
            Key .axis = axis,
            Key .getMinSeparation = minSeparation
        }
            Return Me
        End Function

        Private Function flowLayout(Optional axis As String = "y", Optional minSeparation As Func(Of any, number) = Nothing) As Layout
            Me._directedLinkConstraints = New With {
            Key .axis = axis,
            Key .getMinSeparation = Function() minSeparation
        }
            Return Me
        End Function

        '*
        '     * links defined as source, target pairs over nodes
        '     * @property links {array}
        '     * @default empty list
        '     

        Private Function links() As Array(Of Link(Of Node))
            Return Me._links
        End Function
        Private Function links(x As Array(Of Link(Of Node))) As Layout

            Me._links = x
            Return Me
        End Function

        '*
        '     * list of constraints of various types
        '     * @property constraints
        '     * @type {array}
        '     * @default empty list
        '     

        Private Function constraints() As Array(Of any)
            Return Me._constraints
        End Function
        Private Function constraints(c As Array(Of any)) As Layout

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

        Private Function distanceMatrix() As Array(Of Array(Of number))
            Return Me._distanceMatrix
        End Function
        Private Function distanceMatrix(d As Array(Of Array(Of number))) As Layout

            Me._distanceMatrix = d
            Return Me
        End Function

        '*
        '     * Size of the layout canvas dimensions [x,y]. Currently only used to determine the midpoint which is taken as the starting position
        '     * for nodes with no preassigned x and y.
        '     * @property size
        '     * @type {Array of Number}
        '     

        Private Function size() As Array(Of number)
            Return Me._canvasSize
        End Function
        Private Function size(x As Array(Of number)) As Layout

            Me._canvasSize = x
            Return Me
        End Function

        '*
        '     * Default size (assume nodes are square so both width and height) to use in packing if node width/height are not specified.
        '     * @property defaultNodeSize
        '     * @type {Number}
        '     

        Private Function defaultNodeSize() As Double
            Return Me._defaultNodeSize
        End Function
        Private Function defaultNodeSize(x As Double) As Layout

            Me._defaultNodeSize = x
            Return Me
        End Function

        '*
        '     * The strength of attraction between the group boundaries to each other.
        '     * @property defaultNodeSize
        '     * @type {Number}
        '     

        Private Function groupCompactness() As Double
            Return Me._groupCompactness
        End Function
        Private Function groupCompactness(x As Double) As Layout

            Me._groupCompactness = x
            Return Me
        End Function

        '*
        '     * links have an ideal distance, The automatic layout will compute layout that tries to keep links (AKA edges) as close as possible to this length.
        '     

        Private Function linkDistance() As any
            Return Me._linkDistance
        End Function

        Private Function linkDistance(x As Double) As Layout
            Me._linkDistance = If(GetType(x) = [Function], x, +x)
            Me._linkLengthCalculator = Nothing
            Return Me
        End Function
        Private Function linkDistance(x As LinkNumericPropertyAccessor) As Layout
            Me._linkDistance = If(GetType(x) = [Function], x, +x)
            Me._linkLengthCalculator = Nothing
            Return Me

        End Function

        Private Function linkType(f As [Function]) As Layout
            Me._linkType = f
            Return Me
        End Function

        Private Function linkType(f As Double) As Layout
            Me._linkType = f
            Return Me
        End Function

        Private Function convergenceThreshold() As Double
            Return Me._threshold
        End Function
        Private Function convergenceThreshold(x As Double) As Layout

            Me._threshold = If(GetType(x) = [Function], x, +x)
            Return Me
        End Function

        Private Function alpha() As Double
            Return Me._alpha
        End Function
        Private Function alpha(x As Double) As Layout

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
                    Me.trigger(New With {
                    Key .type = EventType.start,
                    Key .alpha = InlineAssignHelper(Me._alpha, x)
                })
                    Me.kick()
                End If
            End If
            Return Me

        End Function

        Private Function getLinkLength(link As Link(Of Node)) As Double
            If GetType(_linkDistance) = [Function] Then
                Return DirectCast(Me._linkDistance, LinkNumericPropertyAccessor)(link)
            Else
                Return CType(Me._linkDistance, number)
            End If
        End Function

        Private Shared Sub setLinkLength(link As Link(Of Node), length As Double)
            link.length = length
        End Sub

        Private Function getLinkType(link As Link(Of Node)) As Double
            Return If(GetType(_linkType) = [Function], Me._linkType(link), 0)
        End Function

        Private linkAccessor As New LinkLengthTypeAccessor() With {
        Key.getSourceIndex = AddressOf Layout.getSourceIndex,
        Key.getTargetIndex = AddressOf Layout.getTargetIndex,
        Key.setLength = AddressOf Layout.setLinkLength,
        Key.[getType] = Function(l) If(GetType(_linkType) = [Function], Me._linkType(l), 0)
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

        Private Function symmetricDiffLinkLengths(idealLength As Double, Optional w As Double = 1) As Layout
            Me.linkDistance(Function(l) idealLength * l.length)
            Me._linkLengthCalculator = Function() symmetricDiffLinkLengths(Me._links, Me.linkAccessor, w)
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

        Private Function jaccardLinkLengths(idealLength As Double, Optional w As Double = 1) As Layout
            Me.linkDistance(Function(l) idealLength * l.length)
            Me._linkLengthCalculator = Function() jaccardLinkLengths(Me._links, Me.linkAccessor, w)
            Return Me
        End Function

        '*
        '     * start the layout process
        '     * @method start
        '     * @param {number} [initialUnconstrainedIterations=0] unconstrained initial layout iterations
        '     * @param {number} [initialUserConstraintIterations=0] initial layout iterations with user-specified constraints
        '     * @param {number} [initialAllConstraintsIterations=0] initial layout iterations with all constraints including non-overlap
        '     * @param {number} [gridSnapIterations=0] iterations of "grid snap", which pulls nodes towards grid cell centers - grid of size node[0].width - only really makes sense if all nodes have the same width and height
        '     * @param [keepRunning=true] keep iterating asynchronously via the tick method
        '     * @param [centerGraph=true] Center graph on restart
        '     

        Private Function start(Optional initialUnconstrainedIterations As Double = 0, Optional initialUserConstraintIterations As Double = 0, Optional initialAllConstraintsIterations As Double = 0, Optional gridSnapIterations As Double = 0, Optional keepRunning As Boolean = True, Optional centerGraph As Boolean = True) As Layout
            Dim n__1 = Me.nodes().length
            Dim N__2 = n__1 + 2 * Me._groups.Length
            Dim m = Me._links.Length
            Dim w = Me._canvasSize(0)
            Dim h = Me._canvasSize(1)

            Dim x = New Array(N__2)
            Dim y = New Array(N__2)

            Dim G__3 As any = Nothing

            Dim ao = Me._avoidOverlaps

            Me._nodes.ForEach(Function(v, i)
                                  v.index = i
                                  If v.x Is Nothing Then
                                      v.x = w / 2
                                      v.y = h / 2
                                  End If
                                  x(i) = v.x
                                  y(i) = v.y

                              End Function)

            If Me._linkLengthCalculator Then
                Me._linkLengthCalculator()
            End If

            'should we do this to clearly label groups?
            'this._groups.forEach((g, i) => g.groupIndex = i);

            Dim distances As any

            If Me._distanceMatrix Then
                ' use the user specified distanceMatrix
                distances = Me._distanceMatrix
            Else
                ' construct an n X n distance matrix based on shortest paths through graph (with respect to edge.length).
                distances = (New Calculator(N__2, Me._links, AddressOf Layout.getSourceIndex, AddressOf Layout.getTargetIndex, Function(l) Me.getLinkLength(l))).DistanceMatrix()

                ' G is a square matrix with G[i][j] = 1 iff there exists an edge between node i and node j
                ' otherwise 2. (
                G__3 = Descent.createSquareMatrix(N__2, Function() 2)
                Me._links.ForEach(Function(l)
                                      If GetType(l.source) Is Double Then
                                          l.source = Me._nodes(l.source)
                                      End If
                                      If GetType(l.target) Is Double Then
                                          l.target = Me._nodes(l.target)
                                      End If

                                  End Function)
                Me._links.ForEach(Function(e)
                                      Dim u = Layout.getSourceIndex(e)
                                      Dim v = Layout.getTargetIndex(e)

                                      If e.weight Then
                                          G__3(u)(v) = InlineAssignHelper(G__3(v)(u), e.weight)
                                      Else
                                          G__3(u)(v) = InlineAssignHelper(G__3(v)(u), 1)

                                      End If

                                  End Function)
            End If

            Dim D = Descent.createSquareMatrix(N__2, Function(i, j) distances(i)(j))

            If Me._rootGroup AndAlso Me._rootGroup.groups IsNot Nothing Then
                Dim i = n__1
                Dim addAttraction = Function(i, j, strength, idealDistance)
                                        G__3(i)(j) = InlineAssignHelper(G__3(j)(i), strength)
                                        D(i)(j) = InlineAssignHelper(D(j)(i), idealDistance)

                                    End Function
                Me._groups.ForEach(Function(g__4)
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
                                       y(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = 0
                                       x(i) = 0
                                       y(System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)) = 0

                                   End Function)
            Else
                Me._rootGroup = New With {
                Key .leaves = Me._nodes,
                Key .groups = New Object() {}
            }
            End If

            Dim curConstraints = If(Me._constraints Is Nothing, Me._constraints, New Object() {})
            If Me._directedLinkConstraints Then
			(Me.linkAccessor).getMinSeparation = Me._directedLinkConstraints.getMinSeparation

				' todo: add containment constraints between group dummy nodes and their children
			curConstraints = curConstraints.Concat(generateDirectedEdgeConstraints(n__1, Me._links, Me._directedLinkConstraints.axis, (Me.linkAccessor)))
            End If

            Me.avoidOverlaps(False)
            Me._descent = New Descent(New number()() {x, y}, D)

            Me._descent.locks.clear()
            For i As var = 0 To n__1 - 1
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
                Me._descent.project = New Projection(Me._nodes, Me._groups, Me._rootGroup, curConstraints).projectFunctions()
            End If
            Me._descent.run(initialUserConstraintIterations)
            Me.separateOverlappingComponents(w, h, centerGraph)

            ' subsequent iterations will apply all constraints
            Me.avoidOverlaps(ao)
            If ao Then
                Me._nodes.ForEach(Function(v, i)
                                      v.x = x(i)
                                      v.y = y(i)

                                  End Function)
                Me._descent.project = New Projection(Me._nodes, Me._groups, Me._rootGroup, curConstraints, True).projectFunctions()
                Me._nodes.ForEach(Function(v, i)
                                      x(i) = v.x
                                      y(i) = v.y

                                  End Function)
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
                Dim G0 = Descent.createSquareMatrix(N__2, Function(i, j)
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
                Dim edges = Me._links.map(Function(e) New With {
                Key .source = DirectCast(e.source, Node).index,
                Key .target = DirectCast(e.target, Node).index
            })
                Dim vs = Me._nodes.map(Function(v) New With {
                Key .index = v.index
            })
                Me._groups.ForEach(Function(g, i)
                                       vs.push(New With {
                                       Key .index = InlineAssignHelper(g.index, n + i)
                                   })

                                   End Function)
                Me._groups.ForEach(Function(g, i)
                                       If g.leaves IsNot Nothing Then
                                           g.leaves.forEach(Function(v) edges.push(New With {
                                           Key .source = g.index,
                                           Key .target = v.index
                                       }))
                                       End If
                                       If g.groups IsNot Nothing Then
                                           g.groups.forEach(Function(gg) edges.push(New With {
                                           Key .source = g.index,
                                           Key .target = gg.index
                                       }))
                                       End If

                                   End Function)

			' layout the flat graph with dummy nodes and edges
			New Layout().size(Me.size()).nodes(vs).links(edges).avoidOverlaps(False).linkDistance(Me.linkDistance()).symmetricDiffLinkLengths(5).convergenceThreshold(0.0001).start(iterations, 0, 0, 0, False)

			Me._nodes.ForEach(Function(v)
                                  x(v.index) = vs(v.index).x
                                  y(v.index) = vs(v.index).y

                              End Function)
            Else
                Me._descent.run(iterations)
            End If
        End Sub

        ' recalculate nodes position for disconnected graphs
        Private Sub separateOverlappingComponents(width As Double, height As Double, Optional centerGraph As Boolean = True)
            ' recalculate nodes position for disconnected graphs
            If Not Me._distanceMatrix AndAlso Me._handleDisconnected Then
                Dim x = Me._descent.x(0)
                Dim y = Me._descent.x(1)
                Me._nodes.ForEach(Function(v, i)
                                      v.x = x(i)
                                      v.y = y(i)

                                  End Function)
                Dim graphs = separateGraphs(Me._nodes, Me._links)
                applyPacking(graphs, width, height, Me._defaultNodeSize, 1, centerGraph)
                Me._nodes.ForEach(Function(v, i)
                                      Me._descent.x(0)(i) = v.x
                                      Me._descent.x(1)(i) = v.y
                                      If v.bounds Then
                                          v.bounds.setXCentre(v.x)
                                          v.bounds.setYCentre(v.y)
                                      End If

                                  End Function)
            End If
        End Sub

        Private Function [resume]() As Layout
            Return Me.alpha(0.1)
        End Function

        Private Function [stop]() As Layout
            Return Me.alpha(0)
        End Function

        ''' find a visibility graph over the set of nodes.  assumes all nodes have a
        ''' bounds property (a rectangle) and that no pair of bounds overlaps.
        Private Sub prepareEdgeRouting(Optional nodeMargin As Double = 0)
            Me._visibilityGraph = New TangentVisibilityGraph(Me._nodes.map(Function(v)
                                                                               Return v.bounds.inflate(-nodeMargin).vertices()

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

        Private Function routeEdge(edge As any, Optional ah As Double = 5, Optional draw As any = Nothing) As any
            Dim lineData = New Object() {}
            'if (d.source.id === 10 && d.target.id === 11) {
            '    debugger;
            '}
            Dim vg2 = New TangentVisibilityGraph(Me._visibilityGraph.P, New With {
            Key .V = Me._visibilityGraph.V,
            Key .E = Me._visibilityGraph.E
        })
            Dim port1 = New TVGPoint() With {
            Key.x = edge.source.x,
            Key.y = edge.source.y
        }
            Dim port2 = New TVGPoint() With {
            Key.x = edge.target.x,
            Key.y = edge.target.y
        }
            Dim start = vg2.addPoint(port1, edge.source.index)
            Dim [end] = vg2.addPoint(port2, edge.target.index)
            vg2.addEdgeIfVisible(port1, port2, edge.source.index, edge.target.index)
            RaiseEvent draw(vg2)
            Dim sourceInd = Function(e) e.source.id
            Dim targetInd = Function(e) e.target.id
            Dim length = Function(e) e.length()
            Dim spCalc = New Calculator(vg2.V.Length, vg2.E, sourceInd, targetInd, length)
            Dim shortestPath = spCalc.PathFromNodeToNode(start.id, [end].id)

            If shortestPath.Length = 1 OrElse shortestPath.Length = vg2.V.Length Then
                Dim route = makeEdgeBetween(edge.source.innerBounds, edge.target.innerBounds, ah)
                lineData = New any() {route.sourceIntersection, route.arrowStart}
            Else
                Dim n = shortestPath.Length - 2
                Dim p = vg2.V(shortestPath(n)).p
                Dim q = vg2.V(shortestPath(0)).p
                Dim lineData = New any() {edge.source.innerBounds.rayIntersection(p.X, p.Y)}

                For i As var = n To 0 Step -1
                    lineData.push(vg2.V(shortestPath(i)).p)
                Next
                lineData.push(makeEdgeTo(q, edge.target.innerBounds, ah))
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

        'The link source and target may be just a node index, or they may be references to nodes themselves.
        Private Shared Function getSourceIndex(e As Link(Of Node)) As Double
            Return If(GetType(e.source) Is Double, CType(e.source, number), DirectCast(e.source, Node).index)
        End Function

        'The link source and target may be just a node index, or they may be references to nodes themselves.
        Private Shared Function getTargetIndex(e As Link(Of Node)) As Double
            Return If(GetType(e.target) Is Double, CType(e.target, number), DirectCast(e.target, Node).index)
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
            Layout.stopNode(d)
            d.fixed = d.fixed Or 2
            ' set bit 2
        End Sub

        Private Shared Sub dragStart(d As Group)

            Layout.storeOffset(d, Layout.dragOrigin(d))

        End Sub

        ' we clobber any existing desired positions for nodes
        ' in case another tick event occurs before the drag
        Private Shared Sub stopNode(v As Node)
            DirectCast(v, any).px = v.x
            DirectCast(v, any).py = v.y
        End Sub

        ' we store offsets for each node relative to the centre of the ancestor group
        ' being dragged in a pair of properties on the node
        Private Shared Sub storeOffset(d As Group, origin As Point)
            If d.leaves IsNot Nothing Then
                d.leaves.ForEach(Function(v)
                                     v.fixed = v.fixed Or 2
                                     Layout.stopNode(v)
                                     DirectCast(v, any)._dragGroupOffsetX = v.x - origin.x
                                     DirectCast(v, any)._dragGroupOffsetY = v.y - origin.y

                                 End Function)
            End If
            If d.groups IsNot Nothing Then
                d.groups.ForEach(Function(g) Layout.storeOffset(g, origin))
            End If
        End Sub

        ' the drag origin is taken as the centre of the node or group
        Private Shared Function dragOrigin(d As Node) As Point
            Return d
        End Function

        Private Shared Function dragOrigin(d As Group) As Point

            Return New Point() With {
            Key.x = d.bounds.cx(),
            Key.y = d.bounds.cy()
        }

        End Function

        Private Shared Sub drag(d As Group, position As Point)
            If d.leaves IsNot Nothing Then
                d.leaves.ForEach(Function(v)
                                     d.bounds.setXCentre(position.x)
                                     d.bounds.setYCentre(position.y)
			(v).px = (v)._dragGroupOffsetX + position.x
			(v).py = (v)._dragGroupOffsetY + position.y

End Function)
            End If
            If d.groups IsNot Nothing Then
                d.groups.ForEach(Function(g) Layout.drag(g, position))
            End If
        End Sub

        ' for groups, the drag translation is propagated down to all of the children of
        ' the group.
        Private Shared Sub drag(d As Node, position As Point)

		(d).px = position.x
		(d).py = position.y

	End Sub

        ' we unset only bits 2 and 3 so that the user can fix nodes with another a different
        ' bit such that the lock persists between drags
        Private Shared Sub dragEnd(d As any)
            If isGroup(d) Then
                If d.leaves IsNot Nothing Then
                    d.leaves.forEach(Function(v)
                                         Layout.dragEnd(v)
                                         Delete(v._dragGroupOffsetX)
                                         Delete(v._dragGroupOffsetY)

                                     End Function)
                End If
                If d.groups IsNot Nothing Then
                    d.groups.forEach(AddressOf Layout.dragEnd)
                End If
            Else
                ' unset bits 2 and 3
                'd.fixed = 0;
                d.fixed = d.fixed And Not 6
            End If
        End Sub

        ' in d3 hover temporarily locks nodes, currently not used in cola
        Private Shared Sub mouseOver(d As any)
            d.fixed = d.fixed Or 4
            ' set bit 3
            d.px = d.x
            d.py = d.y
            ' set velocity to zero
        End Sub

        ' in d3 hover temporarily locks nodes, currently not used in cola
        Private Shared Sub mouseOut(d As any)
            d.fixed = d.fixed And Not 4
            ' unset bit 3
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace
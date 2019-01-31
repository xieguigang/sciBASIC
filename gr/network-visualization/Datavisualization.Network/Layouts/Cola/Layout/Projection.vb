Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language

Namespace Layouts.Cola

    Public Class Projection
        Private xConstraints As Constraint()
        Private yConstraints As Constraint()
        Private variables As Variable()

        Private nodes As GraphNode()
        Private groups As ProjectionGroup()
        Private rootGroup As ProjectionGroup
        Private avoidOverlaps As Boolean = False

        Sub New(nodes As GraphNode(),
              groups As ProjectionGroup(),
            Optional rootGroup As ProjectionGroup = Nothing,
           Optional constraints As any() = Nothing,
             Optional avoidOverlaps As Boolean = False)

            Me.nodes = nodes
            Me.groups = groups
            Me.rootGroup = rootGroup
            Me.avoidOverlaps = avoidOverlaps

            variables = nodes.Select(Function(v, i)
                                         v.variable = New IndexedVariable(i, 1)
                                         Return v.variable
                                     End Function).toarray

            If (Not constraints Is Nothing) Then createConstraints(constraints)

            If (avoidOverlaps AndAlso rootGroup AndAlso rootGroup.groups IsNot Nothing) Then
                nodes.DoEach(Sub(v)
                                 If (Not v.width OrElse Not v.height) Then
                                     ' If undefined, default to nothing
                                     v.bounds = New Rectangle2D(v.x, v.x, v.y, v.y)
                                     Return
                                 End If
                                 Dim w2 = v.width / 2, h2 = v.height / 2
                                 v.bounds = New Rectangle2D(v.x - w2, v.x + w2, v.y - h2, v.y + h2)
                             End Sub)
                computeGroupBounds(rootGroup)
                Dim i As int = nodes.Length
                groups.DoEach(Sub(g)
                                  g.minVar = New IndexedVariable(++i, If(g.stiffness <> 0, g.stiffness, 0.01))
                                  variables(i) = g.minVar
                                  g.maxVar = New IndexedVariable(++i, If(g.stiffness <> 0, g.stiffness, 0.01))
                                  variables(i) = g.maxVar
                              End Sub)
            End If
        End Sub

        Private Function createSeparation(c As any) As Constraint
            Return New Constraint(Me.nodes(c.left).variable, Me.nodes(c.right).variable, c.gap, If(c.equality IsNot Nothing, c.equality, False))
        End Function

        ' simple satisfaction of alignment constraints to ensure initial feasibility
        Private Sub makeFeasible(c As any)
            If Not Me.avoidOverlaps Then
                Return
            End If
            ' sort nodes in constraint by position (along "guideline")
            Dim axis = "x"c
            Dim [dim] = "width"
            If c.axis = "x"c Then
                axis = "y"c
                [dim] = "height"
            End If
            Dim vs As GraphNode() = c.offsets.map(Function(o) Me.nodes(o.node)).sort(Function(a, b) a(axis) - b(axis))
            Dim p As GraphNode = Nothing
            vs.ForEach(Function(v)
                           ' if two nodes overlap then shove the second one along
                           If p Then
                               Dim nextPos = p(axis) + p([dim])
                               If nextPos > v(axis) Then
                                   v(axis) = nextPos
                               End If
                           End If
                           p = v

                       End Function)
        End Sub

        Private Sub createAlignment(c As any)
            Dim u = Me.nodes(c.offsets(0).node).variable
            Me.makeFeasible(c)
            Dim cs = If(c.axis = "x", Me.xConstraints, Me.yConstraints)
            c.offsets.slice(1).forEach(Function(o)
                                           Dim v = Me.nodes(o.node).variable
                                           cs.push(New Constraint(u, v, o.offset, True))

                                       End Function)
        End Sub

        Private Sub createConstraints(constraints As any())
            Dim isSep = Function(c) c.type Is Nothing OrElse c.type = "separation"
            Me.xConstraints = constraints.filter(Function(c) c.axis = "x" AndAlso isSep(c)).map(Function(c) Me.createSeparation(c))
            Me.yConstraints = constraints.filter(Function(c) c.axis = "y" AndAlso isSep(c)).map(Function(c) Me.createSeparation(c))
            constraints.filter(Function(c) c.type = "alignment").forEach(Function(c) Me.createAlignment(c))
        End Sub

        Private Sub setupVariablesAndBounds(x0 As Number(), y0 As Number(), desired As Number(), getDesired As Func(Of GraphNode, Number))
            Me.nodes.ForEach(Function(v, i)
                                 If v.fixed Then
                                     v.variable.weight = If(v.fixedWeight, v.fixedWeight, 1000)
                                     desired(i) = getDesired(v)
                                 Else
                                     v.variable.weight = 1
                                 End If
                                 Dim w = (v.width OrElse 0) / 2
                                 Dim h = (v.height OrElse 0) / 2
                                 Dim ix = x0(i)
                                 Dim iy = y0(i)
                                 v.bounds = New Rectangle(ix - w, ix + w, iy - h, iy + h)

                             End Function)
        End Sub

        Public Sub xProject(x0 As Number(), y0 As Number(), x As Number())
            If Not Me.rootGroup AndAlso Not (Me.avoidOverlaps OrElse Me.xConstraints) Then
                Return
            End If
            Me.project(x0, y0, x0, x, Function(v) v.px, Me.xConstraints,
                generateXGroupConstraints, Function(v) v.bounds.setXCentre(InlineAssignHelper(x(v.variable.index), v.variable.position())), Function(g)
                                                                                                                                                Dim xmin = InlineAssignHelper(x(g.minVar.index), g.minVar.position())
                                                                                                                                                Dim xmax = InlineAssignHelper(x(g.maxVar.index), g.maxVar.position())
                                                                                                                                                Dim p2 = g.padding / 2
                                                                                                                                                g.bounds.x = xmin - p2
                                                                                                                                                g.bounds.X = xmax + p2

                                                                                                                                            End Function)
        End Sub

        Public Sub yProject(x0 As Number(), y0 As Number(), y As Number())
            If Not Me.rootGroup AndAlso Not Me.yConstraints Then
                Return
            End If
            Me.project(x0, y0, y0, y, Function(v) v.py, Me.yConstraints,
                generateYGroupConstraints, Function(v) v.bounds.setYCentre(InlineAssignHelper(y(v.variable.index), v.variable.position())), Function(g)
                                                                                                                                                Dim ymin = InlineAssignHelper(y(g.minVar.index), g.minVar.position())
                                                                                                                                                Dim ymax = InlineAssignHelper(y(g.maxVar.index), g.maxVar.position())
                                                                                                                                                Dim p2 = g.padding / 2
                                                                                                                                                g.bounds.y = ymin - p2


                                                                                                                                                g.bounds.Y = ymax + p2

                                                                                                                                            End Function)
        End Sub

        Public Function projectFunctions() As any()
            Return New Object() {Function(x0, y0, x) Me.xProject(x0, y0, x), Function(x0, y0, y) Me.yProject(x0, y0, y)}
        End Function

        Private Sub project(x0 As Number(), y0 As Number(), start As Number(), desired As Number(), getDesired As Func(Of GraphNode, Number), cs As Constraint(),
            generateConstraints As Func(Of ProjectionGroup, Constraint()), updateNodeBounds As Func(Of GraphNode, any), updateGroupBounds As Func(Of ProjectionGroup, any))
            Me.setupVariablesAndBounds(x0, y0, desired, getDesired)
            If Me.rootGroup AndAlso Me.avoidOverlaps Then
                computeGroupBounds(Me.rootGroup)
                cs = cs.Concat(generateConstraints(Me.rootGroup))
            End If
            Me.solve(Me.variables, cs, start, desired)
            Me.nodes.ForEach(updateNodeBounds)
            If Me.rootGroup AndAlso Me.avoidOverlaps Then
                Me.groups.ForEach(updateGroupBounds)
                computeGroupBounds(Me.rootGroup)
            End If
        End Sub

        Private Sub solve(vs As Variable(), cs As Constraint(), starting As Number(), desired As Number())
            Dim solver = New Solver(vs, cs)
            solver.setStartingPositions(starting)
            solver.setDesiredPositions(desired)
            solver.solve()
        End Sub
    End Class

    Public Class IndexedVariable : Inherits Variable
        Public index As Integer

        Sub New(index As Integer, w As Number)
            Call MyBase.New(0, w)

            Me.index = index
        End Sub
    End Class

    Public Class GraphNode : Inherits Leaf
        Public fixed As Boolean
        Public fixedWeight As Number?
        Public width As Number
        Public height As Number
        Public x As Number
        Public y As Number
        Public px As Number
        Public py As Number
    End Class
End Namespace
Imports number = System.Double
Imports any = System.Object

Namespace Layouts.Cola

    Class PositionStats
        Private AB As number = 0
        Private AD As number = 0
        Private A2 As number = 0
        Private scale As number

        Public Sub New(scale As number)
            Me.scale = scale
        End Sub

        Private Sub addVariable(v As Variable)
            Dim ai = Me.scale / v.scale
            Dim bi = v.offset / v.scale
            Dim wi = v.weight
            Me.AB += wi * ai * bi
            Me.AD += wi * ai * v.desiredPosition
            Me.A2 += wi * ai * ai
        End Sub

        Private Function getPosn() As number
            Return (Me.AD - Me.AB) / Me.A2
        End Function
    End Class

    Class Constraint
        Private lm As number
        Private active As Boolean = False
        Private unsatisfiable As Boolean = False

        Public left As Variable
        Public right As Variable
        Public gap As number
        Public equality As Boolean = False

        Public Sub New(left As Variable, right As Variable, gap As number, Optional equality As Boolean = False)
            Me.left = left
            Me.right = right
            Me.gap = gap
            Me.equality = equality
        End Sub

        Private Function slack() As number
            Return If(Me.unsatisfiable, Number.MAX_VALUE, Me.right.scale * Me.right.position() - Me.gap - Me.left.scale * Me.left.position())
        End Function
    End Class

    Class Variable
        Private offset As number = 0
        Private block As Block
        Private [cInt] As Constraint()
        Private cOut As Constraint()

        Public desiredPosition As number
        Public weight As number = 1
        Public scale As number = 1

        Public Sub New(desiredPosition As number, Optional weight As number = 1, Optional scale As number = 1)
            Me.desiredPosition = desiredPosition
            Me.weight = weight
            Me.scale = scale
        End Sub

        Private Function dfdv() As number
            Return 2.0 * Me.weight * (Me.position() - Me.desiredPosition)
        End Function

        Private Function position() As number
            Return (Me.block.ps.scale * Me.block.posn + Me.offset) / Me.scale
        End Function

        ' visit neighbours by active constraints within the same block
        Private Sub visitNeighbours(prev As Variable, f As Action(Of Constraint, Variable))
            Dim ff = Function(c, [next]) c.active AndAlso prev <> [next] AndAlso f(c, [next])
            Me.cOut.forEach(Function(c) ff(c, c.right))
            Me.cIn.forEach(Function(c) ff(c, c.left))
        End Sub
    End Class

    Class Block
        Private vars As Variable() = {}
        Private posn As number
        Private ps As PositionStats
        Private blockInd As number

        Public Sub New(v As Variable)
            v.offset = 0
            Me.ps = New PositionStats(v.scale)
            Me.addVariable(v)
        End Sub

        Private Sub addVariable(v As Variable)
            v.block = Me
            Me.vars.push(v)
            Me.ps.addVariable(v)
            Me.posn = Me.ps.getPosn()
        End Sub

        ' move the block where it needs to be to minimize cost
        Private Sub updateWeightedPosition()
            Me.ps.AB = InlineAssignHelper(Me.ps.AD, InlineAssignHelper(Me.ps.A2, 0))
            Dim i As Integer = 0, n As Integer = Me.vars.length
            While i < n
                Me.ps.addVariable(Me.vars(i))
                i += 1
            End While
            Me.posn = Me.ps.getPosn()
        End Sub

        Private Function compute_lm(v As Variable, u As Variable, postAction As Action(Of Constraint)) As number
            Dim dfdv = v.dfdv()
            v.visitNeighbours(u, Function(c, [next])
                                     Dim _dfdv = Me.compute_lm([next], v, postAction)
                                     If [next] Is c.right Then
                                         dfdv += _dfdv * c.left.scale
                                         c.lm = _dfdv
                                     Else
                                         dfdv += _dfdv * c.right.scale
                                         c.lm = -_dfdv
                                     End If
                                     postAction(c)

                                 End Function)
            Return dfdv / v.scale
        End Function

        Private Sub populateSplitBlock(v As Variable, prev As Variable)
            v.visitNeighbours(prev, Function(c, [next])
                                        [next].offset = v.offset + (If([next] Is c.right, c.gap, -c.gap))
                                        Me.addVariable([next])
                                        Me.populateSplitBlock([next], v)

                                    End Function)
        End Sub

        ' traverse the active constraint tree applying visit to each active constraint
        Private Sub traverse(visit As Func(Of Constraint, any), acc As any(), Optional v As Variable = Me.vars(0), Optional prev As Variable = Nothing)
            v.visitNeighbours(prev, Function(c, [next])
                                        acc.push(visit(c))
                                        Me.traverse(visit, acc, [next], v)

                                    End Function)
        End Sub

        ' calculate lagrangian multipliers on constraints and
        ' find the active constraint in this block with the smallest lagrangian.
        ' if the lagrangian is negative, then the constraint is a split candidate.
        Private Function findMinLM() As Constraint
            Dim m As Constraint = Nothing
            Me.compute_lm(Me.vars(0), Nothing, Function(c)
                                                   If Not c.equality AndAlso (m Is Nothing OrElse c.lm < m.lm) Then
                                                       m = c
                                                   End If

                                               End Function)
            Return m
        End Function

        Private Function findMinLMBetween(lv As Variable, rv As Variable) As Constraint
            Me.compute_lm(lv, Nothing, Function() New Object() {})
            Dim m As any = Nothing
            Me.findPath(lv, Nothing, rv, Function(c, [next])
                                             If Not c.equality AndAlso c.right Is [next] AndAlso (m Is Nothing OrElse c.lm < m.lm) Then
                                                 m = c
                                             End If

                                         End Function)
            Return m
        End Function

        Private Function findPath(v As Variable, prev As Variable, [to] As Variable, visit As Action(Of Constraint, Variable)) As Boolean
            Dim endFound = False
            v.visitNeighbours(prev, Function(c, [next])
                                        If Not endFound AndAlso ([next] Is [to] OrElse Me.findPath([next], v, [to], visit)) Then
                                            endFound = True
                                            visit(c, [next])
                                        End If

                                    End Function)
            Return endFound
        End Function

        ' Search active constraint tree from u to see if there is a directed path to v.
        ' Returns true if path is found.
        Private Function isActiveDirectedPathBetween(u As Variable, v As Variable) As Boolean
            If u Is v Then
                Return True
            End If
            Dim i = u.cOut.length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Dim c = u.cOut(i)
                If c.active AndAlso Me.isActiveDirectedPathBetween(c.right, v) Then
                    Return True
                End If
            End While
            Return False
        End Function

        ' split the block into two by deactivating the specified constraint
        Private Shared Function split(c As Constraint) As Block()
            ' DEBUG
            '                    console.log("split on " + c);
            '                    console.assert(c.active, "attempt to split on inactive constraint");
            '        DEBUG 

            c.active = False
            Return New Block() {Block.createSplitBlock(c.left), Block.createSplitBlock(c.right)}
        End Function

        Private Shared Function createSplitBlock(startVar As Variable) As Block
            Dim b = New Block(startVar)
            b.populateSplitBlock(startVar, Nothing)
            Return b
        End Function

        ' find a split point somewhere between the specified variables
        Private Function splitBetween(vl As Variable, vr As Variable) As any
            ' DEBUG
            '                    console.assert(vl.block === this);
            '                    console.assert(vr.block === this);
            '        DEBUG 

            Dim c = Me.findMinLMBetween(vl, vr)
            If c IsNot Nothing Then
                Dim bs = Block.split(c)
                Return New With {
                Key .constraint = c,
                Key .lb = bs(0),
                Key .rb = bs(1)
            }
            End If
            ' couldn't find a split point - for example the active path is all equality constraints
            Return Nothing
        End Function

        Private Sub mergeAcross(b As Block, c As Constraint, dist As number)
            c.active = True
            Dim i As Integer = 0, n As Integer = b.vars.length
            While i < n
                Dim v = b.vars(i)
                v.offset += dist
                Me.addVariable(v)
                i += 1
            End While
            Me.posn = Me.ps.getPosn()
        End Sub

        Private Function cost() As number
            Dim sum = 0
            Dim i = Me.vars.length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Dim v = Me.vars(i)
                Dim d = v.position() - v.desiredPosition
                sum += d * d * v.weight
            End While
            Return sum
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function

        ' DEBUG
        '            toString(): string {
        '                var cs = [];
        '                this.traverse(c=> c.toString() + "\n", cs)
        '                return "b"+this.blockInd + "@" + this.posn + ": vars=" + this.vars.map(v=> v.toString()+":"+v.offset) + ";\n cons=\n" + cs;
        '            }
        '    DEBUG 

    End Class
End Namespace
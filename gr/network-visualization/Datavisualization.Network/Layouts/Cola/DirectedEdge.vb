#Region "Microsoft.VisualBasic::0f06212264dcbe2c8ee471c92d47e8d9, gr\network-visualization\Datavisualization.Network\Layouts\Cola\DirectedEdge.vb"

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

    '     Structure DirectedEdge
    ' 
    ' 
    ' 
    '     Class Node
    ' 
    '         Function: makeRBTree
    ' 
    '     Class Variable
    ' 
    '         Properties: dfdv, position
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: visitNeighbours
    ' 
    '         Operators: (+2 Overloads) IsFalse, (+2 Overloads) IsTrue
    ' 
    '     Class Block
    ' 
    '         Properties: cost
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: compute_lm, createSplitBlock, findMinLM, findMinLMBetween, findPath
    '                   isActiveDirectedPathBetween, split, splitBetween
    ' 
    '         Sub: addVariable, mergeAcross, populateSplitBlock, traverse, updateWeightedPosition
    ' 
    '     Class Blocks
    ' 
    '         Properties: cost
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: insert, merge, remove, split, updateBlockPositions
    ' 
    '     Class Solver
    ' 
    '         Properties: cost
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: mostViolated, solve
    ' 
    '         Sub: satisfy, setDesiredPositions, setStartingPositions
    ' 
    '     Class PositionStats
    ' 
    '         Properties: Posn
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: addVariable
    ' 
    '     Class Constraint
    ' 
    '         Properties: slack
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports number = System.Double

Namespace Layouts.Cola

    ''' <summary>
    ''' An object with three point properties, the intersection with the
    ''' source rectangle (sourceIntersection), the intersection with then
    ''' target rectangle (targetIntersection), And the point an arrow
    ''' head of the specified size would need to start (arrowStart).
    ''' </summary>
    Public Structure DirectedEdge

        Public sourceIntersection As Point2D
        Public targetIntersection As Point2D
        Public arrowStart As Point2D

    End Structure

    Public Class Node

        Public prev As RBNode(Of Node, Object)
        Public [next] As RBNode(Of Node, Object)

        Public r As Rectangle2D
        Public pos As number

        Public Shared Function makeRBTree() As RBNode(Of Node, Object)
            Return New RBNode(Of Node, Object)(Nothing, Nothing)
        End Function

    End Class

    Public Class Variable
        Public offset As number = 0
        Public block As Block
        Public cIn As Constraint()
        Public cOut As Constraint()
        Public weight As number = 1
        Public scale As number = 1
        Public desiredPosition As number

        Public ReadOnly Property dfdv As number
            Get
                Return 2 * weight * (position - desiredPosition)
            End Get
        End Property

        Public ReadOnly Property position As number
            Get
                Return (block.ps.scale * block.posn + offset) / scale
            End Get
        End Property

        Sub New(desiredPosition As number, Optional weight As Double = 1, Optional scale As Double = 1)
            Me.desiredPosition = desiredPosition
            Me.weight = weight
            Me.scale = scale
        End Sub

        Public Sub visitNeighbours(prev As Variable, f As Action(Of Constraint, Variable))
            Dim ff = Sub(c As Constraint, [next] As Variable)
                         If c.active AndAlso Not prev Is [next] Then
                             Call f(c, [next])
                         End If
                     End Sub

            cOut.ForEach(Sub(c, i) ff(c, c.right))
            cIn.ForEach(Sub(c, i) ff(c, c.left))
        End Sub

        Public Shared Operator IsTrue(v As Variable) As Boolean
            Return Not v Is Nothing
        End Operator

        Public Shared Operator IsFalse(v As Variable) As Boolean
            Return v Is Nothing
        End Operator
    End Class

    Public Class Block
        Public ps As PositionStats
        Public posn As number
        Public vars As New List(Of Variable)
        Public blockInd As Integer

        Public ReadOnly Property cost As number
            Get
                Dim sum = 0
                Dim i As int = vars.Count

                For Each v As Variable In vars.ReverseIterator
                    Dim d = v.position - v.desiredPosition
                    sum += d * d * v.weight
                Next

                Return sum
            End Get
        End Property

        Sub New(v As Variable)
            v.offset = 0
            ps = New PositionStats(v.scale)
            addVariable(v)
        End Sub

        Private Sub addVariable(v As Variable)
            v.block = Me
            vars.Add(v)
            ps.addVariable(v)
            posn = ps.Posn
        End Sub

        ''' <summary>
        ''' move the block where it needs to be to minimize cost
        ''' </summary>
        Sub updateWeightedPosition()
            ps.AB = ps.AD = ps.A2 = 0

            For i As Integer = 0 To vars.Count - 1
                ps.addVariable(vars(i))
            Next

            posn = ps.Posn
        End Sub

        Private Function compute_lm(v As Variable, u As Variable, postAction As Action(Of Constraint)) As number
            Dim dfdv = v.dfdv

            v.visitNeighbours(u, Sub(c, [next])
                                     Dim _dfdv = compute_lm([next], v, postAction)
                                     If ([next] Is c.right) Then
                                         dfdv += _dfdv * c.left.scale
                                         c.lm = _dfdv
                                     Else
                                         dfdv += _dfdv * c.right.scale
                                         c.lm = -_dfdv
                                     End If

                                     postAction(c)
                                 End Sub)
            Return dfdv / v.scale
        End Function

        Private Sub populateSplitBlock(v As Variable, prev As Variable)
            v.visitNeighbours(prev, Sub(c, [next])
                                        [next].offset = v.offset + If([next] Is c.right, c.gap, -c.gap)
                                        addVariable([next])
                                        populateSplitBlock([next], v)
                                    End Sub)
        End Sub

        ''' <summary>
        ''' traverse the active constraint tree applying visit to each active constraint
        ''' </summary>
        ''' <param name="visit"></param>
        ''' <param name="acc"></param>
        ''' <param name="v"></param>
        ''' <param name="prev"></param>
        Public Sub traverse(visit As Func(Of Constraint, Object), acc As List(Of Object), Optional v As Variable = Nothing, Optional prev As Variable = Nothing)
            If v Is Nothing Then
                v = vars(0)
            End If

            v.visitNeighbours(prev, Sub(c, [next])
                                        acc.Add(visit(c))
                                        traverse(visit, acc, [next], v)
                                    End Sub)
        End Sub

        ''' <summary>
        ''' Calculate lagrangian multipliers on constraints And
        ''' find the active constraint in this block with the smallest lagrangian.
        ''' if the lagrangian Is negative, then the constraint Is a split candidate.
        ''' </summary>
        ''' <returns></returns>
        Public Function findMinLM() As Constraint
            Dim m As Constraint = Nothing
            compute_lm(vars(0), Nothing, Sub(c)
                                             If (Not c.equality AndAlso (m Is Nothing OrElse c.lm < m.lm)) Then
                                                 m = c
                                             End If
                                         End Sub)
            Return m
        End Function

        Private Function findMinLMBetween(lv As Variable, rv As Variable) As Constraint
            compute_lm(lv, Nothing, Sub(c)
                                        ' do nothing
                                    End Sub)
            Dim m = Nothing
            findPath(lv, Nothing, rv, Sub(c, [next])
                                          If (Not c.equality AndAlso c.right Is [next] AndAlso (m Is Nothing OrElse c.lm < m.lm)) Then
                                              m = c
                                          End If
                                      End Sub)
            Return m
        End Function

        Private Function findPath(v As Variable, prev As Variable, [to] As Variable, visit As Action(Of Constraint, Variable)) As Boolean
            Dim endFound = False
            v.visitNeighbours(prev, Sub(c, [next])
                                        If (Not endFound AndAlso ([next] Is [to] OrElse findPath([next], v, [to], visit))) Then
                                            endFound = True
                                            visit(c, [next])
                                        End If
                                    End Sub)
            Return endFound
        End Function

        ''' <summary>
        ''' Search active constraint tree from u to see if there Is a directed path to v.
        ''' Returns true if path Is found.
        ''' </summary>
        ''' <param name="u"></param>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Function isActiveDirectedPathBetween(u As Variable, v As Variable) As Boolean
            If (u Is v) Then Return True
            Dim i As int = u.cOut.Length

            While --i
                Dim C = u.cOut(i)
                If (C.active AndAlso isActiveDirectedPathBetween(C.right, v)) Then
                    Return True
                End If
            End While

            Return False
        End Function

        ''' <summary>
        ''' split the block into two by deactivating the specified constraint
        ''' </summary>
        ''' <param name="C"></param>
        ''' <returns></returns>
        Public Shared Function split(C As Constraint) As Block()
            C.active = False
            Return {Block.createSplitBlock(C.left), Block.createSplitBlock(C.right)}
        End Function

        Private Shared Function createSplitBlock(startVar As Variable) As Block
            Dim b = New Block(startVar)
            b.populateSplitBlock(startVar, Nothing)
            Return b
        End Function

        ''' <summary>
        ''' find a split point somewhere between the specified variables
        ''' </summary>
        ''' <param name="vl"></param>
        ''' <param name="vr"></param>
        ''' <returns></returns>
        Public Function splitBetween(vl As Variable, vr As Variable) As (constraint As Constraint, lb As Block, rb As Block)
            Dim C = findMinLMBetween(vl, vr)
            If (Not C Is Nothing) Then
                Dim bs = Block.split(C)
                Return (C, bs(0), bs(1))
            End If
            ' couldn't find a split point - for example the active path is all equality constraints
            Return Nothing
        End Function

        Public Sub mergeAcross(b As Block, c As Constraint, dist As number)
            c.active = True
            For i As Integer = 0 To b.vars.Count - 1
                Dim v = b.vars(i)
                v.offset += dist
                addVariable(v)
            Next

            posn = ps.Posn
        End Sub
    End Class

    Public Class Blocks : Implements IEnumerable(Of Block)

        Dim list As New List(Of Block)
        Public vs As Variable()

        Public ReadOnly Property cost As number
            Get
                Return Aggregate b In list.ReverseIterator Into Sum(b.cost)
            End Get
        End Property

        Sub New(vs As Variable())
            Dim n As int = vs.Length

            While (--n)
                Dim b As New Block(vs(n))
                list(CInt(n)) = b
                b.blockInd = n
            End While
        End Sub

        Public Sub insert(b As Block)

            b.blockInd = list.Count
            list.Add(b)

        End Sub

        Public Sub remove(b As Block)
            Dim last = list.Count - 1
            Dim swapBlock = list(last)

            list.RemoveLast

            If (Not b Is swapBlock) Then
                list(b.blockInd) = swapBlock
                swapBlock.blockInd = b.blockInd
            End If
        End Sub

        ''' <summary>
        ''' merge the blocks on either side of the specified constraint, by copying the smaller block into the larger
        ''' And deleting the smaller.
        ''' </summary>
        ''' <param name="c"></param>
        Public Sub merge(c As Constraint)
            Dim l = c.left.block, r = c.right.block
            Dim dist = c.right.offset - c.left.offset - c.gap

            If (l.vars.Count < r.vars.Count) Then
                r.mergeAcross(l, c, dist)
                remove(l)
            Else
                l.mergeAcross(r, c, -dist)
                remove(r)
            End If
        End Sub

        ''' <summary>
        ''' useful, for example, after variable desired positions change.
        ''' </summary>
        Public Sub updateBlockPositions()
            For Each b In list
                b.updateWeightedPosition()
            Next
        End Sub

        ''' <summary>
        ''' split each block across its constraint with the minimum lagrangian
        ''' </summary>
        ''' <param name="inactive"></param>
        Public Sub split(ByRef inactive As List(Of Constraint))
            updateBlockPositions()
            For Each b In list
                Dim v = b.findMinLM()
                If (Not v Is Nothing AndAlso v.lm < Solver.LAGRANGIAN_TOLERANCE) Then
                    b = v.left.block
                    For Each nb In Block.split(v)
                        insert(nb)
                    Next

                    remove(b)
                    inactive.Add(v)

                End If
            Next
        End Sub

        ''' <summary>
        ''' For Each
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Block) Implements IEnumerable(Of Block).GetEnumerator
            For Each b As Block In list
                Yield b
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class


    Public Class Solver
        Public bs As Blocks
        Public inactive As List(Of Constraint)

        Public Const LAGRANGIAN_TOLERANCE = -0.0001
        Public Const ZERO_UPPERBOUND = -0.0000000001

        Public vs As Variable()
        Public cs As Constraint()

        Public ReadOnly Property cost As number
            Get
                Return bs.cost()
            End Get
        End Property

        Sub New(vs As Variable(), cs As Constraint())
            Me.vs = vs
            vs.ForEach(Sub(v, i)
                           v.cIn = {}
                           v.cOut = {}

                       End Sub)
            Me.cs = cs
            cs.ForEach(Sub(C, i)
                           C.left.cOut.Add(C)
                           C.right.cIn.Add(C)
                       End Sub)

            inactive = cs.Select(Function(C)
                                     C.active = False
                                     Return C
                                 End Function).AsList
            bs = Nothing
        End Sub

        ''' <summary>
        ''' set starting positions without changing desired positions.
        ''' Note: it throws away any previous block Structure.
        ''' </summary>
        ''' <param name="ps"></param>
        Public Sub setStartingPositions(ps As number())
            inactive = cs.Select(Function(c)
                                     c.active = False
                                     Return c
                                 End Function).AsList
            bs = New Blocks(vs)
            bs.ForEach(Sub(b, i)
                           b.posn = ps(i)
                       End Sub)
        End Sub

        Public Sub setDesiredPositions(ps As number())
            vs.ForEach(Sub(v, i)
                           v.desiredPosition = ps(i)
                       End Sub)
        End Sub

        Private Function mostViolated() As Constraint
            Dim minSlack = number.MaxValue
            Dim v As Constraint = Nothing
            Dim l = inactive
            Dim n = l.Count
            Dim deletePoint = n

            For i As Integer = 0 To n - 1
                Dim c = l(i)
                If (c.unsatisfiable) Then Continue For
                Dim slack = c.slack()
                If (c.equality OrElse slack < minSlack) Then
                    minSlack = slack
                    v = c
                    deletePoint = i
                    If (c.equality) Then Exit For
                End If
            Next

            If (deletePoint <> n AndAlso (minSlack < Solver.ZERO_UPPERBOUND AndAlso Not v.active OrElse v.equality)) Then

                l(deletePoint) = l(n - 1)
                l.RemoveLast
            End If

            Return v
        End Function

        ''' <summary>
        ''' satisfy constraints by building block structure over violated constraints 
        ''' And moving the blocks to their desired positions
        ''' </summary>
        Public Sub satisfy()
            If (bs Is Nothing) Then
                bs = New Blocks(vs)
            End If

            bs.split(inactive)
            Dim v As Constraint = Nothing

            Do While ((v Is mostViolated()) AndAlso (v.equality OrElse v.slack() < Solver.ZERO_UPPERBOUND AndAlso Not v.active))
                Dim lb = v.left.block
                Dim rb = v.right.block

                If (Not lb Is rb) Then
                    bs.merge(v)
                Else
                    If (lb.isActiveDirectedPathBetween(v.right, v.left)) Then
                        ' cycle found!
                        v.unsatisfiable = True
                        Continue Do
                    End If

                    ' constraint Is within block, need to split first
                    Dim Split = lb.splitBetween(v.left, v.right)
                    If (Split.constraint Is Nothing) Then
                        bs.insert(Split.lb)
                        bs.insert(Split.rb)
                        bs.remove(lb)
                        inactive.Add(Split.constraint)
                    Else

                        v.unsatisfiable = True
                        Continue Do
                    End If

                    If (v.slack() >= 0) Then

                        ' v was satisfied by the above split!
                        inactive.Add(v)
                    Else

                        bs.merge(v)
                    End If
                End If

            Loop

        End Sub

        ''' <summary>
        ''' repeatedly build And split block structure until we converge to an optimal solution
        ''' </summary>
        ''' <returns></returns>
        Public Function solve() As number
            satisfy()
            Dim lastcost = number.MaxValue, cost = bs.cost()
            While (Math.Abs(lastcost - cost) > 0.0001)
                satisfy()
                lastcost = cost
                cost = bs.cost()
            End While
            Return cost
        End Function
    End Class

    Public Class PositionStats
        Public scale As number

        Public AB As number = 0
        Public AD As number = 0
        Public A2 As number = 0

        Public ReadOnly Property Posn As number
            Get
                Return (AD - AB) / A2
            End Get
        End Property

        Sub New(scale As number)
            Me.scale = scale
        End Sub

        Public Sub addVariable(v As Variable)
            Dim ai = scale / v.scale
            Dim bi = v.offset / v.scale
            Dim wi = v.weight

            AB += wi * ai * bi
            AD += wi * ai * v.desiredPosition
            A2 += wi * ai * ai
        End Sub
    End Class

    Public Class Constraint
        Public lm As number
        Public active As Boolean = False
        Public unsatisfiable As Boolean = False

        Public left As Variable
        Public right As Variable
        Public gap As number
        Public equality As Boolean = False

        Sub New(left As Variable, right As Variable, gap As number, Optional equality As Boolean = False)
            Me.left = left
            Me.right = right
            Me.gap = gap
            Me.equality = equality
        End Sub

        Public ReadOnly Property slack As number
            Get
                If unsatisfiable Then
                    Return number.MaxValue
                Else
                    Return right.scale * right.position - gap - left.scale * left.position
                End If
            End Get
        End Property
    End Class
End Namespace

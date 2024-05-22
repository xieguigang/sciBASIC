#Region "Microsoft.VisualBasic::8f00ff9a60ec340de30c10d5c51280fa, gr\network-visualization\network_layout\Cola\Models\Block.vb"

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

    '   Total Lines: 200
    '    Code Lines: 136 (68.00%)
    ' Comment Lines: 36 (18.00%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 28 (14.00%)
    '     File Size: 7.86 KB


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
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace Cola

    Public Class Block
        Public ps As PositionStats
        Public posn As Double
        Public vars As New List(Of Variable)
        Public blockInd As Integer

        Public ReadOnly Property cost As Double
            Get
                Dim sum = 0
                Dim i As i32 = vars.Count

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

        Private Function compute_lm(v As Variable, u As Variable, postAction As Action(Of Constraint)) As Double
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
            Dim i As i32 = u.cOut.Length

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

        Public Sub mergeAcross(b As Block, c As Constraint, dist As Double)
            c.active = True
            For i As Integer = 0 To b.vars.Count - 1
                Dim v = b.vars(i)
                v.offset += dist
                addVariable(v)
            Next

            posn = ps.Posn
        End Sub
    End Class

End Namespace

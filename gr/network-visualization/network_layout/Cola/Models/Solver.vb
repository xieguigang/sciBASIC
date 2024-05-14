#Region "Microsoft.VisualBasic::5e42f293398a742a224bc70982c48b01, gr\network-visualization\network_layout\Cola\Models\Solver.vb"

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

    '   Total Lines: 168
    '    Code Lines: 121
    ' Comment Lines: 16
    '   Blank Lines: 31
    '     File Size: 5.45 KB


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
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Cola

    Public Class Solver
        Public bs As Blocks
        Public inactive As List(Of Constraint)

        Public Const LAGRANGIAN_TOLERANCE = -0.0001
        Public Const ZERO_UPPERBOUND = -0.0000000001

        Public vs As Variable()
        Public cs As List(Of Constraint)

        Public ReadOnly Property cost As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return bs.cost()
            End Get
        End Property

        Sub New(vs As Variable(), cs As IEnumerable(Of Constraint))
            Me.vs = vs

            Me.vs.ForEach(Sub(v, i)
                              v.cIn = {}
                              v.cOut = {}
                          End Sub)
            Me.cs = cs.ToList
            Me.cs.ForEach(Sub(C, i)
                              C.left.cOut.Add(C)
                              C.right.cIn.Add(C)
                          End Sub)

            inactive = Me.cs _
                .Select(Function(C)
                            C.active = False
                            Return C
                        End Function) _
                .AsList

            bs = Nothing
        End Sub

        ''' <summary>
        ''' set starting positions without changing desired positions.
        ''' Note: it throws away any previous block Structure.
        ''' </summary>
        ''' <param name="ps"></param>
        Public Sub setStartingPositions(ps As Double())
            inactive = cs.Select(Function(c)
                                     c.active = False
                                     Return c
                                 End Function).AsList
            bs = New Blocks(vs)
            bs.ForEach(Sub(b, i)
                           b.posn = ps(i)
                       End Sub)
        End Sub

        Public Sub setDesiredPositions(ps As Double())
            vs.ForEach(Sub(v, i)
                           v.desiredPosition = ps(i)
                       End Sub)
        End Sub

        Private Function mostViolated() As Constraint
            Dim minSlack = Double.MaxValue
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
        Public Function solve() As Double
            Dim lastcost = Double.MaxValue
            Dim cost#

            satisfy()
            cost = bs.cost()

            While (stdNum.Abs(lastcost - cost) > 0.0001)
                satisfy()
                lastcost = cost
                cost = bs.cost()
            End While
            Return cost
        End Function
    End Class

End Namespace

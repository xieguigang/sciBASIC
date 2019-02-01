Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.JavaScript

Namespace Layouts.Cola

    Public Class RectAccessors
        Public getCentre As Func(Of Rectangle2D, Double)
        Public getOpen As Func(Of Rectangle2D, Double)
        Public getClose As Func(Of Rectangle2D, Double)
        Public getSize As Func(Of Rectangle2D, Double)
        Public makeRect As Func(Of Double, Double, Double, Double, Rectangle2D)
        Public findNeighbours As Action(Of Node, RBTree(Of Integer, Node))
    End Class

    Module ProjectionExtensions

        Private xRect As New RectAccessors() With {
            .getCentre = Function(r) r.CenterX,
            .getOpen = Function(r) r.Y,
            .getClose = Function(r) r.Y,
            .getSize = Function(r) r.Width(),
            .makeRect = Function(open, close, center, size) New Rectangle2D(center - size / 2, center + size / 2, open, close),
            .findNeighbours = AddressOf findXNeighbours
        }

        Private yRect As New RectAccessors() With {
            .getCentre = Function(r) r.CenterY,
            .getOpen = Function(r) r.X,
            .getClose = Function(r) r.X,
            .getSize = Function(r) r.Height(),
            .makeRect = Function(open, close, center, size) New Rectangle2D(open, close, center - size / 2, center + size / 2),
            .findNeighbours = AddressOf findYNeighbours
        }

        Private Function generateGroupConstraints(root As ProjectionGroup, d As RectAccessors, minSep As Double, Optional isContained As Boolean = False) As Constraint()
            Dim padding = root.padding, gn = If(root.groups IsNot Nothing, root.groups.Length, 0), ln = If(root.leaves IsNot Nothing, root.leaves.Length, 0)
            Dim childConstraints As Constraint() = If(Not gn, New Constraint() {}, root.groups.Reduce(Function(ccs, g) ccs.Concat(generateGroupConstraints(g, f, minSep, True)), New Constraint() {}))
            Dim n = (If(isContained, 2, 0)) + ln + gn
            Dim vs As Variable() = New Variable(n) {}
            Dim rs As Rectangle2D() = New Rectangle2D(n) {}
            Dim i = 0
            Dim add = Sub(r, v)
                          rs(i) = r
                          vs(System.Math.Max(Interlocked.Increment(i), i - 1)) = v
                      End Sub
            If isContained Then
                ' if this group is contained by another, then we add two dummy vars and rectangles for the borders
                Dim b As Rectangle2D = root.bounds
                Dim c = f.getCentre(b), s = f.getSize(b) / 2, open = f.getOpen(b), close = f.getClose(b), min = c - s + padding / 2, max = c + s - padding / 2
                root.minVar.desiredPosition = min
                add(f.makeRect(open, close, min, padding), root.minVar)
                root.maxVar.desiredPosition = max
                add(f.makeRect(open, close, max, padding), root.maxVar)
            End If
            If ln Then
                root.leaves.DoEach(Sub(l) add(l.bounds, l.variable))
            End If
            If gn Then
                root.groups.DoEach(Sub(g)
                                       Dim b As Rectangle2D = g.bounds
                                       add(f.makeRect(f.getOpen(b), f.getClose(b), f.getCentre(b), f.getSize(b)), g.minVar)
                                   End Sub)
            End If
            Dim cs = generateConstraints(rs, vs, f, minSep)
            If gn Then
                vs.DoEach(Sub(v)
                              v.cOut = {}
                              v.cIn = {}
                          End Sub)
                cs.DoEach(Sub(c)
                              c.left.cOut.Add(c)
                              c.right.cIn.Add(c)
                          End Sub)
                root.groups.DoEach(Sub(g)
                                       Dim gapAdjustment = (g.padding - f.getSize(g.bounds)) / 2
                                       g.minVar.cIn.DoEach(Sub(c) c.gap += gapAdjustment)
                                       g.minVar.cOut.DoEach(Sub(c)
                                                                c.left = g.maxVar
                                                                c.gap += gapAdjustment
                                                            End Sub)
                                   End Sub)
            End If
            Return childConstraints.Concat(cs)
        End Function

        Private Function generateConstraints(rs As Rectangle2D(), vars As Variable(), rect As RectAccessors, minSep As Double) As Constraint()
            Dim n__1 = rs.Length
            Dim N__2 = 2 * n__1
            Dim events = New [Event](N__2) {}
            For i As Integer = 0 To n__1 - 1
                Dim r = rs(i)
                Dim v = New Node(vars(i), r, rect.getCentre(r))
                events(i) = New [Event](True, v, rect.getOpen(r))
                events(i + n__1) = New [Event](False, v, rect.getClose(r))
            Next

            Call events.Sort(compareEvents)

            Dim cs As New List(Of Constraint)
            Dim scanline = Node.makeRBTree()

            For i As Integer = 0 To N__2 - 1
                Dim e = events(i)
                Dim v As Node = e.v
                If e.isOpen Then
                    scanline.Insert(v.id, v)
                    rect.findNeighbours(v, scanline)
                Else
                    ' close event
                    scanline.Remove(v.id)
                    Dim makeConstraint = Sub(l, r)
                                             Dim sep = (rect.getSize(l.r) + rect.getSize(r.r)) / 2 + minSep
                                             cs.Add(New Constraint(l.v, r.v, sep))
                                         End Sub
                    Dim visitNeighbours As Action(Of String, String, Action(Of Object, Object)) =
                        Sub(forward, reverse, mkcon)
                            Dim u As New Value(Of Object)
                            Dim it = v(forward).Iterator()

                            While (u = it(forward)()) IsNot Nothing
                                mkcon(u, v)
                                u.Value(reverse).remove(v)
                            End While

                        End Sub

                    visitNeighbours("prev", "next", Sub(u, vi) makeConstraint(u, vi))
                    visitNeighbours("next", "prev", Sub(u, vi) makeConstraint(vi, u))
                End If
            Next

            Return cs
        End Function

        Private Sub findXNeighbours(v As Node, scanline As RBTree(Of Integer, Node))
            Dim f = Sub(forward As String, reverse As String)
                        Dim it = scanline.findIter(v.id)
                        Dim u As New Value(Of Object)

                        While (u = it(forward)()) IsNot Nothing
                            Dim uovervX = u.r.overlapX(v.r)
                            If uovervX <= 0 OrElse uovervX <= u.r.overlapY(v.r) Then
                                v(forward).Insert(u)
                                u(reverse).insert(v)
                            End If
                            If uovervX <= 0 Then
                                Exit While
                            End If
                        End While
                    End Sub

            f("next", "prev")
            f("prev", "next")
        End Sub

        Private Sub findYNeighbours(v As Node, scanline As RBTree(Of Integer, Node))
            Dim f = Sub(forward As String, reverse As String)
                        Dim u = scanline.findIter(v.id)(forward)()

                        If u IsNot Nothing AndAlso u.r.overlapX(v.r) > 0 Then
                            v(forward).Insert(u)
                            u(reverse).insert(v)
                        End If
                    End Sub

            f("next", "prev")
            f("prev", "next")
        End Sub

        Public Function generateXConstraints(rs As Rectangle2D(), vars As Variable()) As Constraint()
            Return generateConstraints(rs, vars, xRect, 0.000001)
        End Function

        Public Function generateYConstraints(rs As Rectangle2D(), vars As Variable()) As Constraint()
            Return generateConstraints(rs, vars, yRect, 0.000001)
        End Function

        Public Function generateXGroupConstraints(root As ProjectionGroup) As Constraint()
            Return generateGroupConstraints(root, xRect, 0.000001)
        End Function

        Public Function generateYGroupConstraints(root As ProjectionGroup) As Constraint()
            Return generateGroupConstraints(root, yRect, 0.000001)
        End Function

        Private Sub removeOverlaps(rs As Rectangle2D())
            Dim vs = rs.Select(Function(r) New Variable(r.CenterX())).ToArray
            Dim cs = generateXConstraints(rs, vs)
            Dim solver = New Solver(vs, cs)
            solver.solve()
            vs.ForEach(Sub(v, i) rs(i).setXCentre(v.position()))
            vs = rs.Select(Function(r) New Variable(r.CenterY()))
            cs = generateYConstraints(rs, vs)
            solver = New Solver(vs, cs)
            solver.solve()
            vs.ForEach(Sub(v, i) rs(i).setYCentre(v.position()))
        End Sub
    End Module
End Namespace
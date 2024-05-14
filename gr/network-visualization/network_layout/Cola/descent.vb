#Region "Microsoft.VisualBasic::73591468d066fd3da98298da82ba578e, gr\network-visualization\network_layout\Cola\descent.vb"

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

    '   Total Lines: 482
    '    Code Lines: 348
    ' Comment Lines: 84
    '   Blank Lines: 50
    '     File Size: 19.56 KB


    '     Class Descent
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: computeStepSize, computeStress, createSquareMatrix, dotProd, offsetDir
    '                   reduceStress, run, rungeKutta
    ' 
    '         Sub: computeDerivatives, computeNextPosition, copy, mApply, matrixApply
    '              mid, rightMultiply, stepAndProject, takeDescentStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Math
Imports stdNum = System.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Cola

    '*
    ' * Uses a gradient descent approach to reduce a stress or p-stress goal function over a graph with specified ideal edge lengths or a square matrix of dissimilarities.
    ' * The standard stress function over a graph nodes with position vectors x,y,z is (mathematica input):
    ' *   stress[x_,y_,z_,D_,w_]:=Sum[w[[i,j]] (length[x[[i]],y[[i]],z[[i]],x[[j]],y[[j]],z[[j]]]-d[[i,j]])^2,{i,Length[x]-1},{j,i+1,Length[x]}]
    ' * where: D is a square matrix of ideal separations between nodes, w is matrix of weights for those separations
    ' *        length[x1_, y1_, z1_, x2_, y2_, z2_] = Sqrt[(x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2]
    ' * below, we use wij = 1/(Dij^2)
    ' *
    ' * @class Descent
    ' 


    '*
    ' * Uses a gradient descent approach to reduce a stress or p-stress goal function over a graph with specified ideal edge lengths or a square matrix of dissimilarities.
    ' * The standard stress function over a graph nodes with position vectors x,y,z is (mathematica input):
    ' *   stress[x_,y_,z_,D_,w_]:=Sum[w[[i,j]] (length[x[[i]],y[[i]],z[[i]],x[[j]],y[[j]],z[[j]]]-d[[i,j]])^2,{i,Length[x]-1},{j,i+1,Length[x]}]
    ' * where: D is a square matrix of ideal separations between nodes, w is matrix of weights for those separations
    ' *        length[x1_, y1_, z1_, x2_, y2_, z2_] = Sqrt[(x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2]
    ' * below, we use wij = 1/(Dij^2)
    ' *
    ' * @class Descent
    ' 

    Public Class Descent
        Public threshold As Double = 0.0001
        '* Hessian Matrix
        '     * @property H {number[][][]}
        '     

        Public H As Double()()()
        '* gradient vector
        '     * @property G {number[][]}
        '     

        Public g As Double()()
        '* positions vector
        '     * @property x {number[][]}
        '     

        Public x As Double()()
        '*
        '     * @property k {number} dimensionality
        '     

        Public k As Integer
        '*
        '     * number of data-points / nodes / size of vectors/matrices
        '     * @property n {number}
        '     

        Public n As Integer

        Public locks As Locks

        Private Shared zeroDistance As Double = 0.0000000001
        Private minD As Double

        ' pool of arrays of size n used internally, allocated in constructor
        Private Hd As Double()()
        Private a As Double()()
        Private b As Double()()
        Private c As Double()()
        Private d As Double()()
        Private e As Double()()
        Private ia As Double()()
        Private ib As Double()()
        Private xtmp As Double()()


        ' Parameters for grid snap stress.
        ' TODO: Make a pluggable "StressTerm" class instead of this
        ' mess.
        Public numGridSnapNodes As Double = 0
        Public snapGridSize As Double = 100
        Public snapStrength As Double = 1000
        Public scaleSnapByMaxH As Boolean = False

        Private random As Random = randf.seeds

        Public project As Action(Of Double(), Double(), Double())() = Nothing


        Public Dmatrix As Double()()
        Public Gmatrix As Double()()

        '*
        '     * @method constructor
        '     * @param x {number[][]} initial coordinates for nodes
        '     * @param D {number[][]} matrix of desired distances between pairs of nodes
        '     * @param G {number[][]} [default=null] if specified, G is a matrix of weights for goal terms between pairs of nodes.
        '     * If G[i][j] > 1 and the separation between nodes i and j is greater than their ideal distance, then there is no contribution for this pair to the goal
        '     * If G[i][j] <= 1 then it is used as a weighting on the contribution of the variance between ideal and actual separation between i and j to the goal function
        '     

        Public Sub New(x As Double()(), Dmatrix As Double()(), Optional G As Double()() = Nothing)
            Me.x = x
            Me.k = x.Length
            ' dimensionality
            Me.n = x(0).Length
            ' number of nodes
            Me.H = New Double(Me.k)()() {}
            Me.g = New Double(Me.k)() {}
            Me.Hd = New Double(Me.k)() {}
            Me.a = New Double(Me.k)() {}
            Me.b = New Double(Me.k)() {}
            Me.c = New Double(Me.k)() {}
            Me.d = New Double(Me.k)() {}
            Me.e = New Double(Me.k)() {}
            Me.ia = New Double(Me.k)() {}
            Me.ib = New Double(Me.k)() {}
            Me.xtmp = New Double(Me.k)() {}
            Me.locks = New Locks()
            Me.minD = Double.MaxValue
            Dim i As Integer = n
            Dim j As Integer
            While System.Math.Max(Interlocked.Decrement(i), i + 1)
                j = n
                While Interlocked.Decrement(j) > i
                    Dim d = Dmatrix(i)(j)
                    If d > 0 AndAlso d < Me.minD Then
                        Me.minD = d
                    End If
                End While
            End While
            If Me.minD = Double.MaxValue Then
                Me.minD = 1
            End If
            i = Me.k
            While System.Math.Max(Interlocked.Decrement(i), i + 1)
                Me.g(i) = New Double(Me.k) {}
                Me.H(i) = New Double(Me.k)() {}
                j = n
                While System.Math.Max(Interlocked.Decrement(j), j + 1)
                    Me.H(i)(j) = New Double(Me.k) {}
                End While
                Me.Hd(i) = New Double(Me.k) {}
                Me.a(i) = New Double(Me.k) {}
                Me.b(i) = New Double(Me.k) {}
                Me.c(i) = New Double(Me.k) {}
                Me.d(i) = New Double(Me.k) {}
                Me.e(i) = New Double(Me.k) {}
                Me.ia(i) = New Double(Me.k) {}
                Me.ib(i) = New Double(Me.k) {}
                Me.xtmp(i) = New Double(Me.k) {}
            End While
        End Sub

        Public Shared Function createSquareMatrix(n As Integer, f As Func(Of Integer, Integer, Integer)) As Double()()
            Dim M As New List(Of Double())

            For i As Integer = 0 To n - 1
                M.Add(New Double(n - 1) {})
                For j As Integer = 0 To n - 1
                    M(i)(j) = f(i, j)
                Next
            Next

            Return M.ToArray
        End Function

        Private Function offsetDir() As Double()
            Dim u = New Double(Me.k) {}
            Dim l# = 0
            Dim x#

            For i As Integer = 0 To Me.k - 1
                u(i) = Me.random.GetNextBetween(0.01, 1) - 0.5
                x = u(i)
                l += x * x
            Next

            l = stdNum.Sqrt(l)

            Return u.Select(Function(xi) xi * Me.minD / l).ToArray
        End Function

        ' compute first and second derivative information storing results in this.g and this.H
        Public Sub computeDerivatives(x As Double()())
            Dim n As Integer = Me.n
            If n < 1 Then
                Return
            End If
            Dim i As Integer
            ' DEBUG
            '                    for (var u: number = 0; u < n; ++u)
            '                        for (i = 0; i < this.k; ++i)
            '                            if (isNaN(x[i][u])) debugger;
            '        DEBUG 

            Dim d__1 As Double() = New Double(Me.k) {}
            Dim d2__2 As Double() = New Double(Me.k) {}
            Dim Huu As Double() = New Double(Me.k) {}
            Dim maxH As Double = 0

            For u As Integer = 0 To n - 1
                For i = 0 To Me.k - 1
                    Me.g(i)(u) = 0
                    Huu(i) = 0
                Next
                For v As Integer = 0 To n - 1
                    If u = v Then
                        Continue For
                    End If

                    ' The following loop randomly displaces nodes that are at identical positions
                    Dim maxDisplaces = n
                    ' avoid infinite loop in the case of numerical issues, such as huge values
                    Dim sd2 = 0.0
                    While System.Math.Max(Interlocked.Decrement(maxDisplaces), maxDisplaces + 1)
                        sd2 = 0.0
                        For i = 0 To Me.k - 1
                            Dim dx#
                            d__1(i) = x(i)(u) - x(i)(v)
                            dx = d__1(i)
                            d2__2(i) = dx * dx
                            sd2 += d2__2(i)
                        Next
                        If sd2 > 0.000000001 Then
                            Exit While
                        End If
                        Dim rd = Me.offsetDir()
                        For i = 0 To Me.k - 1
                            x(i)(v) += rd(i)
                        Next
                    End While
                    Dim l As Double = stdNum.Sqrt(sd2)
                    Dim D__3 As Double = Me.Dmatrix(u)(v)
                    Dim weight = If(Me.Gmatrix IsNot Nothing, Me.Gmatrix(u)(v), 1)
                    If weight > 1 AndAlso l > D__3 OrElse Not D__3.IsNaNImaginary Then
                        For i = 0 To Me.k - 1
                            Me.H(i)(u)(v) = 0
                        Next
                        Continue For
                    End If
                    If weight > 1 Then
                        weight = 1
                    End If
                    Dim D2__4 As Double = D__3 * D__3
                    Dim gs As Double = 2 * weight * (l - D__3) / (D2__4 * l)
                    Dim l3 = l * l * l
                    Dim hs As Double = 2 * -weight / (D2__4 * l3)

                    For i = 0 To Me.k - 1
                        Me.g(i)(u) += d__1(i) * gs
                        Me.H(i)(u)(v) = hs * (l3 + D__3 * (d2__2(i) - sd2) + l * sd2)
                        Huu(i) -= Me.H(i)(u)(v)
                    Next
                Next
                For i = 0 To Me.k - 1
                    Me.H(i)(u)(u) = Huu(i)
                    maxH = stdNum.Max(maxH, Huu(i))
                Next
            Next
            ' Grid snap forces
            Dim r = Me.snapGridSize / 2
            Dim g = Me.snapGridSize
            Dim w = Me.snapStrength
            Dim k = w / (r * r)
            Dim numNodes = Me.numGridSnapNodes
            'var numNodes = n;
            For u As Integer = 0 To numNodes - 1
                For i = 0 To Me.k - 1
                    Dim xiu = Me.x(i)(u)
                    Dim m = xiu / g
                    Dim f = m Mod 1
                    Dim q = m - f
                    Dim a = stdNum.Abs(f)
                    Dim dx = If((a <= 0.5), xiu - q * g, If((xiu > 0), xiu - (q + 1) * g, xiu - (q - 1) * g))
                    If -r < dx AndAlso dx <= r Then
                        If Me.scaleSnapByMaxH Then
                            Me.g(i)(u) += maxH * k * dx
                            Me.H(i)(u)(u) += maxH * k
                        Else
                            Me.g(i)(u) += k * dx
                            Me.H(i)(u)(u) += k
                        End If
                    End If
                Next
            Next
            If Not Me.locks.isEmpty() Then
                Me.locks.apply(Sub(u, p)
                                   For i = 0 To Me.k - 1
                                       Me.H(i)(u)(u) += maxH
                                       Me.g(i)(u) -= maxH * (p(i) - x(i)(u))
                                   Next
                               End Sub)
            End If
        End Sub

        Private Shared Function dotProd(a As Double(), b As Double()) As Double
            Dim x = 0
            Dim i = a.Length
            While System.Math.Max(Interlocked.Decrement(i), i + 1)
                x += a(i) * b(i)
            End While
            Return x
        End Function

        ' result r = matrix m * vector v
        Private Shared Sub rightMultiply(m As Double()(), v As Double(), r As Double())
            Dim i = m.Length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                r(i) = Descent.dotProd(m(i), v)
            End While
        End Sub

        ' computes the optimal step size to take in direction d using the
        ' derivative information in this.g and this.H
        ' returns the scalar multiplier to apply to d to get the optimal step
        Public Function computeStepSize(d As Double()()) As Double
            Dim numerator = 0.0
            Dim denominator = 0.0
            For i As Integer = 0 To Me.k - 1
                numerator += Descent.dotProd(Me.g(i), d(i))
                Descent.rightMultiply(Me.H(i), d(i), Me.Hd(i))
                denominator += Descent.dotProd(d(i), Me.Hd(i))
            Next
            If denominator = 0 OrElse Not denominator.IsNaNImaginary Then
                Return 0
            End If
            Return 1 * numerator / denominator
        End Function

        Public Function reduceStress() As Double
            Me.computeDerivatives(Me.x)
            Dim alpha = Me.computeStepSize(Me.g)
            For i = 0 To Me.k - 1
                Me.takeDescentStep(Me.x(i), Me.g(i), alpha)
            Next
            Return Me.computeStress()
        End Function

        Private Shared Sub copy(a As Double()(), b As Double()())
            Dim m = a.Length
            Dim n = b(0).Length
            For i = 0 To m - 1
                For j = 0 To n - 1
                    b(i)(j) = a(i)(j)
                Next
            Next
        End Sub

        ' takes a step of stepSize * d from x0, and then project against any constraints.
        ' result is returned in r.
        ' x0: starting positions
        ' r: result positions will be returned here
        ' d: unconstrained descent vector
        ' stepSize: amount to step along d
        Private Sub stepAndProject(x0 As Double()(), r As Double()(), d As Double()(), stepSize As Double)
            Descent.copy(x0, r)
            Me.takeDescentStep(r(0), d(0), stepSize)
            If Me.project IsNot Nothing Then
                Me.project(0)(x0(0), x0(1), r(0))
            End If
            Me.takeDescentStep(r(1), d(1), stepSize)
            If Me.project IsNot Nothing Then
                Me.project(1)(r(0), x0(1), r(1))
            End If

            ' todo: allow projection against constraints in higher dimensions
            For i = 2 To Me.k - 1
                Me.takeDescentStep(r(i), d(i), stepSize)
            Next

            ' the following makes locks extra sticky... but hides the result of the projection from the consumer
            'if (!this.locks.isEmpty()) {
            '    this.locks.apply((u, p) => {
            '        for (var i = 0; i < this.k; i++) {
            '            r[i][u] = p[i];
            '        }
            '    });
            '}
        End Sub

        Private Shared Sub mApply(m As Integer, n As Integer, f As Action(Of Integer, Integer))
            Dim i = m
            While System.Math.Max(Interlocked.Decrement(i), i + 1) > 0
                Dim j = n
                While System.Math.Max(Interlocked.Decrement(j), j + 1) > 0
                    f(i, j)
                End While
            End While
        End Sub
        Private Sub matrixApply(f As Action(Of Integer, Integer))
            Descent.mApply(Me.k, Me.n, f)
        End Sub

        Private Sub computeNextPosition(x0 As Double()(), r As Double()())
            Me.computeDerivatives(x0)
            Dim alpha = Me.computeStepSize(Me.g)
            Me.stepAndProject(x0, r, Me.g, alpha)
            ' DEBUG
            '                    for (var u: number = 0; u < this.n; ++u)
            '                        for (var i = 0; i < this.k; ++i)
            '                            if (isNaN(r[i][u])) debugger;
            '        DEBUG 

            If Me.project IsNot Nothing Then
                Me.matrixApply(Sub(i, j)
                                   Me.e(i)(j) = x0(i)(j) - r(i)(j)
                               End Sub)
                Dim beta = Me.computeStepSize(Me.e)
                beta = stdNum.Max(0.2, System.Math.Min(beta, 1))
                Me.stepAndProject(x0, r, Me.e, beta)
            End If
        End Sub

        Public Function run(iterations As Integer) As Double
            Dim stress = Double.MaxValue
            Dim converged = False
            While Not converged AndAlso System.Math.Max(Interlocked.Decrement(iterations), iterations + 1) > 0
                Dim s = Me.rungeKutta()
                converged = stdNum.Abs(stress / s - 1) < Me.threshold
                stress = s
            End While
            Return stress
        End Function

        Public Function rungeKutta() As Double
            Me.computeNextPosition(Me.x, Me.a)
            Descent.mid(Me.x, Me.a, Me.ia)
            Me.computeNextPosition(Me.ia, Me.b)
            Descent.mid(Me.x, Me.b, Me.ib)
            Me.computeNextPosition(Me.ib, Me.c)
            Me.computeNextPosition(Me.c, Me.d)
            Dim disp = 0.0
            Me.matrixApply(Sub(i, j)
                               Dim x = (Me.a(i)(j) + 2.0 * Me.b(i)(j) + 2.0 * Me.c(i)(j) + Me.d(i)(j)) / 6.0
                               Dim d = Me.x(i)(j) - x
                               disp += d * d
                               Me.x(i)(j) = x
                           End Sub)
            Return disp
        End Function

        Private Shared Sub mid(a As Double()(), b As Double()(), m As Double()())
            Descent.mApply(a.Length, a(0).Length, Sub(i, j)
                                                      m(i)(j) = a(i)(j) + (b(i)(j) - a(i)(j)) / 2.0
                                                  End Sub)
        End Sub

        Public Sub takeDescentStep(x As Double(), d As Double(), stepSize As Double)
            For i = 0 To Me.n - 1
                x(i) = x(i) - stepSize * d(i)
            Next
        End Sub

        Public Function computeStress() As Double
            Dim stress = 0.0
            Dim u As Integer = 0, nMinus1 As Integer = Me.n - 1
            While u < nMinus1
                Dim v As Integer = u + 1, n As Integer = Me.n
                While v < n
                    Dim l = 0.0
                    For i = 0 To Me.k - 1
                        Dim dx = Me.x(i)(u) - Me.x(i)(v)
                        l += dx * dx
                    Next
                    l = stdNum.Sqrt(l)
                    Dim d = Me.Dmatrix(u)(v)
                    If Not d.IsNaNImaginary Then
                        Continue While
                    End If
                    Dim rl = d - l
                    Dim d2 = d * d
                    stress += rl * rl / d2
                    v += 1
                End While
                u += 1
            End While
            Return stress
        End Function

    End Class
End Namespace

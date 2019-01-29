Imports number = System.Double
Imports System.Collections.Generic

Namespace Layouts.Cola

    '*
    ' * Descent respects a collection of locks over nodes that should not move
    ' * @class Locks
    ' 

    Class Locks
        Public locks As Dictionary(Of number, number())
        '*
        '     * add a lock on the node at index id
        '     * @method add
        '     * @param id index of node to be locked
        '     * @param x required position for node
        '     

        Private Sub add(id As number, x As number())
            ' DEBUG
            '                    if (isNaN(x[0]) || isNaN(x[1])) debugger;
            '        DEBUG 

            Me.locks(id) = x
        End Sub
        '*
        '     * @method clear clear all locks
        '     

        Private Sub clear()
            Me.locks = New Dictionary(Of number, number())()
        End Sub
        '*
        '     * @isEmpty
        '     * @returns false if no locks exist
        '     

        Private Function isEmpty() As Boolean
            For Each l As var In Me.locks.Keys
                Return False
            Next
            Return True
        End Function
        '*
        '     * perform an operation on each lock
        '     * @apply
        '     

        Private Sub apply(f As Action(Of number, number()))
            For Each l As var In Me.locks.Keys
                f(l, Me.locks(l))
            Next
        End Sub
    End Class

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

    Class Descent
        Public threshold As number = 0.0001
        '* Hessian Matrix
        '     * @property H {number[][][]}
        '     

        Public H As number()()()
        '* gradient vector
        '     * @property G {number[][]}
        '     

        Public g As number()()
        '* positions vector
        '     * @property x {number[][]}
        '     

        Public x As number()()
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

        Private Shared zeroDistance As number = 0.0000000001
        Private minD As number

        ' pool of arrays of size n used internally, allocated in constructor
        Private Hd As number()()
        Private a As number()()
        Private b As number()()
        Private c As number()()
        Private d As number()()
        Private e As number()()
        Private ia As number()()
        Private ib As number()()
        Private xtmp As number()()


        ' Parameters for grid snap stress.
        ' TODO: Make a pluggable "StressTerm" class instead of this
        ' mess.
        Public numGridSnapNodes As number = 0
        Public snapGridSize As number = 100
        Public snapStrength As number = 1000
        Public scaleSnapByMaxH As Boolean = False

        Private random As Random = New PseudoRandom()

        Public project As Func(Of number(), number(), number(), number)() = Nothing


        Public D As number()()
        Public G As number()()

        '*
        '     * @method constructor
        '     * @param x {number[][]} initial coordinates for nodes
        '     * @param D {number[][]} matrix of desired distances between pairs of nodes
        '     * @param G {number[][]} [default=null] if specified, G is a matrix of weights for goal terms between pairs of nodes.
        '     * If G[i][j] > 1 and the separation between nodes i and j is greater than their ideal distance, then there is no contribution for this pair to the goal
        '     * If G[i][j] <= 1 then it is used as a weighting on the contribution of the variance between ideal and actual separation between i and j to the goal function
        '     

        Public Sub New(x As number()(), D__1 As number()(), Optional G As number()() = Nothing)
            Me.x = x
            Me.k = x.length
            ' dimensionality
            Dim n = InlineAssignHelper(Me.n, x(0).length)
            ' number of nodes
            Me.H = New Array(Me.k)
            Me.g = New Array(Me.k)
            Me.Hd = New Array(Me.k)
            Me.a = New Array(Me.k)
            Me.b = New Array(Me.k)
            Me.c = New Array(Me.k)
            Me.d = New Array(Me.k)
            Me.e = New Array(Me.k)
            Me.ia = New Array(Me.k)
            Me.ib = New Array(Me.k)
            Me.xtmp = New Array(Me.k)
            Me.locks = New Locks()
            Me.minD = number.MaxValue
            Dim i As Integer = n
            Dim j As Integer
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                j = n
                While System.Threading.Interlocked.Decrement(j) > i
                    Dim d__2 = D__1(i)(j)
                    If d__2 > 0 AndAlso d__2 < Me.minD Then
                        Me.minD = d__2
                    End If
                End While
            End While
            If Me.minD = number.MaxValue Then
                Me.minD = 1
            End If
            i = Me.k
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Me.g(i) = New Array(n)
                Me.H(i) = New Array(n)
                j = n
                While System.Math.Max(System.Threading.Interlocked.Decrement(j), j + 1)
                    Me.H(i)(j) = New Array(n)
                End While
                Me.Hd(i) = New Array(n)
                Me.a(i) = New Array(n)
                Me.b(i) = New Array(n)
                Me.c(i) = New Array(n)
                Me.d(i) = New Array(n)
                Me.e(i) = New Array(n)
                Me.ia(i) = New Array(n)
                Me.ib(i) = New Array(n)
                Me.xtmp(i) = New Array(n)
            End While
        End Sub

        Public Shared Function createSquareMatrix(n As number, f As Func(Of number, number, number)) As number()()
            Dim M = New Array(n)
            For i As var = 0 To n - 1
                M(i) = New Array(n)
                For j As var = 0 To n - 1
                    M(i)(j) = f(i, j)
                Next
            Next
            Return M
        End Function

        Private Function offsetDir() As number()
            Dim u = New Array(Me.k)
            Dim l = 0
            For i As var = 0 To Me.k - 1
                Dim x = InlineAssignHelper(u(i), Me.random.getNextBetween(0.01, 1) - 0.5)
                l += x * x
            Next
            l = Math.sqrt(l)
            Return u.map(Function(x) x *= Me.minD / l)
        End Function

        ' compute first and second derivative information storing results in this.g and this.H
        Public Sub computeDerivatives(x As number()())
            Dim n As number = Me.n
            If n < 1 Then
                Return
            End If
            Dim i As Integer
            ' DEBUG
            '                    for (var u: number = 0; u < n; ++u)
            '                        for (i = 0; i < this.k; ++i)
            '                            if (isNaN(x[i][u])) debugger;
            '        DEBUG 

            Dim d__1 As number() = New Array(Me.k)
            Dim d2__2 As number() = New Array(Me.k)
            Dim Huu As number() = New Array(Me.k)
            Dim maxH As number = 0

            For u As var = 0 To n - 1
                For i = 0 To Me.k - 1
                    Huu(i) = InlineAssignHelper(Me.g(i)(u), 0)
                Next
                For v As var = 0 To n - 1
                    If u = v Then
                        Continue For
                    End If

                    ' The following loop randomly displaces nodes that are at identical positions
                    Dim maxDisplaces = n
                    ' avoid infinite loop in the case of numerical issues, such as huge values
                    Dim sd2 = 0.0
                    While System.Math.Max(System.Threading.Interlocked.Decrement(maxDisplaces), maxDisplaces + 1)
                        sd2 = 0.0
                        For i = 0 To Me.k - 1
                            Dim dx = InlineAssignHelper(d__1(i), x(i)(u) - x(i)(v))
                            sd2 += InlineAssignHelper(d2__2(i), dx * dx)
                        Next
                        If sd2 > 0.000000001 Then
                            Exit While
                        End If
                        Dim rd = Me.offsetDir()
                        For i = 0 To Me.k - 1
                            x(i)(v) += rd(i)
                        Next
                    End While
                    Dim l As number = Math.sqrt(sd2)
                    Dim D__3 As number = Me.D(u)(v)
                    Dim weight = If(Me.G IsNot Nothing, Me.G(u)(v), 1)
                    If weight > 1 AndAlso l > D__3 OrElse Not isFinite(D__3) Then
                        For i = 0 To Me.k - 1
                            Me.H(i)(u)(v) = 0
                        Next
                        Continue For
                    End If
                    If weight > 1 Then
                        weight = 1
                    End If
                    Dim D2__4 As number = D__3 * D__3
                    Dim gs As number = 2 * weight * (l - D__3) / (D2__4 * l)
                    Dim l3 = l * l * l
                    Dim hs As number = 2 * -weight / (D2__4 * l3)
                    If Not isFinite(gs) Then
                        console.log(gs)
                    End If
                    For i = 0 To Me.k - 1
                        Me.g(i)(u) += d__1(i) * gs
                        Huu(i) -= InlineAssignHelper(Me.H(i)(u)(v), hs * (l3 + D__3 * (d2__2(i) - sd2) + l * sd2))
                    Next
                Next
                For i = 0 To Me.k - 1
                    maxH = Math.Max(maxH, InlineAssignHelper(Me.H(i)(u)(u), Huu(i)))
                Next
            Next
            ' Grid snap forces
            Dim r = Me.snapGridSize / 2
            Dim g = Me.snapGridSize
            Dim w = Me.snapStrength
            Dim k = w / (r * r)
            Dim numNodes = Me.numGridSnapNodes
            'var numNodes = n;
            For u As var = 0 To numNodes - 1
                For i = 0 To Me.k - 1
                    Dim xiu = Me.x(i)(u)
                    Dim m = xiu / g
                    Dim f = m Mod 1
                    Dim q = m - f
                    Dim a = Math.Abs(f)
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
                Me.locks.apply(Function(u, p)
                                   For i = 0 To Me.k - 1
                                       Me.H(i)(u)(u) += maxH
                                       Me.g(i)(u) -= maxH * (p(i) - x(i)(u))
                                   Next

                               End Function)
            End If
            ' DEBUG
            '                    for (var u: number = 0; u < n; ++u)
            '                        for (i = 0; i < this.k; ++i) {
            '                            if (isNaN(this.g[i][u])) debugger;
            '                            for (var v: number = 0; v < n; ++v)
            '                                if (isNaN(this.H[i][u][v])) debugger;
            '                        }
            '        DEBUG 

        End Sub

        Private Shared Function dotProd(a As number(), b As number()) As number
            Dim x = 0
            Dim i = a.length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                x += a(i) * b(i)
            End While
            Return x
        End Function

        ' result r = matrix m * vector v
        Private Shared Sub rightMultiply(m As number()(), v As number(), r As number())
            Dim i = m.length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                r(i) = Descent.dotProd(m(i), v)
            End While
        End Sub

        ' computes the optimal step size to take in direction d using the
        ' derivative information in this.g and this.H
        ' returns the scalar multiplier to apply to d to get the optimal step
        Public Function computeStepSize(d As number()()) As number
            Dim numerator = 0.0
            Dim denominator = 0.0
            For i As var = 0 To Me.k - 1
                numerator += Descent.dotProd(Me.g(i), d(i))
                Descent.rightMultiply(Me.H(i), d(i), Me.Hd(i))
                denominator += Descent.dotProd(d(i), Me.Hd(i))
            Next
            If denominator = 0 OrElse Not isFinite(denominator) Then
                Return 0
            End If
            Return 1 * numerator / denominator
        End Function

        Public Function reduceStress() As number
            Me.computeDerivatives(Me.x)
            Dim alpha = Me.computeStepSize(Me.g)
            For i As var = 0 To Me.k - 1
                Me.takeDescentStep(Me.x(i), Me.g(i), alpha)
            Next
            Return Me.computeStress()
        End Function

        Private Shared Sub copy(a As number()(), b As number()())
            Dim m = a.length
            Dim n = b(0).length
            For i As var = 0 To m - 1
                For j As var = 0 To n - 1
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
        Private Sub stepAndProject(x0 As number()(), r As number()(), d As number()(), stepSize As number)
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
            For i As var = 2 To Me.k - 1
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
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1) > 0
                Dim j = n
                While System.Math.Max(System.Threading.Interlocked.Decrement(j), j + 1) > 0
                    f(i, j)
                End While
            End While
        End Sub
        Private Sub matrixApply(f As Action(Of Integer, Integer))
            Descent.mApply(Me.k, Me.n, f)
        End Sub

        Private Sub computeNextPosition(x0 As number()(), r As number()())
            Me.computeDerivatives(x0)
            Dim alpha = Me.computeStepSize(Me.g)
            Me.stepAndProject(x0, r, Me.g, alpha)
            ' DEBUG
            '                    for (var u: number = 0; u < this.n; ++u)
            '                        for (var i = 0; i < this.k; ++i)
            '                            if (isNaN(r[i][u])) debugger;
            '        DEBUG 

            If Me.project IsNot Nothing Then
                Me.matrixApply(Function(i, j) InlineAssignHelper(Me.e(i)(j), x0(i)(j) - r(i)(j)))
                Dim beta = Me.computeStepSize(Me.e)
                beta = Math.max(0.2, Math.min(beta, 1))
                Me.stepAndProject(x0, r, Me.e, beta)
            End If
        End Sub

        Public Function run(iterations As number) As number
            Dim stress = number.MaxValue
            Dim converged = False
            While Not converged AndAlso System.Math.Max(System.Threading.Interlocked.Decrement(iterations), iterations + 1) > 0
                Dim s = Me.rungeKutta()
                converged = Math.abs(stress / s - 1) < Me.threshold
                stress = s
            End While
            Return stress
        End Function

        Public Function rungeKutta() As number
            Me.computeNextPosition(Me.x, Me.a)
            Descent.mid(Me.x, Me.a, Me.ia)
            Me.computeNextPosition(Me.ia, Me.b)
            Descent.mid(Me.x, Me.b, Me.ib)
            Me.computeNextPosition(Me.ib, Me.c)
            Me.computeNextPosition(Me.c, Me.d)
            Dim disp = 0.0
            Me.matrixApply(Function(i, j)
                               Dim x = (Me.a(i)(j) + 2.0 * Me.b(i)(j) + 2.0 * Me.c(i)(j) + Me.d(i)(j)) / 6.0
                               Dim d = Me.x(i)(j) - x
                               disp += d * d
                               Me.x(i)(j) = x

                           End Function)
            Return disp
        End Function

        Private Shared Sub mid(a As number()(), b As number()(), m As number()())
            Descent.mApply(a.length, a(0).length, Function(i, j) InlineAssignHelper(m(i)(j), a(i)(j) + (b(i)(j) - a(i)(j)) / 2.0))
        End Sub

        Public Sub takeDescentStep(x As number(), d As number(), stepSize As number)
            For i As var = 0 To Me.n - 1
                x(i) = x(i) - stepSize * d(i)
            Next
        End Sub

        Public Function computeStress() As number
            Dim stress = 0.0
            Dim u As Integer = 0, nMinus1 As Integer = Me.n - 1
            While u < nMinus1
                Dim v As Integer = u + 1, n As Integer = Me.n
                While v < n
                    Dim l = 0.0
                    For i As var = 0 To Me.k - 1
                        Dim dx = Me.x(i)(u) - Me.x(i)(v)
                        l += dx * dx
                    Next
                    l = Math.sqrt(l)
                    Dim d = Me.D(u)(v)
                    If Not isFinite(d) Then
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
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class

    ' Linear congruential pseudo random number generator
    Class PseudoRandom
        Private a As number = 214013
        Private c As number = 2531011
        Private m As number = 2147483648UI
        Private range As number = 32767
        Public seed As Integer = 1

        Public Sub New(Optional seed As Integer = 1)
            Me.seed = seed
        End Sub

        ' random real between 0 and 1
        Public Function getNext() As number
            Me.seed = CInt(Math.Truncate((Me.seed * Me.a + Me.c) Mod Me.m))
            Return (Me.seed >> 16) / Me.range
        End Function

        ' random real between min and max
        Public Function getNextBetween(min As number, max As number) As number
            Return min + Me.getNext() * (max - min)
        End Function
    End Class
End Namespace
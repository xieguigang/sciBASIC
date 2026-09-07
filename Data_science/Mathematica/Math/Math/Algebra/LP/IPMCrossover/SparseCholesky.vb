' ============================================================================
' SparseCholesky.vb — 稀疏 LDLᵀ 与 PCG 安全网 [readme §2.2 / plan step 1]
' ----------------------------------------------------------------------------
' 1) MinDegreeOrder：静态最小度启发式（按行度升序）。真正的 AMD（近似最小度 + 商图
'    外部度更新）需要 ~400 行且易写错；这里用度数升序作为排序，正确性由
'    SparseNormalFactor 的残差校验兜底（排序只影响 fill-in，不影响解的正确性）。
'
' 2) Factor：up-looking 稀疏 LDLᵀ（A = L·D·Lᵀ，L 单位下三角）
'      L(k,j) = [A(k,j) − Σ_{t<j} L(k,t)·D(t)·L(j,t)] / D(j)
'      D(k)   = A(k,k) − Σ_{t<k} L(k,t)²·D(t)
'    第 k 行的模式 = A 第 k 行模式在"列闭包"下的传递闭包，用二叉小顶堆按升序遍历；
'    L 按列追加（行号天然升序），求解时只需列、无需转置：
'      前代 u:  for j↑: for i∈col j: u(i) −= L(i,j)·u(j)
'      回代 y:  for j↓: y(j) = w(j) − Σ_{i∈col j} L(i,j)·y(i)
'
' 3) Pcg：雅可比预条件共轭梯度。零 fill-in、O(nnz) 内存，是 fill-in 爆炸时的安全网
'    （基因组规模 GEM 的正规方程图接近随机图，L 的 fill-in 可能失控）。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class SparseChol

        ''' <summary>稀疏 LDLᵀ 因子（按列存储 L 的严格下三角部分）</summary>
        Public Class SparseCholFactor

            Friend n As Int32
            Friend Lp As Int32()
            Friend Li As Int32()
            Friend Lx As Double()
            Friend D As Double()
            Friend perm As Int32()

            Friend Sub New()
            End Sub

            Public ReadOnly Property NonZeros As Long
                Get
                    Return If(Li Is Nothing, 0, CLng(Li.Length))
                End Get
            End Property

            Public Function Solve(rhs As Double()) As Double()
                Dim b(n - 1) As Double

                For t As Int32 = 0 To n - 1
                    b(t) = rhs(perm(t))
                Next

                ' ---- 前代：L·u = b（列方向）----
                Dim u(n - 1) As Double

                Array.Copy(b, u, n)

                For j As Int32 = 0 To n - 1
                    Dim uj As Double = u(j)

                    If uj = 0.0 Then Continue For

                    For p As Int32 = Lp(j) To Lp(j + 1) - 1
                        u(Li(p)) -= Lx(p) * uj
                    Next
                Next

                ' ---- w = D⁻¹·u ----
                For i As Int32 = 0 To n - 1
                    u(i) = u(i) / D(i)
                Next

                ' ---- 回代：Lᵀ·y = w（列方向，逆序）----
                Dim y(n - 1) As Double

                For j As Int32 = n - 1 To 0 Step -1
                    Dim s As Double = u(j)

                    For p As Int32 = Lp(j) To Lp(j + 1) - 1
                        s -= Lx(p) * y(Li(p))
                    Next

                    y(j) = s
                Next

                ' ---- 逆置换 ----
                Dim x(n - 1) As Double

                For t As Int32 = 0 To n - 1
                    x(perm(t)) = y(t)
                Next

                Return x
            End Function

        End Class

        ''' <summary>静态最小度排序：行度升序，度相同按下标（perm: 新下标 → 旧下标）</summary>
        Public Shared Function MinDegreeOrder(A As SymCsr) As Int32()
            Dim n As Int32 = A.N
            Dim idx(n - 1) As Int32

            For i As Int32 = 0 To n - 1
                idx(i) = i
            Next

            If n = 0 Then Return idx

            Dim deg As Int32() = A.RowDegrees()

            Array.Sort(Of Int32, Int32)(deg, idx)

            Return idx
        End Function

        ''' <summary>
        ''' up-looking 稀疏 LDLᵀ；非正定或 fill-in 超过 maxNnz 时返回 Nothing（调用方降级 PCG）
        ''' </summary>
        Public Shared Function Factor(A As SymCsr, perm As Int32(), maxNnz As Long) As SparseCholFactor
            Dim n As Int32 = A.N

            If n <= 0 OrElse perm Is Nothing OrElse perm.Length <> n Then Return Nothing

            ' ---- 逆置换 + 重排后的 CSR 行 ----
            Dim inv(n - 1) As Int32

            For t As Int32 = 0 To n - 1
                inv(perm(t)) = t
            Next

            Dim nnzA As Int32 = CInt(A.NonZeros)
            Dim rp(n) As Int32
            Dim ci(nnzA - 1) As Int32
            Dim vx(nnzA - 1) As Double
            Dim p As Int32 = 0

            For r As Int32 = 0 To n - 1
                rp(r) = p

                Dim src As Int32 = perm(r)

                For q As Int32 = A.RowPtr(src) To A.RowPtr(src + 1) - 1
                    ci(p) = inv(A.ColIdx(q))
                    vx(p) = A.Values(q)
                    p += 1
                Next
            Next

            rp(n) = p

            ' ---- 逐行消去 ----
            Dim Lcols(n - 1) As List(Of Int32)
            Dim Lvals(n - 1) As List(Of Double)

            For j As Int32 = 0 To n - 1
                Lcols(j) = New List(Of Int32)()
                Lvals(j) = New List(Of Double)()
            Next

            Dim D(n - 1) As Double
            Dim work(n - 1) As Double
            Dim inHeap(n - 1) As Int32
            Dim touched(n - 1) As Int32
            Dim tlist As New List(Of Int32)()
            Dim heap As New MinHeap()
            Dim totalNnz As Long = 0

            For k As Int32 = 0 To n - 1
                Dim diag As Double = 0

                tlist.Clear()

                For q As Int32 = rp(k) To rp(k + 1) - 1
                    Dim j As Int32 = ci(q)

                    If j = k Then
                        diag = vx(q)
                    ElseIf j < k Then
                        If touched(j) = 0 Then
                            touched(j) = 1
                            work(j) = 0.0
                            tlist.Add(j)
                        End If

                        work(j) += vx(q)

                        If inHeap(j) = 0 Then
                            inHeap(j) = 1
                            heap.Push(j)
                        End If
                    End If
                Next

                Dim dk As Double = diag

                While heap.Count > 0
                    Dim t As Int32 = heap.Pop()

                    inHeap(t) = 0

                    Dim lt As Double = work(t) / D(t)

                    If lt = 0.0 Then Continue While

                    Lcols(t).Add(k)
                    Lvals(t).Add(lt)
                    totalNnz += 1

                    If totalNnz > maxNnz Then Return Nothing

                    dk -= lt * lt * D(t)

                    Dim ct As List(Of Int32) = Lcols(t)
                    Dim vt As List(Of Double) = Lvals(t)
                    Dim ltdt As Double = lt * D(t)

                    For q2 As Int32 = 0 To ct.Count - 1
                        Dim i As Int32 = ct(q2)

                        ' 列内行号升序，遇到 i ≥ k 之后都不属于第 k 行
                        If i >= k Then Exit For

                        If touched(i) = 0 Then
                            touched(i) = 1
                            work(i) = 0.0
                            tlist.Add(i)
                        End If

                        work(i) -= ltdt * vt(q2)

                        If inHeap(i) = 0 Then
                            inHeap(i) = 1
                            heap.Push(i)
                        End If
                    Next
                End While

                If dk <= 0.0 OrElse Double.IsNaN(dk) OrElse Double.IsInfinity(dk) Then Return Nothing

                D(k) = dk

                For Each j As Int32 In tlist
                    work(j) = 0.0
                    touched(j) = 0
                    inHeap(j) = 0
                Next
            Next

            ' ---- 落地为 CSC ----
            Dim total As Int32 = CInt(totalNnz)
            Dim f As New SparseCholFactor()

            f.n = n
            f.D = D
            f.perm = perm
            f.Lp = New Int32(n) {}
            f.Li = New Int32(std.Max(total, 1) - 1) {}
            f.Lx = New Double(std.Max(total, 1) - 1) {}

            p = 0

            For j As Int32 = 0 To n - 1
                f.Lp(j) = p

                Dim ct As List(Of Int32) = Lcols(j)
                Dim vt As List(Of Double) = Lvals(j)

                For q As Int32 = 0 To ct.Count - 1
                    f.Li(p) = ct(q)
                    f.Lx(p) = vt(q)
                    p += 1
                Next
            Next

            f.Lp(n) = p

            Return f
        End Function

        ''' <summary>小顶堆（按列索引升序遍历第 k 行的模式）</summary>
        Private Class MinHeap

            Private data As Int32() = New Int32(63) {}
            Private cnt As Int32 = 0

            Public ReadOnly Property Count As Int32
                Get
                    Return cnt
                End Get
            End Property

            Public Sub Push(v As Int32)
                If cnt = data.Length Then
                    Dim nd(data.Length * 2 - 1) As Int32
                    Array.Copy(data, nd, cnt)
                    data = nd
                End If

                Dim i As Int32 = cnt

                cnt += 1

                While i > 0
                    Dim par As Int32 = (i - 1) \ 2

                    If data(par) <= v Then Exit While

                    data(i) = data(par)
                    i = par
                End While

                data(i) = v
            End Sub

            Public Function Pop() As Int32
                Dim top As Int32 = data(0)

                cnt -= 1

                If cnt > 0 Then
                    Dim last As Int32 = data(cnt)
                    Dim i As Int32 = 0

                    While True
                        Dim l As Int32 = 2 * i + 1

                        If l >= cnt Then Exit While

                        Dim r As Int32 = l + 1
                        Dim c As Int32 = l

                        If r < cnt AndAlso data(r) < data(l) Then c = r
                        If data(c) >= last Then Exit While

                        data(i) = data(c)
                        i = c
                    End While

                    data(i) = last
                End If

                Return top
            End Function

        End Class

    End Class

    ''' <summary>雅可比预条件共轭梯度：正规方程 fill-in 失控时的安全网（零 fill-in）</summary>
    Public Class Pcg

        Public Class PcgSolver

            Private ReadOnly A As SymCsr
            Private ReadOnly invDiag As Double()
            Private ReadOnly x As Double()
            Private ReadOnly r As Double()
            Private ReadOnly z As Double()
            Private ReadOnly p As Double()
            Private ReadOnly Ap As Double()

            Public Property MaxIter As Int32 = 3000
            Public Property Tol As Double = 0.0000000001
            Public Property LastIters As Int32 = 0
            Public Property Converged As Boolean = False

            Public Sub New(A As SymCsr)
                Me.A = A

                Dim n As Int32 = A.N
                Dim d As Double() = A.Diagonal()

                Me.invDiag = New Double(std.Max(n, 1) - 1) {}
                Me.x = New Double(std.Max(n, 1) - 1) {}
                Me.r = New Double(std.Max(n, 1) - 1) {}
                Me.z = New Double(std.Max(n, 1) - 1) {}
                Me.p = New Double(std.Max(n, 1) - 1) {}
                Me.Ap = New Double(std.Max(n, 1) - 1) {}

                For i As Int32 = 0 To n - 1
                    Me.invDiag(i) = If(d(i) > 0.0 AndAlso Not Double.IsInfinity(d(i)), 1.0 / d(i), 1.0)
                Next
            End Sub

            Public Function Solve(rhs As Double()) As Double()
                Dim n As Int32 = A.N

                If n <= 0 Then Return New Double() {}

                Array.Clear(x, 0, n)
                Array.Copy(rhs, r, n)

                Dim bNorm As Double = LinAlg.Norm2(rhs)

                If bNorm < 0.0000000000000001 Then
                    LastIters = 0
                    Converged = True
                    Return x
                End If

                Dim tolAbs As Double = Tol * bNorm

                For i As Int32 = 0 To n - 1
                    z(i) = r(i) * invDiag(i)
                    p(i) = z(i)
                Next

                Dim rz As Double = LinAlg.Dot(r, z)
                Dim iters As Int32 = 0

                Converged = False

                If std.Abs(rz) < 0.0000000000000001 Then
                    LastIters = 0
                    Converged = True
                    Return x
                End If

                For it As Int32 = 1 To MaxIter
                    iters = it

                    A.MvInto(p, Ap)

                    Dim pAp As Double = LinAlg.Dot(p, Ap)

                    If pAp <= 0.0 OrElse Double.IsNaN(pAp) OrElse Double.IsInfinity(pAp) Then
                        ' 非正定：交给上层提升正则化后重试
                        Exit For
                    End If

                    Dim alpha As Double = rz / pAp

                    For i As Int32 = 0 To n - 1
                        x(i) += alpha * p(i)
                        r(i) -= alpha * Ap(i)
                    Next

                    If LinAlg.Norm2(r) <= tolAbs Then
                        Converged = True
                        Exit For
                    End If

                    For i As Int32 = 0 To n - 1
                        z(i) = r(i) * invDiag(i)
                    Next

                    Dim rzNew As Double = LinAlg.Dot(r, z)

                    If std.Abs(rzNew) < 0.0000000000000001 Then
                        Converged = True
                        Exit For
                    End If

                    Dim beta As Double = rzNew / rz

                    For i As Int32 = 0 To n - 1
                        p(i) = z(i) + beta * p(i)
                    Next

                    rz = rzNew
                Next

                LastIters = iters

                Return x
            End Function

        End Class

        ''' <summary>残差校验：‖M·z − rhs‖ / (1 + ‖rhs‖) ≤ tol</summary>
        Public Shared Function ResidualOk(M As SymCsr, z As Double(), rhs As Double(), tol As Double) As Boolean
            Dim n As Int32 = M.N
            Dim w(n - 1) As Double

            M.MvInto(z, w)

            Dim err As Double = 0.0
            Dim nr As Double = 0.0

            For i As Int32 = 0 To n - 1
                Dim d As Double = w(i) - rhs(i)

                err += d * d
                nr += rhs(i) * rhs(i)
            Next

            Return std.Sqrt(err) <= tol * (1.0 + std.Sqrt(nr))
        End Function

    End Class

End Namespace

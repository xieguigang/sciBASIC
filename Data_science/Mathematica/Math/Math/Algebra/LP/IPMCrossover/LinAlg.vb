' ============================================================================
' LinAlg.vb — 稠密线性代数核心（纯 BCL，行主序 Double(,)）
' ----------------------------------------------------------------------------
' [readme §2.2 数值手段] 静态正则化（M + reg·I）+ 迭代精化（一次）：
'   SolveSpd = Cholesky(LLᵀ) 分解 → 两段回代 → 残差校正一次。
' LuFactorization：部分主元 LU（P·A = L·U，LU 原地合并 + 行置换记录），
'   支持 LuSolve（A x = b）与 LuSolveT（Aᵀ x = b，对偶/影子价计算用）——
'   转置求解的三角回代方向：Uᵀ 下三角用 LU[:i,i]·z[:i]；Lᵀ 上三角用
'   LU[i+1:,i]·v[i+1:]（列方向取元素），这是容易写反的关键点（镜像已对拍）。
' EchelonRank：阶梯消元秩（支持非方阵），crossover 构造基的线性无关检测用。
' ============================================================================

Imports System
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>LU 分解结果：P·A = L·U；LU 原地合并（|L| 对角=1），Piv(k)=位置k处的原始行号</summary>
    Public Class LuFactorization

        Public LU As Double(,)
        Public Piv As Int32()
        Public n As Int32

        Public Sub New(lu As Double(,), piv As Int32())
            Me.LU = lu
            Me.Piv = piv
            Me.n = piv.Length
        End Sub

    End Class

    Public Module LinAlg

        ''' <summary>Cholesky 分解 A = L·Lᵀ（A 须对称正定）；失败返回 Nothing</summary>
        Public Function Cholesky(A As Double(,)) As Double(,)
            Dim n As Int32 = A.GetLength(0)
            Dim L(n - 1, n - 1) As Double
            For j = 0 To n - 1
                Dim d As Double = A(j, j)
                For k = 0 To j - 1
                    d -= L(j, k) * L(j, k)
                Next
                If d <= 0 Then Return Nothing
                L(j, j) = std.Sqrt(d)
                For i = j + 1 To n - 1
                    Dim s As Double = A(i, j)
                    For k = 0 To j - 1
                        s -= L(i, k) * L(j, k)
                    Next
                    L(i, j) = s / L(j, j)
                Next
            Next
            Return L
        End Function

        ''' <summary>Cholesky 两段回代：解 L·Lᵀ·x = rhs</summary>
        Public Function CholSolve(L As Double(,), rhs As Double()) As Double()
            Dim n As Int32 = L.GetLength(0)
            Dim y(n - 1) As Double
            For i = 0 To n - 1
                Dim s As Double = rhs(i)
                For k = 0 To i - 1
                    s -= L(i, k) * y(k)
                Next
                y(i) = s / L(i, i)
            Next
            Dim x(n - 1) As Double
            For i = n - 1 To 0 Step -1
                Dim s As Double = y(i)
                For k = i + 1 To n - 1
                    s -= L(k, i) * x(k)
                Next
                x(i) = s / L(i, i)
            Next
            Return x
        End Function

        ''' <summary>带对角正则化的 Cholesky 分解（A + reg·I = L·Lᵀ）；失败返回 Nothing</summary>
        Public Function Cholesky(A As Double(,), reg As Double) As Double(,)
            Dim n As Int32 = A.GetLength(0)
            Dim Ar(n - 1, n - 1) As Double
            Array.Copy(A, Ar, A.Length)
            For i = 0 To n - 1
                Ar(i, i) += reg
            Next
            Return Cholesky(Ar)
        End Function

        ''' <summary>(M + reg·I) Cholesky 求解 + 一次迭代精化 [readme §2.2]</summary>
        Public Function SolveSpd(M As Double(,), rhs As Double(), reg As Double) As Double()
            Dim n As Int32 = M.GetLength(0)
            Dim Mreg(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    Mreg(i, j) = M(i, j)
                Next
                Mreg(i, i) += reg
            Next
            Dim L = Cholesky(Mreg)
            If L Is Nothing Then Return Nothing
            Dim z = CholSolve(L, rhs)
            ' 迭代精化：res = rhs − Mreg·z；z += L⁻¹res
            Dim res(n - 1) As Double
            For i = 0 To n - 1
                Dim s As Double = rhs(i)
                For j = 0 To n - 1
                    s -= Mreg(i, j) * z(j)
                Next
                res(i) = s
            Next
            Dim dz = CholSolve(L, res)
            For i = 0 To n - 1
                z(i) += dz(i)
            Next
            Return z
        End Function

        ''' <summary>部分主元 LU 分解；矩阵奇异（主元 &lt; 1e-13）返回 Nothing</summary>
        Public Function LuFactor(A As Double(,)) As LuFactorization
            Dim n As Int32 = A.GetLength(0)
            If A.GetLength(1) <> n Then Throw New ArgumentException("LuFactor 需要方阵")
            Dim LU(n - 1, n - 1) As Double
            Array.Copy(A, LU, A.Length)
            Dim piv(n - 1) As Int32
            For i = 0 To n - 1
                piv(i) = i
            Next
            For k = 0 To n - 1
                ' 选主元
                Dim p As Int32 = k
                Dim best As Double = std.Abs(LU(k, k))
                For i = k + 1 To n - 1
                    If std.Abs(LU(i, k)) > best Then
                        best = std.Abs(LU(i, k))
                        p = i
                    End If
                Next
                If best < 0.0000000000001 Then Return Nothing
                If p <> k Then
                    For j = 0 To n - 1
                        Dim t = LU(k, j) : LU(k, j) = LU(p, j) : LU(p, j) = t
                    Next
                    Dim ti = piv(k) : piv(k) = piv(p) : piv(p) = ti
                End If
                ' 消元
                For i = k + 1 To n - 1
                    LU(i, k) /= LU(k, k)
                    Dim lik = LU(i, k)
                    For j = k + 1 To n - 1
                        LU(i, j) -= lik * LU(k, j)
                    Next
                Next
            Next
            Return New LuFactorization(LU, piv)
        End Function

        ''' <summary>解 A·x = rhs</summary>
        Public Function LuSolve(fac As LuFactorization, rhs As Double()) As Double()
            Dim n = fac.n
            Dim b(n - 1) As Double
            For k = 0 To n - 1
                b(k) = rhs(fac.Piv(k))
            Next
            ' 前代（L）
            For i = 1 To n - 1
                Dim s As Double = b(i)
                For k = 0 To i - 1
                    s -= fac.LU(i, k) * b(k)
                Next
                b(i) = s
            Next
            ' 回代（U）
            For i = n - 1 To 0 Step -1
                Dim s As Double = b(i)
                For k = i + 1 To n - 1
                    s -= fac.LU(i, k) * b(k)
                Next
                b(i) = s / fac.LU(i, i)
            Next
            Return b
        End Function

        ''' <summary>解 Aᵀ·x = rhs（影子价 y = B⁻ᵀc_B 等）[镜像对拍覆盖]</summary>
        Public Function LuSolveT(fac As LuFactorization, rhs As Double()) As Double()
            Dim n = fac.n
            ' Uᵀ·z = rhs（Uᵀ 下三角：Uᵀ(i,j) = U(j,i) = LU(j,i)，j ≤ i）
            Dim z(n - 1) As Double
            For i = 0 To n - 1
                Dim s As Double = rhs(i)
                For k = 0 To i - 1
                    s -= fac.LU(k, i) * z(k)
                Next
                z(i) = s / fac.LU(i, i)
            Next
            ' Lᵀ·v = z（Lᵀ 上三角：Lᵀ(i,j) = L(j,i) = LU(j,i)，j > i；单位对角）
            Dim v(n - 1) As Double
            For i = n - 1 To 0 Step -1
                Dim s As Double = z(i)
                For k = i + 1 To n - 1
                    s -= fac.LU(k, i) * v(k)
                Next
                v(i) = s
            Next
            ' x = Pᵀ·v：x(Piv(k)) = v(k)
            Dim x(n - 1) As Double
            For k = 0 To n - 1
                x(fac.Piv(k)) = v(k)
            Next
            Return x
        End Function

        ''' <summary>阶梯消元秩（支持非方阵）——crossover 基构造的线性无关检测</summary>
        Public Function EchelonRank(A As Double(,), Optional tol As Double = 0.0000000001) As Int32
            Dim mi As Int32 = A.GetLength(0)
            Dim n As Int32 = A.GetLength(1)
            Dim M(mi - 1, n - 1) As Double
            Array.Copy(A, M, A.Length)
            Dim scale As Double = 1.0
            For i = 0 To mi - 1
                For j = 0 To n - 1
                    scale = std.Max(scale, std.Abs(M(i, j)))
                Next
            Next
            Dim rank As Int32 = 0
            Dim row As Int32 = 0
            For col = 0 To n - 1
                If row >= mi Then Exit For
                ' 选列主元
                Dim p As Int32 = row
                Dim best As Double = std.Abs(M(row, col))
                For i = row + 1 To mi - 1
                    If std.Abs(M(i, col)) > best Then
                        best = std.Abs(M(i, col))
                        p = i
                    End If
                Next
                If best <= tol * scale Then Continue For
                If p <> row Then
                    For j = 0 To n - 1
                        Dim t = M(row, j) : M(row, j) = M(p, j) : M(p, j) = t
                    Next
                End If
                For i = row + 1 To mi - 1
                    Dim f = M(i, col) / M(row, col)
                    For j = col To n - 1
                        M(i, j) -= f * M(row, j)
                    Next
                Next
                row += 1
                rank += 1
            Next
            Return rank
        End Function

        ''' <summary>矩阵取列子集（新矩阵 m×cols.Count）</summary>
        Public Function TakeCols(A As Double(,), cols As List(Of Int32)) As Double(,)
            Dim m As Int32 = A.GetLength(0)
            Dim B(m - 1, cols.Count - 1) As Double
            For i = 0 To m - 1
                For j = 0 To cols.Count - 1
                    B(i, j) = A(i, cols(j))
                Next
            Next
            Return B
        End Function

        ''' <summary>追加一列（返回新矩阵）</summary>
        Public Function AppendCol(A As Double(,), col As Double()) As Double(,)
            Dim m As Int32 = A.GetLength(0)
            Dim k As Int32 = A.GetLength(1)
            Dim B(m - 1, k) As Double
            For i = 0 To m - 1
                For j = 0 To k - 1
                    B(i, j) = A(i, j)
                Next
                B(i, k) = col(i)
            Next
            Return B
        End Function

        Public Function Dot(a As Double(), b As Double()) As Double
            Dim s As Double = 0
            For i = 0 To a.Length - 1
                s += a(i) * b(i)
            Next
            Return s
        End Function

        Public Function Norm2(a As Double()) As Double
            Return std.Sqrt(Dot(a, a))
        End Function

    End Module

End Namespace

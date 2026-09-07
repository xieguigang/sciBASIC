' ============================================================================
' SparseNormal.vb — 正规方程 A·Θ·Aᵀ 的稀疏装配 [readme §2.2 / plan step 1]
' ----------------------------------------------------------------------------
' A·Θ·Aᵀ = Σ_j θ_j · a_j · a_jᵀ（a_j 为 A 的第 j 列）
'   ⇒ 按列扫描：第 j 列的 k 个非零行两两配对，产生 k² 个累加项。
'   复杂度 O(Σ_j k_j²)，与 A 的列非零元分布有关、与 m·n 无关。
'   GEM（71949 非零元 / 16107 列，平均 4.5/列）≈ 3.3e5 次乘加，可忽略。
'
' 两遍法（先数模式、再填值）+ marker 数组去重，产出对称 CSR（含对角、两个三角全存）。
' 模式在迭代过程中不变（θ 恒 > 0），但每遍成本极低，故不做模式缓存。
' ============================================================================

Imports System
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    ''' <summary>对称稀疏矩阵（CSR，两个三角都存，含对角）</summary>
    Public Class SymCsr

        Public ReadOnly N As Int32
        Public ReadOnly RowPtr As Int32()
        Public ReadOnly ColIdx As Int32()
        Public ReadOnly Values As Double()

        Public Sub New(n As Int32, rowPtr As Int32(), colIdx As Int32(), values As Double())
            Me.N = n
            Me.RowPtr = rowPtr
            Me.ColIdx = colIdx
            Me.Values = values
        End Sub

        Public ReadOnly Property NonZeros As Long
            Get
                Return If(ColIdx Is Nothing, 0, CLng(ColIdx.Length))
            End Get
        End Property

        Public Function Mv(x As Double()) As Double()
            Dim y(N - 1) As Double

            MvInto(x, y)

            Return y
        End Function

        ''' <summary>无分配版本的 A·x（PCG 内循环每步都调用，避免 GC 抖动）</summary>
        Public Sub MvInto(x As Double(), y As Double())
            For i As Int32 = 0 To N - 1
                Dim s As Double = 0
                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    s += Values(p) * x(ColIdx(p))
                Next
                y(i) = s
            Next
        End Sub

        ''' <summary>取对角元（缺失视为 0）</summary>
        Public Function Diagonal() As Double()
            Dim d(N - 1) As Double

            For i As Int32 = 0 To N - 1
                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    If ColIdx(p) = i Then
                        d(i) = Values(p)
                        Exit For
                    End If
                Next
            Next

            Return d
        End Function

        ''' <summary>对角加 reg（M + reg·I）；保证每行对角元存在</summary>
        Public Sub AddDiagonal(reg As Double)
            For i As Int32 = 0 To N - 1
                Dim found As Boolean = False

                For p As Int32 = RowPtr(i) To RowPtr(i + 1) - 1
                    If ColIdx(p) = i Then
                        Values(p) += reg
                        found = True
                        Exit For
                    End If
                Next

                If Not found Then
                    Throw New InvalidOperationException("SymCsr: 第 " & i & " 行缺少对角元")
                End If
            Next
        End Sub

        ''' <summary>深拷贝（正则化阶梯重试时保留未加 reg 的原始版本）</summary>
        Public Function Clone() As SymCsr
            Dim rp As Int32() = New Int32(RowPtr.Length - 1) {}
            Dim ci As Int32() = New Int32(std.Max(ColIdx.Length, 1) - 1) {}
            Dim vx As Double() = New Double(std.Max(Values.Length, 1) - 1) {}

            Array.Copy(RowPtr, rp, RowPtr.Length)
            If ColIdx.Length > 0 Then Array.Copy(ColIdx, ci, ColIdx.Length)
            If Values.Length > 0 Then Array.Copy(Values, vx, Values.Length)

            Return New SymCsr(N, rp, ci, vx)
        End Function

        ''' <summary>每行非零元个数（排序启发式的度数）</summary>
        Public Function RowDegrees() As Int32()
            Dim deg(N - 1) As Int32

            For i As Int32 = 0 To N - 1
                deg(i) = RowPtr(i + 1) - RowPtr(i)
            Next

            Return deg
        End Function

    End Class

    Public Module SparseNormal

        ''' <summary>
        ''' 稀疏装配 A·Θ·Aᵀ（对称 CSR）
        ''' </summary>
        ''' <param name="cp">A 的 CSC 列指针（长度 n+1）</param>
        ''' <param name="ri">A 的 CSC 行索引</param>
        ''' <param name="rv">A 的 CSC 值</param>
        ''' <param name="m">A 的行数</param>
        ''' <param name="n">A 的列数</param>
        ''' <param name="theta">对角缩放（长度 n）</param>
        Public Function Assemble(cp As Int32(), ri As Int32(), rv As Double(),
                                 m As Int32, n As Int32, theta As Double()) As SymCsr
            If m <= 0 Then
                Return New SymCsr(0, New Int32(m + 1) {}, New Int32() {}, New Double() {})
            End If

            Dim marker(m - 1) As Int32
            Dim mpos(m - 1) As Int32
            Dim cnt(m - 1) As Int32

            For i As Int32 = 0 To m - 1
                marker(i) = -1
            Next

            ' ---- pass 1：数模式 ----
            For j As Int32 = 0 To n - 1
                Dim a0 As Int32 = cp(j)
                Dim a1 As Int32 = cp(j + 1) - 1

                For a As Int32 = a0 To a1
                    Dim i As Int32 = ri(a)

                    For b As Int32 = a0 To a1
                        Dim k As Int32 = ri(b)

                        If marker(k) <> j Then
                            marker(k) = j
                            cnt(i) += 1
                        End If
                    Next
                Next
            Next

            ' 空行补一个对角槽位，保证 M+reg·I 的对角严格正
            For i As Int32 = 0 To m - 1
                If cnt(i) = 0 Then cnt(i) = 1
            Next

            Dim rp(m) As Int32

            For i As Int32 = 0 To m - 1
                rp(i + 1) = rp(i) + cnt(i)
            Next

            Dim nnz As Int32 = rp(m)
            Dim ci(nnz - 1) As Int32
            Dim vx(nnz - 1) As Double
            Dim fill(m - 1) As Int32

            For i As Int32 = 0 To m - 1
                marker(i) = -1
            Next

            ' ---- pass 2：填值 ----
            For j As Int32 = 0 To n - 1
                Dim a0 As Int32 = cp(j)
                Dim a1 As Int32 = cp(j + 1) - 1

                For a As Int32 = a0 To a1
                    Dim i As Int32 = ri(a)
                    Dim scaled As Double = rv(a) * theta(j)

                    For b As Int32 = a0 To a1
                        Dim k As Int32 = ri(b)
                        Dim p As Int32

                        If marker(k) <> j Then
                            marker(k) = j
                            p = rp(i) + fill(i)
                            ci(p) = k
                            vx(p) = 0.0
                            mpos(k) = p
                            fill(i) += 1
                        Else
                            p = mpos(k)
                        End If

                        vx(p) += scaled * rv(b)
                    Next
                Next
            Next

            ' 空行补对角（值为 0，随后 AddDiagonal 会加上 reg）
            For i As Int32 = 0 To m - 1
                If fill(i) < cnt(i) Then
                    Dim p As Int32 = rp(i) + fill(i)

                    ci(p) = i
                    vx(p) = 0.0
                    fill(i) += 1
                End If
            Next

            Return New SymCsr(m, rp, ci, vx)
        End Function

    End Module

End Namespace

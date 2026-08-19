Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module TruncatedSVDTest

    Sub Main()
        Dim rand As New Random(42)
        Dim m = 60, n = 80, r = 5

        ' 构造真正的低秩稀疏矩阵：A = Σ_{i=1..5} u_i·v_iᵀ
        ' 其中 u_i, v_i 均为稀疏随机向量（~10% 非零），秩精确 <= 5
        Dim U0(m - 1)() As Double
        For i = 0 To m - 1
            U0(i) = New Double(r - 1) {}
            For j = 0 To r - 1
                If rand.NextDouble() < 0.25 Then
                    U0(i)(j) = rand.NextDouble() * 2 - 1
                End If
            Next
        Next
        Dim V0(n - 1)() As Double
        For i = 0 To n - 1
            V0(i) = New Double(r - 1) {}
            For j = 0 To r - 1
                If rand.NextDouble() < 0.25 Then
                    V0(i)(j) = rand.NextDouble() * 2 - 1
                End If
            Next
        Next

        ' A = U0 · V0ᵀ (m×n, 秩 <= r, 天然稀疏)
        Dim dense(m - 1)() As Double
        Dim nz As Integer = 0
        For i = 0 To m - 1
            dense(i) = New Double(n - 1) {}
            For j = 0 To n - 1
                Dim acc = 0.0
                For tt = 0 To r - 1
                    acc += U0(i)(tt) * V0(j)(tt)
                Next
                dense(i)(j) = acc
                If acc <> 0 Then nz += 1
            Next
        Next

        ' 参照：稠密 SVD 的真实奇异值
        Dim refSvd As New SingularValueDecomposition(New NumericMatrix(dense))
        Dim trueS = refSvd.SingularValues
        Console.WriteLine($"真实矩阵秩: {refSvd.Rank}")

        ' 转为稀疏矩阵（三元组构造）
        Dim rowList As New List(Of Integer)
        Dim colList As New List(Of Integer)
        Dim valList As New List(Of Double)
        For i = 0 To m - 1
            For j = 0 To n - 1
                If dense(i)(j) <> 0 Then
                    rowList.Add(i)
                    colList.Add(j)
                    valList.Add(dense(i)(j))
                End If
            Next
        Next
        Dim A As New SparseMatrix(rowList.ToArray, colList.ToArray, valList.ToArray, m, n)
        Console.WriteLine($"稀疏矩阵: {A.RowDimension}x{A.ColumnDimension}, nnz={rowList.Count} ({nz * 100.0 / (m * n):F1}% 非零)")

        ' ================= 测试1: k = r = 5（满秩恢复）=================
        Dim svd As New TruncatedSVD(A, 5)
        Dim sigma = svd.SingularValues
        Console.WriteLine()
        Console.WriteLine("=== 测试1: 奇异值精度 (k=5, 秩=5) ===")
        Console.WriteLine("真实值: " & String.Join(", ", trueS.Take(5).Select(Function(sv) sv.ToString("F4"))))
        Console.WriteLine("计算值: " & String.Join(", ", sigma.Select(Function(sv) sv.ToString("F4"))))
        Dim svErr = Enumerable.Range(0, 5).Max(Function(idx) Math.Abs(sigma(idx) - trueS(idx)) / trueS(idx))
        Console.WriteLine($"最大相对误差: {svErr:E3}")

        ' ================= 测试2: 重构误差（应接近 0）=================
        Console.WriteLine()
        Console.WriteLine("=== 测试2: 重构误差 (k=5 应接近 0) ===")
        Dim U = svd.U
        Dim V = svd.V
        Dim reconErr = 0.0, frobA = 0.0
        For i = 0 To m - 1
            For j = 0 To n - 1
                Dim approx = 0.0
                For tt As Integer = 0 To 4
                    approx += U(i)(tt) * sigma(tt) * V(j)(tt)
                Next
                reconErr += (dense(i)(j) - approx) ^ 2
                frobA += dense(i)(j) ^ 2
            Next
        Next
        Console.WriteLine($"相对重构误差: {Math.Sqrt(reconErr / frobA):E3}")

        ' ================= 测试3: V 列正交性 =================
        Console.WriteLine()
        Console.WriteLine("=== 测试3: U/V 列正交性 ===")
        Dim orthErrV = 0.0, orthErrU = 0.0
        For ia = 0 To 4
            For ib = 0 To 4
                Dim dotv = 0.0, dotu = 0.0
                For i = 0 To n - 1
                    dotv += V(i)(ia) * V(i)(ib)
                Next
                For i = 0 To m - 1
                    dotu += U(i)(ia) * U(i)(ib)
                Next
                If ia = ib Then
                    dotv -= 1.0
                    dotu -= 1.0
                End If
                orthErrV += dotv * dotv
                orthErrU += dotu * dotu
            Next
        Next
        Console.WriteLine($"V 正交性偏差: {Math.Sqrt(orthErrV):E3}")
        Console.WriteLine($"U 正交性偏差: {Math.Sqrt(orthErrU):E3}")

        ' ================= 测试4: 降维矩阵 =================
        Console.WriteLine()
        Console.WriteLine("=== 测试4: 降维矩阵 ReducedMatrix (m×k) ===")
        Dim Xred = svd.ReducedMatrix
        Console.WriteLine($"维度: {Xred.Length}x{Xred(0).Length} (期望 {m}x5)")
        Dim redErr = 0.0, redNorm = 0.0
        For i = 0 To m - 1
            For tt As Integer = 0 To 4
                Dim av = 0.0
                For j = 0 To n - 1
                    av += dense(i)(j) * V(j)(tt)
                Next
                redErr += (Xred(i)(tt) - av) ^ 2
                redNorm += av ^ 2
            Next
        Next
        Console.WriteLine($"Reduced vs A·V 相对误差: {Math.Sqrt(redErr / redNorm):E3} (期望~0)")
        Dim comp = svd.Components
        Console.WriteLine($"Components 维度: {comp.Length}x{comp(0).Length} (期望 5x{n})")

        ' ================= 测试5: 更小截断 k=3 =================
        Console.WriteLine()
        Console.WriteLine("=== 测试5: 更小截断 k=3 ===")
        Dim svd3 As New TruncatedSVD(A, 3)
        Console.WriteLine("真实值: " & String.Join(", ", trueS.Take(3).Select(Function(sv) sv.ToString("F4"))))
        Console.WriteLine("计算值: " & String.Join(", ", svd3.SingularValues.Select(Function(sv) sv.ToString("F4"))))
        Dim Xred2 = TruncatedSVD.Reduce(A, 3)
        Console.WriteLine($"Reduce(A,3) 维度: {Xred2.Length}x{Xred2(0).Length} (期望 {m}x3)")

        ' ================= 测试6: 参数校验 =================
        Console.WriteLine()
        Console.WriteLine("=== 测试6: 参数校验 ===")
        Try
            Dim dummy1 = New TruncatedSVD(A, 0)
            Console.WriteLine("k=0 未抛异常: FAIL")
        Catch ex As ArgumentException
            Console.WriteLine("k=0 正确抛出 ArgumentException: OK")
        End Try
        Try
            Dim dummy2 = New TruncatedSVD(A, 1000)
            Console.WriteLine("k>min(m,n) 未抛异常: FAIL")
        Catch ex As ArgumentException
            Console.WriteLine("k>min(m,n) 正确抛出 ArgumentException: OK")
        End Try

        ' ================= 测试7: 大规模稀疏性能 =================
        Console.WriteLine()
        Console.WriteLine("=== 测试7: 大规模稀疏矩阵性能 (2000×5000, nnz≈50000, k=20) ===")
        Dim bigM = 2000, bigN = 5000, nnz = 50000
        Dim br(nnz - 1) As Integer, bc(nnz - 1) As Integer, bv(nnz - 1) As Double
        For idx = 0 To nnz - 1
            br(idx) = rand.Next(bigM)
            bc(idx) = rand.Next(bigN)
            bv(idx) = rand.NextDouble() * 2 - 1
        Next
        Dim bigA As New SparseMatrix(br, bc, bv, bigM, bigN)
        Dim sw = System.Diagnostics.Stopwatch.StartNew()
        Dim bigSvd As New TruncatedSVD(bigA, 20)
        sw.Stop()
        Console.WriteLine($"完成耗时: {sw.ElapsedMilliseconds} ms")
        Console.WriteLine($"奇异值(top5): " & String.Join(", ", bigSvd.SingularValues.Take(5).Select(Function(sv) sv.ToString("F4"))))
        Dim bigX = bigSvd.ReducedMatrix
        Console.WriteLine($"降维结果: {bigX.Length}x{bigX(0).Length} 稠密矩阵")

        Console.WriteLine()
        Console.WriteLine("全部测试完成")
    End Sub
End Module

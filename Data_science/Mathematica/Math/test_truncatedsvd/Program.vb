Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Module Program

    Sub Main()
        Dim rand As New Random(42)
        Dim m = 60, n = 80, r = 5

        ' 1. 构造低秩稠密矩阵 A = G1 · G2ᵀ（秩 <= r）
        Dim G1(m - 1)() As Double, G2(n - 1)() As Double
        For i = 0 To m - 1
            G1(i) = New Double(r - 1) {}
            For j = 0 To r - 1
                G1(i)(j) = rand.NextDouble() * 2 - 1
            Next
        Next
        For i = 0 To n - 1
            G2(i) = New Double(r - 1) {}
            For j = 0 To r - 1
                G2(i)(j) = rand.NextDouble() * 2 - 1
            Next
        Next

        Dim dense(m - 1)() As Double
        For i = 0 To m - 1
            dense(i) = New Double(n - 1) {}
            For j = 0 To n - 1
                Dim s = 0.0
                For t = 0 To r - 1
                    s += G1(i)(t) * G2(j)(t)
                Next
                dense(i)(j) = s
            Next
        Next

        ' 2. 随机置零 60% 制造稀疏性（置零不会增大秩）
        For i = 0 To m - 1
            For j = 0 To n - 1
                If rand.NextDouble() < 0.6 Then dense(i)(j) = 0
            Next
        Next

        ' 3. 参照：稠密 SVD 的真实奇异值
        Dim refSvd As New SingularValueDecomposition(New NumericMatrix(dense))
        Dim trueS = refSvd.SingularValues

        ' 4. 转为稀疏矩阵（三元组构造）
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
        Console.WriteLine($"稀疏矩阵: {A.RowDimension}x{A.ColumnDimension}, nnz={rowList.Count} ({rowList.Count * 100.0 / (m * n):F1}% 非零)")

        ' 5. 截断 SVD：k = r = 5（满秩恢复）
        Dim svd As New TruncatedSVD(A, 5)
        Dim s = svd.SingularValues
        Console.WriteLine()
        Console.WriteLine("=== 测试1: 奇异值精度 (k=5, 秩=5) ===")
        Console.WriteLine("真实值: " & String.Join(", ", trueS.Take(5).Select(Function(x) x.ToString("F4"))))
        Console.WriteLine("计算值: " & String.Join(", ", s.Select(Function(x) x.ToString("F4"))))
        Dim svErr = Enumerable.Range(0, 5).Max(Function(i) Math.Abs(s(i) - trueS(i)) / trueS(i))
        Console.WriteLine($"最大相对误差: {svErr:E3}")

        ' 6. 重构误差 ||A - U·Σ·Vᵀ||_F / ||A||_F
        Console.WriteLine()
        Console.WriteLine("=== 测试2: 重构误差 (k=5 应接近 0) ===")
        Dim U = svd.U
        Dim V = svd.V
        Dim reconErr = 0.0, frobA = 0.0
        For i = 0 To m - 1
            For j = 0 To n - 1
                Dim approx = 0.0
                For t = 0 To 4
                    approx += U(i)(t) * s(t) * V(j)(t)
                Next
                reconErr += (dense(i)(j) - approx) ^ 2
                frobA += dense(i)(j) ^ 2
            Next
        Next
        Console.WriteLine($"相对重构误差: {Math.Sqrt(reconErr / frobA):E3}")

        ' 7. V 列正交性
        Console.WriteLine()
        Console.WriteLine("=== 测试3: V 列正交性 ===")
        Dim orthErr = 0.0
        For a = 0 To 4
            For b = 0 To 4
                Dim dot = 0.0
                For i = 0 To n - 1
                    dot += V(i)(a) * V(i)(b)
                Next
                If a = b Then dot -= 1.0
                orthErr += dot * dot
            Next
        Next
        Console.WriteLine($"正交性偏差: {Math.Sqrt(orthErr):E3}")

        ' 8. ReducedMatrix = A·V 一致性 + 形状
        Console.WriteLine()
        Console.WriteLine("=== 测试4: 降维矩阵 ReducedMatrix (m×k) ===")
        Dim X = svd.ReducedMatrix
        Console.WriteLine($"维度: {X.Length}x{X(0).Length} (期望 {m}x5)")
        Dim redErr = 0.0, redNorm = 0.0
        For i = 0 To m - 1
            For t = 0 To 4
                Dim av = 0.0
                For j = 0 To n - 1
                    av += dense(i)(j) * V(j)(t)
                Next
                redErr += (X(i)(t) - av) ^ 2
                redNorm += av ^ 2
            Next
        Next
        Console.WriteLine($"Reduced vs A·V 相对误差: {Math.Sqrt(redErr / redNorm):E3}")

        ' 9. Components 形状
        Dim comp = svd.Components
        Console.WriteLine($"Components 维度: {comp.Length}x{comp(0).Length} (期望 5x{n})")

        ' 10. k=3 截断：前 3 个奇异值仍应准确
        Console.WriteLine()
        Console.WriteLine("=== 测试5: 更小截断 k=3 ===")
        Dim svd3 As New TruncatedSVD(A, 3)
        Console.WriteLine("真实值: " & String.Join(", ", trueS.Take(3).Select(Function(x) x.ToString("F4"))))
        Console.WriteLine("计算值: " & String.Join(", ", svd3.SingularValues.Select(Function(x) x.ToString("F4"))))
        Dim X2 = TruncatedSVD.Reduce(A, 3)
        Console.WriteLine($"Reduce(A,3) 维度: {X2.Length}x{X2(0).Length} (期望 {m}x3)")

        ' 11. 非对称矩阵场景（G1·G2ᵀ 本身一般非对称，此测试天然覆盖）
        ' 12. 参数校验
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

        ' 13. 大规模稀疏性能冒烟测试
        Console.WriteLine()
        Console.WriteLine("=== 测试7: 大规模稀疏矩阵性能 (2000×5000, nnz≈50000, k=20) ===")
        Dim bigM = 2000, bigN = 5000, nnz = 50000
        Dim br(nnz - 1) As Integer, bc(nnz - 1) As Integer, bv(nnz - 1) As Double
        For t = 0 To nnz - 1
            br(t) = rand.Next(bigM)
            bc(t) = rand.Next(bigN)
            bv(t) = rand.NextDouble() * 2 - 1
        Next
        Dim bigA As New SparseMatrix(br, bc, bv, bigM, bigN)
        Dim sw = System.Diagnostics.Stopwatch.StartNew()
        Dim bigSvd As New TruncatedSVD(bigA, 20)
        sw.Stop()
        Console.WriteLine($"完成耗时: {sw.ElapsedMilliseconds} ms")
        Console.WriteLine($"奇异值(top5): " & String.Join(", ", bigSvd.SingularValues.Take(5).Select(Function(x) x.ToString("F4"))))
        Dim bigX = bigSvd.ReducedMatrix
        Console.WriteLine($"降维结果: {bigX.Length}x{bigX(0).Length} 稠密矩阵")

        Console.WriteLine()
        Console.WriteLine("全部测试完成")
    End Sub
End Module

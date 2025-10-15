Imports std = System.Math

''' <summary>
''' NNLS Non-Negative Least-Squares algorithm
''' </summary>
Public Module NonNegativeLeastSquares
    ''' <summary>
    ''' 求解非负最小二乘问题：最小化 ||Ax - b||^2，满足 x >= 0
    ''' </summary>
    ''' <param name="A">设计矩阵 (m x n)</param>
    ''' <param name="b">观测向量 (m)</param>
    ''' <param name="maxIterations">最大迭代次数</param>
    ''' <param name="tolerance">收敛容差</param>
    ''' <returns>非负解向量 x (n)</returns>
    Public Function Solve(A As Double(,), b As Double(), Optional maxIterations As Integer = 1000, Optional tolerance As Double = 0.000000000001) As Double()
        Dim m As Integer = A.GetLength(0)  ' 行数
        Dim n As Integer = A.GetLength(1)  ' 列数

        If b.Length <> m Then
            Throw New ArgumentException("矩阵A和向量b的维度不匹配")
        End If

        ' 初始化
        Dim x As Double() = New Double(n - 1) {}  ' 解向量，初始为0
        Dim P As Boolean() = New Boolean(n - 1) {}  ' 被动集（true表示变量活跃）
        Dim W As Double() = New Double(n - 1) {}   ' 梯度向量
        Dim residual As Double() = New Double(m - 1) {}
        Array.Copy(b, residual, m)  ' 初始残差 = b （因为x=0）

        Dim iter As Integer = 0
        While iter < maxIterations
            iter += 1

            ' 计算梯度: W = A' * (b - A * x) = A' * residual
            For j As Integer = 0 To n - 1
                W(j) = 0.0
                For i As Integer = 0 To m - 1
                    W(j) += A(i, j) * residual(i)
                Next
            Next

            ' 检查收敛条件：所有变量满足KKT条件 (W_j <= tolerance 或 x_j > 0 且 |W_j| 很小)
            Dim maxGrad As Double = 0.0
            Dim maxGradIndex As Integer = -1
            For j As Integer = 0 To n - 1
                If Not P(j) AndAlso W(j) > maxGrad Then
                    maxGrad = W(j)
                    maxGradIndex = j
                End If
            Next

            If maxGrad < tolerance Then
                Exit While  ' 收敛
            End If

            ' 将具有最大正梯度的变量加入被动集
            P(maxGradIndex) = True

            ' 在被动集上迭代求解，直到解非负
            Dim innerIter As Integer = 0
            Dim maxInnerIter As Integer = n * 10 ' 防止内层无限循环
            Dim convergedInner As Boolean = False

            While Not convergedInner AndAlso innerIter < maxInnerIter
                innerIter += 1

                ' 构建被动集对应的子矩阵
                Dim activeVars As New List(Of Integer)()
                For j As Integer = 0 To n - 1
                    If P(j) Then activeVars.Add(j)
                Next
                Dim pCount As Integer = activeVars.Count
                If pCount = 0 Then Exit While

                ' 构建子矩阵 A_p
                Dim A_p As Double(,) = New Double(m - 1, pCount - 1) {}
                For i As Integer = 0 To m - 1
                    For k As Integer = 0 To pCount - 1
                        A_p(i, k) = A(i, activeVars(k))
                    Next
                Next

                ' 求解子问题: min ||A_p * x_p - b||^2
                Dim x_p As Double() = SolveUnconstrainedLS(A_p, b)

                ' 检查被动集上的解是否非负
                Dim allNonNegative As Boolean = True
                Dim minRatio As Double = 1.0
                Dim blockingIndex As Integer = -1

                For k As Integer = 0 To pCount - 1
                    If x_p(k) < -tolerance Then
                        allNonNegative = False
                        ' 计算步长比率，确保非负
                        Dim ratio As Double = x(activeVars(k)) / (x(activeVars(k)) - x_p(k))
                        If ratio < minRatio Then
                            minRatio = ratio
                            blockingIndex = activeVars(k)
                        End If
                    End If
                Next

                If allNonNegative Then
                    ' 接受解：更新被动集变量
                    For k As Integer = 0 To pCount - 1
                        x(activeVars(k)) = x_p(k)
                    Next
                    convergedInner = True
                Else
                    ' 调整解：x = x + minRatio * (x_p - x)
                    For k As Integer = 0 To pCount - 1
                        x(activeVars(k)) += minRatio * (x_p(k) - x(activeVars(k)))
                    Next

                    ' 将阻塞变量移出被动集（确保非负）
                    If blockingIndex >= 0 Then
                        P(blockingIndex) = False
                    Else
                        Exit While
                    End If

                    ' 更新残差以反映调整后的解
                    UpdateResidual(A, b, x, residual)
                End If
            End While

            ' 更新残差
            UpdateResidual(A, b, x, residual)
        End While

        If iter >= maxIterations Then
            ' 警告：未收敛
            Call "Warning: Maximum iterations reached. Solution may not be optimal.".warning
        End If

        Return x
    End Function

    ''' <summary>
    ''' 求解无约束最小二乘问题（使用正规方程法，可考虑扩展为SVD以提升稳定性）
    ''' </summary>
    Private Function SolveUnconstrainedLS(A As Double(,), b As Double()) As Double()
        Dim m As Integer = A.GetLength(0)
        Dim n As Integer = A.GetLength(1)

        ' 计算 A^T * A 和 A^T * b
        Dim ATA As Double(,) = New Double(n - 1, n - 1) {}
        Dim ATb As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            ATb(i) = 0.0
            For k As Integer = 0 To m - 1
                ATb(i) += A(k, i) * b(k)
            Next

            For j As Integer = 0 To n - 1
                ATA(i, j) = 0.0
                For k As Integer = 0 To m - 1
                    ATA(i, j) += A(k, i) * A(k, j)
                Next
            Next
        Next

        ' 使用高斯消元法求解线性方程组 ATA * x = ATb
        Return SolveLinearSystem(ATA, ATb)
    End Function

    ''' <summary>
    ''' 使用部分主元高斯消元法求解线性方程组，提升数值稳定性
    ''' </summary>
    Private Function SolveLinearSystem(A As Double(,), b As Double()) As Double()
        Dim n As Integer = b.Length
        Dim Ab As Double(,) = New Double(n - 1, n) {}
        Dim x As Double() = New Double(n - 1) {}

        ' 构建增广矩阵
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                Ab(i, j) = A(i, j)
            Next
            Ab(i, n) = b(i)
        Next

        ' 部分主元消元
        For pivot As Integer = 0 To n - 1
            ' 查找主元
            Dim maxRow As Integer = pivot
            For row As Integer = pivot + 1 To n - 1
                If std.Abs(Ab(row, pivot)) > std.Abs(Ab(maxRow, pivot)) Then
                    maxRow = row
                End If
            Next

            ' 交换行
            If maxRow <> pivot Then
                For col As Integer = pivot To n
                    Dim temp As Double = Ab(pivot, col)
                    Ab(pivot, col) = Ab(maxRow, col)
                    Ab(maxRow, col) = temp
                Next
            End If

            ' 消元
            If std.Abs(Ab(pivot, pivot)) < 0.000000000000001 Then
                Throw New InvalidOperationException("矩阵奇异，无法求解")
            End If

            For row As Integer = pivot + 1 To n - 1
                Dim factor As Double = Ab(row, pivot) / Ab(pivot, pivot)
                For col As Integer = pivot To n
                    Ab(row, col) -= factor * Ab(pivot, col)
                Next
            Next
        Next

        ' 回代
        For i As Integer = n - 1 To 0 Step -1
            Dim sum As Double = 0.0
            For j As Integer = i + 1 To n - 1
                sum += Ab(i, j) * x(j)
            Next
            x(i) = (Ab(i, n) - sum) / Ab(i, i)
        Next

        Return x
    End Function

    ''' <summary>
    ''' 更新残差向量 r = b - A*x
    ''' </summary>
    Private Sub UpdateResidual(A As Double(,), b As Double(), x As Double(), residual As Double())
        Dim m As Integer = A.GetLength(0)
        Dim n As Integer = A.GetLength(1)

        For i As Integer = 0 To m - 1
            residual(i) = b(i)
            For j As Integer = 0 To n - 1
                residual(i) -= A(i, j) * x(j)
            Next
        Next
    End Sub
End Module
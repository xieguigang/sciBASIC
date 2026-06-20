Imports stdf = System.Math

Namespace LinearAlgebra.Matrix

    ''' <summary>
    ''' 矩阵运算模块 - 使用 VB.NET 基础数学函数实现线性代数运算
    ''' 包含：矩阵乘法、转置、求逆、行列式、特征值分解等
    ''' </summary>
    Public Module MatrixOps

        ''' <summary>矩阵乘法 C = A × B</summary>
        Public Function Multiply(a As Double(,), b As Double(,)) As Double(,)
            Dim rowsA = a.GetLength(0)
            Dim colsA = a.GetLength(1)
            Dim colsB = b.GetLength(1)
            Dim result(rowsA - 1, colsB - 1) As Double
            For i = 0 To rowsA - 1
                For j = 0 To colsB - 1
                    Dim sum = 0.0
                    For k = 0 To colsA - 1
                        sum += a(i, k) * b(k, j)
                    Next
                    result(i, j) = sum
                Next
            Next
            Return result
        End Function

        ''' <summary>矩阵转置</summary>
        Public Function Transpose(a As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(cols - 1, rows - 1) As Double
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    result(j, i) = a(i, j)
                Next
            Next
            Return result
        End Function

        ''' <summary>矩阵加法</summary>
        Public Function Add(a As Double(,), b As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(rows - 1, cols - 1) As Double
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    result(i, j) = a(i, j) + b(i, j)
                Next
            Next
            Return result
        End Function

        ''' <summary>矩阵减法</summary>
        Public Function Subtract(a As Double(,), b As Double(,)) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(rows - 1, cols - 1) As Double
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    result(i, j) = a(i, j) - b(i, j)
                Next
            Next
            Return result
        End Function

        ''' <summary>标量乘矩阵</summary>
        Public Function Scale(a As Double(,), s As Double) As Double(,)
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(rows - 1, cols - 1) As Double
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    result(i, j) = a(i, j) * s
                Next
            Next
            Return result
        End Function

        ''' <summary>矩阵向量乘法 y = A × x</summary>
        Public Function MultiplyVec(a As Double(,), x As Double()) As Double()
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            Dim result(rows - 1) As Double
            For i = 0 To rows - 1
                Dim sum = 0.0
                For j = 0 To cols - 1
                    sum += a(i, j) * x(j)
                Next
                result(i) = sum
            Next
            Return result
        End Function

        ''' <summary>
        ''' 矩阵求逆 - 使用 Gauss-Jordan 消元法带部分主元选取
        ''' </summary>
        Public Function Inverse(a As Double(,)) As Double(,)
            Dim n = a.GetLength(0)
            If a.GetLength(1) <> n Then Throw New Exception("矩阵必须为方阵才能求逆")

            ' 构造增广矩阵 [a | I]
            Dim aug(n - 1, 2 * n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    aug(i, j) = a(i, j)
                Next
                aug(i, i + n) = 1.0
            Next

            ' 前向消元（带部分主元选取）
            For col = 0 To n - 1
                ' 寻找主元
                Dim maxRow = col
                Dim maxVal = stdf.Abs(aug(col, col))
                For row = col + 1 To n - 1
                    If stdf.Abs(aug(row, col)) > maxVal Then
                        maxVal = stdf.Abs(aug(row, col))
                        maxRow = row
                    End If
                Next

                ' 交换行
                If maxRow <> col Then
                    For j = 0 To 2 * n - 1
                        Dim temp = aug(col, j)
                        aug(col, j) = aug(maxRow, j)
                        aug(maxRow, j) = temp
                    Next
                End If

                ' 主元行归一化
                Dim pivot = aug(col, col)
                If stdf.Abs(pivot) < 0.00000000000001 Then Throw New Exception("矩阵奇异，无法求逆")
                For j = 0 To 2 * n - 1
                    aug(col, j) /= pivot
                Next

                ' 消去其他行
                For row = 0 To n - 1
                    If row <> col Then
                        Dim factor = aug(row, col)
                        For j = 0 To 2 * n - 1
                            aug(row, j) -= factor * aug(col, j)
                        Next
                    End If
                Next
            Next

            ' 提取逆矩阵
            Dim result(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    result(i, j) = aug(i, j + n)
                Next
            Next
            Return result
        End Function

        ''' <summary>计算行列式（递归展开法，适用于小矩阵）</summary>
        Public Function Determinant(a As Double(,)) As Double
            Dim n = a.GetLength(0)
            If a.GetLength(1) <> n Then Throw New Exception("行列式要求方阵")
            If n = 1 Then Return a(0, 0)
            If n = 2 Then Return a(0, 0) * a(1, 1) - a(0, 1) * a(1, 0)

            ' 使用 LU 分解法计算行列式（更稳定）
            Dim lu(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    lu(i, j) = a(i, j)
                Next
            Next

            Dim det = 1.0
            For k = 0 To n - 1
                ' 部分主元
                Dim maxRow = k
                Dim maxVal = stdf.Abs(lu(k, k))
                For i = k + 1 To n - 1
                    If stdf.Abs(lu(i, k)) > maxVal Then
                        maxVal = stdf.Abs(lu(i, k))
                        maxRow = i
                    End If
                Next
                If maxRow <> k Then
                    For j = 0 To n - 1
                        Dim temp = lu(k, j)
                        lu(k, j) = lu(maxRow, j)
                        lu(maxRow, j) = temp
                    Next
                    det = -det
                End If

                If stdf.Abs(lu(k, k)) < 0.00000000000001 Then Return 0.0

                det *= lu(k, k)
                For i = k + 1 To n - 1
                    lu(i, k) /= lu(k, k)
                    For j = k + 1 To n - 1
                        lu(i, j) -= lu(i, k) * lu(k, j)
                    Next
                Next
            Next
            Return det
        End Function

        ''' <summary>
        ''' 对称矩阵特征值分解 - 使用 Jacobi 旋转法
        ''' 返回 (eigenvalues, eigenvectors)，eigenvectors 的列是对应的特征向量
        ''' </summary>
        Public Function JacobiEigen(a As Double(,)) As (eigenvalues As Double(), eigenvectors As Double(,))
            Dim n = a.GetLength(0)
            If a.GetLength(1) <> n Then Throw New Exception("特征值分解要求方阵")

            Dim mat(n - 1, n - 1) As Double
            Dim v(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    mat(i, j) = a(i, j)
                Next
                v(i, i) = 1.0
            Next

            Dim maxIter = 200
            Dim tol = 0.000000000001

            For iter = 1 To maxIter
                ' 找最大的非对角元素
                Dim maxOff = 0.0
                Dim p = 0, q = 0
                For i = 0 To n - 2
                    For j = i + 1 To n - 1
                        If stdf.Abs(mat(i, j)) > maxOff Then
                            maxOff = stdf.Abs(mat(i, j))
                            p = i
                            q = j
                        End If
                    Next
                Next

                If maxOff < tol Then Exit For

                ' 计算 Jacobi 旋转
                Dim app = mat(p, p)
                Dim aqq = mat(q, q)
                Dim apq = mat(p, q)

                Dim theta As Double
                If stdf.Abs(app - aqq) < 1.0E-30 Then
                    theta = stdf.PI / 4
                Else
                    theta = 0.5 * stdf.Atan2(2 * apq, app - aqq)
                End If

                Dim c = stdf.Cos(theta)
                Dim s = stdf.Sin(theta)

                ' 应用旋转
                For i = 0 To n - 1
                    Dim tempPi = mat(i, p)
                    Dim tempQi = mat(i, q)
                    mat(i, p) = c * tempPi + s * tempQi
                    mat(i, q) = -s * tempPi + c * tempQi
                Next
                For j = 0 To n - 1
                    Dim tempPj = mat(p, j)
                    Dim tempQj = mat(q, j)
                    mat(p, j) = c * tempPj + s * tempQj
                    mat(q, j) = -s * tempPj + c * tempQj
                Next
                For i = 0 To n - 1
                    Dim tempVi = v(i, p)
                    Dim tempVi2 = v(i, q)
                    v(i, p) = c * tempVi + s * tempVi2
                    v(i, q) = -s * tempVi + c * tempVi2
                Next
            Next

            Dim eigenvalues(n - 1) As Double
            For i = 0 To n - 1
                eigenvalues(i) = mat(i, i)
            Next

            Return (eigenvalues, v)
        End Function

        ''' <summary>矩阵的迹（对角元素之和）</summary>
        Public Function Trace(a As Double(,)) As Double
            Dim n = stdf.Min(a.GetLength(0), a.GetLength(1))
            Dim tr = 0.0
            For i = 0 To n - 1
                tr += a(i, i)
            Next
            Return tr
        End Function

        ''' <summary>打印矩阵（调试用）</summary>
        Public Sub PrintMatrix(a As Double(,), Optional name As String = "")
            If name <> "" Then Console.WriteLine($"{name}:")
            Dim rows = a.GetLength(0)
            Dim cols = a.GetLength(1)
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    Console.Write($"{a(i, j),10:F4} ")
                Next
                Console.WriteLine()
            Next
        End Sub

    End Module
End Namespace
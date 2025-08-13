Imports System.Drawing
Imports System.Math
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace LinearAlgebra

    Public Class EllipseFitResult
        Public Property Center As PointF
        Public Property SemiMajorAxis As Single ' 长半轴
        Public Property SemiMinorAxis As Single ' 短半轴
        Public Property RotationAngle As Single ' 弧度制

        Public Function CreateShape() As EllipseShape
            Return New EllipseShape(SemiMajorAxis, SemiMinorAxis, Center)
        End Function

        Public Shared Function FitEllipse(points As PointF()) As EllipseFitResult
            If points.Length < 5 Then
                Call "至少需要5个点进行拟合".Warning
                Return Nothing
            End If

            ' 1. 构建矩阵 M 和向量 B
            Dim n As Integer = points.Length
            Dim M(n - 1, 5) As Double
            Dim B(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim x = points(i).X, y = points(i).Y
                M(i, 0) = x * x    ' x²
                M(i, 1) = x * y    ' xy
                M(i, 2) = y * y    ' y²
                M(i, 3) = x        ' x
                M(i, 4) = y        ' y
                M(i, 5) = 1        ' 常数项
                B(i) = -x * x      ' -x² (用于构造B向量)
            Next

            ' 2. 解线性方程组 (MᵀM)V = MᵀB
            Dim MT = MatrixTranspose(M)
            Dim MTM = MatrixMultiply(MT, M)
            Dim MTB = MatrixMultiply(MT, B)
            Dim V = SolveLinearSystem(MTM, MTB) ' 解出参数向量V=[A,B,C,D,E,F]

            ' 3. 提取参数
            Dim A = V(0), Bp = V(1), C = V(2), D = V(3), E = V(4), F = V(5)

            ' 4. 转换为几何参数
            Dim discriminant = 4 * A * C - Bp * Bp
            If discriminant <= 0 Then
                Call "拟合结果非椭圆 (B²-4AC≥0)".Warning
                Return Nothing
            End If

            Dim x0 = (2 * C * D - Bp * E) / discriminant
            Dim y0 = (2 * A * E - Bp * D) / discriminant

            ' 计算长短半轴
            Dim numerator = 2 * (A * E * E + C * D * D - Bp * D * E + discriminant * F - A * C * F)
            Dim term1 = A + C
            Dim term2 = Sqrt((A - C) * (A - C) + Bp * Bp)
            ' 计算旋转角 (弧度)
            Dim angle = 0.5 * Atan2(Bp, A - C)

            Dim ra = Sqrt(numerator / (discriminant * (term2 - term1)))
            Dim rb = Sqrt(numerator / (discriminant * (-term2 - term1)))

            ' 确保a为长半轴，b为短半轴
            If ra < rb Then
                Dim temp = ra : ra = rb : rb = temp
            End If

            Return New EllipseFitResult With {
        .Center = New PointF(x0, y0),
        .SemiMajorAxis = ra,
        .SemiMinorAxis = rb,
        .RotationAngle = angle
    }
        End Function

        ' --- 以下为辅助数学函数 ---
        Private Shared Function MatrixTranspose(matrix As Double(,)) As Double(,)
            Dim rows = matrix.GetLength(0), cols = matrix.GetLength(1)
            Dim result(cols - 1, rows - 1) As Double
            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    result(j, i) = matrix(i, j)
                Next
            Next
            Return result
        End Function

        Private Shared Function MatrixMultiply(matrix1 As Double(,), matrix2 As Double()) As Double()
            Dim rows = matrix1.GetLength(0), cols = matrix1.GetLength(1)
            Dim result(rows - 1) As Double
            For i = 0 To rows - 1
                Dim sum = 0.0
                For k = 0 To cols - 1
                    sum += matrix1(i, k) * matrix2(k)
                Next
                result(i) = sum
            Next
            Return result
        End Function

        Private Shared Function MatrixMultiply(matrix1 As Double(,), matrix2 As Double(,)) As Double(,)
            Dim m = matrix1.GetLength(0), n = matrix1.GetLength(1), p = matrix2.GetLength(1)
            Dim result(m - 1, p - 1) As Double
            For i = 0 To m - 1
                For j = 0 To p - 1
                    Dim sum = 0.0
                    For k = 0 To n - 1
                        sum += matrix1(i, k) * matrix2(k, j)
                    Next
                    result(i, j) = sum
                Next
            Next
            Return result
        End Function

        Private Shared Function SolveLinearSystem(A As Double(,), b As Double()) As Double()
            ' 使用高斯消元法求解 Ax = b
            Dim n = b.Length
            Dim augmented(n - 1, n) As Double

            ' 构造增广矩阵
            For i = 0 To n - 1
                For j = 0 To n - 1
                    augmented(i, j) = A(i, j)
                Next
                augmented(i, n) = b(i)
            Next

            ' 前向消元
            For pivot = 0 To n - 2
                For row = pivot + 1 To n - 1
                    Dim factor = augmented(row, pivot) / augmented(pivot, pivot)
                    For col = pivot To n
                        augmented(row, col) -= factor * augmented(pivot, col)
                    Next
                Next
            Next

            ' 回代求解
            Dim x(n - 1) As Double
            For i = n - 1 To 0 Step -1
                Dim sum = augmented(i, n)
                For j = i + 1 To n - 1
                    sum -= augmented(i, j) * x(j)
                Next
                x(i) = sum / augmented(i, i)
            Next
            Return x
        End Function
    End Class
End Namespace
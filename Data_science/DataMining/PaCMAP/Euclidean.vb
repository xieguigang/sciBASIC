Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace PaCMAP
    ''' <summary>
    ''' 欧几里得距离计算
    ''' Euclidean distance calculation
    ''' 
    ''' 原始JavaScript代码:
    ''' export function euclideanDistance(a, b) {
    '''   return tf.tidy(() => a.expandDims(1).sub(b).square().sum(-1).sqrt());
    ''' }
    ''' </summary>
    Public Module EuclideanDistance
        ''' <summary>
        ''' 计算两个张量之间的欧几里得距离矩阵
        ''' Calculate Euclidean distance matrix between two tensors
        ''' 
        ''' 对于输入张量 a (shape: [N, D]) 和 b (shape: [M, D]):
        ''' 1. expandDims(1): a 变为 [N, 1, D]
        ''' 2. sub(b): 广播减法得到 [N, M, D]
        ''' 3. square(): 元素平方 [N, M, D]
        ''' 4. sum(-1): 沿最后一维求和 [N, M]
        ''' 5. sqrt(): 元素平方根 [N, M]
        ''' 
        ''' 结果是距离矩阵，其中 result[i,j] = ||a[i] - b[j]||_2
        ''' </summary>
        ''' <param name="a">第一个张量，形状 [N, D]</param>
        ''' <param name="b">第二个张量，形状 [M, D]</param>
        ''' <returns>距离矩阵，形状 [N, M]</returns>
        Public Function Compute(a As Tensor, b As Tensor) As Tensor
            If a.Rank <> 2 OrElse b.Rank <> 2 Then
                Throw New ArgumentException("Euclidean distance requires 2D tensors")
            End If

            If a.Shape(1) <> b.Shape(1) Then
                Throw New ArgumentException($"Feature dimensions must match: {a.Shape(1)} vs {b.Shape(1)}")
            End If

            Dim n As Integer = a.Shape(0)
            Dim m As Integer = b.Shape(0)
            Dim d As Integer = a.Shape(1)

            Dim result = New Tensor(New Integer() {n, m})

            ' 计算欧几里得距离矩阵
            ' 使用公式: ||a - b||^2 = ||a||^2 + ||b||^2 - 2*a·b
            ' 这比直接计算更高效

            For i = 0 To n - 1
                For j = 0 To m - 1
                    Dim sumSq As Double = 0
                    For k = 0 To d - 1
                        Dim diff As Double = a.Data(i * d + k) - b.Data(j * d + k)
                        sumSq += diff * diff
                    Next
                    result.Data(i * m + j) = std.Sqrt(sumSq)
                Next
            Next

            Return result
        End Function

        ''' <summary>
        ''' 使用链式调用方式计算欧几里得距离
        ''' Calculate Euclidean distance using chainable extension methods
        ''' 
        ''' 实现与原始JavaScript代码相同的链式调用:
        ''' a.expandDims(1).sub(b).square().sum(-1).sqrt()
        ''' </summary>
        Public Function ComputeChainable(a As Tensor, b As Tensor) As Tensor
            ' 链式调用实现
            ' a.expandDims(1) -> [N, 1, D]
            ' .sub(b) -> [N, M, D] (广播)
            ' .square() -> [N, M, D]
            ' .sum(-1) -> [N, M]
            ' .sqrt() -> [N, M]

            Return TensorExtensions.Sum(TensorExtensions.Square(EuclideanDistanceExtensions.BroadcastSubForDistance(TensorExtensions.ExpandDims(a, 1), CType(b, Tensor))), -1).Sqrt()           ' [N, 1, D]
            ' [N, M, D]
            ' [N, M, D]
            ' [N, M]
            ' [N, M]
        End Function
    End Module

    ''' <summary>
    ''' 欧几里得距离的扩展方法
    ''' Extension methods for Euclidean distance
    ''' </summary>
    Public Module EuclideanDistanceExtensions
        ''' <summary>
        ''' 计算与另一个张量的欧几里得距离矩阵
        ''' Calculate Euclidean distance matrix with another tensor
        ''' </summary>
        <Extension()>
        Public Function EuclideanDistanceTo(a As Tensor, b As Tensor) As Tensor
            Return EuclideanDistance.Compute(a, b)
        End Function

        ''' <summary>
        ''' 广播减法（专门用于距离计算）
        ''' Broadcast subtraction for distance calculation
        ''' a: [N, D] -> expandDims -> [N, 1, D]
        ''' b: [M, D]
        ''' result: [N, M, D]
        ''' </summary>
        <Extension()>
        Public Function BroadcastSubForDistance(a As Tensor, b As Tensor) As Tensor
            If a.Rank = 3 AndAlso b.Rank = 2 AndAlso a.Shape(1) = 1 AndAlso a.Shape(2) = b.Shape(1) Then
                ' a: [N, 1, D], b: [M, D] -> result: [N, M, D]
                Dim n As Integer = a.Shape(0)
                Dim m As Integer = b.Shape(0)
                Dim d As Integer = a.Shape(2)

                Dim result = New Tensor(New Integer() {n, m, d})

                For i = 0 To n - 1
                    For j = 0 To m - 1
                        For k = 0 To d - 1
                            Dim aIdx = i * d + k
                            Dim bIdx = j * d + k
                            Dim rIdx = i * m * d + j * d + k
                            result.Data(rIdx) = a.Data(aIdx) - b.Data(bIdx)
                        Next
                    Next
                Next

                Return result
            End If

            ' 如果a已经是[N, 1, D]格式
            If a.Rank = 3 AndAlso b.Rank = 3 AndAlso a.Shape(0) = b.Shape(0) AndAlso a.Shape(1) = 1 AndAlso a.Shape(2) = b.Shape(2) Then


                Dim n As Integer = a.Shape(0)
                Dim m As Integer = b.Shape(1)
                Dim d As Integer = a.Shape(2)

                Dim result = New Tensor(New Integer() {n, m, d})

                For i = 0 To n - 1
                    For j = 0 To m - 1
                        For k = 0 To d - 1
                            Dim aIdx = i * d + k
                            Dim bIdx = i * m * d + j * d + k
                            Dim rIdx = i * m * d + j * d + k
                            result.Data(rIdx) = a.Data(aIdx) - b.Data(bIdx)
                        Next
                    Next
                Next

                Return result
            End If

            Throw New ArgumentException($"Cannot broadcast subtract shapes [{String.Join(",", a.Shape)}] and [{String.Join(",", b.Shape)}]")
        End Function
    End Module
End Namespace

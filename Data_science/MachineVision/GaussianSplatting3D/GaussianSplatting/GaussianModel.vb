Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 3D 高斯模型 - 存储所有高斯椭球的参数
''' 
''' 每个高斯由以下参数定义：
'''   - 位置 (mean)        μ ∈ R³       3 个参数
'''   - 缩放 (scale)       s ∈ R³       3 个参数（各向异性缩放）
'''   - 旋转 (rotation)    q ∈ R⁴       4 个参数（四元数）
'''   - 颜色 (color)       c ∈ R³       3 个参数（RGB，0-1）
'''   - 不透明度 (opacity) α ∈ R        1 个参数（通过 sigmoid 映射到 [0,1]）
''' 
''' 协方差矩阵通过 scale 和 rotation 计算：
'''   Σ = R * S * S^T * R^T
''' 其中 R 是从四元数 q 构造的 3x3 旋转矩阵，S = diag(s)。
''' 
''' 这种参数化保证了 Σ 始终半正定，且梯度可以反向传播到所有参数。
''' </summary>
Public Class GaussianModel

    ''' <summary>高斯数量</summary>
    Public Property Count As Integer

    ' ---- 参数存储（每个高斯一行）----
    ''' <summary>位置 [N, 3]</summary>
    Public Property Positions As Tensor

    ''' <summary>缩放（log空间，确保为正）[N, 3]</summary>
    Public Property Scales As Tensor

    ''' <summary>旋转四元数 [N, 4]</summary>
    Public Property Rotations As Tensor

    ''' <summary>颜色 RGB [N, 3]</summary>
    Public Property Colors As Tensor

    ''' <summary>不透明度（logit空间）[N, 1]</summary>
    Public Property Opacities As Tensor

    ' ---- 梯度存储 ----
    Public PositionsGrad As Tensor
    Public ScalesGrad As Tensor
    Public RotationsGrad As Tensor
    Public ColorsGrad As Tensor
    Public OpacitiesGrad As Tensor

    ' ---- Adam 优化器状态 ----
    Public PositionsM As Tensor, PositionsV As Tensor
    Public ScalesM As Tensor, ScalesV As Tensor
    Public RotationsM As Tensor, RotationsV As Tensor
    Public ColorsM As Tensor, ColorsV As Tensor
    Public OpacitiesM As Tensor, OpacitiesV As Tensor

    Public Sub New(count As Integer)
        Me.Count = count
        Positions = Tensor.Zeros({count, 3})
        Scales = Tensor.Zeros({count, 3})
        Rotations = Tensor.Zeros({count, 4})
        Colors = Tensor.Zeros({count, 3})
        Opacities = Tensor.Zeros({count, 1})

        ' 初始化梯度为零
        ZeroGradients()

        ' 初始化 Adam 状态为零
        PositionsM = Tensor.Zeros({count, 3}) : PositionsV = Tensor.Zeros({count, 3})
        ScalesM = Tensor.Zeros({count, 3}) : ScalesV = Tensor.Zeros({count, 3})
        RotationsM = Tensor.Zeros({count, 4}) : RotationsV = Tensor.Zeros({count, 4})
        ColorsM = Tensor.Zeros({count, 3}) : ColorsV = Tensor.Zeros({count, 3})
        OpacitiesM = Tensor.Zeros({count, 1}) : OpacitiesV = Tensor.Zeros({count, 1})
    End Sub

    ''' <summary>
    ''' 将所有梯度清零
    ''' </summary>
    Public Sub ZeroGradients()
        PositionsGrad = Tensor.Zeros({Count, 3})
        ScalesGrad = Tensor.Zeros({Count, 3})
        RotationsGrad = Tensor.Zeros({Count, 4})
        ColorsGrad = Tensor.Zeros({Count, 3})
        OpacitiesGrad = Tensor.Zeros({Count, 1})
    End Sub

    ''' <summary>
    ''' 从一个 3D 点云初始化高斯模型
    ''' Initialize the Gaussian model from a 3D point cloud.
    ''' 每个点变成一个小的各向同性高斯。
    ''' </summary>
    ''' <param name="pts">点云位置 [N, 3]</param>
    ''' <param name="cols">点云颜色 [N, 3]，范围 [0,1]</param>
    ''' <param name="initScale">初始缩放（世界单位）</param>
    Public Shared Function FromPointCloud(pts As Tensor, cols As Tensor, initScale As Single) As GaussianModel
        Dim n = pts.Shape(0)
        Dim model As New GaussianModel(n)

        ' 位置 = 点云位置
        For i = 0 To n - 1
            For j = 0 To 2
                model.Positions(i, j) = pts(i, j)
                model.Colors(i, j) = std.Max(0.0, std.Min(1.0, cols(i, j)))
            Next
            ' 缩放：log(initScale) 使得 exp(log_s) = initScale
            For j = 0 To 2
                model.Scales(i, j) = std.Log(initScale)
            Next
            ' 旋转：单位四元数 (1, 0, 0, 0)
            model.Rotations(i, 0) = 1.0
            model.Rotations(i, 1) = 0.0
            model.Rotations(i, 2) = 0.0
            model.Rotations(i, 3) = 0.0
            ' 不透明度：logit(0.1) ≈ -2.197
            model.Opacities(i, 0) = -2.197
        Next

        Return model
    End Function

    ''' <summary>
    ''' 从均匀分布的随机点云初始化（用于无先验的从头训练）
    ''' </summary>
    Public Shared Function FromRandom(count As Integer, bounds As Single(), initScale As Single, seed As Integer) As GaussianModel
        Dim rng As New Random(seed)
        Dim model As New GaussianModel(count)
        For i = 0 To count - 1
            ' 在边界框内均匀采样
            model.Positions(i, 0) = rng.NextDouble() * (bounds(3) - bounds(0)) + bounds(0)
            model.Positions(i, 1) = rng.NextDouble() * (bounds(4) - bounds(1)) + bounds(1)
            model.Positions(i, 2) = rng.NextDouble() * (bounds(5) - bounds(2)) + bounds(2)
            ' 随机颜色
            model.Colors(i, 0) = rng.NextDouble()
            model.Colors(i, 1) = rng.NextDouble()
            model.Colors(i, 2) = rng.NextDouble()
            ' 缩放
            For j = 0 To 2
                model.Scales(i, j) = std.Log(initScale)
            Next
            ' 单位四元数
            model.Rotations(i, 0) = 1.0
            ' 不透明度
            model.Opacities(i, 0) = -2.197
        Next
        Return model
    End Function

    ''' <summary>
    ''' 从四元数构造 3x3 旋转矩阵
    ''' Convert a quaternion (w, x, y, z) to a 3x3 rotation matrix.
    ''' </summary>
    Public Shared Function QuaternionToMatrix(w As Double, x As Double, y As Double, z As Double) As Double(,)
        ' 归一化
        Dim n = std.Sqrt(w * w + x * x + y * y + z * z)
        If n < 0.000000000001 Then
            Return New Double(,) {{1, 0, 0}, {0, 1, 0}, {0, 0, 1}}
        End If
        w /= n : x /= n : y /= n : z /= n

        Dim xx = x * x, yy = y * y, zz = z * z
        Dim xy = x * y, xz = x * z, yz = y * z
        Dim wx = w * x, wy = w * y, wz = w * z

        Dim R(2, 2) As Double
        R(0, 0) = 1 - 2 * (yy + zz)
        R(0, 1) = 2 * (xy - wz)
        R(0, 2) = 2 * (xz + wy)
        R(1, 0) = 2 * (xy + wz)
        R(1, 1) = 1 - 2 * (xx + zz)
        R(1, 2) = 2 * (yz - wx)
        R(2, 0) = 2 * (xz - wy)
        R(2, 1) = 2 * (yz + wx)
        R(2, 2) = 1 - 2 * (xx + yy)
        Return R
    End Function

    ''' <summary>
    ''' 计算第 i 个高斯的 3x3 协方差矩阵
    ''' Compute the 3x3 covariance matrix of the i-th Gaussian.
    ''' Σ = R * diag(s²) * R^T
    ''' </summary>
    Public Function GetCovariance(i As Integer) As Double(,)
        Dim sx = std.Exp(Scales(i, 0))
        Dim sy = std.Exp(Scales(i, 1))
        Dim sz = std.Exp(Scales(i, 2))
        Dim w = Rotations(i, 0), x = Rotations(i, 1), y = Rotations(i, 2), z = Rotations(i, 3)
        Dim matR = QuaternionToMatrix(w, x, y, z)

        ' Σ = R * diag(s²) * R^T
        Dim S2(2, 2) As Double
        S2(0, 0) = sx * sx : S2(1, 1) = sy * sy : S2(2, 2) = sz * sz

        Dim RS(2, 2) As Double
        For r = 0 To 2
            For c = 0 To 2
                Dim s = 0.0
                For k = 0 To 2
                    s += matR(r, k) * S2(k, c)
                Next
                RS(r, c) = s
            Next
        Next

        Dim Sigma(2, 2) As Double
        For r = 0 To 2
            For c = 0 To 2
                Dim s = 0.0
                For k = 0 To 2
                    s += RS(r, k) * matR(c, k)  ' R^T = transpose
                Next
                Sigma(r, c) = s
            Next
        Next
        Return Sigma
    End Function

    ''' <summary>
    ''' 获取第 i 个高斯的不透明度（经过 sigmoid）
    ''' </summary>
    Public Function GetOpacity(i As Integer) As Double
        Dim x = Opacities(i, 0)
        ' sigmoid
        If x >= 0 Then
            Dim ex = std.Exp(-x)
            Return 1.0 / (1.0 + ex)
        Else
            Dim ex = std.Exp(x)
            Return ex / (1.0 + ex)
        End If
    End Function

    ''' <summary>
    ''' 添加新的高斯（用于自适应密度控制）
    ''' </summary>
    Public Sub AddGaussians(newPositions As Tensor, newScales As Tensor, newRotations As Tensor,
                            newColors As Tensor, newOpacities As Tensor)
        Dim newCount = newPositions.Shape(0)
        Dim totalCount = Count + newCount

        Positions = Concatenate2D(Positions, newPositions)
        Scales = Concatenate2D(Scales, newScales)
        Rotations = Concatenate2D(Rotations, newRotations)
        Colors = Concatenate2D(Colors, newColors)
        Opacities = Concatenate2D(Opacities, newOpacities)

        ' 重新初始化梯度和 Adam 状态
        Count = totalCount
        ZeroGradients()
        PositionsM = Tensor.Zeros({Count, 3}) : PositionsV = Tensor.Zeros({Count, 3})
        ScalesM = Tensor.Zeros({Count, 3}) : ScalesV = Tensor.Zeros({Count, 3})
        RotationsM = Tensor.Zeros({Count, 4}) : RotationsV = Tensor.Zeros({Count, 4})
        ColorsM = Tensor.Zeros({Count, 3}) : ColorsV = Tensor.Zeros({Count, 3})
        OpacitiesM = Tensor.Zeros({Count, 1}) : OpacitiesV = Tensor.Zeros({Count, 1})
    End Sub

    ''' <summary>
    ''' 删除指定索引的高斯（用于自适应密度控制）
    ''' </summary>
    Public Sub RemoveGaussians(indicesToRemove As HashSet(Of Integer))
        If indicesToRemove.Count = 0 Then Return
        Dim keepList As New List(Of Integer)()
        For i = 0 To Count - 1
            If Not indicesToRemove.Contains(i) Then
                keepList.Add(i)
            End If
        Next
        Dim newCount = keepList.Count
        If newCount = 0 Then Return

        Positions = SelectRows(Positions, keepList)
        Scales = SelectRows(Scales, keepList)
        Rotations = SelectRows(Rotations, keepList)
        Colors = SelectRows(Colors, keepList)
        Opacities = SelectRows(Opacities, keepList)

        Count = newCount
        ZeroGradients()
        PositionsM = Tensor.Zeros({Count, 3}) : PositionsV = Tensor.Zeros({Count, 3})
        ScalesM = Tensor.Zeros({Count, 3}) : ScalesV = Tensor.Zeros({Count, 3})
        RotationsM = Tensor.Zeros({Count, 4}) : RotationsV = Tensor.Zeros({Count, 4})
        ColorsM = Tensor.Zeros({Count, 3}) : ColorsV = Tensor.Zeros({Count, 3})
        OpacitiesM = Tensor.Zeros({Count, 1}) : OpacitiesV = Tensor.Zeros({Count, 1})
    End Sub

    ' ---- 辅助方法 ----
    Private Shared Function Concatenate2D(a As Tensor, b As Tensor) As Tensor
        Dim rowsA = a.Shape(0), rowsB = b.Shape(0), cols = a.Shape(1)
        Dim result = New Tensor(rowsA + rowsB, cols)
        For i = 0 To rowsA - 1
            For j = 0 To cols - 1
                result(i, j) = a(i, j)
            Next
        Next
        For i = 0 To rowsB - 1
            For j = 0 To cols - 1
                result(rowsA + i, j) = b(i, j)
            Next
        Next
        Return result
    End Function

    Private Shared Function SelectRows(t As Tensor, indices As List(Of Integer)) As Tensor
        Dim cols = t.Shape(1)
        Dim result = New Tensor(indices.Count, cols)
        For i = 0 To indices.Count - 1
            For j = 0 To cols - 1
                result(i, j) = t(indices(i), j)
            Next
        Next
        Return result
    End Function

End Class

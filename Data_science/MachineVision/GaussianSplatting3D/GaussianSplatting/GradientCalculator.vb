Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 梯度计算器 - 通过有限差分法计算损失对高斯参数的梯度
''' 
''' Gradient Calculator (Finite Differences)
''' =========================================
''' 完整的 3D Gaussian Splatting 使用解析反向传播（自定义 autograd），
''' 但实现复杂度极高。本实现采用**中心差分法**计算数值梯度：
'''   ∂L/∂θ_i ≈ (L(θ + ε) - L(θ - ε)) / (2ε)
''' 
''' 优点：
'''   - 实现简单，对任意可微参数都适用
'''   - 不需要手动推导反向传播公式
'''   - 易于验证正确性
''' 
''' 缺点：
'''   - 计算量为 O(N_params) 次前向渲染
'''   - 仅适用于 demo / 小规模验证
''' 
''' 为了加速，我们采用以下策略：
'''   1. 每次迭代只采样一部分高斯进行梯度计算（mini-batch）
'''   2. 对每个高斯，只计算其位置和颜色的梯度（最重要的参数）
'''   3. 缩放、旋转、不透明度使用较大的扰动步长
''' </summary>
Public Class GradientCalculator

    ''' <summary>有限差分步长</summary>
    Public Property Epsilon As Double = 0.01

    ''' <summary>每次迭代采样的高斯数量（0 表示全部）</summary>
    Public Property BatchSize As Integer = 0

    Public Sub New(Optional epsilon As Double = 0.01, Optional batchSize As Integer = 0)
        Me.Epsilon = epsilon
        Me.BatchSize = batchSize
    End Sub

    ''' <summary>
    ''' 计算损失对高斯模型所有参数的梯度
    ''' Compute gradients of the loss w.r.t. all Gaussian parameters.
    ''' </summary>
    ''' <param name="model">高斯模型</param>
    ''' <param name="cameras">所有相机</param>
    ''' <param name="targets">所有目标图像</param>
    Public Sub ComputeGradients(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor))
        model.ZeroGradients()

        ' 决定要计算哪些高斯的梯度
        Dim indicesToCompute As New List(Of Integer)()
        If BatchSize > 0 AndAlso BatchSize < model.Count Then
            ' 随机采样
            Dim rng As New Random(42)
            Dim pool As New HashSet(Of Integer)()
            While pool.Count < BatchSize
                pool.Add(rng.Next(model.Count))
            End While
            indicesToCompute = pool.ToList()
        Else
            For i = 0 To model.Count - 1
                indicesToCompute.Add(i)
            Next
        End If

        ' 对每个高斯，对每个参数计算中心差分
        For Each idx In indicesToCompute
            ' 位置梯度（3 个参数）
            For d = 0 To 2
                Dim orig = model.Positions(idx, d)
                model.Positions(idx, d) = orig + Epsilon
                Dim lossPlus = ComputeTotalLoss(model, cameras, targets)
                model.Positions(idx, d) = orig - Epsilon
                Dim lossMinus = ComputeTotalLoss(model, cameras, targets)
                model.Positions(idx, d) = orig
                model.PositionsGrad(idx, d) = (lossPlus - lossMinus) / (2 * Epsilon)
            Next

            ' 颜色梯度（3 个参数）
            For d = 0 To 2
                Dim orig = model.Colors(idx, d)
                model.Colors(idx, d) = orig + Epsilon
                Dim lossPlus = ComputeTotalLoss(model, cameras, targets)
                model.Colors(idx, d) = orig - Epsilon
                Dim lossMinus = ComputeTotalLoss(model, cameras, targets)
                model.Colors(idx, d) = orig
                model.ColorsGrad(idx, d) = (lossPlus - lossMinus) / (2 * Epsilon)
            Next

            ' 缩放梯度（3 个参数，使用较大步长）
            Dim scaleEps = std.Max(Epsilon, 0.05)
            For d = 0 To 2
                Dim orig = model.Scales(idx, d)
                model.Scales(idx, d) = orig + scaleEps
                Dim lossPlus = ComputeTotalLoss(model, cameras, targets)
                model.Scales(idx, d) = orig - scaleEps
                Dim lossMinus = ComputeTotalLoss(model, cameras, targets)
                model.Scales(idx, d) = orig
                model.ScalesGrad(idx, d) = (lossPlus - lossMinus) / (2 * scaleEps)
            Next

            ' 不透明度梯度（1 个参数）
            Dim opEps = std.Max(Epsilon, 0.1)
            Dim origOp = model.Opacities(idx, 0)
            model.Opacities(idx, 0) = origOp + opEps
            Dim lossPlusOp = ComputeTotalLoss(model, cameras, targets)
            model.Opacities(idx, 0) = origOp - opEps
            Dim lossMinusOp = ComputeTotalLoss(model, cameras, targets)
            model.Opacities(idx, 0) = origOp
            model.OpacitiesGrad(idx, 0) = (lossPlusOp - lossMinusOp) / (2 * opEps)
        Next
    End Sub

    ''' <summary>
    ''' 计算模型在所有视图上的总损失
    ''' </summary>
    Private Function ComputeTotalLoss(model As GaussianModel, cameras As List(Of Camera), targets As List(Of Tensor)) As Double
        Dim totalLoss = 0.0
        For v = 0 To cameras.Count - 1
            Dim rendered = SplatRenderer.Render(model, cameras(v))
            totalLoss += SplatRenderer.ComputeMSE(rendered, targets(v))
        Next
        Return totalLoss / cameras.Count
    End Function

End Class


''' <summary>
''' 自适应密度控制
''' 
''' Adaptive Density Control
''' ========================
''' 3DGS 的关键创新之一：动态调整高斯数量
'''   - **克隆 (Clone)**: 对梯度大且尺寸小的高斯进行复制
'''   - **分裂 (Split)**: 对梯度大且尺寸大的高斯进行分裂
'''   - **剪枝 (Prune)**: 删除不透明度过低或尺寸过大的高斯
''' </summary>
Public Class DensityController

    ''' <summary>位置梯度阈值（超过此值认为高斯需要 densify）</summary>
    Public Property GradThreshold As Double = 0.0002

    ''' <summary>最小不透明度（低于此值剪枝）</summary>
    Public Property MinOpacity As Double = 0.005

    ''' <summary>最大缩放（超过此值剪枝，防止高斯过大）</summary>
    Public Property MaxScale As Double = 0.5

    ''' <summary>每隔多少步执行一次密度控制</summary>
    Public Property DensifyInterval As Integer = 100

    Public Sub New(Optional gradThreshold As Double = 0.0002)
        Me.GradThreshold = gradThreshold
    End Sub

    ''' <summary>
    ''' 执行自适应密度控制
    ''' </summary>
    ''' <param name="model">高斯模型（会被修改）</param>
    ''' <param name="step">当前训练步数</param>
    ''' <returns>操作统计信息</returns>
    Public Function Densify(model As GaussianModel, [step] As Integer) As (Cloned As Integer, Split As Integer, Pruned As Integer)
        If [step] Mod DensifyInterval <> 0 OrElse [step] = 0 Then
            Return (0, 0, 0)
        End If

        Dim cloned = 0, split = 0, pruned = 0

        ' 1. 剪枝：删除不透明度过低或尺寸过大的高斯
        Dim toPrune As New HashSet(Of Integer)()
        For i = 0 To model.Count - 1
            Dim op = model.GetOpacity(i)
            Dim sx = std.Exp(model.Scales(i, 0))
            Dim sy = std.Exp(model.Scales(i, 1))
            Dim sz = std.Exp(model.Scales(i, 2))
            Dim maxS = std.Max(sx, std.Max(sy, sz))
            If op < MinOpacity OrElse maxS > MaxScale Then
                toPrune.Add(i)
            End If
        Next
        pruned = toPrune.Count
        If toPrune.Count > 0 Then
            model.RemoveGaussians(toPrune)
        End If

        ' 2. Densify：对位置梯度大的高斯进行克隆或分裂
        Dim toClone As New List(Of Integer)()
        Dim toSplit As New List(Of Integer)()
        For i = 0 To model.Count - 1
            ' 计算位置梯度的范数
            Dim gx = model.PositionsGrad(i, 0)
            Dim gy = model.PositionsGrad(i, 1)
            Dim gz = model.PositionsGrad(i, 2)
            Dim gradNorm = std.Sqrt(gx * gx + gy * gy + gz * gz)
            If gradNorm > GradThreshold Then
                Dim sx = std.Exp(model.Scales(i, 0))
                Dim sy = std.Exp(model.Scales(i, 1))
                Dim sz = std.Exp(model.Scales(i, 2))
                Dim maxS = std.Max(sx, std.Max(sy, sz))
                If maxS > 0.01 Then
                    ' 大高斯 -> 分裂
                    toSplit.Add(i)
                Else
                    ' 小高斯 -> 克隆
                    toClone.Add(i)
                End If
            End If
        Next

        ' 执行克隆
        If toClone.Count > 0 Then
            Dim newPositions = New Tensor(toClone.Count, 3)
            Dim newScales = New Tensor(toClone.Count, 3)
            Dim newRotations = New Tensor(toClone.Count, 4)
            Dim newColors = New Tensor(toClone.Count, 3)
            Dim newOpacities = New Tensor(toClone.Count, 1)
            For k = 0 To toClone.Count - 1
                Dim srcIdx = toClone(k)
                For d = 0 To 2
                    newPositions(k, d) = model.Positions(srcIdx, d)
                    newScales(k, d) = model.Scales(srcIdx, d)
                    newColors(k, d) = model.Colors(srcIdx, d)
                Next
                For d = 0 To 3
                    newRotations(k, d) = model.Rotations(srcIdx, d)
                Next
                newOpacities(k, 0) = model.Opacities(srcIdx, 0)
            Next
            model.AddGaussians(newPositions, newScales, newRotations, newColors, newOpacities)
            cloned = toClone.Count
        End If

        ' 执行分裂（每个高斯分裂成 2 个，尺寸减半）
        If toSplit.Count > 0 Then
            Dim newPositions = New Tensor(toSplit.Count * 2, 3)
            Dim newScales = New Tensor(toSplit.Count * 2, 3)
            Dim newRotations = New Tensor(toSplit.Count * 2, 4)
            Dim newColors = New Tensor(toSplit.Count * 2, 3)
            Dim newOpacities = New Tensor(toSplit.Count * 2, 1)
            For k = 0 To toSplit.Count - 1
                Dim srcIdx = toSplit(k)
                ' 两个子高斯：位置随机偏移，尺寸减半
                Dim rng As New Random(srcIdx + 1)
                For child = 0 To 1
                    Dim offset = New Double() {rng.NextDouble() - 0.5, rng.NextDouble() - 0.5, rng.NextDouble() - 0.5}
                    For d = 0 To 2
                        newPositions(k * 2 + child, d) = model.Positions(srcIdx, d) + offset(d) * 0.1
                        newScales(k * 2 + child, d) = model.Scales(srcIdx, d) - std.Log(2)  ' 尺寸减半
                        newColors(k * 2 + child, d) = model.Colors(srcIdx, d)
                    Next
                    For d = 0 To 3
                        newRotations(k * 2 + child, d) = model.Rotations(srcIdx, d)
                    Next
                    newOpacities(k * 2 + child, 0) = model.Opacities(srcIdx, 0)
                Next
            Next
            model.AddGaussians(newPositions, newScales, newRotations, newColors, newOpacities)
            split = toSplit.Count * 2
        End If

        Return (cloned, split, pruned)
    End Function

End Class

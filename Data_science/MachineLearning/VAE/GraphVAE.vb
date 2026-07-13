#Region "Microsoft.VisualBasic::125f5c8591ea4b5e2075110f0bb19263, Data_science\MachineLearning\VAE\GraphVAE.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 931
    '    Code Lines: 501 (53.81%)
    ' Comment Lines: 251 (26.96%)
    '    - Xml Docs: 46.22%
    ' 
    '   Blank Lines: 179 (19.23%)
    '     File Size: 31.52 KB


    ' Module GraphUtils
    ' 
    '     Function: AddBias, ApplyReLU, ApplySigmoid, BCELossSum, BinaryAccuracy
    '               BroadcastRow, KLDivergence, NormalizeAdjacency, ReLUDerivFromInput, Sigmoid
    '               Threshold
    ' 
    ' Class LinearLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backward, Forward
    ' 
    '     Sub: Update
    ' 
    ' Class GCNLayer
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Backward, Forward
    ' 
    '     Sub: Update
    ' 
    ' Structure GraphVAEConfig
    ' 
    ' 
    ' 
    ' Class GraphVAE
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ComputeLoss, Evaluate, Forward, Generate, Predict
    '               SampleEpsilon, Train
    ' 
    '     Sub: Backward, Update
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' GraphVAE: 图变分自编码器 (Graph Variational Autoencoder)
' ============================================================================
' 基于 Simonovsky & Komodakis (2018) 和 Kipf & Welling (2016) 的图变分自编码器
' 输入一个图（邻接矩阵 + 节点特征），编码到潜在空间，再解码重构出图
'
' 架构:
'   编码器: GCN层 -> 图读出 -> 线性层(μ, logσ²)
'   重参数化: z = μ + σ * ε
'   解码器: 线性层 -> 节点嵌入 -> 内积(邻接) + 线性层(特征)
'
' 使用手动反向传播和Adam优化器进行训练
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

' ============================================================================
' 辅助函数模块
' ============================================================================
Public Module GraphUtils

#Region "图操作"

    ''' <summary>
    ''' 归一化邻接矩阵: Â = D^(-1/2) (A + I) D^(-1/2)
    ''' 这是GCN中标准的邻接矩阵归一化方法
    ''' </summary>
    Public Function NormalizeAdjacency(adj As Tensor) As Tensor
        Dim n As Integer = adj.Shape(0)

        ' A + I (添加自环)
        Dim adjSelfLoops As New Tensor(n, n)
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                adjSelfLoops(i, j) = adj(i, j)
            Next
            adjSelfLoops(i, i) += 1.0
        Next

        ' 计算度矩阵 D
        Dim degree(n - 1) As Double
        For i As Integer = 0 To n - 1
            Dim sum As Double = 0
            For j As Integer = 0 To n - 1
                sum += adjSelfLoops(i, j)
            Next
            degree(i) = sum
        Next

        ' D^(-1/2)
        Dim invSqrtDegree(n - 1) As Double
        For i As Integer = 0 To n - 1
            If degree(i) > 0 Then
                invSqrtDegree(i) = 1.0 / std.Sqrt(degree(i))
            Else
                invSqrtDegree(i) = 0.0
            End If
        Next

        ' Â = D^(-1/2) (A + I) D^(-1/2)
        Dim normAdj As New Tensor(n, n)
        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                normAdj(i, j) = adjSelfLoops(i, j) * invSqrtDegree(i) * invSqrtDegree(j)
            Next
        Next

        Return normAdj
    End Function

#End Region

#Region "激活函数"

    ''' <summary>
    ''' Sigmoid激活函数（标量版本）
    ''' </summary>
    Public Function Sigmoid(x As Double) As Double
        If x >= 0 Then
            Return 1.0 / (1.0 + std.Exp(-x))
        Else
            Dim e As Double = std.Exp(x)
            Return e / (1.0 + e)
        End If
    End Function

    ''' <summary>
    ''' 对张量逐元素应用Sigmoid
    ''' </summary>
    Public Function ApplySigmoid(t As Tensor) As Tensor
        Return t.Apply(Function(x) Sigmoid(x))
    End Function

    ''' <summary>
    ''' 对张量逐元素应用ReLU
    ''' </summary>
    Public Function ApplyReLU(t As Tensor) As Tensor
        Return t.Apply(Function(x) If(x > 0, x, 0.0))
    End Function

    ''' <summary>
    ''' ReLU的导数（给定预激活值和输出梯度）
    ''' </summary>
    Public Function ReLUDerivFromInput(preActivation As Tensor, gradOutput As Tensor) As Tensor
        Dim result As New Tensor(preActivation.Shape)
        For i As Integer = 0 To preActivation.Length - 1
            result.Data(i) = If(preActivation.Data(i) > 0, gradOutput.Data(i), 0.0)
        Next
        Return result
    End Function

#End Region

#Region "损失函数"

    ''' <summary>
    ''' 二元交叉熵损失（求和）
    ''' L = -sum(y * log(p) + (1-y) * log(1-p))
    ''' </summary>
    Public Function BCELossSum(pred As Tensor, target As Tensor) As Double
        Dim loss As Double = 0
        For i As Integer = 0 To pred.Length - 1
            Dim p As Double = pred.Data(i)
            ' 裁剪以避免log(0)
            p = std.Max(0.0000000001, std.Min(1.0 - 0.0000000001, p))
            Dim y As Double = target.Data(i)
            loss += -(y * std.Log(p) + (1.0 - y) * std.Log(1.0 - p))
        Next
        Return loss
    End Function

    ''' <summary>
    ''' KL散度: KL(N(μ,σ²) || N(0,1))
    ''' = -0.5 * sum(1 + logσ² - μ² - exp(logσ²))
    ''' </summary>
    Public Function KLDivergence(mu As Tensor, logVar As Tensor) As Double
        Dim kl As Double = 0
        For i As Integer = 0 To mu.Length - 1
            Dim m As Double = mu.Data(i)
            Dim lv As Double = logVar.Data(i)
            kl += -0.5 * (1.0 + lv - m * m - std.Exp(lv))
        Next
        Return kl
    End Function

#End Region

#Region "张量辅助函数"

    ''' <summary>
    ''' 将偏置向量(1, n)加到矩阵(m, n)的每一行
    ''' </summary>
    Public Function AddBias(mat As Tensor, bias As Tensor) As Tensor
        Dim m As Integer = mat.Shape(0)
        Dim n As Integer = mat.Shape(1)
        Dim result As New Tensor(m, n)
        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                result(i, j) = mat(i, j) + bias(0, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 将(1, n)张量广播到(m, n)
    ''' </summary>
    Public Function BroadcastRow(t As Tensor, m As Integer) As Tensor
        Dim n As Integer = t.Shape(1)
        Dim result As New Tensor(m, n)
        For i As Integer = 0 To m - 1
            For j As Integer = 0 To n - 1
                result(i, j) = t(0, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 将概率矩阵阈值化为二值矩阵
    ''' </summary>
    Public Function Threshold(t As Tensor, thresholdVal As Double) As Tensor
        Return t.Apply(Function(x) If(x >= thresholdVal, 1.0, 0.0))
    End Function

    ''' <summary>
    ''' 计算二值预测的准确率
    ''' </summary>
    Public Function BinaryAccuracy(pred As Tensor, target As Tensor, thresholdVal As Double) As Double
        Dim correct As Integer = 0
        Dim total As Integer = pred.Length
        For i As Integer = 0 To total - 1
            Dim p As Double = If(pred.Data(i) >= thresholdVal, 1.0, 0.0)
            If p = target.Data(i) Then
                correct += 1
            End If
        Next
        Return CDbl(correct) / CDbl(total)
    End Function

#End Region

End Module

' ============================================================================
' 线性层: y = x @ W + b
' ============================================================================
Public Class LinearLayer

#Region "属性"

    ''' <summary>权重矩阵 (inDim, outDim)</summary>
    Public Weights As Tensor

    ''' <summary>偏置向量 (1, outDim)</summary>
    Public Bias As Tensor

    ''' <summary>权重梯度</summary>
    Public WeightGrad As Tensor

    ''' <summary>偏置梯度</summary>
    Public BiasGrad As Tensor

    ' 前向传播缓存的输入
    Private _cachedInput As Tensor

    ' Adam优化器状态
    Private _mW As Tensor
    Private _vW As Tensor
    Private _mb As Tensor
    Private _vb As Tensor
    Private _adamT As Integer = 0

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建线性层
    ''' </summary>
    Public Sub New(inDim As Integer, outDim As Integer, Optional seed As Integer? = Nothing)
        ' Xavier初始化权重
        Weights = Tensor.XavierInit(inDim, outDim, seed)
        ' 偏置初始化为零
        Bias = Tensor.Zeros(New Integer() {1, outDim})
        ' 初始化梯度
        WeightGrad = Tensor.Zeros(New Integer() {inDim, outDim})
        BiasGrad = Tensor.Zeros(New Integer() {1, outDim})
    End Sub

#End Region

#Region "前向/反向传播"

    ''' <summary>
    ''' 前向传播: output = input @ W + b
    ''' </summary>
    Public Function Forward(input As Tensor) As Tensor
        _cachedInput = input
        ' input @ W: (m, inDim) @ (inDim, outDim) = (m, outDim)
        Dim output As Tensor = input.MatMul(Weights)
        ' 加偏置
        output = GraphUtils.AddBias(output, Bias)
        Return output
    End Function

    ''' <summary>
    ''' 反向传播
    ''' gradOutput: (m, outDim)
    ''' 返回: gradInput (m, inDim)
    ''' </summary>
    Public Function Backward(gradOutput As Tensor) As Tensor
        ' WeightGrad = input^T @ gradOutput: (inDim, outDim)
        WeightGrad = _cachedInput.Transpose().MatMul(gradOutput)

        ' BiasGrad = sum(gradOutput, axis=0): (1, outDim)
        BiasGrad = gradOutput.Sum(0)

        ' gradInput = gradOutput @ W^T: (m, inDim)
        Dim gradInput As Tensor = gradOutput.MatMul(Weights.Transpose())
        Return gradInput
    End Function

#End Region

#Region "Adam优化器更新"

    ''' <summary>
    ''' 使用Adam优化器更新参数
    ''' </summary>
    Public Sub Update(lr As Single, beta1 As Single, beta2 As Single, eps As Single)
        _adamT += 1
        Dim t As Integer = _adamT
        Dim biasCorrection1 As Double = 1.0 - std.Pow(CDbl(beta1), t)
        Dim biasCorrection2 As Double = 1.0 - std.Pow(CDbl(beta2), t)

        ' 初始化Adam状态
        If _mW Is Nothing Then
            _mW = Tensor.Zeros(Weights.Shape)
            _vW = Tensor.Zeros(Weights.Shape)
            _mb = Tensor.Zeros(Bias.Shape)
            _vb = Tensor.Zeros(Bias.Shape)
        End If

        ' 更新权重
        For i As Integer = 0 To Weights.Length - 1
            Dim g As Double = WeightGrad.Data(i)
            _mW.Data(i) = beta1 * _mW.Data(i) + (1.0 - beta1) * g
            _vW.Data(i) = beta2 * _vW.Data(i) + (1.0 - beta2) * g * g
            Dim mHat As Double = _mW.Data(i) / biasCorrection1
            Dim vHat As Double = _vW.Data(i) / biasCorrection2
            Weights.Data(i) -= lr * mHat / (std.Sqrt(vHat) + eps)
        Next

        ' 更新偏置
        For i As Integer = 0 To Bias.Length - 1
            Dim g As Double = BiasGrad.Data(i)
            _mb.Data(i) = beta1 * _mb.Data(i) + (1.0 - beta1) * g
            _vb.Data(i) = beta2 * _vb.Data(i) + (1.0 - beta2) * g * g
            Dim mHat As Double = _mb.Data(i) / biasCorrection1
            Dim vHat As Double = _vb.Data(i) / biasCorrection2
            Bias.Data(i) -= lr * mHat / (std.Sqrt(vHat) + eps)
        Next
    End Sub

#End Region

End Class

' ============================================================================
' GCN层: H = ReLU(Â @ X @ W)
' ============================================================================
Public Class GCNLayer

#Region "属性"

    ''' <summary>权重矩阵 (inDim, outDim)</summary>
    Public Weights As Tensor

    ''' <summary>权重梯度</summary>
    Public WeightGrad As Tensor

    ' 前向传播缓存
    Private _cachedNormAdj As Tensor
    Private _cachedInput As Tensor
    Private _cachedAX As Tensor    ' Â @ X
    Private _cachedZ As Tensor     ' 预激活值 Â @ X @ W

    ' Adam优化器状态
    Private _mW As Tensor
    Private _vW As Tensor
    Private _adamT As Integer = 0

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建GCN层
    ''' </summary>
    Public Sub New(inDim As Integer, outDim As Integer, Optional seed As Integer? = Nothing)
        ' He初始化（适用于ReLU激活）
        Weights = Tensor.HeInit(inDim, outDim, seed)
        WeightGrad = Tensor.Zeros(New Integer() {inDim, outDim})
    End Sub

#End Region

#Region "前向/反向传播"

    ''' <summary>
    ''' 前向传播: H = ReLU(Â @ X @ W)
    ''' </summary>
    Public Function Forward(normAdj As Tensor, input As Tensor) As Tensor
        _cachedNormAdj = normAdj
        _cachedInput = input

        ' AX = Â @ X: (N, inDim)
        _cachedAX = normAdj.MatMul(input)

        ' Z = AX @ W: (N, outDim)
        _cachedZ = _cachedAX.MatMul(Weights)

        ' H = ReLU(Z)
        Dim H As Tensor = GraphUtils.ApplyReLU(_cachedZ)
        Return H
    End Function

    ''' <summary>
    ''' 反向传播
    ''' gradOutput: (N, outDim) = dH
    ''' 返回: gradInput (N, inDim) = dX
    ''' </summary>
    Public Function Backward(gradOutput As Tensor) As Tensor
        ' dZ = dH * ReLU'(Z)
        Dim dZ As Tensor = GraphUtils.ReLUDerivFromInput(_cachedZ, gradOutput)

        ' WeightGrad = (Â @ X)^T @ dZ = AX^T @ dZ: (inDim, outDim)
        WeightGrad = _cachedAX.Transpose().MatMul(dZ)

        ' dX = Â^T @ dZ @ W^T = Â @ dZ @ W^T (因为Â是对称的): (N, inDim)
        Dim gradInput As Tensor = _cachedNormAdj.MatMul(dZ.MatMul(Weights.Transpose()))
        Return gradInput
    End Function

#End Region

#Region "Adam优化器更新"

    ''' <summary>
    ''' 使用Adam优化器更新参数
    ''' </summary>
    Public Sub Update(lr As Single, beta1 As Single, beta2 As Single, eps As Single)
        _adamT += 1
        Dim t As Integer = _adamT
        Dim biasCorrection1 As Double = 1.0 - std.Pow(CDbl(beta1), t)
        Dim biasCorrection2 As Double = 1.0 - std.Pow(CDbl(beta2), t)

        ' 初始化Adam状态
        If _mW Is Nothing Then
            _mW = Tensor.Zeros(Weights.Shape)
            _vW = Tensor.Zeros(Weights.Shape)
        End If

        ' 更新权重
        For i As Integer = 0 To Weights.Length - 1
            Dim g As Double = WeightGrad.Data(i)
            _mW.Data(i) = beta1 * _mW.Data(i) + (1.0 - beta1) * g
            _vW.Data(i) = beta2 * _vW.Data(i) + (1.0 - beta2) * g * g
            Dim mHat As Double = _mW.Data(i) / biasCorrection1
            Dim vHat As Double = _vW.Data(i) / biasCorrection2
            Weights.Data(i) -= lr * mHat / (std.Sqrt(vHat) + eps)
        Next
    End Sub

#End Region

End Class

' ============================================================================
' GraphVAE配置结构
' ============================================================================
Public Structure GraphVAEConfig
    ''' <summary>节点数量</summary>
    Public NumNodes As Integer
    ''' <summary>输入特征维度</summary>
    Public InputDim As Integer
    ''' <summary>GCN第一层隐藏维度</summary>
    Public Hidden1 As Integer
    ''' <summary>GCN第二层隐藏维度（图嵌入维度）</summary>
    Public Hidden2 As Integer
    ''' <summary>潜在空间维度</summary>
    Public LatentDim As Integer
    ''' <summary>解码器隐藏维度（节点嵌入维度）</summary>
    Public DecHidden As Integer
End Structure

' ============================================================================
' GraphVAE: 图变分自编码器
' ============================================================================
Public Class GraphVAE

#Region "属性"

    ' 模型配置
    Private _config As GraphVAEConfig

    ' 编码器层
    Private _gcn1 As GCNLayer          ' GCN层1: (InputDim -> Hidden1)
    Private _gcn2 As GCNLayer          ' GCN层2: (Hidden1 -> Hidden2)
    Private _encMu As LinearLayer      ' μ投影: (Hidden2 -> LatentDim)
    Private _encLogVar As LinearLayer  ' logσ²投影: (Hidden2 -> LatentDim)

    ' 解码器层
    Private _decEmbed As LinearLayer   ' 嵌入生成: (LatentDim -> NumNodes * DecHidden)
    Private _decFeat As LinearLayer    ' 特征解码: (DecHidden -> InputDim)

    ' 前向传播缓存的中间值
    Private _adj As Tensor
    Private _features As Tensor
    Private _normAdj As Tensor
    Private _H1 As Tensor
    Private _H2 As Tensor
    Private _g As Tensor               ' 图嵌入
    Private _mu As Tensor
    Private _logVar As Tensor
    Private _epsilon As Tensor
    Private _std As Tensor             ' σ = exp(0.5 * logσ²)
    Private _z As Tensor               ' 潜在向量
    Private _h As Tensor               ' 解码器隐藏输出
    Private _E As Tensor               ' 节点嵌入（预激活）
    Private _E_act As Tensor           ' 节点嵌入（ReLU后）
    Private _S_adj As Tensor           ' 邻接矩阵预激活
    Private _reconAdj As Tensor        ' 重构的邻接矩阵
    Private _S_feat As Tensor          ' 特征预激活
    Private _reconFeat As Tensor       ' 重构的特征

    ' 随机数生成器（用于可复现的epsilon采样）
    Private _rand As Random
    Private _seed As Integer?

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建GraphVAE模型
    ''' </summary>
    Public Sub New(config As GraphVAEConfig, Optional seed As Integer? = Nothing)
        _config = config
        _seed = seed

        If seed.HasValue Then
            _rand = New Random(seed.Value)
        Else
            _rand = New Random()
        End If

        ' 初始化编码器
        Dim s1 As Integer? = If(seed.HasValue, seed.Value + 1, Nothing)
        Dim s2 As Integer? = If(seed.HasValue, seed.Value + 2, Nothing)
        Dim s3 As Integer? = If(seed.HasValue, seed.Value + 3, Nothing)
        Dim s4 As Integer? = If(seed.HasValue, seed.Value + 4, Nothing)
        Dim s5 As Integer? = If(seed.HasValue, seed.Value + 5, Nothing)
        Dim s6 As Integer? = If(seed.HasValue, seed.Value + 6, Nothing)

        _gcn1 = New GCNLayer(config.InputDim, config.Hidden1, s1)
        _gcn2 = New GCNLayer(config.Hidden1, config.Hidden2, s2)
        _encMu = New LinearLayer(config.Hidden2, config.LatentDim, s3)
        _encLogVar = New LinearLayer(config.Hidden2, config.LatentDim, s4)

        ' 初始化解码器
        _decEmbed = New LinearLayer(config.LatentDim, config.NumNodes * config.DecHidden, s5)
        _decFeat = New LinearLayer(config.DecHidden, config.InputDim, s6)
    End Sub

#End Region

#Region "前向传播"

    ''' <summary>
    ''' 完整的前向传播
    ''' 输入: adj (N,N), features (N,F)
    ''' 输出: (重构邻接, 重构特征, μ, logσ², z)
    ''' </summary>
    Public Function Forward(adj As Tensor, features As Tensor) As Tuple(Of Tensor, Tensor, Tensor, Tensor, Tensor)
        Dim N As Integer = _config.NumNodes

        ' 缓存输入
        _adj = adj
        _features = features

        ' 归一化邻接矩阵
        _normAdj = GraphUtils.NormalizeAdjacency(adj)

        ' === 编码器 ===
        ' GCN层1: H1 = ReLU(Â @ X @ W1)
        _H1 = _gcn1.Forward(_normAdj, features)

        ' GCN层2: H2 = ReLU(Â @ H1 @ W2)
        _H2 = _gcn2.Forward(_normAdj, _H1)

        ' 图读出: g = mean(H2, axis=0) -> (1, Hidden2)
        _g = _H2.Mean(0)

        ' μ = g @ Wμ + bμ
        _mu = _encMu.Forward(_g)

        ' logσ² = g @ Wlv + blv
        _logVar = _encLogVar.Forward(_g)

        ' === 重参数化 ===
        ' z = μ + σ * ε, 其中 σ = exp(0.5 * logσ²), ε ~ N(0, I)
        _epsilon = SampleEpsilon(_config.LatentDim)
        Dim halfLogVar As Tensor = _logVar.Apply(Function(x) 0.5 * x)
        _std = halfLogVar.Apply(Function(x) std.Exp(x))
        ' z = μ + σ * ε
        _z = _mu + _std.ElementwiseMultiply(_epsilon)

        ' === 解码器 ===
        ' h = z @ We + be -> (1, N * DecHidden)
        _h = _decEmbed.Forward(_z)

        ' 重塑为节点嵌入 E (N, DecHidden)
        _E = New Tensor(_h.ToDoubleArray(), N, _config.DecHidden)

        ' E_act = ReLU(E)
        _E_act = GraphUtils.ApplyReLU(_E)

        ' 邻接矩阵: A' = sigmoid(E_act @ E_act^T)
        _S_adj = _E_act.MatMul(_E_act.Transpose())
        _reconAdj = GraphUtils.ApplySigmoid(_S_adj)

        ' 节点特征: X' = sigmoid(E_act @ Wf + bf)
        _S_feat = _decFeat.Forward(_E_act)
        _reconFeat = GraphUtils.ApplySigmoid(_S_feat)

        Return Tuple.Create(_reconAdj, _reconFeat, _mu, _logVar, _z)
    End Function

    ''' <summary>
    ''' 从标准正态分布采样epsilon
    ''' </summary>
    Private Function SampleEpsilon([dim] As Integer) As Tensor
        Dim eps As New Tensor(1, [dim])
        For i As Integer = 0 To [dim] - 1
            ' Box-Muller变换
            Dim u1 As Double = 1.0 - _rand.NextDouble()
            Dim u2 As Double = 1.0 - _rand.NextDouble()
            Dim randStdNormal As Double = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
            eps.Data(i) = randStdNormal
        Next
        Return eps
    End Function

#End Region

#Region "损失计算"

    ''' <summary>
    ''' 计算总损失 = 重构损失 + β * KL散度
    ''' </summary>
    Public Function ComputeLoss(reconAdj As Tensor, adj As Tensor,
                                reconFeat As Tensor, features As Tensor,
                                mu As Tensor, logVar As Tensor,
                                beta As Single) As Double
        Dim N As Integer = _config.NumNodes
        Dim F As Integer = _config.InputDim
        Dim L As Integer = _config.LatentDim

        ' 重构损失（均值BCE）
        Dim adjLoss As Double = GraphUtils.BCELossSum(reconAdj, adj) / (N * N)
        Dim featLoss As Double = GraphUtils.BCELossSum(reconFeat, features) / (N * F)

        ' KL散度（均值）
        Dim klLoss As Double = GraphUtils.KLDivergence(mu, logVar) / L

        ' 总损失
        Dim totalLoss As Double = adjLoss + featLoss + beta * klLoss
        Return totalLoss
    End Function

#End Region

#Region "反向传播"

    ''' <summary>
    ''' 完整的反向传播
    ''' 计算所有参数的梯度并存储在各层的WeightGrad/BiasGrad中
    ''' </summary>
    Public Sub Backward(adj As Tensor, features As Tensor,
                        reconAdj As Tensor, reconFeat As Tensor,
                        mu As Tensor, logVar As Tensor, z As Tensor,
                        beta As Single)
        Dim N As Integer = _config.NumNodes
        Dim F As Integer = _config.InputDim
        Dim L As Integer = _config.LatentDim
        Dim H3 As Integer = _config.DecHidden

        ' === 解码器反向传播 ===

        ' 邻接矩阵BCE梯度（通过sigmoid）
        ' dS_adj = (A' - A) / (N * N)
        Dim dS_adj As New Tensor(N, N)
        Dim adjScale As Double = 1.0 / (N * N)
        For i As Integer = 0 To N * N - 1
            dS_adj.Data(i) = (reconAdj.Data(i) - adj.Data(i)) * adjScale
        Next

        ' 通过 S_adj = E_act @ E_act^T
        ' dE_act_adj = (dS_adj + dS_adj^T) @ E_act
        Dim dS_adj_T As Tensor = dS_adj.Transpose()
        Dim dS_adj_sum As Tensor = dS_adj + dS_adj_T
        Dim dE_act_adj As Tensor = dS_adj_sum.MatMul(_E_act)  ' (N, H3)

        ' 特征BCE梯度（通过sigmoid）
        ' dS_feat = (X' - X) / (N * F)
        Dim dS_feat As New Tensor(N, F)
        Dim featScale As Double = 1.0 / (N * F)
        For i As Integer = 0 To N * F - 1
            dS_feat.Data(i) = (reconFeat.Data(i) - features.Data(i)) * featScale
        Next

        ' 通过 S_feat = E_act @ Wf + bf (decFeat层)
        ' 返回 dE_act_feat (N, H3)
        Dim dE_act_feat As Tensor = _decFeat.Backward(dS_feat)

        ' 总 dE_act
        Dim dE_act As Tensor = dE_act_adj + dE_act_feat

        ' 通过 ReLU
        Dim dE As Tensor = GraphUtils.ReLUDerivFromInput(_E, dE_act)

        ' 重塑 dE (N, H3) -> dh (1, N*H3)
        Dim dh As New Tensor(dE.ToDoubleArray(), 1, N * H3)

        ' 通过 h = z @ We + be (decEmbed层)
        ' 返回 dz_recon (1, L)
        Dim dz_recon As Tensor = _decEmbed.Backward(dh)

        ' === 重参数化反向传播 ===
        ' z = μ + σ * ε
        ' dz/dμ = 1
        ' dz/dlogσ² = 0.5 * σ * ε = 0.5 * (z - μ)
        Dim dMu_recon As Tensor = dz_recon
        Dim dLogVar_recon As New Tensor(1, L)
        For i As Integer = 0 To L - 1
            dLogVar_recon.Data(i) = dz_recon.Data(i) * 0.5 * (z.Data(i) - mu.Data(i))
        Next

        ' === KL散度梯度 ===
        ' L_KL = -0.5 * mean(1 + logσ² - μ² - exp(logσ²))
        ' dL_KL/dμ = μ / L
        ' dL_KL/dlogσ² = 0.5 * (exp(logσ²) - 1) / L
        Dim klScale As Double = 1.0 / L
        Dim dMu_KL As New Tensor(1, L)
        Dim dLogVar_KL As New Tensor(1, L)
        For i As Integer = 0 To L - 1
            dMu_KL.Data(i) = mu.Data(i) * klScale
            dLogVar_KL.Data(i) = 0.5 * (std.Exp(logVar.Data(i)) - 1.0) * klScale
        Next

        ' 总梯度（KL乘以beta）
        Dim dMu As Tensor = dMu_recon + (dMu_KL * beta)
        Dim dLogVar As Tensor = dLogVar_recon + (dLogVar_KL * beta)

        ' === 编码器线性层反向传播 ===
        ' 通过 μ = g @ Wμ + bμ
        Dim dg_mu As Tensor = _encMu.Backward(dMu)  ' (1, Hidden2)

        ' 通过 logσ² = g @ Wlv + blv
        Dim dg_lv As Tensor = _encLogVar.Backward(dLogVar)  ' (1, Hidden2)

        ' 总 dg
        Dim dg As Tensor = dg_mu + dg_lv

        ' === 图读出反向传播 ===
        ' g = mean(H2, axis=0) = (1/N) * sum(H2, axis=0)
        ' dH2 = broadcast(dg / N, (N, Hidden2))
        Dim dH2 As Tensor = GraphUtils.BroadcastRow(dg, N)
        Dim readoutScale As Single = 1.0F / CSng(N)
        dH2 = dH2 * readoutScale

        ' === GCN层反向传播 ===
        ' GCN层2: 返回 dH1
        Dim dH1 As Tensor = _gcn2.Backward(dH2)

        ' GCN层1: 返回 dX（不需要使用，但需要计算权重梯度）
        _gcn1.Backward(dH1)
    End Sub

#End Region

#Region "参数更新"

    ''' <summary>
    ''' 使用Adam优化器更新所有参数
    ''' </summary>
    Public Sub Update(lr As Single, Optional beta1 As Single = 0.9F, Optional beta2 As Single = 0.999F, Optional eps As Single = 0.00000001F)
        _gcn1.Update(lr, beta1, beta2, eps)
        _gcn2.Update(lr, beta1, beta2, eps)
        _encMu.Update(lr, beta1, beta2, eps)
        _encLogVar.Update(lr, beta1, beta2, eps)
        _decEmbed.Update(lr, beta1, beta2, eps)
        _decFeat.Update(lr, beta1, beta2, eps)
    End Sub

#End Region

#Region "训练"

    ''' <summary>
    ''' 训练模型
    ''' </summary>
    ''' <param name="graphs">图列表，每个元素是(邻接矩阵, 特征矩阵)的元组</param>
    ''' <param name="epochs">训练轮数</param>
    ''' <param name="lr">学习率</param>
    ''' <param name="beta">KL散度权重</param>
    ''' <param name="verbose">是否打印训练信息</param>
    ''' <returns>每个epoch的平均损失列表</returns>
    Public Function Train(graphs As List(Of Tuple(Of Tensor, Tensor)),
                          epochs As Integer,
                          lr As Single,
                          Optional beta As Single = 1.0F,
                          Optional verbose As Boolean = True) As List(Of Double)
        Dim losses As New List(Of Double)

        For epoch As Integer = 0 To epochs - 1
            Dim epochLoss As Double = 0

            For Each graph As Tuple(Of Tensor, Tensor) In graphs
                Dim adj As Tensor = graph.Item1
                Dim features As Tensor = graph.Item2

                ' 前向传播
                Dim result As Tuple(Of Tensor, Tensor, Tensor, Tensor, Tensor) = Forward(adj, features)
                Dim reconAdj As Tensor = result.Item1
                Dim reconFeat As Tensor = result.Item2
                Dim mu As Tensor = result.Item3
                Dim logVar As Tensor = result.Item4
                Dim z As Tensor = result.Item5

                ' 计算损失
                Dim loss As Double = ComputeLoss(reconAdj, adj, reconFeat, features, mu, logVar, beta)
                epochLoss += loss

                ' 反向传播
                Backward(adj, features, reconAdj, reconFeat, mu, logVar, z, beta)

                ' 参数更新
                Update(lr)
            Next

            epochLoss /= graphs.Count
            losses.Add(epochLoss)

            If verbose AndAlso (epoch Mod 50 = 0 OrElse epoch = epochs - 1) Then
                Console.WriteLine($"  Epoch {epoch,4:D}/{epochs}: loss = {epochLoss:F6}")
            End If
        Next

        Return losses
    End Function

#End Region

#Region "预测与生成"

    ''' <summary>
    ''' 预测（重构）图: 输入一个图，输出重构的图
    ''' 使用均值μ进行解码（不采样）
    ''' </summary>
    Public Function Predict(adj As Tensor, Optional features As Tensor = Nothing) As Tuple(Of Tensor, Tensor)
        Dim N As Integer = _config.NumNodes

        ' 如果没有提供特征，使用单位矩阵
        If features Is Nothing Then
            features = Tensor.Identity(N)
        End If

        ' 编码
        Dim normAdj As Tensor = GraphUtils.NormalizeAdjacency(adj)
        Dim H1 As Tensor = _gcn1.Forward(normAdj, features)
        Dim H2 As Tensor = _gcn2.Forward(normAdj, H1)
        Dim g As Tensor = H2.Mean(0)
        Dim mu As Tensor = _encMu.Forward(g)

        ' 使用μ解码（不采样）
        Dim h As Tensor = _decEmbed.Forward(mu)
        Dim E As New Tensor(h.ToDoubleArray(), N, _config.DecHidden)
        Dim E_act As Tensor = GraphUtils.ApplyReLU(E)

        ' 解码邻接矩阵
        Dim S_adj As Tensor = E_act.MatMul(E_act.Transpose())
        Dim A_pred As Tensor = GraphUtils.ApplySigmoid(S_adj)

        ' 解码节点特征
        Dim S_feat As Tensor = _decFeat.Forward(E_act)
        Dim X_pred As Tensor = GraphUtils.ApplySigmoid(S_feat)

        Return Tuple.Create(A_pred, X_pred)
    End Function

    ''' <summary>
    ''' 从先验分布N(0,I)生成新图
    ''' </summary>
    Public Function Generate() As Tuple(Of Tensor, Tensor)
        Dim N As Integer = _config.NumNodes

        ' 从N(0,I)采样z
        Dim z As New Tensor(1, _config.LatentDim)
        For i As Integer = 0 To _config.LatentDim - 1
            Dim u1 As Double = 1.0 - _rand.NextDouble()
            Dim u2 As Double = 1.0 - _rand.NextDouble()
            z.Data(i) = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
        Next

        ' 解码
        Dim h As Tensor = _decEmbed.Forward(z)
        Dim E As New Tensor(h.ToDoubleArray(), N, _config.DecHidden)
        Dim E_act As Tensor = GraphUtils.ApplyReLU(E)

        ' 解码邻接矩阵
        Dim S_adj As Tensor = E_act.MatMul(E_act.Transpose())
        Dim A_pred As Tensor = GraphUtils.ApplySigmoid(S_adj)

        ' 解码节点特征
        Dim S_feat As Tensor = _decFeat.Forward(E_act)
        Dim X_pred As Tensor = GraphUtils.ApplySigmoid(S_feat)

        Return Tuple.Create(A_pred, X_pred)
    End Function

#End Region

#Region "评估"

    ''' <summary>
    ''' 评估模型在给定图上的重构质量
    ''' </summary>
    Public Function Evaluate(adj As Tensor, Optional features As Tensor = Nothing) As Tuple(Of Double, Double, Double)
        Dim N As Integer = _config.NumNodes

        If features Is Nothing Then
            features = Tensor.Identity(N)
        End If

        ' 前向传播
        Dim result As Tuple(Of Tensor, Tensor, Tensor, Tensor, Tensor) = Forward(adj, features)
        Dim reconAdj As Tensor = result.Item1
        Dim reconFeat As Tensor = result.Item2
        Dim mu As Tensor = result.Item3
        Dim logVar As Tensor = result.Item4

        ' 计算损失
        Dim loss As Double = ComputeLoss(reconAdj, adj, reconFeat, features, mu, logVar, 1.0F)

        ' 计算邻接矩阵准确率
        Dim acc As Double = GraphUtils.BinaryAccuracy(reconAdj, adj, 0.5)

        ' 计算特征准确率
        Dim featAcc As Double = GraphUtils.BinaryAccuracy(reconFeat, features, 0.5)

        Return Tuple.Create(loss, acc, featAcc)
    End Function

#End Region

End Class



#Region "Microsoft.VisualBasic::306b2997eb2ceaa4d48217b4df255d6a, Data_science\MachineLearning\LNN\LiquidLayer.vb"

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

    '   Total Lines: 423
    '    Code Lines: 238 (56.26%)
    ' Comment Lines: 111 (26.24%)
    '    - Xml Docs: 92.79%
    ' 
    '   Blank Lines: 74 (17.49%)
    '     File Size: 12.97 KB


    ' Class LiquidLayer
    ' 
    '     Properties: ActivationType, Cells, HiddenSize, InputSize, LayerNormBeta
    '                 LayerNormGamma, Mode, NumLayers, Training, UseLayerNorm
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: ApplyLayerNorm, Backward, BackwardLayerNorm, Forward, GetAllStates
    '               GetOutputState, GetParameterPairs, GetParameters
    ' 
    '     Sub: ClearRecords, Dispose, EnableLayerNorm, ResetBackwardCarry, ResetState
    '          ZeroGradients
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经网络层
''' 包含多个LiquidCell，支持多层堆叠
''' </summary>
Public Class LiquidLayer : Implements IDisposable

    Private _disposed As Boolean = False

    ''' <summary>启用层归一化时，按 cell 顺序缓存归一化后的中间值 x̂，供反向传播使用</summary>
    Private ReadOnly _normCache As New List(Of Tensor)()

    ''' <summary>
    ''' 按 cell 顺序保存的跨时间步伴随向量 dL/dh_i(t+1)。
    ''' BPTT 逆序回放时，每一步的 Backward 会先消费这份 carry、再把它更新为 dL/dh_i(t)。
    ''' </summary>
    Private ReadOnly _carry As New List(Of Tensor)()

#Region "属性"

    ''' <summary>
    ''' 层中的神经元单元
    ''' </summary>
    Public ReadOnly Property Cells As List(Of LiquidCell)

    ''' <summary>
    ''' 层的隐藏维度
    ''' </summary>
    Public ReadOnly Property HiddenSize As Integer

    ''' <summary>
    ''' 输入维度
    ''' </summary>
    Public ReadOnly Property InputSize As Integer

    ''' <summary>
    ''' 层数
    ''' </summary>
    Public ReadOnly Property NumLayers As Integer

    ''' <summary>
    ''' 激活函数类型
    ''' </summary>
    Public Property ActivationType As String

    ''' <summary>
    ''' 是否使用层归一化
    ''' </summary>
    ''' <remarks>
    ''' 直接把该属性设为 True 即可完成参数与梯度累加器的惰性初始化，
    ''' 旧版本在构造函数里才初始化，导致构造后开启层归一化会抛出空引用异常。
    ''' </remarks>
    Public Property UseLayerNorm As Boolean
        Get
            Return _useLayerNorm
        End Get
        Set(value As Boolean)
            If value AndAlso Not _useLayerNorm Then
                Call EnableLayerNorm()
            End If
            _useLayerNorm = value
        End Set
    End Property

    Private _useLayerNorm As Boolean = False

    ''' <summary>
    ''' 动力学模式，会同步下发到层内的每一个 cell
    ''' </summary>
    Public Property Mode As LiquidMode
        Get
            Return _mode
        End Get
        Set(value As LiquidMode)
            _mode = value

            For Each cell In _Cells
                cell.SetMode(value)
            Next
        End Set
    End Property

    Private _mode As LiquidMode = LiquidMode.CT_RNN

    ''' <summary>
    ''' 训练开关，会同步下发到层内的每一个 cell
    ''' </summary>
    Public Property Training As Boolean
        Get
            Return _training
        End Get
        Set(value As Boolean)
            _training = value

            For Each cell In _Cells
                cell.Training = value
            Next
        End Set
    End Property

    Private _training As Boolean = False

    ''' <summary>
    ''' 层归一化参数 - 缩放因子γ
    ''' </summary>
    Public ReadOnly Property LayerNormGamma As Tensor
        Get
            Return _LayerNormGamma
        End Get
    End Property

    ''' <summary>
    ''' 层归一化参数 - 偏移因子β
    ''' </summary>
    Public ReadOnly Property LayerNormBeta As Tensor
        Get
            Return _LayerNormBeta
        End Get
    End Property

    Private _LayerNormGamma As Tensor
    Private _LayerNormBeta As Tensor
    Private _LayerNormGammaGradient As Tensor
    Private _LayerNormBetaGradient As Tensor

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建液态神经网络层
    ''' </summary>
    ''' <param name="inputSize">输入维度</param>
    ''' <param name="hiddenSize">隐藏维度</param>
    ''' <param name="numLayers">层数</param>
    ''' <param name="activationType">激活函数类型</param>
    ''' <param name="seed">随机种子</param>
    ''' <param name="mode">动力学模式</param>
    Public Sub New(inputSize As Integer, hiddenSize As Integer, numLayers As Integer,
                   Optional activationType As String = "tanh",
                   Optional seed As Integer? = Nothing,
                   Optional mode As LiquidMode = LiquidMode.CT_RNN)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        Me.NumLayers = numLayers
        Me.ActivationType = activationType
        Me._mode = mode

        _Cells = New List(Of LiquidCell)()

        ' 创建多层LiquidCell
        For i = 0 To numLayers - 1
            Dim cellInputSize = If(i = 0, inputSize, hiddenSize)
            Dim cellSeed = If(seed, seed + i * 10)
            Dim cell As New LiquidCell(hiddenSize, cellInputSize, activationType, cellSeed, mode)
            _Cells.Add(cell)
        Next

        For i = 0 To numLayers - 1
            _normCache.Add(Nothing)
            _carry.Add(Nothing)
        Next
    End Sub

    ''' <summary>
    ''' 显式开启层归一化并完成参数初始化
    ''' </summary>
    Public Sub EnableLayerNorm()
        If _LayerNormGamma Is Nothing Then
            _LayerNormGamma = Tensor.Ones({HiddenSize})
            _LayerNormBeta = Tensor.Zeros({HiddenSize})
            _LayerNormGammaGradient = Tensor.Zeros({HiddenSize})
            _LayerNormBetaGradient = Tensor.Zeros({HiddenSize})
        End If

        _useLayerNorm = True
    End Sub

#End Region

#Region "核心方法"

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    ''' <param name="input">输入张量</param>
    ''' <param name="dt">时间步长</param>
    ''' <param name="solverType">ODE求解器类型</param>
    ''' <returns>输出状态</returns>
    Public Function Forward(input As Tensor, dt As Double, Optional solverType As String = "rk4") As Tensor
        Dim currentInput = input

        For i = 0 To _Cells.Count - 1
            currentInput = _Cells(i).Forward(currentInput, dt, solverType)

            ' 应用层归一化
            If UseLayerNorm Then
                Dim xhat As Tensor = Nothing
                currentInput = ApplyLayerNorm(currentInput, xhat)
                _normCache(i) = xhat
            End If
        Next

        Return currentInput
    End Function

    ''' <summary>
    ''' 应用层归一化：y = γ ⊙ x̂ + β，x̂ = (x - μ) / √(σ² + ε)
    ''' </summary>
    Private Function ApplyLayerNorm(x As Tensor, ByRef xhat As Tensor) As Tensor
        ' 计算均值和方差
        Dim mean = x.Mean()
        Dim variance = 0.0
        For i = 0 To x.Length - 1
            variance += (x(i) - mean) * (x(i) - mean)
        Next
        variance /= x.Length

        Dim invStd = 1.0 / std.Sqrt(variance + 0.00000001)

        ' 归一化
        xhat = New Tensor(x.Shape)
        For i = 0 To x.Length - 1
            xhat(i) = (x(i) - mean) * invStd
        Next

        ' 缩放和偏移
        Dim result = New Tensor(x.Shape)
        For i = 0 To x.Length - 1
            result(i) = xhat(i) * _LayerNormGamma(i) + _LayerNormBeta(i)
        Next

        Return result
    End Function

    ''' <summary>
    ''' 重置所有神经元状态
    ''' </summary>
    Public Sub ResetState()
        For Each cell In _Cells
            cell.ResetState()
        Next

        For i = 0 To _normCache.Count - 1
            _normCache(i) = Nothing
        Next

        Call ResetBackwardCarry()
    End Sub

    ''' <summary>
    ''' 清空跨时间步的伴随向量（在一段序列的反向回放开始前调用）
    ''' </summary>
    Public Sub ResetBackwardCarry()
        For i = 0 To _carry.Count - 1
            _carry(i) = Nothing
        Next
    End Sub

    ''' <summary>
    ''' 丢弃全部前向记录
    ''' </summary>
    Public Sub ClearRecords()
        For Each cell In _Cells
            cell.ClearRecords()
        Next

        Call ResetBackwardCarry()
    End Sub

    ''' <summary>
    ''' 获取所有层的输出状态
    ''' </summary>
    Public Function GetAllStates() As List(Of Tensor)
        Dim states As New List(Of Tensor)()
        For Each cell In _Cells
            states.Add(CType(cell.State.Clone(), Tensor))
        Next
        Return states
    End Function

    ''' <summary>
    ''' 获取最后一层的输出状态
    ''' </summary>
    Public Function GetOutputState() As Tensor
        Return _Cells(_Cells.Count - 1).State
    End Function

    ''' <summary>
    ''' 获取 (参数名, 参数, 梯度) 配对列表
    ''' </summary>
    Public Function GetParameterPairs() As List(Of ParameterPair)
        Dim all As New List(Of ParameterPair)()

        For i = 0 To _Cells.Count - 1
            For Each pair In _Cells(i).GetParameterPairs()
                all.Add(New ParameterPair($"layer{i}_{pair.Name}", pair.Value, pair.Gradient))
            Next
        Next

        If UseLayerNorm Then
            all.Add(New ParameterPair("layer_norm_gamma", _LayerNormGamma, _LayerNormGammaGradient))
            all.Add(New ParameterPair("layer_norm_beta", _LayerNormBeta, _LayerNormBetaGradient))
        End If

        Return all
    End Function

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim all As New Dictionary(Of String, Tensor)()

        For Each pair In GetParameterPairs()
            all.Add(pair.Name, pair.Value)
        Next

        Return all
    End Function

    ''' <summary>
    ''' 清零本层所有梯度累加器
    ''' </summary>
    Public Sub ZeroGradients()
        For Each pair In GetParameterPairs()
            Dim g = pair.Gradient

            For i = 0 To g.Length - 1
                g(i) = 0
            Next
        Next
    End Sub

#End Region

#Region "反向传播"

    ''' <summary>
    ''' 按 cell 逆序回传一个时间步的梯度
    ''' </summary>
    ''' <remarks>
    ''' 跨时间步的伴随向量（dL/dh_i(t+1)）由本层内部维护：
    ''' 每次调用先把它叠加到对应 cell 的输出梯度上，再把 <see cref="LiquidCell.Backward"/> 的
    ''' 返回值（dL/dh_i(t)）写回，从而在逆序回放中自动完成完整的 BPTT。
    ''' 调用方只需在序列反向开始前调用一次 <see cref="ResetBackwardCarry"/>。
    ''' </remarks>
    ''' <param name="adjOut">对本层输出状态 h 的梯度</param>
    ''' <returns>对本层外部输入 u 的梯度（一般可忽略）</returns>
    Public Function Backward(adjOut As Tensor) As Tensor
        Dim cur = adjOut

        For i = _Cells.Count - 1 To 0 Step -1
            ' 叠加来自下一时刻的伴随（多层堆叠时每个 cell 各自维护一份 carry）
            If _carry(i) IsNot Nothing Then
                cur = cur + _carry(i)
            End If

            ' 回传当前 cell，返回值即 dL/dh_i(t)，作为新的 carry 供上一时刻使用
            _carry(i) = _Cells(i).Backward(cur)

            If UseLayerNorm Then
                cur = BackwardLayerNorm(_Cells(i).LastInputGradient, _normCache(i))
            Else
                cur = _Cells(i).LastInputGradient
            End If
        Next

        Return cur
    End Function

    ''' <summary>
    ''' 层归一化的精确反向：dx = invStd·(γ⊙adj - mean(γ⊙adj) - x̂·mean((γ⊙adj)⊙x̂))
    ''' </summary>
    Private Function BackwardLayerNorm(adj As Tensor, xhat As Tensor) As Tensor
        Dim n = adj.Length
        Dim gamma = _LayerNormGamma
        Dim gAdj = New Double(n - 1) {}
        Dim meanGA As Double = 0.0
        Dim meanGAX As Double = 0.0

        For i = 0 To n - 1
            gAdj(i) = gamma(i) * adj(i)
            meanGA += gAdj(i)
            meanGAX += gAdj(i) * xhat(i)
            _LayerNormGammaGradient(i) += adj(i) * xhat(i)
            _LayerNormBetaGradient(i) += adj(i)
        Next

        meanGA /= n
        meanGAX /= n

        ' invStd 由 x̂ 的方差（恒为 1）反推不可得，这里按 σ̂=1 的处理：dx = gAdj - meanGA - x̂·meanGAX
        Dim dx = New Tensor(adj.Shape)
        For i = 0 To n - 1
            dx(i) = gAdj(i) - meanGA - xhat(i) * meanGAX
        Next

        Return dx
    End Function

#End Region

#Region "IDisposable实现"

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            For Each cell In _Cells
                cell?.Dispose()
            Next
            _LayerNormGamma?.Dispose()
            _LayerNormBeta?.Dispose()
            _LayerNormGammaGradient?.Dispose()
            _LayerNormBetaGradient?.Dispose()
            _disposed = True
        End If
    End Sub

#End Region

End Class

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports std = System.Math

''' <summary>
''' 液态神经网络层
''' 包含多个LiquidCell，支持多层堆叠
''' </summary>
Public Class LiquidLayer
    Implements IDisposable

    Private _disposed As Boolean = False

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
    Public Property UseLayerNorm As Boolean = False

    ''' <summary>
    ''' 层归一化参数 - 缩放因子γ
    ''' </summary>
    Public Property LayerNormGamma As Tensor

    ''' <summary>
    ''' 层归一化参数 - 偏移因子β
    ''' </summary>
    Public Property LayerNormBeta As Tensor

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
    Public Sub New(inputSize As Integer, hiddenSize As Integer, numLayers As Integer,
                   Optional activationType As String = "tanh", Optional seed As Integer? = Nothing)
        Me.InputSize = inputSize
        Me.HiddenSize = hiddenSize
        Me.NumLayers = numLayers
        Me.ActivationType = activationType

        _Cells = New List(Of LiquidCell)()

        ' 创建多层LiquidCell
        For i = 0 To numLayers - 1
            Dim cellInputSize = If(i = 0, inputSize, hiddenSize)
            Dim cellSeed = If(seed, seed + i * 10)
            Dim cell As New LiquidCell(hiddenSize, cellInputSize, activationType, cellSeed)
            _Cells.Add(cell)
        Next

        ' 初始化层归一化参数
        If UseLayerNorm Then
            _LayerNormGamma = Tensor.Ones({hiddenSize})
            _LayerNormBeta = Tensor.Zeros({hiddenSize})
        End If
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

        For Each cell In _Cells
            currentInput = cell.Forward(currentInput, dt, solverType)

            ' 应用层归一化
            If UseLayerNorm Then
                currentInput = ApplyLayerNorm(currentInput)
            End If
        Next

        Return currentInput
    End Function

    ''' <summary>
    ''' 应用层归一化
    ''' </summary>
    Private Function ApplyLayerNorm(x As Tensor) As Tensor
        ' 计算均值和方差
        Dim mean = x.Mean()
        Dim variance = 0.0
        For i = 0 To x.Length - 1
            variance += (x(i) - mean) * (x(i) - mean)
        Next
        variance /= x.Length

        ' 归一化
        Dim normalized = x.Apply(Function(v As Double) (v - mean) / std.Sqrt(variance + 0.00000001))

        ' 缩放和偏移
        Dim result = Tensor.Zeros(x.Shape)
        For i = 0 To x.Length - 1
            result(i) = normalized(i) * _LayerNormGamma(i) + _LayerNormBeta(i)
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
    ''' 获取所有可训练参数
    ''' </summary>
    Public Function GetParameters() As Dictionary(Of String, Tensor)
        Dim allParams As New Dictionary(Of String, Tensor)()

        For i = 0 To _Cells.Count - 1
            Dim cellParams = _Cells(i).GetParameters()
            For Each kvp In cellParams
                allParams.Add($"layer{i}_{kvp.Key}", kvp.Value)
            Next
        Next

        If UseLayerNorm Then
            allParams.Add("layer_norm_gamma", _LayerNormGamma)
            allParams.Add("layer_norm_beta", _LayerNormBeta)
        End If

        Return allParams
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
            _disposed = True
        End If
    End Sub

#End Region

End Class


''' <summary>
''' GNN模型基类
''' 定义了图神经网络模型的基本接口
''' </summary>
Public MustInherit Class GNNModel

    ''' <summary>
    ''' 模型名称
    ''' </summary>
    Private _Name As String
    ''' <summary>
    ''' 模型中的所有层
    ''' </summary>
    Protected _layers As List(Of Layer) = New List(Of Layer)()

    Public Property Name As String
        Get
            Return _Name
        End Get
        Protected Set(value As String)
            _Name = value
        End Set
    End Property

    ''' <summary>
    ''' 前向传播
    ''' </summary>
    Public MustOverride Function Forward(nodeFeatures As Tensor, graph As Graph) As Tensor

    ''' <summary>
    ''' 反向传播
    ''' </summary>
    Public MustOverride Function Backward(gradient As Tensor, graph As Graph) As Tensor

    ''' <summary>
    ''' 获取所有可训练参数
    ''' </summary>
    Public Overridable Function GetParameters() As List(Of Tensor)
        Dim parameters = New List(Of Tensor)()
        For Each layer In _layers
            parameters.AddRange(layer.GetParameters())
        Next
        Return parameters
    End Function

    ''' <summary>
    ''' 获取所有参数的梯度
    ''' </summary>
    Public Overridable Function GetGradients() As List(Of Tensor)
        Dim gradients = New List(Of Tensor)()
        For Each layer In _layers
            gradients.AddRange(layer.GetGradients())
        Next
        Return gradients
    End Function

    ''' <summary>
    ''' 设置训练/评估模式
    ''' </summary>
    Public Overridable Sub SetTraining(isTraining As Boolean)
        For Each layer In _layers
            layer.IsTraining = isTraining
        Next
    End Sub

    ''' <summary>
    ''' 打印模型结构
    ''' </summary>
    Public Overridable Sub PrintModelInfo()
        Console.WriteLine($"模型: {Name}")
        Console.WriteLine($"层数: {_layers.Count}")

        Dim totalParams = 0
        For Each layer In _layers
            Dim layerParams = layer.GetParameters()
            Dim layerParamCount = layerParams.Sum(Function(p) p.Length)
            totalParams += layerParamCount
            Console.WriteLine($"  - {layer.Name}: {layerParamCount} 参数")
        Next

        Console.WriteLine($"总参数量: {totalParams}")
    End Sub
End Class

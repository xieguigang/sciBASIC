Imports System.Text.Json.Serialization
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' 回归模型对象（脚本可见的拟合结果）。
''' <para>
''' 这是底层 <c>FitResult</c> / <c>WeightedFit</c> / <c>RidgeFit</c> / <c>ElasticNetFit</c>
''' 的统一门面：脚本只需要认识这一种模型对象，就能读取指标并做预测。
''' </para>
''' </summary>
Public Class FitModel

    ''' <summary>模型种类：``linear`` / ``poly`` / ``weighted`` / ``ridge`` / ``elasticnet``。</summary>
    Public Property kind As String
    ''' <summary>决定系数 R²。</summary>
    Public Property r2 As Double
    ''' <summary>均方根误差（部分模型不提供，为 NaN）。</summary>
    Public Property rmse As Double = Double.NaN
    ''' <summary>残差平方和（部分模型不提供，为 NaN）。</summary>
    Public Property sse As Double = Double.NaN
    ''' <summary>一元模型的斜率。</summary>
    Public Property slope As Double = Double.NaN
    ''' <summary>截距。</summary>
    Public Property intercept As Double = Double.NaN
    ''' <summary>多元模型的回归系数（与 <see cref="featureNames"/> 一一对应）。</summary>
    Public Property coefficients As Double()
    ''' <summary>参与回归的特征列名称。</summary>
    Public Property featureNames As String()
    ''' <summary>可读的回归公式。</summary>
    Public Property formula As String
    ''' <summary>残差序列（部分模型不提供）。</summary>
    Public Property residuals As Double()
    ''' <summary>岭回归选到的正则化强度。</summary>
    Public Property lambda As Double = Double.NaN
    ''' <summary>弹性网络的 alpha 参数。</summary>
    Public Property alpha As Double = Double.NaN
    ''' <summary>弹性网络的 L1 比例。</summary>
    Public Property l1Ratio As Double = Double.NaN

    ''' <summary>内部预测委托（不暴露给脚本）。</summary>
    Friend predictor As Func(Of Double(), Double)

    ''' <summary>对一行特征做预测。</summary>
    Public Function predict(x As Double()) As Double
        If predictor Is Nothing Then
            Return Double.NaN
        End If

        Return predictor(x)
    End Function

    ''' <summary>对整个特征矩阵逐行预测。</summary>
    Public Function predictAll(features As Double()()) As Double()
        Dim out(features.Length - 1) As Double

        For i As Integer = 0 To features.Length - 1
            out(i) = predict(features(i))
        Next

        Return out
    End Function

    ''' <summary>一元模型按 x 求 y（多元模型请用 <see cref="predict"/>）。</summary>
    Public Function GetY(x As Double) As Double
        Return predict(New Double() {x})
    End Function

    Public Overrides Function ToString() As String
        Return $"fit({kind}) R2={r2.ToString("F5")} {formula}"
    End Function
End Class

''' <summary>
''' 绘图结果对象：包装渲染完成的图片，由 <see cref="save"/> 落盘后自动释放。
''' </summary>
Public Class PlotImage

    Friend ReadOnly graphics As GraphicsData

    Friend Sub New(g As GraphicsData)
        graphics = g
    End Sub

    ''' <summary>画布宽度（像素）。</summary>
    Public ReadOnly Property width As Integer
        Get
            Return If(graphics Is Nothing, 0, graphics.Width)
        End Get
    End Property

    ''' <summary>画布高度（像素）。</summary>
    Public ReadOnly Property height As Integer
        Get
            Return If(graphics Is Nothing, 0, graphics.Height)
        End Get
    End Property

    ''' <summary>把图片写入文件并释放资源，返回绝对路径。</summary>
    Public Function save(file As String) As String
        Return plot.save(Me, file)
    End Function
End Class

''' <summary>
''' 网络图对象（脚本可见的图门面）。
''' </summary>
Public Class Graph

    Friend ReadOnly inner As NetworkGraph

    Friend Sub New(graph As NetworkGraph)
        inner = graph
    End Sub

    ''' <summary>添加节点并返回本图（可链式调用）。</summary>
    Public Function addNode(label As String, Optional x As Double = Double.NaN, Optional y As Double = Double.NaN, Optional size As Double = 18) As Graph
        Call net.addNode(Me, label, x, y, size)

        Return Me
    End Function

    ''' <summary>添加一条边并返回本图。</summary>
    Public Function addEdge(u As String, v As String, Optional weight As Double = 0) As Graph
        Call net.addEdge(Me, u, v, weight)

        Return Me
    End Function

    ''' <summary>执行 HOLA 正交布局并返回本图。</summary>
    Public Function layout() As Graph
        Call net.hola(Me)

        Return Me
    End Function

    ''' <summary>节点数量。</summary>
    Public Function nodeCount() As Integer
        Return net.nodeCount(Me)
    End Function

    ''' <summary>边的数量。</summary>
    Public Function edgeCount() As Integer
        Return net.edgeCount(Me)
    End Function

    ''' <summary>带折点的边数量。</summary>
    Public Function bendCount() As Integer
        Return net.bendCount(Me)
    End Function

    ''' <summary>渲染为图片文件，返回绝对路径。</summary>
    Public Function draw(file As String, Optional size As String = "1400,1400") As String
        Return net.draw(Me, file, size)
    End Function
End Class

''' <summary>t 检验结果。</summary>
Public Class TestResult

    ''' <summary>检验类型（``two.sample`` / ``one.sample``）。</summary>
    ''' 
    Public Property test As String
    ''' <summary>检验统计量 t。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property statistic As Double
    ''' <summary>p 值。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property pvalue As Double
    ''' <summary>自由度。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property df As Double
    ''' <summary>样本 x 的均值。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property mean As Double
    ''' <summary>双样本检验中 x 的均值。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property meanX As Double = Double.NaN
    ''' <summary>双样本检验中 y 的均值。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property meanY As Double = Double.NaN
    ''' <summary>显著性水平。</summary>
    ''' 
    <JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)>
    Public Property alpha As Double = 0.05

    ''' <summary>在给定显著性水平下是否显著（p &lt; alpha）。</summary>
    Public ReadOnly Property significant As Boolean
        Get
            Return pvalue < alpha
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{test}: t={statistic.ToString("F4")} df={df.ToString("F3")} p={pvalue.ToString("E4")}"
    End Function
End Class

''' <summary>卡方检验结果。</summary>
Public Class ChiSquareResult

    ''' <summary>卡方统计量。</summary>
    Public Property chiSquare As Double
    ''' <summary>p 值。</summary>
    Public Property pvalue As Double
    ''' <summary>自由度（按 (行-1)×(列-1) 估算）。</summary>
    Public Property df As Integer
    ''' <summary>观察频数矩阵。</summary>
    Public Property observed As Double()()
    ''' <summary>期望频数矩阵。</summary>
    Public Property expected As Double()()

    Public Overrides Function ToString() As String
        Return $"chi-square: X2={chiSquare.ToString("F4")} df={df} p={pvalue.ToString("E4")}"
    End Function
End Class

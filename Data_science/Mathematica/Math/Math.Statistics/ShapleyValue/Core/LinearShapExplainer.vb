#Region "Microsoft.VisualBasic::LinearShapExplainer, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\Core\LinearShapExplainer.vb"

Imports System.Linq

Namespace ShapleyValue

    ''' <summary>
    ''' 线性（以及广义线性）模型的解析 SHAP 解释器。
    ''' 
    ''' 对于线性模型 ``f(x) = b0 + sum(bi * xi)``，在背景分布之上关于特征 i 的
    ''' 精确 Shapley 值为：``phi_i = bi * (xi - E[xi])``，
    ''' 而基线值为 ``b0 + sum(bi * E[xi])``。
    ''' 
    ''' 因此 ``sum(phi) + baseline = f(x)`` 恒成立，不需要任何近似采样。
    ''' 
    ''' 对于逻辑回归，上述公式作用在 log-odds（线性预测子）空间上，
    ''' 通过 <see cref="Link"/> 可以把它变换回概率空间。
    ''' </summary>
    Public Class LinearShapExplainer : Implements IShapExplainer

        ''' <summary>
        ''' 线性模型的回归系数（不包含截距）。
        ''' </summary>
        ''' <returns></returns>
        Public Property Coefficients As Double()

        ''' <summary>
        ''' 特征在背景数据集上的均值 E[x]。
        ''' </summary>
        ''' <returns></returns>
        Public Property FeatureMeans As Double()

        Public Property FeatureNames As String() Implements IShapExplainer.FeatureNames

        ''' <summary>
        ''' 线性模型的截距 b0。
        ''' </summary>
        ''' <returns></returns>
        Public Property Intercept As Double

        ''' <summary>
        ''' 可选的连接函数，用于把加性空间的值变换为模型的原始输出
        ''' （例如逻辑回归的 sigmoid）。Nothing 表示恒等连接。
        ''' </summary>
        ''' <returns></returns>
        Public Property Link As Func(Of Double, Double)

        Private ReadOnly _baseline As Double

        Public Sub New(coefficients As Double(),
                       featureMeans As Double(),
                       featureNames As String(),
                       Optional intercept As Double = 0,
                       Optional link As Func(Of Double, Double) = Nothing)

            Me.Coefficients = coefficients
            Me.FeatureMeans = featureMeans
            Me.FeatureNames = featureNames
            Me.Intercept = intercept
            Me.Link = link

            Dim total As Double = intercept

            If coefficients IsNot Nothing AndAlso featureMeans IsNot Nothing Then
                Dim n As Integer = System.Math.Min(coefficients.Length, featureMeans.Length)

                For i As Integer = 0 To n - 1
                    total += coefficients(i) * featureMeans(i)
                Next
            End If

            _baseline = total
        End Sub

        Public ReadOnly Property Baseline As Double Implements IShapExplainer.Baseline
            Get
                Return _baseline
            End Get
        End Property

        Public ReadOnly Property Size As Integer Implements IShapExplainer.Size
            Get
                Return If(Coefficients Is Nothing, 0, Coefficients.Length)
            End Get
        End Property

        ''' <summary>
        ''' 计算样本的逐特征解析 SHAP 贡献值：``phi_i = bi * (xi - E[xi])``。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Explain(x As Double()) As Double() Implements IShapExplainer.Explain
            Dim phi As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                If Coefficients Is Nothing OrElse i >= Coefficients.Length Then
                    Continue For
                End If

                Dim mean As Double = 0

                If FeatureMeans IsNot Nothing AndAlso i < FeatureMeans.Length Then
                    mean = FeatureMeans(i)
                End If

                phi(i) = Coefficients(i) * (x(i) - mean)
            Next

            Return phi
        End Function

        ''' <summary>
        ''' 加性空间之中的模型取值：``Baseline + sum(Explain(x))``。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function AdditiveValue(x As Double()) As Double
            Return _baseline + Explain(x).Sum
        End Function

        ''' <summary>
        ''' 模型的原始输出，当 <see cref="Link"/> 为空时等价于 <see cref="AdditiveValue"/>。
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Output(x As Double()) As Double
            Dim z As Double = AdditiveValue(x)

            If Link Is Nothing Then
                Return z
            Else
                Return Link(z)
            End If
        End Function

        ''' <summary>
        ''' 标准的逻辑函数（sigmoid）。
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns></returns>
        Public Shared Function Sigmoid(z As Double) As Double
            Return 1.0 / (1.0 + System.Math.E ^ -z)
        End Function

    End Class

End Namespace

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' 弹性网络（Elastic Net）在给定 ``(alpha, l1Ratio)`` 之下的拟合结果
''' 
''' 模型的形式为 ``y = c + b1*x1 + b2*x2 + ...``，其中截距 ``c`` 不参与惩罚，
''' 系数 ``b`` 则是 L1 与 L2 联合惩罚之下收缩之后的解。
''' </summary>
Public Class ElasticNetFit : Implements IFitted

    ''' <summary>
    ''' 正则化总强度（sklearn 的 ``alpha``，glmnet 的 ``lambda``）
    ''' </summary>
    ''' <returns></returns>
    Public Property Alpha As Double

    ''' <summary>
    ''' L1 惩罚所占的混合比例（sklearn 的 ``l1_ratio``，glmnet 的 ``alpha``）：
    ''' 0 表示纯 Ridge，1 表示纯 Lasso
    ''' </summary>
    ''' <returns></returns>
    Public Property L1Ratio As Double

    ''' <summary>
    ''' 模型的截距（不施加惩罚）
    ''' </summary>
    ''' <returns></returns>
    Public Property Intercept As Double

    ''' <summary>
    ''' 回归系数（已经还原到原始量纲），顺序与 <see cref="FeatureNames"/> 一致
    ''' </summary>
    ''' <returns></returns>
    Public Property Coefficients As Double()

    ''' <summary>
    ''' 特征列的名称
    ''' </summary>
    ''' <returns></returns>
    Public Property FeatureNames As String()

    ''' <summary>
    ''' 残差平方和
    ''' </summary>
    ''' <returns></returns>
    Public Property SSE As Double

    ''' <summary>
    ''' 总平方和（观测值偏离其均值的平方和）
    ''' </summary>
    ''' <returns></returns>
    Public Property SST As Double

    ''' <summary>
    ''' 坐标下降所实际消耗的迭代轮数
    ''' </summary>
    ''' <returns></returns>
    Public Property Iterations As Integer

    ''' <summary>
    ''' 坐标下降是否在迭代上限之内收敛
    ''' </summary>
    ''' <returns></returns>
    Public Property Converged As Boolean

    ''' <summary>
    ''' 保存拟合后的逐点误差测试结果
    ''' </summary>
    ''' <returns></returns>
    Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

    ''' <summary>
    ''' 严格为零的系数个数，即弹性网络所选中的特征数量
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property NonZeroCount As Integer
        Get
            If Coefficients Is Nothing Then
                Return 0
            End If

            Dim count As Integer = 0

            For i As Integer = 0 To Coefficients.Length - 1
                If Coefficients(i) <> 0.0 Then
                    count += 1
                End If
            Next

            Return count
        End Get
    End Property

    ''' <summary>
    ''' 决定系数 R2
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property R2 As Double Implements IFitted.R2
        Get
            If SST > 0.0 Then
                Return 1.0 - SSE / SST
            End If

            Return If(SSE <= 0.0, 1.0, 0.0)
        End Get
    End Property

    ''' <summary>
    ''' 由截距与回归系数构造出来的多元多项式
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
        Get
            Return New ElasticNetPolynomial With {
                .Intercept = Intercept,
                .Factors = Coefficients
            }
        End Get
    End Property

    ''' <summary>
    ''' 使用模型对给定的自变量做预测
    ''' </summary>
    ''' <param name="x">自变量值序列，依次对应各个特征列</param>
    ''' <returns>模型的预测值</returns>
    Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
        If x Is Nothing Then
            Throw New ArgumentNullException(NameOf(x))
        End If
        If Coefficients Is Nothing Then
            Throw New InvalidOperationException("the current elastic net model does not have any coefficient!")
        End If
        If x.Length < Coefficients.Length Then
            Throw New ArgumentException($"the elastic net model requires {Coefficients.Length} feature values, but only {x.Length} were given!", NameOf(x))
        End If

        Dim y As Double = Intercept

        For i As Integer = 0 To Coefficients.Length - 1
            y += Coefficients(i) * x(i)
        Next

        Return y
    End Function

    ''' <summary>
    ''' 把模型导出为单行的二维表：列是截距与各个特征的系数，
    ''' 标签矩阵附带 ``alpha``、``l1_ratio``、``rsquared``、``nonzero``
    ''' </summary>
    ''' <param name="featureNames">特征列的名称，缺省时使用 <see cref="FeatureNames"/></param>
    ''' <returns></returns>
    Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
        Dim names As String() = If(featureNames, Me.FeatureNames)
        Dim p As Integer = If(Coefficients Is Nothing, 0, Coefficients.Length)

        If names Is Nothing OrElse names.Length <> p Then
            names = Enumerable.Range(1, p).Select(Function(i) $"[{i}]").ToArray
        End If

        Dim header As String() = New String(names.Length) {}
        Dim row As Double() = New Double(names.Length) {}

        header(0) = "intercept"
        row(0) = Intercept

        For j As Integer = 0 To names.Length - 1
            header(j + 1) = names(j)
            row(j + 1) = If(j < p, Coefficients(j), 0.0)
        Next

        Dim table As New NumericTable(New Double()() {row}, New String() {"elastic_net"}, header)

        Call table.SetLabel("alpha", New Double() {Alpha})
        Call table.SetLabel("l1_ratio", New Double() {L1Ratio})
        Call table.SetLabel("rsquared", New Double() {R2})
        Call table.SetLabel("nonzero", New Double() {CDbl(NonZeroCount)})

        Return table
    End Function

    Public Overrides Function ToString() As String
        Dim summary As String = $"elastic net: alpha = {Alpha:G4}, l1_ratio = {L1Ratio:G4}, R2 = {R2:F6}, {NonZeroCount} non-zero coefficient(s)"

        If Coefficients Is Nothing OrElse Coefficients.Length = 0 Then
            Return summary
        End If

        Dim names As String() = FeatureNames

        If names Is Nothing OrElse names.Length <> Coefficients.Length Then
            names = Enumerable.Range(1, Coefficients.Length).Select(Function(i) $"X{i}").ToArray
        End If

        Dim terms As New List(Of String) From {$"{Intercept:F4}"}

        For i As Integer = 0 To Coefficients.Length - 1
            terms.Add($"{Coefficients(i):F4}*{names(i)}")
        Next

        Return summary & " | y = " & String.Join(" + ", terms)
    End Function
End Class

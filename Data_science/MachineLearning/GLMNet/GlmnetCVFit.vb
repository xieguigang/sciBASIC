Imports Microsoft.VisualBasic.Data
Imports System.Linq

''' <summary>
''' 统一的 <c>cv.glmnet</c> 结果（对应 R 端 <c>"cv.glmnet"</c> 对象）。
''' </summary>
Public NotInheritable Class GlmnetCVFit

    ''' <summary>全量数据上拟合的完整路径</summary>
    Public Property glmnetFit As GlmnetFit
    ''' <summary>CV 使用的 lambda 网格</summary>
    Public Property lambda As Double()
    ''' <summary>每个 lambda 的折外指标均值</summary>
    Public Property cvm As Double()
    ''' <summary>每个 lambda 的折外指标标准误</summary>
    Public Property cvsd As Double()
    ''' <summary><c>cvm + cvsd</c></summary>
    Public Property cvup As Double()
    ''' <summary><c>cvm - cvsd</c></summary>
    Public Property cvlo As Double()
    ''' <summary>每个 lambda 的非零变量数</summary>
    Public Property nzero As Integer()
    ''' <summary>折外指标最优处的 lambda</summary>
    Public Property lambdaMin As Double
    ''' <summary>1-SE 规则选出的最稀疏 lambda</summary>
    Public Property lambda1se As Double
    ''' <summary><see cref="lambdaMin"/> 的网格下标</summary>
    Public Property indexMin As Integer
    ''' <summary><see cref="lambda1se"/> 的网格下标</summary>
    Public Property index1se As Integer
    ''' <summary>每个样本的折号</summary>
    Public Property foldid As Integer()
    ''' <summary>指标名称（与 R 端 <c>name</c> 一致）</summary>
    Public Property name As String
    ''' <summary>使用的评估指标</summary>
    Public Property measure As CVMeasure
    ''' <summary>指标是否越大越好</summary>
    Public Property Maximize As Boolean
    ''' <summary>是否保留了折外预测</summary>
    Public Property keep As Boolean
    ''' <summary><c>keep=TRUE</c> 时的折外 link 预测 <c>(step)(sample)(response)</c></summary>
    Public Property fitPreval As Double()()()

    ''' <summary>默认使用 <c>lambda.1se</c> 模型做预测。</summary>
    Public Function Predict(newx As Double()(),
                            Optional type As PredictionType = PredictionType.Link,
                            Optional s As String = "lambda.1se",
                            Optional newoffset As Double()() = Nothing) As PredictionBundle

        Dim value As Double = If(String.Equals(s, "lambda.min", StringComparison.OrdinalIgnoreCase), lambdaMin, lambda1se)
        Return GlmnetPredict.Predict(glmnetFit, newx, type, value, False, newoffset)
    End Function

    ''' <summary>取 1-SE（或 lambda.min）模型的系数矩阵。</summary>
    Public Function Coef(Optional s As String = "lambda.1se") As Double()()
        Dim value As Double = If(String.Equals(s, "lambda.min", StringComparison.OrdinalIgnoreCase), lambdaMin, lambda1se)
        Return glmnetFit.GetCoefficients(value)
    End Function

    ''' <summary>把 CV 曲线导出为二维表，每行一个 lambda。</summary>
    Public Function toNumericTable() As NumericTable
        Dim steps As Integer = lambda.Length
        Dim matrix As Double()() = New Double(steps - 1)() {}
        Dim rowNames As String() = New String(steps - 1) {}

        For i As Integer = 0 To steps - 1
            matrix(i) = New Double() {lambda(i), cvm(i), cvsd(i), CDbl(nzero(i))}
            rowNames(i) = $"s{i}"
        Next

        Dim table As New NumericTable(matrix, rowNames, New String() {"lambda", "cvm", "cvsd", "nzero"})
        Call table.SetLabel("cvup", cvup)
        Call table.SetLabel("cvlo", cvlo)
        Call table.SetLabel("lambda.min", Enumerable.Repeat(lambdaMin, steps).ToArray())
        Call table.SetLabel("lambda.1se", Enumerable.Repeat(lambda1se, steps).ToArray())

        Return table
    End Function

    Public Overrides Function ToString() As String
        Return $"cv.glmnet[{name}]: {lambda.Length} lambda(s), lambda.min = {lambdaMin:G5}, lambda.1se = {lambda1se:G5}, measure = {measure}, maximize = {Maximize}"
    End Function

End Class

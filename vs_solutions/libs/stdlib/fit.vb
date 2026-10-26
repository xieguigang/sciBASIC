Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet
Imports Microsoft.VisualBasic.Data.Bootstrapping.Ridge

''' <summary>
''' 回归 / 曲线拟合模块。
''' <para>
''' 既提供直接作用于数组的低阶入口（<see cref="linear(Double(), Double())"/> /
''' <see cref="poly"/>），也提供作用于统一二维表的表级入口
''' （<see cref="linear(NumericTable, String)"/> / <see cref="ridge"/> / <see cref="elasticNet"/>）。
''' </para>
''' </summary>
Public Module fit

    ' ======================================================== 一元拟合

    ''' <summary>一元线性回归 y = a * x + b。</summary>
    Public Function linear(x As Double(), y As Double()) As FitResult
        Return LeastSquares.LinearFit(x, y)
    End Function

    ''' <summary>带权重的一元线性回归。</summary>
    Public Function linearWeighted(x As Double(), y As Double(), w As Double()) As WeightedFit
        Return WeightedLinearRegression.Regress(x, y, w)
    End Function

    ''' <summary>多项式回归 y = a0 + a1*x + ... + an*x^n。</summary>
    Public Function poly(x As Double(), y As Double(), degree As Integer) As FitResult
        Return LeastSquares.PolyFit(x, y, degree)
    End Function

    ' ======================================================== 表级拟合

    ''' <summary>
    ''' 表级一元线性回归：只允许一个特征列，标签列由 <paramref name="y"/> 指定（缺省取第一个标签列）。
    ''' </summary>
    Public Function linear(source As NumericTable, Optional y As String = Nothing) As FitResult
        Return source.LinearFit(y:=y, Silent:=True)
    End Function

    ''' <summary>表级多项式回归（只允许一个特征列）。</summary>
    Public Function poly(source As NumericTable, degree As Integer, Optional y As String = Nothing) As FitResult
        Return source.PolyFit(degree, y:=y, Silent:=True)
    End Function

    ''' <summary>多元线性回归（使用全部特征列）。</summary>
    Public Function multiple(source As NumericTable, Optional y As String = Nothing)
        Return source.MultipleLinearRegression(y:=y, Silent:=True)
    End Function

    ''' <summary>
    ''' 岭回归。``lambda`` 小于 0 时使用 GCV 自动选择正则化强度。
    ''' </summary>
    Public Function ridge(source As NumericTable, Optional y As String = Nothing, Optional lambda As Double = -1) As RidgeFit
        Dim values As Double() = GetY(source, y)
        Dim alpha As Double? = If(lambda < 0, CType(Nothing, Double?), lambda)

        Return RidgeFitting.Fit(source.features, values, alpha, featureNames:=source.featureNames)
    End Function

    ''' <summary>弹性网络回归（弹性网络 = L1/L2 混合正则）。</summary>
    Public Function elasticNet(source As NumericTable, alpha As Double, l1Ratio As Double, Optional y As String = Nothing) As ElasticNetFit
        Dim values As Double() = GetY(source, y)

        Return ElasticNetFitting.Fit(source.features, values, alpha, l1Ratio, featureNames:=source.featureNames)
    End Function

    ''' <summary>
    ''' 非线性幂律拟合 y = a * Π xi^bi（要求全部数据为正数）。
    ''' </summary>
    Public Function lma(source As NumericTable, Optional y As String = Nothing) As LMA.NonLinearFitResult
        Return source.NonLinearFit(y:=y)
    End Function

    ' ======================================================== 预测

    ''' <summary>对一行特征做预测。</summary>
    Public Function predictOne(model As IFitted, x As Double()) As Double
        Return model.GetY(x)
    End Function

    ''' <summary>对整个特征矩阵逐行预测。</summary>
    Public Function predict(model As IFitted, features As Double()()) As Double()
        Dim out(features.Length - 1) As Double

        For i As Integer = 0 To features.Length - 1
            out(i) = model.GetY(features(i))
        Next

        Return out
    End Function

    ''' <summary>预测一张表的每个样本，返回只有一列的预测结果表。</summary>
    Public Function predictTable(model As IFitted, source As NumericTable, Optional name As String = "prediction") As NumericTable
        Dim values As Double() = predict(model, source.features)

        Return NumericTable.FromRows(source.RowNamesOrDefault, values.Select(Function(v) New Double() {v}).ToArray, {name})
    End Function

    ''' <summary>
    ''' 把预测值与（可选的）残差写回表的标签列，返回原表。
    ''' </summary>
    Public Function setPrediction(source As NumericTable, model As IFitted,
                                  Optional withResidual As Boolean = False,
                                  Optional y As String = Nothing) As NumericTable
        Return source.SetPrediction(model, withResidual:=withResidual, y:=y)
    End Function

    ' ======================================================== 指标

    ''' <summary>决定系数 R²。</summary>
    Public Function r2(model As IFitted) As Double
        Return model.R2
    End Function

    ''' <summary>残差平方和（仅 <see cref="FitResult"/> 提供）。</summary>
    Public Function sse(model As FitResult) As Double
        Return model.SSE
    End Function

    ''' <summary>均方根误差（仅 <see cref="FitResult"/> 提供）。</summary>
    Public Function rmse(model As FitResult) As Double
        Return model.RMSE
    End Function

    ''' <summary>回归公式的可读文本。</summary>
    Public Function formula(model As IFitted) As String
        If model.Polynomial Is Nothing Then
            Return $"R2={model.R2}"
        End If

        Return model.Polynomial.ToString
    End Function

    ''' <summary>取标签列：未指定名称时使用第一个标签列。</summary>
    Private Function GetY(source As NumericTable, y As String) As Double()
        If String.IsNullOrEmpty(y) Then
            If source.labelNames Is Nothing OrElse source.labelNames.Length = 0 Then
                Throw New ArgumentException("the fit target table should contains at least one label column", NameOf(source))
            End If

            y = source.labelNames(0)
        End If

        Return source.GetLabel(y)
    End Function
End Module

Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.ElasticNet
Imports Microsoft.VisualBasic.Data.Bootstrapping.Ridge

''' <summary>
''' 回归 / 曲线拟合模块。
''' <para>
''' 所有入口都返回统一的 <see cref="FitModel"/> 门面对象，脚本只需认识一种模型类型；
''' 既提供数组入口（<see cref="linear(Double(), Double())"/> / <see cref="poly(Double(), Double(), Integer)"/>），
''' 也提供表级入口（<see cref="linear(NumericTable, String)"/> / <see cref="ridge"/> / <see cref="elasticNet"/>）。
''' </para>
''' </summary>
Public Module fit

    ' ======================================================== 数组入口

    ''' <summary>一元线性回归 y = a * x + b。</summary>
    Public Function linear(x As Double(), y As Double()) As FitModel
        Return FromLinear(LeastSquares.LinearFit(x, y), "linear")
    End Function

    ''' <summary>带权重的一元线性回归。</summary>
    Public Function linearWeighted(x As Double(), y As Double(), w As Double()) As FitModel
        Return FromWeighted(WeightedLinearRegression.Regress(x, y, w))
    End Function

    ''' <summary>多项式回归 y = a0 + a1*x + ... + an*x^n。</summary>
    Public Function poly(x As Double(), y As Double(), degree As Integer) As FitModel
        Return FromLinear(LeastSquares.PolyFit(x, y, degree), "poly")
    End Function

    ' ======================================================== 表级入口

    ''' <summary>
    ''' 表级一元线性回归：只允许一个特征列，标签列由 <paramref name="y"/> 指定（缺省取第一个标签列）。
    ''' </summary>
    Public Function linear(source As NumericTable, Optional y As String = Nothing) As FitModel
        Return FromLinear(source.LinearFit(y:=y, Silent:=True), "linear")
    End Function

    ''' <summary>表级多项式回归（只允许一个特征列）。</summary>
    Public Function poly(source As NumericTable, degree As Integer, Optional y As String = Nothing) As FitModel
        Return FromLinear(source.PolyFit(degree, y:=y, Silent:=True), "poly")
    End Function

    ''' <summary>
    ''' 岭回归（多元）。<paramref name="lambda"/> 小于 0 时用 GCV 自动选择正则化强度。
    ''' </summary>
    Public Function ridge(source As NumericTable, Optional y As String = Nothing, Optional lambda As Double = -1) As FitModel
        Dim values As Double() = GetY(source, y)
        Dim strength As Double? = If(lambda < 0, CType(Nothing, Double?), lambda)
        Dim fitted As RidgeFit = RidgeFitting.Fit(source.features, values, strength, featureNames:=source.featureNames)

        Return FromRidge(fitted)
    End Function

    ''' <summary>弹性网络回归（alpha 为总正则强度，l1Ratio 为 L1 占比）。</summary>
    Public Function elasticNet(source As NumericTable, alpha As Double, l1Ratio As Double, Optional y As String = Nothing) As FitModel
        Dim values As Double() = GetY(source, y)
        Dim fitted As ElasticNetFit = ElasticNetFitting.Fit(source.features, values, alpha, l1Ratio, featureNames:=source.featureNames)

        Return FromElasticNet(fitted)
    End Function

    ''' <summary>
    ''' 非线性幂律拟合 y = a * Π xi^bi（要求全部数据为正数）。
    ''' </summary>
    Public Function lma(source As NumericTable, Optional y As String = Nothing) As FitModel
        Dim values As Double() = GetY(source, y)
        Dim result As LMA.NonLinearFitResult = source.NonLinearFit(y:=y)
        Dim model As New FitModel With {
            .kind = "power-law",
            .r2 = result.R2,
            .rmse = result.RMSE,
            .sse = result.SSE,
            .formula = result.Formula,
            .residuals = result.Residuals
        }

        model.predictor = Function(row) result.GetY(row)

        Return model
    End Function

    ' ======================================================== 预测

    ''' <summary>对一行特征做预测。</summary>
    Public Function predict(model As FitModel, x As Double()) As Double
        Return model.predict(x)
    End Function

    ''' <summary>对整个特征矩阵逐行预测。</summary>
    Public Function predict(model As FitModel, features As Double()()) As Double()
        Return model.predictAll(features)
    End Function

    ''' <summary>预测一张表的每个样本，返回只有一列的预测结果表。</summary>
    Public Function predictTable(model As FitModel, source As NumericTable, Optional name As String = "prediction") As NumericTable
        Dim values As Double() = model.predictAll(source.features)

        Return NumericTable.FromRows(source.RowNamesOrDefault, values.Select(Function(v) New Double() {v}).ToArray, {name})
    End Function

    ''' <summary>
    ''' 把预测值（以及可选的残差）写回表的标签列，返回原表。
    ''' </summary>
    Public Function setPrediction(source As NumericTable, model As FitModel,
                                  Optional withResidual As Boolean = False,
                                  Optional y As String = Nothing) As NumericTable

        Dim observed As Double() = GetY(source, y)
        Dim predicted As Double() = model.predictAll(source.features)

        Call source.SetLabel("prediction", predicted)

        If withResidual Then
            Dim residual(predicted.Length - 1) As Double

            For i As Integer = 0 To predicted.Length - 1
                residual(i) = predicted(i) - observed(i)
            Next

            Call source.SetLabel("residual", residual)
        End If

        Return source
    End Function

    ' ======================================================== 内部构造

    Private Function FromLinear(fitted As FitResult, kind As String) As FitModel
        Dim model As New FitModel With {
            .kind = kind,
            .r2 = fitted.R_square,
            .rmse = fitted.RMSE,
            .sse = fitted.SSE,
            .slope = fitted.Slope,
            .intercept = fitted.Intercept,
            .residuals = fitted.Residuals,
            .formula = If(fitted.Polynomial Is Nothing, "", fitted.Polynomial.ToString)
        }

        ' FitResult 同时有默认属性 GetY(x As Double) 与接口方法 GetY(ParamArray Double())，
        ' 通过接口引用调用才能拿到"按特征向量求值"的语义
        Dim api As IFitted = fitted

        model.predictor = Function(row) api.GetY(row)

        Return model
    End Function

    Private Function FromWeighted(fitted As WeightedFit) As FitModel
        Dim model As New FitModel With {
            .kind = "weighted",
            .r2 = fitted.CorrelationCoefficient,
            .formula = If(fitted.Polynomial Is Nothing, "", fitted.Polynomial.ToString)
        }
        Dim api As IFitted = fitted

        model.predictor = Function(row) api.GetY(row)

        Return model
    End Function

    Private Function FromRidge(fitted As RidgeFit) As FitModel
        Dim model As New FitModel With {
            .kind = "ridge",
            .r2 = fitted.R2,
            .sse = fitted.SSE,
            .intercept = fitted.BestIntercept,
            .coefficients = fitted.BestCoefficients,
            .featureNames = fitted.featureNames,
            .lambda = fitted.BestLambda,
            .rmse = If(fitted.SSE >= 0, System.Math.Sqrt(fitted.SSE), Double.NaN)
        }

        model.predictor = Function(row)
                              Dim acc As Double = model.intercept
                              Dim b As Double() = model.coefficients

                              For i As Integer = 0 To b.Length - 1
                                  acc += b(i) * row(i)
                              Next

                              Return acc
                          End Function

        Return model
    End Function

    Private Function FromElasticNet(fitted As ElasticNetFit) As FitModel
        Dim model As New FitModel With {
            .kind = "elasticnet",
            .r2 = fitted.R2,
            .intercept = fitted.Intercept,
            .coefficients = fitted.Coefficients,
            .alpha = fitted.Alpha,
            .l1Ratio = fitted.L1Ratio
        }

        model.predictor = Function(row)
                              Dim acc As Double = model.intercept
                              Dim b As Double() = model.coefficients

                              For i As Integer = 0 To b.Length - 1
                                  acc += b(i) * row(i)
                              Next

                              Return acc
                          End Function

        Return model
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

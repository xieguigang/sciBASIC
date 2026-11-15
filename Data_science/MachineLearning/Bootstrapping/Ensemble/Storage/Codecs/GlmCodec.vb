Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports LassoNS = Microsoft.VisualBasic.Data.Bootstrapping.LASSO
Imports LogisticNS = Microsoft.VisualBasic.Data.Bootstrapping.Logistic
Imports MultivariateNS = Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports RidgeNS = Microsoft.VisualBasic.Data.Bootstrapping.Ridge

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 逻辑回归（二分类）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' 预测状态只有三样：标准化系数、截距与标准化参数（均值/标准差）。
    ''' 三者都是小数组，直接内联在清单里即可，不需要 CSV 附表。
    ''' <c>Formula</c> 是抽象类，因此持久化的是 <c>Factors</c> 数组，
    ''' 恢复时重建具体的 <see cref="Polynomial"/>。
    ''' </remarks>
    Public NotInheritable Class LogisticCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "logistic"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is LogisticClassifier
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As LogisticClassifier = DirectCast(model, LogisticClassifier)
            Dim fitted As LogisticNS.LogisticFit = adapter.FittedModel

            If fitted Is Nothing OrElse fitted.Polynomial Is Nothing OrElse fitted.Polynomial.Factors Is Nothing Then
                Throw New InvalidOperationException("the logistic adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("classifier", "base", AlgorithmName)

            manifest.featureWidth = fitted.Polynomial.Factors.Length
            manifest.classes = CType(adapter.Classes.Clone(), Double())
            manifest.SetVector("factors", fitted.Polynomial.Factors)
            manifest.SetScalar("intercept", fitted.Intercept)
            manifest.SetScalar("logLoss", fitted.LogLoss)
            manifest.SetVector("standardizationMeans",
                               If(fitted.Standardization.Means, Array.Empty(Of Double)()))
            manifest.SetVector("standardizationSds",
                               If(fitted.Standardization.Sds, Array.Empty(Of Double)()))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim factors As Double() = manifest.GetVector("factors")
            Dim means As Double() = manifest.GetVector("standardizationMeans")
            Dim sds As Double() = manifest.GetVector("standardizationSds")
            Dim norm As New LogisticNS.Standardization With {
                .Means = If(means.Length = 0, Nothing, means),
                .Sds = If(sds.Length = 0, Nothing, sds)
            }
            Dim restored As New LogisticNS.LogisticFit With {
                .Polynomial = New Polynomial With {.Factors = factors},
                .Intercept = manifest.GetScalar("intercept"),
                .Standardization = norm,
                .LogLoss = manifest.GetScalar("logLoss")
            }

            Return New LogisticClassifier(restored, manifest.classes)
        End Function

    End Class

    ''' <summary>
    ''' 岭回归的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' 预测只用「最优 lambda 步」的截距与系数；其余统计量（R² / GCV / RSS / DF）
    ''' 也一并持久化，以便恢复后的模型在诊断输出上与保存前一致。
    ''' 系数矩阵按「行 = lambda 步、列 = 特征」落 CSV。
    ''' </remarks>
    Public NotInheritable Class RidgeCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "ridge"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is RidgeRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As RidgeRegressor = DirectCast(model, RidgeRegressor)
            Dim fitted As RidgeNS.RidgeFit = adapter.FittedModel

            If fitted Is Nothing OrElse fitted.coefficients Is Nothing Then
                Throw New InvalidOperationException("the ridge adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("regressor", "base", AlgorithmName)

            manifest.featureWidth = If(fitted.coefficients.Length = 0, 0, fitted.coefficients(0).Length)
            manifest.SetInteger("bestIndex", fitted.bestIndex)
            manifest.SetVector("lambdas", fitted.lambdas)
            manifest.SetVector("intercepts", fitted.intercepts)
            manifest.SetVector("rsquared", fitted.rsquared)
            manifest.SetScalar("SSE", fitted.SSE)
            manifest.SetScalar("SST", fitted.SST)
            manifest.SetVector("rss", If(fitted.rss, Array.Empty(Of Double)()))
            manifest.SetVector("df", If(fitted.df, Array.Empty(Of Double)()))
            manifest.SetVector("gcv", If(fitted.gcv, Array.Empty(Of Double)()))

            Dim names As String() = fitted.featureNames

            If names IsNot Nothing AndAlso names.Length > 0 Then
                manifest.SetText("featureNames", names)
            End If

            Call bundle.WriteCsv(manifest, "coefficients", folder,
                                 New String() {"step", "feature", "value"},
                                 EnumerateCoefficients(fitted))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim lambdas As Double() = manifest.GetVector("lambdas")
            Dim intercepts As Double() = manifest.GetVector("intercepts")
            Dim steps As Integer = lambdas.Length

            If intercepts.Length <> steps Then
                Throw New System.IO.InvalidDataException(
                    $"the ridge manifest has {intercepts.Length} intercept(s) for {steps} lambda step(s)!")
            End If

            Dim width As Integer = manifest.featureWidth
            Dim coefficients As Double()() = New Double(steps - 1)() {}

            For k As Integer = 0 To steps - 1
                coefficients(k) = New Double(width - 1) {}
            Next

            Dim table As CsvTable = bundle.ReadCsv(manifest, "coefficients")
            Dim cellSteps As Integer() = table.Integers("step", "ridge/coefficients")
            Dim cellFeatures As Integer() = table.Integers("feature", "ridge/coefficients")
            Dim cellValues As Double() = table.Doubles("value", "ridge/coefficients")

            For i As Integer = 0 To table.RowCount - 1
                coefficients(cellSteps(i))(cellFeatures(i)) = cellValues(i)
            Next

            Dim names As String() = Nothing

            If manifest.texts IsNot Nothing Then
                Call manifest.texts.TryGetValue("featureNames", names)
            End If

            Dim restored As New RidgeNS.RidgeFit With {
                .lambdas = lambdas,
                .intercepts = intercepts,
                .coefficients = coefficients,
                .rsquared = manifest.GetVector("rsquared"),
                .rss = manifest.GetVector("rss"),
                .df = manifest.GetVector("df"),
                .gcv = manifest.GetVector("gcv"),
                .bestIndex = manifest.GetInteger("bestIndex"),
                .featureNames = names,
                .SSE = manifest.GetScalar("SSE"),
                .SST = manifest.GetScalar("SST")
            }

            Return New RidgeRegressor(restored)
        End Function

        Private Shared Iterator Function EnumerateCoefficients(fitted As RidgeNS.RidgeFit) As IEnumerable(Of String())
            For k As Integer = 0 To fitted.coefficients.Length - 1
                Dim row As Double() = fitted.coefficients(k)

                If row Is Nothing Then
                    Continue For
                End If

                For j As Integer = 0 To row.Length - 1
                    Yield New String() {CStr(k), CStr(j), ModelFormat.Number(row(j))}
                Next
            Next
        End Function

    End Class

    ''' <summary>
    ''' 多元线性回归（无截距）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' 模型本体就是 <c>beta</c> 系数向量，直接内联在清单里。
    ''' </remarks>
    Public NotInheritable Class MlrCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "mlr"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is MlrRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As MlrRegressor = DirectCast(model, MlrRegressor)
            Dim fitted As MultivariateNS.MLRFit = adapter.FittedModel

            If fitted Is Nothing OrElse fitted.beta Is Nothing Then
                Throw New InvalidOperationException("the mlr adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("regressor", "base", AlgorithmName)

            manifest.featureWidth = fitted.beta.Length
            manifest.SetVector("beta", fitted.beta)
            manifest.SetInteger("N", fitted.N)
            manifest.SetInteger("p", fitted.p)
            manifest.SetScalar("SSE", fitted.SSE)
            manifest.SetScalar("SST", fitted.SST)

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim restored As New MultivariateNS.MLRFit With {
                .beta = manifest.GetVector("beta"),
                .N = manifest.GetInteger("N"),
                .p = manifest.GetInteger("p"),
                .SSE = manifest.GetScalar("SSE"),
                .SST = manifest.GetScalar("SST")
            }

            Return New MlrRegressor(restored)
        End Function

    End Class

    ''' <summary>
    ''' LASSO 的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <c>compressedWeights</c> 是「压缩存贮」的正则化路径：每步只有前
    ''' <c>numberOfWeights(k)</c> 项有效，其余为 0。因此 CSV 只写有效项
    ''' （step, position, value），既省空间又保持可读；<c>indices</c> 作为
    ''' 解压指针放在清单里。
    ''' </remarks>
    Public NotInheritable Class LassoCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "lasso"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is LassoRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As LassoRegressor = DirectCast(model, LassoRegressor)
            Dim fitted As LassoNS.LassoFit = adapter.FittedModel

            If fitted Is Nothing OrElse fitted.compressedWeights Is Nothing Then
                Throw New InvalidOperationException("the lasso adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("regressor", "base", AlgorithmName)

            manifest.featureWidth = fitted.numFeatures
            manifest.SetInteger("numberOfLambdas", fitted.numberOfLambdas)
            manifest.SetInteger("selectedStep", adapter.LambdaIndex)
            manifest.SetInteger("numberOfPasses", fitted.numberOfPasses)
            manifest.SetVector("intercepts", fitted.intercepts)
            manifest.SetVector("lambdas", fitted.lambdas)
            manifest.SetVector("rsquared", fitted.rsquared)
            manifest.SetIntegerVector("numberOfWeights", fitted.numberOfWeights)
            manifest.SetIntegerVector("nonZeroWeights", fitted.nonZeroWeights)
            manifest.SetIntegerVector("indices", fitted.indices)

            If fitted.featureNames IsNot Nothing AndAlso fitted.featureNames.Length > 0 Then
                manifest.SetText("featureNames", fitted.featureNames)
            End If

            Call bundle.WriteCsv(manifest, "compressed-weights", folder,
                                 New String() {"step", "position", "value"},
                                 EnumerateCompressedWeights(fitted))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim steps As Integer = manifest.GetInteger("numberOfLambdas")
            Dim width As Integer = manifest.featureWidth
            Dim numberOfWeights As Integer() = manifest.GetIntegerVector("numberOfWeights")
            Dim compressed As Double()() = New Double(steps - 1)() {}

            For k As Integer = 0 To steps - 1
                compressed(k) = New Double(System.Math.Max(1, numberOfWeights(k)) - 1) {}
            Next

            Dim table As CsvTable = bundle.ReadCsv(manifest, "compressed-weights")
            Dim cellSteps As Integer() = table.Integers("step", "lasso/compressed-weights")
            Dim cellPositions As Integer() = table.Integers("position", "lasso/compressed-weights")
            Dim cellValues As Double() = table.Doubles("value", "lasso/compressed-weights")

            For i As Integer = 0 To table.RowCount - 1
                Dim stepIndex As Integer = cellSteps(i)
                Dim position As Integer = cellPositions(i)

                If stepIndex < 0 OrElse stepIndex >= steps Then
                    Throw New System.IO.InvalidDataException(
                        $"the lasso weight row[{i}] has an out-of-range step ({stepIndex})!")
                End If
                If position < 0 OrElse position >= numberOfWeights(stepIndex) Then
                    Throw New System.IO.InvalidDataException(
                        $"the lasso weight row[{i}] has an out-of-range position ({position}) " &
                        $"for step {stepIndex} (expected < {numberOfWeights(stepIndex)})!")
                End If

                compressed(stepIndex)(position) = cellValues(i)
            Next

            Dim names As String() = Nothing

            If manifest.texts IsNot Nothing Then
                Call manifest.texts.TryGetValue("featureNames", names)
            End If

            Dim restored As LassoNS.LassoFit = LassoNS.LassoFit.Restore(
                steps,
                width,
                manifest.GetVector("intercepts"),
                compressed,
                manifest.GetIntegerVector("indices"),
                numberOfWeights,
                manifest.GetIntegerVector("nonZeroWeights"),
                manifest.GetVector("lambdas"),
                manifest.GetVector("rsquared"),
                names,
                manifest.GetInteger("numberOfPasses"))

            Return New LassoRegressor(restored, manifest.GetInteger("selectedStep"))
        End Function

        Private Shared Iterator Function EnumerateCompressedWeights(fitted As LassoNS.LassoFit) As IEnumerable(Of String())
            For k As Integer = 0 To fitted.numberOfLambdas - 1
                Dim row As Double() = fitted.compressedWeights(k)

                If row Is Nothing Then
                    Continue For
                End If

                For j As Integer = 0 To fitted.numberOfWeights(k) - 1
                    Yield New String() {CStr(k), CStr(j), ModelFormat.Number(row(j))}
                Next
            Next
        End Function

    End Class

End Namespace

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier
Imports BayesRegNS = Microsoft.VisualBasic.MachineLearning.BayesianRegression
Imports NbModel = Microsoft.VisualBasic.DataMining.Kernel.Classifier.Bayesian
Imports GaussianModel = Microsoft.VisualBasic.MachineLearning.BayesianRegression.GaussianBayes

Namespace Ensemble.Persistence

    ''' <summary>
    ''' 朴素贝叶斯分类器（离散频率表路径）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 条件频率表是模型的主体，以**长表**（class, feature, key, count）落在 CSV 里：
    ''' 相比把 <c>Dictionary(Of Long, Integer)</c> 直接塞进 JSON，长表可以被
    ''' <c>awk</c> / Excel / pandas 直接读取与比对，也不会因为键是数字字符串
    ''' 而产生跨语言歧义。
    ''' </para>
    ''' <para>
    ''' 先验、类别计数与「每个维度的不同取值个数」都是小数组，直接内联在清单里。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NaiveBayesCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "naive-bayes"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is NaiveBayesClassifier
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As NaiveBayesClassifier = DirectCast(model, NaiveBayesClassifier)
            Dim fitted As NbModel = adapter.FittedModel

            If fitted Is Nothing Then
                Throw New InvalidOperationException("the naive bayes adapter holds no fitted model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("classifier", "base", AlgorithmName)

            manifest.featureWidth = fitted.FeatureWidth
            manifest.classes = CType(adapter.Classes.Clone(), Double())
            manifest.SetIntegerVector("modelClasses", fitted.Classes)
            manifest.SetVector("logPriors", fitted.LogPriors)
            manifest.SetIntegerVector("classCounts", fitted.ClassCounts)
            manifest.SetIntegerVector("distinct", fitted.DistinctCounts)
            manifest.SetInteger("sampleSize", fitted.SampleSize)
            manifest.SetScalar("smoothing", fitted.Smoothing)

            Call bundle.WriteCsv(manifest, "tables", folder,
                                 New String() {"class", "feature", "key", "count"},
                                 EnumerateTables(fitted))

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            If manifest.classes Is Nothing OrElse manifest.classes.Length = 0 Then
                Throw New System.IO.InvalidDataException(
                    "the naive bayes manifest carries no class labels!")
            End If

            Dim modelClasses As Integer() = manifest.GetIntegerVector("modelClasses")
            Dim width As Integer = manifest.featureWidth
            Dim classCount As Integer = modelClasses.Length
            Dim table As CsvTable = bundle.ReadCsv(manifest, "tables")

            Dim tables As Dictionary(Of Long, Integer)() = New Dictionary(Of Long, Integer)(classCount * width - 1) {}

            For i As Integer = 0 To tables.Length - 1
                tables(i) = New Dictionary(Of Long, Integer)()
            Next

            Dim classes As Integer() = table.Integers("class", "naive-bayes/tables")
            Dim features As Integer() = table.Integers("feature", "naive-bayes/tables")
            Dim keys As String() = table.Texts("key")
            Dim counts As Integer() = table.Integers("count", "naive-bayes/tables")

            For i As Integer = 0 To table.RowCount - 1
                Dim classIndex As Integer = classes(i)
                Dim featureIndex As Integer = features(i)

                If classIndex < 0 OrElse classIndex >= classCount Then
                    Throw New System.IO.InvalidDataException(
                        $"the naive bayes table row[{i}] has an out-of-range class index ({classIndex})!")
                End If
                If featureIndex < 0 OrElse featureIndex >= width Then
                    Throw New System.IO.InvalidDataException(
                        $"the naive bayes table row[{i}] has an out-of-range feature index ({featureIndex})!")
                End If

                Dim key As Long = CLng(ModelFormat.ParseNumber(keys(i), $"naive-bayes/tables row[{i}] key"))
                Dim slot As Integer = classIndex * width + featureIndex

                tables(slot)(key) = counts(i)
            Next

            Dim restored As NbModel = NbModel.Restore(
                modelClasses,
                manifest.GetVector("logPriors"),
                manifest.GetIntegerVector("classCounts"),
                width,
                manifest.GetInteger("sampleSize"),
                tables,
                manifest.GetIntegerVector("distinct"),
                manifest.GetScalar("smoothing"))

            Return New NaiveBayesClassifier(restored, manifest.classes)
        End Function

        Private Shared Iterator Function EnumerateTables(fitted As NbModel) As IEnumerable(Of String())
            Dim width As Integer = fitted.FeatureWidth
            Dim classes As Integer() = fitted.Classes

            For k As Integer = 0 To classes.Length - 1
                For j As Integer = 0 To width - 1
                    Dim counts As Dictionary(Of Long, Integer) = fitted.Tables(k * width + j)

                    For Each item As KeyValuePair(Of Long, Integer) In counts
                        Yield New String() {
                            CStr(k),
                            CStr(j),
                            ModelFormat.Number(CDbl(item.Key)),
                            CStr(item.Value)
                        }
                    Next
                Next
            Next
        End Function

    End Class

    ''' <summary>
    ''' 贝叶斯回归（目标分箱 + 后验期望）的编解码器。
    ''' </summary>
    ''' <remarks>
    ''' 该模型由三部分状态组成，缺一不可：
    ''' <list type="number">
    ''' <item>目标分箱器（边界、箱宽、代表值）；</item>
    ''' <item>底层朴素贝叶斯分类器（离散频率表或高斯均值/方差）；</item>
    ''' <item>箱编号与代表值的映射关系。</item>
    ''' </list>
    ''' 其中 (2) 的数据量最大，同样以 CSV 长表承载。
    ''' </remarks>
    Public NotInheritable Class BayesianRegressionCodec
        Implements IModelCodec

        ''' <summary>算法标识</summary>
        Public Const AlgorithmName As String = "bayesian-regression"

        Private Const PosteriorKey As String = "posterior"

        Public ReadOnly Property Algorithm As String Implements IModelCodec.Algorithm
            Get
                Return AlgorithmName
            End Get
        End Property

        Public Function CanHandle(model As Object) As Boolean Implements IModelCodec.CanHandle
            Return TypeOf model Is BayesianRegressor
        End Function

        Public Function SaveModel(model As Object, bundle As ModelBundle, folder As String) As ModelManifest Implements IModelCodec.SaveModel
            Dim adapter As BayesianRegressor = DirectCast(model, BayesianRegressor)
            Dim fitted As BayesRegNS.BayesianRegression = adapter.FittedModel

            If fitted Is Nothing OrElse Not fitted.IsTrained Then
                Throw New InvalidOperationException("the bayesian regression adapter holds no trained model!")
            End If

            Dim manifest As ModelManifest = ModelManifest.Create("regressor", "base", AlgorithmName)
            Dim binning As BayesRegNS.TargetBinning = fitted.Binning

            manifest.featureWidth = fitted.FeatureWidth
            manifest.SetParameter("mode", fitted.Mode.ToString())
            manifest.SetInteger("bins", fitted.Bins)
            manifest.SetScalar("smoothing", fitted.Smoothing)
            manifest.SetScalar("varianceFloor", fitted.VarianceFloor)
            manifest.SetIntegerVector("modelClasses", fitted.Classes)
            manifest.SetVector("representatives", fitted.Representatives)

            manifest.SetScalar("binMinimum", binning.Minimum)
            manifest.SetScalar("binMaximum", binning.Maximum)
            manifest.SetScalar("binInterval", binning.Interval)
            manifest.SetInteger("binCount", binning.BinCount)

            Call SavePosterior(fitted.PosteriorModel, manifest, bundle, folder)

            Return manifest
        End Function

        Public Function LoadModel(manifest As ModelManifest, bundle As ModelBundle) As Object Implements IModelCodec.LoadModel
            Dim mode As BayesRegNS.BayesFeatureMode

            If Not [Enum].TryParse(manifest.GetParameter("mode"), mode) Then
                Throw New System.IO.InvalidDataException(
                    $"the bayesian regression manifest has an invalid feature mode ('{manifest.GetParameter("mode")}')!")
            End If

            Dim binning As BayesRegNS.TargetBinning = BayesRegNS.TargetBinning.Restore(
                manifest.GetScalar("binMinimum"),
                manifest.GetScalar("binMaximum"),
                manifest.GetScalar("binInterval"),
                manifest.GetInteger("binCount"),
                manifest.GetVector("representatives"))

            Dim posterior As NaiveBayesModel = LoadPosterior(manifest, bundle)

            Dim restored As BayesRegNS.BayesianRegression = BayesRegNS.BayesianRegression.Restore(
                mode,
                manifest.GetInteger("bins"),
                manifest.GetScalar("smoothing"),
                manifest.GetScalar("varianceFloor"),
                binning,
                posterior,
                manifest.GetIntegerVector("modelClasses"),
                manifest.GetVector("representatives"))

            Return New BayesianRegressor(restored)
        End Function

        Private Shared Sub SavePosterior(posterior As NaiveBayesModel, manifest As ModelManifest,
                                         bundle As ModelBundle, folder As String)
            If posterior Is Nothing Then
                Throw New InvalidOperationException("the bayesian regression model has no posterior model!")
            End If

            Dim discrete As NbModel = TryCast(posterior, NbModel)

            If discrete IsNot Nothing Then
                manifest.SetParameter(PosteriorKey, "discrete")
                manifest.SetVector("posteriorLogPriors", discrete.LogPriors)
                manifest.SetIntegerVector("posteriorClassCounts", discrete.ClassCounts)
                manifest.SetIntegerVector("posteriorDistinct", discrete.DistinctCounts)
                manifest.SetInteger("posteriorSampleSize", discrete.SampleSize)
                manifest.SetScalar("posteriorSmoothing", discrete.Smoothing)

                Call bundle.WriteCsv(manifest, "posterior-tables", folder,
                                     New String() {"class", "feature", "key", "count"},
                                     EnumerateDiscreteTables(discrete))

                Return
            End If

            Dim gaussian As GaussianModel = TryCast(posterior, GaussianModel)

            If gaussian Is Nothing Then
                Throw New NotSupportedException(
                    $"the posterior model type '{posterior.GetType().FullName}' can not be persisted!")
            End If

            manifest.SetParameter(PosteriorKey, "continuous")
            manifest.SetVector("posteriorLogPriors", gaussian.LogPriors)
            manifest.SetIntegerVector("posteriorClassCounts", gaussian.ClassCounts)
            manifest.SetInteger("posteriorSampleSize", gaussian.SampleSize)
            manifest.SetScalar("posteriorVarianceFloor", gaussian.VarianceFloor)

            Call bundle.WriteCsv(manifest, "posterior-gaussian", folder,
                                 New String() {"class", "feature", "mean", "variance"},
                                 EnumerateGaussian(gaussian))
        End Sub

        Private Shared Function LoadPosterior(manifest As ModelManifest, bundle As ModelBundle) As NaiveBayesModel
            Dim posterior As String = manifest.GetParameter(PosteriorKey)
            Dim width As Integer = manifest.featureWidth
            Dim modelClasses As Integer() = manifest.GetIntegerVector("modelClasses")

            Select Case posterior
                Case "discrete"
                    Dim table As CsvTable = bundle.ReadCsv(manifest, "posterior-tables")
                    Dim classCount As Integer = modelClasses.Length
                    Dim tables As Dictionary(Of Long, Integer)() =
                        New Dictionary(Of Long, Integer)(classCount * width - 1) {}

                    For i As Integer = 0 To tables.Length - 1
                        tables(i) = New Dictionary(Of Long, Integer)()
                    Next

                    Dim classes As Integer() = table.Integers("class", "bayesian-regression/posterior-tables")
                    Dim features As Integer() = table.Integers("feature", "bayesian-regression/posterior-tables")
                    Dim keys As String() = table.Texts("key")
                    Dim counts As Integer() = table.Integers("count", "bayesian-regression/posterior-tables")

                    For i As Integer = 0 To table.RowCount - 1
                        If classes(i) < 0 OrElse classes(i) >= classCount Then
                            Throw New System.IO.InvalidDataException(
                                $"the bayesian regression posterior table row[{i}] " &
                                $"has an out-of-range class index ({classes(i)})!")
                        End If
                        If features(i) < 0 OrElse features(i) >= width Then
                            Throw New System.IO.InvalidDataException(
                                $"the bayesian regression posterior table row[{i}] " &
                                $"has an out-of-range feature index ({features(i)})!")
                        End If

                        Dim key As Long = CLng(ModelFormat.ParseNumber(
                            keys(i), $"bayesian-regression/posterior-tables row[{i}] key"))

                        tables(classes(i) * width + features(i))(key) = counts(i)
                    Next

                    Return NbModel.Restore(
                        modelClasses,
                        manifest.GetVector("posteriorLogPriors"),
                        manifest.GetIntegerVector("posteriorClassCounts"),
                        width,
                        manifest.GetInteger("posteriorSampleSize"),
                        tables,
                        manifest.GetIntegerVector("posteriorDistinct"),
                        manifest.GetScalar("posteriorSmoothing"))

                Case "continuous"
                    Dim table As CsvTable = bundle.ReadCsv(manifest, "posterior-gaussian")
                    Dim classCount As Integer = modelClasses.Length
                    Dim means As Double()() = New Double(classCount - 1)() {}
                    Dim variances As Double()() = New Double(classCount - 1)() {}

                    For k As Integer = 0 To classCount - 1
                        means(k) = New Double(width - 1) {}
                        variances(k) = New Double(width - 1) {}
                    Next

                    Dim classes As Integer() = table.Integers("class", "bayesian-regression/posterior-gaussian")
                    Dim features As Integer() = table.Integers("feature", "bayesian-regression/posterior-gaussian")
                    Dim cellMeans As Double() = table.Doubles("mean", "bayesian-regression/posterior-gaussian")
                    Dim cellVariances As Double() = table.Doubles("variance", "bayesian-regression/posterior-gaussian")

                    For i As Integer = 0 To table.RowCount - 1
                        If classes(i) < 0 OrElse classes(i) >= classCount Then
                            Throw New System.IO.InvalidDataException(
                                $"the bayesian regression gaussian table row[{i}] " &
                                $"has an out-of-range class index ({classes(i)})!")
                        End If
                        If features(i) < 0 OrElse features(i) >= width Then
                            Throw New System.IO.InvalidDataException(
                                $"the bayesian regression gaussian table row[{i}] " &
                                $"has an out-of-range feature index ({features(i)})!")
                        End If

                        means(classes(i))(features(i)) = cellMeans(i)
                        variances(classes(i))(features(i)) = cellVariances(i)
                    Next

                    Return GaussianModel.Restore(
                        modelClasses,
                        manifest.GetVector("posteriorLogPriors"),
                        manifest.GetIntegerVector("posteriorClassCounts"),
                        width,
                        manifest.GetInteger("posteriorSampleSize"),
                        means,
                        variances,
                        manifest.GetScalar("posteriorVarianceFloor"))

                Case Else
                    Throw New System.IO.InvalidDataException(
                        $"unsupported bayesian regression posterior model '{posterior}'!")
            End Select
        End Function

        Private Shared Iterator Function EnumerateDiscreteTables(fitted As NbModel) As IEnumerable(Of String())
            Dim width As Integer = fitted.FeatureWidth

            For k As Integer = 0 To fitted.Classes.Length - 1
                For j As Integer = 0 To width - 1
                    For Each item As KeyValuePair(Of Long, Integer) In fitted.Tables(k * width + j)
                        Yield New String() {
                            CStr(k),
                            CStr(j),
                            ModelFormat.Number(CDbl(item.Key)),
                            CStr(item.Value)
                        }
                    Next
                Next
            Next
        End Function

        Private Shared Iterator Function EnumerateGaussian(fitted As GaussianModel) As IEnumerable(Of String())
            Dim width As Integer = fitted.FeatureWidth

            For k As Integer = 0 To fitted.ClassCount - 1
                For j As Integer = 0 To width - 1
                    Yield New String() {
                        CStr(k),
                        CStr(j),
                        ModelFormat.Number(fitted.Means(k)(j)),
                        ModelFormat.Number(fitted.Variances(k)(j))
                    }
                Next
            Next
        End Function

    End Class

End Namespace

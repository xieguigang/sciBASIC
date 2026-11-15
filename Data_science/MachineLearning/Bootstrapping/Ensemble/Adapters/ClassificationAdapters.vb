Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports LogisticNS = Microsoft.VisualBasic.Data.Bootstrapping.Logistic
Imports SvmModelType = Microsoft.VisualBasic.MachineLearning.SVM.Model

Namespace Ensemble

    ''' <summary>
    ''' 朴素贝叶斯分类器的集成适配器。
    ''' </summary>
    ''' <remarks>
    ''' <c>Bayesian.Posterior</c> 的顺序等于 <c>Bayesian.Classes</c>（升序类别码），
    ''' 而 Bagging 子集可能缺失某些类别，因此这里始终通过
    ''' <c>ProbabilityMath.AlignTo</c> 对齐到集成层的全局 <c>Classes</c>。
    ''' </remarks>
    Public NotInheritable Class NaiveBayesClassifier
        Implements IClassifier
        Implements IFeatureSchema

        Private ReadOnly fitted As Bayesian
        Private ReadOnly classesArray As Double()

        Public Sub New(fitted As Bayesian, classes As Double())
            Me.fitted = fitted
            Me.classesArray = classes
        End Sub

        ''' <summary>底层已训练的朴素贝叶斯模型（供持久化/诊断）</summary>
        Public ReadOnly Property FittedModel As Bayesian
            Get
                Return fitted
            End Get
        End Property

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        ''' <summary>训练期的特征维度</summary>
        Public ReadOnly Property FeatureWidth As Integer Implements IFeatureSchema.FeatureWidth
            Get
                Return If(fitted Is Nothing, 0, fitted.FeatureWidth)
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Dim posterior As Double() = fitted.Posterior(x)
            Dim sourceClasses As Double() = fitted.Classes.Select(Function(c) CDbl(c)).ToArray()

            Return ProbabilityMath.Normalize(
                ProbabilityMath.AlignTo(posterior, sourceClasses, classesArray))
        End Function

    End Class

    ''' <summary>朴素贝叶斯分类基学习器</summary>
    Public NotInheritable Class NaiveBayesClassifierLearner : Implements IClassifierLearner

        ''' <summary>加性平滑系数（Laplace 平滑，默认 1.0）</summary>
        Public Property Smoothing As Double = 1.0R

        Public ReadOnly Property Name As String Implements IClassifierLearner.Name
            Get
                Return "naive-bayes"
            End Get
        End Property

        Public Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier Implements IClassifierLearner.Fit
            Dim fitted As Bayesian = Bayesian.Load(x, yIndex, Smoothing)

            Return New NaiveBayesClassifier(fitted, classes)
        End Function

    End Class

    ''' <summary>
    ''' 逻辑回归分类适配器（仅二分类）。
    ''' </summary>
    ''' <remarks>
    ''' <c>LogisticFit.GetY</c> 返回 <c>P(y=1|x)</c>，这里展开成
    ''' <c>[1-p, p]</c> 以符合集成层的概率向量契约。
    ''' </remarks>
    Public NotInheritable Class LogisticClassifier
        Implements IClassifier
        Implements IFeatureSchema

        Private ReadOnly fitted As LogisticNS.LogisticFit
        Private ReadOnly classesArray As Double()

        Public Sub New(fitted As LogisticNS.LogisticFit, classes As Double())
            Me.fitted = fitted
            Me.classesArray = classes
        End Sub

        ''' <summary>底层已训练的逻辑回归模型（供持久化/诊断）</summary>
        Public ReadOnly Property FittedModel As LogisticNS.LogisticFit
            Get
                Return fitted
            End Get
        End Property

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        ''' <summary>训练期的特征维度（等于标准化空间中的系数个数）</summary>
        Public ReadOnly Property FeatureWidth As Integer Implements IFeatureSchema.FeatureWidth
            Get
                If fitted Is Nothing OrElse fitted.Polynomial Is Nothing OrElse fitted.Polynomial.Factors Is Nothing Then
                    Return 0
                End If

                Return fitted.Polynomial.Factors.Length
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Dim p As Double = fitted.GetY(x)

            If Double.IsNaN(p) OrElse p < 0.0R OrElse p > 1.0R Then
                Throw New InvalidOperationException($"the logistic model returned an invalid probability ({p})!")
            End If

            Return New Double() {1.0R - p, p}
        End Function

    End Class

    ''' <summary>逻辑回归基学习器（仅二分类，多分类请使用朴素贝叶斯或 SVM）</summary>
    Public NotInheritable Class LogisticClassifierLearner : Implements IClassifierLearner

        ''' <summary>初始学习率（逻辑回归内部会自适应放大）</summary>
        Public Property Rate As Double = 0.1R
        ''' <summary>最大迭代次数</summary>
        Public Property Iterations As Integer = 2000
        ''' <summary>收敛容差</summary>
        Public Property Tolerance As Double = 0.0000001R

        Public ReadOnly Property Name As String Implements IClassifierLearner.Name
            Get
                Return "logistic"
            End Get
        End Property

        Public Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier Implements IClassifierLearner.Fit
            If classes.Length <> 2 Then
                Throw New NotSupportedException(
                    $"the logistic adapter only supports binary classification, but got {classes.Length} class(es); " &
                    "use naive bayes or SVM for multi-class tasks!")
            End If

            Dim instances As LogisticNS.Instance() = New LogisticNS.Instance(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                ' 逻辑回归不会修改输入特征，这里直接复用行数组以避免额外拷贝
                instances(i) = New LogisticNS.Instance(0, x(i))
                instances(i).label = yIndex(i)
            Next

            Dim model As New LogisticNS.Logistic(x(0).Length, Rate) With {
                .ITERATIONS = Iterations,
                .Tolerance = Tolerance
            }
            Dim fitted As LogisticNS.LogisticFit = model.train(instances)

            Return New LogisticClassifier(fitted, classes)
        End Function

    End Class

    ''' <summary>
    ''' SVM 分类适配器（<c>C_SVC</c>），概率来自
    ''' <c>svm_predict_probability</c>，顺序由 <c>model.classLabels</c> 决定。
    ''' </summary>
    Public NotInheritable Class SvmClassifier
        Implements IClassifier
        Implements IFeatureSchema

        Private ReadOnly fitted As SvmModelType
        Private ReadOnly classesArray As Double()
        Private ReadOnly labelOffset As Integer
        Private ReadOnly featureWidthValue As Integer

        ''' <param name="fitted">已训练的 SVM 模型</param>
        ''' <param name="classes">全局类别顺序</param>
        ''' <param name="labelOffset">特征与标签的索引基</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知</param>
        Public Sub New(fitted As SvmModelType, classes As Double(), labelOffset As Integer,
                       Optional featureWidth As Integer = 0)
            Me.fitted = fitted
            Me.classesArray = classes
            Me.labelOffset = labelOffset
            Me.featureWidthValue = System.Math.Max(0, featureWidth)
        End Sub

        ''' <summary>底层已训练的 SVM 模型（供持久化/诊断）</summary>
        Public ReadOnly Property FittedModel As SvmModelType
            Get
                Return fitted
            End Get
        End Property

        ''' <summary>类别标签的索引基（持久化恢复时必须一并保存）</summary>
        Public ReadOnly Property IndexOffset As Integer
            Get
                Return labelOffset
            End Get
        End Property

        ''' <summary>训练期的特征维度；<c>0</c> 表示未知</summary>
        Public ReadOnly Property FeatureWidth As Integer Implements IFeatureSchema.FeatureWidth
            Get
                Return featureWidthValue
            End Get
        End Property

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Dim probability As Double() = fitted.PredictProbability(SvmAdapter.ToNodes(x, labelOffset))
            Dim labels As Integer() = fitted.classLabels

            If labels Is Nothing OrElse labels.Length <> probability.Length Then
                Throw New InvalidOperationException(
                    "the SVM model does not carry class labels; retrain it with Parameter.probability = True!")
            End If

            Dim result As Double() = New Double(classesArray.Length - 1) {}

            For i As Integer = 0 To probability.Length - 1
                Dim slot As Integer = labels(i) - labelOffset

                If slot >= 0 AndAlso slot < result.Length Then
                    result(slot) = probability(i)
                End If
            Next

            Return ProbabilityMath.Normalize(result)
        End Function

    End Class

    ''' <summary>供 SVM 分类/回归共用的输入与标签转换</summary>
    Public NotInheritable Class SvmAdapter

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 把稠密特征转成 SVM 的稀疏 <c>Node()</c>；<paramref name="offset"/> 为索引基，
        ''' 训练与预测必须使用同一个值（官方示例为 1 基）。
        ''' </summary>
        Friend Shared Function ToNodes(x As Double(), offset As Integer) As Node()
            Dim nodes As Node() = New Node(x.Length - 1) {}

            For j As Integer = 0 To x.Length - 1
                nodes(j) = New Node(j + offset, x(j))
            Next

            Return nodes
        End Function

        ''' <summary>
        ''' 构造 <c>Problem</c>：占位标签走默认编码，随后用真实标签覆盖 <c>Y</c>，
        ''' 避免 <c>ClassEncoder</c> 把连续目标改写成 0..n-1 的序号。
        ''' </summary>
        Friend Shared Function CreateProblem(x As Double()(), labels As Double(),
                                            featureOffset As Integer, labelOffset As Integer) As Problem
            Dim nodes As Node()() = New Node(x.Length - 1)() {}
            Dim placeholder As String() = New String(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                nodes(i) = ToNodes(x(i), featureOffset)
                placeholder(i) = CStr(i)
            Next

            Dim problem As New Problem(placeholder, nodes, x(0).Length)
            Dim colored As ColorClass() = New ColorClass(labels.Length - 1) {}

            For i As Integer = 0 To labels.Length - 1
                ' labelOffset = 0 表示回归（保留真实目标值）；
                ' labelOffset = 特征索引基 表示分类（标签值 = 类别索引 + 偏移）
                colored(i) = New ColorClass With {
                    .factor = labels(i) + labelOffset,
                    .name = CStr(labels(i))
                }
            Next

            problem.Y = colored

            Return problem
        End Function

    End Class

    ''' <summary>SVM 分类基学习器</summary>
    Public NotInheritable Class SvmClassifierLearner : Implements IClassifierLearner

        ''' <summary>惩罚系数 <c>C</c></summary>
        Public Property C As Double = 1.0R
        ''' <summary>RBF 核的 <c>gamma</c>；小于等于 0 时自动取 <c>1 / 特征数</c></summary>
        Public Property Gamma As Double = 0.0R
        ''' <summary>特征索引基，默认 1（与官方示例一致）</summary>
        Public Property IndexOffset As Integer = 1

        Public ReadOnly Property Name As String Implements IClassifierLearner.Name
            Get
                Return "svm"
            End Get
        End Property

        Public Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier Implements IClassifierLearner.Fit
            Dim offset As Integer = IndexOffset
            Dim labels As Double() = New Double(yIndex.Length - 1) {}

            For i As Integer = 0 To yIndex.Length - 1
                labels(i) = yIndex(i)
            Next

            ' 标签值 = 类别索引 + offset，预测期用 model.classLabels 反解回 0..K-1
            Dim problem As Problem = SvmAdapter.CreateProblem(x, labels, offset, offset)
            Dim gamma As Double = If(Gamma > 0.0R, Gamma, 1.0R / x(0).Length)
            Dim parameter As New Parameter With {
                .svmType = SvmType.C_SVC,
                .kernelType = KernelType.RBF,
                .c = C,
                .gamma = gamma,
                .probability = True,
                .cacheSize = 100
            }
            Dim fitted As SvmModelType = Training.Train(problem, parameter)

            Return New SvmClassifier(fitted, classes, offset, x(0).Length)
        End Function

    End Class

    ''' <summary>
    ''' XGBoost 分类适配器（仅二分类，<c>binary:logistic</c> 输出单值概率）。
    ''' </summary>
    Public NotInheritable Class XGBoostClassifier
        Implements IClassifier
        Implements IFeatureSchema

        Private ReadOnly fitted As XGBoostModel
        Private ReadOnly classesArray As Double()

        Public Sub New(fitted As XGBoostModel, classes As Double())
            Me.fitted = fitted
            Me.classesArray = classes
        End Sub

        ''' <summary>底层已训练的 XGBoost 模型（供持久化/诊断）</summary>
        Public ReadOnly Property FittedModel As XGBoostModel
            Get
                Return fitted
            End Get
        End Property

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        ''' <summary>训练期的特征维度</summary>
        Public ReadOnly Property FeatureWidth As Integer Implements IFeatureSchema.FeatureWidth
            Get
                Return If(fitted Is Nothing, 0, fitted.FeatureWidth)
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Dim p As Double = fitted.PredictProbability(x)

            Return New Double() {1.0R - p, p}
        End Function

    End Class

    ''' <summary>XGBoost 分类基学习器（仅二分类）</summary>
    Public NotInheritable Class XGBoostClassifierLearner : Implements IClassifierLearner

        ''' <summary>提升轮数</summary>
        Public Property Rounds As Integer = 30
        ''' <summary>树最大深度</summary>
        Public Property MaxDepth As Integer = 6
        ''' <summary>随机种子</summary>
        Public Property Seed As Integer = 42

        Public ReadOnly Property Name As String Implements IClassifierLearner.Name
            Get
                Return "xgboost"
            End Get
        End Property

        Public Function Fit(x As Double()(), yIndex As Integer(), classes As Double()) As IClassifier Implements IClassifierLearner.Fit
            If classes.Length <> 2 Then
                Throw New NotSupportedException(
                    $"the xgboost adapter only supports binary classification, but got {classes.Length} class(es)!")
            End If

            Dim labels As Double() = New Double(yIndex.Length - 1) {}

            For i As Integer = 0 To yIndex.Length - 1
                labels(i) = yIndex(i)
            Next

            Dim fitted As XGBoostModel = XGBoostModel.Fit(x, labels, "logloss", Rounds, MaxDepth, Seed)

            Return New XGBoostClassifier(fitted, classes)
        End Function

    End Class

End Namespace

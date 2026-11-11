Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.RandomForests
Imports Microsoft.VisualBasic.MachineLearning.SVM
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports BayesRegNS = Microsoft.VisualBasic.MachineLearning.BayesianRegression
Imports LassoNS = Microsoft.VisualBasic.Data.Bootstrapping.LASSO
Imports MultivariateNS = Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports RfData = Microsoft.VisualBasic.MachineLearning.RandomForests.Data
Imports RidgeNS = Microsoft.VisualBasic.Data.Bootstrapping.Ridge
Imports SvmModelType = Microsoft.VisualBasic.MachineLearning.SVM.Model

Namespace Ensemble

#Region "random forest"

    ''' <summary>
    ''' 随机森林回归适配器。
    ''' </summary>
    ''' <remarks>
    ''' <c>RanFog.Predict</c> 返回各树叶值的平均，是连续回归值，不提供类别概率；
    ''' 因此该算法族只作为回归基学习器暴露（分类请使用朴素贝叶斯 / SVM / 逻辑回归）。
    ''' </remarks>
    Public NotInheritable Class RandomForestRegressor : Implements IRegressor

        Private ReadOnly fitted As RanFog

        Friend Sub New(fitted As RanFog)
            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            ' RanFog 只提供批量接口，这里复用单行包装
            Return fitted.Predict(New Double()() {x})(0)
        End Function

    End Class

    ''' <summary>随机森林回归基学习器</summary>
    Public NotInheritable Class RandomForestRegressorLearner : Implements IRegressorLearner

        ''' <summary>树的棵数</summary>
        Public Property TreeCount As Integer = 100
        ''' <summary>单棵树允许的最大分支数</summary>
        Public Property MaxBranch As Integer = 2000
        ''' <summary>每次分裂随机尝试的特征数；小于等于 0 时自动取 sqrt(特征数)</summary>
        Public Property Mtry As Integer = 0
        ''' <summary>是否使用 Gini 不纯度代替 MSE 作为分裂准则</summary>
        Public Property UseGini As Boolean = False

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "random-forest"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim width As Integer = x(0).Length

            ' RanFog 没有 Data(x, y) 构造器，attributeNames 必须显式给出，
            ' 否则 N_attributes = 0 会在分裂时直接崩溃
            Dim data As New RfData With {
                .Genotype = x,
                .phenotype = y,
                .ID = Enumerable.Range(1, y.Length).Select(Function(i) CStr(i)).ToArray(),
                .attributeNames = Enumerable.Range(1, width).Select(Function(i) $"F{i}").ToArray()
            }
            Dim mtry As Integer = If(Mtry > 0, Mtry, System.Math.Max(1, CInt(System.Math.Sqrt(width))))
            Dim forest As New RanFog With {
                .max_tree = TreeCount,
                .max_branch = MaxBranch,
                .mtry = mtry,
                .LF_c = If(UseGini, LF_c.Gini_Index, LF_c.Mean_Squared_Error)
            }

            Call forest.Run(data)

            Return New RandomForestRegressor(forest)
        End Function

    End Class

#End Region

#Region "bayesian regression"

    ''' <summary>高斯/离散贝叶斯回归适配器</summary>
    Public NotInheritable Class BayesianRegressor : Implements IRegressor

        Private ReadOnly fitted As BayesRegNS.BayesianRegression

        Friend Sub New(fitted As BayesRegNS.BayesianRegression)
            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Return fitted.Predict(x)
        End Function

    End Class

    ''' <summary>贝叶斯回归基学习器</summary>
    Public NotInheritable Class BayesianRegressorLearner : Implements IRegressorLearner

        ''' <summary>目标分箱数</summary>
        Public Property Bins As Integer = 10
        ''' <summary>特征处理模式：连续特征应使用 <c>Continuous</c></summary>
        Public Property Mode As BayesRegNS.BayesFeatureMode = BayesRegNS.BayesFeatureMode.Continuous
        ''' <summary>加性平滑系数</summary>
        Public Property Smoothing As Double = 1.0R
        ''' <summary>高斯方差下限</summary>
        Public Property VarianceFloor As Double = 0.000000001R

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "bayesian-regression"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim model As New BayesRegNS.BayesianRegression()

            Call model.Train(x, y, Mode, Bins, Smoothing, VarianceFloor)

            Return New BayesianRegressor(model)
        End Function

    End Class

#End Region

#Region "svm regression"

    ''' <summary>SVR 回归适配器（取 <c>SVMPrediction.score</c>）</summary>
    Public NotInheritable Class SvmRegressor : Implements IRegressor

        Private ReadOnly fitted As SvmModelType
        Private ReadOnly labelOffset As Integer

        Friend Sub New(fitted As SvmModelType, labelOffset As Integer)
            Me.fitted = fitted
            Me.labelOffset = labelOffset
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Dim prediction As SVMPrediction = fitted.Predict(SvmAdapter.ToNodes(x, labelOffset))

            ' 回归模型的 class 字段是 Integer.MinValue 哨兵，真实值在 score 里
            Return prediction.score
        End Function

    End Class

    ''' <summary>SVR 回归基学习器</summary>
    Public NotInheritable Class SvmRegressorLearner : Implements IRegressorLearner

        ''' <summary>惩罚系数 <c>C</c></summary>
        Public Property C As Double = 1.0R
        ''' <summary>RBF 核 <c>gamma</c>；小于等于 0 时自动取 <c>1 / 特征数</c></summary>
        Public Property Gamma As Double = 0.0R
        ''' <summary>不敏感带宽度 <c>epsilon</c></summary>
        Public Property Epsilon As Double = 0.1R
        ''' <summary>特征索引基，默认 1</summary>
        Public Property IndexOffset As Integer = 1

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "svr"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim offset As Integer = IndexOffset
            Dim problem As Problem = SvmAdapter.CreateProblem(x, y, offset, 0)
            Dim gamma As Double = If(Gamma > 0.0R, Gamma, 1.0R / x(0).Length)
            Dim parameter As New Parameter With {
                .svmType = SvmType.EPSILON_SVR,
                .kernelType = KernelType.RBF,
                .c = C,
                .gamma = gamma,
                .P = Epsilon,
                .probability = False,
                .cacheSize = 100
            }
            Dim fitted As SvmModelType = Training.Train(problem, parameter)

            Return New SvmRegressor(fitted, offset)
        End Function

    End Class

#End Region

#Region "data fittings"

    ''' <summary>岭回归适配器</summary>
    Public NotInheritable Class RidgeRegressor : Implements IRegressor

        Private ReadOnly fitted As RidgeNS.RidgeFit

        Friend Sub New(fitted As RidgeNS.RidgeFit)
            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Return fitted.GetY(x)
        End Function

    End Class

    ''' <summary>岭回归基学习器（内部按 GCV 选择最优 lambda）</summary>
    Public NotInheritable Class RidgeRegressorLearner : Implements IRegressorLearner

        ''' <summary>正则化强度；为空时走 GCV 自动选择（Lambda 是 VB 关键字，故命名为 Penalty）</summary>
        Public Property Penalty As Double? = Nothing
        ''' <summary>lambda 网格步数</summary>
        Public Property NumberOfLambdas As Integer = 100
        ''' <summary>是否先标准化各列</summary>
        Public Property Standardize As Boolean = True

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "ridge"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            ' RidgeFitting 是 Module（静态方法），不是可实例化的类
            Dim fitted As RidgeNS.RidgeFit = RidgeNS.RidgeFitting.Fit(
                x, y,
                Penalty,
                NumberOfLambdas,
                standardize:=Standardize)

            Return New RidgeRegressor(fitted)
        End Function

    End Class

    ''' <summary>多元线性回归适配器（无截距，与既有 MLR 语义一致）</summary>
    Public NotInheritable Class MlrRegressor : Implements IRegressor

        Private ReadOnly fitted As MultivariateNS.MLRFit

        Friend Sub New(fitted As MultivariateNS.MLRFit)
            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Return fitted.GetY(x)
        End Function

    End Class

    ''' <summary>多元线性回归基学习器</summary>
    Public NotInheritable Class MlrRegressorLearner : Implements IRegressorLearner

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "mlr"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim matrix As New NumericMatrix(x)
            Dim fitted As MultivariateNS.MLRFit = MultivariateNS.MLRFit.LinearFitting(matrix, New Vector(y))

            Return New MlrRegressor(fitted)
        End Function

    End Class

    ''' <summary>LASSO 适配器（按 R² 选择正则化路径上的最优步）</summary>
    Public NotInheritable Class LassoRegressor : Implements IRegressor

        Private ReadOnly fitted As LassoNS.LassoFit
        Private ReadOnly selectedStep As Integer

        Friend Sub New(fitted As LassoNS.LassoFit, selectedStep As Integer)
            Me.fitted = fitted
            Me.selectedStep = selectedStep
        End Sub

        ''' <summary>实际使用的 lambda 下标</summary>
        Public ReadOnly Property LambdaIndex As Integer
            Get
                Return selectedStep
            End Get
        End Property

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Dim weights As Double() = fitted.getWeights(selectedStep)
            Dim value As Double = fitted.intercepts(selectedStep)

            For j As Integer = 0 To weights.Length - 1
                value += weights(j) * x(j)
            Next

            Return value
        End Function

    End Class

    ''' <summary>LASSO 回归基学习器</summary>
    Public NotInheritable Class LassoRegressorLearner : Implements IRegressorLearner

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "lasso"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim n As Integer = x.Length
            Dim d As Integer = x(0).Length
            Dim names As String() = Enumerable.Range(1, d).Select(Function(i) $"F{i}").ToArray()
            Dim generator As New LassoNS.LassoFitGenerator()

            Call generator.init(names, n)

            ' LASSO 以「特征为第一维」组织数据，每个特征列都必须独立分配
            For j As Integer = 0 To d - 1
                Dim column As Double() = New Double(n - 1) {}

                For i As Integer = 0 To n - 1
                    column(i) = x(i)(j)
                Next

                Call generator.setFeatureValues(j, column)
            Next

            For i As Integer = 0 To n - 1
                Call generator.setTarget(i, y(i))
            Next

            Dim fitted As LassoNS.LassoFit = generator.fit(-1)
            Dim best As Integer = 0

            For k As Integer = 1 To fitted.rsquared.Length - 1
                If Double.IsNaN(fitted.rsquared(k)) Then
                    Continue For
                End If
                If Double.IsNaN(fitted.rsquared(best)) OrElse fitted.rsquared(k) > fitted.rsquared(best) Then
                    best = k
                End If
            Next

            Return New LassoRegressor(fitted, best)
        End Function

    End Class

#End Region

#Region "xgboost"

    ''' <summary>XGBoost 回归适配器（单输出）</summary>
    Public NotInheritable Class XGBoostRegressor : Implements IRegressor

        Private ReadOnly fitted As XGBoostModel

        Friend Sub New(fitted As XGBoostModel)
            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Return fitted.Predict(x)
        End Function

    End Class

    ''' <summary>XGBoost 回归基学习器</summary>
    Public NotInheritable Class XGBoostRegressorLearner : Implements IRegressorLearner

        ''' <summary>提升轮数</summary>
        Public Property Rounds As Integer = 30
        ''' <summary>树最大深度</summary>
        Public Property MaxDepth As Integer = 6
        ''' <summary>随机种子</summary>
        Public Property Seed As Integer = 42

        Public ReadOnly Property Name As String Implements IRegressorLearner.Name
            Get
                Return "xgboost"
            End Get
        End Property

        Public Function Fit(x As Double()(), y As Double()) As IRegressor Implements IRegressorLearner.Fit
            Dim fitted As XGBoostModel = XGBoostModel.Fit(x, y, "squareloss", Rounds, MaxDepth, Seed)

            Return New XGBoostRegressor(fitted)
        End Function

    End Class

#End Region

End Namespace

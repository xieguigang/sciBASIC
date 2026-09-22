#Region "Microsoft.VisualBasic::LinearShapModel, MachineLearning\ML_SHAP\Models\LinearShapModel.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Logistic
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

''' <summary>
''' 线性模型（多元线性回归）/ 广义线性模型（逻辑回归）的 SHAP 适配器。
''' 
''' 分类任务使用逻辑回归，回归任务使用多元线性回归（MLR）。
''' 两者的 SHAP 值都可以通过解析公式 ``phi_i = bi * (xi - E[xi])`` 精确计算。
''' </summary>
Public Class LinearShapModel

    ''' <summary>
    ''' 拟合得到的模型（<see cref="MLRFit"/> 或者 <see cref="LogisticFit"/>）。
    ''' </summary>
    ''' <returns></returns>
    Public Property Fitted As IFitted

    ''' <summary>
    ''' 回归系数（不包含截距）。
    ''' </summary>
    ''' <returns></returns>
    Public Property Coefficients As Double()

    ''' <summary>
    ''' 截距项。DataFittings 的线性/逻辑回归模型均不包含截距，因此该值为 0。
    ''' </summary>
    ''' <returns></returns>
    Public Property Intercept As Double

    ''' <summary>
    ''' 可选的连接函数（逻辑回归为 sigmoid）。
    ''' </summary>
    ''' <returns></returns>
    Public Property Link As Func(Of Double, Double)

    Public Property Training As ShapDataset
    Public Property IsClassification As Boolean

    Public ReadOnly Property Name As String
        Get
            Return If(IsClassification, "LogisticRegression", "LinearRegression")
        End Get
    End Property

    ''' <summary>
    ''' 训练线性/逻辑回归模型。
    ''' </summary>
    ''' <param name="dataset">训练数据集</param>
    ''' <param name="isClassification">是否进行分类任务（逻辑回归）</param>
    ''' <param name="iterations">逻辑回归梯度下降的迭代次数</param>
    ''' <param name="rate">逻辑回归的学习率</param>
    ''' <returns></returns>
    Public Shared Function Train(dataset As ShapDataset,
                                 isClassification As Boolean,
                                 Optional iterations As Integer = 1500,
                                 Optional rate As Double = 0.1) As LinearShapModel

        Dim table As New NumericTable(dataset.Features, dataset.IDs, dataset.FeatureNames) With {
            .labels = dataset.Labels.Select(Function(y) New Double() {y}).ToArray,
            .labelNames = New String() {"Y"}
        }

        If isClassification Then
            Dim fit As LogisticFit = table.LogisticRegression(rate:=rate, iterations:=iterations, Silent:=True)

            Return New LinearShapModel With {
                .Fitted = fit,
                .Coefficients = fit.Polynomial.Factors,
                .Intercept = 0,
                .Link = AddressOf LinearShapExplainer.Sigmoid,
                .Training = dataset,
                .IsClassification = True
            }
        Else
            Dim fit As MLRFit = table.MultipleLinearRegression(Silent:=True)

            Return New LinearShapModel With {
                .Fitted = fit,
                .Coefficients = fit.beta,
                .Intercept = 0,
                .Link = Nothing,
                .Training = dataset,
                .IsClassification = False
            }
        End If
    End Function

    ''' <summary>
    ''' 对新样本进行预测。分类任务返回正类概率，回归任务返回回归值。
    ''' </summary>
    ''' <param name="features"></param>
    ''' <returns></returns>
    Public Function Predict(features As Double()()) As Double()
        Dim fitted As IFitted = Me.Fitted

        Return features.Select(Function(row) fitted.GetY(row)).ToArray
    End Function

    ''' <summary>
    ''' 模型在单个样本上的原始输出。
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function Output(x As Double()) As Double
        Return Fitted.GetY(x)
    End Function

    Public Function PredictResult() As ModelPrediction
        Return New ModelPrediction With {
            .ModelName = Name,
            .DatasetName = Training.Name,
            .IsClassification = IsClassification,
            .IDs = Training.IDs,
            .Labels = Training.Labels,
            .Predictions = Predict(Training.Features)
        }
    End Function

    ''' <summary>
    ''' 构建解析 SHAP 解释器。
    ''' </summary>
    ''' <returns></returns>
    Public Function BuildExplainer() As IShapExplainer
        Dim means As Double() = ComputeFeatureMeans(Training.Features, Training.Width)
        Return New LinearShapExplainer(Coefficients, means, Training.FeatureNames, Intercept, Link)
    End Function

    Private Shared Function ComputeFeatureMeans(features As Double()(), width As Integer) As Double()
        Dim means As Double() = New Double(width - 1) {}

        If features Is Nothing OrElse features.Length = 0 Then
            Return means
        End If

        For j As Integer = 0 To width - 1
            Dim sum As Double = 0

            For i As Integer = 0 To features.Length - 1
                sum += features(i)(j)
            Next

            means(j) = sum / features.Length
        Next

        Return means
    End Function

End Class

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data

Namespace MGaussian

    ''' <summary>
    ''' 多响应高斯模型的 K 折交叉验证结果：在完整数据的 lambda 路径之上，
    ''' 附带每一折的折外误差、逐 lambda 的均值与标准误，以及 glmnet 语义的
    ''' ``lambda.min`` / ``lambda.1se``。
    ''' </summary>
    Public Class MGaussianCVFit

        ''' <summary>在完整数据上拟合出来的最终 lambda 路径</summary>
        Public Property Model As MGaussianFit
        ''' <summary>折数</summary>
        Public Property nfolds As Integer
        ''' <summary>评估指标（全响应合并的 mse 或 mae）</summary>
        Public Property measure As CVMeasure
        ''' <summary>``cvMean(step)``，各折折外误差的均值</summary>
        Public Property cvMean As Double()
        ''' <summary>``cvSe(step)``，均值的标准误 ``sd / sqrt(nfolds)``</summary>
        Public Property cvSe As Double()
        ''' <summary>``foldErrors(fold)(step)``，逐折误差矩阵</summary>
        Public Property foldErrors As Double()()
        ''' <summary>``foldId(sample)``，每个样本所属的折编号（确定性分折）</summary>
        Public Property foldId As Integer()
        ''' <summary>折外误差最小处的 lambda 下标</summary>
        Public Property BestIndex As Integer
        ''' <summary>1-SE 规则选出的 lambda 下标（惩罚更强、模型更稀疏）</summary>
        Public Property BestIndex1se As Integer

        ''' <summary>lambda 网格（来自完整数据的路径）</summary>
        Public ReadOnly Property lambdas As Double()
            Get
                Return Model.lambdas
            End Get
        End Property

        ''' <summary>使折外误差最小的 lambda</summary>
        Public ReadOnly Property lambdaMin As Double
            Get
                Return Model.lambdas(BestIndex)
            End Get
        End Property

        ''' <summary>
        ''' 1-SE 规则选出的最大 lambda：满足
        ''' ``cvMean(step) &lt;= cvMean(min) + cvSe(min)`` 的最强惩罚。
        ''' </summary>
        Public ReadOnly Property lambda1se As Double
            Get
                Return Model.lambdas(BestIndex1se)
            End Get
        End Property

        ''' <summary>用选中的 lambda 做多响应预测</summary>
        ''' <param name="newx">待预测特征矩阵</param>
        ''' <param name="use1se">True 时用 lambda.1se，缺省用 lambda.min</param>
        Public Function Predict(newx As Double()(), Optional use1se As Boolean = False) As Double()()
            Return Model.Predict(newx, If(use1se, BestIndex1se, BestIndex))
        End Function

        ''' <summary>取得选中 lambda 之下某一个响应的 IFitted 视图</summary>
        Public Function GetResponseModel(k As Integer, Optional use1se As Boolean = False) As IFitted
            Return Model.GetResponseModel(k, If(use1se, BestIndex1se, BestIndex))
        End Function

        ''' <summary>
        ''' 导出 CV 曲线表：每行一个 lambda 步骤，特征列为
        ''' ``cvMean`` / ``cvSe`` / ``df`` / ``trainMse``，标签列为 ``lambdas``。
        ''' </summary>
        Public Function toNumericTable() As NumericTable
            Dim steps As Integer = Model.lambdas.Length
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(steps, 4)
            Dim rowNames As String() = New String(steps - 1) {}
            Dim lambdaColumn As Double() = New Double(steps - 1) {}

            For li As Integer = 0 To steps - 1
                rowNames(li) = $"lambda_{li + 1}"
                matrix(li)(0) = cvMean(li)
                matrix(li)(1) = cvSe(li)
                matrix(li)(2) = CDbl(Model.df(li))
                matrix(li)(3) = Model.trainMse(li)
                lambdaColumn(li) = Model.lambdas(li)
            Next

            Dim table As New NumericTable(matrix, rowNames,
                                         New String() {"cvMean", "cvSe", "df", "trainMse"})

            Call table.SetLabel("lambdas", lambdaColumn)

            Return table
        End Function

        Public Overrides Function ToString() As String
            Return $"MGaussianCV({measure}, {nfolds}-fold): lambda.min={lambdaMin:G4} @ {BestIndex}, lambda.1se={lambda1se:G4} @ {BestIndex1se}, cvMean(min)={cvMean(BestIndex):F4}"
        End Function
    End Class
End Namespace

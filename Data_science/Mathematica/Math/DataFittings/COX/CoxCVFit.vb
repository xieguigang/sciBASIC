Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data

Namespace Cox

    ''' <summary>
    ''' Cox 模型的 K 折交叉验证结果：在完整数据的 lambda 路径之上，附带每折的
    ''' 折外指标、逐 lambda 的均值与标准误，以及 ``lambda.min`` / ``lambda.1se``。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="cvMean"/> 的方向取决于 <see cref="measure"/>：
    ''' ``Deviance`` 越小越好，``CIndex`` 越大越好（Harrell C 指数），
    ''' <see cref="Maximize"/> 会指出当前方向。
    ''' </remarks>
    Public Class CoxCVFit

        ''' <summary>在完整数据上拟合出来的最终 lambda 路径</summary>
        Public Property Model As CoxFit
        ''' <summary>折数</summary>
        Public Property nfolds As Integer
        ''' <summary>评估指标</summary>
        Public Property measure As CoxCVMeasure
        ''' <summary>是否使用 readme 中的 grouped（减法）偏似然</summary>
        Public Property grouped As Boolean
        ''' <summary>``cvMean(step)``，各折折外指标的均值</summary>
        Public Property cvMean As Double()
        ''' <summary>``cvSe(step)``，均值的标准误 ``sd / sqrt(nfolds)``</summary>
        Public Property cvSe As Double()
        ''' <summary>``foldErrors(fold)(step)``，逐折指标矩阵</summary>
        Public Property foldErrors As Double()()
        ''' <summary>``foldId(sample)``，确定性分折编号</summary>
        Public Property foldId As Integer()
        ''' <summary>最优指标处的 lambda 下标</summary>
        Public Property BestIndex As Integer
        ''' <summary>1-SE 规则选出的 lambda 下标（更稀疏）</summary>
        Public Property BestIndex1se As Integer

        ''' <summary>C 指数方向为越大越好，deviance 方向为越小越好</summary>
        Public ReadOnly Property Maximize As Boolean
            Get
                Return measure = CoxCVMeasure.CIndex
            End Get
        End Property

        ''' <summary>lambda 网格</summary>
        Public ReadOnly Property lambdas As Double()
            Get
                Return Model.lambdas
            End Get
        End Property

        ''' <summary>最优指标对应的 lambda</summary>
        Public ReadOnly Property lambdaMin As Double
            Get
                Return Model.lambdas(BestIndex)
            End Get
        End Property

        ''' <summary>1-SE 规则选出的最大 lambda（惩罚最强）</summary>
        Public ReadOnly Property lambda1se As Double
            Get
                Return Model.lambdas(BestIndex1se)
            End Get
        End Property

        ''' <summary>最优指标值</summary>
        Public ReadOnly Property BestScore As Double
            Get
                Return cvMean(BestIndex)
            End Get
        End Property

        ''' <summary>用选中 lambda 预测风险得分</summary>
        Public Function Predict(newx As Double()(), Optional use1se As Boolean = False) As Double()
            Return Model.Predict(newx, If(use1se, BestIndex1se, BestIndex))
        End Function

        ''' <summary>用选中 lambda 预测个体化生存曲线</summary>
        Public Function PredictSurvival(newx As Double()(), Optional use1se As Boolean = False) As SurvivalCurve()
            Return Model.PredictSurvival(newx, If(use1se, BestIndex1se, BestIndex))
        End Function

        ''' <summary>取得选中 lambda 的 IFitted 风险得分视图</summary>
        Public Function GetRiskModel(Optional use1se As Boolean = False) As IFitted
            Return Model.GetRiskModel(If(use1se, BestIndex1se, BestIndex))
        End Function

        ''' <summary>
        ''' 导出 CV 曲线表：每行一个 lambda 步骤，特征列为
        ''' ``cvMean`` / ``cvSe`` / ``df`` / ``devRatio`` / ``cIndex``，
        ''' 标签列为 ``lambdas``。
        ''' </summary>
        Public Function toNumericTable() As NumericTable
            Dim steps As Integer = Model.lambdas.Length
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(steps, 5)
            Dim rowNames As String() = New String(steps - 1) {}
            Dim lambdaColumn As Double() = New Double(steps - 1) {}

            For li As Integer = 0 To steps - 1
                rowNames(li) = $"lambda_{li + 1}"
                matrix(li)(0) = cvMean(li)
                matrix(li)(1) = cvSe(li)
                matrix(li)(2) = CDbl(Model.df(li))
                matrix(li)(3) = Model.devRatio(li)
                matrix(li)(4) = Model.cIndex(li)
                lambdaColumn(li) = Model.lambdas(li)
            Next

            Dim table As New NumericTable(matrix, rowNames,
                                         New String() {"cvMean", "cvSe", "df", "devRatio", "cIndex"})

            Call table.SetLabel("lambdas", lambdaColumn)

            Return table
        End Function

        Public Overrides Function ToString() As String
            Return $"CoxCV({measure}, {nfolds}-fold, grouped={grouped}): lambda.min={lambdaMin:G4} @ {BestIndex}, lambda.1se={lambda1se:G4} @ {BestIndex1se}, score(min)={BestScore:F4}"
        End Function
    End Class
End Namespace

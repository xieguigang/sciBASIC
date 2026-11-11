Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace MGaussian

    ''' <summary>
    ''' 多响应高斯模型的 lambda 正则化路径结果。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 每一步的系数都是 ``p x K`` 矩阵，且已经还原到输入数据的原始量纲：
    ''' <see cref="getCoefficients"/> 返回 ``(p+1) x K``（第 0 行是各响应的截距），
    ''' <see cref="groupNorms"/> 的含义与 readme 中 ``type.coef="2norm"`` 一致，
    ''' 即每个变量的 K 维系数的 l2 范数，用来观察变量「同进同出」。
    ''' </para>
    ''' <para>
    ''' 本类**不实现** <see cref="IFitted"/>（它是单响应契约）；
    ''' 用 <see cref="Predict"/> / <see cref="PredictPath"/> 做多响应预测，
    ''' 或用 <see cref="GetResponseModel"/> 取得某一个响应的 <see cref="IFitted"/> 视图。
    ''' </para>
    ''' </remarks>
    Public Class MGaussianFit

        ''' <summary>每一步的 lambda（严格递减）</summary>
        Public Property lambdas As Double()
        ''' <summary>``intercepts(step)(task)``，原始量纲下的截距</summary>
        Public Property intercepts As Double()()
        ''' <summary>``coefficients(step)(feature)(task)``，原始量纲下的斜率</summary>
        Public Property coefficients As Double()()()
        ''' <summary>``groupNorms(step)(feature)``，变量系数向量的 l2 范数</summary>
        Public Property groupNorms As Double()()
        ''' <summary>每一步的非零变量组数（共享支持集大小）</summary>
        Public Property df As Integer()
        ''' <summary>每一步在训练数据上的全响应均方误差</summary>
        Public Property trainMse As Double()
        ''' <summary>每一步的坐标下降是否收敛</summary>
        Public Property converged As Boolean()
        ''' <summary>每一步的坐标下降实际轮数</summary>
        Public Property iterations As Integer()

        ''' <summary>弹性网混合参数</summary>
        Public Property alpha As Double
        ''' <summary>拟合时是否标准化了特征列</summary>
        Public Property standardize As Boolean
        ''' <summary>拟合时是否标准化了响应列</summary>
        Public Property standardizeResponse As Boolean
        ''' <summary>是否含独立截距</summary>
        Public Property HasIntercept As Boolean
        ''' <summary>特征列名</summary>
        Public Property featureNames As String()
        ''' <summary>响应列名</summary>
        Public Property responseNames As String()

        ''' <summary>特征列均值（预处理留档）</summary>
        Public Property FeatureMeans As Double()
        ''' <summary>特征列尺度（预处理留档）</summary>
        Public Property FeatureScales As Double()
        ''' <summary>响应列均值（预处理留档）</summary>
        Public Property ResponseMeans As Double()
        ''' <summary>响应列尺度（预处理留档）</summary>
        Public Property ResponseScales As Double()

        ''' <summary>训练特征矩阵，供逐响应视图计算 R2 / ErrorTest</summary>
        Public Property TrainingX As Double()()
        ''' <summary>训练响应矩阵，供逐响应视图计算 R2 / ErrorTest</summary>
        Public Property TrainingY As Double()()
        ''' <summary>路径步数</summary>
        Public Property PathLength As Integer

        ''' <summary>特征数量 p</summary>
        Public ReadOnly Property NumberOfFeatures As Integer
            Get
                Return If(coefficients Is Nothing OrElse coefficients.Length = 0, 0, coefficients(0).Length)
            End Get
        End Property

        ''' <summary>响应数量 K</summary>
        Public ReadOnly Property NumberOfResponses As Integer
            Get
                Return If(intercepts Is Nothing OrElse intercepts.Length = 0, 0, intercepts(0).Length)
            End Get
        End Property

        ''' <summary>路径起点（惩罚最强）</summary>
        Public ReadOnly Property MaxLambda As Double
            Get
                Return lambdas(0)
            End Get
        End Property

        ''' <summary>路径终点（惩罚最弱）</summary>
        Public ReadOnly Property MinLambda As Double
            Get
                Return lambdas(lambdas.Length - 1)
            End Get
        End Property

        ''' <summary>把 ``-1`` 解析为路径最后一步，并校验下标范围</summary>
        Private Function ResolveIndex(lambdaIdx As Integer) As Integer
            Dim steps As Integer = lambdas.Length
            Dim idx As Integer = If(lambdaIdx < 0, steps - 1, lambdaIdx)

            If idx < 0 OrElse idx >= steps Then
                Throw New ArgumentOutOfRangeException(NameOf(lambdaIdx),
                    $"lambda index {lambdaIdx} is out of range [0, {steps - 1}]!")
            End If

            Return idx
        End Function

        ''' <summary>
        ''' 取出指定 lambda 步骤的 ``(p+1) x K`` 系数矩阵：
        ''' 第 0 行是截距，其后每一行对应一个特征列。
        ''' </summary>
        Public Function getCoefficients(lambdaIdx As Integer) As Double()()
            Dim idx As Integer = ResolveIndex(lambdaIdx)
            Dim p As Integer = NumberOfFeatures
            Dim k As Integer = NumberOfResponses
            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(p + 1, k)

            For c As Integer = 0 To k - 1
                matrix(0)(c) = intercepts(idx)(c)
            Next
            For j As Integer = 0 To p - 1
                For c As Integer = 0 To k - 1
                    matrix(j + 1)(c) = coefficients(idx)(j)(c)
                Next
            Next

            Return matrix
        End Function

        ''' <summary>取出指定 lambda 步骤每个变量的 K 维系数 l2 范数（2-norm 路径）</summary>
        Public Function getGroupNorms(lambdaIdx As Integer) As Double()
            Return CType(groupNorms(ResolveIndex(lambdaIdx)).Clone(), Double())
        End Function

        ''' <summary>
        ''' 预测：返回 ``result(sample)(response)`` 的 ``N x K`` 矩阵。
        ''' </summary>
        ''' <param name="newx">待预测特征矩阵，列数必须与训练时一致</param>
        ''' <param name="lambdaIdx">lambda 步骤下标，``-1`` 表示路径最后一步（惩罚最弱）</param>
        Public Function Predict(newx As Double()(), Optional lambdaIdx As Integer = -1) As Double()()
            Dim idx As Integer = ResolveIndex(lambdaIdx)
            Dim p As Integer = NumberOfFeatures
            Dim k As Integer = NumberOfResponses

            If newx Is Nothing Then
                Throw New ArgumentNullException(NameOf(newx))
            End If
            If newx.Length = 0 Then
                Throw New ArgumentException("the prediction requires a non-empty feature matrix!", NameOf(newx))
            End If

            Dim result As Double()() = RectangularArray.Matrix(Of Double)(newx.Length, k)

            For i As Integer = 0 To newx.Length - 1
                Dim row As Double() = newx(i)

                If row Is Nothing OrElse row.Length < p Then
                    Throw New ArgumentException($"the prediction row {i} has {If(row Is Nothing, 0, row.Length)} features, but the model requires {p}!", NameOf(newx))
                End If

                For c As Integer = 0 To k - 1
                    result(i)(c) = intercepts(idx)(c)
                Next

                For j As Integer = 0 To p - 1
                    If Not Double.IsFinite(row(j)) Then
                        Throw New ArgumentException($"the prediction value newx({i})({j}) must be a finite number!", NameOf(newx))
                    End If

                    For c As Integer = 0 To k - 1
                        result(i)(c) += row(j) * coefficients(idx)(j)(c)
                    Next
                Next
            Next

            Return result
        End Function

        ''' <summary>预测单个样本，返回长度为 K 的响应向量</summary>
        Public Function PredictSingle(x As Double(), Optional lambdaIdx As Integer = -1) As Double()
            Dim rows As Double()() = New Double(0)() {}
            rows(0) = x
            Return Predict(rows, lambdaIdx)(0)
        End Function

        ''' <summary>
        ''' 多 lambda 预测：返回 ``result(step)(sample)(response)`` 的三维数组，
        ''' 对应 readme 中 ``predict(..., s=c(...))`` 的 ``N x K x L`` 输出。
        ''' </summary>
        ''' <param name="newx">待预测特征矩阵</param>
        ''' <param name="lambdaIdx">需要输出的步骤下标数组；Nothing 表示输出全部步骤</param>
        Public Function PredictPath(newx As Double()(), lambdaIdx As Integer()) As Double()()()
            Dim steps As Integer

            If lambdaIdx Is Nothing Then
                steps = lambdas.Length
                lambdaIdx = New Integer(steps - 1) {}

                For li As Integer = 0 To steps - 1
                    lambdaIdx(li) = li
                Next
            Else
                steps = lambdaIdx.Length
            End If

            If steps = 0 Then
                Throw New ArgumentException("at least one lambda index is required for a path prediction!", NameOf(lambdaIdx))
            End If

            Dim result As Double()()() = New Double(steps - 1)()() {}

            For li As Integer = 0 To steps - 1
                result(li) = Predict(newx, lambdaIdx(li))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 取得某一个响应在指定 lambda 步骤上的 <see cref="IFitted"/> 视图，
        ''' 从而可以复用 ``NumericTablePrediction.SetPrediction`` 等既有的单响应链路。
        ''' </summary>
        ''' <param name="k">响应列下标（从 0 开始）</param>
        ''' <param name="lambdaIdx">lambda 步骤下标，``-1`` 表示路径最后一步</param>
        Public Function GetResponseModel(k As Integer, Optional lambdaIdx As Integer = -1) As IFitted
            Dim idx As Integer = ResolveIndex(lambdaIdx)

            If k < 0 OrElse k >= NumberOfResponses Then
                Throw New ArgumentOutOfRangeException(NameOf(k),
                    $"response index {k} is out of range [0, {NumberOfResponses - 1}]!")
            End If

            Return New MGaussianResponseModel(Me, k, idx)
        End Function

        ''' <summary>
        ''' 把系数路径导出为二维表：每一行是一个 lambda 步骤，
        ''' 每个特征列给出该变量 K 维系数的 l2 范数（``type.coef="2norm"`` 视角），
        ''' 标签列携带 ``lambdas`` / ``df`` / ``mse`` / ``iterations`` / ``converged``。
        ''' </summary>
        Public Function toNumericTable() As NumericTable
            Dim steps As Integer = lambdas.Length
            Dim p As Integer = NumberOfFeatures
            Dim names As String() = featureNames

            If names Is Nothing OrElse names.Length <> p Then
                names = MGaussianFitting.DefaultNames(p, "X")
            End If

            Dim matrix As Double()() = RectangularArray.Matrix(Of Double)(steps, p)
            Dim labels As Double()() = RectangularArray.Matrix(Of Double)(steps, 5)
            Dim rowNames As String() = New String(steps - 1) {}

            For li As Integer = 0 To steps - 1
                rowNames(li) = $"lambda_{li + 1}"

                For j As Integer = 0 To p - 1
                    matrix(li)(j) = groupNorms(li)(j)
                Next

                labels(li)(0) = lambdas(li)
                labels(li)(1) = CDbl(df(li))
                labels(li)(2) = trainMse(li)
                labels(li)(3) = CDbl(iterations(li))
                labels(li)(4) = If(converged(li), 1.0, 0.0)
            Next

            Dim table As New NumericTable(matrix, rowNames, names)
            Dim labelNames As String() = {"lambdas", "df", "mse", "iterations", "converged"}

            For c As Integer = 0 To labelNames.Length - 1
                ' 循环变量不能直接进 lambda（BC42324），先拷到局部变量
                Dim col As Integer = c
                Dim vec As Double() = New Double(steps - 1) {}

                For li As Integer = 0 To steps - 1
                    vec(li) = labels(li)(col)
                Next

                Call table.SetLabel(labelNames(c), vec)
            Next

            Return table
        End Function

        Public Overrides Function ToString() As String
            Dim last As Integer = lambdas.Length - 1
            Return $"MGaussian(alpha={alpha:F2}, K={NumberOfResponses}, p={NumberOfFeatures}): {lambdas.Length} lambda steps, lambda=[{lambdas(0):G4}, {lambdas(last):G4}], df(last)={df(last)}, mse(last)={trainMse(last):F4}"
        End Function
    End Class
End Namespace

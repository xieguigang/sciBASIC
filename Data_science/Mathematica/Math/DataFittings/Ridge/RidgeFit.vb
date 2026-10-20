Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Ridge

    ''' <summary>
    ''' 岭回归的拟合结果，其中保存了 lambda 正则化路径上面的全部解（岭迹）。
    ''' 
    ''' 路径上面的每一步都由 <see cref="lambdas"/> 之中的一个 lambda 值、
    ''' <see cref="intercepts"/> 之中的一个截距与 <see cref="coefficients"/>
    ''' 之中的一组回归系数组成；<see cref="bestIndex"/> 是 GCV 准则挑出来的
    ''' 最优的那一步，<see cref="GetY"/> 等预测接口默认使用这一步的模型。
    ''' </summary>
    Public Class RidgeFit : Implements IFitted

        ''' <summary>
        ''' 路径上面每一个步骤的 lambda 值
        ''' </summary>
        ''' <returns></returns>
        Public Property lambdas As Double()

        ''' <summary>
        ''' 路径上面每一个步骤的截距（已经还原到原始量纲，且不施加惩罚）
        ''' </summary>
        ''' <returns></returns>
        Public Property intercepts As Double()

        ''' <summary>
        ''' 路径上面每一个步骤的回归系数（已经还原到原始量纲），
        ''' 每一行对应一个 lambda、每一列对应一个特征
        ''' </summary>
        ''' <returns></returns>
        Public Property coefficients As Double()()

        ''' <summary>
        ''' 路径上面每一个步骤的 R2
        ''' </summary>
        ''' <returns></returns>
        Public Property rsquared As Double()

        ''' <summary>
        ''' 路径上面每一个步骤的残差平方和
        ''' </summary>
        ''' <returns></returns>
        Public Property rss As Double()

        ''' <summary>
        ''' 路径上面每一个步骤的有效自由度 ``tr(H)``
        ''' </summary>
        ''' <returns></returns>
        Public Property df As Double()

        ''' <summary>
        ''' 路径上面每一个步骤的 GCV 准则值，越小越好
        ''' </summary>
        ''' <returns></returns>
        Public Property gcv As Double()

        ''' <summary>
        ''' GCV 最小的那一步在路径上面的下标
        ''' </summary>
        ''' <returns></returns>
        Public Property bestIndex As Integer

        ''' <summary>
        ''' 特征列的名称
        ''' </summary>
        ''' <returns></returns>
        Public Property featureNames As String()

        ''' <summary>
        ''' 拟合的时候是否对特征做了标准化
        ''' </summary>
        ''' <returns></returns>
        Public Property standardize As Boolean

        ''' <summary>
        ''' 最优模型的残差平方和
        ''' </summary>
        ''' <returns></returns>
        Public Property SSE As Double

        ''' <summary>
        ''' 总平方和（观测值偏离其均值的平方和）
        ''' </summary>
        ''' <returns></returns>
        Public Property SST As Double

        ''' <summary>
        ''' 保存拟合后的逐点误差测试结果
        ''' </summary>
        ''' <returns></returns>
        Public Property ErrorTest As IFitError() Implements IFitted.ErrorTest

        ''' <summary>
        ''' 最优模型（GCV 最小的那一步）的 lambda 值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BestLambda As Double
            Get
                If lambdas Is Nothing OrElse lambdas.Length = 0 Then
                    Return 0.0
                End If

                Return lambdas(bestIndex)
            End Get
        End Property

        ''' <summary>
        ''' 最优模型的截距
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BestIntercept As Double
            Get
                If intercepts Is Nothing OrElse intercepts.Length = 0 Then
                    Return 0.0
                End If

                Return intercepts(bestIndex)
            End Get
        End Property

        ''' <summary>
        ''' 最优模型的回归系数
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BestCoefficients As Double()
            Get
                Return getCoefficients(bestIndex)
            End Get
        End Property

        ''' <summary>
        ''' 最优模型的决定系数 R2
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property R2 As Double Implements IFitted.R2
            Get
                If rsquared Is Nothing OrElse rsquared.Length = 0 Then
                    Return 0.0
                End If

                Return rsquared(bestIndex)
            End Get
        End Property

        ''' <summary>
        ''' 由最优模型的截距与回归系数构造出来的多元多项式
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Polynomial As Formula Implements IFitted.Polynomial
            Get
                Return New RidgePolynomial With {
                    .Intercept = BestIntercept,
                    .Factors = BestCoefficients
                }
            End Get
        End Property

        ''' <summary>
        ''' 取出路径上面指定步骤的回归系数
        ''' </summary>
        ''' <param name="lambdaIdx">步骤下标，即 <see cref="lambdas"/> 的下标</param>
        ''' <returns>该 lambda 之下的回归系数向量（原始量纲）</returns>
        Public Function getCoefficients(lambdaIdx As Integer) As Double()
            If coefficients Is Nothing OrElse coefficients.Length = 0 Then
                Return New Double() {}
            End If
            If lambdaIdx < 0 OrElse lambdaIdx >= coefficients.Length Then
                Throw New ArgumentOutOfRangeException(NameOf(lambdaIdx))
            End If

            Return coefficients(lambdaIdx)
        End Function

        ''' <summary>
        ''' 使用最优模型对给定的自变量做预测
        ''' </summary>
        ''' <param name="x">自变量值序列，依次对应各个特征列</param>
        ''' <returns>最优模型的预测值</returns>
        Public Function GetY(ParamArray x() As Double) As Double Implements IFitted.GetY
            Return PredictAt(bestIndex, x)
        End Function

        ''' <summary>
        ''' 使用路径上面指定步骤的模型对给定的自变量做预测
        ''' </summary>
        ''' <param name="lambdaIdx">步骤下标，即 <see cref="lambdas"/> 的下标</param>
        ''' <param name="x">自变量值序列，依次对应各个特征列</param>
        ''' <returns>该 lambda 之下模型的预测值</returns>
        Public Function PredictAt(lambdaIdx As Integer, x As Double()) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Dim coef As Double() = getCoefficients(lambdaIdx)

            If x.Length < coef.Length Then
                Throw New ArgumentException($"the ridge model requires {coef.Length} feature values, but only {x.Length} were given!", NameOf(x))
            End If

            Dim y As Double = intercepts(lambdaIdx)

            For i As Integer = 0 To coef.Length - 1
                y += coef(i) * x(i)
            Next

            Return y
        End Function

        ''' <summary>
        ''' 将岭迹（正则化路径）导出为统一的二维表对象 <see cref="NumericTable"/>：
        ''' 
        ''' 1. 每一行对应 lambda 路径上面的一个步骤
        ''' 2. 表的第一列是模型的截距，其余的特征列是各个变量的回归系数
        ''' 3. 标签矩阵之中附带 ``lambdas``、``rsquared``、``gcv``、``df`` 与
        '''    ``rss`` 这几个路径指标
        ''' </summary>
        ''' <param name="featureNames">
        ''' 特征列的名称。为空或者数量不匹配的时候自动生成 ``[1]..[n]`` 序号
        ''' </param>
        ''' <returns></returns>
        Public Function toNumericTable(Optional featureNames As String() = Nothing) As NumericTable
            Dim names As String() = If(featureNames, Me.featureNames)
            Dim p As Integer = If(coefficients Is Nothing OrElse coefficients.Length = 0, 0, coefficients(0).Length)

            If names Is Nothing OrElse names.Length <> p Then
                names = Enumerable.Range(1, p).Select(Function(i) $"[{i}]").ToArray
            End If

            Dim steps As Integer = lambdas.Length
            Dim header As String() = New String(names.Length) {}
            Dim labelNames As String() = {"lambdas", "rsquared", "gcv", "df", "rss"}
            Dim matrix As Double()() = New Double(steps - 1)() {}
            Dim labels As Double()() = New Double(steps - 1)() {}

            header(0) = "intercept"

            For j As Integer = 0 To names.Length - 1
                header(j + 1) = names(j)
            Next

            For i As Integer = 0 To steps - 1
                Dim w As Double() = getCoefficients(i)
                Dim row As Double() = New Double(names.Length) {}

                row(0) = intercepts(i)

                For j As Integer = 0 To names.Length - 1
                    row(j + 1) = If(j < w.Length, w(j), 0.0)
                Next

                matrix(i) = row
                labels(i) = New Double() {
                    lambdas(i),
                    rsquared(i),
                    gcv(i),
                    df(i),
                    rss(i)
                }
            Next

            Dim rowNames As String() = Enumerable.Range(1, steps).Select(Function(i) $"lambda_{i}").ToArray
            Dim table As New NumericTable(matrix, rowNames, header)

            For j As Integer = 0 To labelNames.Length - 1
                ' 循环变量不能直接进 lambda（BC42324），先拷到局部变量
                Dim col As Integer = j
                Dim vec As Double() = labels.Select(Function(row) row(col)).ToArray

                Call table.SetLabel(labelNames(j), vec)
            Next

            Return table
        End Function

        Public Overrides Function ToString() As String
            Dim summary As String = $"ridge: {lambdas.Length} step(s), best lambda = {BestLambda:G4}, R2 = {R2:F6}"

            If featureNames Is Nothing OrElse featureNames.Length = 0 Then
                Return summary
            End If

            Dim best As Double() = BestCoefficients
            Dim terms As New List(Of String) From {$"{BestIntercept:F4}"}

            For i As Integer = 0 To best.Length - 1
                terms.Add($"{best(i):F4}*{featureNames(i)}")
            Next

            Return summary & " | y = " & String.Join(" + ", terms)
        End Function
    End Class
End Namespace

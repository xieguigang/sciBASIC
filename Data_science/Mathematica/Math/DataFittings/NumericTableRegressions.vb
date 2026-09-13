#Region "Microsoft.VisualBasic::NumericTableRegressions, Data_science\Mathematica\Math\DataFittings\NumericTableRegressions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Scripting
Imports FittingExtensions = Microsoft.VisualBasic.Data.Bootstrapping.Extensions
Imports FeatureProjectionNS = Microsoft.VisualBasic.Data.Bootstrapping.FeatureProjection
Imports LassoNS = Microsoft.VisualBasic.Data.Bootstrapping.LASSO
Imports LogisticNS = Microsoft.VisualBasic.Data.Bootstrapping.Logistic
Imports MultivariateNS = Microsoft.VisualBasic.Data.Bootstrapping.Multivariate

''' <summary>
''' 回归/曲线拟合算法的统一二维表入口。
''' 
''' 输入约定：
''' 
''' 1. 自变量 X 取表的**全部**特征列（<see cref="NumericTable.features"/>）
''' 2. 因变量 y 由参数 ``y`` 指定标签列名，缺省为表中的第一个标签列；
'''    表中没有标签列的时候会抛出 <see cref="ArgumentException"/>
''' 3. 一元/多项式类的算法（<see cref="LinearFit"/>、<see cref="PolyFit"/>、
'''    <see cref="StandardCurveFit"/>、<see cref="WeightedLinearFit"/>、
'''    <see cref="Lowess"/>、<see cref="LoessFit"/>、<see cref="AutoPointDeletion"/>、
'''    <see cref="GaussNewtonFit"/>、<see cref="BayesianCurveFit"/>）要求表
'''    之中**有且仅有一个**特征列，否则会抛出 <see cref="ArgumentException"/>
''' 
''' 所有的入口方法都直接返回既有的拟合模型对象，模型的输入/输出约定保持不变；
''' 若需要把模型的预测值写回到表的标签矩阵之中，请使用
''' <see cref="NumericTablePrediction.SetPrediction"/> 扩展方法。
''' </summary>
Public Module NumericTableRegressions

#Region "input helpers"

    ''' <summary>
    ''' 读取响应/因变量的向量：指定了 <paramref name="y"/> 的时候读取对应的标签列，
    ''' 否则自动读取第一个标签列
    ''' </summary>
    Private Function ResponseVector(table As NumericTable, y As String, caller As String) As Double()
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Dim name As String = y

        If String.IsNullOrEmpty(name) Then
            If table.labelNames Is Nothing OrElse table.labelNames.Length = 0 OrElse table.labels Is Nothing Then
                Throw New ArgumentException(
                    $"the '{caller}' regression requires a response variable, but the given table has no label column for use as the response variable y!",
                    NameOf(table)
                )
            End If

            name = table.labelNames(0)
        End If

        ' 指定的标签列不存在的时候，GetLabel会给出明确的 KeyNotFoundException
        Return table.GetLabel(name)
    End Function

    ''' <summary>
    ''' 获取特征矩阵（行主序的锯齿数组）
    ''' </summary>
    Private Function FeatureMatrix(table As NumericTable, caller As String) As Double()()
        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        If table.nsamples = 0 OrElse table.nfeatures = 0 Then
            Throw New ArgumentException($"the '{caller}' regression requires a non-empty feature matrix!", NameOf(table))
        End If

        Return table.NumericRows
    End Function

    ''' <summary>
    ''' 获取唯一的特征列。只有单变量的算法（线性/多项式/局部回归）才可以调用这个方法
    ''' </summary>
    Private Function SingleFeature(table As NumericTable, caller As String) As Double()
        Call FeatureMatrix(table, caller)

        If table.nfeatures <> 1 Then
            Throw New ArgumentException(
                $"the '{caller}' regression is an univariate model, it requires exactly 1 feature column, but the given table has {table.nfeatures} feature columns!",
                NameOf(table)
            )
        End If

        Return table.Feature(FeatureNameList(table)(0))
    End Function

    ''' <summary>
    ''' 获取特征矩阵的列名（缺失的时候自动生成 ``1..n`` 序号）
    ''' </summary>
    Private Function FeatureNameList(table As NumericTable) As String()
        Dim m As Integer = table.nfeatures

        If table.featureNames IsNot Nothing AndAlso table.featureNames.Length = m Then
            Return table.featureNames
        End If

        Return Enumerable.Range(1, m).Select(Function(i) CStr(i)).ToArray
    End Function

#End Region

#Region "linear && polynomial"

    ''' <summary>
    ''' 一元线性回归：``y = a + b * x``。
    ''' 
    ''' 要求表中只有一个特征列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function LinearFit(table As NumericTable,
                              Optional y As String = Nothing,
                              Optional Silent As Boolean = False) As FitResult

        Dim xv As Double() = SingleFeature(table, NameOf(LinearFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(LinearFit))

        Return LeastSquares.LinearFit(xv, yv)
    End Function

    ''' <summary>
    ''' 多项式回归：``y = a0 + a1*x + a2*x^2 + ... + an*x^n``。
    ''' 
    ''' 要求表中只有一个特征列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="poly_n">期望拟合的最高阶数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function PolyFit(table As NumericTable,
                            poly_n As Integer,
                            Optional y As String = Nothing,
                            Optional Silent As Boolean = False) As FitResult

        Dim xv As Double() = SingleFeature(table, NameOf(PolyFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(PolyFit))

        Return LeastSquares.PolyFit(xv, yv, poly_n)
    End Function

    ''' <summary>
    ''' 标准曲线的线性回归建模（等价于 <see cref="Extensions.LinearRegression(Vector, Vector, Weights)"/>）。
    ''' 
    ''' + ``weighted`` 为空：<see cref="FitResult"/>
    ''' + ``weighted`` 非空：<see cref="WeightedFit"/>
    ''' 
    ''' 要求表中只有一个特征列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="weighted">加权回归的权重函数，为空的时候进行普通的线性拟合</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function StandardCurveFit(table As NumericTable,
                                     Optional weighted As Weights = Nothing,
                                     Optional y As String = Nothing,
                                     Optional Silent As Boolean = False) As IFitted

        Dim xv As Double() = SingleFeature(table, NameOf(StandardCurveFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(StandardCurveFit))

        Return FittingExtensions.LinearRegression(xv.AsVector, yv.AsVector, weighted)
    End Function

    ''' <summary>
    ''' 加权线性回归：
    ''' 
    ''' + ``weighted`` 为空的时候，权重缺省取 ``1 / x^2``（倒数方差权重）
    ''' 
    ''' 要求表中只有一个特征列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="orderOfPolynomial">多项式的阶数</param>
    ''' <param name="weights">权重函数，为空的时候使用 ``1 / x^2``</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function WeightedLinearFit(table As NumericTable,
                                      Optional orderOfPolynomial As Integer = 2,
                                      Optional weights As Weights = Nothing,
                                      Optional y As String = Nothing,
                                      Optional Silent As Boolean = False) As WeightedFit

        Dim xv As Double() = SingleFeature(table, NameOf(WeightedLinearFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(WeightedLinearFit))
        Dim X As Vector = xv.AsVector
        Dim W As Vector = If(weights Is Nothing, 1 / (X ^ 2), weights(X))

        Return WeightedLinearRegression.Regress(X, yv.AsVector, W, orderOfPolynomial)
    End Function

    ''' <summary>
    ''' 多元线性回归（MLR）。使用表中的全部特征列作为自变量。
    ''' </summary>
    ''' <remarks>
    ''' 模型的形式为 ``y = b1*x1 + b2*x2 + ... + bn*xn``，**不包含截距项**；
    ''' 若需要截距，请在表中显式地加入一列常量（例如全为 1 的 ``bias`` 特征列）。
    ''' </remarks>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function MultipleLinearRegression(table As NumericTable,
                                             Optional y As String = Nothing,
                                             Optional Silent As Boolean = False) As MultivariateNS.MLRFit

        Dim xm As Double()() = FeatureMatrix(table, NameOf(MultipleLinearRegression))
        Dim yv As Double() = ResponseVector(table, y, NameOf(MultipleLinearRegression))

        Return MultivariateNS.LinearFittingAlgorithm.LinearFitting(xm.ToMatrix, yv)
    End Function

    ''' <summary>
    ''' 非负最小二乘（NNLS）：求解 ``min ||A*x - b||, x >= 0``。
    ''' 
    ''' 这里 A 为表的特征矩阵、b 为响应变量，返回的系数向量的长度等于特征列的数量，
    ''' 每一个系数对应 <see cref="NumericTable.featureNames"/> 之中的一列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 A、指定标签列为 b 的二维表</param>
    ''' <param name="maxIterations">最大的迭代次数</param>
    ''' <param name="tolerance">收敛的容差</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function Nnls(table As NumericTable,
                         Optional maxIterations As Integer = 1000,
                         Optional tolerance As Double = 0.000000000001,
                         Optional y As String = Nothing,
                         Optional Silent As Boolean = False) As Double()

        Dim xm As Double()() = FeatureMatrix(table, NameOf(Nnls))
        Dim yv As Double() = ResponseVector(table, y, NameOf(Nnls))

        Return NonNegativeLeastSquares.Solve(xm.ToMatrix, yv, maxIterations, tolerance)
    End Function

#End Region

#Region "local regression"

    ''' <summary>
    ''' LOWESS 局部加权回归散点平滑。
    ''' 
    ''' 要求表中只有一个特征列。返回已经按照 x 升序排序的
    ''' ``(x, yfit)``：其中 ``yfit`` 为平滑之后的拟合值。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="f">平滑窗口占样本数量的比例，缺省为 2/3</param>
    ''' <param name="nsteps">鲁棒迭代的次数</param>
    ''' <param name="delta">用于减少计算量的非负参数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function Lowess(table As NumericTable,
                           Optional f As Double = 2.0 / 3.0,
                           Optional nsteps As Integer = 3,
                           Optional delta As Double = 0.01,
                           Optional y As String = Nothing,
                           Optional Silent As Boolean = False) As (x As Double(), yfit As Double())

        Dim xv As Double() = SingleFeature(table, NameOf(Lowess))
        Dim yv As Double() = ResponseVector(table, y, NameOf(Lowess))
        Dim result = LowessFittings.Lowess(xv, yv, xv.Length, f, nsteps, delta)

        Return (result.x, result.y)
    End Function

    ''' <summary>
    ''' LOESS 局部回归模型。
    ''' 
    ''' 要求表中只有一个特征列。可以使用
    ''' <see cref="LOESS.PredictLOESS(LOESSModel, Double)"/> 对新的 x 值进行预测。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="span">局部窗口的跨度</param>
    ''' <param name="degree">局部多项式的阶数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoessFit(table As NumericTable,
                             span As Double,
                             degree As Integer,
                             Optional y As String = Nothing,
                             Optional Silent As Boolean = False) As LOESSModel

        Dim xv As Double() = SingleFeature(table, NameOf(LoessFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(LoessFit))

        Return LOESS.FitLOESS(xv, yv, span, degree)
    End Function

#End Region

#Region "classification && regularized"

    ''' <summary>
    ''' 逻辑回归（梯度下降求解）。使用表中的全部特征列作为自变量。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为类别编号的二维表</param>
    ''' <param name="rate">学习率</param>
    ''' <param name="iterations">梯度下降的迭代次数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function LogisticRegression(table As NumericTable,
                                       Optional rate As Double = 0.0001,
                                       Optional iterations As Integer = 3000,
                                       Optional y As String = Nothing,
                                       Optional Silent As Boolean = False) As LogisticNS.LogisticFit

        Dim xm As Double()() = FeatureMatrix(table, NameOf(LogisticRegression))
        Dim yv As Double() = ResponseVector(table, y, NameOf(LogisticRegression))
        Dim n As Integer = table.nsamples
        Dim instances As LogisticNS.Instance() = New LogisticNS.Instance(n - 1) {}
        Dim model As New LogisticNS.Logistic(table.nfeatures, rate, Nothing) With {
            .ITERATIONS = iterations
        }

        For i As Integer = 0 To n - 1
            instances(i) = New LogisticNS.Instance(CInt(yv(i)), xm(i))
        Next

        Return model.Train(instances)
    End Function

    ''' <summary>
    ''' LASSO 回归（L1 正则化的线性回归）。使用表中的全部特征列作为自变量。
    ''' 
    ''' 注意：返回的 ``LassoFit`` 并不是 <see cref="IFitted"/> 模型，
    ''' 其预测值需要根据 lambda 路径上的截距
    ''' （``LassoFit.intercepts``）与权重
    ''' （``LassoFit.getWeights``）自行计算。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="maxAllowedFeaturesPerModel">
    ''' 每一个模型所允许的最大特征数量，小于 0 的时候表示不做限制
    ''' </param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭LASSO求解器内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function LassoRegression(table As NumericTable,
                                    Optional maxAllowedFeaturesPerModel As Integer = -1,
                                    Optional y As String = Nothing,
                                    Optional Silent As Boolean = False) As LassoNS.LassoFit

        Dim xm As Double()() = FeatureMatrix(table, NameOf(LassoRegression))
        Dim yv As Double() = ResponseVector(table, y, NameOf(LassoRegression))
        Dim names As String() = FeatureNameList(table)
        Dim n As Integer = table.nsamples
        Dim generator As New LassoNS.LassoFitGenerator()

        LassoNS.LassoFitGenerator.tqdm_verbose = Not Silent

        Call generator.init(names, n)

        For j As Integer = 0 To names.Length - 1
            Call generator.setFeatureValues(j, table.Feature(names(j)))
        Next

        For i As Integer = 0 To n - 1
            Call generator.setTarget(i, yv(i))
        Next

        Return generator.fit(maxAllowedFeaturesPerModel)
    End Function

#End Region

#Region "robust && nonlinear"

    ''' <summary>
    ''' 自动识别并且剔除异常点之后再进行线性回归建模。
    ''' 
    ''' 要求表中只有一个特征列。<paramref name="removed"/> 会返回被剔除的
    ''' 样本在表中的行下标（基于最终的 ``(x, y)`` 值进行回溯匹配）。
    ''' </summary>
    ''' <param name="table">特征矩阵为 X、指定标签列为 y 的二维表</param>
    ''' <param name="weighted">加权回归的权重函数</param>
    ''' <param name="max">
    ''' 最多允许自动剔除的点的数量：
    ''' 
    ''' + 负数：自动确定
    ''' + 0：不删除任何点
    ''' + 正数：允许自动剔除的最大点数
    ''' </param>
    ''' <param name="keepsLowestPoint">是否保留 x 值最小的点</param>
    ''' <param name="removesZeroY">是否剔除 y 为零的点</param>
    ''' <param name="removed">返回被剔除的样本行下标</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function AutoPointDeletion(table As NumericTable,
                                      Optional weighted As Weights = Nothing,
                                      Optional max As Integer = -1,
                                      Optional keepsLowestPoint As Boolean = False,
                                      Optional removesZeroY As Boolean = False,
                                      Optional ByRef removed As Integer() = Nothing,
                                      Optional y As String = Nothing,
                                      Optional Silent As Boolean = False) As IFitted

        Dim xv As Double() = SingleFeature(table, NameOf(AutoPointDeletion))
        Dim yv As Double() = ResponseVector(table, y, NameOf(AutoPointDeletion))
        Dim points As PointF() = xv _
            .SeqIterator _
            .Select(Function(i) New PointF(CSng(i.value), CSng(yv(i.i)))) _
            .ToArray
        ' 注意：DoubleLinear.AutoPointDeletion 的 removed 参数类型是
        ' Microsoft.VisualBasic.Language.List(Of T)，而不是系统默认的泛型集合，
        ' 因此这里必须使用完全限定名，避免在运行时出现无效的类型转换异常
        Dim removedPoints As New Microsoft.VisualBasic.Language.List(Of PointF)

#Disable Warning BC40000
        ' 这里是对既有的点集入口做转调，警告由表入口进行替代
        Dim fit As IFitted = DoubleLinear.AutoPointDeletion(points, weighted, max, removedPoints, keepsLowestPoint, removesZeroY)
#Enable Warning BC40000

        removed = MapRemovedRows(xv, yv, removedPoints)

        Return fit
    End Function

    ''' <summary>
    ''' 将 ``(x, y)`` 点对被剔除的记录回溯映射为表中的样本行下标。
    ''' 
    ''' 由于 ``PointF`` 只能够保存 ``Single`` 精度的数值，
    ''' 因此这里也会使用单精度进行比较。
    ''' </summary>
    Private Function MapRemovedRows(xv As Double(), yv As Double(), removed As IEnumerable(Of PointF)) As Integer()
        Dim used As New HashSet(Of Integer)
        Dim idx As New List(Of Integer)

        For Each p As PointF In removed
            For i As Integer = 0 To xv.Length - 1
                If used.Contains(i) Then
                    Continue For
                End If
                If CSng(xv(i)) = p.X AndAlso CSng(yv(i)) = p.Y Then
                    Call used.Add(i)
                    Call idx.Add(i)
                    Exit For
                End If
            Next
        Next

        Return idx.OrderBy(Function(i) i).ToArray
    End Function

    ''' <summary>
    ''' 高斯-牛顿法求解非线性曲线函数的参数。使用表的唯一特征列作为 x。
    ''' </summary>
    ''' <param name="table">特征矩阵为 x、指定标签列为 y 的二维表</param>
    ''' <param name="fitFunction">需要拟合的曲线函数 ``f(x, args)``</param>
    ''' <param name="argumentSize">曲线函数之中待求解的参数个数</param>
    ''' <param name="initialArgs">参数的初始值，为空的时候由求解器随机生成</param>
    ''' <param name="maxIterations">最大的迭代次数</param>
    ''' <param name="rmseTol">均方根误差的收敛容差</param>
    ''' <param name="iterTol">迭代量的收敛容差</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns>求解出来的曲线函数参数</returns>
    ''' <remarks>
    ''' 本方法只是把二维表适配为 <see cref="GaussNewtonSolver"/> 所需要的点序列，
    ''' 求解器自身对于「非方阵」的雅可比矩阵存在维度检查缺陷（属于既有的实现问题），
    ''' 当样本数量与参数个数不相等的时候可能会抛出维度错误；
    ''' 一般的线性/多项式建模请优先使用 <see cref="MultipleLinearRegression"/>
    ''' 或者 <see cref="PolyFit"/>。
    ''' </remarks>
    <Extension>
    Public Function GaussNewtonFit(table As NumericTable,
                                   fitFunction As GaussNewtonSolver.FitFunction,
                                   argumentSize As Integer,
                                   Optional initialArgs As Double() = Nothing,
                                   Optional maxIterations As Integer = 1000,
                                   Optional rmseTol As Double = 0.00000001,
                                   Optional iterTol As Double = 0.000000000000001,
                                   Optional y As String = Nothing,
                                   Optional Silent As Boolean = False) As Double()

        If fitFunction Is Nothing Then
            Throw New ArgumentNullException(NameOf(fitFunction))
        End If

        Dim xv As Double() = SingleFeature(table, NameOf(GaussNewtonFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(GaussNewtonFit))
        Dim points As DataPoint() = xv _
            .SeqIterator _
            .Select(Function(i) New DataPoint(i.value, yv(i.i))) _
            .ToArray
        Dim solver As New GaussNewtonSolver(fitFunction, maxIterations, rmseTol, iterTol)

        If initialArgs Is Nothing OrElse initialArgs.Length = 0 Then
            Return solver.Fit(points, argumentSize)
        Else
            Return solver.Fit(points, initialArgs)
        End If
    End Function

    ''' <summary>
    ''' 贝叶斯曲线拟合模型（多项式基函数 + 高斯先验）。
    ''' 
    ''' 要求表中只有一个特征列。
    ''' </summary>
    ''' <param name="table">特征矩阵为 x、指定标签列为 t 的二维表</param>
    ''' <param name="m">拟合曲线的阶数</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns></returns>
    <Extension>
    Public Function BayesianCurveFit(table As NumericTable,
                                     m As Integer,
                                     Optional y As String = Nothing,
                                     Optional Silent As Boolean = False) As BayesianCurveFitting

        Dim xv As Double() = SingleFeature(table, NameOf(BayesianCurveFit))
        Dim yv As Double() = ResponseVector(table, y, NameOf(BayesianCurveFit))

        Return New BayesianCurveFitting(xv, yv, m)
    End Function

#End Region

#Region "feature projection && LMA inputs"

    ''' <summary>
    ''' 对响应变量按照样本序号（``1..n``）进行多项式投影，返回投影系数向量。
    ''' 
    ''' 这是 <see cref="FeatureProjection.Project(Vector, Integer)"/> 的表入口实现。
    ''' </summary>
    ''' <param name="table">提供响应变量的二维表</param>
    ''' <param name="dimension">投影的维度（多项式的阶数）</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <param name="Silent">是否关闭算法内部的进度输出</param>
    ''' <returns>投影系数向量</returns>
    <Extension>
    Public Function FeatureProjection(table As NumericTable,
                                      dimension As Integer,
                                      Optional y As String = Nothing,
                                      Optional Silent As Boolean = False) As Vector

        Dim yv As Double() = ResponseVector(table, y, NameOf(FeatureProjection))

        Return FeatureProjectionNS.Project(yv.AsVector, dimension)
    End Function

    ''' <summary>
    ''' 将二维表转换为 <see cref="LMA.FitInput"/> 的集合，以提供给
    ''' <see cref="LMA.NonLinearFit"/> 使用。
    ''' 
    ''' 注意：<see cref="LMA.NonLinearFit"/> 目前尚未实现（会抛出
    ''' <see cref="NotImplementedException"/>），因此这里只是完成数据形态的适配。
    ''' </summary>
    ''' <param name="table">特征矩阵为自变量、指定标签列为 y 的二维表</param>
    ''' <param name="y">响应变量所在的标签列名，缺省为第一个标签列</param>
    ''' <returns></returns>
    <Extension>
    Public Function AsFitInputs(table As NumericTable, Optional y As String = Nothing) As LMA.FitInput()
        Dim xm As Double()() = FeatureMatrix(table, NameOf(AsFitInputs))
        Dim yv As Double() = ResponseVector(table, y, NameOf(AsFitInputs))
        Dim names As String() = FeatureNameList(table)
        Dim n As Integer = table.nsamples
        Dim inputs As LMA.FitInput() = New LMA.FitInput(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim factors As New Dictionary(Of String, Double)

            For j As Integer = 0 To names.Length - 1
                factors(names(j)) = xm(i)(j)
            Next

            inputs(i) = New LMA.FitInput With {
                .factors = factors,
                .y = yv(i)
            }
        Next

        Return inputs
    End Function

#End Region
End Module

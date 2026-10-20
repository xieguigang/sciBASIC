#Region "Microsoft.VisualBasic::069b6312a79e771279d3290e6c090929, Data_science\Mathematica\Math\DataFittings\LMA.vb"

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
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 40
    '    Code Lines: 16 (40.00%)
    ' Comment Lines: 19 (47.50%)
    '    - Xml Docs: 89.47%
    ' 
    '   Blank Lines: 5 (12.50%)
    '     File Size: 1.28 KB


    ' Module LMA
    ' 
    '     Function: NonLinearFit
    '     Structure FitInput
    ' 
    '         Function: ToString
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Bootstrapping.LevenbergMarquardt
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.Serialization.JSON
Imports std = System.Math

''' <summary>
''' 任意非线性模型函数：由参数向量和单个样本的自变量向量计算出模型的输出值。
''' 
''' 参数的顺序由 <see cref="NonLinearFitResult.ParameterNames"/> 决定，自变量的
''' 顺序由 <see cref="NonLinearFitResult.FactorNames"/> 决定。
''' </summary>
''' <param name="params">待优化的模型参数向量</param>
''' <param name="x">单个样本的自变量向量</param>
''' <returns>模型在该样本处的估计输出值</returns>
Public Delegate Function NonLinearModel(params As Double(), x As Double()) As Double

''' <summary>
''' 驱动既有 <see cref="LmSolver"/> 内核的非线性模型桥。
''' 
''' 任意一个 <see cref="NonLinearModel"/> 委托经由这个桥包装之后，就满足了
''' <see cref="LmScalarModel"/> 所要求的「逐样本给出模型值、一阶导数与二阶
''' 导数」的契约，从而可以直接交给 <see cref="LmSumSquaresError"/> 求解。
''' 
''' 导数一律采用中心差分近似，差分步长随着参数自身的量纲自适应：
''' 
''' ```
''' h = eps * (1 + |p|)
''' ```
''' </summary>
Friend Class LmFitModel : Implements LmScalarModel

    ''' <summary>
    ''' 自变量矩阵，行是样本、列是自变量
    ''' </summary>
    ReadOnly xm As Double()()
    ''' <summary>
    ''' 观测值向量
    ''' </summary>
    ReadOnly yv As Double()
    ''' <summary>
    ''' 被包装的非线性模型
    ''' </summary>
    ReadOnly fn As NonLinearModel
    ''' <summary>
    ''' 差分步长基准
    ''' </summary>
    ReadOnly fdEps As Double

    Sub New(xm As Double()(), yv As Double(), fn As NonLinearModel, fdEps As Double)
        Me.xm = xm
        Me.yv = yv
        Me.fn = fn
        Me.fdEps = fdEps
    End Sub

    Public ReadOnly Property MeasuredData As Double() Implements LmScalarModel.MeasuredData
        Get
            Return yv
        End Get
    End Property

    Public Function eval(dataIdx As Integer, optParams As Double()) As Double Implements LmScalarModel.eval
        Return fn(optParams, xm(dataIdx))
    End Function

    Public Function jacobian(dataIdx As Integer, optParams As Double()) As Double() Implements LmScalarModel.jacobian
        Dim n As Integer = optParams.Length
        Dim row As Double() = xm(dataIdx)
        Dim jac As Double() = New Double(n - 1) {}
        Dim k As Integer

        For k = 0 To n - 1
            Dim saved As Double = optParams(k)
            Dim h As Double = fdEps * (1.0 + std.Abs(saved))

            optParams(k) = saved + h
            Dim fPlus As Double = fn(optParams, row)
            optParams(k) = saved - h
            Dim fMinus As Double = fn(optParams, row)
            optParams(k) = saved

            jac(k) = (fPlus - fMinus) / (2.0 * h)
        Next

        Return jac
    End Function

    Public Function hessian(dataIdx As Integer, optParams As Double()) As Double()() Implements LmScalarModel.hessian
        Dim n As Integer = optParams.Length
        Dim row As Double() = xm(dataIdx)
        Dim hes As Double()() = RectangularArray.Matrix(Of Double)(n, n)
        Dim steps As Double() = New Double(n - 1) {}
        Dim fPlus As Double() = New Double(n - 1) {}
        Dim fMinus As Double() = New Double(n - 1) {}
        Dim f0 As Double = fn(optParams, row)
        Dim i As Integer
        Dim j As Integer

        ' 步长先算好，后面每次差分都要用到同一个 h
        For i = 0 To n - 1
            steps(i) = fdEps * (1.0 + std.Abs(optParams(i)))
        Next

        For i = 0 To n - 1
            Dim saved As Double = optParams(i)

            optParams(i) = saved + steps(i)
            fPlus(i) = fn(optParams, row)
            optParams(i) = saved - steps(i)
            fMinus(i) = fn(optParams, row)
            optParams(i) = saved
        Next

        ' 对角：二阶中心差分
        For i = 0 To n - 1
            hes(i)(i) = (fPlus(i) - 2.0 * f0 + fMinus(i)) / (steps(i) * steps(i))
        Next

        ' 非对角：四个角点的混合差分，并按对称性只计算上三角
        For i = 0 To n - 1
            For j = i + 1 To n - 1
                Dim savedI As Double = optParams(i)
                Dim savedJ As Double = optParams(j)

                optParams(i) = savedI + steps(i)
                optParams(j) = savedJ + steps(j)
                Dim fpp As Double = fn(optParams, row)
                optParams(j) = savedJ - steps(j)
                Dim fpm As Double = fn(optParams, row)
                optParams(i) = savedI - steps(i)
                Dim fmm As Double = fn(optParams, row)
                optParams(j) = savedJ + steps(j)
                Dim fmp As Double = fn(optParams, row)

                optParams(i) = savedI
                optParams(j) = savedJ

                Dim d As Double = (fpp - fpm - fmp + fmm) / (4.0 * steps(i) * steps(j))

                hes(i)(j) = d
                hes(j)(i) = d
            Next
        Next

        Return hes
    End Function
End Class

''' <summary>
''' 非线性最小二乘拟合的结果
''' 
''' 这个类型保存了拟合出来的参数、拟合优度、残差与预测模型，与线性拟合的
''' <see cref="FitResult"/> 风格一致。注意：非线性模型没有多项式形式，
''' 因此这里并没有实现 <see cref="IFitted"/> 接口。
''' </summary>
Public Class NonLinearFitResult

    ''' <summary>
    ''' 拟合好的模型，可以直接用于预测；委托无法序列化，故而不参与 JSON 输出
    ''' </summary>
    ''' <returns></returns>
    <IgnoreDataMember>
    Public Property Model As NonLinearModel

    ''' <summary>
    ''' 模型的文本形式（公式字符串入口时为用户给出的原始公式，
    ''' 默认幂律入口时为自动生成的幂律公式）
    ''' </summary>
    ''' <returns></returns>
    Public Property Formula As String

    ''' <summary>
    ''' 自变量的名称，同时也是模型自变量向量的顺序
    ''' </summary>
    ''' <returns></returns>
    Public Property FactorNames As String()

    ''' <summary>
    ''' 模型参数的名称，同时也是参数向量 <see cref="Values"/> 的顺序
    ''' </summary>
    ''' <returns></returns>
    Public Property ParameterNames As String()

    ''' <summary>
    ''' 参数名到参数值的映射
    ''' </summary>
    ''' <returns></returns>
    Public Property Parameters As Dictionary(Of String, Double)

    ''' <summary>
    ''' 残差平方和
    ''' </summary>
    ''' <returns></returns>
    Public Property SSE As Double

    ''' <summary>
    ''' 均方根误差
    ''' </summary>
    ''' <returns></returns>
    Public Property RMSE As Double

    ''' <summary>
    ''' 决定系数 R2，等于 1 - SSE / SStotal
    ''' </summary>
    ''' <returns></returns>
    Public Property R2 As Double

    ''' <summary>
    ''' 残差向量，等于观测值减去拟合值
    ''' </summary>
    ''' <returns></returns>
    Public Property Residuals As Double()

    ''' <summary>
    ''' 每一个样本的模型拟合值
    ''' </summary>
    ''' <returns></returns>
    Public Property FittedValues As Double()

    ''' <summary>
    ''' 求解器实际消耗掉的迭代次数
    ''' </summary>
    ''' <returns></returns>
    Public Property Iterations As Integer

    ''' <summary>
    ''' 求解是否命中了收敛判据；False 表示迭代次数被耗尽，结果可能尚未收敛
    ''' </summary>
    ''' <returns></returns>
    Public Property Converged As Boolean

    ''' <summary>
    ''' 参数向量的缓存
    ''' </summary>
    Friend valuesVec As Double()

    ''' <summary>
    ''' 以向量形式获取拟合出来的参数值
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Values As Double()
        Get
            If valuesVec Is Nothing Then
                Return New Double() {}
            End If

            Return CType(valuesVec.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' 使用拟合好的模型对给定自变量做预测
    ''' </summary>
    ''' <param name="x">自变量向量，顺序见 <see cref="FactorNames"/></param>
    ''' <returns>模型的输出值</returns>
    Public Function GetY(ParamArray x As Double()) As Double
        If Model Is Nothing Then
            Throw New InvalidOperationException("the current fitting result does not have a valid model for prediction!")
        End If

        Return Model(valuesVec, x)
    End Function

    Public Overrides Function ToString() As String
        Dim summary As String = If(
            String.IsNullOrEmpty(Formula),
            $"R2 = {R2:F6}, RMSE = {RMSE:G6}, SSE = {SSE:G6}",
            $"{Formula} | R2 = {R2:F6}, RMSE = {RMSE:G6}, SSE = {SSE:G6}"
        )

        If Parameters Is Nothing OrElse Parameters.Count = 0 Then
            Return summary
        End If

        Return summary & " | " & Parameters.GetJson
    End Function
End Class

''' <summary>
''' ### Levenberg–Marquardt algorithm
''' 
''' In mathematics and computing, the Levenberg–Marquardt algorithm 
''' (LMA or just LM), also known as the damped least-squares (DLS) 
''' method, is used to solve non-linear least squares problems. 
''' 
''' These minimization problems arise especially in least squares 
''' curve fitting.
''' 
''' 本模块提供了三种拟合入口：
''' 
''' 1. 不给出模型的入口，拟合幂律（Cobb-Douglas / 异速生长）模型
'''    ``y = p0 * x1^p1 * x2^p2 * ...``；
''' 2. 给出 <see cref="NonLinearModel"/> 委托的入口；
''' 3. 给出公式字符串与待估参数名的入口，例如 ``a*exp(-b*x1) + c``。
''' 
''' 三种入口最终都会收敛到同一条求解路径之上。
''' </summary>
''' <remarks>
''' solver implements at <see cref="LmSolver"/>
''' </remarks>
Public Module LMA

    ''' <summary>
    ''' 中心差分步长的基准值
    ''' </summary>
    Const fdEpsilon As Double = 0.00001
    ''' <summary>
    ''' 阻尼因子的初值系数
    ''' </summary>
    Const damping As Double = 0.001
    ''' <summary>
    ''' 梯度与参数变化的收敛容差
    ''' </summary>
    Const eps As Double = 0.00000001
    ''' <summary>
    ''' 仅用于查询内建符号的表达式引擎（模块成员天然是 Shared 的）
    ''' </summary>
    Private ReadOnly symbolEngine As New ExpressionEngine()

    Public Structure FitInput
        ''' <summary>
        ''' The matrix row
        ''' </summary>
        Dim factors As Dictionary(Of String, Double)
        ''' <summary>
        ''' The non-linear function result output 
        ''' </summary>
        Dim y As Double

        Public Overrides Function ToString() As String
            Return $"f(X) = {y} = f({factors.GetJson})"
        End Function
    End Structure

    ''' <summary>
    ''' 不给出模型函数时的缺省入口：拟合幂律（Cobb-Douglas / 异速生长）模型
    ''' 
    ''' ```
    ''' y = p0 * x1^p1 * x2^p2 * ... * xm^pm
    ''' ```
    ''' 
    ''' 这个模型对指数参数是非线性的，因此必须使用迭代求解。参数初值由对数域的
    ''' 普通最小二乘给出（对模型两边取对数之后它就变成了一个线性模型）。
    ''' 
    ''' 由于该模型要求对自变量取对数，因此**全部自变量与响应变量都必须为正数**，
    ''' 否则会抛出 <see cref="ArgumentException"/>；存在非正数的数据时请自行
    ''' 对数据进行平移，或者改用显式给出模型的那一个 NonLinearFit 入口。
    ''' </summary>
    ''' <param name="matrix">样本集合，每一个样本的自变量由 <see cref="FitInput.factors"/> 给出</param>
    ''' <param name="iterations">求解器所允许的最大迭代次数</param>
    ''' <returns>拟合结果，参数依次为 ``p0`` 以及各个自变量的指数</returns>
    <Extension>
    Public Function NonLinearFit(matrix As IEnumerable(Of FitInput), Optional iterations% = 2000) As NonLinearFitResult
        Dim inputs As FitInput() = LoadInputs(matrix)
        Dim xm As Double()() = Nothing
        Dim yv As Double() = Nothing
        Dim names As String() = Nothing

        UnpackInputs(inputs, NameOf(NonLinearFit), xm, yv, names)
        RequirePositive(xm, yv, names, NameOf(NonLinearFit))

        Dim size As Integer = names.Length
        Dim paramNames As String() = New String(size) {}
        Dim terms As New List(Of String)

        paramNames(0) = "p0"
        terms.Add(paramNames(0))

        For i As Integer = 0 To size - 1
            paramNames(i + 1) = "p" & (i + 1)
            terms.Add($"{names(i)}^{paramNames(i + 1)}")
        Next

        Dim model As NonLinearModel = PowerLawModel()

        Return RunFit(
            xm:=xm,
            yv:=yv,
            model:=model,
            guess:=PowerLawGuess(xm, yv),
            paramNames:=paramNames,
            factorNames:=names,
            formula:=String.Join(" * ", terms),
            iterations:=iterations,
            approxHessian:=True
        )
    End Function

    ''' <summary>
    ''' 给出模型委托的非线性拟合入口。
    ''' 
    ''' 自变量向量的顺序取自第一个样本的 <see cref="FitInput.factors"/> 键序，
    ''' 并且会逐样本校验键集是否完全一致。
    ''' </summary>
    ''' <param name="matrix">样本集合</param>
    ''' <param name="model">待拟合的非线性模型</param>
    ''' <param name="guess">参数的初始猜测值，缺省时全部取 1</param>
    ''' <param name="names">参数名，缺省时自动生成 ``p1, p2, ...``；
    ''' <paramref name="guess"/> 为空的时候必须给出这个参数</param>
    ''' <param name="iterations">求解器所允许的最大迭代次数</param>
    ''' <param name="approxHessian">
    ''' 是否使用高斯-牛顿近似（H ≈ J'J）代替精确的海塞矩阵。
    ''' 缺省为 True，这也是 Levenberg-Marquardt 的标准做法；
    ''' 置为 False 会对每一个样本多算 O(p^2) 次模型求值
    ''' </param>
    ''' <returns>拟合结果</returns>
    <Extension>
    Public Function NonLinearFit(matrix As IEnumerable(Of FitInput),
                                 model As NonLinearModel,
                                 Optional guess As Double() = Nothing,
                                 Optional names As String() = Nothing,
                                 Optional iterations% = 2000,
                                 Optional approxHessian As Boolean = True) As NonLinearFitResult

        If model Is Nothing Then
            Throw New ArgumentNullException(NameOf(model))
        End If

        Dim inputs As FitInput() = LoadInputs(matrix)
        Dim xm As Double()() = Nothing
        Dim yv As Double() = Nothing
        Dim factorNames As String() = Nothing

        UnpackInputs(inputs, NameOf(NonLinearFit), xm, yv, factorNames)

        Dim size As Integer = 0

        If guess IsNot Nothing Then
            size = guess.Length
        ElseIf names IsNot Nothing Then
            size = names.Length
        Else
            Throw New ArgumentException(
                $"the '{NameOf(NonLinearFit)}' requires either the initial guess vector or the parameter names to determine the size of the model parameters!",
                NameOf(guess)
            )
        End If

        If names Is Nothing Then
            names = New String(size - 1) {}

            For i As Integer = 0 To size - 1
                names(i) = "p" & (i + 1)
            Next
        ElseIf names.Length <> size Then
            Throw New ArgumentException(
                $"the size of the parameter names ({names.Length}) is not matched with the size of the initial guess ({size})!",
                NameOf(names)
            )
        End If

        If guess Is Nothing Then
            guess = New Double(size - 1) {}

            For i As Integer = 0 To size - 1
                guess(i) = 1.0
            Next
        End If

        Return RunFit(
            xm:=xm,
            yv:=yv,
            model:=model,
            guess:=guess,
            paramNames:=names,
            factorNames:=factorNames,
            formula:=Nothing,
            iterations:=iterations,
            approxHessian:=approxHessian
        )
    End Function

    ''' <summary>
    ''' 给出公式字符串的非线性拟合入口。
    ''' 
    ''' 公式之中的自变量按**列名**绑定，例如表的特征列名为 ``x1`` 与 ``x2`` 时，
    ''' 可以写作 ``a*exp(-b*x1) + c*log(x2)``；<paramref name="parameters"/>
    ''' 所给出的符号是待估计的参数，其余符号必须在 <see cref="FitInput.factors"/> 
    ''' 的键名之中，否则会抛出 <see cref="ArgumentException"/>。
    ''' 
    ''' 公式支持的内建函数与 <see cref="ExpressionEngine"/> 一致
    ''' （``exp``/``log``/``ln``/``pow``/``sqrt``/``sin``/``cos`` 等等）。
    ''' </summary>
    ''' <param name="matrix">样本集合</param>
    ''' <param name="formula">模型的公式文本</param>
    ''' <param name="parameters">待估计的参数符号名</param>
    ''' <param name="guess">参数的初始猜测值，缺省时全部取 1</param>
    ''' <param name="iterations">求解器所允许的最大迭代次数</param>
    ''' <param name="approxHessian">是否使用高斯-牛顿近似代替精确的海塞矩阵</param>
    ''' <returns>拟合结果</returns>
    <Extension>
    Public Function NonLinearFit(matrix As IEnumerable(Of FitInput),
                                 formula As String,
                                 parameters As String(),
                                 Optional guess As Double() = Nothing,
                                 Optional iterations% = 2000,
                                 Optional approxHessian As Boolean = True) As NonLinearFitResult

        If String.IsNullOrEmpty(formula) Then
            Throw New ArgumentNullException(NameOf(formula))
        End If
        If parameters Is Nothing OrElse parameters.Length = 0 Then
            Throw New ArgumentException(
                $"the '{NameOf(NonLinearFit)}' requires at least one parameter symbol to be optimized!",
                NameOf(parameters)
            )
        End If

        Dim inputs As FitInput() = LoadInputs(matrix)
        Dim xm As Double()() = Nothing
        Dim yv As Double() = Nothing
        Dim rawNames As String() = Nothing

        UnpackInputs(inputs, NameOf(NonLinearFit), xm, yv, rawNames)

        Dim factorNames As String() = SanitizeFactorNames(rawNames, parameters)
        Dim model As NonLinearModel = CompileFormula(formula, parameters, factorNames)

        If guess Is Nothing Then
            guess = New Double(parameters.Length - 1) {}

            For i As Integer = 0 To guess.Length - 1
                guess(i) = 1.0
            Next
        ElseIf guess.Length <> parameters.Length Then
            Throw New ArgumentException(
                $"the size of the initial guess ({guess.Length}) is not matched with the size of the parameters ({parameters.Length})!",
                NameOf(guess)
            )
        End If

        Return RunFit(
            xm:=xm,
            yv:=yv,
            model:=model,
            guess:=guess,
            paramNames:=parameters,
            factorNames:=factorNames,
            formula:=formula,
            iterations:=iterations,
            approxHessian:=approxHessian
        )
    End Function

    ''' <summary>
    ''' 物化并且校验输入的样本集合
    ''' </summary>
    Private Function LoadInputs(matrix As IEnumerable(Of FitInput)) As FitInput()
        If matrix Is Nothing Then
            Throw New ArgumentNullException(NameOf(matrix))
        End If

        Dim inputs As FitInput() = matrix.ToArray

        If inputs.Length = 0 Then
            Throw New ArgumentException("the non-linear fitting requires at least one sample data input!", NameOf(matrix))
        End If

        Return inputs
    End Function

    ''' <summary>
    ''' 把样本集合拆成自变量矩阵、观测向量与自变量名称。
    ''' 
    ''' 自变量的顺序以第一个样本的键序为准：字典的枚举顺序并不是契约保证，
    ''' 因此这里必须对后续样本的键集做一致性校验。
    ''' </summary>
    Private Sub UnpackInputs(inputs As FitInput(), caller As String,
                             ByRef xm As Double()(),
                             ByRef yv As Double(),
                             ByRef names As String())

        Dim n As Integer = inputs.Length

        If inputs(0).factors Is Nothing OrElse inputs(0).factors.Count = 0 Then
            Throw New ArgumentException(
                $"the '{caller}' requires at least one factor column in the sample data!",
                NameOf(inputs)
            )
        End If

        names = inputs(0).factors.Keys.ToArray

        Dim m As Integer = names.Length

        For i As Integer = 1 To n - 1
            If inputs(i).factors Is Nothing OrElse inputs(i).factors.Count <> m Then
                Throw New ArgumentException(
                    $"the factor columns of the sample data at index {i} is not matched with the first sample!",
                    NameOf(inputs)
                )
            End If

            For Each name As String In names
                If Not inputs(i).factors.ContainsKey(name) Then
                    Throw New ArgumentException(
                        $"the sample data at index {i} does not have the factor column '{name}'!",
                        NameOf(inputs)
                    )
                End If
            Next
        Next

        xm = RectangularArray.Matrix(Of Double)(n, m)
        yv = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To m - 1
                xm(i)(j) = inputs(i).factors(names(j))
            Next

            yv(i) = inputs(i).y
        Next
    End Sub

    ''' <summary>
    ''' 缺省入口的幂律模型：``p0 * x1^p1 * x2^p2 * ...``
    ''' 
    ''' 求值走对数域（``p0 * exp(sum(pj * log(xj)))``）以避免底数为负数时的
    ''' <see cref="Double.NaN"/>，同时把乘法与乘方化成了加法与乘法。
    ''' </summary>
    Private Function PowerLawModel() As NonLinearModel
        Return Function(params As Double(), xi As Double())
                   Dim s As Double = 0.0

                   For j As Integer = 0 To xi.Length - 1
                       s += params(j + 1) * std.Log(xi(j))
                   Next

                   Return params(0) * std.Exp(s)
               End Function
    End Function

    ''' <summary>
    ''' 幂律模型的参数初值：在对数域上做一次普通最小二乘。
    ''' 
    ''' 对模型两边取对数之后它变成了 ``ln(y) = ln(p0) + sum(pj * ln(xj))``，
    ''' 是一个对参数线性的模型，其 OLS 解就是幂律模型参数的极好初值。
    ''' </summary>
    Private Function PowerLawGuess(xm As Double()(), yv As Double()) As Double()
        Dim n As Integer = yv.Length
        Dim m As Integer = xm(0).Length
        Dim guess As Double() = New Double(m) {}
        Dim beta As Double() = Nothing

        Try
            Dim A As Double(,) = New Double(n - 1, m) {}
            Dim b As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                A(i, 0) = 1.0

                For j As Integer = 0 To m - 1
                    A(i, j + 1) = std.Log(xm(i)(j))
                Next

                b(i) = std.Log(yv(i))
            Next

            ' OLS 在正规方程奇异的时候会向控制台打印警告，
            ' 初值计算是内部细节，这里临时把输出静音掉
            Dim stdout As TextWriter = Console.Out

            Try
                Console.SetOut(TextWriter.Null)
                beta = OLS.Solve(A, b, n, m + 1)
            Finally
                Console.SetOut(stdout)
            End Try
        Catch ex As Exception
            beta = Nothing
        End Try

        If beta Is Nothing Then
            ' 退化路径：常数模型 y = mean(y)
            guess(0) = yv.Average()

            If guess(0) <= 0 Then
                guess(0) = 1.0
            End If

            Return guess
        End If

        guess(0) = std.Exp(beta(0))

        For j As Integer = 1 To m
            guess(j) = beta(j)
        Next

        Return guess
    End Function

    ''' <summary>
    ''' 幂律入口要求全部自变量与响应变量为正数
    ''' </summary>
    Private Sub RequirePositive(xm As Double()(), yv As Double(), names As String(), caller As String)
        For i As Integer = 0 To yv.Length - 1
            If yv(i) <= 0 Then
                Throw New ArgumentException(
                    $"the default power-law model of the '{caller}' requires a positive response value, but the observed value at index {i} is {yv(i)}!",
                    NameOf(yv)
                )
            End If

            For j As Integer = 0 To names.Length - 1
                If xm(i)(j) <= 0 Then
                    Throw New ArgumentException(
                        $"the default power-law model of the '{caller}' requires a positive factor value, but the factor '{names(j)}' at index {i} is {xm(i)(j)}!",
                        NameOf(xm)
                    )
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 把公式文本编译成模型委托。
    ''' 
    ''' 公式只解析一次，之后复用同一个 <see cref="ExpressionEngine"/> 实例，
    ''' 逐样本把自变量与参数写进符号表之后再求值。
    ''' </summary>
    Private Function CompileFormula(formula As String, parameters As String(), factorNames As String()) As NonLinearModel
        Dim parsed As Expression

        Try
            parsed = ExpressionEngine.Parse(formula)
        Catch ex As Exception
            Throw New ArgumentException($"the model formula '{formula}' is not a valid math expression: {ex.Message}", NameOf(formula))
        End Try

        If parsed Is Nothing Then
            Throw New ArgumentException($"the model formula '{formula}' is not a valid math expression!", NameOf(formula))
        End If

        Dim env As New ExpressionEngine()

        ' 符号校验是尽力而为的：表达式树的部分节点类型可能无法给出符号列表
        Try
            For Each symbol As String In parsed.GetVariableSymbols()
                If env.CheckFunction(symbol) Then
                    Continue For
                End If
                If parameters.Contains(symbol) Then
                    Continue For
                End If
                If factorNames.Contains(symbol) Then
                    Continue For
                End If

                Throw New ArgumentException(
                    $"the symbol '{symbol}' of the model formula '{formula}' is neither a model parameter nor a factor column!",
                    NameOf(formula)
                )
            Next
        Catch ex As ArgumentException
            Throw
        Catch ex As Exception
            ' 取不到符号列表的时候不做校验，改由下面的试算来兜底
        End Try

        ' 试算一次：把全部符号都放进去求值，缺失的符号会在这里暴露出来
        For Each name As String In factorNames
            env.SetSymbol(name, 1.0)
        Next
        For Each name As String In parameters
            env.SetSymbol(name, 1.0)
        Next

        Try
            Call env.Evaluate(parsed)
        Catch ex As KeyNotFoundException
            Throw New ArgumentException(
                $"the model formula '{formula}' can not be evaluated, some of the symbols are unknown to the expression engine!",
                NameOf(formula)
            )
        Catch ex As Exception
            Throw New ArgumentException(
                $"the model formula '{formula}' can not be evaluated: {ex.Message}",
                NameOf(formula)
            )
        End Try

        Dim aliases As String() = factorNames
        Dim symbols As String() = parameters

        Return Function(params As Double(), xi As Double())
                   For j As Integer = 0 To aliases.Length - 1
                       Call env.SetSymbol(aliases(j), xi(j))
                   Next
                   For k As Integer = 0 To symbols.Length - 1
                       Call env.SetSymbol(symbols(k), params(k))
                   Next

                   Return env.Evaluate(parsed)
               End Function
    End Function

    ''' <summary>
    ''' 把自变量的名字规范成表达式引擎可以识别的标识符。
    ''' 
    ''' 表达式引擎的分词器要求标识符以字母开头，因此诸如 ``1, 2, 3`` 这样的
    ''' 数字列名（无列名的表就是这种回退名）会被自动替换成 ``x1, x2, ...``。
    ''' </summary>
    Private Function SanitizeFactorNames(rawNames As String(), parameters As String()) As String()
        Dim aliases As String() = New String(rawNames.Length - 1) {}
        Dim used As New HashSet(Of String)

        For Each name As String In parameters
            If Not IsIdentifier(name) Then
                Throw New ArgumentException(
                    $"the parameter symbol '{name}' is not a valid identifier of the expression engine, it must start with a letter!",
                    NameOf(parameters)
                )
            End If

            used.Add(name)
        Next

        Dim nextAlias As Integer = 1

        For i As Integer = 0 To rawNames.Length - 1
            Dim raw As String = rawNames(i)

            If IsIdentifier(raw) AndAlso Not used.Contains(raw) AndAlso Not IsBuiltinSymbol(raw) Then
                aliases(i) = raw
            Else
                Dim candidate As String = Nothing

                Do
                    candidate = "x" & nextAlias
                    nextAlias += 1
                Loop While used.Contains(candidate)

                aliases(i) = candidate
            End If

            used.Add(aliases(i))
        Next

        Return aliases
    End Function

    ''' <summary>
    ''' 是否是一个合法的表达式标识符：以字母开头，且只由字母、数字与下划线组成
    ''' </summary>
    Private Function IsIdentifier(name As String) As Boolean
        If String.IsNullOrEmpty(name) Then
            Return False
        End If
        If Not Char.IsLetter(name.First) Then
            Return False
        End If

        For Each c As Char In name
            If Not (Char.IsLetterOrDigit(c) OrElse c = "_"c) Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' 是否与表达式引擎的内建符号（常数或者函数）重名
    ''' </summary>
    Private Function IsBuiltinSymbol(name As String) As Boolean
        Return symbolEngine.CheckFunction(name) OrElse name = "PI" OrElse name = "E"
    End Function

    ''' <summary>
    ''' 三类入口共用的求解路径：构造模型桥 → 交给 <see cref="LmSolver"/> 迭代 → 统计拟合结果
    ''' </summary>
    Private Function RunFit(xm As Double()(),
                            yv As Double(),
                            model As NonLinearModel,
                            guess As Double(),
                            paramNames As String(),
                            factorNames As String(),
                            formula As String,
                            iterations As Integer,
                            approxHessian As Boolean) As NonLinearFitResult

        If iterations <= 0 Then
            Throw New ArgumentException("the max iteration number must be a positive integer!", NameOf(iterations))
        End If

        Dim n As Integer = yv.Length
        Dim size As Integer = guess.Length
        Dim opt As Double() = New Double(size - 1) {}

        For i As Integer = 0 To size - 1
            If Double.IsNaN(guess(i)) OrElse Double.IsInfinity(guess(i)) Then
                Throw New ArgumentException($"the initial guess of the parameter '{paramNames(i)}' is not a finite number!", NameOf(guess))
            End If

            opt(i) = guess(i)
        Next

        ' 求解器会就地修改参数向量，这里给它一份工作副本
        Dim lmModel As New LmFitModel(xm, yv, model, fdEpsilon)
        Dim err As New LmSumSquaresError(lmModel)
        Dim err0 As Double = err.eval(opt)

        If Double.IsNaN(err0) OrElse Double.IsInfinity(err0) Then
            Throw New ArgumentException(
                $"the given model produced a non-finite error value at the initial guess [{guess.JoinBy(", ")}], please check the model and the initial guess!",
                NameOf(guess)
            )
        End If

        Dim solver As New LmSolver(err, damping, iterations, eps, eps)

        Call solver.solve(opt, approxHessian)

        ' 统计
        Dim fitted As Double() = New Double(n - 1) {}
        Dim residual As Double() = New Double(n - 1) {}
        Dim mean As Double = yv.Average()
        Dim sse As Double = 0.0
        Dim sst As Double = 0.0

        For i As Integer = 0 To n - 1
            fitted(i) = model(opt, xm(i))
            residual(i) = yv(i) - fitted(i)

            sse += residual(i) * residual(i)

            Dim d As Double = yv(i) - mean

            sst += d * d
        Next

        Dim parameters As New Dictionary(Of String, Double)

        For i As Integer = 0 To size - 1
            parameters(paramNames(i)) = opt(i)
        Next

        Dim result As New NonLinearFitResult With {
            .Model = model,
            .Formula = formula,
            .FactorNames = factorNames,
            .ParameterNames = paramNames,
            .Parameters = parameters,
            .FittedValues = fitted,
            .Residuals = residual,
            .SSE = sse,
            .RMSE = std.Sqrt(sse / n),
            .R2 = If(sst > 0.0, 1.0 - sse / sst, If(sse <= 0.0, 1.0, 0.0)),
            .Iterations = solver.Iterations,
            .Converged = solver.Converged
        }

        ' 参数向量缓存供 GetY 直接使用；这里存副本，避免外部改动结果对象
        result.valuesVec = CType(opt.Clone(), Double())

        Return result
    End Function
End Module

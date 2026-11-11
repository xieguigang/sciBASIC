#Region "Microsoft.VisualBasic::e37abdef0d33d2ebf9373ab41b81b0da, Data_science\Mathematica\Math\DataFittings\Logistic\Logistic.vb"

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

#End Region

Imports System
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Logistic

    ''' <summary>
    ''' 二分类逻辑回归：以批量梯度下降最小化交叉熵
    ''' <c>J(w,b) = mean(softplus(z) - y*z)</c>，其中 <c>z = x*w + b</c>。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 训练、<c>predict</c>、<c>LogisticFit.GetY</c> 与 <c>LogisticFit.ErrorTest</c>
    ''' 全部返回同一个量：正类概率 <c>P(y=1|x)</c>。历史上 <c>predict</c> 返回
    ''' <c>1 - p</c> 而 <c>GetY</c> 返回 <c>p</c>，导致误差记录整体反向，现已统一。
    ''' </para>
    ''' <para>
    ''' 截距是独立标量（<c>Intercept</c>），不是额外的一列特征，因此
    ''' <c>Polynomial.Factors</c> 的长度始终等于特征数，不会破坏
    ''' <c>IFitted</c> 与 SHAP 解析解对系数向量的长度契约。
    ''' </para>
    ''' <para>
    ''' 数值内核集中在 <c>LogisticKernels</c>：批量 logits 走 FMA 点积，
    ''' sigmoid 走 <c>SimdMath.ExpInPlace</c>，梯度累加走 AXPY，均复用预分配缓冲。
    ''' </para>
    ''' </remarks>
    Public Class Logistic

        ''' <summary>学习率（初始值）。开启 <c>AdaptiveRate</c> 时只是起点。</summary>
        Public Property ALPHA As Double = 0.0001
        ''' <summary>最大梯度下降迭代次数</summary>
        Public Property ITERATIONS As Integer = 3000
        ''' <summary>收敛容差：相邻两轮成本改善小于该值即早停；小于等于 0 表示不早停</summary>
        Public Property Tolerance As Double = 0.0000001R
        ''' <summary>是否拟合独立截距项（决策面不必过原点）</summary>
        Public Property FitIntercept As Boolean = True
        ''' <summary>
        ''' 是否自适应调整学习率。默认开启：默认 <c>ALPHA</c> = 1e-4 在多数数据上
        ''' 收敛极慢，自适应会在成本下降时放大步长、上升时回退并减半。
        ''' </summary>
        Public Property AdaptiveRate As Boolean = True
        ''' <summary>是否使用分块并行计算梯度（默认关闭以保证结果完全可复现）</summary>
        Public Property UseParallel As Boolean = False
        ''' <summary>触发并行所需的最小工作量 <c>样本数 * 特征数</c></summary>
        Public Property MinParallelWork As Integer = 200000
        ''' <summary>是否先对训练数据做按列标准化（参数会随模型传给推理阶段）</summary>
        Public Property StandardizeData As Boolean = False
        ''' <summary>外部显式给定的标准化参数；为空时不进行变换</summary>
        Public Property Standardize As Standardization

        ''' <summary>学到的权重向量（保留既有 friend 字段以兼容同程序集代码）</summary>
        Friend theta As Vector

        ' VB 标识符大小写不敏感：下列私有字段刻意使用与公共属性不同的名字
        Private weightBuffer As Double()
        Private bias As Double
        Private costs As New List(Of Double)()
        Private finalAlpha As Double
        Private parallelCount As Integer
        Private println As Action(Of String)

        ''' <summary>学到的截距标量</summary>
        Public ReadOnly Property Intercept As Double
            Get
                Return bias
            End Get
        End Property

        ''' <summary>学到的权重副本（跨程序集可读取，用于回归对拍与模型检查）</summary>
        Public ReadOnly Property Weights As Double()
            Get
                If weightBuffer Is Nothing Then
                    Return Array.Empty(Of Double)()
                End If

                Return CType(weightBuffer.Clone(), Double())
            End Get
        End Property

        ''' <summary>每一轮迭代的交叉熵成本，用于验证单调下降与早停</summary>
        Public ReadOnly Property CostHistory As Double()
            Get
                Return costs.ToArray()
            End Get
        End Property

        ''' <summary>训练过程中实际使用的最终学习率</summary>
        Public ReadOnly Property EffectiveAlpha As Double
            Get
                Return finalAlpha
            End Get
        End Property

        ''' <summary>
        ''' 实际走分块并行路径的迭代次数。用于确认并行分支确实被触发，
        ''' 而不是因为工作量不足被静默降级成串行。
        ''' </summary>
        Public ReadOnly Property ParallelIterations As Integer
            Get
                Return parallelCount
            End Get
        End Property

        ''' <param name="n">特征数；仅保留为兼容参数，权重统一从 0 开始</param>
        ''' <param name="rate">学习率</param>
        ''' <param name="println">可选的进度输出回调，按固定轮数间隔采样成本值</param>
        Public Sub New(n As Integer, Optional rate As Double = 0.0001, Optional println As Action(Of String) = Nothing)
            Me.ALPHA = rate
            Me.println = println
            ' 说明：旧实现在这里做 ``theta = Vector.rand(n) * n``，但该值从未被
            ' train 使用（train 另建全零权重并在结束时覆盖 theta），属于死代码；
            ' 而且 [0, n) 的初值会让 sigmoid 饱和、梯度消失，因此不再保留。
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' 数值稳定的标量 sigmoid：<c>exp(-|z|)</c> 区间约减，避免 <c>exp(-z)</c> 上溢。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function sigmoid(z As Double) As Double
            If z >= 0.0R Then
                Dim e As Double = std.Exp(-z)
                Return 1.0R / (1.0R + e)
            Else
                Dim e As Double = std.Exp(z)
                Return e / (1.0R + e)
            End If
        End Function

        ''' <summary>
        ''' 校验训练数据的标签、维度与有限性，返回特征维数。
        ''' </summary>
        Private Shared Function ValidateInstances(raw As Instance()) As Integer
            If raw Is Nothing OrElse raw.Length = 0 Then
                Throw New ArgumentException("the logistic regression requires at least one training instance!", NameOf(raw))
            End If
            If raw(0) Is Nothing OrElse raw(0).x Is Nothing OrElse raw(0).x.Length = 0 Then
                Throw New ArgumentException("the first training instance has no feature values!", NameOf(raw))
            End If

            Dim dims As Integer = raw(0).x.Length

            For i As Integer = 0 To raw.Length - 1
                Dim item As Instance = raw(i)

                If item Is Nothing OrElse item.x Is Nothing Then
                    Throw New ArgumentException($"the training instance at index {i} is null!", NameOf(raw))
                End If
                If item.x.Length <> dims Then
                    Throw New ArgumentException($"feature size not agree at index {i}: {item.x.Length} vs {dims}!", NameOf(raw))
                End If
                If item.label <> 0.0R AndAlso item.label <> 1.0R Then
                    Throw New ArgumentException($"logistic regression requires 0/1 labels, but index {i} has label = {item.label}!", NameOf(raw))
                End If

                For j As Integer = 0 To dims - 1
                    If Double.IsNaN(item.x(j)) OrElse Double.IsInfinity(item.x(j)) Then
                        Throw New ArgumentException($"feature value at [{i},{j}] is not a finite number!", NameOf(raw))
                    End If
                Next
            Next

            Return dims
        End Function

        ''' <summary>
        ''' 训练模型。所有预测入口在训练后统一返回正类概率。
        ''' </summary>
        Public Function train(instances As IEnumerable(Of Instance)) As LogisticFit
            Dim raw As Instance() = If(instances Is Nothing,
                                       Array.Empty(Of Instance)(),
                                       instances.ToArray())
            Dim dims As Integer = ValidateInstances(raw)
            Dim size As Integer = raw.Length
            Dim norm As Standardization = Standardize
            Dim rows As Double()() = New Double(size - 1)() {}
            Dim labels As Double() = New Double(size - 1) {}

            For i As Integer = 0 To size - 1
                labels(i) = raw(i).label
                rows(i) = raw(i).x
            Next

            If StandardizeData Then
                ' 由训练数据自身产出 μ/σ，并把同一份参数随模型传给推理阶段
                Dim normalized As Instance() = Instance.ZScore(raw, norm)
                For i As Integer = 0 To size - 1
                    rows(i) = normalized(i).x
                Next
            ElseIf Not norm.IsEmpty Then
                For i As Integer = 0 To size - 1
                    rows(i) = norm.Apply(rows(i))
                Next
            End If

            Me.Standardize = norm
            Me.parallelCount = 0
            Me.weightBuffer = New Double(dims - 1) {}
            Me.bias = 0.0R
            Me.costs.Clear()
            Me.finalAlpha = ALPHA

            ' 一次性扁平化为行主序连续内存，摊到数千轮迭代可忽略
            Dim flat As Double(,) = LogisticKernels.Flatten(rows)

            ' 所有缓冲区都在循环外一次性分配并复用，避免每轮 GC
            Dim z As Double() = New Double(size - 1) {}
            Dim h As Double() = New Double(size - 1) {}
            Dim delta As Double() = New Double(size - 1) {}
            Dim gradient As Double() = New Double(dims - 1) {}
            Dim prevWeights As Double() = New Double(dims - 1) {}
            Dim partials As Double()() = Nothing
            Dim prevBias As Double = 0.0R
            Dim rate As Double = ALPHA
            Dim previousCost As Double = Double.PositiveInfinity
            Const MaxRate As Double = 1.0R
            Const MinRate As Double = 0.000000000001R
            Const Growth As Double = 1.2R

            For iteration As Integer = 0 To ITERATIONS - 1
                Call LogisticKernels.BatchLogits(flat, weightBuffer, bias, z)
                Call LogisticKernels.SigmoidInPlace(z, h)

                Dim cost As Double = LogisticKernels.CrossEntropy(z, h, labels)

                If Double.IsNaN(cost) OrElse Double.IsInfinity(cost) Then
                    Throw New InvalidOperationException($"the logistic training diverged at iteration {iteration}: cost = {cost}!")
                End If

                ' 自适应步长：成本上升说明上一轮步长过大，回退权重并减半；
                ' 回退轮不更新权重，成本曲线保持单调，下一轮用缩小后的步长重算。
                If AdaptiveRate AndAlso cost > previousCost Then
                    Call Array.Copy(prevWeights, weightBuffer, dims)
                    bias = prevBias
                    rate = std.Max(rate * 0.5R, MinRate)
                    Call costs.Add(previousCost)

                    If println IsNot Nothing AndAlso iteration Mod 100 = 0 Then
                        Call println($"logistic[{iteration}] backtrack, rate={rate:E3}, cost={previousCost:F8}")
                    End If

                    Continue For
                End If

                Call costs.Add(cost)

                If AdaptiveRate Then
                    rate = std.Min(rate * Growth, MaxRate)
                End If

                If println IsNot Nothing AndAlso iteration Mod 100 = 0 Then
                    Call println($"logistic[{iteration}] cost={cost:F8}, rate={rate:E3}")
                End If

                ' 早停：成本改善小于容差
                If Tolerance > 0.0R AndAlso iteration > 0 AndAlso previousCost - cost < Tolerance Then
                    Exit For
                End If

                Call Array.Copy(weightBuffer, prevWeights, dims)
                prevBias = bias
                previousCost = cost

                Call LogisticKernels.FillDelta(h, labels, delta)

                Dim applied As Boolean = False
                Dim gradBias As Double = LogisticKernels.AccumulateGradient(
                    rows, delta, gradient,
                    UseParallel AndAlso SimdParallel.Enable,
                    MinParallelWork, partials, applied)

                If applied Then
                    parallelCount += 1
                End If

                Dim scale As Double = rate / size

                For j As Integer = 0 To dims - 1
                    weightBuffer(j) -= scale * gradient(j)
                Next

                If FitIntercept Then
                    bias -= scale * gradBias
                End If
            Next

            Me.finalAlpha = rate
            Me.theta = New Vector(CType(weightBuffer.Clone(), Double()))

            Return LogisticFit.CreateFit(Me, raw)
        End Function

        ''' <summary>
        ''' 返回正类概率 <c>P(y=1|x) = sigmoid(x*w + b)</c>。
        ''' 与 <c>LogisticFit.GetY</c> 共用同一个实现，二者不可能再出现方向不一致。
        ''' </summary>
        Public Function predict(x As Double()) As Double
            If weightBuffer Is Nothing Then
                Throw New InvalidOperationException("the logistic model has not been trained yet; call train() before predict()!")
            End If

            Return LogisticFit.PredictProbability(weightBuffer, If(FitIntercept, bias, 0.0R), Standardize, x)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::9d1f2e40c8b17f5a6d3c4e2b8a0f1c73, Data_science\Mathematica\Math\ANOVA\MultivariateAnalysis\MultivariateMetrics.vb"

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

    ' Module MultivariateMetrics
    ' 
    '     Function: ComponentVariance, Cumulative, IsOpls, IsPca, IsPls
    '               Q2, Q2PerComponent, R2Y, R2YPerComponent
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' 多元统计分析结果（<see cref="MultivariateAnalysisResult"/>）的通用指标提取器。
''' 
''' <para>
''' 将散落在各个分析流程内部的指标公式集中到这里，绘图代码与置换检验都复用同一套实现，
''' 避免出现「同名的 R2Y 在两处算出不同数值」这一类问题。
''' </para>
''' 
''' <para>
''' 所有方法都是「防御式」的：结果对象不完整时返回 <see cref="Double.NaN"/> 或者空数组，
''' 绝不抛出异常 —— 因为 PCA 结果本身就没有 Q2/VIP 这些字段。
''' </para>
''' </summary>
Public Module MultivariateMetrics

    ''' <summary>
    ''' 判断结果是否来自 PCA 分析。
    ''' </summary>
    Public Function IsPca(result As MultivariateAnalysisResult) As Boolean
        Return result IsNot Nothing AndAlso result.analysis Is GetType(PCA)
    End Function

    ''' <summary>
    ''' 判断结果是否来自 PLS(DA) 分析。
    ''' </summary>
    Public Function IsPls(result As MultivariateAnalysisResult) As Boolean
        Return result IsNot Nothing AndAlso result.analysis Is GetType(PLS)
    End Function

    ''' <summary>
    ''' 判断结果是否来自 OPLS(DA) 分析。
    ''' </summary>
    Public Function IsOpls(result As MultivariateAnalysisResult) As Boolean
        Return result IsNot Nothing AndAlso result.analysis Is GetType(OPLS)
    End Function

    ''' <summary>
    ''' 模型的 R2Y（响应的可解释方差比例）。
    ''' 
    ''' <para>
    ''' 这里使用的是**中心化**的定义（SIMCA / 置换检验图的标准做法）：
    ''' <c>R2Y = 1 - SSE/SST</c>，其中 <c>SST = Σ(y - ȳ)²</c>。
    ''' </para>
    ''' 
    ''' <para>
    ''' 注意：不能直接用 <c>1 - SsPreds(末)/SsPreds(0)</c>。因为 <c>SsPreds(0)</c> 是**未中心化**的
    ''' <c>Σy²</c>，它把响应均值也计入了「总平方和」。对于取值 1..k 的分类编码，
    ''' 均值项会占据绝大部分平方和（例如两类编码 1/2 时，理论上限只有 10%），
    ''' 于是模型无论拟合得多好，R2Y 都只能得到 0.1 左右的荒谬数值。
    ''' </para>
    ''' 
    ''' <para>
    ''' PCA 没有响应变量，返回 <see cref="Double.NaN"/>。
    ''' </para>
    ''' </summary>
    ''' <param name="result">多元分析结果</param>
    Public Function R2Y(result As MultivariateAnalysisResult) As Double
        Dim stat = result?.StatisticsObject
        Dim y As Double() = stat?.YVariables
        Dim predicted = result?.PredictedYs

        If y IsNot Nothing AndAlso predicted IsNot Nothing AndAlso predicted.Count = y.Length AndAlso y.Length > 1 Then
            Dim mean As Double = 0

            For i As Integer = 0 To y.Length - 1
                mean += y(i)
            Next

            mean /= y.Length

            Dim sse As Double = 0
            Dim sst As Double = 0

            For i As Integer = 0 To y.Length - 1
                Dim error_ As Double = y(i) - predicted(i)
                Dim deviation As Double = y(i) - mean

                sse += error_ * error_
                sst += deviation * deviation
            Next

            If sst > 0 Then
                Return SafeValue(1 - sse / sst)
            End If
        End If

        ' ---- 退化路径：没有预测值的时候，用 SsPreds 扣除均值项之后估算 ----
        Dim ss = result?.SsPreds

        If ss Is Nothing OrElse ss.Count < 2 OrElse y Is Nothing Then
            Return Double.NaN
        End If

        Dim average As Double = 0

        For i As Integer = 0 To y.Length - 1
            average += y(i)
        Next

        average /= y.Length

        Dim centeredTotal As Double = ss(0) - y.Length * average * average

        Return PercentageOfVariance(centeredTotal, ss(ss.Count - 1))
    End Function

    ''' <summary>
    ''' 每一个建模成分对应的累积响应方差解释率（百分数），长度与 <c>SsPreds</c> 对齐。
    ''' </summary>
    Public Function R2YPerComponent(result As MultivariateAnalysisResult) As Double()
        Dim ss = result?.SsPreds

        If ss Is Nothing OrElse ss.Count < 2 Then
            Return New Double() {}
        End If

        Dim total As Double = ss(0)
        Dim perComponent As Double() = New Double(ss.Count - 2) {}

        For i As Integer = 0 To perComponent.Length - 1
            perComponent(i) = PercentageOfVariance(total, ss(i + 1))
        Next

        Return perComponent
    End Function

    ''' <summary>
    ''' 模型的可预测性指标 Q2（交叉验证结果）。
    ''' 
    ''' <para>
    ''' 取**最后一个成分的单步 Q2**（<c>1 - PRESS/SS</c>），而不是 <c>Q2Cums</c> 的累计值。
    ''' 原因：<c>Q2Cums</c> 是各步 <c>PRESS/SS</c> 比值的连乘，在置换检验里遇到
    ''' 「模型完全不成立」的样本时，连乘会把 Q2 放大到 -70 这种量级，图上的 Q2 序列
    ''' 就会把 R2Y 压成一条贴边的直线。单步 Q2 的取值范围要稳定得多。
    ''' </para>
    ''' 
    ''' <para>
    ''' PCA 没有交叉验证过程，返回 <see cref="Double.NaN"/>。
    ''' </para>
    ''' </summary>
    ''' <param name="result">多元分析结果</param>
    Public Function Q2(result As MultivariateAnalysisResult) As Double
        Dim values = result?.Q2Values

        If values IsNot Nothing AndAlso values.Count > 0 Then
            Return SafeValue(values(values.Count - 1))
        End If

        Dim cum = result?.Q2Cums

        If cum Is Nothing OrElse cum.Count = 0 Then
            Return Double.NaN
        End If

        Return SafeValue(cum(cum.Count - 1))
    End Function

    ''' <summary>
    ''' 每一个成分的累计 Q2 序列（百分数之外的原始比例值）。
    ''' </summary>
    Public Function Q2PerComponent(result As MultivariateAnalysisResult) As Double()
        Dim cum = result?.Q2Cums

        If cum Is Nothing OrElse cum.Count = 0 Then
            Return New Double() {}
        End If

        Return cum.ToArray
    End Function

    ''' <summary>
    ''' 每一个成分对 X 的方差解释率（百分数），用作得分图/碎石图的坐标轴标题。
    ''' 
    ''' <para>
    ''' PCA 直接使用 <see cref="MultivariateAnalysisResult.Contributions"/>（该字段本身就是百分数）；
    ''' PLS/OPLS 则根据 <c>t p'</c> 逐个成分地从 <c>XScaled</c> 中扣除后计算累计 R2X 增量。
    ''' </para>
    ''' </summary>
    Public Function ComponentVariance(result As MultivariateAnalysisResult) As Double()
        If result Is Nothing Then
            Return New Double() {}
        End If

        ' 注意：ObservableCollection 同时实现了 ICollection/IList/IReadOnlyCollection，
        ' 直接调用 IsNullOrEmpty 会导致扩展方法重载歧义（BC30521），因此这里显式判断 Count。
        If result.Contributions IsNot Nothing AndAlso result.Contributions.Count > 0 Then
            Return result.Contributions.Select(Function(v) SafeValue(v)).ToArray
        End If

        ' OPLS 的载荷是正交滤波之后的 p/po，无法用 t·p' 还原 X 的方差分解，
        ' 强行估算只会得到 0.x% 这种误导性的数字，因此不返回方差解释率
        If IsOpls(result) Then
            Return New Double() {}
        End If

        Return EstimateR2X(result)
    End Function

    ''' <summary>
    ''' 计算数组的累计和。
    ''' </summary>
    Public Function Cumulative(values As IEnumerable(Of Double)) As Double()
        If values Is Nothing Then
            Return New Double() {}
        End If

        Dim list As Double() = values.ToArray
        Dim cum As Double() = New Double(list.Length - 1) {}
        Dim sum As Double = 0

        For i As Integer = 0 To list.Length - 1
            sum += list(i)
            cum(i) = sum
        Next

        Return cum
    End Function

    ''' <summary>
    ''' 通过逐个成分扣除 <c>t p'</c> 的方式估算每个成分对 X 的方差解释率（百分数）。
    ''' </summary>
    Private Function EstimateR2X(result As MultivariateAnalysisResult) As Double()
        Dim stat = result.StatisticsObject
        Dim tList = result.TPreds
        Dim pList = result.PPreds

        If stat Is Nothing OrElse stat.XScaled Is Nothing Then
            Return New Double() {}
        End If
        If tList Is Nothing OrElse pList Is Nothing Then
            Return New Double() {}
        End If
        If tList.Count = 0 OrElse tList.Count <> pList.Count Then
            Return New Double() {}
        End If

        Dim x As Double(,) = stat.XScaled
        Dim n As Integer = x.GetLength(0)
        Dim m As Integer = x.GetLength(1)
        Dim total As Double = 0

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To m - 1
                total += x(i, j) * x(i, j)
            Next
        Next

        If total <= 0 Then
            Return New Double() {}
        End If

        ' 残差矩阵：X - sum(t p')
        Dim residual As Double(,) = DirectCast(x.Clone(), Double(,))
        Dim variance As Double() = New Double(tList.Count - 1) {}
        Dim previous As Double = 0

        For k As Integer = 0 To variance.Length - 1
            Dim t As Double() = tList(k)
            Dim p As Double() = pList(k)

            If t Is Nothing OrElse p Is Nothing Then
                Return variance
            End If
            If t.Length <> n OrElse p.Length <> m Then
                Return variance
            End If

            Dim ssResidual As Double = 0

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    residual(i, j) -= t(i) * p(j)
                    ssResidual += residual(i, j) * residual(i, j)
                Next
            Next

            Dim r2x As Double = PercentageOfVariance(total, ssResidual)

            variance(k) = r2x - previous
            previous = r2x
        Next

        Return variance
    End Function

    ''' <summary>
    ''' <c>1 - residual / total</c>，带除零与非法值保护。
    ''' </summary>
    Private Function PercentageOfVariance(total As Double, residual As Double) As Double
        If Double.IsNaN(total) OrElse Double.IsInfinity(total) Then
            Return Double.NaN
        End If
        If total <= 0 Then
            Return Double.NaN
        End If

        Return SafeValue(1 - residual / total)
    End Function

    ''' <summary>
    ''' 把 NaN/Infinity 统一压成 <see cref="Double.NaN"/>，避免污染下游的回归与绘图计算。
    ''' </summary>
    Public Function SafeValue(value As Double) As Double
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return Double.NaN
        Else
            Return value
        End If
    End Function

End Module

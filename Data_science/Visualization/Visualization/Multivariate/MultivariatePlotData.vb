#Region "Microsoft.VisualBasic::c47a18d3f9b54e60a1c2d5e8f36b0a94, Data_science\Visualization\Visualization\Multivariate\MultivariatePlotData.vb"

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

    ' Enum MultivariateEllipseMode
    ' 
    '     Confidence, None, StandardDeviation
    ' 
    ' 
    ' Class MultivariateLoadingSet
    ' 
    '     Properties: Scores, Size, VariableNames, X, XTitle, Y, YTitle
    ' 
    ' Class MultivariateOutlierSeries
    ' 
    '     Properties: DModX, DModXThreshold, Groups, HotellingT2, Outliers
    '                 SampleNames, Spe, SpeThreshold, T2Threshold
    ' 
    ' Class MultivariatePredictionSet
    ' 
    '     Properties: Groups, Intercept, Measured, Predicted, R2, Rmsee
    '                 SampleNames, Slope, Size
    ' 
    ' Class MultivariateScoreSet
    ' 
    '     Properties: Groups, GroupNames, SampleNames, Size, X, XTitle
    '                 Y, YTitle
    ' 
    ' Structure MultivariateSPlotPoint
    ' 
    '     Properties: Correlation, Covariance, Variable
    ' 
    ' Structure MultivariateVarianceComponent
    ' 
    '     Properties: Cumulative, Name, Value
    ' 
    ' Structure MultivariateVipEntry
    ' 
    '     Properties: Variable, Value
    ' 
    ' Module MultivariateAnalysisAdapters
    ' 
    '     Function: ToLoadingSet, ToOutlierSeries, ToPredictionSet, ToScoreSet
    '               ToSPlot, ToVarianceComponents, ToVipEntries
    ' 
    ' Module MultivariatePlotHelper
    ' 
    '     Function: Finite, FormatPercent, GroupColors, GroupNames, SelectGroup
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis.ANOVA
Imports dist = Microsoft.VisualBasic.Math.Statistics.Distributions.Distribution
Imports std = System.Math

''' <summary>
''' 分组椭圆的计算/绘制模式。
''' </summary>
Public Enum MultivariateEllipseMode
    ''' <summary>不绘制椭圆。</summary>
    None
    ''' <summary>
    ''' 统计置信椭圆：基于协方差矩阵特征分解 + 卡方（Normal）或者 F 分布（T）的置信水平，
    ''' 具体实现见 <c>PCAEllipseCalculator</c>。
    ''' </summary>
    Confidence
    ''' <summary>k 倍标准差椭圆：<c>Geometry.EstimateEllipse</c> 的简化版，k = 2 约等于 95%。</summary>
    StandardDeviation
End Enum

''' <summary>
''' 得分图的数据模型：样本名 + 分组 + 两个成分的得分。
''' 
''' <para>
''' 这个类型与绘图引擎无关，既可以由 <see cref="MultivariateAnalysisResult"/> 自动适配得到，
''' 也可以手工组装（例如把 UMAP / SOM 的嵌入坐标当作「得分」来复用同一套绘图代码）。
''' </para>
''' </summary>
Public Class MultivariateScoreSet

    Public Property SampleNames As String() = {}
    Public Property Groups As String() = {}
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    ''' <summary>X 轴标题（已经附带了方差解释率）</summary>
    Public Property XTitle As String = ""
    ''' <summary>Y 轴标题（已经附带了方差解释率）</summary>
    Public Property YTitle As String = ""

    ''' <summary>样本数量（取 X/Y 的较小长度）</summary>
    Public ReadOnly Property Size As Integer
        Get
            Return std.Min(X.Length, Y.Length)
        End Get
    End Property

    ''' <summary>按出现顺序去重的分组名称列表</summary>
    Public ReadOnly Property GroupNames As String()
        Get
            Return MultivariatePlotHelper.GroupNames(Groups)
        End Get
    End Property

    ''' <summary>
    ''' 从多元分析结果构造得分集。
    ''' </summary>
    ''' <param name="result">PCA / PLS / OPLS 的分析结果</param>
    ''' <param name="xComponent">X 轴的成分下标（0 基）</param>
    ''' <param name="yComponent">Y 轴的成分下标（0 基）</param>
    ''' <param name="groups">显式指定的分组标签（长度必须等于样本数）</param>
    ''' <param name="sampleNames">显式指定的样本名称</param>
    ''' <param name="orthogonal">
    ''' 对于 OPLS 结果，True 时把 Y 轴取为正交成分得分（标准的 OPLS-DA 得分图），
    ''' False 时退回普通的成分对。
    ''' </param>
    Public Shared Function FromResult(result As MultivariateAnalysisResult,
                                      Optional xComponent As Integer = 0,
                                      Optional yComponent As Integer = 1,
                                      Optional groups As String() = Nothing,
                                      Optional sampleNames As String() = Nothing,
                                      Optional orthogonal As Boolean = True) As MultivariateScoreSet

        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim scores = result.TPreds

        If scores Is Nothing OrElse scores.Count = 0 Then
            Throw New InvalidOperationException("the analysis result does not contain any score vector!")
        End If

        Dim variance As Double() = MultivariateMetrics.ComponentVariance(result)
        Dim isOpls As Boolean = MultivariateMetrics.IsOpls(result)
        Dim scoreX As Double()
        Dim scoreY As Double()
        Dim titleX As String
        Dim titleY As String

        If isOpls AndAlso orthogonal AndAlso result.ToPreds.Count > 0 Then
            ' 标准 OPLS-DA 得分图：预测成分 t[1] 对第一个正交成分 t_o[1]
            scoreX = scores(0)
            scoreY = result.ToPreds(0)
            titleX = "t[1]" & MultivariatePlotHelper.VarianceText(variance, 0)
            titleY = "t_o[1]"
        ElseIf isOpls AndAlso result.ToPreds.Count > 0 AndAlso scores.Count < 2 Then
            ' OPLS 只有一个预测成分，无法凑出成分对时自动退化为「预测 vs 正交」
            scoreX = scores(0)
            scoreY = result.ToPreds(0)
            titleX = "t[1]" & MultivariatePlotHelper.VarianceText(variance, 0)
            titleY = "t_o[1]"
        Else
            Dim xIndex As Integer = MultivariatePlotHelper.ClampIndex(xComponent, scores.Count)
            Dim yIndex As Integer = If(scores.Count > 1, MultivariatePlotHelper.ClampIndex(yComponent, scores.Count), xIndex)

            scoreX = scores(xIndex)
            scoreY = scores(yIndex)
            titleX = ComponentTitle(result, xIndex) & MultivariatePlotHelper.VarianceText(variance, xIndex)
            titleY = ComponentTitle(result, yIndex) & MultivariatePlotHelper.VarianceText(variance, yIndex)
        End If

        Return New MultivariateScoreSet With {
            .SampleNames = If(sampleNames, SampleNamesOf(result)),
            .Groups = MultivariatePlotHelper.ResolveGroups(result, groups),
            .X = scoreX,
            .Y = scoreY,
            .XTitle = titleX,
            .YTitle = titleY
        }
    End Function

    Private Shared Function ComponentTitle(result As MultivariateAnalysisResult, index As Integer) As String
        Dim prefix As String

        If MultivariateMetrics.IsPca(result) Then
            prefix = "PC"
        ElseIf MultivariateMetrics.IsOpls(result) Then
            prefix = "t"
        Else
            prefix = "T"
        End If

        Return prefix & (index + 1).ToString
    End Function

    Friend Shared Function SampleNamesOf(result As MultivariateAnalysisResult) As String()
        Dim stat = result?.StatisticsObject

        If stat Is Nothing Then
            Return Nothing
        End If

        ' NumericTable 路径下 YLabels2 是样本行名；旧路径下 YLabels2 同样是样本名
        If stat.YLabels2 IsNot Nothing AndAlso stat.YLabels2.Count > 0 Then
            Return stat.YLabels2.ToArray
        End If
        If stat.YLabels IsNot Nothing AndAlso stat.YLabels.Count > 0 Then
            Return stat.YLabels.ToArray
        End If

        Return Nothing
    End Function
End Class

''' <summary>
''' 载荷图 / 双标图的数据模型。
''' </summary>
Public Class MultivariateLoadingSet

    Public Property VariableNames As String() = {}
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property XTitle As String = ""
    Public Property YTitle As String = ""

    ''' <summary>
    ''' 双标图叠加用的得分集；为 Nothing 时只画载荷散点。
    ''' </summary>
    Public Property Scores As MultivariateScoreSet = Nothing

    Public ReadOnly Property Size As Integer
        Get
            Return std.Min(X.Length, Y.Length)
        End Get
    End Property

    ''' <summary>
    ''' 从多元分析结果构造载荷集。
    ''' </summary>
    ''' <param name="result">PCA / PLS / OPLS 的分析结果</param>
    ''' <param name="xComponent">X 轴的成分下标（0 基）</param>
    ''' <param name="yComponent">Y 轴的成分下标（0 基）</param>
    ''' <param name="variableNames">显式指定的变量名列表</param>
    ''' <param name="scores">需要叠加到同一坐标系上的得分集（双标图）</param>
    Public Shared Function FromResult(result As MultivariateAnalysisResult,
                                      Optional xComponent As Integer = 0,
                                      Optional yComponent As Integer = 1,
                                      Optional variableNames As String() = Nothing,
                                      Optional scores As MultivariateScoreSet = Nothing) As MultivariateLoadingSet

        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim loadings = result.PPreds

        If loadings Is Nothing OrElse loadings.Count = 0 Then
            Throw New InvalidOperationException("the analysis result does not contain any loading vector!")
        End If

        Dim variance As Double() = MultivariateMetrics.ComponentVariance(result)
        Dim loadingX As Double()
        Dim loadingY As Double()
        Dim titleX As String
        Dim titleY As String

        If MultivariateMetrics.IsOpls(result) Then
            ' OPLS 的载荷图通常是 p[1] 对 po[1]（预测载荷对正交载荷）
            loadingX = loadings(0)
            loadingY = If(result.PoPreds.Count > 0, result.PoPreds(0), loadings(0))
            titleX = "p[1]" & MultivariatePlotHelper.VarianceText(variance, 0)
            titleY = If(result.PoPreds.Count > 0, "po[1]", "p[1]")
        Else
            Dim xIndex As Integer = MultivariatePlotHelper.ClampIndex(xComponent, loadings.Count)
            Dim yIndex As Integer = If(loadings.Count > 1, MultivariatePlotHelper.ClampIndex(yComponent, loadings.Count), xIndex)

            loadingX = loadings(xIndex)
            loadingY = loadings(yIndex)
            titleX = "Loading " & (xIndex + 1).ToString & MultivariatePlotHelper.VarianceText(variance, xIndex)
            titleY = "Loading " & (yIndex + 1).ToString & MultivariatePlotHelper.VarianceText(variance, yIndex)
        End If

        Dim labels As String() = variableNames

        If labels Is Nothing AndAlso result.StatisticsObject IsNot Nothing Then
            Dim stat = result.StatisticsObject

            If stat.XLabels IsNot Nothing AndAlso stat.XLabels.Count > 0 Then
                labels = stat.XLabels.ToArray
            End If
        End If

        If labels Is Nothing Then
            labels = Enumerable.Range(1, loadingX.Length).Select(Function(i) "X" & i.ToString).ToArray
        End If

        Return New MultivariateLoadingSet With {
            .VariableNames = labels,
            .X = loadingX,
            .Y = loadingY,
            .XTitle = titleX,
            .YTitle = titleY,
            .Scores = scores
        }
    End Function
End Class

''' <summary>
''' OPLS-DA 的 S-plot 数据点：<c>p(1)</c>（协方差）对 <c>p(corr)</c>（相关系数）。
''' </summary>
Public Structure MultivariateSPlotPoint

    Public Property Variable As String
    Public Property Covariance As Double
    Public Property Correlation As Double

    Public Overrides Function ToString() As String
        Return $"{Variable} p1={Covariance.ToString("F4")} p(corr)={Correlation.ToString("F4")}"
    End Function
End Structure

''' <summary>
''' VIP 图 / 系数图的条目。
''' </summary>
Public Structure MultivariateVipEntry

    Public Property Variable As String
    Public Property Value As Double

    Public Overrides Function ToString() As String
        Return $"{Variable} = {Value.ToString("F4")}"
    End Function
End Structure

''' <summary>
''' 碎石图 / 方差解释图的单个成分条目。
''' </summary>
Public Structure MultivariateVarianceComponent

    ''' <summary>成分名称（PC1 / T1 / t[1] ...）</summary>
    Public Property Name As String

    ''' <summary>该成分的方差解释率（百分数）</summary>
    Public Property Value As Double

    ''' <summary>截至该成分的累计方差解释率（百分数）</summary>
    Public Property Cumulative As Double

    Public Overrides Function ToString() As String
        Return $"{Name} {Value.ToString("F2")}% ({Cumulative.ToString("F2")}%)"
    End Function
End Structure

''' <summary>
''' 预测值对实测值散点图的数据模型。
''' </summary>
Public Class MultivariatePredictionSet

    Public Property SampleNames As String() = {}
    Public Property Groups As String() = {}
    Public Property Measured As Double() = {}
    Public Property Predicted As Double() = {}

    ''' <summary>决定系数（实测值与预测值的 Pearson 相关系数的平方）</summary>
    Public Property R2 As Double = Double.NaN
    ''' <summary>均方根误差</summary>
    Public Property Rmsee As Double = Double.NaN
    ''' <summary>实测值对预测值的回归斜率</summary>
    Public Property Slope As Double = Double.NaN
    ''' <summary>实测值对预测值的回归截距</summary>
    Public Property Intercept As Double = Double.NaN

    Public ReadOnly Property Size As Integer
        Get
            Return std.Min(Measured.Length, Predicted.Length)
        End Get
    End Property

    ''' <summary>
    ''' 从多元分析结果构造预测集（响应的实测值来自 <c>StatisticsObject.YVariables</c>）。
    ''' </summary>
    Public Shared Function FromResult(result As MultivariateAnalysisResult,
                                      Optional groups As String() = Nothing,
                                      Optional sampleNames As String() = Nothing) As MultivariatePredictionSet

        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim stat = result.StatisticsObject
        Dim measured As Double() = If(stat?.YVariables, New Double() {})
        Dim predicted As Double() = result.PredictedYs.ToArray
        Dim dataset As New MultivariatePredictionSet With {
            .SampleNames = If(sampleNames, MultivariateScoreSet.SampleNamesOf(result)),
            .Groups = MultivariatePlotHelper.ResolveGroups(result, groups),
            .Measured = measured,
            .Predicted = predicted,
            .Rmsee = result.Rmsee
        }

        Call dataset.UpdateFit()

        Return dataset
    End Function

    ''' <summary>
    ''' 重新计算 R2 / RMSEE / 回归系数（手工修改数据之后需要调用）。
    ''' </summary>
    Public Sub UpdateFit()
        Dim n As Integer = Size

        If n < 2 Then
            Return
        End If

        Dim meanM As Double = 0
        Dim meanP As Double = 0

        For i As Integer = 0 To n - 1
            meanM += Measured(i)
            meanP += Predicted(i)
        Next

        meanM /= n
        meanP /= n

        Dim sxy#, sxx#, syy#, sse#

        For i As Integer = 0 To n - 1
            Dim dm As Double = Measured(i) - meanM
            Dim dp As Double = Predicted(i) - meanP
            Dim err As Double = Measured(i) - Predicted(i)

            sxy += dm * dp
            sxx += dm * dm
            syy += dp * dp
            sse += err * err
        Next

        If sxx > 0 AndAlso syy > 0 Then
            R2 = (sxy / std.Sqrt(sxx * syy)) ^ 2
        End If

        Slope = If(sxx > 0, sxy / sxx, Double.NaN)
        Intercept = If(sxx > 0, meanP - Slope * meanM, Double.NaN)
        Rmsee = std.Sqrt(sse / std.Max(1, n - 1))
    End Sub
End Class

''' <summary>
''' Hotelling T2 / SPE(DModX) 离群诊断图的数据模型。
''' </summary>
Public Class MultivariateOutlierSeries

    Public Property SampleNames As String() = {}
    Public Property Groups As String() = {}

    ''' <summary>每个样本的 Hotelling T2 统计量</summary>
    Public Property HotellingT2 As Double() = {}
    ''' <summary>每个样本的 SPE（残差平方和）</summary>
    Public Property Spe As Double() = {}
    ''' <summary>每个样本的 DModX（标准化的 SPE）</summary>
    Public Property DModX As Double() = {}

    ''' <summary>T2 的 95% 控制限</summary>
    Public Property T2Threshold As Double = Double.NaN
    ''' <summary>DModX 的 95% 控制限</summary>
    Public Property DModXThreshold As Double = Double.NaN
    ''' <summary>SPE 的 95% 控制限</summary>
    Public Property SpeThreshold As Double = Double.NaN

    ''' <summary>样本是否超出任意一个控制限</summary>
    Public Property Outliers As Boolean() = {}

    Public ReadOnly Property Size As Integer
        Get
            Return HotellingT2.Length
        End Get
    End Property

    ''' <summary>
    ''' 从多元分析结果计算 T2 / SPE / DModX。
    ''' </summary>
    ''' <remarks>
    ''' T2 控制限使用精确的 F 分布：<c>k(n-1)/(n-k) * F(0.95, k, n-k)</c>；
    ''' SPE 控制限使用 Nomikos &amp; MacGregor 的卡方近似（自由度四舍五入到整数）。
    ''' </remarks>
    Public Shared Function FromResult(result As MultivariateAnalysisResult,
                                      Optional groups As String() = Nothing,
                                      Optional sampleNames As String() = Nothing) As MultivariateOutlierSeries

        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim stat = result.StatisticsObject
        Dim scores = result.TPreds

        If stat Is Nothing OrElse stat.XScaled Is Nothing Then
            Throw New InvalidOperationException("the analysis result does not contain the scaled X matrix!")
        End If
        If scores Is Nothing OrElse scores.Count = 0 Then
            Throw New InvalidOperationException("the analysis result does not contain any score vector!")
        End If

        Dim x As Double(,) = stat.XScaled
        Dim n As Integer = x.GetLength(0)
        Dim m As Integer = x.GetLength(1)
        Dim k As Integer = std.Max(1, scores.Count)

        ' ---- Hotelling T2 ----
        Dim t2 As Double() = New Double(n - 1) {}

        For c As Integer = 0 To scores.Count - 1
            Dim t As Double() = scores(c)

            If t Is Nothing OrElse t.Length <> n Then
                Continue For
            End If

            Dim mean As Double = t.Average
            Dim variance As Double = 0

            For i As Integer = 0 To n - 1
                variance += (t(i) - mean) ^ 2
            Next

            variance /= std.Max(1, n - 1)

            If variance <= 0 Then
                Continue For
            End If

            For i As Integer = 0 To n - 1
                t2(i) += t(i) * t(i) / variance
            Next
        Next

        ' ---- 残差 -> SPE / DModX ----
        Dim residual As Double(,)= DirectCast(x.Clone(), Double(,))

        For c As Integer = 0 To scores.Count - 1
            Dim t As Double() = scores(c)
            Dim p As Double() = If(result.PPreds.Count > c, result.PPreds(c), Nothing)

            If t Is Nothing OrElse p Is Nothing OrElse t.Length <> n OrElse p.Length <> m Then
                Continue For
            End If

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    residual(i, j) -= t(i) * p(j)
                Next
            Next
        Next

        Dim spe As Double() = New Double(n - 1) {}
        Dim dmodx As Double() = New Double(n - 1) {}
        Dim dof As Integer = std.Max(1, m - k)

        For i As Integer = 0 To n - 1
            Dim s As Double = 0

            For j As Integer = 0 To m - 1
                s += residual(i, j) * residual(i, j)
            Next

            spe(i) = s
            dmodx(i) = std.Sqrt(s / dof)
        Next

        ' ---- 控制限 ----
        Dim series As New MultivariateOutlierSeries With {
            .SampleNames = If(sampleNames, MultivariateScoreSet.SampleNamesOf(result)),
            .Groups = MultivariatePlotHelper.ResolveGroups(result, groups),
            .HotellingT2 = t2,
            .Spe = spe,
            .DModX = dmodx
        }

        If n > k + 1 Then
            ' 标准公式：k(n²-1)/(n(n-k)) · F(0.05, k, n-k)
            ' 注意 Distribution.FDistributionInverse 的输入是**上尾概率**，
            ' 因此 95% 控制限要传 0.05 而不是 0.95
            series.T2Threshold = k * (n * n - 1) / (n * (n - k)) * dist.FDistributionInverse(0.05, k, n - k)
        End If

        Call series.ComputeSpeThreshold()
        Call series.UpdateOutliers()

        Return series
    End Function

    ''' <summary>
    ''' 用 Nomikos &amp; MacGregor 的卡方近似重新计算 SPE / DModX 控制限。
    ''' 
    ''' <para>
    ''' <c>SPE ~ g·χ²(h)</c>，其中 <c>g = σ²/(2μ)</c>、<c>h = 2μ²/σ²</c>；
    ''' 同样地，<see cref="dist.ChiSquareInverse"/> 使用上尾概率，95% 控制限传 0.05。
    ''' </para>
    ''' </summary>
    Public Sub ComputeSpeThreshold()
        Dim n As Integer = Spe.Length

        If n < 3 Then
            Return
        End If

        Dim mean As Double = 0
        Dim variance As Double = 0

        For i As Integer = 0 To n - 1
            mean += Spe(i)
        Next

        mean /= n

        For i As Integer = 0 To n - 1
            variance += (Spe(i) - mean) ^ 2
        Next

        variance /= (n - 1)

        If mean <= 0 OrElse variance <= 0 Then
            Return
        End If

        Dim g As Double = variance / (2 * mean)
        Dim h As Integer = std.Max(1, CInt(std.Round(2 * mean * mean / variance)))

        SpeThreshold = g * dist.ChiSquareInverse(0.05, h)

        ' DModX = sqrt(SPE / dof)，是 SPE 的固定比例缩放，控制限按同样比例换算
        If SpeThreshold > 0 AndAlso Spe(0) > 0 AndAlso DModX(0) > 0 Then
            DModXThreshold = std.Sqrt(SpeThreshold) * (DModX(0) / std.Sqrt(Spe(0)))
        End If
    End Sub

    ''' <summary>
    ''' 根据当前的 T2 / DModX 控制限重新标记离群样本。
    ''' </summary>
    Public Sub UpdateOutliers()
        Dim n As Integer = HotellingT2.Length

        Outliers = New Boolean(n - 1) {}

        For i As Integer = 0 To n - 1
            Outliers(i) = (Not Double.IsNaN(T2Threshold) AndAlso HotellingT2(i) > T2Threshold) OrElse
                (Not Double.IsNaN(DModXThreshold) AndAlso DModX(i) > DModXThreshold)
        Next
    End Sub
End Class

''' <summary>
''' 与绘图无关的通用辅助函数（分组解析、颜色分配、数值格式化）。
''' </summary>
Public Module MultivariatePlotHelper

    ''' <summary>
    ''' 按出现顺序去重的分组名称列表。
    ''' </summary>
    Public Function GroupNames(groups As String()) As String()
        If groups Is Nothing OrElse groups.Length = 0 Then
            Return New String() {}
        End If

        Dim names As New List(Of String)
        Dim seen As New HashSet(Of String)

        For Each name As String In groups
            Dim key As String = If(name, "")

            If seen.Add(key) Then
                Call names.Add(key)
            End If
        Next

        Return names.ToArray
    End Function

    ''' <summary>
    ''' 为每个分组分配一个稳定的颜色（按分组名称首次出现的顺序取主题调色板）。
    ''' </summary>
    Public Function GroupColors(groups As String(), palette As Color()) As Dictionary(Of String, Color)
        Dim colors As New Dictionary(Of String, Color)
        Dim names As String() = GroupNames(groups)
        Dim paletteFallback As Color() = If(palette Is Nothing OrElse palette.Length = 0,
            New Color() {Color.FromArgb(31, 119, 180)}, palette)

        For i As Integer = 0 To names.Length - 1
            colors(names(i)) = paletteFallback(i Mod paletteFallback.Length)
        Next

        Return colors
    End Function

    ''' <summary>
    ''' 解析样本的分组标签：显式传入 &gt; decoder 反查 &gt; 整数编码映射为 <c>Class 1..k</c> &gt; 单一分组 <c>All</c>。
    ''' </summary>
    Public Function ResolveGroups(result As MultivariateAnalysisResult, Optional groups As String() = Nothing) As String()
        Dim stat = result?.StatisticsObject
        Dim n As Integer

        If stat IsNot Nothing AndAlso stat.XDataMatrix IsNot Nothing Then
            n = stat.XDataMatrix.GetLength(0)
        Else
            n = If(result?.PredictedYs?.Count, 0)
        End If

        If groups IsNot Nothing AndAlso groups.Length = n AndAlso n > 0 Then
            Return groups
        End If

        Dim y As Double() = stat?.YVariables

        If y Is Nothing OrElse y.Length <> n OrElse n = 0 Then
            Return Enumerable.Repeat("All", std.Max(1, n)).ToArray
        End If

        ' 连续型响应（例如回归型的 PLS）取值过于分散，强行离散化只会得到一堆单样本分组
        Dim distinct As Integer = y.Distinct.Count

        If distinct > std.Max(10, n \ 2) Then
            Return Enumerable.Repeat("All", n).ToArray
        End If

        Dim decoder = stat.decoder
        Dim byCode As New Dictionary(Of Integer, String)

        If decoder IsNot Nothing Then
            For Each pair As KeyValuePair(Of String, Integer) In decoder
                byCode(pair.Value) = pair.Key
            Next
        End If

        Dim labels As String() = New String(n - 1) {}

        For i As Integer = 0 To n - 1
            Dim code As Integer = CInt(std.Round(y(i)))

            If byCode.Count > 0 AndAlso byCode.ContainsKey(code) Then
                labels(i) = byCode(code)
            Else
                labels(i) = "Class " & code.ToString
            End If
        Next

        Return labels
    End Function

    ''' <summary>
    ''' 从候选值中挑出第一个需要绘制的分组（用于「只画某组」的场景）。
    ''' </summary>
    Public Function SelectGroup(groups As String(), name As String) As Boolean()
        If groups Is Nothing Then
            Return New Boolean() {}
        End If

        Dim flags As Boolean() = New Boolean(groups.Length - 1) {}

        For i As Integer = 0 To groups.Length - 1
            flags(i) = String.Equals(groups(i), name, StringComparison.OrdinalIgnoreCase)
        Next

        Return flags
    End Function

    ''' <summary>
    ''' 把方差解释率格式化成坐标轴标题的附加文本，例如 <c> (68.3%)</c>。
    ''' </summary>
    Public Function VarianceText(variance As Double(), index As Integer) As String
        If variance Is Nothing Then
            Return ""
        End If
        If index < 0 OrElse index >= variance.Length Then
            Return ""
        End If

        Dim value As Double = variance(index)

        If Double.IsNaN(value) Then
            Return ""
        End If

        Return $" ({value.ToString("F1")}%)"
    End Function

    ''' <summary>
    ''' 成分下标越界时钳制到合法范围。
    ''' </summary>
    Public Function ClampIndex(index As Integer, count As Integer) As Integer
        If count <= 0 Then
            Return 0
        End If

        Return std.Max(0, std.Min(index, count - 1))
    End Function

    ''' <summary>
    ''' 把非法数值（NaN / Infinity）压成 <paramref name="fallback"/>，避免污染像素坐标计算。
    ''' </summary>
    Public Function Finite(value As Double, Optional fallback As Double = 0) As Double
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return fallback
        Else
            Return value
        End If
    End Function

    ''' <summary>
    ''' 计算一个文本标签的放置位置：默认放在 (x, y)，与已有标签重叠时上下交替避让，
    ''' 并且始终把它夹紧在 <paramref name="bounds"/> 之内（避免标签被挤到绘图区外面）。
    ''' </summary>
    ''' <param name="occupied">已经占用的文本矩形（会被原地更新）</param>
    Public Function PlaceLabel(x As Single, y As Single, size As SizeF, bounds As RectangleF,
                               occupied As List(Of RectangleF)) As PointF

        Dim offsets As Single() = {0, 1, -1, 2, -2, 3, -3, 4, -4}

        For Each offset As Single In offsets
            Dim cy As Single = y + offset * (size.Height + 1)
            Dim point As PointF = ClampLabel(x, cy, size, bounds)
            Dim rect As New RectangleF(point.X, point.Y, size.Width, size.Height)
            Dim collision As Boolean = False

            For Each used As RectangleF In occupied
                If rect.IntersectsWith(used) Then
                    collision = True

                    Exit For
                End If
            Next

            If Not collision Then
                Call occupied.Add(rect)

                Return point
            End If
        Next

        ' 所有候选位置都被占用：仍然把标签画在夹紧之后的位置上
        Return ClampLabel(x, y, size, bounds)
    End Function

    Private Function ClampLabel(x As Single, y As Single, size As SizeF, bounds As RectangleF) As PointF
        Dim cx As Single = std.Max(bounds.Left, std.Min(x, bounds.Right - size.Width))
        Dim cy As Single = std.Max(bounds.Top, std.Min(y, bounds.Bottom - size.Height))

        Return New PointF(cx, cy)
    End Function

    ''' <summary>
    ''' 数据数组的最小/最大值（自动跳过非法值）。
    ''' </summary>
    Public Sub RangeOf(values As Double(), ByRef min As Double, ByRef max As Double)
        min = Double.MaxValue
        max = Double.MinValue

        For Each v As Double In values
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If

            If v < min Then min = v
            If v > max Then max = v
        Next

        If min > max Then
            min = 0
            max = 1
        End If
    End Sub

End Module

''' <summary>
''' 把 <see cref="MultivariateAnalysisResult"/> 适配成各个图表使用的数据模型的扩展方法集合。
''' </summary>
Public Module MultivariateAnalysisAdapters

    ''' <summary>构造得分集（等价于 <see cref="MultivariateScoreSet.FromResult"/>）。</summary>
    <Extension>
    Public Function ToScoreSet(result As MultivariateAnalysisResult,
                               Optional xComponent As Integer = 0,
                               Optional yComponent As Integer = 1,
                               Optional groups As String() = Nothing,
                               Optional sampleNames As String() = Nothing,
                               Optional orthogonal As Boolean = True) As MultivariateScoreSet

        Return MultivariateScoreSet.FromResult(result, xComponent, yComponent, groups, sampleNames, orthogonal)
    End Function

    ''' <summary>构造载荷集（等价于 <see cref="MultivariateLoadingSet.FromResult"/>）。</summary>
    <Extension>
    Public Function ToLoadingSet(result As MultivariateAnalysisResult,
                                 Optional xComponent As Integer = 0,
                                 Optional yComponent As Integer = 1,
                                 Optional variableNames As String() = Nothing,
                                 Optional scores As MultivariateScoreSet = Nothing) As MultivariateLoadingSet

        Return MultivariateLoadingSet.FromResult(result, xComponent, yComponent, variableNames, scores)
    End Function

    ''' <summary>
    ''' 构造 S-plot 数据（只对 OPLS 结果有意义：第一预测成分的 p1 与 p(corr)）。
    ''' </summary>
    <Extension>
    Public Function ToSPlot(result As MultivariateAnalysisResult, Optional variableNames As String() = Nothing) As MultivariateSPlotPoint()
        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim covariances = result.PPredCovs
        Dim coefficients = result.PPredCoeffs

        If covariances Is Nothing OrElse covariances.Count = 0 Then
            Return New MultivariateSPlotPoint() {}
        End If

        Dim cov As Double() = covariances(0)
        Dim cor As Double() = If(coefficients.Count > 0, coefficients(0), New Double() {})

        If cov Is Nothing Then
            Return New MultivariateSPlotPoint() {}
        End If

        Dim labels As String() = variableNames

        If labels Is Nothing AndAlso result.StatisticsObject IsNot Nothing Then
            Dim stat = result.StatisticsObject

            If stat.XLabels IsNot Nothing AndAlso stat.XLabels.Count > 0 Then
                labels = stat.XLabels.ToArray
            End If
        End If

        If labels Is Nothing Then
            labels = Enumerable.Range(1, cov.Length).Select(Function(i) "X" & i.ToString).ToArray
        End If

        Dim points As MultivariateSPlotPoint() = New MultivariateSPlotPoint(cov.Length - 1) {}

        For i As Integer = 0 To cov.Length - 1
            points(i) = New MultivariateSPlotPoint With {
                .Variable = If(i < labels.Length, labels(i), "X" & (i + 1).ToString),
                .Covariance = cov(i),
                .Correlation = If(i < cor.Length, cor(i), 0)
            }
        Next

        Return points
    End Function

    ''' <summary>
    ''' 构造 VIP 或者回归系数的条目列表（按数值降序排列）。
    ''' </summary>
    <Extension>
    Public Function ToVipEntries(result As MultivariateAnalysisResult,
                                 Optional useCoefficients As Boolean = False,
                                 Optional variableNames As String() = Nothing) As MultivariateVipEntry()

        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim values As Double() = If(useCoefficients, result.Coefficients.ToArray, result.Vips.ToArray)

        If values Is Nothing OrElse values.Length = 0 Then
            Return New MultivariateVipEntry() {}
        End If

        Dim labels As String() = variableNames

        If labels Is Nothing AndAlso result.StatisticsObject IsNot Nothing Then
            Dim stat = result.StatisticsObject

            If stat.XLabels IsNot Nothing AndAlso stat.XLabels.Count > 0 Then
                labels = stat.XLabels.ToArray
            End If
        End If

        If labels Is Nothing Then
            labels = Enumerable.Range(1, values.Length).Select(Function(i) "X" & i.ToString).ToArray
        End If

        Dim entries As MultivariateVipEntry() = New MultivariateVipEntry(values.Length - 1) {}

        For i As Integer = 0 To values.Length - 1
            entries(i) = New MultivariateVipEntry With {
                .Variable = If(i < labels.Length, labels(i), "X" & (i + 1).ToString),
                .Value = MultivariatePlotHelper.Finite(values(i))
            }
        Next

        ' VIP 只关心大小，直接降序；回归系数有正负之分，按绝对值降序才有可读性
        If useCoefficients Then
            Return entries.OrderByDescending(Function(e) std.Abs(e.Value)).ToArray
        Else
            Return entries.OrderByDescending(Function(e) e.Value).ToArray
        End If
    End Function

    ''' <summary>
    ''' 构造碎石图使用的成分方差解释率列表。
    ''' </summary>
    <Extension>
    Public Function ToVarianceComponents(result As MultivariateAnalysisResult) As MultivariateVarianceComponent()
        If result Is Nothing Then
            Throw New ArgumentNullException(NameOf(result))
        End If

        Dim variance As Double() = MultivariateMetrics.ComponentVariance(result)

        If variance Is Nothing OrElse variance.Length = 0 Then
            Return New MultivariateVarianceComponent() {}
        End If

        Dim prefix As String = If(MultivariateMetrics.IsPca(result), "PC", If(MultivariateMetrics.IsOpls(result), "t", "T"))
        Dim components As MultivariateVarianceComponent() = New MultivariateVarianceComponent(variance.Length - 1) {}
        Dim cumulative As Double = 0

        For i As Integer = 0 To variance.Length - 1
            Dim value As Double = MultivariatePlotHelper.Finite(variance(i))

            cumulative += value

            components(i) = New MultivariateVarianceComponent With {
                .Name = prefix & (i + 1).ToString,
                .Value = value,
                .Cumulative = cumulative
            }
        Next

        Return components
    End Function

    ''' <summary>构造预测值-实测值散点图的数据。</summary>
    <Extension>
    Public Function ToPredictionSet(result As MultivariateAnalysisResult,
                                    Optional groups As String() = Nothing,
                                    Optional sampleNames As String() = Nothing) As MultivariatePredictionSet

        Return MultivariatePredictionSet.FromResult(result, groups, sampleNames)
    End Function

    ''' <summary>构造 T2 / SPE 离群诊断图的数据。</summary>
    <Extension>
    Public Function ToOutlierSeries(result As MultivariateAnalysisResult,
                                    Optional groups As String() = Nothing,
                                    Optional sampleNames As String() = Nothing) As MultivariateOutlierSeries

        Return MultivariateOutlierSeries.FromResult(result, groups, sampleNames)
    End Function

End Module

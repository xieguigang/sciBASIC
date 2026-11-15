Imports System.Data
Imports System.Drawing
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports std = System.Math

' ---------------------------------------------------------------------------
'  ShapPlot / ShapPlotUtils.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  SHAP 结果图共用的取数与配色工具：维度校验、按重要性排序、特征列 / 贡献列提取、
'  蓝红色阶、对称值域、top-N 贡献聚合。8 张图全部依赖这些工具，避免各写一遍。
' ---------------------------------------------------------------------------

Namespace ShapPlot

    ''' <summary>
    ''' 瀑布图 / 力导向图所使用的「top-N 贡献 + 其余特征合并」聚合结果。
    ''' </summary>
    Public Class ShapContributionGroup

        ''' <summary>
        ''' 按 |贡献| 降序排列的特征贡献，名字为特征名。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Items As (name As String, value As Double)()

        ''' <summary>
        ''' 未被展示的特征的贡献总和。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Others As Double

        ''' <summary>
        ''' 被合并到 <see cref="Others"/> 之中的特征数量。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property OthersCount As Integer

        ''' <summary>
        ''' 基线值 / 期望值。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Baseline As Double

        ''' <summary>
        ''' 样本的模型原始输出。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Output As Double

        ''' <summary>
        ''' 样本 ID。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property SampleId As String

        Sub New(items As (name As String, value As Double)(), others As Double, othersCount As Integer,
                baseline As Double, output As Double, sampleId As String)

            Me.Items = items
            Me.Others = others
            Me.OthersCount = othersCount
            Me.Baseline = baseline
            Me.Output = output
            Me.SampleId = sampleId
        End Sub

        ''' <summary>
        ''' 全部贡献之和（等于 <c>Output</c> 与 <c>Baseline</c> 之差在加性空间内的形式）。
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Total As Double
            Get
                Return Items.Select(Function(i) i.value).Sum() + Others
            End Get
        End Property
    End Class

    ''' <summary>
    ''' SHAP 图表共用的取数与配色工具。
    ''' 
    ''' 所有图表都以「<see cref="ShapAnalysisResult"/> + <see cref="NumericTable"/>」作为输入：
    ''' 前者提供逐样本的 SHAP 贡献，后者提供样本的原始特征取值（用于着色与坐标轴）。
    ''' </summary>
    Public Module ShapPlotUtils

        ''' <summary>SHAP 正值（把预测推高）的经典红色。</summary>
        Public ReadOnly PositiveColor As Color = Color.FromArgb(255, 0, 81)
        ''' <summary>SHAP 负值（把预测拉低）的经典蓝色。</summary>
        Public ReadOnly NegativeColor As Color = Color.FromArgb(0, 139, 251)
        ''' <summary>零贡献处的过渡色（近白色）。</summary>
        Public ReadOnly NeutralColor As Color = Color.FromArgb(242, 242, 242)

        ''' <summary>缺省显示的特征数量。</summary>
        Public Const DefaultMaxDisplay As Integer = 15

        ' ========================================================
        '  数据校验
        ' ========================================================
        ''' <summary>
        ''' 校验分析结果与特征表是否可以直接用来绘图。
        ''' </summary>
        ''' <param name="shap">SHAP 分析结果</param>
        ''' <param name="features">样本特征表</param>
        ''' <exception cref="ArgumentNullException">输入为空</exception>
        ''' <exception cref="InvalidConstraintException">样本数量或者特征数量对不上</exception>
        Public Sub Validate(shap As ShapAnalysisResult, features As NumericTable)
            If shap Is Nothing Then
                Throw New ArgumentNullException(NameOf(shap))
            End If
            If features Is Nothing Then
                Throw New ArgumentNullException(NameOf(features))
            End If

            Dim n As Integer = SampleCount(shap)
            Dim p As Integer = FeatureCount(shap)

            If n <= 0 Then
                Throw New InvalidConstraintException("the given SHAP analysis result contains no explanation record!")
            End If
            If features.features Is Nothing OrElse features.nsamples = 0 Then
                Throw New InvalidConstraintException("the given feature table contains no sample!")
            End If
            If features.nsamples <> n Then
                Throw New InvalidConstraintException(
                    $"the feature table has {features.nsamples} samples but the SHAP result holds {n} explanations!")
            End If
            If p <= 0 Then
                Throw New InvalidConstraintException("the given SHAP analysis result contains no feature importance vector!")
            End If
            If features.nfeatures < p Then
                Throw New InvalidConstraintException(
                    $"the feature table has only {features.nfeatures} features but the SHAP result requires {p}!")
            End If
        End Sub

        ''' <summary>参与解释的样本数量。</summary>
        Public Function SampleCount(shap As ShapAnalysisResult) As Integer
            If shap Is Nothing OrElse shap.Explanations Is Nothing Then
                Return 0
            Else
                Return shap.Explanations.Count
            End If
        End Function

        ''' <summary>参与解释的特征数量。</summary>
        Public Function FeatureCount(shap As ShapAnalysisResult) As Integer
            If shap Is Nothing OrElse shap.MeanAbsShap Is Nothing Then
                Return 0
            Else
                Return shap.MeanAbsShap.Length
            End If
        End Function

        ' ========================================================
        '  特征与样本的索引视图
        ' ========================================================
        ''' <summary>
        ''' 按全局重要性（平均绝对 SHAP 值）降序返回特征下标，最多取 <paramref name="topN"/> 个。
        ''' </summary>
        ''' <param name="shap">SHAP 分析结果</param>
        ''' <param name="topN">最多返回的特征数量，小于等于 0 表示全部</param>
        Public Function TopFeatures(shap As ShapAnalysisResult, Optional topN As Integer = DefaultMaxDisplay) As Integer()
            Dim width As Integer = FeatureCount(shap)

            If width = 0 Then
                Return New Integer() {}
            End If

            Dim idx As Integer() = New Integer(width - 1) {}

            For i As Integer = 0 To width - 1
                idx(i) = i
            Next

            Array.Sort(idx, Function(a, b)
                               Dim cmp As Double = shap.MeanAbsShap(b).CompareTo(shap.MeanAbsShap(a))
                               If cmp <> 0 Then Return cmp
                               Return a.CompareTo(b)
                           End Function)

            If topN > 0 AndAlso idx.Length > topN Then
                Return idx.Take(topN).ToArray
            Else
                Return idx
            End If
        End Function

        ''' <summary>
        ''' 取特征名字；名字缺失的时候自动生成 <c>F1..Fn</c>。
        ''' </summary>
        Public Function FeatureName(shap As ShapAnalysisResult, idx As Integer) As String
            If shap Is Nothing OrElse shap.FeatureNames Is Nothing OrElse idx >= shap.FeatureNames.Length Then
                Return $"F{idx + 1}"
            Else
                Return shap.FeatureNames(idx)
            End If
        End Function

        ''' <summary>
        ''' 取样本 ID；行名缺失的时候自动生成 <c>S0001</c> 形式的序号。
        ''' </summary>
        Public Function SampleId(features As NumericTable, i As Integer) As String
            Dim names As String() = If(features Is Nothing, Nothing, features.rowNames)

            If names IsNot Nothing AndAlso i >= 0 AndAlso i < names.Length Then
                Return names(i)
            Else
                Return $"S{(i + 1).ToString("D4")}"
            End If
        End Function

        ''' <summary>
        ''' 在分析结果之中定位指定样本的下标；找不到的时候返回 0（首个样本）。
        ''' </summary>
        Public Function FindSample(shap As ShapAnalysisResult, sampleId As String) As Integer
            If String.IsNullOrEmpty(sampleId) OrElse shap Is Nothing OrElse shap.Explanations Is Nothing Then
                Return 0
            End If

            For i As Integer = 0 To shap.Explanations.Count - 1
                If String.Equals(shap.Explanations(i).ID, sampleId, StringComparison.OrdinalIgnoreCase) Then
                    Return i
                End If
            Next

            Return 0
        End Function

        ' ========================================================
        '  列向量提取（一次性取出，避免在循环里重复查找）
        ' ========================================================
        ''' <summary>
        ''' 一次性取出特征表之中的第 <paramref name="idx"/> 列。
        ''' </summary>
        Public Function FeatureColumn(features As NumericTable, idx As Integer) As Double()
            Dim n As Integer = If(features Is Nothing, 0, features.nsamples)
            Dim v As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim row As Double() = features.features(i)
                v(i) = If(row Is Nothing OrElse idx >= row.Length, 0, row(idx))
            Next

            Return v
        End Function

        ''' <summary>
        ''' 一次性取出所有样本在第 <paramref name="idx"/> 个特征上的 SHAP 贡献值。
        ''' </summary>
        Public Function ContributionColumn(shap As ShapAnalysisResult, idx As Integer) As Double()
            Dim n As Integer = SampleCount(shap)
            Dim v As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim contributions As Double() = shap.Explanations(i).Contributions
                v(i) = If(contributions Is Nothing OrElse idx >= contributions.Length, 0, contributions(idx))
            Next

            Return v
        End Function

        ' ========================================================
        '  数值归一化与配色
        ' ========================================================
        ''' <summary>
        ''' 线性归一化到 <c>0..1</c>；值域为零的时候全部返回 0.5。
        ''' </summary>
        Public Function Normalize(values As Double()) As Double()
            If values Is Nothing OrElse values.Length = 0 Then
                Return New Double() {}
            End If

            Dim lo As Double = values.Min()
            Dim hi As Double = values.Max()
            Dim span As Double = hi - lo
            Dim t As Double() = New Double(values.Length - 1) {}

            For i As Integer = 0 To values.Length - 1
                Dim ratio As Double = If(span <= 0 OrElse Double.IsNaN(span), 0.5, (values(i) - lo) / span)

                If Double.IsNaN(ratio) Then
                    ratio = 0.5
                End If

                t(i) = std.Max(0, std.Min(1, ratio))
            Next

            Return t
        End Function

        ''' <summary>
        ''' 在两个颜色之间做线性插值（插值系数会被钳制到 <c>0..1</c>）。
        ''' </summary>
        ''' <remarks>
        ''' 注意：<see cref="Color"/> 的分量是 <c>Byte</c>，而 VB 的 <c>Byte - Byte</c> 仍然是 Byte 运算，
        ''' 在本工程的 <c>RemoveIntegerChecks</c> 配置下会发生回绕（例如 0 - 139 变成 117），
        ''' 因此这里必须先把各个分量显式提升为 <c>Double</c> 再参与插值。
        ''' </remarks>
        Public Function LerpColor(a As Color, b As Color, t As Double) As Color
            If Double.IsNaN(t) Then
                t = 0
            ElseIf t < 0 Then
                t = 0
            ElseIf t > 1 Then
                t = 1
            End If

            Dim aA As Double = a.A, bA As Double = b.A
            Dim aR As Double = a.R, bR As Double = b.R
            Dim aG As Double = a.G, bG As Double = b.G
            Dim aB As Double = a.B, bB As Double = b.B

            Return Color.FromArgb(
                Channel(aA + (bA - aA) * t),
                Channel(aR + (bR - aR) * t),
                Channel(aG + (bG - aG) * t),
                Channel(aB + (bB - aB) * t))
        End Function

        ''' <summary>
        ''' 把插值结果钳制到合法的颜色分量区间 <c>0..255</c>。
        ''' </summary>
        Private Function Channel(value As Double) As Integer
            If Double.IsNaN(value) OrElse value <= 0 Then
                Return 0
            ElseIf value >= 255 Then
                Return 255
            Else
                Return CInt(std.Round(value))
            End If
        End Function

        ''' <summary>
        ''' 特征值高低的经典蓝红色阶：低值蓝、高值红（<paramref name="t"/> 为 <c>0..1</c>）。
        ''' </summary>
        Public Function ValueColor(t As Double) As Color
            Return LerpColor(NegativeColor, PositiveColor, t)
        End Function

        ''' <summary>
        ''' SHAP 值符号色阶：负值蓝、零附近为中性色、正值红。
        ''' </summary>
        ''' <param name="v">SHAP 值</param>
        ''' <param name="vmin">负向极值（应当小于等于 0）</param>
        ''' <param name="vmax">正向极值（应当大于等于 0）</param>
        Public Function ShapColor(v As Double, vmin As Double, vmax As Double) As Color
            If v >= 0 Then
                Dim t As Double = If(vmax <= 0, 0, std.Min(1, v / vmax))
                Return LerpColor(NeutralColor, PositiveColor, t)
            Else
                Dim t As Double = If(vmin >= 0, 0, std.Min(1, v / vmin))
                Return LerpColor(NeutralColor, NegativeColor, t)
            End If
        End Function

        ''' <summary>
        ''' 以 0 为中心的对称值域（两侧留 5% 余量）；数据全为零的时候回退为 <c>-1..1</c>。
        ''' </summary>
        Public Function SymmetricRange(values As IEnumerable(Of Double)) As (min As Double, max As Double)
            Dim vmin As Double = 0
            Dim vmax As Double = 0
            Dim any As Boolean = False

            If values IsNot Nothing Then
                For Each v As Double In values
                    If Double.IsNaN(v) Then
                        Continue For
                    End If

                    If Not any Then
                        vmin = v
                        vmax = v
                        any = True
                    Else
                        vmin = std.Min(vmin, v)
                        vmax = std.Max(vmax, v)
                    End If
                Next
            End If

            If Not any Then
                Return (-1, 1)
            End If

            Dim m As Double = std.Max(std.Abs(vmin), std.Abs(vmax))

            If m <= 0 OrElse Double.IsNaN(m) Then
                m = 1
            End If

            m *= 1.05

            Return (-m, m)
        End Function

        ' ========================================================
        '  贡献聚合
        ' ========================================================
        ''' <summary>
        ''' 按 |贡献| 降序取前 <paramref name="topN"/> 个特征，其余特征合并到 <see cref="ShapContributionGroup.Others"/>。
        ''' </summary>
        ''' <param name="shap">SHAP 分析结果</param>
        ''' <param name="features">样本特征表（仅用于取样本 ID）</param>
        ''' <param name="sampleIndex">样本下标</param>
        ''' <param name="topN">显示的特征数量，小于等于 0 表示全部显示</param>
        Public Function AggregateTop(shap As ShapAnalysisResult,
                                     features As NumericTable,
                                     sampleIndex As Integer,
                                     Optional topN As Integer = 10) As ShapContributionGroup

            Dim explanation As ShapExplanation = shap.Explanations(sampleIndex)
            Dim ranked As (name As String, value As Double)() = explanation.Ranked()
            Dim others As Double = 0
            Dim othersCount As Integer = 0

            If topN > 0 AndAlso ranked.Length > topN Then
                For i As Integer = topN To ranked.Length - 1
                    others += ranked(i).value
                    othersCount += 1
                Next

                ranked = ranked.Take(topN).ToArray
            End If

            Return New ShapContributionGroup(
                ranked,
                others,
                othersCount,
                explanation.Baseline,
                explanation.Output,
                If(String.IsNullOrEmpty(explanation.ID), SampleId(features, sampleIndex), explanation.ID))
        End Function

        ''' <summary>
        ''' 取值域 <c>[lo, hi]</c> 两侧各留一定比例的余量；值域为零的时候回退为正负 1。
        ''' </summary>
        Public Function PadRange(ByRef lo As Double, ByRef hi As Double, Optional pad As Double = 0.05) As (min As Double, max As Double)
            If hi - lo <= 0 Then
                Dim baseValue As Double = If(lo = 0, 1, std.Abs(lo) * 0.5)
                Return (lo - baseValue, hi + baseValue)
            End If

            Dim span As Double = (hi - lo) * pad
            Return (lo - span, hi + span)
        End Function

        ' ========================================================
        '  图例
        ' ========================================================
        ''' <summary>
        ''' 绘制「特征值高低」的蓝→红渐变图例条（蜂群图 / 依赖图共用）。
        ''' </summary>
        ''' <param name="g">绘图设备</param>
        ''' <param name="theme">主题，提供字体与文字颜色</param>
        ''' <param name="rect">色条矩形；宽大于高时按水平条绘制，否则按垂直条绘制</param>
        ''' <param name="title">图例标题（例如 <c>Feature value</c>）</param>
        ''' <param name="lowLabel">低值端标签</param>
        ''' <param name="highLabel">高值端标签</param>
        Public Sub DrawValueGradient(g As IGraphics, theme As PlotTheme, rect As RectangleF,
                                     Optional title As String = "",
                                     Optional lowLabel As String = "Low",
                                     Optional highLabel As String = "High")

            If g Is Nothing OrElse theme Is Nothing Then
                Return
            End If
            If rect.Width <= 0 OrElse rect.Height <= 0 Then
                Return
            End If

            Dim horizontal As Boolean = rect.Width >= rect.Height
            Const steps As Integer = 96

            For i As Integer = 0 To steps - 1
                Dim t As Double = i / (steps - 1)
                Dim c As Color = ValueColor(t)

                Using br As New SolidBrush(c)
                    If horizontal Then
                        g.FillRectangle(br, rect.X + CSng(rect.Width * i / steps), rect.Y,
                                        CSng(rect.Width / steps) + 1, rect.Height)
                    Else
                        ' 垂直条：底部为低值（蓝），顶部为高值（红）
                        g.FillRectangle(br, rect.X, rect.Bottom - CSng(rect.Height * (i + 1) / steps),
                                        rect.Width, CSng(rect.Height / steps) + 1)
                    End If
                End Using
            Next

            Using pen As New Pen(theme.AxisColor, 0.8F)
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height)
            End Using

            Using br As New SolidBrush(theme.TextColor)
                Dim font As Font = theme.TickLabelFont

                If horizontal Then
                    g.DrawString(lowLabel, font, br, rect.X, rect.Bottom + 2)
                    g.DrawString(highLabel, font, br,
                                 rect.Right - g.MeasureString(highLabel, font).Width, rect.Bottom + 2)

                    If Not String.IsNullOrEmpty(title) Then
                        g.DrawString(title, theme.AnnotationFont, br,
                                     rect.X, rect.Y - theme.AnnotationFont.GetHeight(g) - 4)
                    End If
                Else
                    g.DrawString(highLabel, font, br, rect.Right + 4, rect.Y)
                    g.DrawString(lowLabel, font, br,
                                 rect.Right + 4, rect.Bottom - font.GetHeight(g))

                    If Not String.IsNullOrEmpty(title) Then
                        Dim w As Single = g.MeasureString(title, theme.AnnotationFont).Width
                        g.DrawString(title, theme.AnnotationFont, br,
                                     rect.X + rect.Width / 2 - w / 2, rect.Y - theme.AnnotationFont.GetHeight(g) - 4)
                    End If
                End If
            End Using
        End Sub

    End Module

End Namespace

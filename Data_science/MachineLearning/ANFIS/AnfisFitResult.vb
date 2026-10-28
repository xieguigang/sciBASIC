Option Strict On
Option Explicit On

Imports System
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports std = System.Math

''' <summary>
''' ANFIS 的拟合结果：误差指标、训练历史与可解释性诊断。
''' </summary>
Public Class AnfisFitResult

    ''' <summary>训练好的模型（推理内核定义在 <c>FuzzyLogic.TSK</c>）。</summary>
    Public Property Model As TskNetwork
    ''' <summary>决定系数（在全部参与评估的样本上计算，多输出时按元素汇总）。</summary>
    Public Property R2 As Double
    ''' <summary>均方根误差（按元素，多输出时同尺度）。</summary>
    Public Property RMSE As Double
    ''' <summary>残差平方和。</summary>
    Public Property SSE As Double
    ''' <summary>总平方和。</summary>
    Public Property SST As Double
    ''' <summary>拟合值，形状 <c>[样本数, 输出数]</c>。</summary>
    Public Property FittedValues As Double()()
    ''' <summary>残差 <c>y - yhat</c>，形状 <c>[样本数, 输出数]</c>。</summary>
    Public Property Residuals As Double()()

    ''' <summary>每轮训练误差（RMSE）。</summary>
    Public Property TrainRmseHistory As Double()
    ''' <summary>每轮验证误差（RMSE）；没有验证集时为空数组。</summary>
    Public Property ValidationRmseHistory As Double()
    ''' <summary>每轮结论参数的 L2 范数（观察 LSE 是否在发散）。</summary>
    Public Property ThetaNormHistory As Double()

    ''' <summary>每条规则在训练集上的平均归一化触发强度（可解释性与剪枝依据）。</summary>
    Public Property RuleActivations As Double()
    ''' <summary>本次拟合中被剪掉的规则数量（未剪枝时为 0）。</summary>
    Public Property PrunedRuleCount As Integer

    ''' <summary>实际训练轮数。</summary>
    Public Property Iterations As Integer
    ''' <summary>验证误差最优的那一轮（从 0 计）。</summary>
    Public Property BestEpoch As Integer
    ''' <summary>是否因为早停而提前结束。</summary>
    Public Property Converged As Boolean
    ''' <summary>训练过程中的降级/告警信息。</summary>
    Public Property Warnings As String()

    ''' <summary>第 <paramref name="index"/> 个样本的拟合输出。</summary>
    Public Function GetY(index As Integer) As Double()
        Return FittedValues(index)
    End Function

    ''' <summary>按平均触发强度排序的规则摘要（含结论系数）。</summary>
    Public Function GetRuleSummary() As String()
        If Model Is Nothing Then
            Return New String() {}
        End If

        Dim n As Integer = Model.InputCount
        Dim o As Integer = Model.OutputCount
        Dim lines(Model.Rules.RuleCount - 1) As String

        For r As Integer = 0 To Model.Rules.RuleCount - 1
            Dim sb As New StringBuilder()
            Dim parts As String() = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                parts(i) = $"{Model.Inputs(i).Name} IS {Model.Inputs(i).FunctionLabel(Model.Rules.Antecedent(r, i))}"
            Next

            Dim activation As Double = If(RuleActivations IsNot Nothing AndAlso r < RuleActivations.Length, RuleActivations(r), 0.0)

            Call sb.Append($"rule{r + 1} [{activation.ToString("P2")}] IF {String.Join(" AND ", parts)} THEN ")

            Dim outputParts As String() = New String(o - 1) {}

            For k As Integer = 0 To o - 1
                Dim terms As String() = New String(n) {}
                Dim offset As Integer = r * (n + 1)

                For j As Integer = 0 To n - 1
                    terms(j) = $"{Model.Theta(offset + j, k).ToString("G5")}*{Model.Inputs(j).Name}"
                Next

                terms(n) = Model.Theta(offset + n, k).ToString("G5")
                outputParts(k) = $"{Model.OutputNames(k)} = {String.Join(" + ", terms)}"
            Next

            Call sb.Append(String.Join("; ", outputParts))
            lines(r) = sb.ToString()
        Next

        Return lines
    End Function

    ''' <summary>可读摘要。</summary>
    Public Overrides Function ToString() As String
        Dim text As New StringBuilder()

        Call text.AppendLine($"ANFIS: {If(Model Is Nothing, "?", Model.ToString())}")
        Call text.AppendLine($"R2 = {R2:G6}, RMSE = {RMSE:G6}, SSE = {SSE:G6}")
        Call text.AppendLine($"epochs = {Iterations}, best = {BestEpoch}, early stop = {Converged}")

        If PrunedRuleCount > 0 Then
            Call text.AppendLine($"pruned rules = {PrunedRuleCount}")
        End If

        If Warnings IsNot Nothing AndAlso Warnings.Length > 0 Then
            For Each message As String In Warnings
                Call text.AppendLine($"warning: {message}")
            Next
        End If

        Return text.ToString().TrimEnd()
    End Function

End Class

''' <summary>交叉验证结果。</summary>
Public Class AnfisCrossValidationResult

    ''' <summary>每一折的验证 RMSE。</summary>
    Public Property FoldRmse As Double()
    ''' <summary>每一折的验证 R2。</summary>
    Public Property FoldR2 As Double()
    ''' <summary>每一折的模型（便于检查折间稳定性）。</summary>
    Public Property Models As TskNetwork()

    ''' <summary>折平均 RMSE。</summary>
    Public ReadOnly Property MeanRmse As Double
        Get
            If FoldRmse Is Nothing OrElse FoldRmse.Length = 0 Then
                Return Double.NaN
            Else
                Return FoldRmse.Average()
            End If
        End Get
    End Property

    ''' <summary>折平均 R2。</summary>
    Public ReadOnly Property MeanR2 As Double
        Get
            If FoldR2 Is Nothing OrElse FoldR2.Length = 0 Then
                Return Double.NaN
            Else
                Return FoldR2.Average()
            End If
        End Get
    End Property

    ''' <summary>RMSE 的折间标准差（模型对数据划分的敏感程度）。</summary>
    Public ReadOnly Property StdRmse As Double
        Get
            If FoldRmse Is Nothing OrElse FoldRmse.Length < 2 Then
                Return 0.0
            End If

            Dim mean As Double = FoldRmse.Average()
            Dim sum As Double = 0.0

            For Each value As Double In FoldRmse
                sum += (value - mean) * (value - mean)
            Next

            Return std.Sqrt(sum / (FoldRmse.Length - 1))
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"CV RMSE = {MeanRmse:G6} +/- {StdRmse:G6}, R2 = {MeanR2:G6}, folds = {If(FoldRmse Is Nothing, 0, FoldRmse.Length)}"
    End Function

End Class

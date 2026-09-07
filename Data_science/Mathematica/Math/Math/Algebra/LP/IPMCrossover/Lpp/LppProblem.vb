' ============================================================================
' LppProblem.vb — LP 输入模型 + 标准形转换
' ----------------------------------------------------------------------------
' 输入（原始空间）：min/max cᵀx，约束 A_i·x {<=,>=,=} b_i（变量默认 x ≥ 0）。
' 标准形 [readme §一]：min c̃ᵀx̃，Ãx̃ = b̃，x̃ ≥ 0：
'   max → min：c̃ = −c_orig（σ = −1），报告量乘 σ 映回原始方向；
'   ≤ 行加 +松弛；≥ 行加 −松弛；= 行不加；
'   b_i < 0 的行整体翻转（flipSign = −1，保证 b̃ ≥ 0 供 Phase-1 人造基使用）。
' 映射回原始空间（LppSolver 提取时使用）：
'   shadowPrice_i = σ·flipSign_i·y_i；slack_i = b_i − A_i·x（原始数据直算）。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace LPP

    Public Class LppVariable

        Public Name As String
        Public Objective As Double

        Public Sub New(name As String, objective As Double)
            Me.Name = name
            Me.Objective = objective
        End Sub

    End Class

    Public Class LppConstraint

        ''' <summary>变量名 → 系数（缺省为 0）</summary>
        Public Coefficients As Dictionary(Of String, Double)
        ''' <summary>"<=" / ">=" / "="</summary>
        Public Op As String
        Public Rhs As Double

        Public Sub New(coefficients As Dictionary(Of String, Double), op As String, rhs As Double)
            Me.Coefficients = coefficients
            Me.Op = op
            Me.Rhs = rhs
        End Sub

    End Class

    Public Class LppProblem

        ''' <summary>"min" / "max"</summary>
        Public ObjectiveSense As String = "min"
        Public Variables As New List(Of LppVariable)()
        Public Constraints As New List(Of LppConstraint)()

        Public Function VariableNames() As String()
            Return Variables.Select(Function(v) v.Name).ToArray()
        End Function

        Public Function ConstraintTypes() As String()
            Return Constraints.Select(Function(k) k.Op).ToArray()
        End Function

    End Class

    ''' <summary>标准形（IPM/单纯形的工作对象）</summary>
    Public Class StandardForm

        Public A As Double(,)          ' m×(n+mSlack)
        Public b As Double()           ' m（已保证 ≥ 0）
        Public c As Double()           ' n+mSlack（内部 min 方向）
        Public M As Int32              ' 行数
        Public N As Int32              ' 原始变量数
        Public NSlack As Int32         ' 松弛/剩余变量数
        Public Sigma As Int32          ' +1: 原为 min；−1: 原为 max（c̃ = σ·c_orig）
        Public FlipSign As Double()    ' 每行翻转符号（±1）
        Public VarNames As String()
        Public ConstraintTypeList As String()
        ' 原始空间数据（slack/reduced cost 直算用；翻转与松弛加列之前）
        Public AOriginal As Double(,)
        Public BOriginal As Double()
        Public COriginal As Double()

        ''' <summary>由 LppProblem 构造标准形</summary>
        Public Shared Function FromProblem(prob As LppProblem) As StandardForm
            Dim sf As New StandardForm()
            sf.N = prob.Variables.Count
            sf.M = prob.Constraints.Count
            sf.Sigma = If(prob.ObjectiveSense.ToLowerInvariant().StartsWith("max"), -1, 1)
            sf.VarNames = prob.VariableNames()
            sf.ConstraintTypeList = prob.ConstraintTypes()
            sf.FlipSign = New Double(sf.M - 1) {}
            sf.NSlack = 0
            Dim slackOfRow(sf.M - 1) As Int32      ' −1 = 无；否则 +1(≤) / −1(≥)
            For i = 0 To sf.M - 1
                Dim op = prob.Constraints(i).Op
                If op = "<=" Then
                    slackOfRow(i) = 1
                    sf.NSlack += 1
                ElseIf op = ">=" Then
                    slackOfRow(i) = -1
                    sf.NSlack += 1
                ElseIf op = "=" Then
                    slackOfRow(i) = -1
                    ' 占位（下面统一计数后再分配）
                Else
                    Throw New ArgumentException($"未知约束类型: {op}")
                End If
            Next
            ' 先数一遍真正的松弛个数（= 行无松弛）
            Dim totalSlack As Int32 = 0
            For i = 0 To sf.M - 1
                If prob.Constraints(i).Op <> "=" Then totalSlack += 1
            Next
            sf.NSlack = totalSlack
            Dim ncols = sf.N + sf.NSlack
            sf.A = New Double(sf.M - 1, ncols - 1) {}
            sf.b = New Double(sf.M - 1) {}
            sf.c = New Double(ncols - 1) {}
            ' 系数与 RHS
            Dim slackPtr As Int32 = sf.N
            For i = 0 To sf.M - 1
                Dim con = prob.Constraints(i)
                For Each kvp In con.Coefficients
                    Dim j = Array.IndexOf(sf.VarNames, kvp.Key)
                    If j < 0 Then Throw New ArgumentException($"约束引用了未知变量: {kvp.Key}")
                    sf.A(i, j) += kvp.Value
                Next
                sf.b(i) = con.Rhs
                ' 松弛列
                If con.Op = "<=" Then
                    sf.A(i, slackPtr) = 1.0
                    slackPtr += 1
                ElseIf con.Op = ">=" Then
                    sf.A(i, slackPtr) = -1.0
                    slackPtr += 1
                End If
            Next
            ' 行翻转（b ≥ 0）
            For i = 0 To sf.M - 1
                If sf.b(i) < 0 Then
                    sf.FlipSign(i) = -1.0
                    For j = 0 To ncols - 1
                        sf.A(i, j) = -sf.A(i, j)
                    Next
                    sf.b(i) = -sf.b(i)
                Else
                    sf.FlipSign(i) = 1.0
                End If
            Next
            ' 内部目标：σ·c_orig（松弛列成本 0）
            For j = 0 To sf.N - 1
                sf.c(j) = sf.Sigma * prob.Variables(j).Objective
            Next
            ' 保留原始空间数据
            sf.AOriginal = New Double(sf.M - 1, sf.N - 1) {}
            For i = 0 To sf.M - 1
                For j = 0 To sf.N - 1
                    sf.AOriginal(i, j) = sf.A(i, j)
                Next
            Next
            sf.BOriginal = New Double(sf.M - 1) {}
            For i = 0 To sf.M - 1
                sf.BOriginal(i) = prob.Constraints(i).Rhs
            Next
            sf.COriginal = New Double(sf.N - 1) {}
            For j = 0 To sf.N - 1
                sf.COriginal(j) = prob.Variables(j).Objective
            Next
            Return sf
        End Function

        ''' <summary>影子价映射回原始方向：σ·flipSign_i·y_i = ∂(原始目标)/∂b_i</summary>
        Public Function MapShadowPrice(i As Int32, y As Double()) As Double
            Return Sigma * FlipSign(i) * y(i)
        End Function

    End Class

End Namespace

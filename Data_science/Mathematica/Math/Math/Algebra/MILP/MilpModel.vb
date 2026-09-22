' ============================================================================
' MilpModel.vb — 混合整数线性规划（MILP）数据模型
' ----------------------------------------------------------------------------
' 本文件只负责"问题建模"，不包含任何求解逻辑：
'   · MilpVarType  : 变量类型（连续 / 一般整数 / 二进制）
'   · MilpVariable : 继承既有 LppVariable，追加整数类型标记
'   · MilpModel    : 目标方向 + 变量表 + 约束表（约束复用 IPMCrossover.LppConstraint）
'
' 设计说明（复用优先，不重写既有 LP 代码）：
'   · 变量沿用 LppVariable 的 symbol / coefficient / LowerBound / UpperBound；
'   · 约束沿用 LppConstraint 的 Coefficients(Dictionary) / Op("<=", ">=", "=") / Rhs；
'   · 目标方向沿用 "min" / "max" 字符串约定（与 LppProblem 一致）。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' MILP 变量类型。
    ''' </summary>
    Public Enum MilpVarType

        ''' <summary>连续变量（可取实数）</summary>
        Continuous = 0
        ''' <summary>一般整数变量</summary>
        GeneralInteger = 1
        ''' <summary>二进制（0/1）变量</summary>
        Binary = 2

    End Enum

    ''' <summary>
    ''' MILP 变量：在既有 <see cref="LppVariable"/> 之上追加整数类型标记。
    ''' </summary>
    Public Class MilpVariable
        Inherits LppVariable

        ''' <summary>
        ''' 变量类型，默认连续。
        ''' </summary>
        Public Property VarType As MilpVarType = MilpVarType.Continuous

        Public Sub New()
        End Sub

        Public Sub New(name As String, objective As Double,
                       Optional type As MilpVarType = MilpVarType.Continuous)

            MyBase.New(name, objective)

            Me.VarType = type
        End Sub

        ''' <summary>
        ''' 是否为整数变量（含二进制）。
        ''' </summary>
        Public ReadOnly Property IsInteger As Boolean
            Get
                Return VarType = MilpVarType.GeneralInteger OrElse VarType = MilpVarType.Binary
            End Get
        End Property

        ''' <summary>
        ''' 是否为二进制变量。
        ''' </summary>
        Public ReadOnly Property IsBinary As Boolean
            Get
                Return VarType = MilpVarType.Binary
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{symbol} ({VarType})"
        End Function

    End Class

    ''' <summary>
    ''' MILP 模型：线性目标 + 线性约束 + 变量类型/上下界。
    ''' </summary>
    ''' <remarks>
    ''' 建模 API 采用流式写法，便于构造演示与测试问题：
    ''' <code>
    ''' Dim m As New MilpModel With {.ObjectiveSense = "max"}
    ''' m.AddVariable("x", 3).AddVariable("y", 5, MilpVarType.GeneralInteger)
    ''' m.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1}, {"y", 2}}, "&lt;=", 10)
    ''' </code>
    ''' </remarks>
    Public Class MilpModel

        ''' <summary>
        ''' 目标方向："min" 或 "max"。默认 "min"。
        ''' </summary>
        Public Property ObjectiveSense As String = "min"

        Public ReadOnly Property Variables As New List(Of MilpVariable)()
        Public ReadOnly Property Constraints As New List(Of LppConstraint)()

        ''' <summary>
        ''' 追加一个变量并返回模型本身（流式建模）。
        ''' </summary>
        Public Function AddVariable(name As String, objective As Double,
                                    Optional type As MilpVarType = MilpVarType.Continuous,
                                    Optional lowerBound As Double = 0,
                                    Optional upperBound As Double = Double.PositiveInfinity) As MilpModel

            Dim v As New MilpVariable(name, objective, type)

            If type = MilpVarType.Binary Then
                ' 二进制变量强制落在 [0, 1]（若用户传入更紧的界则保留更紧者）
                v.LowerBound = System.Math.Max(0.0, lowerBound)
                v.UpperBound = System.Math.Min(1.0, upperBound)
            Else
                v.LowerBound = lowerBound
                v.UpperBound = upperBound
            End If

            Variables.Add(v)

            Return Me
        End Function

        ''' <summary>
        ''' 追加一个约束并返回模型本身（流式建模）。
        ''' </summary>
        ''' <param name="coefficients">变量名 → 系数（缺省视为 0）</param>
        ''' <param name="op">"&lt;=" / "&gt;=" / "="</param>
        ''' <param name="rhs">约束右端项</param>
        Public Function AddConstraint(coefficients As Dictionary(Of String, Double),
                                      op As String, rhs As Double) As MilpModel

            Constraints.Add(New LppConstraint(coefficients, op, rhs))

            Return Me
        End Function

        ''' <summary>
        ''' 变量名 → 列索引。
        ''' </summary>
        Public Function VariableIndex() As Dictionary(Of String, Integer)
            Dim idx As New Dictionary(Of String, Integer)()

            For i As Integer = 0 To Variables.Count - 1
                Dim name As String = Variables(i).symbol

                If name.StringEmpty Then
                    name = "x" & (i + 1)
                    Variables(i).symbol = name
                End If

                idx(name) = i
            Next

            Return idx
        End Function

        ''' <summary>
        ''' 目标函数在当前变量顺序下的系数向量。
        ''' </summary>
        Public Function ObjectiveCoefficients() As Double()
            Return Variables.Select(Function(v) v.coefficient).ToArray()
        End Function

        ''' <summary>
        ''' 变量名称数组。
        ''' </summary>
        Public Function VariableNames() As String()
            Return Variables.Select(Function(v) v.symbol).ToArray()
        End Function

        ''' <summary>
        ''' 模型合法性校验。通过返回 <c>Nothing</c>，否则返回错误消息。
        ''' </summary>
        Public Function Validate() As String
            If Variables.Count = 0 Then Return "MILP 模型不包含任何变量。"
            If Constraints.Count = 0 Then Return Nothing

            Dim sense As String = If(ObjectiveSense, "min").ToLowerInvariant().Trim()

            If sense <> "min" AndAlso sense <> "max" Then
                Return $"未知的目标方向: {ObjectiveSense}（应为 ""min"" 或 ""max""）"
            End If

            Dim names As New HashSet(Of String)(StringComparer.Ordinal)
            Dim idx As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

            For i As Integer = 0 To Variables.Count - 1
                Dim v As MilpVariable = Variables(i)
                Dim name As String = If(v.symbol, "x" & (i + 1))

                If Not names.Add(name) Then
                    Return $"存在重复的变量名: {name}"
                End If

                idx(name) = i

                If Double.IsNaN(v.LowerBound) OrElse Double.IsNaN(v.UpperBound) Then
                    Return $"变量 {name} 的上下界为 NaN。"
                End If

                If v.LowerBound > v.UpperBound + 0.000000001 Then
                    Return $"变量 {name} 的上下界矛盾: [{v.LowerBound}, {v.UpperBound}]"
                End If

                If v.IsBinary AndAlso (v.LowerBound < -0.000000001 OrElse v.UpperBound > 1.000000001) Then
                    Return $"二进制变量 {name} 的界超出 [0, 1]: [{v.LowerBound}, {v.UpperBound}]"
                End If
            Next

            For k As Integer = 0 To Constraints.Count - 1
                Dim con As LppConstraint = Constraints(k)
                Dim op As String = If(con.Op, "").Trim()

                If op <> "<=" AndAlso op <> ">=" AndAlso op <> "=" AndAlso
                   op <> "≤" AndAlso op <> "≥" AndAlso op <> "<>" Then

                    Return $"约束 #{k + 1} 的运算符非法: {con.Op}"
                End If

                If Double.IsNaN(con.Rhs) Then
                    Return $"约束 #{k + 1} 的右端项为 NaN。"
                End If

                If con.Coefficients Is Nothing Then Continue For

                For Each kvp As KeyValuePair(Of String, Double) In con.Coefficients
                    If Not idx.ContainsKey(kvp.Key) Then
                        Return $"约束 #{k + 1} 引用了未知变量: {kvp.Key}"
                    End If

                    If Double.IsNaN(kvp.Value) Then
                        Return $"约束 #{k + 1} 中变量 {kvp.Key} 的系数为 NaN。"
                    End If
                Next
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 转换为既有 LP 层的 <see cref="LppProblem"/>（变量类型信息在此丢失，
        ''' 仅用于连续 LP 松弛的交叉校验/复用既有入口）。
        ''' </summary>
        Public Function ToLppProblem() As LppProblem
            Dim p As New LppProblem With {.ObjectiveSense = ObjectiveSense}

            p.Variables.AddRange(Variables.Cast(Of LppVariable)())

            For Each con As LppConstraint In Constraints
                p.Constraints.Add(New LppConstraint(con.Coefficients, con.Op, con.Rhs))
            Next

            Return p
        End Function

        Public Overrides Function ToString() As String
            Return $"{ObjectiveSense} {Variables.Count} vars ({Variables.Where(Function(v) v.IsInteger).Count()} integer), {Constraints.Count} constraints"
        End Function

    End Class

End Namespace

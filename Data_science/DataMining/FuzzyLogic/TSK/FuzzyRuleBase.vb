Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' TSK 的规则库：以**下标表**而不是字符串表示规则。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 第 <c>r</c> 条规则的第 <c>i</c> 个前件就是第 <c>i</c> 个输入的第
    ''' <c>Antecedent(r, i)</c> 个模糊集。用整数表而不是
    ''' <c>"IF x1 IS Cold AND x2 IS High"</c> 这样的文本，有两个直接好处：
    ''' 网格划分下的全部规则可以一次枚举完（<c>m^n</c> 个），前向/反向完全向量化；
    ''' 规则剪枝也只是删掉若干行，不需要重新解析。
    ''' </para>
    ''' <para>
    ''' 文本形式仅用于展示（<see cref="ToAntecedentText"/>），不参与计算。
    ''' </para>
    ''' </remarks>
    Public Class FuzzyRuleBase

        Private ReadOnly _antecedent As Integer(,)

        ''' <summary>从下标表创建（行 = 规则，列 = 输入）。</summary>
        Public Sub New(antecedent As Integer(,))
            If antecedent Is Nothing Then
                Throw New ArgumentNullException(NameOf(antecedent))
            End If
            If antecedent.GetLength(0) = 0 Then
                Throw New ArgumentException("the rule base requires at least one rule", NameOf(antecedent))
            End If

            _antecedent = antecedent
        End Sub

        ''' <summary>输入维数 n。</summary>
        Public ReadOnly Property InputCount As Integer
            Get
                Return _antecedent.GetLength(1)
            End Get
        End Property

        ''' <summary>规则数 R。</summary>
        Public ReadOnly Property RuleCount As Integer
            Get
                Return _antecedent.GetLength(0)
            End Get
        End Property

        ''' <summary>第 <paramref name="rule"/> 条规则的第 <paramref name="inputIndex"/> 个前件（模糊集编号）。</summary>
        Public Function Antecedent(rule As Integer, inputIndex As Integer) As Integer
            Return _antecedent(rule, inputIndex)
        End Function

        ''' <summary>
        ''' 按每个输入的模糊集个数枚举完整的网格规则库。
        ''' </summary>
        ''' <param name="mfCounts">每个输入的模糊集个数 <c>m_i</c></param>
        ''' <param name="enableMask">
        ''' 可选：长度等于 <c>m^n</c> 的启用掩膜（编号顺序与返回的完整网格一致）。
        ''' 传 <c>Nothing</c> 表示全部启用。
        ''' </param>
        ''' <param name="maxRules">规则数上限，&lt;= 0 表示不限制</param>
        ''' <remarks>
        ''' 规则数随输入维数呈指数增长（<c>prod(m_i)</c>），这是 TSK 最主要的规模约束。
        ''' 超过上限时这里会抛出带指引的异常，而不是等到分配出一个巨大的结论参数矩阵才失败。
        ''' </remarks>
        Public Shared Function CreateFullGrid(mfCounts As Integer(),
                                              Optional enableMask As Boolean() = Nothing,
                                              Optional maxRules As Integer = 0) As FuzzyRuleBase

            If mfCounts Is Nothing OrElse mfCounts.Length = 0 Then
                Throw New ArgumentException("the membership function count table is empty", NameOf(mfCounts))
            End If

            Dim total As Long = 1
            Dim n As Integer = mfCounts.Length

            For i As Integer = 0 To n - 1
                If mfCounts(i) <= 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(mfCounts), $"input [{i}] has no membership function")
                End If

                total *= mfCounts(i)

                If maxRules > 0 AndAlso total > maxRules Then
                    Throw New InvalidOperationException(
                        $"{n} inputs with {String.Join("x", mfCounts)} membership functions need {total} rules, " &
                        $"which exceeds MaxRules = {maxRules}. Reduce the membership function count per input, " &
                        $"select rules explicitly with TskStructureOptions.RuleEnableMask, or switch to a clustering based structure.")
                End If
            Next

            If total > 0 AndAlso total > Integer.MaxValue Then
                Throw New InvalidOperationException($"the rule base of {total} rules exceeds the addressable size of an array")
            End If

            Dim rules As Integer = CInt(total)
            Dim keep(rules - 1) As Boolean

            If enableMask Is Nothing Then
                For r As Integer = 0 To rules - 1
                    keep(r) = True
                Next
            ElseIf enableMask.Length <> rules Then
                Throw New ArgumentException($"the enable mask must have {rules} entries but got {enableMask.Length}", NameOf(enableMask))
            Else
                Call Array.Copy(enableMask, keep, rules)
            End If

            Dim used As Integer = 0

            For r As Integer = 0 To rules - 1
                If keep(r) Then
                    used += 1
                End If
            Next

            If used = 0 Then
                Throw New InvalidOperationException("the enable mask disabled every rule; at least one rule must remain active")
            End If

            Dim ante(used - 1, n - 1) As Integer
            Dim row As Integer = 0

            For r As Integer = 0 To rules - 1
                If Not keep(r) Then
                    Continue For
                End If

                Dim rest As Integer = r

                ' 低位对应最后一个输入：这样相邻编号只在最后一个输入的模糊集上变化，
                ' 便于人工检查与导出
                For i As Integer = n - 1 To 0 Step -1
                    ante(row, i) = rest Mod mfCounts(i)
                    rest = rest \ mfCounts(i)
                Next

                row += 1
            Next

            Return New FuzzyRuleBase(ante)
        End Function

        ''' <summary>只保留指定下标的规则，其余丢弃（用于剪枝）。</summary>
        Public Function SelectRules(indices As Integer()) As FuzzyRuleBase
            If indices Is Nothing OrElse indices.Length = 0 Then
                Throw New ArgumentException("at least one rule must be selected", NameOf(indices))
            End If

            Dim n As Integer = InputCount
            Dim ante(indices.Length - 1, n - 1) As Integer

            For r As Integer = 0 To indices.Length - 1
                Dim src As Integer = indices(r)

                If src < 0 OrElse src >= RuleCount Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"rule index {src} is out of range [0, {RuleCount})")
                End If

                For i As Integer = 0 To n - 1
                    ante(r, i) = _antecedent(src, i)
                Next
            Next

            Return New FuzzyRuleBase(ante)
        End Function

        ''' <summary>规则表的下标表副本（行 = 规则，列 = 输入）。</summary>
        Public Function ToAntecedentTable() As Integer(,)
            Return CType(_antecedent.Clone(), Integer(,))
        End Function

        ''' <summary>深拷贝。</summary>
        Public Function Clone() As FuzzyRuleBase
            Return New FuzzyRuleBase(ToAntecedentTable())
        End Function

        ''' <summary>把规则库导出为 IF-THEN 文本（前件部分；结论为线性函数，由模型提供系数）。</summary>
        Public Function ToAntecedentText(inputs As TskInputVariable()) As String()
            If inputs Is Nothing OrElse inputs.Length <> InputCount Then
                Throw New ArgumentException($"expected {InputCount} input variables", NameOf(inputs))
            End If

            Dim lines(RuleCount - 1) As String

            For r As Integer = 0 To RuleCount - 1
                Dim parts As String() = New String(InputCount - 1) {}

                For i As Integer = 0 To InputCount - 1
                    parts(i) = $"{inputs(i).Name} IS {inputs(i).FunctionLabel(_antecedent(r, i))}"
                Next

                lines(r) = $"rule{r + 1}: IF {String.Join(" AND ", parts)}"
            Next

            Return lines
        End Function

        ''' <summary>可读描述。</summary>
        Public Overrides Function ToString() As String
            Return $"{RuleCount} rules x {InputCount} antecedents"
        End Function

    End Class

End Namespace

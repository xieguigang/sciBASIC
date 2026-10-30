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
    ''' 另一种划分方式（**散射划分**，scatter partitioning）下，每条规则对每个输入都有自己的
    ''' 高斯中心/宽度，规则之间不再共享模糊集。这时 <see cref="IsScatter"/> 为 <c>True</c>，
    ''' <see cref="RuleCenters"/> / <see cref="RuleSigmas"/> 给出 <c>[R, n]</c> 的参数，
    ''' 而 <see cref="Antecedent"/> 只用来表示「该输入是否参与本条规则」：
    ''' <c>&gt;= 0</c> 参与、<c>-1</c> 表示 don't care（该规则对这一维没有约束）。
    ''' 规则数由簇数决定，与输入维数解耦——这是它能支持十几个输入的原因。
    ''' </para>
    ''' <para>
    ''' 文本形式仅用于展示（<see cref="ToAntecedentText"/> / <see cref="Describe(Integer, TskInputVariable())"/>），
    ''' 不参与计算。
    ''' </para>
    ''' </remarks>
    Public Class FuzzyRuleBase

        Private ReadOnly _antecedent As Integer(,)
        Private ReadOnly _centers As Double(,)
        Private ReadOnly _sigmas As Double(,)

        ''' <summary>每条规则参与前向计算的输入下标（延迟构建并缓存）。</summary>
        Private _active As Integer()()

        ''' <summary>从下标表创建（行 = 规则，列 = 输入）；网格划分使用。</summary>
        Public Sub New(antecedent As Integer(,))
            If antecedent Is Nothing Then
                Throw New ArgumentNullException(NameOf(antecedent))
            End If
            If antecedent.GetLength(0) = 0 Then
                Throw New ArgumentException("the rule base requires at least one rule", NameOf(antecedent))
            End If

            _antecedent = antecedent
        End Sub

        ''' <summary>从下标表 + 每规则高斯参数创建（散射划分）。</summary>
        ''' <param name="antecedent">
        ''' 形状 <c>[R, n]</c>；<c>&gt;= 0</c> 表示该规则使用这个输入，<c>-1</c> 表示 don't care
        ''' </param>
        ''' <param name="centers">高斯中心，形状 <c>[R, n]</c>；don't care 位置的值不会被使用</param>
        ''' <param name="sigmas">高斯宽度（必须为正），形状 <c>[R, n]</c></param>
        Public Sub New(antecedent As Integer(,), centers As Double(,), sigmas As Double(,))
            Me.New(antecedent)

            If centers Is Nothing OrElse sigmas Is Nothing Then
                Throw New ArgumentNullException(NameOf(centers))
            End If
            If centers.GetLength(0) <> RuleCount OrElse centers.GetLength(1) <> InputCount Then
                Throw New ArgumentException($"the rule centers must be ({RuleCount}, {InputCount})", NameOf(centers))
            End If
            If sigmas.GetLength(0) <> RuleCount OrElse sigmas.GetLength(1) <> InputCount Then
                Throw New ArgumentException($"the rule sigmas must be ({RuleCount}, {InputCount})", NameOf(sigmas))
            End If

            _centers = centers
            _sigmas = sigmas
        End Sub

        ''' <summary>是否为散射划分（每条规则自带高斯前件）。</summary>
        Public ReadOnly Property IsScatter As Boolean
            Get
                Return _centers IsNot Nothing
            End Get
        End Property

        ''' <summary>每规则的高斯中心 <c>[R, n]</c>；网格划分时为 <c>Nothing</c>。</summary>
        Public ReadOnly Property RuleCenters As Double(,)
            Get
                Return _centers
            End Get
        End Property

        ''' <summary>每规则的高斯宽度 <c>[R, n]</c>；网格划分时为 <c>Nothing</c>。</summary>
        Public ReadOnly Property RuleSigmas As Double(,)
            Get
                Return _sigmas
            End Get
        End Property

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

        ''' <summary>第 <paramref name="rule"/> 条规则的第 <paramref name="inputIndex"/> 个前件（网格：模糊集编号；散射：<c>-1</c> 表示 don't care）。</summary>
        Public Function Antecedent(rule As Integer, inputIndex As Integer) As Integer
            Return _antecedent(rule, inputIndex)
        End Function

        ''' <summary>该规则是否使用第 <paramref name="inputIndex"/> 个输入。</summary>
        Public Function IsActive(rule As Integer, inputIndex As Integer) As Boolean
            Return _antecedent(rule, inputIndex) >= 0
        End Function

        ''' <summary>第 <paramref name="rule"/> 条规则实际用到的输入下标（散射划分下可能少于 n）。</summary>
        Public Function ActiveInputs(rule As Integer) As Integer()
            Dim table As Integer()() = ActiveInputTable()

            Return table(rule)
        End Function

        ''' <summary>全部规则的活跃输入表（延迟构建并缓存）。</summary>
        ''' <remarks>前向/反向在每个行块都会遍历它，缓存一次可以避免每行重新扫描掩码。</remarks>
        Public Function ActiveInputTable() As Integer()()
            If _active IsNot Nothing Then
                Return _active
            End If

            Dim n As Integer = InputCount
            Dim table As Integer()() = New Integer(RuleCount - 1)() {}

            For r As Integer = 0 To RuleCount - 1
                Dim indices As New List(Of Integer)

                For i As Integer = 0 To n - 1
                    If _antecedent(r, i) >= 0 Then
                        Call indices.Add(i)
                    End If
                Next

                table(r) = indices.ToArray()
            Next

            _active = table

            Return table
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
        ''' 输入维数很高时应当改用 <see cref="CreateScatter"/>。
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

        ''' <summary>
        ''' 由「每规则高斯中心/宽度 + 活跃掩码」构建散射规则库。
        ''' </summary>
        ''' <param name="centers">高斯中心，形状 <c>[R, n]</c></param>
        ''' <param name="sigmas">高斯宽度（正数），形状 <c>[R, n]</c></param>
        ''' <param name="active">
        ''' 活跃掩码，形状 <c>[R, n]</c>；<c>Nothing</c> 时按「中心为有限数即活跃」推断
        ''' </param>
        ''' <param name="maxRules">规则数上限，&lt;= 0 表示不限制</param>
        Public Shared Function CreateScatter(centers As Double(,),
                                             sigmas As Double(,),
                                             Optional active As Boolean(,) = Nothing,
                                             Optional maxRules As Integer = 0) As FuzzyRuleBase

            If centers Is Nothing OrElse sigmas Is Nothing Then
                Throw New ArgumentNullException(NameOf(centers))
            End If

            Dim rules As Integer = centers.GetLength(0)
            Dim n As Integer = centers.GetLength(1)

            If rules <= 0 Then
                Throw New ArgumentException("the rule base requires at least one rule", NameOf(centers))
            End If
            If sigmas.GetLength(0) <> rules OrElse sigmas.GetLength(1) <> n Then
                Throw New ArgumentException($"the rule sigmas must be ({rules}, {n})", NameOf(sigmas))
            End If
            If maxRules > 0 AndAlso rules > maxRules Then
                Throw New InvalidOperationException(
                    $"{rules} cluster rules exceed MaxRules = {maxRules}. Reduce ScatterRuleCount.")
            End If

            Dim ante(rules - 1, n - 1) As Integer

            For r As Integer = 0 To rules - 1
                Dim used As Integer = 0

                For i As Integer = 0 To n - 1
                    ' 注意：不能叫 on —— On 是 VB 关键字
                    Dim enabled As Boolean

                    If active Is Nothing Then
                        enabled = Not Double.IsNaN(centers(r, i))
                    Else
                        enabled = active(r, i)
                    End If

                    If enabled AndAlso sigmas(r, i) > 0.0 Then
                        ante(r, i) = 0
                        used += 1
                    Else
                        ante(r, i) = -1
                    End If
                Next

                If used = 0 Then
                    Throw New InvalidOperationException(
                        $"the cluster rule {r + 1} has no active input; every rule needs at least one antecedent")
                End If
            Next

            Return New FuzzyRuleBase(ante, CType(centers.Clone(), Double(,)), CType(sigmas.Clone(), Double(,)))
        End Function

        ''' <summary>
        ''' 就地更新散射规则的高斯参数（供 <see cref="TskNetwork.SetPremiseParameters"/> 使用）。
        ''' </summary>
        ''' <param name="centers">新的中心，形状 <c>[R, n]</c></param>
        ''' <param name="sigmas">新的宽度，形状 <c>[R, n]</c>；非正数会被裁剪到 <paramref name="minSigma"/></param>
        ''' <param name="minSigma">宽度的下限，防止梯度更新把某条规则压成脉冲</param>
        Friend Sub UpdateScatterParameters(centers As Double(,), sigmas As Double(,), Optional minSigma As Double = 1.0E-06)
            If Not IsScatter Then
                Throw New InvalidOperationException("this rule base is not a scatter rule base")
            End If
            If centers Is Nothing OrElse sigmas Is Nothing Then
                Throw New ArgumentNullException(NameOf(centers))
            End If

            Dim rules As Integer = RuleCount
            Dim n As Integer = InputCount

            If centers.GetLength(0) <> rules OrElse centers.GetLength(1) <> n Then
                Throw New ArgumentException($"the rule centers must be ({rules}, {n})", NameOf(centers))
            End If
            If sigmas.GetLength(0) <> rules OrElse sigmas.GetLength(1) <> n Then
                Throw New ArgumentException($"the rule sigmas must be ({rules}, {n})", NameOf(sigmas))
            End If

            For r As Integer = 0 To rules - 1
                For i As Integer = 0 To n - 1
                    _centers(r, i) = centers(r, i)
                    _sigmas(r, i) = If(sigmas(r, i) > minSigma, sigmas(r, i), minSigma)
                Next
            Next
        End Sub

        ''' <summary>只保留指定下标的规则，其余丢弃（用于剪枝）。</summary>
        Public Function SelectRules(indices As Integer()) As FuzzyRuleBase
            If indices Is Nothing OrElse indices.Length = 0 Then
                Throw new ArgumentException("at least one rule must be selected", NameOf(indices))
            End If

            Dim n As Integer = InputCount
            Dim ante(indices.Length - 1, n - 1) As Integer
            Dim centers As Double(,) = Nothing
            Dim sigmas As Double(,) = Nothing

            If IsScatter Then
                centers = New Double(indices.Length - 1, n - 1) {}
                sigmas = New Double(indices.Length - 1, n - 1) {}
            End If

            For r As Integer = 0 To indices.Length - 1
                Dim src As Integer = indices(r)

                If src < 0 OrElse src >= RuleCount Then
                    Throw New ArgumentOutOfRangeException(NameOf(indices), $"rule index {src} is out of range [0, {RuleCount})")
                End If

                For i As Integer = 0 To n - 1
                    ante(r, i) = _antecedent(src, i)

                    If IsScatter Then
                        centers(r, i) = _centers(src, i)
                        sigmas(r, i) = _sigmas(src, i)
                    End If
                Next
            Next

            If IsScatter Then
                Return New FuzzyRuleBase(ante, centers, sigmas)
            Else
                Return New FuzzyRuleBase(ante)
            End If
        End Function

        ''' <summary>规则表的下标表副本（行 = 规则，列 = 输入）。</summary>
        Public Function ToAntecedentTable() As Integer(,)
            Return CType(_antecedent.Clone(), Integer(,))
        End Function

        ''' <summary>散射参数的中心表副本；网格划分时返回 <c>Nothing</c>。</summary>
        Public Function ToCenterTable() As Double(,)
            If _centers Is Nothing Then
                Return Nothing
            End If

            Return CType(_centers.Clone(), Double(,))
        End Function

        ''' <summary>散射参数的宽度表副本；网格划分时返回 <c>Nothing</c>。</summary>
        Public Function ToSigmaTable() As Double(,)
            If _sigmas Is Nothing Then
                Return Nothing
            End If

            Return CType(_sigmas.Clone(), Double(,))
        End Function

        ''' <summary>深拷贝。</summary>
        Public Function Clone() As FuzzyRuleBase
            If IsScatter Then
                Return New FuzzyRuleBase(ToAntecedentTable(), ToCenterTable(), ToSigmaTable())
            Else
                Return New FuzzyRuleBase(ToAntecedentTable())
            End If
        End Function

        ''' <summary>
        ''' 描述一条规则的前件部分。
        ''' </summary>
        ''' <param name="rule">规则下标</param>
        ''' <param name="inputs">输入变量（提供名字；网格划分下还需要用它的模糊集标签）</param>
        ''' <returns>例如 <c>IF x1 IS N(0.42, 0.18) AND x3 IS N(-1.2, 0.4)</c></returns>
        ''' <remarks>
        ''' 散射划分下 don't care 的输入会被跳过；网格划分下所有输入都会出现。
        ''' 这个方法是「规则文本」的唯一实现，避免各个调用方各写一套 <c>FunctionLabel(-1)</c> 那种越界标签。
        ''' </remarks>
        Public Function Describe(rule As Integer, inputs As TskInputVariable()) As String
            If inputs Is Nothing OrElse inputs.Length <> InputCount Then
                Throw New ArgumentException($"expected {InputCount} input variables", NameOf(inputs))
            End If

            Dim parts As New List(Of String)
            Dim n As Integer = InputCount

            If IsScatter Then
                Dim active As Integer() = ActiveInputs(rule)

                For Each i As Integer In active
                    Call parts.Add($"{inputs(i).Name} IS N({_centers(rule, i):G4}, {_sigmas(rule, i):G3})")
                Next
            Else
                For i As Integer = 0 To n - 1
                    Call parts.Add($"{inputs(i).Name} IS {inputs(i).FunctionLabel(_antecedent(rule, i))}")
                Next
            End If

            If parts.Count = 0 Then
                Return $"rule{rule + 1}: IF TRUE"
            End If

            Return $"rule{rule + 1}: IF {String.Join(" AND ", parts)}"
        End Function

        ''' <summary>把规则库导出为 IF-THEN 文本（前件部分；结论为线性函数，由模型提供系数）。</summary>
        Public Function ToAntecedentText(inputs As TskInputVariable()) As String()
            If inputs Is Nothing OrElse inputs.Length <> InputCount Then
                Throw New ArgumentException($"expected {InputCount} input variables", NameOf(inputs))
            End If

            Dim lines(RuleCount - 1) As String

            For r As Integer = 0 To RuleCount - 1
                lines(r) = Describe(r, inputs)
            Next

            Return lines
        End Function

        ''' <summary>可读描述。</summary>
        Public Overrides Function ToString() As String
            If IsScatter Then
                Return $"{RuleCount} cluster rules x {InputCount} inputs (scatter)"
            Else
                Return $"{RuleCount} rules x {InputCount} antecedents (grid)"
            End If
        End Function

    End Class

End Namespace

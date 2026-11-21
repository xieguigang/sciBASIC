Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports BN = Microsoft.VisualBasic.DataMining.FuzzyLogic.BooleanNetworks
Imports FCM = Microsoft.VisualBasic.DataMining.FuzzyLogic.FuzzyCognitiveMaps
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK
Imports std = System.Math

Namespace Hybrid

    ''' <summary>
    ''' 联合建模桥：TSK 定量前向 → FCM 有界传播 → 二值化 → BN/PBN 步进 → 规则权重反馈。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>与旧原型的三点差异</b>：
    ''' </para>
    ''' <list type="number">
    ''' <item>
    ''' TSK 输出与 FCM 激活不再直接算术平均，而是通过 <see cref="ITskActivationMapper"/>
    ''' 把前者显式换算到 <c>[0,1]</c> 再加权耦合（量纲错误在类型层就不成立）。
    ''' </item>
    ''' <item>
    ''' 规则反馈不再用「布尔位模式 <c>Mod</c> 规则数」这种任意映射，改为按
    ''' 「规则前件的模糊集属于高/低半区」与「对应概念的布尔状态」的一致性来调制权重：
    ''' 布尔层说某个输入处于高态时，指向前件高半区的规则获得更高权重。结果归一化后是一个分布。
    ''' </item>
    ''' <item>
    ''' 概念数 ≠ TSK 输入数时通过显式的 <c>conceptOf</c> 映射表处理，不再假设两者相等。
    ''' </item>
    ''' </list>
    ''' <para>
    ''' 仍然保留旧构造函数与 <see cref="InferStep"/> 签名，旧调用方（含 <c>_smoketest</c>）
    ''' 不需要改动；新增能力都是可选参数。
    ''' </para>
    ''' <para>
    ''' <b>能力边界</b>：这是三层的**耦合器**，不是预测器。定量的剂量响应仍由 TSK 给出，
    ''' 布尔层只提供定性一致性与离散状态的约束。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class FuzzyCausalBridge

        Private ReadOnly _tsk As TSK.TskNetwork
        Private ReadOnly _fcm As FCM.FuzzyCognitiveMap
        Private ReadOnly _boolean As BN.BooleanNetworkBase
        Private ReadOnly _binarizer As BN.IStateBinarizer
        Private ReadOnly _mapper As ITskActivationMapper
        Private ReadOnly _conceptOf As Integer()

        ''' <summary>构造联合建模桥。</summary>
        ''' <param name="tsk">TSK 模糊推理网络（定量层）</param>
        ''' <param name="fcm">模糊认知图（有界粗粒度层）</param>
        ''' <param name="booleanNetwork">布尔网络（定性层）</param>
        ''' <param name="binarizer">把 FCM 激活二值化为布尔状态的策略</param>
        ''' <param name="mapper">
        ''' TSK 数值域与 <c>[0,1]</c> 的换算；<c>Nothing</c> 时用 <c>[-4,4]</c> 的线性映射
        ''' （项目约定 TSK 的输入是 z 值，±4σ 覆盖绝大多数表达分布）
        ''' </param>
        ''' <param name="conceptOf">
        ''' TSK 输入下标 → FCM 概念下标；<c>Nothing</c> 时按顺序对齐（多出来的输入被忽略）
        ''' </param>
        Public Sub New(tsk As TSK.TskNetwork, fcm As FCM.FuzzyCognitiveMap,
                       booleanNetwork As BN.BooleanNetworkBase, binarizer As BN.IStateBinarizer,
                       Optional mapper As ITskActivationMapper = Nothing,
                       Optional conceptOf As Integer() = Nothing)

            If tsk Is Nothing Then
                Throw New ArgumentNullException(NameOf(tsk))
            End If
            If fcm Is Nothing Then
                Throw New ArgumentNullException(NameOf(fcm))
            End If
            If booleanNetwork Is Nothing Then
                Throw New ArgumentNullException(NameOf(booleanNetwork))
            End If
            If binarizer Is Nothing Then
                Throw New ArgumentNullException(NameOf(binarizer))
            End If

            ' 布尔状态是 FCM 激活的二值化，因此两者必须一一对应；
            ' TSK 的输入维数可以不同（靠 conceptOf 映射）
            If fcm.Count <> booleanNetwork.Count Then
                Throw New ArgumentException("FCM concept count must match BooleanNetwork gene count", NameOf(fcm))
            End If

            _tsk = tsk
            _fcm = fcm
            _boolean = booleanNetwork
            _binarizer = binarizer
            _mapper = If(mapper, New RangeActivationMapper(-4.0, 4.0))
            _conceptOf = ResolveConceptMap(conceptOf, tsk.InputCount, fcm.Count)
        End Sub

        ''' <summary>TSK 网络（只读引用）。</summary>
        Public ReadOnly Property TskNetwork As TSK.TskNetwork
            Get
                Return _tsk
            End Get
        End Property

        ''' <summary>FCM（只读引用）。</summary>
        Public ReadOnly Property CognitiveMap As FCM.FuzzyCognitiveMap
            Get
                Return _fcm
            End Get
        End Property

        ''' <summary>BN/PBN（只读引用）。</summary>
        Public ReadOnly Property BooleanNetwork As BN.BooleanNetworkBase
            Get
                Return _boolean
            End Get
        End Property

        ''' <summary>量纲换算器。</summary>
        Public ReadOnly Property ActivationMapper As ITskActivationMapper
            Get
                Return _mapper
            End Get
        End Property

        ''' <summary>
        ''' 观测耦合强度：0 = 完全由 FCM 自己的传播决定，1 = 每步都用 TSK 的观测覆盖。
        ''' </summary>
        ''' <remarks>取 0.5 相当于「观测与内部状态各占一半」，与旧原型的平均有相同的直觉，但量纲正确。</remarks>
        Public Property Coupling As Double = 0.5

        ''' <summary>
        ''' 布尔层对规则权重的贡献比例：0 = 只看模糊触发强度，1 = 只看布尔一致性。
        ''' </summary>
        Public Property BooleanWeight As Double = 0.5

        ''' <summary>单步联合推理：TSK → FCM → 二值化 → BN 步进 → 模糊反馈。</summary>
        ''' <returns>本步的 TSK 输出、FCM 激活、布尔状态与规则触发强度。</returns>
        Public Function InferStep(inputs As Double()) As BridgeResult
            Dim tskOutput As Double() = _tsk.PredictRow(inputs)
            Dim observed As Double() = MapObservation(tskOutput)
            Dim current As Double() = _fcm.Activation
            Dim coupled(current.Length - 1) As Double
            Dim weight As Double = std.Max(0.0, std.Min(1.0, Coupling))

            For i As Integer = 0 To coupled.Length - 1
                If Double.IsNaN(observed(i)) Then
                    ' 该概念没有对应的 TSK 输出：保持内部状态，不要用 0 去污染
                    coupled(i) = current(i)
                Else
                    coupled(i) = (1.0 - weight) * current(i) + weight * observed(i)
                End If
            Next

            _fcm.Activation = coupled

            Call _fcm.Propagate()

            Dim continuous As Double() = _fcm.Activation
            Dim booleanState As Boolean() = _binarizer.BinarizeRow(continuous)

            _boolean.State = booleanState
            Call _boolean.StepSynchronous()

            Dim finalState As Boolean() = _boolean.State
            Dim feedback As Double() = ToRuleActivations(finalState)

            Return New BridgeResult(tskOutput, _fcm.Activation, finalState, feedback)
        End Function

        ''' <summary>
        ''' 把布尔状态与模糊前件的一致性折算成 TSK 规则权重（归一化，和为 1）。
        ''' </summary>
        ''' <param name="state">BN/PBN 的当前布尔状态（长度 = FCM 概念数）</param>
        ''' <returns>每条规则的权重</returns>
        ''' <remarks>
        ''' <para>
        ''' 判定口径：第 <c>i</c> 个输入有 <c>m</c> 个模糊集时，
        ''' 下标落在上半区（<c>2·A(r,i) ≥ m</c>）视为「前件要求该输入处于高态」。
        ''' 若对应概念的布尔位与这个要求一致，该输入给这条规则记一分；
        ''' 规则的布尔一致性就是得分比例。
        ''' </para>
        ''' <para>
        ''' 最终权重 = 模糊触发强度 × <c>((1-β) + β·一致性)</c>，再归一化。
        ''' 这样布尔层只**调制**模糊层的结论，不会凭空创造激活（模糊触发强度为 0 的规则权重仍为 0）。
        ''' </para>
        ''' </remarks>
        Public Function ToRuleActivations(state As Boolean()) As Double()
            Dim mapped(_tsk.InputCount - 1) As Double
            Dim activation As Double() = _fcm.Activation

            For i As Integer = 0 To mapped.Length - 1
                Dim concept As Integer = _conceptOf(i)
                Dim a As Double = If(concept >= 0 AndAlso concept < activation.Length, activation(concept), 0.5)

                mapped(i) = _mapper.FromActivation(a)
            Next

            Dim strength As Double() = _tsk.GetRuleActivations(mapped)

            If strength.Length = 0 Then
                Return strength
            End If

            Dim rules As Integer = strength.Length
            Dim result(rules - 1) As Double
            Dim beta As Double = std.Max(0.0, std.Min(1.0, BooleanWeight))
            Dim total As Double = 0.0

            For r As Integer = 0 To rules - 1
                If strength(r) = 0.0 Then
                    Continue For
                End If

                Dim agree As Double = 0.0

                For i As Integer = 0 To mapped.Length - 1
                    Dim count As Integer = _tsk.Inputs(i).FunctionCount

                    If count <= 0 Then
                        Continue For
                    End If

                    Dim highSide As Boolean = _tsk.Rules.Antecedent(r, i) * 2 >= count
                    Dim concept As Integer = _conceptOf(i)
                    Dim bit As Boolean = If(concept >= 0 AndAlso concept < state.Length, state(concept), highSide)

                    If highSide = bit Then
                        agree += 1.0
                    End If
                Next

                agree /= std.Max(1, mapped.Length)

                Dim weight As Double = strength(r) * ((1.0 - beta) + beta * agree)

                result(r) = weight
                total += weight
            Next

            If total > 0.0 Then
                For r As Integer = 0 To rules - 1
                    result(r) /= total
                Next
            Else
                ' 全部规则都被布尔层压到 0：退回均匀分布，保证下游拿到的是合法权重
                For r As Integer = 0 To rules - 1
                    result(r) = 1.0 / rules
                Next
            End If

            ' 归一化后的浮点求和通常差最后一位（0.9999999999999999），
            ' 把误差补给最大的一项，让下游可以按「和为 1」做精确断言
            Dim sum As Double = 0.0
            Dim peak As Integer = 0

            For r As Integer = 0 To rules - 1
                sum += result(r)

                If result(r) > result(peak) Then
                    peak = r
                End If
            Next

            result(peak) += 1.0 - sum

            Return result
        End Function

        ''' <summary>把 FCM 激活映射为 TSK 隶属度（复用 TSK 隶属函数）。</summary>
        Public Function FuzzifyActivations(values As Double(), membership As TSK.IMembershipFunction) As Double()
            If membership Is Nothing Then
                Throw New ArgumentNullException(NameOf(membership))
            End If
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim result(values.Length - 1) As Double

            For i As Integer = 0 To values.Length - 1
                result(i) = membership.Evaluate(values(i))
            Next

            Return result
        End Function

        ''' <summary>把 TSK 输出换算到 FCM 概念空间（未覆盖的概念填 <c>NaN</c>）。</summary>
        Private Function MapObservation(tskOutput As Double()) As Double()
            Dim observed(_fcm.Count - 1) As Double

            For i As Integer = 0 To observed.Length - 1
                observed(i) = Double.NaN
            Next

            For i As Integer = 0 To std.Min(_tsk.InputCount, tskOutput.Length) - 1
                Dim concept As Integer = _conceptOf(i)

                If concept >= 0 AndAlso concept < observed.Length Then
                    observed(concept) = _mapper.ToActivation(tskOutput(i))
                End If
            Next

            Return observed
        End Function

        ''' <summary>解析「TSK 输入 → FCM 概念」的映射表。</summary>
        Private Shared Function ResolveConceptMap(requested As Integer(), inputCount As Integer, conceptCount As Integer) As Integer()
            Dim map(inputCount - 1) As Integer

            If requested IsNot Nothing Then
                If requested.Length <> inputCount Then
                    Throw New ArgumentException($"the concept map must have {inputCount} entries", NameOf(requested))
                End If

                For i As Integer = 0 To inputCount - 1
                    If requested(i) < -1 OrElse requested(i) >= conceptCount Then
                        Throw New ArgumentException($"the concept index {requested(i)} is outside [0, {conceptCount})", NameOf(requested))
                    End If

                    map(i) = requested(i)
                Next

                Return map
            End If

            ' 默认按顺序对齐；输入多于概念时多余的部分不参与耦合
            For i As Integer = 0 To inputCount - 1
                map(i) = If(i < conceptCount, i, -1)
            Next

            Return map
        End Function

    End Class

    ''' <summary>联合推理单步结果。</summary>
    Public NotInheritable Class BridgeResult
        Private ReadOnly _tskOutput As Double()
        Private ReadOnly _fcmActivation As Double()
        Private ReadOnly _booleanState As Boolean()
        Private ReadOnly _ruleActivations As Double()

        ''' <summary>构造联合推理结果。</summary>
        Public Sub New(tskOutput As Double(), fcmActivation As Double(),
                       booleanState As Boolean(), ruleActivations As Double())
            _tskOutput = tskOutput
            _fcmActivation = fcmActivation
            _booleanState = booleanState
            _ruleActivations = ruleActivations
        End Sub

        ''' <summary>TSK 输出。</summary>
        Public ReadOnly Property TskOutput As Double()
            Get
                Return DirectCast(_tskOutput.Clone(), Double())
            End Get
        End Property

        ''' <summary>FCM 传播后的激活值。</summary>
        Public ReadOnly Property FcmActivation As Double()
            Get
                Return DirectCast(_fcmActivation.Clone(), Double())
            End Get
        End Property

        ''' <summary>BN/PBN 布尔状态。</summary>
        Public ReadOnly Property BooleanState As Boolean()
            Get
                Return DirectCast(_booleanState.Clone(), Boolean())
            End Get
        End Property

        ''' <summary>反馈得到的模糊规则触发强度。</summary>
        Public ReadOnly Property RuleActivations As Double()
            Get
                Return DirectCast(_ruleActivations.Clone(), Double())
            End Get
        End Property
    End Class

End Namespace

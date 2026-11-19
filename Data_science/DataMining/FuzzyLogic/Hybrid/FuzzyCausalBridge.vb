Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports BN = Microsoft.VisualBasic.DataMining.FuzzyLogic.BooleanNetworks
Imports FCM = Microsoft.VisualBasic.DataMining.FuzzyLogic.FuzzyCognitiveMaps
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

Namespace Hybrid

    ''' <summary>
    ''' 联合建模最小原型：串联 TSK 模糊化 → FCM 传播 → 二值化 → BN/PBN 步进，
    ''' 并把布尔状态反馈为 TSK 模糊规则的触发强度。
    ''' </summary>
    Public NotInheritable Class FuzzyCausalBridge

        Private ReadOnly _tsk As TSK.TskNetwork
        Private ReadOnly _fcm As FCM.FuzzyCognitiveMap
        Private ReadOnly _boolean As BN.BooleanNetworkBase
        Private ReadOnly _binarizer As BN.IStateBinarizer

        ''' <summary>构造联合建模桥。</summary>
        Public Sub New(tsk As TSK.TskNetwork, fcm As FCM.FuzzyCognitiveMap,
                       booleanNetwork As BN.BooleanNetworkBase, binarizer As BN.IStateBinarizer)
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
            If fcm.Count <> booleanNetwork.Count Then
                Throw New ArgumentException("FCM concept count must match BooleanNetwork gene count", NameOf(fcm))
            End If

            _tsk = tsk
            _fcm = fcm
            _boolean = booleanNetwork
            _binarizer = binarizer
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

        ''' <summary>单步联合推理：TSK → FCM → 二值化 → BN 步进 → 模糊反馈。</summary>
        ''' <returns>本步的 TSK 输出、FCM 激活、布尔状态与规则触发强度。</returns>
        Public Function InferStep(inputs As Double()) As BridgeResult
            Dim tskOutput As Double() = _tsk.PredictRow(inputs)
            Dim activations As Double() = _fcm.Activation
            Dim n As Integer = System.Math.Min(tskOutput.Length, activations.Length)

            For i As Integer = 0 To n - 1
                activations(i) = (activations(i) + tskOutput(i)) / 2.0
            Next

            _fcm.Activation = activations
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
        ''' 把布尔状态映射为 TSK 模糊规则触发强度：每个布尔状态组合对应一条规则的权重。
        ''' </summary>
        ''' <remarks>
        ''' 规则索引 = 布尔状态位模式对规则数的模；这是一种确定性的最小反馈映射，
        ''' 用于验证联合链路可运行，不试图替代 TSK 原有的隶属函数推理。
        ''' </remarks>
        Public Function ToRuleActivations(state As Boolean()) As Double()
            Dim activations As Double() = _tsk.GetRuleActivations(_fcm.Activation)
            Dim code As ULong = BN.StateSpace.Encode(state)

            If activations.Length = 0 Then
                Return activations
            End If

            Dim target As Integer = CInt(code Mod CULng(activations.Length))
            Dim result(activations.Length - 1) As Double

            result(target) = 1.0

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

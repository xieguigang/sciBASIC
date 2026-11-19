Option Strict On
Option Explicit On

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports BN = Microsoft.VisualBasic.DataMining.FuzzyLogic.BooleanNetworks
Imports FCM = Microsoft.VisualBasic.DataMining.FuzzyLogic.FuzzyCognitiveMaps
Imports HY = Microsoft.VisualBasic.DataMining.FuzzyLogic.Hybrid
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

''' <summary>联合建模自检：连续 → TSK → FCM → 二值化 → BN → 模糊反馈全链路。</summary>
Public Module HybridAudit

    Public Function Run() As Integer
        Call DebugAudit.Mark("联合建模自检开始")

        Call TestBridge()
        Call TestFuzzify()

        Dim failures As Integer = DebugAudit.FailureCount

        Call DebugAudit.Mark("联合建模自检结束", failures)

        Return failures
    End Function

    Private Function CreateBridge() As HY.FuzzyCausalBridge
        Dim inputs As TSK.TskInputVariable() = {
            TSK.TskInputVariable.CreateGrid("x1", -1.0, 1.0, 3, TSK.MembershipKind.Gaussian)
        }
        Dim rules As TSK.FuzzyRuleBase = TSK.FuzzyRuleBase.CreateFullGrid({3})
        Dim tskNet As New TSK.TskNetwork(inputs, rules, 1)
        Dim theta(5, 0) As Double

        theta(0, 0) = 0.5
        tskNet.Theta = theta

        Dim concepts As FCM.FcmConcept() = {
            New FCM.FcmConcept("G1", 0.5),
            New FCM.FcmConcept("G2", 0.1)
        }
        Dim weights(1, 1) As Double

        weights(0, 1) = 0.5
        Dim fcm As New FCM.FuzzyCognitiveMap(concepts, weights, New FCM.ClampedLinearActivation(0.0, 1.0))

        Dim a As New BN.BooleanNode("G1", New Integer() {}, BN.BooleanTruthTable.FromValues(0, True))
        Dim b As New BN.BooleanNode("G2", New Integer() {0}, BN.BooleanTruthTable.CreateAnd(1))
        Dim net As New BN.BooleanNetwork(New BN.BooleanNode() {a, b}, New Boolean() {False, False})
        Dim binarizer As New BN.FixedThresholdBinarizer(0.4)

        Return New HY.FuzzyCausalBridge(tskNet, fcm, net, binarizer)
    End Function

    Private Sub TestBridge()
        Dim bridge As HY.FuzzyCausalBridge = CreateBridge()
        Dim result As HY.BridgeResult = bridge.InferStep(New Double() {0.5})

        Call DebugAudit.AreEqual(1, result.TskOutput.Length, "TSK 输出维度")
        Call DebugAudit.AreEqual(2, result.FcmActivation.Length, "FCM 激活维度")
        Call DebugAudit.AreEqual(2, result.BooleanState.Length, "布尔状态维度")
        Call DebugAudit.AreEqual(3, result.RuleActivations.Length, "反馈规则数")
        Call DebugAudit.AreEqual(True, result.BooleanState(0), "常量真基因进布尔网络")

        Dim sum As Double = 0.0

        For Each value As Double In result.RuleActivations
            sum += value
        Next

        Call DebugAudit.AreEqual(1.0, sum, "反馈触发强度和为 1")
    End Sub

    Private Sub TestFuzzify()
        Dim bridge As HY.FuzzyCausalBridge = CreateBridge()
        Dim membership As TSK.IMembershipFunction = TSK.MembershipFactory.Create(
            TSK.MembershipKind.Gaussian, New Double() {0.0, 1.0})
        Dim values As Double() = bridge.FuzzifyActivations(New Double() {-1.0, 0.0, 1.0}, membership)

        Call DebugAudit.AreEqual(3, values.Length, "模糊化输出长度")
        Call DebugAudit.AreEqual(True, values(1) >= values(0) AndAlso values(1) >= values(2),
                                 "Gaussian 中心隶属度最高")
    End Sub
End Module

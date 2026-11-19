Option Strict On
Option Explicit On

Imports System
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports BN = Microsoft.VisualBasic.DataMining.FuzzyLogic.BooleanNetworks

''' <summary>
''' BN/PBN 自检：真值表、同步异步步进、吸引子、PBN 可复现、推断与 JSON 往返。
''' 全部断言经 DebugAudit 记录，不在此模块定义 Main。
''' </summary>
Public Module BooleanNetworksAudit

    Public Function Run() As Integer
        Call DebugAudit.Mark("BN/PBN 自检开始")

        Call TestTruthTable()
        Call TestNetworkSteps()
        Call TestAttractor()
        Call TestPbnReproducibility()
        Call TestInference()
        Call TestJson()

        Dim failures As Integer = DebugAudit.FailureCount

        Call DebugAudit.Mark("BN/PBN 自检结束", failures)

        Return failures
    End Function

    Private Sub TestTruthTable()
        Dim andTable As BN.BooleanTruthTable = BN.BooleanTruthTable.CreateAnd(2)

        Call DebugAudit.AreEqual(False, andTable.Evaluate(New Boolean() {True, False}), "AND 真值表 10")
        Call DebugAudit.AreEqual(True, andTable.Evaluate(New Boolean() {True, True}), "AND 真值表 11")

        Dim xorTable As BN.BooleanTruthTable = BN.BooleanTruthTable.CreateXor(2)

        Call DebugAudit.AreEqual(True, xorTable.Evaluate(New Boolean() {True, False}), "XOR 真值表 10")
        Call DebugAudit.AreEqual(False, xorTable.Evaluate(New Boolean() {True, True}), "XOR 真值表 11")

        Dim negated As BN.BooleanTruthTable = andTable.Negate()

        Call DebugAudit.AreEqual(True, negated.Evaluate(New Boolean() {True, False}), "取反语义")
    End Sub

    Private Function CreateNetwork() As BN.BooleanNetwork
        Dim a As New BN.BooleanNode("A", New Integer() {}, BN.BooleanTruthTable.FromValues(0, True))
        Dim b As New BN.BooleanNode("B", New Integer() {0}, BN.BooleanTruthTable.CreateNot())
        Dim c As New BN.BooleanNode("C", New Integer() {0, 1}, BN.BooleanTruthTable.CreateAnd(2))
        Dim nodes As BN.BooleanNode() = {a, b, c}

        Return New BN.BooleanNetwork(nodes, New Boolean() {False, False, False})
    End Function

    Private Sub TestNetworkSteps()
        Dim net As BN.BooleanNetwork = CreateNetwork()
        Dim trajectory As BN.StateTrajectory = net.Run(3)

        Call DebugAudit.AreEqual(4, trajectory.Count, "轨迹长度含初始状态")
        Call DebugAudit.AreEqual(True, net.StateOf("A"), "常量真节点")
        Call DebugAudit.AreEqual(False, net.StateOf("B"), "A 为真是 NOT 节点取假")

        Dim asyncNet As BN.BooleanNetwork = CreateNetwork()
        Dim asyncState As Boolean() = asyncNet.StepAsynchronous()

        Call DebugAudit.AreEqual(3, asyncState.Length, "异步步进状态长度")

        Dim binarizer As New BN.FixedThresholdBinarizer(0.5)

        Call DebugAudit.AreEqual(True, binarizer.Binarize(0.7), "固定阈值二值化高值")
        Call DebugAudit.AreEqual(False, binarizer.Binarize(0.2), "固定阈值二值化低值")
    End Sub

    Private Sub TestAttractor()
        Dim only As New BN.BooleanNode("X", New Integer() {0}, BN.BooleanTruthTable.CreateNot())
        Dim net As New BN.BooleanNetwork(New BN.BooleanNode() {only}, New Boolean() {False})
        Dim attractor As BN.AttractorResult = net.RunToAttractor(100)

        Call DebugAudit.Require(attractor IsNot Nothing, "NOT 自环应检测到极限环")
        Call DebugAudit.AreEqual(2, attractor.Period, "NOT 自环周期为 2")
        Call DebugAudit.AreEqual(False, attractor.IsFixedPoint, "NOT 自环不是不动点")
    End Sub

    Private Sub TestPbnReproducibility()
        Dim a As New BN.BooleanNode("A", New Integer() {}, BN.BooleanTruthTable.FromValues(0, True))
        Dim b As BN.BooleanNode = BN.ProbabilisticBooleanNetwork.CreateNode(
            "B", New Integer() {0},
            BN.ProbabilisticBooleanNetwork.CreateRule(BN.BooleanTruthTable.CreateNot(), 0.7),
            BN.ProbabilisticBooleanNetwork.CreateRule(BN.BooleanTruthTable.CreateAnd(1), 0.3))
        Dim nodes As BN.BooleanNode() = {a, b}

        Dim first As New BN.ProbabilisticBooleanNetwork(nodes, New Boolean() {False, False}, 42)
        Dim second As New BN.ProbabilisticBooleanNetwork(nodes, New Boolean() {False, False}, 42)
        Dim firstTrajectory As BN.StateTrajectory = first.Run(20)
        Dim secondTrajectory As BN.StateTrajectory = second.Run(20)
        Dim same As Boolean = True

        For i As Integer = 0 To firstTrajectory.Count - 1
            If BN.StateSpace.Key(firstTrajectory(i)) <> BN.StateSpace.Key(secondTrajectory(i)) Then
                same = False
                Exit For
            End If
        Next

        Call DebugAudit.AreEqual(True, same, "相同种子采样轨迹一致")
        Call DebugAudit.AreEqual(True, first.SampleCount > 0, "PBN 实际发生概率采样")
    End Sub

    Private Sub TestInference()
        Dim names As String() = {"A", "B"}
        Dim data(5, 1) As Boolean

        data(0, 0) = False : data(0, 1) = False
        data(1, 0) = False : data(1, 1) = False
        data(2, 0) = True : data(2, 1) = True
        data(3, 0) = True : data(3, 1) = True
        data(4, 0) = False : data(4, 1) = False
        data(5, 0) = True : data(5, 1) = True

        Dim expression As New BN.BooleanExpressionData(names, data)
        Dim fitted As BN.InferenceResult = New BN.BooleanNetworkInference(1).Fit(expression)

        Call DebugAudit.AreEqual(1, fitted.Parents(1).Length, "B 的父节点数为 1")
        Call DebugAudit.AreEqual(0, fitted.Parents(1)(0), "B 的父节点是 A")
        Call DebugAudit.AreEqual(6, CInt(fitted.Scores(1)), "B 的规则完全一致")

        Dim pbn As BN.PbnInferenceResult = New BN.PbnInference(1, 0.0).Fit(expression, 7)

        Call DebugAudit.AreEqual(2, pbn.Nodes.Length, "PBN 推断节点数")
        Call DebugAudit.AreEqual(True, pbn.Probabilities(1).Length >= 1, "PBN 每个基因至少一个函数")

        Dim sum As Double = 0.0

        For Each p As Double In pbn.Probabilities(1)
            sum += p
        Next

        Call DebugAudit.AreEqual(True, Math.Abs(sum - 1.0) < 1e-9, "PBN 概率和为 1")
    End Sub

    Private Sub TestJson()
        Dim net As BN.BooleanNetwork = CreateNetwork()
        Dim json As String = BN.BooleanNetworkDto.FromModel(net).ToJson()
        Dim restored As BN.BooleanNetworkBase = BN.BooleanNetworkDto.FromJson(json)

        Call DebugAudit.AreEqual(3, restored.Count, "BN JSON 往返节点数")

        For i As Integer = 1 To 5
            Call net.StepSynchronous()
            Call restored.StepSynchronous()
        Next

        Call DebugAudit.AreEqual(True, BN.StateSpace.Key(net.State) = BN.StateSpace.Key(restored.State),
                                 "BN JSON 往返后状态轨迹一致")
    End Sub
End Module

Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports FCM = Microsoft.VisualBasic.DataMining.FuzzyLogic.FuzzyCognitiveMaps

''' <summary>FCM 自检：激活函数、迭代收敛、权重学习与 JSON 往返。</summary>
Public Module FuzzyCognitiveMapsAudit

    Public Function Run() As Integer
        Call DebugAudit.Mark("FCM 自检开始")

        Call TestActivations()
        Call TestPropagation()
        Call TestInference()
        Call TestJson()

        Dim failures As Integer = DebugAudit.FailureCount

        Call DebugAudit.Mark("FCM 自检结束", failures)

        Return failures
    End Function

    Private Sub TestActivations()
        Dim sigmoid As New FCM.SigmoidActivation(1.0)

        Call DebugAudit.AreEqual(0.5, sigmoid.Evaluate(0.0), "Sigmoid(0)=0.5")
        Call DebugAudit.AreEqual(True, sigmoid.Evaluate(10.0) > 0.99, "Sigmoid 饱和上界")
        Call DebugAudit.AreEqual(True, sigmoid.Evaluate(-10.0) < 0.01, "Sigmoid 饱和下界")

        Dim binary As New FCM.BinaryActivation(0.5)

        Call DebugAudit.AreEqual(1.0, binary.Evaluate(0.6), "二值激活高值")
        Call DebugAudit.AreEqual(0.0, binary.Evaluate(0.4), "二值激活低值")

        Dim clamped As New FCM.ClampedLinearActivation(-1.0, 1.0)

        Call DebugAudit.AreEqual(-1.0, clamped.Evaluate(-5.0), "裁剪下界")
        Call DebugAudit.AreEqual(1.0, clamped.Evaluate(5.0), "裁剪上界")
    End Sub

    Private Function CreateMap() As FCM.FuzzyCognitiveMap
        Dim concepts As FCM.FcmConcept() = {
            New FCM.FcmConcept("GeneA", 1.0),
            New FCM.FcmConcept("GeneB", 0.0),
            New FCM.FcmConcept("GeneC", 0.0)
        }
        Dim weights(2, 2) As Double

        weights(0, 1) = 0.8
        weights(1, 2) = -0.6
        weights(2, 0) = 0.3

        Return New FCM.FuzzyCognitiveMap(concepts, weights, New FCM.ClampedLinearActivation(-1.0, 1.0))
    End Function

    Private Sub TestPropagation()
        Dim map As FCM.FuzzyCognitiveMap = CreateMap()
        Dim history As List(Of Double()) = map.Run(5)

        Call DebugAudit.AreEqual(6, history.Count, "FCM 轨迹含初始状态")
        Call DebugAudit.AreEqual(True, history(1)(1) > 0.0, "正向因果传播")
        Call DebugAudit.AreEqual(True, history(2)(2) < 0.0, "负向因果抑制")

        Dim convergedMap As FCM.FuzzyCognitiveMap = CreateMap()
        Dim steps As Integer = convergedMap.RunToConvergence(1e-8, 500)

        Call DebugAudit.AreEqual(True, steps >= 0, "FCM 收敛或达到上限")
        Call DebugAudit.AreEqual(0.8, convergedMap.GetWeight(0, 1), "权重读取")
    End Sub

    Private Sub TestInference()
        ' 构造精确满足 c(t+1) = W c(t) 的序列，W(0,1)=2、W(1,0)=1、对角为 0
        Dim sequence As New List(Of Double())

        sequence.Add(New Double() {1.0, 0.0})
        sequence.Add(New Double() {0.0, 2.0})
        sequence.Add(New Double() {2.0, 0.0})
        sequence.Add(New Double() {0.0, 4.0})
        sequence.Add(New Double() {4.0, 0.0})
        sequence.Add(New Double() {0.0, 8.0})

        Dim hebbian As Double(,) = New FCM.FcmInference().FitHebbian(sequence, 2, 0.1)

        Call DebugAudit.AreEqual(2, hebbian.GetLength(0), "Hebbian 权重矩阵维度 0")
        Call DebugAudit.AreEqual(2, hebbian.GetLength(1), "Hebbian 权重矩阵维度 1")

        Dim leastSquares As Double(,) = New FCM.FcmInference().FitLeastSquares(sequence, 2)

        Call DebugAudit.AreEqual(True, Math.Abs(leastSquares(0, 1) - 2.0) < 0.1,
                                 "最小二乘恢复 w(0,1)=2")
        Call DebugAudit.AreEqual(True, Math.Abs(leastSquares(1, 0) - 1.0) < 0.1,
                                 "最小二乘恢复 w(1,0)=1")
    End Sub

    Private Sub TestJson()
        Dim map As FCM.FuzzyCognitiveMap = CreateMap()
        Dim json As String = FCM.FcmDto.FromModel(map).ToJson()
        Dim restored As FCM.FuzzyCognitiveMap = FCM.FcmDto.FromJson(json)
        Dim a As Double() = map.Run(6).Item(6)
        Dim b As Double() = restored.Run(6).Item(6)

        Call DebugAudit.AreEqual(3, restored.Count, "FCM JSON 往返概念数")

        Dim worst As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            worst = Math.Max(worst, Math.Abs(a(i) - b(i)))
        Next

        Call DebugAudit.AreEqual(True, worst < 1e-9, $"FCM JSON 往返轨迹一致 (max diff {worst:E2})")
    End Sub
End Module

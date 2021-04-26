Imports Microsoft.VisualBasic.My.JavaScript

Public Class viterbiFactory : Inherits HMMChainAlgorithm

    Sub New(HMM, obSequence)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function initViterbi() As List(Of Double)
        Dim initTrellis As New List(Of Double)
        Dim obIndex = HMM.observables.IndexOf(obSequence(0))
        Dim obEmission = HMM.emissionMatrix(obIndex)

        Call HMM.initialProb.ForEach(Sub(p, i)
                                         initTrellis.Add(p * obEmission(i))
                                     End Sub)
        Return initTrellis
    End Function

    Public Function recViterbi(prevTrellis() As Double, obIndex As Integer, psiArrays() As List(Of Integer), trellisSequence As List(Of Double())) As TrellisPsi
        If (obIndex = obSequence.Length) Then
            Return New TrellisPsi With {.psiArrays = psiArrays, .trellisSequence = trellisSequence.ToArray}
        End If

        Dim nextTrellis As Double() = HMM.states.map(Function(state, stateIndex)
                                                         Dim trellisArr As Double() = prevTrellis.map(Function(prob, i)
                                                                                                          Dim trans = HMM.transMatrix(i)(stateIndex)
                                                                                                          Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(obIndex)))(stateIndex)
                                                                                                          Return prob * trans * emiss
                                                                                                      End Function)
                                                         Dim maximized = trellisArr.Max()
                                                         psiArrays(stateIndex).Add(trellisArr.IndexOf(maximized))
                                                         Return maximized
                                                     End Function)
        trellisSequence.Add(nextTrellis)

        Return recViterbi(nextTrellis, obIndex + 1, psiArrays, trellisSequence)
    End Function

    Public Function termViterbi(recTrellisPsi As TrellisPsi) As termViterbi
        Dim finalTrellis As Double() = recTrellisPsi.trellisSequence(recTrellisPsi.trellisSequence.Length - 1)
        Dim maximizedProbability = finalTrellis.Max()

        recTrellisPsi.psiArrays.ForEach(Sub(psiArr, i)
                                            psiArr.Add(finalTrellis.IndexOf(maximizedProbability))
                                        End Sub)

        Return New termViterbi With {
            .maximizedProbability = maximizedProbability,
            .psiArrays = recTrellisPsi.psiArrays
        }
    End Function
    Public Function backViterbi(psiArrays As Integer()()) As Object()
        Dim backtraceObj = obSequence.reduce(Function(acc As List(Of Psi), currS As Object, i As Integer)
                                                 If (acc.Count = 0) Then
                                                     Dim finalPsiIndex = psiArrays(0).Length - 1
                                                     Dim finalPsi = psiArrays(0)(finalPsiIndex)
                                                     acc.Add(New Psi With {.psi = finalPsi, .index = finalPsiIndex})
                                                     Return acc
                                                 End If
                                                 Dim prevPsi = acc(acc.Count - 1)
                                                 Dim psi = psiArrays(prevPsi.psi)(prevPsi.index - 1)
                                                 acc.Add(New Psi With {.psi = psi, .index = prevPsi.index - 1})
                                                 Return acc
                                             End Function, New List(Of Psi))
        Return backtraceObj.AsEnumerable.Reverse().map(Function(e) HMM.states(e.psi))
    End Function
End Class

Public Class Psi

    Public Property psi As Integer
    Public Property index As Integer

End Class

Public Class TrellisPsi

    Public Property trellisSequence As Double()()
    Public Property psiArrays As List(Of Integer)()

End Class

Public Class termViterbi
    Public Property maximizedProbability As Double
    Public Property psiArrays As List(Of Integer)()
End Class
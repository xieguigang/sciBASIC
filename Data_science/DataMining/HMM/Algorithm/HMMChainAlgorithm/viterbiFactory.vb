Public Class viterbiFactory : Inherits HMMChainAlgorithm

    Sub New(HMM, obSequence)
        Call MyBase.New(HMM, obSequence)
    End Sub

    Public Function initViterbi()
        Dim initTrellis = {}
        Dim obIndex = HMM.observables.indexOf(obSequence(0))
        Dim obEmission = HMM.emissionMatrix(obIndex)
        HMM.initialProb.forEach(Sub(p, i)
                                    initTrellis.push(p * obEmission(i))
                                End Sub)
        Return initTrellis
    End Function
    Public Function recViterbi(prevTrellis, obIndex, psiArrays, trellisSequence)
        If (obIndex = obSequence.length) Then Return {psiArrays, trellisSequence}
        Dim nextTrellis = HMM.states.map(Function(state, stateIndex)
                                             Dim trellisArr = prevTrellis.map(Function(prob, i)
                                                                                  Dim trans = HMM.transMatrix(i)(stateIndex)
                                                                                  Dim emiss = HMM.emissionMatrix(HMM.observables.indexOf(obSequence(obIndex)))(stateIndex)
                                                                                  Return prob * trans * emiss
                                                                              End Function)
                                             Dim maximized = Math.Max(trellisArr)
                                             psiArrays(stateIndex).push(trellisArr.indexOf(maximized))
                                             Return maximized
                                         End Function, {})
        trellisSequence.push(nextTrellis)
        Return recViterbi(nextTrellis, obIndex + 1, psiArrays, trellisSequence)
    End Function
    Public Function termViterbi(recTrellisPsi)
        Dim finalTrellis = recTrellisPsi.trellisSequence(recTrellisPsi.trellisSequence.length - 1)
        Dim maximizedProbability = Math.Max(finalTrellis)
        recTrellisPsi.psiArrays.forEach(Sub(psiArr) psiArr.push(finalTrellis.indexOf(maximizedProbability)))
        Return New With {.maximizedProbability = maximizedProbability, .psiArrays = recTrellisPsi.psiArrays}
    End Function
    Public Function backViterbi(psiArrays)
        Dim backtraceObj = obSequence.reduce(Function(acc, currS, i)
                                                 If (acc.length = 0) Then
                                                     Dim finalPsiIndex = psiArrays(0).length - 1
                                                     Dim finalPsi = psiArrays(0)(finalPsiIndex)
                                                     acc.push(New With {.psi = finalPsi, .Index = finalPsiIndex})
                                                     Return acc
                                                 End If
                                                 Dim prevPsi = acc(acc.length - 1)
                                                 Dim psi = psiArrays(prevPsi.psi)(prevPsi.index - 1)
                                                 acc.push(New With {.psi = psi, .Index = prevPsi.index - 1})
                                                 Return acc
                                             End Function, {})
        Return backtraceObj.reverse().map(Function(e) HMM.states(e.psi))
    End Function
End Class

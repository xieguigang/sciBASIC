Public Class Bayes : Inherits HMMAlgorithm
    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub
    Public Function bayesTheorem(ob, hState)
        Dim hStateIndex = HMM.states.indexOf(hState)
        Dim obIndex = HMM.observables.indexOf(ob)
        Dim emissionProb = HMM.emissionMatrix(obIndex)(hStateIndex)
        Dim initHState = HMM.initialProb(hStateIndex)
        Dim obProb = HMM.emissionMatrix(obIndex).reduce(Function(total, em, i)
                                                            total += (em() * HMM.initialProb(i))
                                                            Return total
                                                        End Function, 0)
        Dim bayesResult = (emissionProb * initHState) / obProb
        Return bayesResult
    End Function
End Class

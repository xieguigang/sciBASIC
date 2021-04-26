Imports Microsoft.VisualBasic.My.JavaScript

Public Class Bayes : Inherits HMMAlgorithm

    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub

    Public Function bayesTheorem(ob, hState) As Double
        Dim hStateIndex = HMM.states.IndexOf(hState)
        Dim obIndex As Integer = HMM.observables.indexOf(ob)
        Dim emissionProb = HMM.emissionMatrix(obIndex)(hStateIndex)
        Dim initHState = HMM.initialProb(hStateIndex)
        Dim obProb = HMM.emissionMatrix(obIndex) _
            .reduce(Function(total As Double, em As Double, i As Integer)
                        total += (em * HMM.initialProb(i))
                        Return total
                    End Function, 0.0)
        Dim bayesResult = (emissionProb * initHState) / obProb

        Return bayesResult
    End Function
End Class

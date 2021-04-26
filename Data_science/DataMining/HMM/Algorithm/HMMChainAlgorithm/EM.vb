Imports Microsoft.VisualBasic.My.JavaScript

Public Class EM : Inherits HMMChainAlgorithm

    Dim forwardObj As Alpha
    Dim backwardBetas As Double()()

    Sub New(HMM As HMM, forwardObj As Alpha, backwardBetas As Double()(), obSequence As Chain)
        Call MyBase.New(HMM, obSequence)

        Me.forwardObj = forwardObj
        Me.backwardBetas = backwardBetas
    End Sub

    Public Function initialGamma(stateI As Integer) As Double
        Return gamma(forwardObj.alphas(0)(stateI), backwardBetas(0)(stateI), forwardObj.alphaF)
    End Function

    Public Function gammaTimesInState(stateI As Integer) As Double
        Dim gammas As New List(Of Double)

        For t = 0 To obSequence.Length - 1
            gammas.Add(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
        Next

        Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
    End Function

    Public Function gammaTransFromState(stateI As Integer) As Double
        Dim gammas As New List(Of Double)

        For t = 0 To obSequence.Length - 2
            gammas.Add(gamma(forwardObj.alphas(t)(stateI), backwardBetas(t)(stateI), forwardObj.alphaF))
        Next

        Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
    End Function

    Public Function xiTransFromTo(stateI As Integer, stateJ As Integer) As Double
        Dim xis As New List(Of Double)

        For t = 0 To obSequence.Length - 2
            Dim alpha = forwardObj.alphas(t)(stateI)
            Dim trans = HMM.transMatrix(stateI)(stateJ)
            Dim emiss = HMM.emissionMatrix(HMM.observables.IndexOf(obSequence(t + 1)))(stateJ)
            Dim beta = backwardBetas(t + 1)(stateJ)

            xis.Add(xi(alpha, trans, emiss, beta, forwardObj.alphaF))
        Next

        Return xis.reduce(Function(tot, curr) tot + curr, 0.0)
    End Function

    Public Function gammaTimesInStateWithOb(stateI As Integer, obIndex As Integer) As Double
        Dim obsK = HMM.observables(obIndex)
        Dim stepsWithOb = obSequence.obSequence _
            .reduce(Function(tot, curr, i)
                        If (curr = obsK) Then
                            tot.Add(i)
                        End If

                        Return tot
                    End Function, New List(Of Integer))
        Dim gammas As New List(Of Double)

        stepsWithOb.ForEach(Sub([step])
                                gammas.Add(gamma(forwardObj.alphas([step])(stateI), backwardBetas([step])(stateI), forwardObj.alphaF))
                            End Sub)

        Return gammas.reduce(Function(tot, curr) tot + curr, 0.0)
    End Function

End Class

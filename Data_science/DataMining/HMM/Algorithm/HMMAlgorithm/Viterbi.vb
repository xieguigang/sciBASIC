Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.My.JavaScript

Public Class Viterbi : Inherits HMMAlgorithm

    Sub New(HMM)
        Call MyBase.New(HMM)
    End Sub

    Public Function viterbiAlgorithm(obSequence) As viterbiSequence
        Dim viterbi As New viterbiFactory(HMM, obSequence)
        Dim initTrellis = viterbi.initViterbi()
        ' Initialization Of psi arrays Is equal To 0, but I use null because 0 could later represent a state index
        Dim psiArrays As New PsiArray(HMM.states.map(Function(s) New List(Of Integer)))
        Dim recTrellisPsi = viterbi.recViterbi(initTrellis.ToArray, 1, psiArrays, New List(Of Double()) From {initTrellis.ToArray})
        Dim pTerm = viterbi.termViterbi(recTrellisPsi)
        Dim backtrace = viterbi.backViterbi(pTerm.psiArrays)

        Return New viterbiSequence With {
            .stateSequence = backtrace,
            .trellisSequence = recTrellisPsi.trellisSequence,
            .terminationProbability = pTerm.maximizedProbability
        }
    End Function
End Class

Public Class viterbiSequence
    Public Property trellisSequence As Double()()
    Public Property terminationProbability As Double
    Public Property stateSequence As Object()
End Class


Public Class PsiArray

    Friend ReadOnly matrix As New List(Of List(Of Integer))

    Default Public ReadOnly Property Item(i As Integer) As List(Of Integer)
        Get
            Return matrix(i)
        End Get
    End Property

    Sub New(matrix As List(Of Integer)())
        Me.matrix = matrix.ToList
    End Sub

    Public Sub Add(i As Integer, data As Integer)
        matrix(i).Add(data)
    End Sub

    Public Sub Add(data As IEnumerable(Of Integer))
        matrix.Add(data.ToList)
    End Sub

    Public Sub forEach(apply As Action(Of List(Of Integer), Integer))
        Dim i As i32 = Scan0

        For Each item As List(Of Integer) In matrix
            Call apply(item, ++i)
        Next
    End Sub
End Class
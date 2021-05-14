Imports Microsoft.VisualBasic.Language

Namespace Models

    Public Class viterbiSequence
        Public Property trellisSequence As Double()()
        Public Property terminationProbability As Double
        Public Property stateSequence As String()
    End Class


    Public Class PsiArray

        Friend ReadOnly matrix As New List(Of List(Of Integer))

        Default Public ReadOnly Property Item(i As Integer) As List(Of Integer)
            Get
                Return matrix(i)
            End Get
        End Property

        Sub New(matrix As List(Of Integer)())
            Me.matrix = matrix.AsList
        End Sub

        Public Sub Add(i As Integer, data As Integer)
            matrix(i).Add(data)
        End Sub

        Public Sub Add(data As IEnumerable(Of Integer))
            matrix.Add(data.AsList)
        End Sub

        Public Sub forEach(apply As Action(Of List(Of Integer), Integer))
            Dim i As i32 = Scan0

            For Each item As List(Of Integer) In matrix
                Call apply(item, ++i)
            Next
        End Sub
    End Class
End Namespace
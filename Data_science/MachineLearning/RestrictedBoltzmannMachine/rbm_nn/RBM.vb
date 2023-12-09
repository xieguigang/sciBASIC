Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm

    ''' <summary>
    ''' Created by kenny on 5/12/14.
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/kennycason/rbm/tree/master
    ''' </remarks>
    Public Class RBM

        Public ReadOnly Property VisibleSize As Integer
            Get
                Return Weights.rows()
            End Get
        End Property

        Public ReadOnly Property HiddenSize As Integer
            Get
                Return Weights.columns()
            End Get
        End Property

        Public Property Weights As DenseMatrix

        Public Sub New(visibleSize As Integer, hiddenSize As Integer)
            Weights = DenseMatrix.randomGaussian(visibleSize, hiddenSize)
        End Sub

        Public Sub addVisibleNodes(n As Integer)
            Dim weights = DenseMatrix.make(VisibleSize + n, HiddenSize)

            ' copy original values
            For i = 0 To weights.rows() - 1
                For j = 0 To weights.columns() - 1
                    weights.set(i, j, weights.get(i, j))
                Next
            Next
            ' randomly init new weights;
            For i = 0 To weights.rows() - 1
                For j = weights.columns() To weights.columns() - 1
                    weights.set(i, j, randf.NextGaussian() * 0.1)
                Next
            Next

            Me.Weights = weights
        End Sub

        Public Overrides Function ToString() As String
            Return "RBM{" & "visibleSize=" & VisibleSize.ToString() & ", hiddenSize=" & HiddenSize.ToString() & ", weights=" & Weights.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace

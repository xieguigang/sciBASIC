Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nn.rbm

    ''' <summary>
    ''' Created by kenny on 5/12/14.
    ''' 
    ''' </summary>
    Public Class RBM

        Private weightsField As DenseMatrix

        Public Sub New(visibleSize As Integer, hiddenSize As Integer)
            weightsField = DenseMatrix.randomGaussian(visibleSize, hiddenSize)
        End Sub

        Public Overridable ReadOnly Property VisibleSize As Integer
            Get
                Return weightsField.rows()
            End Get
        End Property

        Public Overridable ReadOnly Property HiddenSize As Integer
            Get
                Return weightsField.columns()
            End Get
        End Property

        Public Overridable Sub addVisibleNodes(n As Integer)

            Dim weights = DenseMatrix.make(VisibleSize + n, HiddenSize)
            ' copy original values
            For i = 0 To weightsField.rows() - 1
                For j = 0 To weightsField.columns() - 1
                    weights.set(i, j, weightsField.get(i, j))
                Next
            Next
            ' randomly init new weights;
            For i = 0 To weightsField.rows() - 1
                For j = weightsField.columns() To weights.columns() - 1
                    weights.set(i, j, randf.NextGaussian() * 0.1)
                Next
            Next
            weightsField = weights
        End Sub

        Public Overridable Property Weights As DenseMatrix
            Get
                Return weightsField
            End Get
            Set(value As DenseMatrix)
                weightsField = value
            End Set
        End Property


        Public Overrides Function ToString() As String
            Return "RBM{" & "visibleSize=" & VisibleSize.ToString() & ", hiddenSize=" & HiddenSize.ToString() & ", weights=" & weightsField.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace

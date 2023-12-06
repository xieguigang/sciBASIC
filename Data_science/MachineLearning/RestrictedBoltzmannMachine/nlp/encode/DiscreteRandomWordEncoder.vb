Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nlp.encode

    ''' <summary>
    ''' Created by kenny on 6/3/14.
    ''' generate a random vector for a word
    ''' </summary>
    Public Class DiscreteRandomWordEncoder
        Implements WordEncoder

        Private ReadOnly dimensions As Integer

        Public Sub New()
            Me.New(20)
        End Sub

        Public Sub New(dimensions As Integer)
            Me.dimensions = dimensions
        End Sub

        Public Overridable Function encode(word As String) As DenseMatrix Implements WordEncoder.encode
            Dim matrix = DenseMatrix.make(1, dimensions)
            For i = 0 To dimensions - 1
                matrix.set(0, i, If(randf.NextGaussian() > 0, 1.0, 0.0))
            Next
            Return matrix
        End Function

    End Class

End Namespace

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' This layer is useful when we are dealing with ReLU neurons. Why is that?
    ''' Because ReLU neurons have unbounded activations and we need LRN to normalize
    ''' that. We want to detect high frequency features with a large response. If we
    ''' normalize around the local neighborhood of the excited neuron, it becomes even
    ''' more sensitive as compared to its neighbors.
    ''' 
    ''' At the same time, it will dampen the responses that are uniformly large in any
    ''' given local neighborhood. If all the values are large, then normalizing those
    ''' values will diminish all of them. So basically we want to encourage some kind
    ''' of inhibition and boost the neurons with relatively larger activations. This
    ''' has been discussed nicely in Section 3.3 of the original paper by Krizhevsky et al.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class LocalResponseNormalizationLayer
        Implements Layer

        ' 
        ' 		 The constants k, n, alpha and beta are hyper-parameters whose
        ' 		   values are determined using a validation set; we used
        ' 		   k = 2, n = 5, alpha = 10^-4, beta = 0.75
        ' 	
        ' 		   quote from http://www.cs.toronto.edu/~fritz/absps/imagenet.pdf
        ' 		

        Private ReadOnly k As Double = 2.0
        Private ReadOnly n As Double = 5.0
        Private ReadOnly alpha As Double = 0.0001
        Private ReadOnly beta As Double = 0.75

        Private in_act, out_act, S_cache_ As DataBlock

        Public Overridable ReadOnly Property BackPropagationResult As IList(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Return New List(Of BackPropResult)()
            End Get
        End Property

        Public Sub New()
            ' checks
            If n Mod 2 = 0 Then
                VBDebugger.EchoLine("WARNING: n should be odd for LRN layer")
            End If
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db

            Dim A As DataBlock = db.cloneAndZero()
            S_cache_ = db.cloneAndZero()
            Dim n2 = std.Floor(n / 2)
            For x = 0 To db.SX - 1
                For y = 0 To db.SY - 1
                    For i = 0 To db.Depth - 1

                        Dim ai = db.getWeight(x, y, i)

                        ' normalize in a window of size n
                        Dim den = 0.0
                        For j As Integer = std.Max(0, i - n2) To std.Min(i + n2, db.Depth - 1)
                            Dim aa = db.getWeight(x, y, j)
                            den += aa * aa
                        Next
                        den *= alpha / n
                        den += k
                        S_cache_.setWeight(x, y, i, den) ' will be useful for backprop
                        den = std.Pow(den, beta)
                        A.setWeight(x, y, i, ai / den)
                    Next
                Next
            Next

            out_act = A
            Return out_act ' dummy identity function for now
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            ' evaluate gradient wrt data
            Dim V = in_act ' we need to set dw of this
            V.clearGradient()

            Dim n2 As Integer = std.Floor(n / 2)
            For x = 0 To V.SX - 1
                For y = 0 To V.SY - 1
                    For i = 0 To V.Depth - 1

                        Dim chain_grad = out_act.getGradient(x, y, i)
                        Dim S = S_cache_.getWeight(x, y, i)
                        Dim SB = std.Pow(S, beta)
                        Dim SB2 = SB * SB

                        ' normalize in a window of size n
                        For j = std.Max(0, i - n2) To std.Min(i + n2, V.Depth - 1)
                            Dim aj = V.getWeight(x, y, j)
                            Dim g = -aj * beta * std.Pow(S, beta - 1) * alpha / n * 2 * aj
                            If j = i Then
                                g += SB
                            End If
                            g /= SB2
                            g *= chain_grad
                            V.addGradient(x, y, j, g)
                        Next

                    Next
                Next
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"local_response_norm()"
        End Function
    End Class

End Namespace

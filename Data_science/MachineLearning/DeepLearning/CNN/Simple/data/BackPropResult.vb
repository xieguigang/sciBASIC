Namespace CNN.data

    ''' <summary>
    ''' When we have done a back propagation of the network we will receive a
    ''' result of weight adjustments required to learn. This result set will
    ''' contain the data used by the trainer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class BackPropResult

        Friend l1_decay_mul, l2_decay_mul As Double

        Private w As Double()
        Private dw As Double()

        Public Overridable ReadOnly Property L1DecayMul As Double
            Get
                Return l1_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property L2DecayMul As Double
            Get
                Return l2_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property Weights As Double()
            Get
                Return w
            End Get
        End Property

        Public Overridable ReadOnly Property Gradients As Double()
            Get
                Return dw
            End Get
        End Property

        Public Sub New(w As Double(), dw As Double(), l1_decay_mul As Double, l2_decay_mul As Double)
            Me.w = w
            Me.dw = dw
            Me.l1_decay_mul = l1_decay_mul
            Me.l2_decay_mul = l2_decay_mul
        End Sub

    End Class
End Namespace

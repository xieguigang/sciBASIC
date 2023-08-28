Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.MachineLearning.CNN.losslayers

Namespace CNN

    ''' <summary>
    ''' A network class holding the layers and some helper functions
    ''' for training and validation.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com) and s.chekanov 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/kalaspuffar/JavaCNN
    ''' </remarks>
    Public Class ConvolutionalNN

        Dim m_layers As Layer()

        ''' <summary>
        ''' Accumulate parameters and gradients for the entire network
        ''' </summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult)
            Get
                For Each l As Layer In m_layers
                    For Each subset In l.BackPropagationResult
                        Yield subset
                    Next
                Next
            End Get
        End Property

        ''' <summary>
        ''' This is a convenience function for returning the argmax
        ''' prediction, assuming the last layer of the net is a softmax
        ''' </summary>
        Public Overridable ReadOnly Property Prediction As Integer
            Get
                Dim S = CType(m_layers(m_layers.Length - 1), LossLayer)
                Dim p = S.OutAct.Weights
                Dim maxv = p(0)
                Dim maxi = 0
                For i = 1 To p.Length - 1
                    If p(i) > maxv Then
                        maxv = p(i)
                        maxi = i
                    End If
                Next
                Return maxi
            End Get
        End Property

        Public Sub New(layers As LayerBuilder)
            Me.m_layers = CType(layers, List(Of Layer)).ToArray
        End Sub

        ' 
        ' 		 Forward prop the network.
        ' 		 The trainer class passes is_training = true, but when this function is
        ' 		 called from outside (not from the trainer), it defaults to prediction mode
        ' 		
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim act = m_layers(0).forward(db, training)
            For i = 1 To m_layers.Length - 1
                act = m_layers(i).forward(act, training)
            Next
            Return act
        End Function

        Public Overridable Function getCostLoss(db As DataBlock, y As Integer) As Double
            Dim N = m_layers.Length
            forward(db, False)
            Return CType(m_layers(N - 1), LossLayer).backward(y)
        End Function

        ''' <summary>
        ''' Backprop: compute gradients wrt all parameters
        ''' </summary>
        Public Overridable Function backward(y As Integer) As Double
            Dim N = m_layers.Length
            Dim loss = CType(m_layers(N - 1), LossLayer).backward(y)
            For i = N - 2 To 0 Step -1 ' first layer assumed input
                m_layers(i).backward()
            Next
            Return loss
        End Function
    End Class

End Namespace

Imports Microsoft.VisualBasic.MachineLearning.ConsoleApp1.data
Imports Microsoft.VisualBasic.MachineLearning.ConsoleApp1.losslayers
Imports Microsoft.VisualBasic.MachineLearning.ConsoleApp1.layers

Namespace ConsoleApp1

    ''' <summary>
    ''' A network class holding the layers and some helper functions
    ''' for training and validation.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com) and s.chekanov 
    ''' </summary>
    Public Class JavaCNN

        Private layers As IList(Of Layer)

        Public Sub New(layers As IList(Of Layer))
            Me.layers = layers
        End Sub

        ' 
        ' 		 Forward prop the network.
        ' 		 The trainer class passes is_training = true, but when this function is
        ' 		 called from outside (not from the trainer), it defaults to prediction mode
        ' 		
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim act = layers(0).forward(db, training)
            For i = 1 To layers.Count - 1
                act = layers(i).forward(act, training)
            Next
            Return act
        End Function

        Public Overridable Function getCostLoss(db As DataBlock, y As Integer) As Double
            forward(db, False)
            Dim N = layers.Count
            Dim loss = CType(layers(N - 1), LossLayer).backward(y)
            Return loss
        End Function


        ''' <summary>
        ''' Backprop: compute gradients wrt all parameters
        ''' </summary>
        Public Overridable Function backward(y As Integer) As Double
            Dim N = layers.Count
            Dim loss = CType(layers(N - 1), LossLayer).backward(y)
            For i = N - 2 To 0 Step -1 ' first layer assumed input
                layers(i).backward()
            Next
            Return loss
        End Function

        ''' <summary>
        ''' Accumulate parameters and gradients for the entire network
        ''' </summary>
        Public Overridable ReadOnly Property BackPropagationResult As IList(Of BackPropResult)
            Get
                Dim result As IList(Of BackPropResult) = New List(Of BackPropResult)()
                For Each l In layers
                    Dim subResult = l.BackPropagationResult
                    CType(result, List(Of BackPropResult)).AddRange(subResult)
                Next
                Return result
            End Get
        End Property

        ''' <summary>
        ''' This is a convenience function for returning the argmax
        ''' prediction, assuming the last layer of the net is a softmax
        ''' </summary>
        Public Overridable ReadOnly Property Prediction As Integer
            Get
                Dim S = CType(layers(layers.Count - 1), LossLayer)
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

    End Class

End Namespace

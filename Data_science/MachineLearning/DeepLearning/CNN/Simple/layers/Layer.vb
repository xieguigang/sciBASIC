Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.layers

    ''' <summary>
    ''' A convolution neural network is built of layers that the data traverses
    ''' back and forth in order to predict what the network sees in the data.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Interface Layer
        Function forward(db As DataBlock, training As Boolean) As DataBlock
        Sub backward()
        ReadOnly Property BackPropagationResult As IList(Of BackPropResult)
    End Interface

End Namespace

Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.layers

    ''' <summary>
    ''' A convolution neural network is built of layers that the data traverses
    ''' back and forth in order to predict what the network sees in the data.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Interface Layer

        ReadOnly Property BackPropagationResult As IList(Of BackPropResult)

        Function forward(db As DataBlock, training As Boolean) As DataBlock
        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        Sub backward()

    End Interface

End Namespace

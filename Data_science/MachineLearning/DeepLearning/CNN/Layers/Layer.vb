Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.layers

    ''' <summary>
    ''' A convolution neural network is built of layers that the data traverses
    ''' back and forth in order to predict what the network sees in the data.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Interface Layer

        ''' <summary>
        ''' adjust the weight at here in the trainer module
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property BackPropagationResult As IEnumerable(Of BackPropResult)
        ReadOnly Property Type As LayerTypes

        Function forward(db As DataBlock, training As Boolean) As DataBlock
        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        Sub backward()

    End Interface

End Namespace

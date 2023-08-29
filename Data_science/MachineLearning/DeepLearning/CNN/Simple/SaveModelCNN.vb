Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers

Namespace CNN

    Public Module SaveModelCNN

        Public Sub Write(cnn As ConvolutionalNN, file As Stream)
            Using writer As New BinaryWriter(file, Encoding.ASCII)
                Call Write(cnn, writer)
                Call writer.Flush()
            End Using
        End Sub

        Private Sub Write(cnn As ConvolutionalNN, wr As BinaryWriter)
            Call wr.Write(Encoding.ASCII.GetBytes("CNN"))
            Call wr.Write(cnn.layerNum)

            '            Call wr.Write(cnn.ALPHA)
            '            Call wr.Write(cnn.LAMBDA)

            '            Call wr.Write(cnn.batchSize)

            For i As Integer = 0 To cnn.LayerNum - 1
                Call Write(layer:=cnn(i), wr)
            Next
        End Sub

        Private Sub Write(layer As Layer, wr As BinaryWriter)
            Call wr.Write(0&)
            Call wr.Write(CInt(layer.Type))

            Select Case layer.Type
                Case Convolutional.LayerTypes.Convolution : Write(layer, wr)

                Case Else
                    Throw New NotImplementedException(layer.Type.Description)
            End Select
        End Sub

        Private Sub Write(layer As ConvolutionLayer, wr As BinaryWriter)

        End Sub
    End Module
End Namespace
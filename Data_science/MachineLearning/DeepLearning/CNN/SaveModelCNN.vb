Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

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

            For i As Integer = 0 To cnn.LayerNum - 1
                Call Write(layer:=cnn(i), wr)
            Next
        End Sub

        Private Sub Write(layer As Layer, wr As BinaryWriter)
            Call wr.Write(0&)
            Call wr.Write(CInt(layer.Type))
            Call wr.Flush()

            Dim save As New ObjectOutputStream(wr)
            ' do not close/dispose the stream at here
            ' or the file stream data will be close at here
            ' so that we can not save the next layer object
            Call save.WriteObject(layer)
            Call save.Flush()
        End Sub
    End Module
End Namespace
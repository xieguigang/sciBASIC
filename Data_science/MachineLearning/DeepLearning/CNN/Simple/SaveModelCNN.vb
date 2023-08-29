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
                Case Convolutional.LayerTypes.Convolution
                Case Convolutional.LayerTypes.Input
                Case Convolutional.LayerTypes.Output
                Case Convolutional.LayerTypes.Pool
                Case Convolutional.LayerTypes.ReLU
                Case Convolutional.LayerTypes.SoftMax

            End Select
        End Sub

        '        Private Sub Write(m As Double()()()(), wr As BinaryWriter)
        '            If m.IsNullOrEmpty Then
        '                Call wr.Write(-1)
        '                Return
        '            Else
        '                Call wr.Write(m.Length)
        '            End If

        '            For Each i As Double()()() In m
        '                Call wr.Write(i.Length)
        '                For Each j As Double()() In i
        '                    Call wr.Write(j.Length)
        '                    For Each k As Double() In j
        '                        Call wr.Write(k.Length)
        '                        For Each x As Double In k
        '                            Call wr.Write(x)
        '                        Next
        '                    Next
        '                Next
        '            Next
        '        End Sub

        '        Private Sub Write(dims As Dimension, wr As BinaryWriter)
        '            If dims Is Nothing Then
        '                Call wr.Write(-1)
        '                Call wr.Write(-1)
        '            Else
        '                Call wr.Write(dims.x)
        '                Call wr.Write(dims.y)
        '            End If
        '        End Sub
    End Module
End Namespace
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Convolutional

    Public Module SaveModel

        <Extension>
        Public Function Save(model As CeNiN, file As Stream) As Boolean
            Using writer As New BinaryWriter(file, Encoding.ASCII)
                Try
                    Call model.save(writer)
                    Call writer.Flush()
                Catch ex As Exception
                    Call App.LogException(ex)
                    Return False
                End Try
            End Using

            Return True
        End Function

        <Extension>
        Private Sub save(model As CeNiN, wr As BinaryWriter)
            Call wr.Write(Encoding.ASCII.GetBytes(CeNiN.CeNiN_FILE_HEADER))
            Call wr.Write(model.layerCount)

            For Each i As Integer In model.inputSize
                Call wr.Write(i)
            Next
            For Each a As Single In model.inputLayer.avgPixel
                Call wr.Write(a)
            Next

            ' write for each layer
            For Each layer As Layer In From cv As Layer
                                       In model.layers
                                       Where Not (TypeOf cv Is Input OrElse TypeOf cv Is Output)

                Call model.save(layer, wr)
            Next

            wr.Write("EOF")
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Layer, wr As BinaryWriter)
            Call wr.Write(layer.type.Description)

            Select Case layer.type
                Case LayerTypes.Convolution : Call model.save(DirectCast(layer, Convolution), wr)
                Case LayerTypes.ReLU : Call model.save(DirectCast(layer, ReLU), wr)
                Case LayerTypes.Pool : Call model.save(DirectCast(layer, Pool), wr)
                Case LayerTypes.SoftMax : Call model.save(DirectCast(layer, SoftMax), wr)
                Case Else
                    Throw New InvalidDataException(layer.type.ToString)
            End Select

            wr.Flush()
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As SoftMax, wr As BinaryWriter)
            ' class count
            Call wr.Write(model.classCount)

            For Each name As String In model.outputLayer.m_classes
                Call wr.Write(name)
            Next
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Pool, wr As BinaryWriter)
            For Each pad As Integer In layer.pad
                Call wr.Write(CByte(pad))
            Next
            For Each p As Integer In layer.pool
                Call wr.Write(CByte(p))
            Next
            For Each s As Integer In layer.stride
                Call wr.Write(CByte(s))
            Next
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As ReLU, wr As BinaryWriter)
            ' do nothing
        End Sub

        <Extension>
        Private Sub save(model As CeNiN, layer As Convolution, wr As BinaryWriter)
            ' write pad data
            For Each pad As Integer In layer.pad
                Call wr.Write(CByte(pad))
            Next
            For Each d As Integer In layer.weights.Dimensions
                Call wr.Write(d)
            Next
            For Each stride As Integer In layer.stride
                Call wr.Write(CByte(stride))
            Next
            For Each w As Single In layer.weights.data
                Call wr.Write(w)
            Next
            For Each b As Single In layer.biases.data
                Call wr.Write(b)
            Next
        End Sub
    End Module
End Namespace
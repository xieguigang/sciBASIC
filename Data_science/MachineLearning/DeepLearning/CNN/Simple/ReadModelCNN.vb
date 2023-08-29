Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace CNN

    Public Module ReadModelCNN

        Public Function Read(file As Stream) As ConvolutionalNN
            Using rd As New BinaryReader(file)
                Dim magic As String = Encoding.ASCII.GetString(rd.ReadBytes(3))

                If magic <> "CNN" Then
                    Throw New InvalidDataException("error magic header!")
                End If

                Return Read(rd)
            End Using
        End Function

        Private Function Read(rd As BinaryReader) As ConvolutionalNN
            Dim layerNum As Integer = rd.ReadInt32
            Dim layers As New LayerBuilder(initialized:=True)

            For i As Integer = 0 To layerNum - 1
                Call layers.add(ReadLayer(rd))
            Next

            Return New ConvolutionalNN(layers)
        End Function

        Private Function ReadLayer(rd As BinaryReader) As Layer
            If 0 <> rd.ReadInt64() Then
                Throw New InvalidDataException("invalid file format!")
            End If

            ' verify the stream parser by use this flag data
            Dim type As LayerTypes = CType(rd.ReadInt32, LayerTypes)
            Dim layer As Layer = DirectCast(New ObjectInputStream(rd).ReadObject, Layer)

            If type <> layer.type Then
                Throw New InvalidDataException("The CNN layer type mis-matched!")
            Else
                Return layer
            End If
        End Function
    End Module
End Namespace
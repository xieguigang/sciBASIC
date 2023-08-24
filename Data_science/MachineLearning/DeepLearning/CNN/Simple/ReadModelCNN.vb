Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN

    Public Module ReadModelCNN

        Public Function Read(file As Stream) As CNN
            Using rd As New BinaryReader(file)
                Dim magic As String = Encoding.ASCII.GetString(rd.ReadBytes(3))

                If magic <> "CNN" Then
                    Throw New InvalidDataException("error magic header!")
                End If

                Return Read(rd)
            End Using
        End Function

        Private Function Read(rd As BinaryReader) As CNN
            Dim layerNum As Integer = rd.ReadInt32
            Dim alpha As Double = rd.ReadDouble
            Dim lambda As Double = rd.ReadDouble
            Dim batchSize As Integer = rd.ReadInt32
            Dim layers As New LayerBuilder

            For i As Integer = 0 To layerNum - 1
                Call layers.add(ReadLayer(rd))
            Next

            Return New CNN(layers, batchSize) With {
                .ALPHA = alpha,
                .LAMBDA = lambda
            }
        End Function

        Private Function ReadLayer(rd As BinaryReader) As Layer
            If 0 <> rd.ReadInt64() Then
                Throw New InvalidDataException("invalid file format!")
            End If

            Dim type As LayerTypes = CType(rd.ReadInt32, LayerTypes)
            Dim outMapNum As Integer = rd.ReadInt32
            Dim classNum As Integer = rd.ReadInt32
            Dim mapSize As Dimension = readDims(rd)
            Dim kernelSize As Dimension = readDims(rd)
            Dim scaleSize As Dimension = readDims(rd)
            Dim blen As Integer = rd.ReadInt32
            Dim bias As Double() = Nothing

            If blen <> -1 Then
                bias = New Double(blen - 1) {}

                For i As Integer = 0 To bias.Length - 1
                    bias(i) = rd.ReadDouble
                Next
            End If

            Dim kernel = readMatrix(rd).ToArray
            Dim outmaps = readMatrix(rd).ToArray
            Dim errors = readMatrix(rd).ToArray

            Return New Layer(mapSize, kernelSize, scaleSize, type, outMapNum, classNum) With {
                .bias = bias,
                .m_errors = errors,
                .m_kernel = kernel,
                .m_outmaps = outmaps
            }
        End Function

        Private Iterator Function readMatrix(rd As BinaryReader) As IEnumerable(Of Double()()())
            Dim lm As Integer = rd.ReadInt32

            If lm = -1 Then
                Return
            End If

            For i As Integer = 0 To lm - 1
                Dim li As Integer = rd.ReadInt32
                Dim ri As Double()()() = New Double(li - 1)()() {}

                For j As Integer = 0 To ri.Length - 1
                    Dim lj As Integer = rd.ReadInt32
                    Dim rj As Double()() = New Double(lj - 1)() {}

                    For k As Integer = 0 To rj.Length - 1
                        Dim lk As Integer = rd.ReadInt32
                        Dim rk As Double() = New Double(lk - 1) {}

                        For n As Integer = 0 To rk.Length - 1
                            rk(n) = rd.ReadDouble
                        Next

                        rj(k) = rk
                    Next

                    ri(j) = rj
                Next

                Yield ri
            Next
        End Function

        Private Function readDims(rd As BinaryReader) As Dimension
            Dim x = rd.ReadInt32
            Dim y = rd.ReadInt32

            If x = -1 AndAlso y = -1 Then
                Return Nothing
            Else
                Return New Dimension(x, y)
            End If
        End Function
    End Module
End Namespace
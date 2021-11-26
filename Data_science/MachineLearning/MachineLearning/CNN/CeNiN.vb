Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Convolutional

    ''' <summary>
    ''' CeNiN (means "fetus" in Turkish) is a minimal implementation of feed-forward phase of deep Convolutional Neural Networks
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/atasoyhus/CeNiN
    ''' </remarks>
    Public Class CeNiN

        Const CeNiN_FILE_HEADER As String = "CeNiN NEURAL NETWORK FILE"

        Public layerCount As Integer
        Public classCount As Integer
        Public totalWeightCount As Integer
        Public totalBiasCount As Integer
        Public layers As Layer()
        Public inputLayer As Input
        Public outputLayer As Output

        ''' <summary>
        ''' read file and construct a CNN model
        ''' </summary>
        ''' <param name="path"></param>
        Public Sub New(path As String)
            Dim f As FileStream = Nothing
            Dim br As BinaryReader = Nothing

            Try
                f = New FileStream(path, FileMode.Open)
                br = New BinaryReader(f, Encoding.ASCII, False)
                Dim c = br.ReadChars(25)

                If Not (New String(c)).Equals(CeNiN_FILE_HEADER) Then
                    Throw New Exception("Invalid file header!")
                End If

                layerCount = br.ReadInt32()
                Dim inputSize = New Integer(2) {}

                For i = 0 To 3 - 1
                    inputSize(i) = br.ReadInt32()
                Next

                inputLayer = New Input(inputSize)

                For i = 0 To 3 - 1
                    inputLayer.avgPixel(i) = br.ReadSingle()
                Next

                inputLayer.setOutputDims()
                Dim layerChain As Layer = inputLayer
                Dim currentLayer As Layer = inputLayer
                totalWeightCount = 0
                totalBiasCount = 0
                Dim layerList As List(Of Layer) = New List(Of Layer)()
                layerList.Add(currentLayer)
                Dim endOfFile = False

                While Not endOfFile
                    Dim layerT As String = br.ReadString()

                    If layerT.Equals("conv") Then
                        Dim pad = New Integer(3) {}

                        For i = 0 To 4 - 1
                            pad(i) = br.ReadByte()
                        Next

                        Dim inputTensorDims = currentLayer.outputDims
                        Dim cLayer As Convolution = New Convolution(inputTensorDims, pad)
                        Dim dims = New Integer(3) {}

                        For i = 0 To 4 - 1
                            dims(i) = br.ReadInt32()
                        Next

                        For i = 0 To 2 - 1
                            cLayer.stride(i) = br.ReadByte()
                        Next

                        cLayer.weights = New Tensor(dims)

                        For i = 0 To cLayer.weights.TotalLength - 1
                            cLayer.weights.data(i) = br.ReadSingle()
                        Next

                        totalWeightCount += cLayer.weights.TotalLength
                        cLayer.biases = New Tensor(New Integer() {dims(3)})

                        For i = 0 To cLayer.biases.TotalLength - 1
                            cLayer.biases.data(i) = br.ReadSingle()
                        Next

                        totalBiasCount += cLayer.biases.TotalLength
                        cLayer.setOutputDims()
                        currentLayer = cLayer
                    ElseIf layerT.Equals("relu") Then
                        Dim rLayer As ReLU = New ReLU(currentLayer.outputDims)
                        rLayer.setOutputDims()
                        currentLayer = rLayer
                    ElseIf layerT.Equals("pool") Then
                        Dim pad = New Integer(3) {}

                        For i = 0 To 4 - 1
                            pad(i) = br.ReadByte()
                        Next

                        Dim pLayer As Pool = New Pool(currentLayer.outputDims, pad)

                        For i = 0 To 2 - 1
                            pLayer.pool(i) = br.ReadByte()
                        Next

                        For i = 0 To 2 - 1
                            pLayer.stride(i) = br.ReadByte()
                        Next

                        pLayer.setOutputDims()
                        currentLayer = pLayer
                    ElseIf layerT.Equals("softmax") Then
                        classCount = br.ReadInt32()
                        Dim classes = New String(classCount - 1) {}

                        For i = 0 To classCount - 1
                            classes(i) = br.ReadString()
                        Next

                        Dim smLayer As SoftMax = New SoftMax(currentLayer.outputDims)
                        currentLayer.appendNext(smLayer)
                        outputLayer = New Output(smLayer.InputTensorDims, classes)
                        smLayer.appendNext(outputLayer)
                        layerList.Add(smLayer)
                        layerList.Add(outputLayer)
                        Continue While
                    ElseIf layerT.Equals("EOF") Then
                        endOfFile = True
                        Continue While
                    Else
                        Throw New Exception("The following layer is not implemented: " & layerT)
                    End If

                    layerList.Add(currentLayer)
                    layerChain.appendNext(currentLayer)
                    layerChain = layerChain.nextLayer
                End While

                layers = layerList.ToArray()
            Catch e As Exception
            Finally
                If br IsNot Nothing Then br.Close()
                If f IsNot Nothing Then f.Close()
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Return layerCount & "+2 layers, " _
                & totalWeightCount & " weights and " _
                & totalBiasCount & " biases were loaded"
        End Function
    End Class
End Namespace

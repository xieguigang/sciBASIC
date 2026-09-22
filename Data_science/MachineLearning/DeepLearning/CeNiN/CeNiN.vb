#Region "Microsoft.VisualBasic::890eb49fb980cd6750d5ff15d7a232ff, Data_science\MachineLearning\DeepLearning\CeNiN\CeNiN.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 238
    '    Code Lines: 148 (62.18%)
    ' Comment Lines: 41 (17.23%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 49 (20.59%)
    '     File Size: 8.84 KB


    '     Class CeNiN
    ' 
    '         Properties: inputSize
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: loadConvolutionLayer, (+2 Overloads) LoadFile, loadModel, loadPoolLayer, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow

Namespace Convolutional

    ''' <summary>
    ''' CeNiN (means "fetus" in Turkish) is a minimal implementation of feed-forward 
    ''' phase of deep Convolutional Neural Networks
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/atasoyhus/CeNiN
    ''' </remarks>
    Public Class CeNiN

        Friend Const CeNiN_FILE_HEADER As String = "CeNiN NEURAL NETWORK FILE"

        ''' <summary>Number of layers stored in the model file, excluding the implicit input and output layers.</summary>
        Public layerCount As Integer
        ''' <summary>Number of output classes read from the trailing softmax layer.</summary>
        Public classCount As Integer
        ''' <summary>Total number of convolution weights loaded from the model file.</summary>
        Public totalWeightCount As Integer
        ''' <summary>Total number of convolution bias values loaded from the model file.</summary>
        Public totalBiasCount As Integer
        ''' <summary>All loaded layers, ordered from the input layer through to the output layer.</summary>
        Public layers As Layer()
        ''' <summary>The input layer of the loaded network.</summary>
        Public inputLayer As Input
        ''' <summary>The output layer, exposing the class labels and their predicted probabilities.</summary>
        Public outputLayer As Output

        ''' <summary>
        ''' Gets the spatial size <c>[height, width, depth]</c> declared by the network's input layer.
        ''' </summary>
        Public ReadOnly Property inputSize As Integer()
            Get
                Return inputLayer.inputSize
            End Get
        End Property

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Reads a CeNiN model file and constructs the corresponding network.
        ''' </summary>
        ''' <param name="path">
        ''' Path of the binary model file. The file must start with the <c>CeNiN NEURAL NETWORK FILE</c>
        ''' header followed by the serialized layer chain.
        ''' </param>
        Public Sub New(path As String)
            Using f As Stream = path.Open(FileMode.Open, doClear:=False, [readOnly]:=True),
                br As New BinaryReader(f, Encoding.ASCII, False)

                Try
                    Call loadModel(br)
                Catch ex As Exception
                    Call App.LogException(ex)
                End Try
            End Using
        End Sub

        Private Function loadConvolutionLayer(currentLayer As Layer, br As BinaryReader) As Layer
            Dim pad = New Integer(3) {}

            For i = 0 To 4 - 1
                pad(i) = br.ReadByte()
            Next

            Dim inputTensorDims = currentLayer.outputDims
            Dim cLayer As New Convolution(inputTensorDims, pad)
            Dim dims = New Integer(3) {}

            For i = 0 To 4 - 1
                dims(i) = br.ReadInt32()
            Next

            For i = 0 To 2 - 1
                cLayer.stride(i) = br.ReadByte()
            Next

            cLayer.weights = New Tensor(dims)

            For i = 0 To cLayer.weights.TotalLength - 1
                cLayer.weights.Data(i) = br.ReadSingle()
            Next

            totalWeightCount += cLayer.weights.TotalLength
            cLayer.biases = New Tensor(New Integer() {dims(3)})

            For i = 0 To cLayer.biases.TotalLength - 1
                cLayer.biases.Data(i) = br.ReadSingle()
            Next

            totalBiasCount += cLayer.biases.TotalLength
            cLayer.setOutputDims()

            Return cLayer
        End Function

        Private Function loadPoolLayer(currentLayer As Layer, br As BinaryReader) As Layer
            Dim pad = New Integer(3) {}

            For i = 0 To 4 - 1
                pad(i) = br.ReadByte()
            Next

            Dim pLayer As New Pool(currentLayer.outputDims, pad)

            For i = 0 To 2 - 1
                pLayer.pool(i) = br.ReadByte()
            Next

            For i = 0 To 2 - 1
                pLayer.stride(i) = br.ReadByte()
            Next

            pLayer.setOutputDims()

            Return pLayer
        End Function

        Private Function loadModel(br As BinaryReader) As CeNiN
            Dim c = br.ReadChars(25)

            If New String(c) <> CeNiN_FILE_HEADER Then
                Throw New Exception("Invalid file header!")
            Else
                layerCount = br.ReadInt32()
            End If

            Dim inputSize = New Integer(2) {}

            For i As Integer = 0 To 3 - 1
                inputSize(i) = br.ReadInt32()
            Next

            inputLayer = New Input(inputSize)

            For i As Integer = 0 To 3 - 1
                inputLayer.avgPixel(i) = br.ReadSingle()
            Next

            inputLayer.setOutputDims()

            Dim layerChain As Layer = inputLayer
            Dim currentLayer As Layer = inputLayer

            totalWeightCount = 0
            totalBiasCount = 0

            Dim layerList As New List(Of Layer)() From {currentLayer}
            Dim endOfFile = False

            While Not endOfFile
                Dim layerT As String = br.ReadString()

                If layerT = "conv" Then
                    currentLayer = loadConvolutionLayer(currentLayer, br)
                ElseIf layerT = "relu" Then
                    Dim rLayer As ReLU = New ReLU(currentLayer.outputDims)
                    rLayer.setOutputDims()
                    currentLayer = rLayer
                ElseIf layerT = "pool" Then
                    currentLayer = loadPoolLayer(currentLayer, br)
                ElseIf layerT = "softmax" Then
                    Dim classes = New String(br.ReadInt32() - 1) {}

                    classCount = classes.Length

                    For i As Integer = 0 To classCount - 1
                        classes(i) = br.ReadString()
                    Next

                    Dim smLayer As New SoftMax(currentLayer.outputDims)

                    currentLayer.appendNext(smLayer)
                    outputLayer = New Output(smLayer.inputTensorDims, classes)
                    smLayer.appendNext(outputLayer)
                    layerList.Add(smLayer)
                    layerList.Add(outputLayer)

                    Continue While
                ElseIf layerT = "EOF" Then
                    Exit While
                Else
                    Throw New Exception("The following layer is not implemented: " & layerT)
                End If

                layerList.Add(currentLayer)
                layerChain.appendNext(currentLayer)
                layerChain = layerChain.nextLayer
            End While

            layers = layerList.ToArray()

            Return Me
        End Function

        ''' <summary>
        ''' Loads a CeNiN model from a binary stream positioned at the beginning of the model data.
        ''' </summary>
        ''' <param name="file">The stream that contains a serialized CeNiN model.</param>
        ''' <returns>The deserialized <see cref="CeNiN"/> model.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadFile(file As Stream) As CeNiN
            Using br = New BinaryReader(file, Encoding.ASCII, False)
                Return New CeNiN().loadModel(br)
            End Using
        End Function

        ''' <summary>
        ''' Loads a CeNiN model from an already opened binary reader.
        ''' </summary>
        ''' <param name="br">The reader positioned at the beginning of the serialized model data.</param>
        ''' <returns>The deserialized <see cref="CeNiN"/> model.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadFile(br As BinaryReader) As CeNiN
            Return New CeNiN().loadModel(br)
        End Function

        ''' <summary>
        ''' Returns a short human readable summary of the loaded network.
        ''' </summary>
        ''' <returns>
        ''' A multi-line text that reports the number of layers, weights and biases, followed by the
        ''' description of the output layer.
        ''' </returns>
        Public Overrides Function ToString() As String
            Return layerCount & "+2 layers, " _
                & totalWeightCount & " weights and " _
                & totalBiasCount & " biases were loaded" & vbCrLf & vbCrLf _
                & outputLayer.ToString
        End Function
    End Class
End Namespace

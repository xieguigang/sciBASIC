#Region "Microsoft.VisualBasic::5e957d215ed83b17cea435b04555b244, Data_science\MachineLearning\DeepLearning\CeNiN\CeNiN.vb"

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

    '   Total Lines: 208
    '    Code Lines: 148 (71.15%)
    ' Comment Lines: 11 (5.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 49 (23.56%)
    '     File Size: 6.83 KB


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
Imports Microsoft.VisualBasic.Math.LinearAlgebra

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

        Public layerCount As Integer
        Public classCount As Integer
        Public totalWeightCount As Integer
        Public totalBiasCount As Integer
        Public layers As Layer()
        Public inputLayer As Input
        Public outputLayer As Output

        Public ReadOnly Property inputSize As Integer()
            Get
                Return inputLayer.inputSize
            End Get
        End Property

        Private Sub New()
        End Sub

        ''' <summary>
        ''' read file and construct a CNN model
        ''' </summary>
        ''' <param name="path"></param>
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
                cLayer.weights.data(i) = br.ReadSingle()
            Next

            totalWeightCount += cLayer.weights.TotalLength
            cLayer.biases = New Tensor(New Integer() {dims(3)})

            For i = 0 To cLayer.biases.TotalLength - 1
                cLayer.biases.data(i) = br.ReadSingle()
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadFile(file As Stream) As CeNiN
            Using br = New BinaryReader(file, Encoding.ASCII, False)
                Return New CeNiN().loadModel(br)
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadFile(br As BinaryReader) As CeNiN
            Return New CeNiN().loadModel(br)
        End Function

        Public Overrides Function ToString() As String
            Return layerCount & "+2 layers, " _
                & totalWeightCount & " weights and " _
                & totalBiasCount & " biases were loaded" & vbCrLf & vbCrLf _
                & outputLayer.ToString
        End Function
    End Class
End Namespace

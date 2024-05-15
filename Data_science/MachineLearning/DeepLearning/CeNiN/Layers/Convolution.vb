#Region "Microsoft.VisualBasic::de1dbf86023da46a7ce02f88e63efc70, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Convolution.vb"

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

    '   Total Lines: 150
    '    Code Lines: 116
    ' Comment Lines: 1
    '   Blank Lines: 33
    '     File Size: 5.37 KB


    '     Class Convolution
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: layerFeedNext
    ' 
    '         Sub: setOutputDims
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

Namespace Convolutional

    Public Class Convolution : Inherits Layer

        Public stride As Integer()
        Public weights As Tensor
        Public biases As Tensor

        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Convolution
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            Call MyBase.New(inputTensorDims, pad)

            stride = New Integer(1) {}
        End Sub

        Public Overloads Sub setOutputDims()
            Dim newHeight = CInt(stdNum.Floor((inputTensorDims(0) - weights.Dimensions(0)) / stride(0))) + 1
            Dim newWidth = CInt(stdNum.Floor((inputTensorDims(1) - weights.Dimensions(1)) / stride(1))) + 1

            outputDims = New Integer() {newHeight, newWidth, weights.Dimensions(3)}
        End Sub

        Protected Overrides Function layerFeedNext() As Layer
            Dim inputHeight = inputTensorDims(0)
            Dim inputWidth = inputTensorDims(1)
            Dim filterHeight = weights.Dimensions(0)
            Dim filterWidth = weights.Dimensions(1)
            Dim channelCount = weights.Dimensions(2)
            Dim filterCount = weights.Dimensions(3)
            Dim mCountH = inputHeight - filterHeight + 1
            Dim mCountW = inputWidth - filterWidth + 1
            Dim possibleH As New Tensor(New Integer() {outputDims(0), 1})
            Dim j As i32 = 0
            Dim i = 0

            While i < mCountH
                possibleH.data(++j) = i
                i += stride(0)
            End While

            Dim possibleW As New Tensor(New Integer() {1, outputDims(1)})

            j = 0
            i = 0

            While i < mCountW
                possibleW.data(++j) = i
                i += stride(1)
            End While

            Dim startingIndexes = possibleW + possibleH * inputWidth

            possibleH.Dispose()
            possibleW.Dispose()
            possibleH = New Tensor(New Integer() {filterHeight, 1})

            For i = 0 To filterHeight - 1
                possibleH.data(i) = i
            Next

            possibleW = New Tensor(New Integer() {1, filterWidth})

            For i = 0 To filterWidth - 1
                possibleW.data(i) = i
            Next

            Dim offsets = possibleW + possibleH * inputWidth
            possibleH.Dispose()
            possibleW.Dispose()
            startingIndexes.reshape(New Integer() {startingIndexes.TotalLength, 1})
            offsets.reshape(New Integer() {1, offsets.TotalLength})
            Dim allIndexes = startingIndexes + offsets
            startingIndexes.Dispose()
            offsets.Dispose()
            Dim outputH_W = outputDims(0) * outputDims(1)
            Dim allInOne As New Tensor(New Integer() {outputH_W, filterHeight * filterWidth * channelCount})
            Dim h_W = inputHeight * inputWidth
            Dim fH_fW = filterHeight * filterWidth
            Dim h_w_fH_fW = h_W * fH_fW
            Dim tmp As Integer

            ' A bit faster:
            Dim aiInd As i32
            Dim aioInd As i32 = Scan0

            For ch = 0 To channelCount - 1
                Dim fH_fW_ch = fH_fW * ch
                Dim h_W_ch = h_W * ch

                For m = 0 To fH_fW - 1
                    aiInd = m * outputH_W
                    aioInd = (fH_fW_ch + m) * outputH_W

                    For k = 0 To outputH_W - 1
                        tmp = CInt(allIndexes.data(++aiInd)) + h_W_ch
                        allInOne.data(++aioInd) = inputTensor.data(tmp)
                    Next
                Next
            Next

            allIndexes.Dispose()
            nextLayer.inputTensor.reshape(New Integer() {allInOne.Dimensions(0), filterCount})

            Dim x = allInOne.Dimensions(1)
            Dim y = filterCount
            Dim z = allInOne.Dimensions(0)
            Dim po As New ParallelOptions() With {.MaxDegreeOfParallelism = Environment.ProcessorCount}

            Call Tasks.Parallel.For(0, y, po,
                 Sub(f)
                     Dim aioInd_ As Integer
                     Dim outputInd_ As Integer
                     Dim weightsInd_ As Integer

                     For g = 0 To z - 1
                         Dim sum As Single = 0

                         For h = 0 To x - 1
                             aioInd_ = h * z + g
                             weightsInd_ = f * x + h
                             sum += weights.data(weightsInd_) * allInOne.data(aioInd_)
                         Next

                         outputInd_ = f * z + g

                         SyncLock nextLayer.inputTensor.data
                             nextLayer.inputTensor.data(outputInd_) = sum + biases.data(f)
                         End SyncLock
                     Next
                 End Sub)

            Call nextLayer.inputTensor.reshape(outputDims)
            Call allInOne.Dispose()

            Call disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace

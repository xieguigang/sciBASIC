#Region "Microsoft.VisualBasic::c128f702a707469381b8eb9f3e7162ed, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\Pool.vb"

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

    '   Total Lines: 88
    '    Code Lines: 66
    ' Comment Lines: 0
    '   Blank Lines: 22
    '     File Size: 2.77 KB


    '     Class Pool
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

Imports stdNum = System.Math

Namespace Convolutional

    Public Class Pool : Inherits Layer

        Public pool As Integer()
        Public stride As Integer()

        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.Pool
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer(), pad As Integer())
            Call MyBase.New(inputTensorDims, pad)

            pool = New Integer(1) {}
            stride = New Integer(1) {}
        End Sub

        Public Overloads Sub setOutputDims()
            outputDims = New Integer(2) {
                CInt(stdNum.Floor(inputTensorDims(0) / stride(0))),
                CInt(stdNum.Floor(inputTensorDims(1) / stride(1))),
                inputTensorDims(2)
            }
        End Sub

        Protected Overrides Function layerFeedNext() As Layer
            Dim inputHeight = inputTensorDims(0)
            Dim inputWidth = inputTensorDims(1)
            Dim channelCount = inputTensorDims(2)
            Dim poolHeight = pool(0)
            Dim poolWidth = pool(1)
            Dim inputInd = New Integer() {0, 0, 0}
            Dim outputInd = New Integer() {0, 0, 0}
            Dim max As Single
            Dim i As Integer = 0
            Dim j As Integer = 0

            While inputInd(2) < channelCount
                outputInd(2) = inputInd(2)
                i = 0

                While i <= inputHeight - poolHeight
                    outputInd(0) = CInt(stdNum.Floor(i / stride(0)))
                    j = 0

                    While j <= inputWidth - poolWidth
                        outputInd(1) = CInt(stdNum.Floor(j / stride(1)))
                        max = Single.MinValue
                        inputInd(0) = i

                        While inputInd(0) < i + poolHeight
                            inputInd(1) = j

                            While inputInd(1) < j + poolWidth
                                Dim f As Single = inputTensor(inputInd)

                                If f > max Then
                                    max = f
                                End If

                                inputInd(1) += 1
                            End While

                            inputInd(0) += 1
                        End While

                        Call writeNextLayerInput(outputInd, max)

                        j += stride(1)
                    End While

                    i += stride(0)
                End While

                inputInd(2) += 1
            End While

            disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace

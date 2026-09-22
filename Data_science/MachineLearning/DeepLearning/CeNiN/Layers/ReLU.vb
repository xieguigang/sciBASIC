#Region "Microsoft.VisualBasic::f308daf1a697ded08a5cc3321e2064a5, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\ReLU.vb"

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

    '   Total Lines: 63
    '    Code Lines: 37 (58.73%)
    ' Comment Lines: 12 (19.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (22.22%)
    '     File Size: 2.06 KB


    '     Class ReLU
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: layerFeedNext
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Convolutional

    ''' <summary>
    ''' Rectified linear unit layer: applies <c>f(x) = max(0, x)</c> element wise.
    ''' </summary>
    Friend Class ReLU : Inherits Layer

        ''' <summary>Gets the layer kind, always <see cref="CNN.LayerTypes.ReLU"/>.</summary>
        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.ReLU
            End Get
        End Property

        ''' <summary>
        ''' Creates a ReLU layer.
        ''' </summary>
        ''' <param name="inputTensorDims">The dimensions <c>[height, width, channels]</c> of the input.</param>
        Public Sub New(inputTensorDims As Integer())
            Call MyBase.New(inputTensorDims)
        End Sub

        ''' <summary>
        ''' Applies the rectifier to every element of the input tensor.
        ''' </summary>
        ''' <returns>This layer instance once the activations have been written to the next layer.</returns>
        Protected Overrides Function layerFeedNext() As Layer
            Dim inputHeight = inputTensorDims(0)
            Dim inputWidth = inputTensorDims(1)
            Dim channelCount = inputTensorDims(2)
            Dim f As Single
            Dim inputInd = New Integer() {0, 0, 0}

            While inputInd(0) < inputHeight
                inputInd(1) = 0

                While inputInd(1) < inputWidth
                    inputInd(2) = 0

                    While inputInd(2) < channelCount
                        f = inputTensor(inputInd)

                        If f < 0 Then
                            f = 0
                        End If

                        Call writeNextLayerInput(inputInd, f)

                        inputInd(2) += 1
                    End While

                    inputInd(1) += 1
                End While

                inputInd(0) += 1
            End While

            Call disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::580fa6488565b1f10f5fcded80bb4bf8, Data_science\MachineLearning\DeepLearning\CNN\Layers\Layer.vb"

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

    '   Total Lines: 35
    '    Code Lines: 10 (28.57%)
    ' Comment Lines: 19 (54.29%)
    '    - Xml Docs: 94.74%
    ' 
    '   Blank Lines: 6 (17.14%)
    '     File Size: 1.44 KB


    '     Interface Layer
    ' 
    '         Properties: BackPropagationResult, Type
    ' 
    '         Function: forward
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

Namespace CNN.layers

    ''' <summary>
    ''' A convolution neural network is built of layers that the data traverses
    ''' back and forth in order to predict what the network sees in the data.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Interface Layer

        ''' <summary>
        ''' Gets the parameter and gradient blocks of this layer; the trainer reads them to adjust the weights.
        ''' </summary>
        ReadOnly Property BackPropagationResult As IEnumerable(Of BackPropResult)
        ''' <summary>Gets the kind of this layer.</summary>
        ReadOnly Property Type As LayerTypes

        ''' <summary>
        ''' Runs the forward pass of the layer.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">When <c>True</c> the layer runs in training mode (for example dropout is active).</param>
        ''' <returns>The output data block passed to the next layer.</returns>
        Function forward(db As DataBlock, training As Boolean) As DataBlock
        ''' <summary>
        ''' Computes and accumulates the gradients with respect to the weights and biases of this layer.
        ''' </summary>
        Sub backward()

    End Interface

End Namespace

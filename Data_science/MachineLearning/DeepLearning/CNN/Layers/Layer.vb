#Region "Microsoft.VisualBasic::ca29c7949833a75d56174d47a78f5e6f, Data_science\MachineLearning\DeepLearning\CNN\Layers\Layer.vb"

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

    '   Total Lines: 29
    '    Code Lines: 10
    ' Comment Lines: 13
    '   Blank Lines: 6
    '     File Size: 975 B


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
        ''' adjust the weight at here in the trainer module
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property BackPropagationResult As IEnumerable(Of BackPropResult)
        ReadOnly Property Type As LayerTypes

        Function forward(db As DataBlock, training As Boolean) As DataBlock
        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        Sub backward()

    End Interface

End Namespace

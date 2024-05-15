#Region "Microsoft.VisualBasic::5060db9f9c4ca8d14aacc7e237c5294c, Data_science\MachineLearning\DeepLearning\CNN\layers\losslayers\LossLayer.vb"

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

    '   Total Lines: 61
    '    Code Lines: 35
    ' Comment Lines: 13
    '   Blank Lines: 13
    '     File Size: 2.04 KB


    '     Class LossLayer
    ' 
    '         Properties: OutAct
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional
Imports Layer = Microsoft.VisualBasic.MachineLearning.CNN.layers.Layer

Namespace CNN.losslayers

    ''' <summary>
    ''' Created by danielp on 1/25/17.
    ''' </summary>
    Public MustInherit Class LossLayer : Inherits DataLink
        Implements Layer

        Protected Friend num_inputs, out_depth, out_sx, out_sy As Integer

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public Overridable ReadOnly Property OutAct As DataBlock
            Get
                Return out_act
            End Get
        End Property

        Public MustOverride ReadOnly Property Type As LayerTypes Implements Layer.Type

        Public Sub New(def As OutputDefinition)
            ' computed
            num_inputs = def.outY * def.outX * def.depth
            out_depth = num_inputs
            out_sx = 1
            out_sy = 1

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
        End Sub

        ''' <summary>
        ''' compute and accumulate gradient wrt weights and bias of this layer
        ''' </summary>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public MustOverride Function backward(y As Integer) As Double
        Public MustOverride Function backward(y As Double()) As Double()
        Public MustOverride Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward

    End Class

End Namespace

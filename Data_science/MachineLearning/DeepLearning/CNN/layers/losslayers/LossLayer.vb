#Region "Microsoft.VisualBasic::2a1ca94ea8e167e0d17ad1286a50bd11, Data_science\MachineLearning\DeepLearning\CNN\Layers\losslayers\LossLayer.vb"

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

    '   Total Lines: 84
    '    Code Lines: 34 (40.48%)
    ' Comment Lines: 35 (41.67%)
    '    - Xml Docs: 94.29%
    ' 
    '   Blank Lines: 15 (17.86%)
    '     File Size: 3.54 KB


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
Imports Layer = Microsoft.VisualBasic.MachineLearning.CNN.layers.Layer

Namespace CNN.losslayers

    ''' <summary>
    ''' Base class of the loss layers that terminate a network: it flattens the incoming activations into a vector and
    ''' computes the loss together with the gradient consumed by the backward pass.
    ''' </summary>
    Public MustInherit Class LossLayer : Inherits DataLink
        Implements Layer

        ''' <summary>Number of inputs and the flattened output shape of this loss layer.</summary>
        Protected Friend num_inputs, out_depth, out_sx, out_sy As Integer

        ''' <summary>Gets the parameter and gradient blocks of this layer; loss layers have none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the output activations produced by the most recent forward pass.</summary>
        Public Overridable ReadOnly Property OutAct As DataBlock
            Get
                Return out_act
            End Get
        End Property

        ''' <summary>Gets the kind of this loss layer.</summary>
        Public MustOverride ReadOnly Property Type As LayerTypes Implements Layer.Type

        ''' <summary>
        ''' Creates the loss layer and flattens the shared output definition to a vector.
        ''' </summary>
        ''' <param name="def">The shared output definition that carries the last layer shape.</param>
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

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Computes and accumulates the gradients of this layer; the loss layers perform their work in the typed
        ''' <c>backward</c> overloads.
        ''' </summary>
        Public Overridable Sub backward() Implements Layer.backward
        End Sub

        ''' <summary>
        ''' Computes the loss and the input gradients for a classification target.
        ''' </summary>
        ''' <param name="y">Index of the target class.</param>
        ''' <returns>The loss value.</returns>
        Public MustOverride Function backward(y As Integer) As Double

        ''' <summary>
        ''' Computes the loss and the input gradients for a continuous (regression) target.
        ''' </summary>
        ''' <param name="y">The target output vector.</param>
        ''' <returns>The per element loss vector.</returns>
        Public MustOverride Function backward(y As Double()) As Double()

        ''' <summary>
        ''' Runs the forward pass of this loss layer.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">When <c>True</c> the layer runs in training mode.</param>
        ''' <returns>The output data block.</returns>
        Public MustOverride Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward

    End Class

End Namespace

#Region "Microsoft.VisualBasic::8e63e0522ee8c3f7914b1ec4a34cc8f0, Data_science\MachineLearning\DeepLearning\CNN\Layers\InputLayer.vb"

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

    '   Total Lines: 81
    '    Code Lines: 37 (45.68%)
    ' Comment Lines: 30 (37.04%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 14 (17.28%)
    '     File Size: 2.91 KB


    '     Class InputLayer
    ' 
    '         Properties: dims, out_depth, Type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: forward, ToString
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data

Namespace CNN.layers

    ''' <summary>
    ''' The input layer is a simple layer that will pass the data though and
    ''' create a window into the full training data set. So for instance if
    ''' we have an image of size 28x28x1 which means that we have 28 pixels
    ''' in the x axle and 28 pixels in the y axle and one color (gray scale),
    ''' then this layer might give you a window of another size example 24x24x1
    ''' that is randomly chosen in order to create some distortion into the
    ''' dataset so the algorithm don't over-fit the training.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class InputLayer : Inherits DataLink
        Implements Layer

        ''' <summary>
        ''' the image data size dimension [width, height]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dims As Dimension

        ''' <summary>
        ''' the image data channels, example as color rgb channels, brightness, etc
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property out_depth As Integer

        ''' <summary>Gets the parameter and gradient blocks of this layer; the input layer has none.</summary>
        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        ''' <summary>Gets the kind of this layer, always <see cref="LayerTypes.Input"/>.</summary>
        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Input
            End Get
        End Property

        ''' <summary>Creates an empty layer, used by the deserializer.</summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' Creates the input layer and updates the shared output definition with the image size.
        ''' </summary>
        ''' <param name="def">The shared output definition passed to the next layer.</param>
        ''' <param name="out_sx">Image width.</param>
        ''' <param name="out_sy">Image height.</param>
        ''' <param name="out_depth">
        ''' Number of channels; usually one for brightness, but it may be greater than one, for example three for RGB.
        ''' </param>
        Public Sub New(def As OutputDefinition, out_sx As Integer, out_sy As Integer, Optional out_depth As Integer = 1)
            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth

            Me.dims = New Dimension(out_sx, out_sy)
            Me.out_depth = out_depth
        End Sub

        ''' <summary>
        ''' Passes the data through unchanged; the input layer performs no computation.
        ''' </summary>
        ''' <param name="db">The input data block.</param>
        ''' <param name="training">Ignored by the input layer.</param>
        ''' <returns>The same data block that was passed in.</returns>
        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            out_act = db
            Return out_act
        End Function

        ''' <summary>Does nothing; the input layer has no parameters to update.</summary>
        Public Overridable Sub backward() Implements Layer.backward

        End Sub

        ''' <summary>Returns a short description of this layer.</summary>
        ''' <returns>A text of the form <c>input(dims: ...)</c>.</returns>
        Public Overrides Function ToString() As String
            Return $"input(dims: {dims})"
        End Function
    End Class

End Namespace

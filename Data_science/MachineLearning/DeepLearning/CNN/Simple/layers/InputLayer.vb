﻿Imports Microsoft.VisualBasic.MachineLearning.CNN.data

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
    Public Class InputLayer : Implements Layer

        Private in_act As DataBlock
        Private out_act As DataBlock

        Public ReadOnly Property dims As Dimension
        Public ReadOnly Property out_depth As Integer

        Public Overridable ReadOnly Property BackPropagationResult As IList(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Return New List(Of BackPropResult)()
            End Get
        End Property

        Public Sub New(def As OutputDefinition, out_sx As Integer, out_sy As Integer, out_depth As Integer)
            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth

            Me.dims = New Dimension(out_sx, out_sy)
            Me.out_depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            out_act = db
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward

        End Sub

        Public Overrides Function ToString() As String
            Return "input()"
        End Function
    End Class

End Namespace
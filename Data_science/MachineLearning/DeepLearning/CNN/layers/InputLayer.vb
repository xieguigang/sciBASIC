Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.Convolutional

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

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Input
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="def"></param>
        ''' <param name="out_sx">image width</param>
        ''' <param name="out_sy">image height</param>
        ''' <param name="out_depth">
        ''' usually be one channel, color brightness, this parameter value could 
        ''' be greater than 1, example value 3 probabilty for rgb channels
        ''' </param>
        Public Sub New(def As OutputDefinition, out_sx As Integer, out_sy As Integer, Optional out_depth As Integer = 1)
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
            Return $"input(dims: {dims})"
        End Function
    End Class

End Namespace

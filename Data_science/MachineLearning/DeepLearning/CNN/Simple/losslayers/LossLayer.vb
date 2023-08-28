
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers

Namespace CNN.losslayers

    ''' <summary>
    ''' Created by danielp on 1/25/17.
    ''' </summary>
    <Serializable>
    Public MustInherit Class LossLayer
        Implements Layer
        Public MustOverride Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
        Protected Friend num_inputs, out_depth, out_sx, out_sy As Integer
        Protected Friend in_act, out_act As DataBlock

        Public Sub New(def As OutputDefinition)
            ' computed
            num_inputs = def.OutY * def.OutX * def.Depth
            out_depth = num_inputs
            out_sx = 1
            out_sy = 1

            def.OutX = out_sx
            def.OutY = out_sy
            def.Depth = out_depth
        End Sub

        Public Overridable Sub backward() Implements Layer.backward
        End Sub
        Public MustOverride Function backward(y As Integer) As Double

        Public Overridable ReadOnly Property BackPropagationResult As IList(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Return New List(Of BackPropResult)()
            End Get
        End Property

        Public Overridable ReadOnly Property OutAct As DataBlock
            Get
                Return out_act
            End Get
        End Property
    End Class

End Namespace

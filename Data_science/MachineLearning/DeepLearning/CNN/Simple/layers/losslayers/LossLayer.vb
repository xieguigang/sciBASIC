
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports Microsoft.VisualBasic.MachineLearning.CNN.layers

Namespace CNN.losslayers

    ''' <summary>
    ''' Created by danielp on 1/25/17.
    ''' </summary>
    Public MustInherit Class LossLayer
        Implements Layer

        Protected Friend num_inputs, out_depth, out_sx, out_sy As Integer
        Protected Friend in_act, out_act As DataBlock

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
        Public MustOverride Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward

    End Class

End Namespace

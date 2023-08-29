Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace CNN.layers

    ''' <summary>
    ''' This layer will remove some random activations in order to
    ''' defeat over-fitting.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class DropoutLayer
        Implements Layer

        Private out_depth, out_sx, out_sy As Integer
        Private in_act, out_act As DataBlock
        Private ReadOnly drop_prob As Double = 0.5
        Private dropped As Boolean()

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public Sub New(def As OutputDefinition)
            ' computed
            out_sx = def.outX
            out_sy = def.outY
            out_depth = def.depth

            dropped = New Boolean(out_sx * out_sy * out_depth - 1) {}
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db
            Dim V2 As DataBlock = db.clone()
            Dim N = db.Weights.Length
            If training Then
                ' do dropout
                For i = 0 To N - 1
                    If randf.NextDouble() < drop_prob Then
                        V2.setWeight(i, 0)
                        dropped(i) = True
                    Else
                        ' drop!
                        dropped(i) = False
                    End If
                Next
            Else
                ' scale the activations during prediction
                For i = 0 To N - 1
                    V2.mulGradient(i, drop_prob)
                Next
            End If
            out_act = V2
            Return out_act ' dummy identity function for now
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act ' we need to set dw of this
            Dim chain_grad = out_act
            Dim N = V.Weights.Length
            V.clearGradient() ' zero out gradient wrt data
            For i = 0 To N - 1
                If Not dropped(i) Then
                    V.setGradient(i, chain_grad.getGradient(i)) ' copy over the gradient
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return "dropout()"
        End Function
    End Class

End Namespace

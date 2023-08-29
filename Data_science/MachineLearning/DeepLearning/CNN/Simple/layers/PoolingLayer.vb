Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports std = System.Math


Namespace CNN.layers

    ''' <summary>
    ''' This layer will reduce the dataset by creating a smaller zoomed out
    ''' version. In essence you take a cluster of pixels take the sum of them
    ''' and put the result in the reduced position of the new image.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    <Serializable>
    Public Class PoolingLayer : Implements Layer

        Private in_depth, in_sx, in_sy As Integer
        Private out_depth, out_sx, out_sy As Integer
        Private sx, sy, stride, padding As Integer

        Private switchx As Integer()
        Private switchy As Integer()

        Private in_act, out_act As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public Sub New(def As OutputDefinition, sx As Integer, stride As Integer, padding As Integer)
            Me.sx = sx
            Me.stride = stride

            in_depth = def.depth
            in_sx = def.outX
            in_sy = def.outY

            ' optional
            sy = Me.sx
            Me.padding = padding

            ' computed
            out_depth = in_depth
            out_sx = CInt(std.Floor((in_sx + Me.padding * 2 - Me.sx) / Me.stride + 1))
            out_sy = CInt(std.Floor((in_sy + Me.padding * 2 - sy) / Me.stride + 1))

            ' store switches for x,y coordinates for where the max comes from, for each output neuron
            switchx = New Integer(out_sx * out_sy * out_depth - 1) {}
            switchy = New Integer(out_sx * out_sy * out_depth - 1) {}
            switchx.fill(0)
            switchy.fill(0)

            def.outX = out_sx
            def.outY = out_sy
            def.depth = out_depth
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            in_act = db

            Dim lA As DataBlock = New DataBlock(out_sx, out_sy, out_depth, 0.0)

            Dim n = 0 ' a counter for switches
            For d = 0 To out_depth - 1
                Dim x = -padding
                Dim ax = 0

                While ax < out_sx
                    Dim y = -padding
                    Dim ay = 0

                    While ay < out_sy

                        ' convolve centered at this particular location
                        Dim a As Double = -99999 ' hopefully small enough ;\
                        Dim winx = -1
                        Dim winy = -1
                        For fx = 0 To sx - 1
                            For fy = 0 To sy - 1
                                Dim oy = y + fy
                                Dim ox = x + fx
                                If oy >= 0 AndAlso oy < db.SY AndAlso ox >= 0 AndAlso ox < db.SX Then
                                    Dim v = db.getWeight(ox, oy, d)
                                    ' perform max pooling and store pointers to where
                                    ' the max came from. This will speed up backprop
                                    ' and can help make nice visualizations in future
                                    If v > a Then
                                        a = v
                                        winx = ox
                                        winy = oy
                                    End If
                                End If
                            Next
                        Next
                        switchx(n) = winx
                        switchy(n) = winy
                        n += 1
                        lA.setWeight(ax, ay, d, a)
                        y += stride
                        ay += 1
                    End While

                    x += stride
                    ax += 1
                End While
            Next
            out_act = lA
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            ' pooling layers have no parameters, so simply compute
            ' gradient wrt data here
            Dim V = in_act
            V.clearGradient() ' zero out gradient wrt data

            Dim n = 0
            For d = 0 To out_depth - 1
                Dim x = -padding
                Dim ax = 0

                While ax < out_sx
                    Dim y = -padding
                    Dim ay = 0

                    While ay < out_sy
                        Dim chain_grad = out_act.getGradient(ax, ay, d)
                        V.addGradient(switchx(n), switchy(n), d, chain_grad)
                        n += 1
                        y += stride
                        ay += 1
                    End While

                    x += stride
                    ax += 1
                End While
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return "pooling()"
        End Function
    End Class

End Namespace

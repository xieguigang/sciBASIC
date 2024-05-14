#Region "Microsoft.VisualBasic::17e3497f6e822a1f5c19837849e462d1, Data_science\MachineLearning\DeepLearning\CNN\layers\LocalResponseNormalizationLayer.vb"

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

    '   Total Lines: 132
    '    Code Lines: 81
    ' Comment Lines: 28
    '   Blank Lines: 23
    '     File Size: 5.04 KB


    '     Class LocalResponseNormalizationLayer
    ' 
    '         Properties: Type
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
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' This layer is useful when we are dealing with ReLU neurons. Why is that?
    ''' Because ReLU neurons have unbounded activations and we need LRN to normalize
    ''' that. We want to detect high frequency features with a large response. If we
    ''' normalize around the local neighborhood of the excited neuron, it becomes even
    ''' more sensitive as compared to its neighbors.
    ''' 
    ''' At the same time, it will dampen the responses that are uniformly large in any
    ''' given local neighborhood. If all the values are large, then normalizing those
    ''' values will diminish all of them. So basically we want to encourage some kind
    ''' of inhibition and boost the neurons with relatively larger activations. This
    ''' has been discussed nicely in Section 3.3 of the original paper by Krizhevsky et al.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class LocalResponseNormalizationLayer : Inherits DataLink
        Implements Layer

        ' 
        ' 		 The constants k, n, alpha and beta are hyper-parameters whose
        ' 		   values are determined using a validation set; we used
        ' 		   k = 2, n = 5, alpha = 10^-4, beta = 0.75
        ' 	
        ' 		   quote from http://www.cs.toronto.edu/~fritz/absps/imagenet.pdf
        ' 		

        Private ReadOnly k As Double = 2.0
        Private ReadOnly n As Double = 5.0
        Private ReadOnly alpha As Double = 0.0001
        Private ReadOnly beta As Double = 0.75

        Dim S_cache_ As DataBlock

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.LRN
            End Get
        End Property

        Sub New()
        End Sub

        Public Sub New(n As Integer)
            ' checks
            If n Mod 2 = 0 Then
                VBDebugger.EchoLine("WARNING: n should be odd for LRN layer")
            End If

            Me.n = n
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim A As DataBlock = db.cloneAndZero()
            Dim n2 = std.Floor(n / 2)

            in_act = db
            S_cache_ = db.cloneAndZero()

            For x = 0 To db.SX - 1
                For y = 0 To db.SY - 1
                    For i = 0 To db.Depth - 1

                        Dim ai = db.getWeight(x, y, i)

                        ' normalize in a window of size n
                        Dim den = 0.0
                        For j As Integer = std.Max(0, i - n2) To std.Min(i + n2, db.Depth - 1)
                            Dim aa = db.getWeight(x, y, j)
                            den += aa * aa
                        Next
                        den *= alpha / n
                        den += k
                        S_cache_.setWeight(x, y, i, den) ' will be useful for backprop
                        den = std.Pow(den, beta)
                        A.setWeight(x, y, i, ai / den)
                    Next
                Next
            Next

            out_act = A
            Return out_act ' dummy identity function for now
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            ' evaluate gradient wrt data
            ' we need to set dw of this
            Dim V = in_act.clearGradient()
            Dim n2 As Integer = std.Floor(n / 2)

            For x = 0 To V.SX - 1
                For y = 0 To V.SY - 1
                    For i = 0 To V.Depth - 1

                        Dim chain_grad = out_act.getGradient(x, y, i)
                        Dim S = S_cache_.getWeight(x, y, i)
                        Dim SB = std.Pow(S, beta)
                        Dim SB2 = SB * SB

                        ' normalize in a window of size n
                        For j = std.Max(0, i - n2) To std.Min(i + n2, V.Depth - 1)
                            Dim aj = V.getWeight(x, y, j)
                            Dim g = -aj * beta * std.Pow(S, beta - 1) * alpha / n * 2 * aj
                            If j = i Then
                                g += SB
                            End If
                            g /= SB2
                            g *= chain_grad
                            V.addGradient(x, y, j, g)
                        Next

                    Next
                Next
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"LRN()"
        End Function
    End Class

End Namespace

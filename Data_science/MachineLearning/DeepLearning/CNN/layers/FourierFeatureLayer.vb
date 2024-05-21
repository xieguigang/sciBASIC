#Region "Microsoft.VisualBasic::106b42bb647e53951b4cf33124e8acc9, Data_science\MachineLearning\DeepLearning\CNN\Layers\FourierFeatureLayer.vb"

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

    '   Total Lines: 98
    '    Code Lines: 58
    ' Comment Lines: 26
    '   Blank Lines: 14
    '     File Size: 4.06 KB


    '     Class FourierFeatureLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: forward
    ' 
    '         Sub: backward
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.CNN.data
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' Based on the paper "Fourier Features Let Networks Learn
    ''' High Frequency Functions in Low Dimensional Domains" (2020) presented 
    ''' at NeurIPS (https://bmild.github.io/fourfeat/index.html) 
    ''' by Matthew Tancik, Pratul P. Srinivasan, Ben Mildenhall, Sara Fridovich-Keil,
    ''' Nithin Raghavan, Utkarsh Singhal, Ravi Ramamoorthi, Jonathan T. Barron and Ren Ng.
    ''' 
    ''' Please see the Python implementation to see examples of the concept in action:
    ''' https://github.com/tancik/fourier-feature-networks/blob/master/Demo.ipynb. 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/karpathy/convnetjs/blob/5a6136314877d75be5b2228e55d1431f9a74626f/src/convnet_layers_fourier_feature.js
    ''' </remarks>
    Public Class FourierFeatureLayer : Inherits DataLink
        Implements Layer

        Public ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.FourierFeature
            End Get
        End Property

        Dim in_depth, in_sx, in_sy As Integer
        Dim out_depth, out_sx, out_sy As Integer
        Dim gaussian_mapping_scale As Double = -1

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="def"></param>
        ''' <param name="gaussian_mapping_scale">
        ''' optional - whether to factor in a random number (sampled from a Gaussian)
        ''' in the mapping or not (defaults to -1, which signifies not to include it)
        ''' </param>
        Sub New(def As OutputDefinition, Optional gaussian_mapping_scale As Double = -1)
            Me.in_depth = def.depth
            Me.in_sx = def.outX
            Me.in_sy = def.outY
            Me.gaussian_mapping_scale = gaussian_mapping_scale
            Me.out_depth = in_depth * 2 * std.Abs(gaussian_mapping_scale)
            Me.out_sx = in_sx
            Me.out_sy = in_sy
        End Sub

        Public Sub backward() Implements Layer.backward
            ' no parameters, so simply compute gradient wrt data here
            ' zero out gradient wrt data
            Call in_act.clearGradient()
        End Sub

        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim mappedFeature As New DataBlock(out_sx, out_sy, out_depth, 0) With {.trace = Me.ToString}

            For d As Integer = 0 To out_depth - 1
                Dim x = 0, y = 0

                For ax As Integer = 0 To out_sx - 1
                    x += 1
                    For ay As Integer = 0 To out_sy - 1
                        y += 1
                        Dim v = db.getWeight(ax, ay, d Mod in_depth)
                        Dim randomProjFactor As Double = 1
                        If gaussian_mapping_scale > 0 Then
                            randomProjFactor *= randf.NextDouble
                        End If

                        Dim a As Double
                        ' for the first "half" of the fourier feature - use cosine
                        If d < (out_depth / 2) Then
                            a = std.Cos(2 * std.PI * v * randomProjFactor)
                        Else
                            ' use sine for the second "half"
                            a = std.Sin(2 * std.PI * v * randomProjFactor)
                        End If

                        mappedFeature.setWeight(ax, ay, d, a)
                    Next
                Next
            Next

            out_act = mappedFeature

            Return out_act
        End Function
    End Class
End Namespace

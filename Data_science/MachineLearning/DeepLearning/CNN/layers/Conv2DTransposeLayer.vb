#Region "Microsoft.VisualBasic::c66403b43f07766086c5d2925b464c61, Data_science\MachineLearning\DeepLearning\CNN\Layers\Conv2DTransposeLayer.vb"

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

    '   Total Lines: 235
    '    Code Lines: 182
    ' Comment Lines: 15
    '   Blank Lines: 38
    '     File Size: 10.12 KB


    '     Class Conv2DTransposeLayer
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
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace CNN.layers

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/TrevorBlythe/MentisJS/blob/main/src/layers/DeconvLayer.js
    ''' </remarks>
    Public Class Conv2DTransposeLayer : Inherits DataLink
        Implements Layer

        Public ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                Yield New BackPropResult(filterw, filterws, l1_decay_mul, l2_decay_mul)
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Conv2DTranspose
            End Get
        End Property

        Friend out_depth, out_sx, out_sy As Integer
        Friend in_depth, in_sx, in_sy As Integer
        Friend filterWidth, filterHeight As Integer
        Friend stride As Integer
        Friend l1_decay_mul As Double = 0.0
        Friend l2_decay_mul As Double = 1.0

        Dim filters As Integer
        Dim filterw, filterws As Double()
        Dim useBias As Boolean
        ' Dim inData As Double()
        Dim b, bs As Double()
        Dim hMFHPO As Integer
        Dim wMFWPO As Integer
        Dim hMFWMF As Integer
        Dim wIH As Integer
        Dim wIHID As Integer
        Dim fWIH As Integer
        Dim fWIHID As Integer

        Sub New()
        End Sub

        Public Sub New(def As OutputDefinition,
                       output As OutputDefinition,
                       filter As Dimension,
                       Optional filters As Integer = 3,
                       Optional stride As Integer = 1,
                       Optional useBias As Boolean = False)

            Me.in_depth = output.depth
            Me.in_sx = output.outX
            Me.in_sy = output.outY

            Me.out_depth = def.depth
            Me.useBias = useBias

            Me.filterWidth = filter.x
            Me.filterHeight = filter.y

            Me.stride = stride
            Me.filters = filters
            Me.filterw = New Double(filters * in_depth * filterWidth * filterHeight - 1) {}
            Me.filterws = New Double(filters * in_depth * filterWidth * filterHeight - 1) {}

            Dim in_w As Integer = std.Ceiling((in_sx - filterWidth + 1) / stride)
            Dim in_h As Integer = std.Ceiling((in_sy - filterHeight + 1) / stride)

            If def.outX <> in_w OrElse def.outY <> in_h OrElse def.depth <> filters Then
                Throw New InvalidProgramException($"The dimension of the input is not matched! required(w:{in_w}, h:{in_h}, depth:{filters}), but given(w:{def.outX}, h:{def.outY}, depth:{def.depth})!")
            End If

            ' Me.inData = New Double(in_w * in_h * filters - 1) {}

            Dim out_len As Integer = in_sx * in_sy * in_depth

            Me.b = New Double(out_len - 1) {}
            Me.bs = New Double(out_len - 1) {}

            If filterWidth > in_sx OrElse filterHeight > in_sy Then
                Throw New InvalidProgramException("Conv layer error: filters cannot be bigger than the input")
            End If

            ' init random weights
            Me.filterw = Vector.rand(-1, 1, size:=filterw.Length)

            For a As Integer = 0 To 3 - 1
                Dim newFilterw = Me.filterw.ToArray
                For f As Integer = 0 To filters - 1
                    For d As Integer = 0 To in_depth - 1
                        For x As Integer = 0 To filterWidth - 1
                            For y As Integer = 0 To filterHeight - 1
                                Dim count As Integer = 0
                                Dim ind As Integer = f * in_depth * filterWidth * filterHeight + x + y * filterWidth + d * filterWidth * filterHeight
                                Dim indR As Integer = f * in_depth * filterWidth * filterHeight + (x + 1) + y * filterWidth + d * filterWidth * filterHeight
                                Dim indL As Integer = f * in_depth * filterWidth * filterHeight + (x - 1) + y * filterWidth + d * filterWidth * filterHeight
                                Dim indD As Integer = f * in_depth * filterWidth * filterHeight + x + (y + 1) * filterWidth + d * filterWidth * filterHeight
                                Dim indU As Integer = f * in_depth * filterWidth * filterHeight + x + (y - 1) * filterWidth + d * filterWidth * filterHeight

                                If (x < filterWidth - 1) Then count += filterw(indR)
                                If (x > 1) Then count += filterw(indL)
                                If (y < filterHeight - 1) Then count += filterw(indD)
                                If (y > 1) Then count += filterw(indU)
                                newFilterw(ind) += count / 5
                            Next
                        Next
                    Next
                Next

                filterw = newFilterw
            Next

            If useBias Then
                b = Vector.rand(-1, 1, b.Length)
            Else
                b = Vector.Zero([Dim]:=b.Length)
            End If

            'Everything below here Is precalculated constants used in forward/backward
            'to optimize this And make sure we are as effeiciant as possible.
            'DONT CHANGE THESE Or BIG BREAKY BREAKY!
            Me.hMFHPO = std.Ceiling((Me.in_sy - Me.filterHeight + 1) / Me.stride)
            Me.wMFWPO = std.Ceiling((Me.in_sx - Me.filterWidth + 1) / Me.stride)
            Me.hMFWMF = Me.hMFHPO * Me.wMFWPO
            Me.wIH = Me.in_sx * Me.in_sy
            Me.wIHID = Me.in_sx * Me.in_sy * Me.in_depth
            Me.fWIH = Me.filterWidth * Me.filterHeight
            Me.fWIHID = Me.in_depth * Me.filterHeight * Me.filterWidth

            def.outX = in_sx
            def.outY = in_sy
            def.depth = in_depth

            out_act = New DataBlock(in_sx, in_sy, in_depth) With {.trace = Me.ToString}
        End Sub

        Public Sub backward() Implements Layer.backward
            Dim costs As Double() = in_act.clearGradient().Gradients
            Dim err As Double() = out_act.Gradients
            Dim inData As Double() = in_act.w

            For i As Integer = 0 To filters - 1
                Dim iHMFWMF = i * hMFWMF
                Dim iFWIHID = i * fWIHID
                For g As Integer = 0 To hMFHPO - 1
                    Dim ga = g * stride
                    Dim gWMFWPO = g * wMFWPO
                    For b As Integer = 0 To wMFWPO - 1
                        Dim odi = b + gWMFWPO + iHMFWMF
                        Dim ba = b * stride
                        For h As Integer = 0 To in_depth - 1
                            Dim hWIH = h * wIH
                            Dim hFWIH = h * fWIH + iFWIHID
                            For j As Integer = 0 To filterHeight - 1
                                Dim jGAIWBA = (j + ga) * in_sx + hWIH + ba
                                Dim jFWHFWIH = j * filterWidth + hFWIH
                                For k As Integer = 0 To filterWidth - 1
                                    costs(odi) += filterw(k + jFWHFWIH) * err(k + jGAIWBA)
                                    filterws(k + jFWHFWIH) += inData(odi) * err(k + jGAIWBA)
                                Next
                            Next
                        Next
                    Next
                Next
            Next

            For i As Integer = 0 To out_act.Weights.Length - 1
                bs(i) += err(i)
            Next
        End Sub

        Public Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim outData As Double()

            If out_act Is Nothing Then
                ' fix the null reference when load model
                out_act = New DataBlock(in_sx, in_sy, in_depth) With {.trace = Me.ToString}
            End If

            in_act = db.clone
            outData = out_act.w

            Dim inData As Double() = in_act.w
            Dim odiMax As New List(Of Integer)

            ' -------------Beginning of monstrosity-----------------
            For i As Integer = 0 To filters - 1
                Dim iHMFWMF = i * hMFWMF
                Dim iFWIHID = i * fWIHID

                For g As Integer = 0 To hMFHPO - 1
                    Dim ga = g * stride
                    Dim gWMFWPO = g * wMFWPO
                    For b As Integer = 0 To wMFWPO - 1
                        Dim odi = b + gWMFWPO + iHMFWMF
                        Dim ba = b * stride
                        odiMax.Add(odi)
                        For h As Integer = 0 To in_depth - 1
                            Dim hWIH = h * wIH + ba
                            Dim hFWIH = h * fWIH + iFWIHID
                            For j As Integer = 0 To filterHeight - 1
                                Dim jGAIWBA = (j + ga) * in_sx + hWIH
                                Dim jFWHFWIH = j * filterWidth + hFWIH
                                For k As Integer = 0 To filterWidth - 1
                                    outData(k + jGAIWBA) += inData(odi) * filterw(k + jFWHFWIH)
                                Next
                            Next
                        Next
                    Next
                Next
            Next
            ' -------------End of monstrosity-----------------
            Dim max = odiMax.Max

            If useBias Then
                For i As Integer = 0 To outData.Length - 1
                    outData(i) += b(i)
                Next
            End If

            Return out_act
        End Function

        Public Overrides Function ToString() As String
            Return "conv2d_transpose()"
        End Function
    End Class
End Namespace

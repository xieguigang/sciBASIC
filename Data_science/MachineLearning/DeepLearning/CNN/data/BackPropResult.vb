#Region "Microsoft.VisualBasic::df646f6919b4656555721eb4b95f00b2, Data_science\MachineLearning\DeepLearning\CNN\data\BackPropResult.vb"

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

    '   Total Lines: 91
    '    Code Lines: 43 (47.25%)
    ' Comment Lines: 35 (38.46%)
    '    - Xml Docs: 97.14%
    ' 
    '   Blank Lines: 13 (14.29%)
    '     File Size: 4.14 KB


    '     Class BackPropResult
    ' 
    '         Properties: Gradients, L1DecayMul, L2DecayMul, ModifiedHandler, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: NotifyModified
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace CNN.data

    ''' <summary>
    ''' When we have done a back propagation of the network we will receive a
    ''' result of weight adjustments required to learn. This result set will
    ''' contain the data used by the trainer.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class BackPropResult

        Friend l1_decay_mul, l2_decay_mul As Double

        Dim w As Double()
        Dim dw As Double()

        ''' <summary>Multiplier applied to the L1 regularization term of this parameter block.</summary>
        Public Overridable ReadOnly Property L1DecayMul As Double
            Get
                Return l1_decay_mul
            End Get
        End Property

        ''' <summary>Multiplier applied to the L2 regularization term of this parameter block.</summary>
        Public Overridable ReadOnly Property L2DecayMul As Double
            Get
                Return l2_decay_mul
            End Get
        End Property

        ''' <summary>Gets the parameter values (weights and biases) of this block.</summary>
        Public Overridable ReadOnly Property Weights As Double()
            Get
                Return w
            End Get
        End Property

        ''' <summary>Gets the gradients that correspond one to one with <see cref="Weights"/>.</summary>
        Public Overridable ReadOnly Property Gradients As Double()
            Get
                Return dw
            End Get
        End Property

        ''' <summary>
        ''' Callback invoked after the trainer has written <see cref="Weights"/> / <see cref="Gradients"/> in place.
        ''' </summary>
        ''' <remarks>
        ''' These arrays are usually the backing storage of a <c>DataBlock</c>, and the CUDA back end caches them in device
        ''' memory. Because the trainer writes the arrays directly, bypassing the Tensor indexer, the Tensor itself cannot
        ''' notice the change; this callback must declare the cache stale, otherwise the GPU side keeps reusing the old copy
        ''' and silently produces wrong numbers. A value of <c>Nothing</c> means that this parameter block has no device-side
        ''' cache.
        ''' </remarks>
        Public Property ModifiedHandler As Action

        ''' <summary>
        ''' Creates a back propagation result for one parameter block.
        ''' </summary>
        ''' <param name="w">The parameter values.</param>
        ''' <param name="dw">The gradients corresponding to <paramref name="w"/>.</param>
        ''' <param name="l1_decay_mul">Multiplier of the L1 regularization term.</param>
        ''' <param name="l2_decay_mul">Multiplier of the L2 regularization term.</param>
        ''' <param name="modifiedHandler">Optional callback used to invalidate device-side caches after an in-place write.</param>
        Public Sub New(w As Double(), dw As Double(), l1_decay_mul As Double, l2_decay_mul As Double,
                       Optional modifiedHandler As Action = Nothing)
            Me.w = w
            Me.dw = dw
            Me.l1_decay_mul = l1_decay_mul
            Me.l2_decay_mul = l2_decay_mul
            Me.ModifiedHandler = modifiedHandler
        End Sub

        ''' <summary>
        ''' Called by the trainer after writing the parameters or gradients in place, declaring the device-side cached copies
        ''' stale.
        ''' </summary>
        Public Sub NotifyModified()
            Dim handler = ModifiedHandler

            If handler IsNot Nothing Then Call handler()
        End Sub

        ''' <summary>Returns a short diagnostic description of this parameter block.</summary>
        ''' <returns>A text that reports the block length, the decay multipliers and the first few weight and gradient values.</returns>
        Public Overrides Function ToString() As String
            Return $"[len:{w.Length}] l1_decay_mul:{l1_decay_mul}, l2_decay_mul:{l2_decay_mul}; w:{w.Take(13).JoinBy(", ")}...; dw:{dw.Take(13).JoinBy(", ")}..."
        End Function

    End Class
End Namespace

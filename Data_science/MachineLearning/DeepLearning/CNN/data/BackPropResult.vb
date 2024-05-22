#Region "Microsoft.VisualBasic::e00d57299821bb7fae2ad026b3faa334, Data_science\MachineLearning\DeepLearning\CNN\data\BackPropResult.vb"

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

    '   Total Lines: 55
    '    Code Lines: 37 (67.27%)
    ' Comment Lines: 7 (12.73%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 11 (20.00%)
    '     File Size: 1.70 KB


    '     Class BackPropResult
    ' 
    '         Properties: Gradients, L1DecayMul, L2DecayMul, Weights
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

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

        Public Overridable ReadOnly Property L1DecayMul As Double
            Get
                Return l1_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property L2DecayMul As Double
            Get
                Return l2_decay_mul
            End Get
        End Property

        Public Overridable ReadOnly Property Weights As Double()
            Get
                Return w
            End Get
        End Property

        Public Overridable ReadOnly Property Gradients As Double()
            Get
                Return dw
            End Get
        End Property

        Public Sub New(w As Double(), dw As Double(), l1_decay_mul As Double, l2_decay_mul As Double)
            Me.w = w
            Me.dw = dw
            Me.l1_decay_mul = l1_decay_mul
            Me.l2_decay_mul = l2_decay_mul
        End Sub

        Public Overrides Function ToString() As String
            Return $"[len:{w.Length}] l1_decay_mul:{l1_decay_mul}, l2_decay_mul:{l2_decay_mul}; w:{w.Take(13).JoinBy(", ")}...; dw:{dw.Take(13).JoinBy(", ")}..."
        End Function

    End Class
End Namespace

#Region "Microsoft.VisualBasic::6f6612875dba27d66fd25f5896fb9dfc, Data_science\MachineLearning\DeepLearning\CNN\Layers\TanhLayer.vb"

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

    '   Total Lines: 60
    '    Code Lines: 42 (70.00%)
    ' Comment Lines: 7 (11.67%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 11 (18.33%)
    '     File Size: 1.92 KB


    '     Class TanhLayer
    ' 
    '         Properties: Type
    ' 
    '         Constructor: (+1 Overloads) Sub New
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
    ''' Implements Tanh nonlinearity elementwise x to tanh(x)
    ''' so the output is between -1 and 1.
    ''' 
    ''' @author Daniel Persson (mailto.woden@gmail.com)
    ''' </summary>
    Public Class TanhLayer : Inherits DataLink
        Implements Layer

        Public Overridable ReadOnly Iterator Property BackPropagationResult As IEnumerable(Of BackPropResult) Implements Layer.BackPropagationResult
            Get
                ' no data
            End Get
        End Property

        Public ReadOnly Property Type As LayerTypes Implements Layer.Type
            Get
                Return LayerTypes.Tanh
            End Get
        End Property

        Sub New()
        End Sub

        Public Overridable Function forward(db As DataBlock, training As Boolean) As DataBlock Implements Layer.forward
            Dim V2 As DataBlock = db.cloneAndZero()
            Dim N = db.Weights.Length

            in_act = db

            For i = 0 To N - 1
                V2.setWeight(i, std.Tanh(db.getWeight(i)))
            Next
            out_act = V2
            Return out_act
        End Function

        Public Overridable Sub backward() Implements Layer.backward
            Dim V = in_act ' we need to set dw of this
            Dim V2 = out_act
            Dim N = V.Weights.Length
            V.clearGradient() ' zero out gradient wrt data
            For i = 0 To N - 1
                Dim v2wi = V2.getWeight(i)
                V.setGradient(i, (1.0 - v2wi * v2wi) * V2.getGradient(i))
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return "tanh()"
        End Function
    End Class

End Namespace

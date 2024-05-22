#Region "Microsoft.VisualBasic::b4ee01f0cd8e646644cf857e11c6708c, Data_science\MachineLearning\DeepLearning\CNN\layers\LeakyReluLayer.vb"

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
    '    Code Lines: 45 (75.00%)
    ' Comment Lines: 1 (1.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 14 (23.33%)
    '     File Size: 1.76 KB


    '     Class LeakyReluLayer
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

Namespace CNN.layers

    Public Class LeakyReluLayer : Inherits RectifiedLinearUnitsLayer
        Implements Layer

        Public Overrides ReadOnly Property Type As LayerTypes
            Get
                Return LayerTypes.LeakyReLU
            End Get
        End Property

        Dim leakySlope As Double = 0.01

        Sub New(Optional leakySlope As Double = 0.01)
            Me.leakySlope = leakySlope
        End Sub

        Sub New()
        End Sub

        Public Overrides Function forward(db As DataBlock, training As Boolean) As DataBlock
            Dim V2 As DataBlock = db.clone()
            Dim N = db.Weights.Length
            Dim V2w = V2.Weights

            in_act = db

            For i As Integer = 0 To N - 1
                If V2w(i) < threshold Then
                    V2.setWeight(i, V2w(i) * leakySlope) ' threshold at 0
                End If
            Next

            out_act = V2

            Return out_act
        End Function

        Public Overrides Sub backward()
            ' zero out gradient wrt data
            Dim V = in_act.clearGradient() ' we need to set dw of this
            Dim V2 = out_act
            Dim N = V.Weights.Length

            For i As Integer = 0 To N - 1
                If V2.getWeight(i) <= threshold Then
                    V.setGradient(i, V2.getGradient(i) * leakySlope) ' threshold
                Else
                    V.setGradient(i, V2.getGradient(i))
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return "leaky_relu()"
        End Function
    End Class
End Namespace

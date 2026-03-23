#Region "Microsoft.VisualBasic::0d4e33708e6ae833dfb7ea9e853afb94, Data_science\MachineLearning\DeepLearning\CeNiN\Layers\SoftMax.vb"

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

    '   Total Lines: 45
    '    Code Lines: 34 (75.56%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (24.44%)
    '     File Size: 1.32 KB


    '     Class SoftMax
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: layerFeedNext
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace Convolutional

    Public Class SoftMax : Inherits Layer

        Public Overrides ReadOnly Property type As CNN.LayerTypes
            Get
                Return CNN.LayerTypes.SoftMax
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(inputTensorDims As Integer())
            Call MyBase.New(inputTensorDims)
        End Sub

        Protected Overrides Function layerFeedNext() As Layer
            Dim max = Single.MinValue

            For i As Integer = 0 To inputTensor.TotalLength - 1
                If inputTensor.Data(i) > max Then
                    max = inputTensor.Data(i)
                End If
            Next

            Dim sum As Single = 0
            Dim nLMR As Double() = nextLayer.inputTensor.Data

            For i As Integer = 0 To inputTensor.TotalLength - 1
                nLMR(i) = std.Exp(inputTensor.Data(i) - max)
                sum += nLMR(i)
            Next

            For i As Integer = 0 To inputTensor.TotalLength - 1
                nLMR(i) /= sum
            Next

            Call disposeInputTensor()

            Return Me
        End Function
    End Class
End Namespace

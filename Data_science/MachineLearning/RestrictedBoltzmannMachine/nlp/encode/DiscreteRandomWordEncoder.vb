#Region "Microsoft.VisualBasic::d460ac4c00e3f32c312da46af24885c8, Data_science\MachineLearning\RestrictedBoltzmannMachine\nlp\encode\DiscreteRandomWordEncoder.vb"

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

    '   Total Lines: 33
    '    Code Lines: 21 (63.64%)
    ' Comment Lines: 4 (12.12%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (24.24%)
    '     File Size: 955 B


    '     Class DiscreteRandomWordEncoder
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: encode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace nlp.encode

    ''' <summary>
    ''' Created by kenny on 6/3/14.
    ''' generate a random vector for a word
    ''' </summary>
    Public Class DiscreteRandomWordEncoder
        Implements WordEncoder

        Private ReadOnly dimensions As Integer

        Public Sub New()
            Me.New(20)
        End Sub

        Public Sub New(dimensions As Integer)
            Me.dimensions = dimensions
        End Sub

        Public Function encode(word As String) As DenseMatrix Implements WordEncoder.encode
            Dim matrix = DenseMatrix.make(1, dimensions)
            For i = 0 To dimensions - 1
                matrix.set(0, i, If(randf.NextGaussian() > 0, 1.0, 0.0))
            Next
            Return matrix
        End Function

    End Class

End Namespace

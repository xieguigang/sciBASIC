#Region "Microsoft.VisualBasic::ca582987afd8892d8497954ad1b6034a, Data_science\MachineLearning\RestrictedBoltzmannMachine\nlp\encode\RandomWordEncoder.vb"

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

    '   Total Lines: 34
    '    Code Lines: 21 (61.76%)
    ' Comment Lines: 4 (11.76%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (26.47%)
    '     File Size: 929 B


    '     Class RandomWordEncoder
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
    Public Class RandomWordEncoder
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
                matrix.set(0, i, randf.NextDouble())
            Next
            Return matrix
        End Function

    End Class

End Namespace

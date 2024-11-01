#Region "Microsoft.VisualBasic::ae30e77ce6c79356fb0c6678e0a65b76, Data_science\MachineLearning\DeepLearning\RNN\math\Random.vb"

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

    '   Total Lines: 54
    '    Code Lines: 27 (50.00%)
    ' Comment Lines: 17 (31.48%)
    '    - Xml Docs: 17.65%
    ' 
    '   Blank Lines: 10 (18.52%)
    '     File Size: 1.83 KB


    '     Class Random
    ' 
    '         Function: (+2 Overloads) randn, randomChoice, randomLike
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports rand = Microsoft.VisualBasic.Math.RandomExtensions

Namespace RNN

    ''' <summary>
    ''' Helper functions for randomness.
    ''' </summary>
    Public Class Random

        ' Random matrix

        ' Returns an MxN matrix filled with numbers drawn from a standard normal
        ' distribution.
        ' Requires that M > 0 and N > 0.
        Public Shared Function randn(M As Integer, N As Integer) As Matrix
            Dim lM = Matrix.zeros(M, N)
            lM.apply(Function(d) rand.NextDouble())
            Return lM
        End Function

        ' Returns an k-dimensional vector filled with numbers drawn from a standard
        ' normal distribution.
        ' Requires that k > 0.
        Public Shared Function randn(k As Integer) As Matrix
            Return randn(1, k)
        End Function

        ' Returns a matrix shaped like m filled with numbers drawn from a standard
        ' normal distribution.
        ' Requires that m != null.
        Public Shared Function randomLike(m As Matrix) As Matrix
            Return randn(m.M, m.N)
        End Function

        ' Random choice 

        ' Samples an index from a random distribution with the given probabilities
        ' p. Will work properly, if the sum of probabilities is 1.0.
        ' Requires that p != null.
        Public Shared Function randomChoice(p As Double()) As Integer
            Dim random As Double = rand.NextDouble()
            Dim cumulative = 0.0

            For i = 0 To p.Length - 1
                cumulative += p(i)
                If cumulative > random Then
                    Return i
                End If
            Next
            Return p.Length - 1 ' Fallback: probabilities did not sum up to a 1.0;
        End Function
    End Class

End Namespace

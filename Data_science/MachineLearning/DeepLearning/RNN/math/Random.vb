#Region "Microsoft.VisualBasic::8f05f9cd91e6ac6e1d540e8ba88bdca9, Data_science\MachineLearning\DeepLearning\RNN\math\Random.vb"

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

    '   Total Lines: 63
    '    Code Lines: 27 (42.86%)
    ' Comment Lines: 26 (41.27%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 10 (15.87%)
    '     File Size: 2.32 KB


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

        ''' <summary>
        ''' Creates an M x N matrix filled with random values.
        ''' </summary>
        ''' <param name="M">Number of rows; must be greater than zero.</param>
        ''' <param name="N">Number of columns; must be greater than zero.</param>
        ''' <returns>The random matrix.</returns>
        Public Shared Function randn(M As Integer, N As Integer) As Matrix
            Dim lM = Matrix.zeros(M, N)
            lM.apply(Function(d) rand.NextDouble())
            Return lM
        End Function

        ''' <summary>
        ''' Creates a k dimensional vector filled with random values.
        ''' </summary>
        ''' <param name="k">The vector length; must be greater than zero.</param>
        ''' <returns>The random vector.</returns>
        Public Shared Function randn(k As Integer) As Matrix
            Return randn(1, k)
        End Function

        ''' <summary>
        ''' Creates a matrix shaped like <paramref name="m"/> filled with random values.
        ''' </summary>
        ''' <param name="m">The template matrix; it must not be <c>Nothing</c>.</param>
        ''' <returns>The random matrix.</returns>
        Public Shared Function randomLike(m As Matrix) As Matrix
            Return randn(m.M, m.N)
        End Function

        ' Random choice 

        ''' <summary>
        ''' Samples an index from the discrete distribution given by <paramref name="p"/>.
        ''' </summary>
        ''' <param name="p">The probabilities; they should sum up to 1.0.</param>
        ''' <returns>The sampled index.</returns>
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

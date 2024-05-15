#Region "Microsoft.VisualBasic::d2f86b5d966d0eb86447e41dd618d393, Data_science\MachineLearning\RestrictedBoltzmannMachine\math\functions\distance\EuclideanDistanceFunction.vb"

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

    '   Total Lines: 23
    '    Code Lines: 15
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 729 B


    '     Class EuclideanDistanceFunction
    ' 
    '         Function: distance, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace math.functions.distance

    ''' <summary>
    ''' Created by kenny on 2/16/14.
    ''' </summary>
    Public Class EuclideanDistanceFunction
        Implements DistanceFunction

        Public Function distance(item1 As DenseMatrix, item2 As DenseMatrix) As Double Implements DistanceFunction.distance
            Dim sumSq As Double = 0
            For i = 0 To item1.columns() - 1
                sumSq += System.Math.Pow(item1.get(0, i) - item2.get(0, i), 2)
            Next
            Return System.Math.Sqrt(sumSq)
        End Function

        Public Overrides Function ToString() As String
            Return "EuclideanDistanceFunction"
        End Function

    End Class

End Namespace

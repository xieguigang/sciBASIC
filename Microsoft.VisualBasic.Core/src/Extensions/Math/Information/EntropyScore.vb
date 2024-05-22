#Region "Microsoft.VisualBasic::19fa0c055ab84fffbb32da43d8534268, Microsoft.VisualBasic.Core\src\Extensions\Math\Information\EntropyScore.vb"

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

    '   Total Lines: 49
    '    Code Lines: 31 (63.27%)
    ' Comment Lines: 10 (20.41%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 8 (16.33%)
    '     File Size: 1.73 KB


    '     Module EntropyScore
    ' 
    '         Function: DiffEntropy, Entropy, Mix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Math.Information

    ''' <summary>
    ''' Unweighted entropy similarity
    ''' </summary>
    Public Module EntropyScore

        <Extension>
        Private Function Entropy(v As Dictionary(Of String, Double)) As Double
            Dim sumAll As Double = v.Values.Sum
            Dim p As Double() = (From x As Double In v.Values Select x / sumAll).ToArray
            Dim s As Double = p.ShannonEntropy

            Return s
        End Function

        ''' <summary>
        ''' Unweighted entropy similarity
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DiffEntropy(v1 As Dictionary(Of String, Double), v2 As Dictionary(Of String, Double)) As Double
            Dim SAB = Mix(v1, v2).Entropy
            Dim SA = v1.Entropy
            Dim SB = v2.Entropy
            ' Unweighted entropy similarity
            Dim s As Double = 1 - (2 * SAB - SA - SB) / stdNum.Log(4)

            Return If(s < 0, 0, s)
        End Function

        Private Function Mix(v1 As Dictionary(Of String, Double), v2 As Dictionary(Of String, Double)) As Dictionary(Of String, Double)
            Return v1 _
                .JoinIterates(v2) _
                .GroupBy(Function(d) d.Key) _
                .ToDictionary(Function(d) d.Key,
                              Function(d)
                                  Return Aggregate x In d Into Sum(x.Value)
                              End Function)
        End Function

    End Module
End Namespace

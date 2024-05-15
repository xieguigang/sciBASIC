#Region "Microsoft.VisualBasic::f3b45212a8c2337a550473f582945d20, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\MantelTest\corr.vb"

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

    '   Total Lines: 41
    '    Code Lines: 33
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.61 KB


    '     Module corr
    ' 
    '         Function: GetCorrelations, Pearson
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Hypothesis.Mantel

    Public Module corr

        <Extension>
        Public Function GetCorrelations(mat As Double()(), cor As Func(Of Double(), Double(), (cor As Double, pvalue As Double))) As (cor As Double()(), pvalue As Double()())
            Dim evalData = mat _
                .SeqIterator _
                .AsParallel _
                .Select(Function(d)
                            Dim vec As New List(Of Double)
                            Dim vec2 As New List(Of Double)
                            Dim x As Double() = d.value

                            For Each y As Double() In mat
                                vec += cor(x, y).cor
                                vec2 += cor(x, y).pvalue
                            Next

                            Return (d.i, vec.ToArray, vec2.ToArray)
                        End Function) _
                .OrderBy(Function(d) d.i) _
                .ToArray
            Dim corr As Double()() = evalData.Select(Function(i) i.Item2).ToArray
            Dim pvalue As Double()() = evalData.Select(Function(i) i.Item3).ToArray

            Return (corr, pvalue)
        End Function

        Public Function Pearson(x As Double(), y As Double()) As (cor#, pvalue#)
            Dim pvalue As Double
            Dim corVal = Correlations.GetPearson(x, y, prob:=pvalue, throwMaxIterError:=False)

            Return (corVal, pvalue)
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::b88522aead5d0145d6c676083dc729f7, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\FishersExact\FishersExactPvalues.vb"

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

    '   Total Lines: 51
    '    Code Lines: 22
    ' Comment Lines: 23
    '   Blank Lines: 6
    '     File Size: 1.80 KB


    '     Class FishersExactPvalues
    ' 
    '         Properties: greater_pvalue, hyper_state, less_pvalue, matrix, two_tail_pvalue
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Hypothesis.FishersExact

    ''' <summary>
    ''' `FishersExactPvalues` holds the pvalues calculated by the `fishers_exact` function.
    ''' </summary>
    Public Class FishersExactPvalues

        ''' <summary>
        ''' pvalue for the two-tailed test. Use this when there Is no prior alternative.
        ''' </summary>
        ''' <returns></returns>
        Public Property two_tail_pvalue As Double
        ''' <summary>
        ''' pvalue for the "left" Or "lesser" tail. Use this when the alternative to
        ''' independence Is that there Is negative association between the variables.
        ''' That Is, the observations tend to lie in lower left And upper right.
        ''' </summary>
        ''' <returns></returns>
        Public Property less_pvalue As Double
        ''' <summary>
        ''' Use this when the alternative to independence Is that there Is positive
        ''' association between the variables. That Is, the observations tend to lie
        ''' in upper left And lower right.
        ''' </summary>
        ''' <returns></returns>
        Public Property greater_pvalue As Double

        ''' <summary>
        ''' [a,b,c,d]
        ''' </summary>
        ''' <returns></returns>
        Public Property matrix As Integer()

        Public Property hyper_state As HyperState

        Public Overrides Function ToString() As String
            Return $"
	Fisher's Exact Test for Count Data

data:  [a={matrix(0)}, b={matrix(1)}, c={matrix(2)}, d={matrix(3)}]
p-value = {two_tail_pvalue}
alternative hypothesis: true odds ratio is not equal to 1
95 percent confidence interval:
  0.008512238 20.296715040
sample estimates:
odds ratio 
  0.693793 
"
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::9da705021af3870ca621f4feee5aad29, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\TwoSampleResult.vb"

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

    '   Total Lines: 28
    '    Code Lines: 21
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 794 B


    '     Class TwoSampleResult
    ' 
    '         Properties: MeanX, MeanY, y
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Hypothesis

    Public Class TwoSampleResult : Inherits TtestResult

        Public Property MeanX As Double
        Public Property MeanY As Double

        Public Property y As Double()

        Public Overrides Function ToString() As String
            Dim ci95 = Me.ci95

            Return $"
	Welch Two Sample t-test

data:  {x.GetJson} and {y.GetJson}
t = {TestValue}, df = {DegreeFreedom}, p-value <= {Pvalue}
alternative hypothesis: {Valid.ToString.ToUpper} difference in means is {opt.alternative.Description} {Mean}
{(1 - opt.alpha) * 100} percent confidence interval:
 {ci95(0)} {ci95(1)}
sample estimates:
mean of x mean of y 
 {MeanX}  {MeanY}"
        End Function
    End Class
End Namespace

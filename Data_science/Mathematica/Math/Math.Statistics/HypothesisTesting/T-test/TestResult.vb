#Region "Microsoft.VisualBasic::28c6a9b9281bc747c3228de8c183862a, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\TestResult.vb"

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

    '     Class TtestResult
    ' 
    '         Properties: alpha, alternative, ci95, DegreeFreedom, Mean
    '                     Pvalue, StdErr, TestValue, x
    ' 
    '         Function: ToString, Valid
    ' 
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

    Public Class TtestResult

        ''' <summary>
        ''' the degrees of freedom for the t-statistic.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Welch–Satterthwaite equation 的计算结果为小数</remarks>
        Public Property DegreeFreedom As Double
        ''' <summary>
        ''' the p-value For the test.
        ''' </summary>
        ''' <returns></returns>
        Public Property Pvalue As Double
        ''' <summary>
        ''' the value of the t-statistic.
        ''' </summary>
        ''' <returns></returns>
        Public Property TestValue As Double
        ''' <summary>
        ''' the alternative hypothesis.
        ''' </summary>
        ''' <returns></returns>
        Public Property alternative As Hypothesis
        Public Property alpha As Double
        ''' <summary>
        ''' Sample mean
        ''' </summary>
        ''' <returns></returns>
        Public Property Mean As Double
        Public Property StdErr As Double

        Public Property x As Double()

        Public ReadOnly Property ci95 As Double()
            Get
                Dim interval = Math.Statistics.CI95(Mean, StdErr, x.Length)
                Dim ci = {interval.Min, interval.Max}

                Return ci
            End Get
        End Property

        ''' <summary>
        ''' Alternative hypothesis result
        ''' </summary>
        ''' <returns></returns>
        Public Function Valid() As Boolean
            Return Pvalue >= alpha
        End Function

        Public Overrides Function ToString() As String
            Dim ci95 = Me.ci95

            Return $"
	One Sample t-test

data:  {x.GetJson}
t = {TestValue}, df = {DegreeFreedom}, p-value = {Pvalue}
alternative hypothesis: {Valid.ToString.ToUpper} mean is not equal to 0
{(1 - alpha) * 100} percent confidence interval:
 {ci95.Min}  {ci95.Max}
sample estimates:
   mean of x 
{Mean} "
        End Function
    End Class

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
alternative hypothesis: {Valid.ToString.ToUpper} difference in means is not equal to {MeanX}
{(1 - alpha) * 100} percent confidence interval:
 {ci95(0)} {ci95(1)}
sample estimates:
mean of x mean of y 
 {MeanX}  {MeanY}"
        End Function
    End Class
End Namespace

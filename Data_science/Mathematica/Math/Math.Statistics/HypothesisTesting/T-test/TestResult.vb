#Region "Microsoft.VisualBasic::bda9c5aa85929f36dbf2635386dd758f, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\TestResult.vb"

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
    '         Properties: ci95, DegreeFreedom, Mean, opt, Pvalue
    '                     SD, StdErr, TestValue, x
    ' 
    '         Function: ToString, Valid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

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

        Public Property opt As Topt

        ''' <summary>
        ''' Sample mean
        ''' </summary>
        ''' <returns></returns>
        Public Property Mean As Double
        Public Property StdErr As Double
        Public Property SD As Double

        Public Property x As Double()

        Public ReadOnly Property ci95 As Double()
            Get
                Dim interval = Math.Statistics.CI95(Mean, SD, x.Length)
                Dim ci = {interval.Min, interval.Max}

                Return ci
                'Dim pm As Double
                'Dim dist As New StudenttDistribution(DegreeFreedom)

                'Select Case opt.alternative
                '    Case Hypothesis.Greater     ' mu > mu[0]
                '        pm = stdNum.Abs(dist.inv(opt.alpha)) * StdErr
                '        Return {Mean - pm, Double.PositiveInfinity}
                '    Case Hypothesis.Less  ' mu < mu[0]
                '        pm = stdNum.Abs(dist.inv(opt.alpha)) * StdErr
                '        Return {Double.NegativeInfinity, Mean + pm}
                '    Case Else ' mu != mu[0]
                '        pm = stdNum.Abs(dist.inv(opt.alpha / 2)) * StdErr
                '        Return {Mean - pm, Mean + pm}
                'End Select
            End Get
        End Property

        ''' <summary>
        ''' Alternative hypothesis result
        ''' </summary>
        ''' <returns></returns>
        Public Function Valid() As Boolean
            Return Pvalue >= opt.alpha
        End Function

        Public Overrides Function ToString() As String
            Dim ci95 = Me.ci95

            Return $"
	One Sample t-test

data:  {x.GetJson}
t = {TestValue}, df = {DegreeFreedom}, p-value = {Pvalue}
alternative hypothesis: {Valid.ToString.ToUpper} mean is {opt.alternative.Description} {opt.mu}
{(1 - opt.alpha) * 100} percent confidence interval:
 {ci95.Min}  {ci95.Max}
sample estimates:
   mean of x 
{Mean} "
        End Function
    End Class
End Namespace

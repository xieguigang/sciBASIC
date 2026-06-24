#Region "Microsoft.VisualBasic::5445d12c16181cb867c118af7a64c15a, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\FTest.vb"

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

    '   Total Lines: 149
    '    Code Lines: 99 (66.44%)
    ' Comment Lines: 24 (16.11%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 26 (17.45%)
    '     File Size: 4.52 KB


    ' Class FTest
    ' 
    '     Properties: ConfidenceInterval95, F, PValue, SingleTailedPval, XdegreeOfFreedom
    '                 XVariance, YdegreeOfFreedom, YVariance
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math.Statistics.Distributions
Imports std = System.Math

Public Class FTest

    Dim x, y As Double()

    ' 确保定义这些常量
    Private Const F_EPSILON As Double = 0.0000000000000001
    Private Const I_PI As Double = std.PI / 2.0

    <Field("freedom1")>
    Public ReadOnly Property XdegreeOfFreedom As Integer
        Get
            Return x.Length - 1
        End Get
    End Property

    <Field("freedom2")>
    Public ReadOnly Property YdegreeOfFreedom As Integer
        Get
            Return y.Length - 1
        End Get
    End Property

    <Field("var.x")>
    Public ReadOnly Property XVariance As Double
        Get
            Return x.SD ^ 2
        End Get
    End Property

    <Field("var.y")>
    Public ReadOnly Property YVariance As Double
        Get
            Return y.SD ^ 2
        End Get
    End Property

    ''' <summary>
    ''' ratio of variances 
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Field("F")>
    Public ReadOnly Property F As Double
        Get
            Dim q As Double = XVariance
            Dim p As Double = YVariance

            ' 直接使用方差相除
            Return q / p
        End Get
    End Property

    <Field("single_tailed p-value")>
    Public ReadOnly Property SingleTailedPval As Double
        Get
            Dim fRatio As Double = Me.F
            Dim df1 As Integer = XdegreeOfFreedom
            Dim df2 As Integer = YdegreeOfFreedom

            ' 统计学标准处理：保证 F >= 1 以提高数值稳定性并正确计算双尾P值
            If fRatio < 1.0 Then
                fRatio = 1.0 / fRatio
                ' 交换自由度
                df1.Swap(df2)
            End If

            Dim single_tailed_pval As Double = Distribution.FDistribution(
                fValue:=fRatio,
                freedom1:=df1,
                freedom2:=df2
            )

            Return single_tailed_pval
        End Get
    End Property

    ''' <summary>
    ''' double tailed p-value
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <Field("double_tailed p-value")>
    Public ReadOnly Property PValue As Double
        Get
            ' 限制最大值为 1.0
            Return std.Min(1.0, SingleTailedPval * 2)
        End Get
    End Property

    ''' <summary>
    ''' 获取 95% 置信区间
    ''' </summary>
    ''' <returns>包含下限和上限的 Tuple</returns>
    Public ReadOnly Property ConfidenceInterval95 As DoubleRange
        Get
            Dim alpha As Double = 0.05
            Dim df1 As Integer = XdegreeOfFreedom
            Dim df2 As Integer = YdegreeOfFreedom
            Dim fRatio As Double = Me.F

            ' 我们需要找到右尾面积为 alpha/2 (即0.025) 时的 F 临界值
            Dim upperTailProb As Double = alpha / 2.0

            ' 求上限临界值 F_{0.975, df1, df2} (即右侧面积为0.025的点)
            Dim F_upper_df1_df2 As Double = Distribution.FDistributionInverse(upperTailProb, df1, df2)

            ' 求上限临界值 F_{0.975, df2, df1} (注意这里自由度交换了)
            Dim F_upper_df2_df1 As Double = Distribution.FDistributionInverse(upperTailProb, df2, df1)

            ' 计算置信区间下限
            Dim lower As Double = fRatio / F_upper_df1_df2

            ' 计算置信区间上限 (利用了 F_{0.025, df1, df2} = 1 / F_{0.975, df2, df1} 的性质)
            Dim upper As Double = fRatio * F_upper_df2_df1

            Return New DoubleRange(lower, upper)
        End Get
    End Property

    Sub New(x As Double(), y As Double())
        Me.x = x
        Me.y = y
    End Sub

    Public Overrides Function ToString() As String
        Dim reject As String = "true ratio of variances is not equal to 1"
        Dim accept As String = "true ratio of variances is equal to 1"
        Dim ci95 = ConfidenceInterval95

        Return $"

	F test to compare two variances

data:  x and y
F = {F}, num df = {XdegreeOfFreedom}, denom df = {YdegreeOfFreedom}, p-value = {PValue}
alternative hypothesis: {If(PValue < 0.05, reject, accept)}
95 percent confidence interval:
  {ci95.Min} {ci95.Max}
sample estimates:
ratio of variances 
          {F}
"
    End Function
End Class

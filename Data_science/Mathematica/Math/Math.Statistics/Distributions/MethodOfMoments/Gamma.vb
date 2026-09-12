#Region "Microsoft.VisualBasic::2e8412f640e1a73de07d4e5f89194975, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Gamma.vb"

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

    '   Total Lines: 64
    '    Code Lines: 41 (64.06%)
    ' Comment Lines: 11 (17.19%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 12 (18.75%)
    '     File Size: 2.27 KB


    '     Class Gamma
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.MethodOfMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Gamma : Inherits ContinuousDistribution

        Private _Alpha As Double
        Private _Beta As Double

        Public Sub New()
            'for reflection
            _Alpha = 0
            _Beta = 0
        End Sub

        Public Sub New(data As Double())
            'http://www.itl.nist.gov/div898/handbook/eda/section3/eda366b.htm
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Alpha = std.Pow((BPM.Mean() / BPM.StDev()), 2)
            _Beta = 1 / (BPM.StDev() / BPM.Mean())
            PeriodOfRecord = (BPM.SampleSize())
        End Sub

        Public Sub New(Alpha As Double, Beta As Double)
            _Alpha = Alpha
            _Beta = Beta
        End Sub

        ''' <summary>
        ''' 使用牛顿迭代法求解 Gamma 分布的逆累积分布函数（分位数）。
        ''' </summary>
        ''' <remarks>
        ''' BUG FIX: 原实现的循环条件 <c>|cdf - p| &lt;= eps Or i = 100</c> 是**反的** ——
        ''' 它会在估计值已经足够精确（或恰好迭代到第 100 次）时继续循环，反而在尚未收敛时立即退出，
        ''' 因此通常只做了一两次迭代就返回，分位数结果不可靠。这里改为标准的收敛判据
        ''' （未收敛且迭代次数未用尽时继续），并加入 pdf=0 与越界保护。
        ''' </remarks>
        Public Overrides Function GetInvCDF(probability As Double) As Double
            If probability <= 0 Then
                Return 0
            ElseIf probability >= 1 Then
                Return Double.PositiveInfinity
            End If

            Dim xn As Double = _Alpha / _Beta
            Dim i As Integer = 0

            Do While i < 100
                Dim cdf As Double = GetCDF(xn)
                Dim diff As Double = cdf - probability

                If std.Abs(diff) <= 0.00000000000001 Then
                    Exit Do
                End If

                Dim pdf As Double = GetPDF(xn)

                If pdf <= 0 Then
                    Exit Do
                End If

                Dim nextValue As Double = xn - diff / pdf

                If nextValue <= 0 Then
                    ' 保持解在正数域内
                    nextValue = xn / 2
                End If

                xn = nextValue
                i += 1
            Loop

            Return xn
        End Function

        Public Overrides Function GetCDF(value As Double) As Double
            Return SpecialFunctions.IncompleteGamma(_Alpha, _Beta * value) / std.Exp(SpecialFunctions.gammaln(_Alpha))
        End Function

        Public Overrides Function GetPDF(value As Double) As Double
            Return (((std.Pow(_Beta, _Alpha)) * ((std.Pow(value, _Alpha - 1)) * std.Exp(-_Beta * value)) / std.Exp(SpecialFunctions.gammaln(_Alpha))))
        End Function

        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Beta <= 0 Then Yield New Exception("Beta must be greater than 0")
        End Function
    End Class

End Namespace

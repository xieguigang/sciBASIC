#Region "Microsoft.VisualBasic::26e005b70610addc80f083157c45cab9, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Gamma.vb"

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

    '   Total Lines: 58
    '    Code Lines: 42
    ' Comment Lines: 11
    '   Blank Lines: 5
    '     File Size: 2.30 KB


    '     Class Gamma
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

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
    Public Class Gamma
        Inherits Distributions.ContinuousDistribution

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
            _Alpha = stdNum.Pow((BPM.Mean() / BPM.StDev()), 2)
            _Beta = 1 / (BPM.StDev() / BPM.Mean())
            PeriodOfRecord = (BPM.SampleSize())
        End Sub
        Public Sub New(Alpha As Double, Beta As Double)
            _Alpha = Alpha
            _Beta = Beta
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim xn As Double = _Alpha / _Beta
            Dim testvalue As Double = GetCDF(xn)
            Dim i As Integer = 0
            Do
                xn = xn - ((testvalue - probability) / GetPDF(xn))
                testvalue = GetCDF(xn)
                i += 1
            Loop While stdNum.Abs(testvalue - probability) <= 0.00000000000001 Or i = 100
            Return xn
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return SpecialFunctions.IncompleteGamma(_Alpha, _Beta * value) / stdNum.Exp(SpecialFunctions.gammaln(_Alpha))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (((stdNum.Pow(_Beta, _Alpha)) * ((stdNum.Pow(value, _Alpha - 1)) * stdNum.Exp(-_Beta * value)) / stdNum.Exp(SpecialFunctions.gammaln(_Alpha))))
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Beta <= 0 Then Yield New Exception("Beta must be greater than 0")
        End Function
    End Class

End Namespace

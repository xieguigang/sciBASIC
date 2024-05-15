#Region "Microsoft.VisualBasic::968f8036199258ba62fae5595d4e86e5, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Logistic.vb"

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
    '    Code Lines: 49
    ' Comment Lines: 10
    '   Blank Lines: 5
    '     File Size: 2.37 KB


    '     Class Logistic
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate, Y
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
Namespace Distributions.LinearMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Logistic
        Inherits Distributions.ContinuousDistribution

        Private _K As Double
        Private _Alpha As Double
        Private _Xi As Double
        Public Sub New()
            'for reflection
            _K = 0
            _Alpha = 0
            _Xi = 0
        End Sub
        Public Sub New(data As Double())
            Dim LM As New MomentFunctions.LinearMoments(data)
            _K = -LM.T3()
            _Alpha = LM.L2() * stdNum.Sin(_K * stdNum.PI) / (_K * stdNum.PI)
            _Xi = LM.L1() - _Alpha * ((1 / _K) - (stdNum.PI / stdNum.Sin(_K * stdNum.PI)))
            PeriodOfRecord = (LM.SampleSize())
        End Sub
        Public Sub New(K As Double, Alpha As Double, Xi As Double)
            _K = K
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            If _K <> 0 Then
                Return _Xi + _Alpha * (1 - (stdNum.Pow((1 - probability) / probability, _K))) / _K
            Else
                Return _Xi - _Alpha * (stdNum.Log((1 - probability) / probability))
            End If
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return (1 / (1 + stdNum.Exp(-Y(value))))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * stdNum.Exp(-(1 - _K) * Y(value)) / (stdNum.Pow((1 + stdNum.Exp(-Y(value))), 2))
        End Function
        Private Function Y(value As Double) As Double
            If _K <> 0 Then
                Return (1 / -_K) * stdNum.Log(1 - _K * (value - _Xi) / _Alpha)
            Else
                Return (value - _Xi) / _Alpha
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Alpha = 0 Then Yield New Exception("Alpha cannot be zero")
        End Function
    End Class

End Namespace

#Region "Microsoft.VisualBasic::9ba0188a43ca2000189a3c7bb69c7234, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Pareto.vb"

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

    '   Total Lines: 68
    '    Code Lines: 52 (76.47%)
    ' Comment Lines: 11 (16.18%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 5 (7.35%)
    '     File Size: 2.39 KB


    '     Class Pareto
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
    Public Class Pareto
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
            If LM.L2() = 0 Then
                _K = (1 - 3 * LM.T3()) / (1 + LM.T3())
                _Alpha = (1 + _K) * (2 + _K) * LM.L2()
                _Xi = LM.L1() - (2 + _K) * LM.L2()
                PeriodOfRecord = (LM.SampleSize())
            Else
                'coefficient of variation cannot be zero.
            End If
        End Sub
        Public Sub New(K As Double, Alpha As Double, Xi As Double)
            _K = K
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            If _K <> 0 Then
                Return _Xi + (_Alpha * (1 - stdNum.Pow(1 - probability, _K)) / _K)
            Else
                Return _Xi - _Alpha * stdNum.Log(1 - probability)
            End If
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return 1 - stdNum.Exp(-Y(value))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * stdNum.Exp(-(1 - _K) * Y(value))
        End Function
        Public Overridable Function Y(value As Double) As Double
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

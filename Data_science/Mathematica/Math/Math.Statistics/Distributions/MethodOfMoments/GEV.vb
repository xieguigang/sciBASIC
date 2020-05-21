#Region "Microsoft.VisualBasic::a44ed6387d2d0ee2994a535ede7ec810, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\GEV.vb"

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

    '     Class GEV
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, T, Tinv
    '                   Validate
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
    ''' <remarks>
    ''' https://en.wikipedia.org/wiki/Generalized_extreme_value_distribution
    ''' </remarks>
    Public Class GEV : Inherits Distributions.ContinuousDistribution

        Private _Mu As Double
        Private _Sigma As Double
        Private _Xi As Double
        Public Sub New(mu As Double, sigma As Double, xi As Double)
            _Mu = mu
            _Sigma = sigma
            _Xi = xi
        End Sub
        Public Sub New(data As Double())
            'not implemented
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            If probability <= 0 Then
                If _Xi > 0 Then
                    Return _Mu - _Sigma / _Xi
                Else
                    Return Double.NegativeInfinity
                End If
            ElseIf probability >= 1 Then
                If _Xi < 0 Then
                    Return _Mu - _Sigma / _Xi
                Else
                    Return Double.PositiveInfinity
                End If
            End If
            Return Tinv(probability)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            If _Xi > 0 AndAlso value <= _Mu - _Sigma / _Xi Then Return 0
            If _Xi < 0 AndAlso value >= _Mu - _Sigma / _Xi Then Return 1
            Return stdNum.Exp(-T(value))
        End Function

        Public Overrides Function GetPDF(value As Double) As Double
            If _Xi > 0 AndAlso value <= _Mu - _Sigma / _Xi Then Return 0
            If _Xi < 0 AndAlso value >= _Mu - _Sigma / _Xi Then Return 0
            Dim tx As Double = T(value)
            Return (1 / _Sigma) * stdNum.Pow(tx, _Xi + 1) * stdNum.Exp(-tx)
        End Function
        Private Function T(x As Double) As Double
            If _Xi <> 0 Then
                Return stdNum.Pow((1 + ((x - _Mu) / _Sigma) * _Xi), -1 / _Xi)
            Else
                Return stdNum.Exp(-(x - _Mu) / _Sigma)
            End If
        End Function
        Private Function Tinv(probability As Double) As Double
            If _Xi <> 0 Then
                Return _Mu - _Sigma * (stdNum.Pow(stdNum.Log(1 / probability), _Xi) - 1) / _Xi
            Else
                Return _Mu - _Sigma * stdNum.Log(stdNum.Log(1 / probability))
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Xi <= 0 Then Yield New Exception("Xi must be greater than 0")
            If _Sigma <= 0 Then Yield New Exception("Sigma must be greater than 0")
        End Function
    End Class

End Namespace

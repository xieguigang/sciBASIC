#Region "Microsoft.VisualBasic::74f140179d51c0648f62000f6f7b2193, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Uniform.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.MethodOfMoments

    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Uniform
        Inherits Distributions.ContinuousDistribution

        Private _Min As Double
        Private _Max As Double
        Public Overridable Function GetMin() As Double
            Return _Min
        End Function
        Public Overridable Function GetMax() As Double
            Return _Max
        End Function
        Public Sub New()
            'for reflection
            _Min = 0
            _Max = 0
        End Sub
        Public Sub New(min As Double, max As Double)
            _Min = min
            _Max = max
        End Sub
        Public Sub New(data As Double())
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Min = BPM.Min()
            _Max = BPM.Max()
            PeriodOfRecord = (BPM.SampleSize())
            'alternative method
            'double dist = java.lang.Math.sqrt(3*BPM.GetSampleVariance());
            '_Min = BPM.GetMean() - dist;
            '_Max = BPM.GetMean() + dist;
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Return _Min + ((_Max - _Min) * probability)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            If value < _Min Then
                Return 0
            ElseIf value <= _Max Then
                Return (value - _Min) / (_Min - _Max)
            Else
                Return 1
            End If
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            If value < _Min Then
                Return 0
            ElseIf value <= _Max Then
                Return 1 / (_Max - _Min)
            Else
                Return 0
            End If
        End Function
        Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
            Dim errs As New List(Of Distributions.ContinuousDistributionError)
            If _Min > _Max Then errs.Add(New Distributions.ContinuousDistributionError("The min cannot be greater than the max in the uniform distribuiton."))
            Return errs
        End Function
    End Class

End Namespace

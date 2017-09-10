#Region "Microsoft.VisualBasic::f1aa8b464f471b1f9d2682a03ec63cde, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Triangular.vb"

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

Imports System
Imports System.Collections.Generic

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
	Public Class Triangular
		Inherits Distributions.ContinuousDistribution

		Private _Min As Double
		Private _Max As Double
		Private _MostLikely As Double
		Public Overridable Function GetMin() As Double
			Return _Min
		End Function
		Public Overridable Function GetMax() As Double
			Return _Max
		End Function
		Public Overridable Function GetMostLikely() As Double
			Return _MostLikely
		End Function
		Public Sub New()
			'_SampleSize = 0; for reflection
			_Min = 0
			_Max = 0
			_MostLikely = 0
		End Sub
		Public Sub New( min As Double,  max As Double,  mostlikely As Double)
			'_SampleSize = 0;
			_Min = min
			_Max = max
			_MostLikely = mostlikely
		End Sub
		Public Sub New( data As Double())
			Dim PM As New MomentFunctions.ProductMoments(data)
			'Simple Method:
	'        _Min = PM.GetMin();
	'        _Max = PM.GetMax();
	'        _MostLikely = PM.GetMean();

			'Alternate Method from Ben Chaon
			Dim sqrt2 As Double = Math.Sqrt(2)
			Dim sqrt3 As Double = Math.Sqrt(3)
            Dim a3 As Double = PM.Skew()
            Dim b3 As Double
			Dim angle As Double
			Dim aa As Double
			Dim bb As Double
			If 8-a3*a3<0 Then
				a3 = Math.Sin(a3)*2*sqrt2
				b3 = 0
			Else
				b3 = Math.Sqrt(8-a3*a3)
			End If
			angle = Math.Atan2(b3, a3)
			aa = Math.Cos(angle/3.0)
			bb = Math.Sin(angle/3.0)
            _Min = (PM.Mean() + sqrt2 * PM.StandardDeviation() * (aa - sqrt3 * bb))
            _MostLikely = (PM.Mean() - 2 * sqrt2 * PM.StandardDeviation() * aa)
            _Max = (PM.Mean() + sqrt2 * PM.StandardDeviation() * (aa + sqrt3 * bb))
            PeriodOfRecord = (PM.SampleSize())
        End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Dim a As Double = _MostLikely - _Min
			Dim b As Double = _Max - _MostLikely
			If probability <= 0 Then
				Return _Min
			ElseIf probability < (a/(_Max - _Min)) Then
				Return _Min + Math.Sqrt(probability * (_Max - _Min) * a)
			ElseIf probability < 1 Then
				Return _Max - Math.Sqrt((1-probability)*(_Max - _Min)* b)
			Else
				Return _Max
			End If
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			If value<_Min Then Return 0
			If value<_MostLikely Then Return (Math.Pow((value-_Min),2)/(_Max-_Min)*(_MostLikely-_Min))
			If value<=_Max Then Return 1-(Math.Pow((_Max-value),2)/(_Max-_Min)*(_Max-_MostLikely))
			Return 1
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			If value<_Min Then Return 0
			If value<_MostLikely Then Return (2*(value-_Min)/(_Max-_Min)*(_MostLikely-_Min))
			If value<=_Max Then Return (2*(_Max-value)/(_Max-_Min)*(_Max-_MostLikely))
			Return 0
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Min>_Max Then errors.Add(New Distributions.ContinuousDistributionError("Min cannot be greater than Max"))
			If _Min>_MostLikely Then errors.Add(New Distributions.ContinuousDistributionError("Min cannot be greater than MostLikely"))
			If _MostLikely>_Max Then errors.Add(New Distributions.ContinuousDistributionError("MostLikely cannot be greater than Max"))
			Return errors
		End Function
	End Class

End Namespace

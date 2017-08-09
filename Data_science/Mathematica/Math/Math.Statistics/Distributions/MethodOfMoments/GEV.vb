#Region "Microsoft.VisualBasic::276583d0212be1c905ce48664176d898, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\GEV.vb"

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
	Public Class GEV
		Inherits Distributions.ContinuousDistribution

		Private _Mu As Double 'https://en.wikipedia.org/wiki/Generalized_extreme_value_distribution
		Private _Sigma As Double
		Private _Xi As Double
		Public Sub New( mu As Double,  sigma As Double,  xi As Double)
			_Mu = mu
			_Sigma = sigma
			_Xi = xi
		End Sub
		Public Sub New( data As Double())
			'not implemented
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			If probability<=0 Then
				If _Xi>0 Then
					Return _Mu-_Sigma/_Xi
				Else
					Return Double.NegativeInfinity
				End If
			ElseIf probability>=1 Then
				If _Xi<0 Then
					Return _Mu-_Sigma/_Xi
				Else
					Return Double.PositiveInfinity
				End If
			End If
			Return Tinv(probability)
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			If _Xi>0 AndAlso value <= _Mu-_Sigma/_Xi Then Return 0
			If _Xi<0 AndAlso value >= _Mu-_Sigma/_Xi Then Return 1
			Return Math.Exp(-T(value))
		End Function

		Public Overrides Function GetPDF( value As Double) As Double
			If _Xi>0 AndAlso value <= _Mu-_Sigma/_Xi Then Return 0
			If _Xi<0 AndAlso value >= _Mu-_Sigma/_Xi Then Return 0
			Dim tx As Double = T(value)
			Return (1/_Sigma)*Math.Pow(tx, _Xi+1)*Math.Exp(-tx)
		End Function
		Private Function T( x As Double) As Double
			If _Xi<>0 Then
				Return Math.Pow((1+((x-_Mu)/_Sigma)*_Xi),-1/_Xi)
			Else
				Return Math.Exp(-(x-_Mu)/_Sigma)
			End If
		End Function
		Private Function Tinv( probability As Double) As Double
			If _Xi<>0 Then
				Return _Mu-_Sigma*(Math.Pow(Math.Log(1/probability), _Xi)-1)/_Xi
			Else
				Return _Mu-_Sigma*Math.Log(Math.Log(1/probability))
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Xi<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Xi must be greater than 0"))
			If _Sigma<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Sigma must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace

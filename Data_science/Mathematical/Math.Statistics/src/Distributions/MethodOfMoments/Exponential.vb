#Region "Microsoft.VisualBasic::dfca08bbc2d6df771bef05977bef4b43, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\src\Distributions\MethodOfMoments\Exponential.vb"

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
	Public Class Exponential
		Inherits Distributions.ContinuousDistribution

		Private _Lambda As Double
		Public Sub New()
			'for reflection
			_Lambda = 1
		End Sub
		Public Sub New( lambda As Double)
			_Lambda = lambda
		End Sub
		Public Sub New( data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Lambda = 1/BPM.GetMean()
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Return Math.Log(probability)/_Lambda
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Return 1- Math.Exp(-_Lambda*value)
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			If value<0 Then
				Return 0
			Else
				Return _Lambda * Math.Exp(-_Lambda*value)
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Lambda<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Lambda must be greater than 0"))
			Return errors
		End Function

	End Class

End Namespace

#Region "Microsoft.VisualBasic::3e0e9e85d128a939c33afbef7b502980, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\Exponential.vb"

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
Namespace Distributions.LinearMoments


	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class Exponential
		Inherits Distributions.ContinuousDistribution

		Private _Alpha As Double
		Private _Xi As Double
		Public Sub New()
			'for reflection
			_Alpha = 0
			_Xi = 0
		End Sub
		Public Sub New( data As Double())
			Dim LM As New MomentFunctions.LinearMoments(data)
			_Alpha = 2 * LM.GetL2()
			_Xi = LM.GetL1() - _Alpha
			PeriodOfRecord = (LM.GetSampleSize())
		End Sub
		Public Sub New( Alpha As Double,  Xi As Double)
			_Alpha = Alpha
			_Xi = Xi
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Return _Xi - _Alpha * Math.Log(1 - probability)
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Return 1 - Math.Exp(-(value - _Xi) / _Alpha)
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Return (1/_Alpha) * Math.Exp(-(value - _Xi) / _Alpha)
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Alpha = 0 Then errors.Add(New Distributions.ContinuousDistributionError("Alpha cannot be zero"))
			Return errors
		End Function
	End Class

End Namespace

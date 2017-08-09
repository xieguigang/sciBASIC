#Region "Microsoft.VisualBasic::1c59cd022a9f370e501c202d98840830, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Rayleigh.vb"

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
	Public Class Rayleigh
		Inherits Distributions.ContinuousDistribution

		Private _Sigma As Double
		Public Overridable Function GetSigma() As Double
			Return _Sigma
		End Function
		Public Sub New()
			'for reflection
			_Sigma = 1
		End Sub
		Public Sub New( sigma As Double)
			_Sigma = sigma
		End Sub
		Public Sub New( data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Sigma = BPM.StDev()
			PeriodOfRecord = (BPM.SampleSize())
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Return _Sigma * Math.Sqrt(-2*Math.Log(probability))
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Return 1-(Math.Exp(-(Math.Pow(value, 2))/(2*(Math.Pow(_Sigma,2)))))
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Return (value/(Math.Pow(_Sigma, 2)))* Math.Exp(-(Math.Pow(value, 2))/(2*(Math.Pow(_Sigma,2))))
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errs As New List(Of Distributions.ContinuousDistributionError)
			If _Sigma<=0 Then errs.Add(New Distributions.ContinuousDistributionError("Sigma cannot be less than or equal to zero in the Rayleigh distribuiton."))
			Return errs
		End Function
	End Class

End Namespace

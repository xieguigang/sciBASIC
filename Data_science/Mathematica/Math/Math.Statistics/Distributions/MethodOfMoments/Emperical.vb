#Region "Microsoft.VisualBasic::f841ca8a087a519dd49d1a015358e679, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Emperical.vb"

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
	Public Class Emperical
		Inherits Distributions.ContinuousDistribution

		Private _CumulativeProbabilities As Double()
		Private _ExceedanceValues As Double()
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Dim index As Integer = Array.BinarySearch(_CumulativeProbabilities, probability)
			'interpolate or step?
			'check for array out of bounds...
			Return _ExceedanceValues(index)
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Dim index As Integer = Array.BinarySearch(_ExceedanceValues, value)
			'interpolate or step?
			Return _CumulativeProbabilities(index)
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _CumulativeProbabilities.Length <> _ExceedanceValues.Length Then errors.Add(New Distributions.ContinuousDistributionError("Cumulative Probability values and Emperical Exceedance values are different lengths."))
			Return errors
		End Function

	End Class

End Namespace

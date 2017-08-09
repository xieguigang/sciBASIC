#Region "Microsoft.VisualBasic::74e13d30547d28180a3a154fe89706f8, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\LogPearsonIII.vb"

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
	Public Class LogPearsonIII
		Inherits Distributions.ContinuousDistribution

		Private _Alpha As Double
		Private _Beta As Double
		Private _Xi As Double
		Public Sub New()
			'for reflection
			_Alpha = 0
			_Beta = 0
			_Xi = 0
		End Sub
		Public Sub New( data As Double())
			Dim LM As New MomentFunctions.LinearMoments(data)
			PeriodOfRecord = (LM.GetSampleSize())
			Dim z As Double = 0
			Dim a As Double = 0
			Dim abst3 As Double = Math.Abs(LM.GetT3())
			If abst3>0 AndAlso abst3<(1\3) Then
				z = 3 * Math.PI * (LM.GetT3()*LM.GetT3())
				a = ((1+(0.2906*z))/(z+(0.1882*(z*z))+(0.0442*(z*z*z))))
			ElseIf abst3<1 Then
				z = 1 - Math.Abs(LM.GetT3())
				a = ((0.36067 * z - (0.59567 * (z * z)) + (0.25361 * (z*z*z))) / (1 - 2.78861 * z + (2.56096 * (z*z)) - (0.77045 * (z*z*z))))
			Else
				'no solution because t3 is greater than or equal to 1.
			End If
			Dim gamma As Double = (2/Math.Sqrt(a)) * Math.Sign(LM.GetT3())
            Dim sigma As Double = (12 * Math.Sqrt(Math.PI) * Math.Sqrt(a) * Math.Exp(SpecialFunctions.gammaln(a))) / Math.Exp(SpecialFunctions.gammaln(a + 0.5)) 'need gammaln
            If gamma<>0 Then
				_Alpha = 4/(gamma*gamma)
				_Xi = LM.GetL1() - ((2* sigma)/gamma)
				_Beta = 0.5*sigma*Math.Abs(gamma)
			Else
				'normal distribution fits better.
			End If
		End Sub
		Public Sub New( alpha As Double,  beta As Double,  xi As Double)
			_Alpha = alpha
			_Beta = beta
			_Xi = xi
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
            Return 1 - (SpecialFunctions.IncompleteGamma(_Alpha, ((_Xi - value) / _Beta)) / Math.Exp(SpecialFunctions.gammaln(_Alpha)))
            '        If _gamma < 0 Then
            '            Return 1 - (incompletegammalower(_alpha, ((_xi - value) / _beta)) / Math.Exp(gammaln(_alpha)))
            '        Else
            '            Return (incompletegammalower(_alpha, ((value - _xi) / _beta)) / Math.Exp(gammaln(_alpha)))
            '        End If
        End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Beta = 0 Then errors.Add(New Distributions.ContinuousDistributionError("Beta cannot be zero"))
			Return errors
		End Function
	End Class

End Namespace

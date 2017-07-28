#Region "Microsoft.VisualBasic::12d593854ef7c6bb48ff76d389ca9aba, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\Distributions\MethodOfMoments\LogPearsonIII.vb"

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
	Public Class LogPearsonIII
		Inherits Distributions.ContinuousDistribution

		Private _Mean As Double
		Private _StDev As Double
		Private _Skew As Double 'cannot be zero
		Public Sub New()
			'for reflection
			_Mean = 0
			_StDev = 1
			_Skew = 0
		End Sub
		Public Sub New( mean As Double,  stdev As Double,  skew As Double,  sampleSize As Integer)
			_Mean = mean
			_StDev = stdev
			_Skew = skew
            PeriodOfRecord = (sampleSize)
        End Sub
        Public Sub New(data As Double())
            For i As Integer = 0 To data.Length - 1
                data(i) = Math.Log10(data(i))
            Next i
            Dim PM As New MomentFunctions.ProductMoments(data)
            _Mean = PM.Mean()
            _StDev = PM.StandardDeviation
            _Skew = PM.Skew()
            PeriodOfRecord = (PM.SampleSize())
        End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			If _Skew = 0 Then
				Dim zeroSkewNorm As New Normal(_Mean,_StDev)
				Dim logflow As Double = zeroSkewNorm.GetInvCDF(probability)
				Return Math.Pow(10,logflow)
			Else
				Dim sn As New Normal
				Dim z As Double = sn.GetInvCDF(probability)
				Dim k As Double = (2/_Skew)*(Math.Pow((z-_Skew/6.0)* _Skew/6.0+1,3)-1)
				Dim logflow As Double = _Mean + (k*_StDev)
				Return Math.Pow(10, logflow)
			End If
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overridable Function Bullentin17BConfidenceLimit( probability As Double,  alphaValue As Double) As Double
			Dim sn As New Normal(0,1)
			Dim z1 As Double =sn.GetInvCDF(probability)
			Dim k As Double = 0
			If _Skew = 0 Then
				k = z1
			Else
				k = (2/_Skew)*(Math.Pow((z1-_Skew/6.0)* _Skew/6.0+1,3)-1)
			End If
			Dim z As Double = sn.GetInvCDF(alphaValue)
			Dim zSquared As Double = Math.Pow(z, 2)
			Dim kSquared As Double = Math.Pow(k, 2)
            Dim Avalue As Double = (1 - (zSquared) / 2 \ (PeriodOfRecord() - 1))
            Dim Bvalue As Double = (kSquared) - ((zSquared) / PeriodOfRecord())
            Dim RootValue As Double = Math.Sqrt(kSquared-(Avalue*Bvalue))
			If alphaValue>.5 Then
				Return Math.Pow(10,_Mean + _StDev*(k + RootValue)/Avalue)
			Else
				Return Math.Pow(10,_Mean + _StDev*(k - RootValue)/Avalue)
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _StDev<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Standard of Deviation must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace

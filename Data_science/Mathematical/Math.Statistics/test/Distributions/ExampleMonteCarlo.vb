#Region "Microsoft.VisualBasic::ea172807118ba5200dd45d9c7eb0c2b8, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\test\Distributions\ExampleMonteCarlo.vb"

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

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions

	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class ExampleMonteCarlo
		Public Shared Sub Main(ByVal args As String())
			MonteCarlo()
		End Sub
		Public Shared Sub MonteCarlo()
			'this is a very trivial example of creating a monte carlo using a
			'standard normal distribution
			Dim SN As New Distributions.MethodOfMoments.Normal
			Dim output As Double() = New Double(999){}
			Dim r As New Random
			For i As Integer = 0 To output.Length - 1
				output(i) =SN.GetInvCDF(r.NextDouble())
			Next i
			'output now contains 1000 random normally distributed values.

			'to evaluate the mean and standard deviation of the output
			'you can use Basic Product Moment Stats
			Dim BPM As New MomentFunctions.BasicProductMoments(output)
			Console.WriteLine("Mean: " & BPM.GetMean())
			Console.WriteLine("StDev:" & BPM.GetStDev())
			Console.WriteLine("Sample Size: " & BPM.GetSampleSize())
			Console.WriteLine("Minimum: " & BPM.GetMin())
			Console.WriteLine("Maximum: " & BPM.GetMax())
		End Sub
	End Class

End Namespace

#Region "Microsoft.VisualBasic::da9d82797579ebb6a630ffd98883fa61, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\MomentFunctions\LinearMoments.vb"

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
Namespace MomentFunctions


	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class LinearMoments
		Private _L1 As Double
		Private _L2 As Double
		Private _L3 As Double
		Private _L4 As Double
		Private _Count As Integer
		Private _Max As Double
		Private _Min As Double
		Public Sub New( data As Double())
			Array.Sort(data) 'sorts ascending based on javadocs
			_Count = data.Length
			_Min = data(0)
			_Max = data(_Count-1)
			Dim cl2 As Long
			Dim cl3 As Long
			Dim cl4 As Long
			Dim cr1 As Long
			Dim cr2 As Long
			Dim cr3 As Long
			Dim sl1 As Double = 0
			Dim sl2 As Double = 0
			Dim sl3 As Double = 0
			Dim sl4 As Double = 0
			For i As Integer = 0 To data.Length - 1
				cl2 = CLng((i*(i-1))\2)
				cl3 = CLng((cl2*(i-2))\3)
				cr1 = _Count - (i+1)
				cr2 = CLng(cr1 * ((_Count-(i+2)))\2)
				cr3 = CLng(cr2 * ((_Count-(i+3)))\3)
				sl1 += data(i)
				sl2 += data(i) * (i - cr1)
				sl3 += data(i) * (cl2 - 2 * i * cr1 + cr2)
				sl4 += data(i) * (cl3 - 3 * cl2 * cr1 + 3 * i * cr2 - cr3)
			Next i
			cl2 = CLng(_Count) * (_Count-1)\2 ' not sure order of operations is correct here..
			cl3 = CLng(cl2) * (_Count - 2)\3
			cl4 = CLng(cl3) * (_Count - 3)\4
			_L1 = sl1/_Count
			_L2 = sl2 / cl2 \ 2
			_L3 = sl3 / cl3 \ 3
			_L4 = sl4 / cl4 \ 4
		End Sub
		Public Overridable Function GetL1() As Double
			Return _L1
		End Function
		Public Overridable Function GetL2() As Double
			Return _L2
		End Function
		Public Overridable Function GetL3() As Double
			Return _L3
		End Function
		Public Overridable Function GetL4() As Double
			Return _L4
		End Function
		Public Overridable Function GetT1() As Double
			Return _L2/_L1
		End Function
		Public Overridable Function GetT3() As Double
			Return _L3/_L2
		End Function
		Public Overridable Function GetT4() As Double
			Return _L4/_L2
		End Function
		Public Overridable Function GetMax() As Double
			Return _Max
		End Function
		Public Overridable Function GetMin() As Double
			Return _Min
		End Function
		Public Overridable Function GetSampleSize() As Integer
			Return _Count
		End Function
	End Class

End Namespace

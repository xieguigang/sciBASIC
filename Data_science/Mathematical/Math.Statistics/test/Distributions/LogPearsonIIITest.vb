#Region "Microsoft.VisualBasic::a7460b34d96227132c46dd6880a4ad61, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\test\Distributions\LogPearsonIIITest.vb"

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
Imports org.junit.Assert

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions

	''' 
	''' <summary>
	''' @author Q0HECWPL
	''' </summary>
	Public Class LogPearsonIIITest
		Private _LPII As Distributions.MethodOfMoments.LogPearsonIII
		Public Sub New()
		End Sub

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Shared Sub setUpClass()
		End Sub

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Shared Sub tearDownClass()
		End Sub

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Sub setUp()
			_LPII = New Distributions.MethodOfMoments.LogPearsonIII(5,1,1,0)
		End Sub

'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Sub tearDown()
		End Sub

		''' <summary>
		''' Test of GetInvCDF method, of class LogPearsonIII.
		''' </summary>
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Sub testGetInvCDF()
			Console.WriteLine("GetInvCDF")
			Dim probability As Double = 0.5
			Dim expResult As Double = 68852.5605818443
			Dim result As Double = _LPII.GetInvCDF(probability)
			assertEquals(expResult, result, 0.0)
			' TODO review the generated test code and remove the default call to fail.
			'fail("The test case is a prototype.");
		End Sub

		''' <summary>
		''' Test of GetCDF method, of class LogPearsonIII.
		''' </summary>
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Sub testGetCDF()
			Console.WriteLine("GetCDF")
			Dim value As Double = 0.0
			Dim expResult As Double = 0.0
			Dim result As Double = _LPII.GetCDF(value)
			assertEquals(expResult, result, 0.0)
			' TODO review the generated test code and remove the default call to fail.
			'fail("The test case is a prototype.");
		End Sub

		''' <summary>
		''' Test of GetPDF method, of class LogPearsonIII.
		''' </summary>
'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
		Public Overridable Sub testGetPDF()
			Console.WriteLine("GetPDF")
			Dim value As Double = 0.0
			Dim expResult As Double = 0.0
			Dim result As Double = _LPII.GetPDF(value)
			assertEquals(expResult, result, 0.0)
			' TODO review the generated test code and remove the default call to fail.
			'fail("The test case is a prototype.");
		End Sub

	End Class

End Namespace

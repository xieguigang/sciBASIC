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
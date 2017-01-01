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
	Public Class ContinuousDistributionError
		Private _ErrorMessage As String
		Public Overridable Function ErrorMessage() As String
			Return _ErrorMessage
		End Function
		Public Sub New(ByVal Message As String)
			_ErrorMessage = Message
		End Sub
	End Class

End Namespace
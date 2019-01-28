Namespace org.renjin.hdf5

	Public Class Flags
'JAVA TO VB CONVERTER NOTE: The variable value was renamed since Visual Basic does not allow variables and other class members to have the same name:
		Private value_Renamed As SByte

		Public Sub New(ByVal value As SByte)
			Me.value_Renamed = value
		End Sub

		Public Overridable Function isSet(ByVal bitIndex As Integer) As Boolean
			Return (value_Renamed And (1 << bitIndex)) <> 0
		End Function

		Public Overridable Function value() As SByte
			Return value_Renamed
		End Function
	End Class

End Namespace
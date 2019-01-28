Namespace org.renjin.hdf5.message


	Public Class GroupInfoMessage
		Inherits Message

		Public Const MESSAGE_TYPE As Integer = &HA

		Private linkPhaseChangeMaximumCompactValue As Integer = -1
		Private linkPhaseChangeMinimumDenseValue As Integer = -1
		Private estimatedNumberEntries As Integer = -1
		Private estimatedLinkNameLengthOfEntries As Integer = -1

		Public Sub New(reader As org.renjin.hdf5.HeaderReader)
			Dim version As SByte = reader.readByte()
			Dim flags As org.renjin.hdf5.Flags = reader.readFlags()

			If flags.isSet(0) Then
				linkPhaseChangeMaximumCompactValue = reader.readUInt16()
				linkPhaseChangeMinimumDenseValue = reader.readUInt16()
			End If

			If flags.isSet(1) Then
				estimatedNumberEntries = reader.readUInt16()
				estimatedLinkNameLengthOfEntries = reader.readUInt16()
			End If
		End Sub
	End Class

End Namespace
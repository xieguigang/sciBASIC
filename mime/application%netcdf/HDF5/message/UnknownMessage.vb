Namespace org.renjin.hdf5.message

	Public Class UnknownMessage
        Inherits MessageBase

        Private type As Integer
		Private ReadOnly messageData() As SByte

		Public Sub New(messageType As Integer, messageData() As SByte)
			Me.type = messageType
			Me.messageData = messageData
		End Sub
	End Class

End Namespace
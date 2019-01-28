Namespace org.renjin.hdf5.message




	Public Class LinkInfoMessage
		Inherits Message

		Public Const MESSAGE_TYPE As Integer = &H2

		Private Const FLAG_CREATION_ORDER_TRACKED As Integer = &H1
		Private Const FLAG_CREATION_ORDER_INDEXED As Integer = &H2
		Private ReadOnly version As SByte
		Private ReadOnly flags As SByte
		Private maximumCreationIndex As Long
		Private ReadOnly fractalHeapAddress As Long
		Private ReadOnly nameIndexAddress As Long
		Private creationOrderIndex As Long

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public LinkInfoMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Public Sub New(ByVal reader As org.renjin.hdf5.HeaderReader)
			version = reader.readByte()
			If version <> 0 Then
				Throw New System.NotSupportedException("Version: " & version)
			End If
			flags = reader.readByte()

			maximumCreationIndex = -1
			If (flags And FLAG_CREATION_ORDER_TRACKED) <> 0 Then
				maximumCreationIndex = reader.readUInt64()
			End If

			fractalHeapAddress = reader.readOffset()
			nameIndexAddress = reader.readOffset()

			creationOrderIndex = -1
			If (flags And FLAG_CREATION_ORDER_INDEXED) <> 0 Then
				creationOrderIndex = reader.readOffset()
			End If
		End Sub

		Public Overridable Function hasFractalHeap() As Boolean
			Return fractalHeapAddress <> -1
		End Function

		Public Overridable Property FractalHeapAddress As Long
			Get
				Return fractalHeapAddress
			End Get
		End Property


	End Class

End Namespace
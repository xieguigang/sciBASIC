Namespace org.renjin.hdf5.chunked



	Public Class ChunkNode

		Private ReadOnly nodeLevel As Integer
		Private ReadOnly nodeType As SByte
		Private ReadOnly addressLeftSibling As Long
		Private ReadOnly addressRightSibling As Long

		Private keys() As ChunkKey

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ChunkNode(org.renjin.hdf5.message.DataLayoutMessage dataLayout, org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Public Sub New(dataLayout As org.renjin.hdf5.message.DataLayoutMessage, reader As org.renjin.hdf5.HeaderReader)
			reader.checkSignature("TREE")
			nodeType = reader.readByte()
			If nodeType <> 1 Then
                Throw New Exception("Expected nodeType = chunk (1)")
            End If
			nodeLevel = reader.readUInt8()
			Dim entriesUsed As Integer = reader.readUInt16()
			addressLeftSibling = reader.readOffset()
			addressRightSibling = reader.readOffset()

			keys = New ChunkKey(entriesUsed){}

			For i As Integer = 0 To entriesUsed
				keys(i) = New ChunkKey(reader, dataLayout.Dimensionality, (i < entriesUsed))
			Next i
		End Sub

		Public Overridable Property Leaf As Boolean
			Get
				Return nodeLevel = 0
			End Get
		End Property

		Public Overridable Function findChildAddress(chunkCoordinates() As Long) As ChunkKey
			For i As Integer = 0 To keys.Length - 2
				Dim lower As Integer = keys(i).compare(chunkCoordinates)
				Dim upper As Integer = keys(i+1).compare(chunkCoordinates)

				If lower <= 0 AndAlso upper > 0 Then
					Return keys(i)
				End If
			Next i
            Throw New Exception()
        End Function
	End Class

End Namespace
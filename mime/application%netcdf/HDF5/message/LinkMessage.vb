Namespace org.renjin.hdf5.message



	''' <summary>
	''' This message encodes the information for a link in a group’s object header, when the group is storing its
	''' links "compactly", or in the group’s fractal heap, when the group is storing its links "densely".
	''' 
	''' A group is storing its links compactly when the fractal heap address in the Link Info Message is
	''' set to the "undefined address" value.
	''' </summary>
	Public Class LinkMessage
        Inherits MessageBase

        Public Const MESSAGE_TYPE As Integer = &H6

		Public Const HARD_LINK As SByte = 0
		Public Const SOFT_LINK As SByte = 1
		Public Const EXTERNAL As SByte = 64

		Private version As SByte

        Private creationOrder As Long
		Private charset As java.nio.charset.Charset

        Public Overridable Property LinkName As String
        Public Overridable Property LinkType As SByte
        Public Overridable Property Address As Long

        'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        'ORIGINAL LINE: public LinkMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
        Public Sub New(reader As org.renjin.hdf5.HeaderReader)
			version = reader.readByte()
			Dim flags As org.renjin.hdf5.Flags = reader.readFlags()

			If flags.isSet(3) Then
				linkType = reader.readByte()
			End If

			If flags.isSet(2) Then
				creationOrder = reader.readUInt64()
			End If

			charset = org.renjin.repackaged.guava.base.Charsets.US_ASCII
			If flags.isSet(4) Then
				Dim charsetIndex As SByte = reader.readByte()
				Select Case charsetIndex
					Case 0
						charset = org.renjin.repackaged.guava.base.Charsets.US_ASCII
					Case 1
						charset = org.renjin.repackaged.guava.base.Charsets.UTF_8
				End Select
			End If

			Dim linkNameLength As Integer = reader.readVariableLengthSizeAsInt(flags)
			linkName = reader.readString(linkNameLength, charset)

			Select Case linkType
				Case HARD_LINK
					address = reader.readOffset()
			End Select
		End Sub

    End Class

End Namespace
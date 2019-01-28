Imports System
Imports System.Collections.Generic
Imports org.renjin.hdf5.message

Namespace org.renjin.hdf5




	Public Class DataObject

		Private Const MESSAGE_SHARED_BIT As Integer = 1

		Private ReadOnly file As Hdf5Data
		Private address As Long
		Private version As SByte
		Private ReadOnly messages As IList(Of Message) = New List(Of Message)()
		Private ReadOnly continuations As LinkedList(Of ContinuationMessage) = New java.util.ArrayDeque(Of ContinuationMessage)()

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public DataObject(Hdf5Data file, long address) throws java.io.IOException
		Public Sub New(file As Hdf5Data, address As Long)
			Me.file = file
			Me.address = address

			Dim reader As HeaderReader = file.readerAt(address)
			If reader.peekByte() = AscW("O"c) Then
				readVersion2(reader)
			Else
				readVersion1(reader)
			End If
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion1(HeaderReader reader) throws java.io.IOException
		Private Sub readVersion1(reader As HeaderReader)
			version = reader.readByte()
			If version <> 1 Then
				Throw New java.io.IOException("Unsupported data object header version: " & version)
			End If
			Dim reserved0 As SByte = reader.readByte()
			Dim totalNumberOfMessages As Integer = reader.readUInt16()
			Dim objectReferenceCount As Long = reader.readUInt32()
			Dim objectHeaderSize As Integer = reader.readUInt32AsInt()

			readMessagesVersion1(reader, objectHeaderSize)


			Dim continuation As ContinuationMessage
			continuation = continuations.RemoveFirst()
			Do While continuation IsNot Nothing
				readContinuationV1(continuation)
				continuation = continuations.RemoveFirst()
			Loop
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readContinuationV1(ContinuationMessage continuationMessage) throws java.io.IOException
'JAVA TO VB CONVERTER NOTE: The parameter continuationMessage was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Private Sub readContinuationV1(continuationMessage_Renamed As ContinuationMessage)
			Dim reader As HeaderReader = file.readerAt(continuationMessage_Renamed.Offset, continuationMessage_Renamed.Length)

			' Continuation blocks for version 1 object headers have no special formatting information;
			' they are merely a list of object header message info sequences (type, size, flags, reserved bytes and
			' data for each message sequence). See the description of Version 1 Data Object Header Prefix.
			readMessagesVersion1(reader, org.renjin.repackaged.guava.primitives.Ints.checkedCast(continuationMessage_Renamed.Length))
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readMessagesVersion1(HeaderReader reader, int objectHeaderSize) throws java.io.IOException
		Private Sub readMessagesVersion1(reader As HeaderReader, objectHeaderSize As Integer)

			Do While objectHeaderSize > 0

				reader.alignTo(8)

				Dim messageType As Integer = reader.readUInt16()
				Dim messageDataSize As Integer = reader.readUInt16()

				If messageType <> 0 Then
					Dim messageFlags As Flags = reader.readFlags()
					Dim padding() As SByte = reader.readBytes(3)
					Dim messageData() As SByte = reader.readBytes(messageDataSize)

					If messageFlags.isSet(MESSAGE_SHARED_BIT) Then
						Throw New System.NotSupportedException("Shared message")
					Else
						addMessage(createMessage(messageType, messageData))
					End If
				End If

				objectHeaderSize -= 8
				objectHeaderSize -= messageDataSize
			Loop
		End Sub

		Private Sub addMessage(message As Message)
			messages.Add(message)
			If TypeOf message Is ContinuationMessage Then
				continuations.AddLast(CType(message, ContinuationMessage))
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion2(HeaderReader reader) throws java.io.IOException
		Private Sub readVersion2(reader As HeaderReader)
			reader.checkSignature("OHDR")

			version = reader.readByte()
			If version <> 2 Then
				Throw New java.io.IOException("Unsupported data object header version: " & version)
			End If
			Dim flags As Flags = reader.readFlags()

			If flags.isSet(5) Then
				Dim accessTime As Integer = reader.readInt()
				Dim modificationTime As Integer = reader.readInt()
				Dim changeTime As Integer = reader.readInt()
				Dim birthTime As Integer = reader.readInt()
			End If

			If flags.isSet(4) Then
				Dim maxNumberCompactAttributes As Integer = reader.readUInt16()
				Dim maxNumberDenseAttributes As Integer = reader.readUInt16()
			End If

			Dim chunkLength As Integer = reader.readVariableLengthSizeAsInt(flags)

			reader.updateLimit(chunkLength)

			readMessagesV2(reader, flags)

			Dim continuation As ContinuationMessage
			continuation = continuations.RemoveFirst()
			Do While continuation IsNot Nothing
				readContinuationV2(continuation, flags)
				continuation = continuations.RemoveFirst()
			Loop
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readContinuationV2(ContinuationMessage continuationMessage, Flags flags) throws java.io.IOException
'JAVA TO VB CONVERTER NOTE: The parameter continuationMessage was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Private Sub readContinuationV2(continuationMessage_Renamed As ContinuationMessage, flags As Flags)
			Dim reader As HeaderReader = file.readerAt(continuationMessage_Renamed.Offset, continuationMessage_Renamed.Length)
			reader.checkSignature("OCHK")
			readMessagesV2(reader, flags)
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readMessagesV2(HeaderReader reader, Flags flags) throws java.io.IOException
		Private Sub readMessagesV2(reader As HeaderReader, flags As Flags)

			' A gap in an object header chunk is inferred by the end of the messages for the chunk before the beginning
			' of the chunk’s checksum. Gaps are always smaller than the size of an object header message prefix
			' (message type + message size + message flags).
			Dim messageDataPrefixSize As Integer = 4

			Do While reader.remaining() > messageDataPrefixSize
				Dim messageType As Integer = reader.readUInt8()
				If messageType = 0 Then
					Exit Do
				End If

				Dim messageDataSize As Integer = reader.readUInt16()
				Dim messageFlags As Flags = reader.readFlags()

				Dim messageCreationOrder As Short
				If flags.isSet(2) Then
					messageCreationOrder = reader.readByte()
				End If
				Dim messageData() As SByte = reader.readBytes(messageDataSize)

				addMessage(createMessage(messageType, messageData))
			Loop
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Message createMessage(int messageType, byte[] messageData) throws java.io.IOException
		Private Function createMessage(messageType As Integer, messageData() As SByte) As Message

			Dim reader As New HeaderReader(file.Superblock, java.nio.ByteBuffer.wrap(messageData))
			Select Case messageType
				Case LinkInfoMessage.MESSAGE_TYPE
					Return New LinkInfoMessage(reader)
				Case LinkMessage.MESSAGE_TYPE
					Return New LinkMessage(reader)
				Case GroupInfoMessage.MESSAGE_TYPE
					Return New GroupInfoMessage(reader)
				Case DataspaceMessage.MESSAGE_TYPE
					Return New DataspaceMessage(reader)
				Case DatatypeMessage.MESSAGE_TYPE
					Return New DatatypeMessage(reader)
				Case FillValueMessage.MESSAGE_TYPE
					Return New FillValueMessage(reader)
				Case DataLayoutMessage.MESSAGE_TYPE
					Return New DataLayoutMessage(reader)
				Case ContinuationMessage.MESSAGE_TYPE
					Return New ContinuationMessage(reader)
				Case DataStorageMessage.MESSAGE_TYPE
					Return New DataStorageMessage(reader)
				Case AttributeMessage.MESSAGE_TYPE
					Return New AttributeMessage(reader)
				Case SymbolTableMessage.MESSAGE_TYPE
					Return New SymbolTableMessage(reader)
				Case Else
					Return New UnknownMessage(messageType, messageData)
			End Select
		End Function

		Public Overridable Function getMessages(Of T As Message)(messageClass As Type) As IEnumerable(Of T)
			Return org.renjin.repackaged.guava.collect.Iterables.filter(messages, messageClass)
		End Function

		Public Overridable Function getMessage(Of T As Message)(messageClass As Type) As T
			Return org.renjin.repackaged.guava.collect.Iterables.getOnlyElement(getMessages(messageClass))
		End Function

		Public Overridable Function hasMessage(messageClass As Type) As Boolean
'JAVA TO VB CONVERTER NOTE: The variable message was renamed since Visual Basic does not handle local variables named the same as class members well:
			For Each message_Renamed As Message In messages
				If message_Renamed.GetType().Equals(messageClass) Then
					Return True
				End If
			Next message_Renamed
			Return False
		End Function

		Public Overridable Function getMessageIfPresent(Of T As Message)(messageClass As Type) As org.renjin.repackaged.guava.base.Optional(Of T)
'JAVA TO VB CONVERTER NOTE: The variable message was renamed since Visual Basic does not handle local variables named the same as class members well:
			For Each message_Renamed As Message In messages
				If message_Renamed.GetType().Equals(messageClass) Then
					Return org.renjin.repackaged.guava.base.Optional.of(Of T)(CType(message_Renamed, T))
				End If
			Next message_Renamed
			Return org.renjin.repackaged.guava.base.Optional.absent()
		End Function
	End Class
End Namespace
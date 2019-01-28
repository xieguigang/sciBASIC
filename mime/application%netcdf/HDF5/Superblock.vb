Imports Microsoft.VisualBasic
Imports System

Namespace org.renjin.hdf5


	Public Class Superblock

		Private Const MAX_LENGTH As Long = 1000

		Private ReadOnly superBlockVersion As SByte
		Private offsetsSize As SByte
		Private lengthsSize As SByte
		Private fileConsistencyFlags As SByte
		Private baseAddress As Long
		Private superBlockExtensionAddress As Long
		Private endOfFileAddress As Long
		Private rootGroupObjectHeaderAddress As Long
		Private superBlockChecksum As Integer
		Private driverInformationBlockAddress As Long
		Private groupLeafNodeK As Integer

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Superblock(java.nio.channels.FileChannel channel) throws java.io.IOException
		Public Sub New(ByVal channel As java.nio.channels.FileChannel)

			Dim buffer As java.nio.MappedByteBuffer = channel.map(java.nio.channels.FileChannel.MapMode.READ_ONLY, 0, Math.Min(channel.size(), MAX_LENGTH))
			buffer.order(java.nio.ByteOrder.LITTLE_ENDIAN)

			readAndCheckSignature(buffer)

			superBlockVersion = buffer.get()

			If superBlockVersion = 0 Then
				readVersion0(buffer)
			ElseIf superBlockVersion = 2 OrElse superBlockVersion = 3 Then
				readVersion2(buffer)
			Else
				Throw New java.io.IOException("Unsupported superblock version: " & superBlockVersion)
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion0(java.nio.MappedByteBuffer buffer) throws java.io.IOException
		Private Sub readVersion0(ByVal buffer As java.nio.MappedByteBuffer)

			Dim freeSpaceStorageVersion As Integer = buffer.get()
			Dim rootGroupSymbolTableEntryVersion As Integer = buffer.get()
			Dim reserved As SByte = buffer.get()

			Dim sharedHeaderMessageFormatVersion As Integer = buffer.get()
			offsetsSize = buffer.get()
			lengthsSize = buffer.get()
			Dim reserved1 As SByte = buffer.get()

			groupLeafNodeK = buffer.Short
			Dim groupInternalNodeK As Integer = buffer.Short

			Dim fileConsistencyFlags As Integer = buffer.Int


			If offsetsSize = 8 Then
				baseAddress = buffer.Long
				Dim freeFilespaceInfoAddress As Long = buffer.Long
				endOfFileAddress = buffer.Long
				driverInformationBlockAddress = buffer.Long

				' Root Group Symbol Table Entry should start here...
				Dim linkNameOffset As Long = buffer.Long
				rootGroupObjectHeaderAddress = buffer.Long
				Dim cacheType As Integer = buffer.Int

				If cacheType = 2 Then
					Throw New System.NotSupportedException("Root Group Symbol Table Entry / cacheType = " & cacheType)
				End If
				Dim reserved2 As Integer = buffer.Int

				Dim scratchPad(15) As SByte
				buffer.get(scratchPad)

			Else
				Throw New System.NotSupportedException("offsetsSize = " & offsetsSize)
			End If

		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion2(java.nio.MappedByteBuffer buffer) throws java.io.IOException
		Private Sub readVersion2(ByVal buffer As java.nio.MappedByteBuffer)
	'        
	'         * This value contains the number of bytes used to store addresses in the file. The values for the
	'         * addresses of objects in the file are offsets relative to a base address, usually the address of the
	'         * superblock signature. This allows a wrapper to be added after the file is created without invalidating
	'         * the internal offset locations.
	'         
			offsetsSize = buffer.get()
			lengthsSize = buffer.get()
			fileConsistencyFlags = buffer.get()

			If offsetsSize <> 8 OrElse lengthsSize <> 8 Then
				Throw New java.io.IOException("Unsupported offsets/length size: " & offsetsSize)
			End If

			baseAddress = buffer.Long
			superBlockExtensionAddress = buffer.Long
			endOfFileAddress = buffer.Long
			rootGroupObjectHeaderAddress = buffer.Long
			superBlockChecksum = buffer.Int
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readAndCheckSignature(java.nio.ByteBuffer buffer) throws java.io.IOException
		Private Sub readAndCheckSignature(ByVal buffer As java.nio.ByteBuffer)
			Dim array(7) As SByte
			buffer.get(array)

			If Not(array(1) = AscW("H"c) AndAlso array(2) = AscW("D"c) AndAlso array(3) = AscW("F"c) AndAlso array(4) = ControlChars.Cr AndAlso array(5) = ControlChars.Lf AndAlso array(6) = &H1a AndAlso array(7) = ControlChars.Lf) Then 'array[0] == 137 &&
				Throw New java.io.IOException("Invalid format signature: " & java.util.Arrays.ToString(array))
			End If
		End Sub

		Public Overridable Property SuperBlockVersion As SByte
			Get
				Return superBlockVersion
			End Get
		End Property

		Public Overridable Property BaseAddress As Long
			Get
				Return baseAddress
			End Get
		End Property

		Public Overridable Property OffsetSize As SByte
			Get
				Return offsetsSize
			End Get
		End Property

		Public Overridable Property LengthSize As SByte
			Get
				Return lengthsSize
			End Get
		End Property

		Public Overridable Property RootGroupObjectHeaderAddress As Long
			Get
				Return rootGroupObjectHeaderAddress
			End Get
		End Property

	End Class

End Namespace
Imports System

Namespace org.renjin.hdf5


	Public Class Hdf5Data
	  Private channel As java.nio.channels.FileChannel
	  Private superblock As Superblock

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public Hdf5Data(java.io.File file) throws java.io.IOException
	  Public Sub New(file As java.io.File)
		Dim randomAccessFile As New java.io.RandomAccessFile(file, "r")
		channel = randomAccessFile.Channel
		superblock = New Superblock(channel)
	  End Sub

	  Public Overridable Property Superblock As Superblock
		  Get
			Return superblock
		  End Get
	  End Property

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public DataObject objectAt(long address) throws java.io.IOException
	  Public Overridable Function objectAt(address As Long) As DataObject
		Return New DataObject(Me, address)
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public HeaderReader readerAt(long address) throws java.io.IOException
	  Public Overridable Function readerAt(address As Long) As HeaderReader
		Return readerAt(address, 1024 * 5)
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public HeaderReader readerAt(long address, long maxHeaderSize) throws java.io.IOException
	  Public Overridable Function readerAt(address As Long, maxHeaderSize As Long) As HeaderReader
		Dim buffer As java.nio.MappedByteBuffer = channel.map(java.nio.channels.FileChannel.MapMode.READ_ONLY, address, Math.Min(maxHeaderSize, channel.size() - address))

		Return New HeaderReader(superblock, buffer)
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public java.nio.ByteBuffer bufferAt(long address, long size) throws java.io.IOException
	  Public Overridable Function bufferAt(address As Long, size As Long) As java.nio.ByteBuffer
		Return channel.map(java.nio.channels.FileChannel.MapMode.READ_ONLY, address, size)
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public int read(java.nio.ByteBuffer buffer, long address) throws java.io.IOException
	  Public Overridable Function read(buffer As java.nio.ByteBuffer, address As Long) As Integer
		Return channel.read(buffer, address)
	  End Function

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public java.nio.MappedByteBuffer map(java.nio.channels.FileChannel.MapMode mode, long address, long size) throws java.io.IOException
	  Public Overridable Function map(mode As java.nio.channels.FileChannel.MapMode, address As Long, size As Long) As java.nio.MappedByteBuffer
		Return channel.map(mode, address, size)
	  End Function
	End Class

End Namespace
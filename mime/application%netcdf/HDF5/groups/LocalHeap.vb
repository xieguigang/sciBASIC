Namespace org.renjin.hdf5.groups



	''' <summary>
	''' A local heap is a collection of small pieces of data that are particular to a single object in the HDF5 file.
	''' Objects can be inserted and removed from the heap at any time. The address of a heap does not change once the
	''' heap is created. For example, a group stores addresses of objects in symbol table nodes with the names of
	''' links stored in the group’s local heap.
	''' </summary>
	Public Class LocalHeap

	  Private file As org.renjin.hdf5.Hdf5Data
	  Private ReadOnly dataSegmentSize As Long
	  Private ReadOnly headOfFreeListOffset As Long
	  Private ReadOnly dataSegmentAddress As Long

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public LocalHeap(org.renjin.hdf5.Hdf5Data file, long address) throws java.io.IOException
	  Public Sub New(ByVal file As org.renjin.hdf5.Hdf5Data, ByVal address As Long)
		Me.file = file
		Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(address, 48)
		reader.checkSignature("HEAP")
		Dim version As Integer = reader.readUInt8()
		If version <> 0 Then
		  Throw New System.NotSupportedException("Version: " & version)
		End If
		reader.readReserved(3)
		dataSegmentSize = reader.readLength()
		headOfFreeListOffset = reader.readLength()

		dataSegmentAddress = reader.readOffset()

	  End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public String stringAt(long offset) throws java.io.IOException
	  Public Overridable Function stringAt(ByVal offset As Long) As String
		Dim reader As org.renjin.hdf5.HeaderReader = file.readerAt(dataSegmentAddress + offset)
		Return reader.readNullTerminatedAsciiString()
	  End Function
	End Class

End Namespace
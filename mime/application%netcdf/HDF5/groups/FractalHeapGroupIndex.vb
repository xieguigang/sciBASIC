Namespace org.renjin.hdf5.groups



	Public Class FractalHeapGroupIndex
		Implements GroupIndex

	  Private ReadOnly heap As FractalHeap
	  Private file As org.renjin.hdf5.Hdf5Data

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public FractalHeapGroupIndex(org.renjin.hdf5.Hdf5Data file, long address) throws java.io.IOException
	  Public Sub New(file As org.renjin.hdf5.Hdf5Data, address As Long)
		Me.file = file
		heap = New FractalHeap(file, address)

	  End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public org.renjin.hdf5.DataObject getObject(String name) throws java.io.IOException
	  Public Overrides Function getObject(name As String) As org.renjin.hdf5.DataObject Implements GroupIndex.getObject
		Dim rootBlock As FractalHeap.DirectBlock = heap.RootBlock
		Dim link As org.renjin.hdf5.message.LinkMessage = rootBlock.readLinkMessage()
		If link.LinkName.Equals(name) Then
		  Return file.objectAt(link.Address)
		End If
		Throw New System.ArgumentException(name)
	  End Function
	End Class

End Namespace
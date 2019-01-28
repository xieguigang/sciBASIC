Namespace org.renjin.hdf5.message



	''' <summary>
	''' The object header continuation is the location in the file of a block containing more header messages for the
	''' current data object. This can be used when header blocks become too large or are likely to change over time.
	''' </summary>
	Public Class ContinuationMessage
		Inherits Message

	  Public Const MESSAGE_TYPE As Integer = &H10
	  Private ReadOnly offset As Long
	  Private ReadOnly length As Long


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ContinuationMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
	  Public Sub New(reader As org.renjin.hdf5.HeaderReader)
		offset = reader.readOffset()
		length = reader.readLength()
	  End Sub

	  Public Overridable Property Length As Long
		  Get
			Return length
		  End Get
	  End Property

	  Public Overridable Property Offset As Long
		  Get
			Return offset
		  End Get
	  End Property
	End Class

End Namespace
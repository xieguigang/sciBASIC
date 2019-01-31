Namespace org.renjin.hdf5.groups



	Public Class GroupBTree
		Implements GroupIndex

	  Private ReadOnly file As org.renjin.hdf5.Hdf5Data
	  Private ReadOnly symbolTable As org.renjin.hdf5.message.SymbolTableMessage
	  Private ReadOnly localHeap As LocalHeap
	  Private ReadOnly rootNode As Node

	  Private Class Node
		  Private ReadOnly outerInstance As GroupBTree


		Private ReadOnly nodeLevel As Integer
		Private ReadOnly leftSibling As Long
		Private ReadOnly rightSibling As Long
		Private ReadOnly keys() As SymbolTableNode

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private Node(long address) throws java.io.IOException
		Private Sub New(outerInstance As GroupBTree, address As Long)
				Me.outerInstance = outerInstance
		  Dim reader As org.renjin.hdf5.HeaderReader = outerInstance.file.readerAt(address)
		  reader.checkSignature("TREE")
		  Dim nodeType As Integer = reader.readUInt8()
		  If nodeType <> 0 Then
                    Throw New Exception("Expected nodeType 0 (group nodes), found: " & nodeType)
                End If

		  nodeLevel = reader.readUInt8()
		  Dim entriesUsed As Integer = reader.readUInt16()
		  leftSibling = reader.readOffset()
		  rightSibling = reader.readOffset()

		  ' For nodes of node type 0 (group nodes), the key is formatted as follows:
		  ' A single field of Size of Lengths bytes:
		  '     Indicates the byte offset into the local heap for the first object name in the
		  '     subtree which that key describes.

		  keys = New SymbolTableNode(entriesUsed){}
		  For i As Integer = 0 To entriesUsed
			Dim nodeAddress As Long = reader.readLength()
			If nodeAddress <> 0 Then
			  keys(i) = New SymbolTableNode(nodeAddress)
			End If
		  Next i
		End Sub

            Public Overridable ReadOnly Property Leaf As Boolean
                Get
                    Return nodeLevel = 0
                End Get
            End Property

        End Class


	  Public Class SymbolTableNode
		  Private ReadOnly outerInstance As GroupBTree


		Private ReadOnly entries() As SymbolTableEntry

            Private Sub New(outerInstance As GroupBTree, address As Long)
				Me.outerInstance = outerInstance
		  Dim reader As org.renjin.hdf5.HeaderReader = outerInstance.file.readerAt(address)
		  reader.checkSignature("SNOD")
		  Dim version As Integer = reader.readUInt8()
		  If version <> 1 Then
			Throw New System.NotSupportedException("Version: " & version)
		  End If
		  reader.readReserved(1)
		  Dim numSymbols As Integer = reader.readUInt16()

		  entries = New SymbolTableEntry(numSymbols - 1){}
		  For i As Integer = 0 To numSymbols - 1
			entries(i) = New SymbolTableEntry(reader)
		  Next i
		End Sub
	  End Class


	  Public Class SymbolTableEntry
		  Private ReadOnly outerInstance As GroupBTree


		Private ReadOnly linkNameOffset As Long
		Private ReadOnly linkName As String
		Private ReadOnly objectHeaderAddress As Long
		Private ReadOnly cacheType As Integer
		Private ReadOnly scratch() As SByte

            Public Sub New(outerInstance As GroupBTree, reader As org.renjin.hdf5.HeaderReader)
				Me.outerInstance = outerInstance
		  linkNameOffset = reader.readOffset()
		  linkName = outerInstance.localHeap.stringAt(linkNameOffset)
		  objectHeaderAddress = reader.readOffset()
		  cacheType = reader.readUInt32AsInt()
		  reader.readReserved(4)
		  scratch = reader.readBytes(16)
		End Sub
	  End Class

        Public Sub New(file As org.renjin.hdf5.Hdf5Data, symbolTable As org.renjin.hdf5.message.SymbolTableMessage)
		Me.file = file
		Me.symbolTable = symbolTable
		Me.localHeap = New LocalHeap(file, symbolTable.LocalHeapAddress)
		Me.rootNode = New Node(Me, symbolTable.getbTreeAddress())
	  End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public org.renjin.hdf5.DataObject getObject(String name) throws java.io.IOException
	  Public Overrides Function getObject(name As String) As org.renjin.hdf5.DataObject Implements GroupIndex.getObject

		Dim node As Node = rootNode
		Do While Not node.Leaf
		  Throw New System.NotSupportedException("TODO")
		Loop

		Dim symbolTableNode As SymbolTableNode = findChildAddress(node, name)
		For Each entry As SymbolTableEntry In symbolTableNode.entries
		  If entry.linkName.Equals(name) Then
			Return file.objectAt(entry.objectHeaderAddress)
		  End If
		Next entry


		Throw New System.ArgumentException(name)
	  End Function

	  Public Overridable Function findChildAddress(node As Node, name As String) As SymbolTableNode

		If node.keys.length = 2 AndAlso node.keys(0) Is Nothing Then
		  Return node.keys(1)
		End If
		Throw New System.NotSupportedException("TODO")
	'    for (int i = 0; i < node.keys.length - 1; i++) {
	'      int lower = compare(node.keys[i], name);
	'      int upper = compare(node.keys[i+1], name);
	'
	'      if(lower <= 0 && upper > 0) {
	'        return node.keys[i];
	'      }
	'    }
	'    throw new IllegalStateException();
	  End Function

	  Private Function compare(key As SymbolTableNode, name As String) As Integer
		If key Is Nothing Then
		  Return -1
		End If
		Dim first As Integer = key.entries(0).linkName.CompareTo(name)
		If first <= 0 Then
		  Return first
		End If
		Throw New System.NotSupportedException("TODO")
	  End Function
	End Class

End Namespace
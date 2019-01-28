Imports System.Text

Namespace org.renjin.hdf5.chunked




	Public Class ChunkKey
		''' <summary>
		''' Size of chunk in bytes.
		''' </summary>
		Private chunkSize As Integer

		''' <summary>
		''' Filter mask, a 32-bit bit field indicating which filters have been skipped for this chunk. Each filter has an
		''' index number in the pipeline (starting at 0, with the first filter to apply) and if that filter is skipped, the
		''' bit corresponding to its index is set.
		''' </summary>
		Private filterMask As Integer

		Private offset() As Long

		Private childPointer As Long

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public ChunkKey(org.renjin.hdf5.HeaderReader reader, int dimensionality, boolean hasChildPointer) throws java.io.IOException
		Public Sub New(ByVal reader As org.renjin.hdf5.HeaderReader, ByVal dimensionality As Integer, ByVal hasChildPointer As Boolean)

			chunkSize = reader.readUInt32AsInt()

			filterMask = reader.readUInt()

	'        
	'         * The offset of the chunk within the dataset where D is the number of dimensions of the dataset, and the last
	'         * value is the offset within the dataset’s datatype and should always be zero. For example, if a chunk in a
	'         * 3-dimensional dataset begins at the position [5,5,5], there will be three such 64-bit values, each with the
	'         * value of 5, followed by a 0 value.
	'         

			offset = New Long(dimensionality - 1){}
			For j As Integer = 0 To dimensionality - 1
				offset(j) = reader.readUInt64()
			Next j
			Dim zero As Long = reader.readUInt64()

			If hasChildPointer Then
				Me.childPointer = reader.readOffset()
			End If
		End Sub

		''' <summary>
		''' The tree node contains file addresses of subtrees or data depending on the node level.
		''' 
		''' <p>Nodes at Level 0 point to data addresses, either raw data chunks or group nodes. Nodes at non-zero levels
		''' point to other nodes of the same B-tree.
		''' 
		''' <p>For raw data chunk nodes, the child pointer is the address of a single raw data chunk. For group nodes,
		''' the child pointer points to a symbol table, which contains information for multiple symbol table entries.
		''' </summary>
		Public Overridable Property ChildPointer As Long
			Get
				Return childPointer
			End Get
		End Property

		''' 
		''' <returns> the size of the chunk, in bytes. </returns>
		Public Overridable Property ChunkSize As Integer
			Get
				Return chunkSize
			End Get
		End Property


		''' <summary>
		''' Compares this chunk's offset with the given index.
		''' </summary>
		Public Overridable Function compare(ByVal index() As Long) As Integer
			For i As Integer = 0 To offset.Length - 1
				If offset(i) <> index(i) Then
					Return Long.Compare(offset(i), index(i))
				End If
			Next i
			Return 0
		End Function

		''' <summary>
		''' 	The offset of the chunk within the dataset where D is the number of dimensions of the dataset.
		''' 	For example, if a chunk in a 3-dimensional dataset begins at the position [5,5,5], there will be three
		''' 	such 64-bit values, each with the value of 5.
		''' </summary>
		Public Overridable Property Offset As Long()
			Get
				Return offset
			End Get
		End Property

		Public Overrides Function ToString() As String
			Dim s As New StringBuilder("ChunkKey{")
			For i As Integer = 0 To offset.Length - 1
				If i > 0 Then
					s.Append(",")
				End If
				s.Append(offset(i))
			Next i
			s.Append("}")
			Return s.ToString()
		End Function

		Public Overrides Function Equals(ByVal o As Object) As Boolean
			If Me Is o Then
				Return True
			End If
			If o Is Nothing OrElse Me.GetType() IsNot o.GetType() Then
				Return False
			End If

			Dim key As ChunkKey = CType(o, ChunkKey)

			Return java.util.Arrays.Equals(offset, key.offset)

		End Function

		Public Overrides Function GetHashCode() As Integer
			Return java.util.Arrays.hashCode(offset)
		End Function
	End Class

End Namespace
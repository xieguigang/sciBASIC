
'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO

Namespace edu.arizona.cs.hdf5.structure


	Public Class BLinkTreeNode
		Public Shared ReadOnly m_SIGNATURE As SByte() = New SByte() {CSByte("T"C), CSByte("R"C), CSByte("E"C), CSByte("E"C)}

		Private m_signature As SByte()
		Private m_nodeType As Integer
		Private m_nodeLevel As Integer
		Private m_entriesUsed As Integer

		Private m_addressOfLeftSibling As Long
		Private m_addressOfRightSibling As Long

		Private m_offsetToLocalHeap As List(Of System.Nullable(Of Long))
		Private m_keyOfChild As List(Of SByte())

		Private m_addressOfChild As List(Of System.Nullable(Of Long))

		Private m_totalBLinkTreeNodeSize As Integer

        Public Sub New([in] As BinaryReader, sb As Superblock)

			' signature
			Me.m_signature = New SByte(3) {}

			For i As Integer = 0 To 3
				Me.m_signature(i) = [in].readByte()
			Next

			If Not Me.validSignature Then
				Throw New IOException("signature is not valid")
			End If

			Me.m_nodeType = [in].readByte()
			Me.m_nodeLevel = [in].readByte()
			Me.m_entriesUsed = [in].readShort()

			Me.m_totalBLinkTreeNodeSize = 8

			Me.m_addressOfLeftSibling = ReadHelper.readO([in], sb)
			Me.m_addressOfRightSibling = ReadHelper.readO([in], sb)

			Me.m_totalBLinkTreeNodeSize += sb.sizeOfOffsets * 2

			Me.m_offsetToLocalHeap = New List(Of System.Nullable(Of Long))()
			Me.m_keyOfChild = New List(Of SByte())()
			Me.m_addressOfChild = New List(Of System.Nullable(Of Long))()

			For i As Integer = 0 To Me.m_entriesUsed - 1
				If Me.m_nodeType = 0 Then
					Dim key As System.Nullable(Of Long) = ReadHelper.readL([in], sb)
					Me.m_offsetToLocalHeap.Add(key)
				ElseIf Me.m_nodeType = 1 Then
					Dim chunksize As Integer = [in].readInt()
					Dim filtermask As Integer = [in].readInt()
				Else
					Throw New IOException("node type is not implemented")
				End If
			Next
		End Sub

		Public Overridable ReadOnly Property signature() As SByte()
			Get
				Return Me.m_signature
			End Get
		End Property

		Public Overridable ReadOnly Property validSignature() As Boolean
			Get
				For i As Integer = 0 To 3
					If Me.m_signature(i) <> m_SIGNATURE(i) Then
						Return False
					End If
				Next
				Return True
			End Get
		End Property

		Public Overridable ReadOnly Property totalBLinkTreeNodeSize() As Integer
			Get
				Return Me.m_totalBLinkTreeNodeSize
			End Get
		End Property

		Public Overridable Sub printValues()
			Console.WriteLine("BLinkTreeNode >>>")
			Console.WriteLine("signature : " & (Me.m_signature(0) And &Hff).ToString("x") & (Me.m_signature(1) And &Hff).ToString("x") & (Me.m_signature(2) And &Hff).ToString("x") & (Me.m_signature(3) And &Hff).ToString("x"))
			'
'            System.out.println("version : " + this.m_version);
'            System.out.println("data segment size : " + this.m_dataSegmentSize);
'            System.out.println("offset to head of free list : " + this.m_offsetToHeadOfFreeList);
'            System.out.println("address of data segment : " + this.m_addressOfDataSegment);
'            
'            System.out.println("total local heap size : " + this.m_totalLocalHeapSize);
'            

			Console.WriteLine("BLinkTreeNode <<<")
		End Sub
	End Class

End Namespace

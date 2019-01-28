Imports System

Namespace org.renjin.hdf5.message



	''' <summary>
	''' The dataspace message describes the number of dimensions (in other words, "rank") and size of each dimension that
	''' the data object has. This message is only used for datasets which have a simple, rectilinear, array-like layout;
	''' datasets requiring a more complex layout are not yet supported.
	''' </summary>
	Public Class DataspaceMessage
		Inherits Message

		Public Const MESSAGE_TYPE As Integer = &H1

		Public Enum Type
			''' <summary>
			''' A scalar dataspace; in other words, a dataspace with a single, dimensionless element.
			''' </summary>
			SCALAR

			''' <summary>
			''' A simple dataspace; in other words, a dataspace with a rank greater than 0 and an appropriate number of dimensions.
			''' </summary>
			SIMPLE

			''' <summary>
			''' 	A null dataspace; in other words, a dataspace with no elements.
			''' </summary>
			NULL
		End Enum

		Private ReadOnly version As SByte
		Private dimensionality As Integer

		Private dimensionSize() As Long
		Private maximumSize() As Long
		Private permutationIndex() As Long

		Private type As Type = Type.SIMPLE

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public DataspaceMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Public Sub New(reader As org.renjin.hdf5.HeaderReader)
			version = reader.readByte()
			If version = 1 Then
				readVersion1(reader)
			ElseIf version = 2 Then
				readVersion2(reader)
			Else
				Throw New System.NotSupportedException("Dataspace Message Version: " & version)
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion1(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readVersion1(reader As org.renjin.hdf5.HeaderReader)
			dimensionality = reader.readUInt8()
			Dim flags As org.renjin.hdf5.Flags = reader.readFlags()
			reader.readReserved(1)
			reader.readReserved(4)

			dimensionSize = New Long(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				dimensionSize(i) = reader.readLength()
			Next i
			If flags.isSet(0) Then
				readMaximumSize(reader)
			End If
			If flags.isSet(1) Then
				readPermutationIndices(reader)
			End If
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readVersion2(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readVersion2(reader As org.renjin.hdf5.HeaderReader)
			dimensionality = reader.readUInt8()
			Dim flags As org.renjin.hdf5.Flags = reader.readFlags()

			Dim typeIndex As Integer = reader.readUInt8()
			type = System.Enum.GetValues(GetType(Type))(typeIndex)

			dimensionSize = New Long(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				dimensionSize(i) = reader.readLength()
			Next i
			If flags.isSet(0) Then
				readMaximumSize(reader)
			End If
		End Sub


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readMaximumSize(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readMaximumSize(reader As org.renjin.hdf5.HeaderReader)
			maximumSize = New Long(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				maximumSize(i) = reader.readLength()
			Next i
		End Sub

'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: private void readPermutationIndices(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Private Sub readPermutationIndices(reader As org.renjin.hdf5.HeaderReader)
			permutationIndex = New Long(dimensionality - 1){}
			For i As Integer = 0 To dimensionality - 1
				permutationIndex(i) = reader.readLength()
			Next i
		End Sub


		Public Overridable Property TotalElementCount As Long
			Get
				Dim count As Long = 1
				For i As Integer = 0 To dimensionality - 1
					count *= dimensionSize(i)
				Next i
				Return count
			End Get
		End Property

		Public Overridable Property Dimensionality As Integer
			Get
				Return dimensionality
			End Get
		End Property

		Public Overridable Function getDimensionSize(d As Integer) As Long
			Return dimensionSize(d)
		End Function

		Public Overridable Property Type As Type
			Get
				Return type
			End Get
		End Property
	End Class

End Namespace
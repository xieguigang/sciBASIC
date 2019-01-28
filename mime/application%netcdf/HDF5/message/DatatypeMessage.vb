Imports System.Text

Namespace org.renjin.hdf5.message




	Public Class DatatypeMessage
		Inherits Message

		Public Const MESSAGE_TYPE As Integer = &H3

		Private dataClass As DataClass
		Private ReadOnly size As Integer
		Private byteOrder As java.nio.ByteOrder
		Private signLocation As Integer
		Private bitOffset As Integer
		Private bitPrecision As Integer
		Private exponentLocation As Integer
		Private exponentSize As Integer
		Private mantissaLocation As Integer
		Private mantissaSize As Integer
		Private exponentBias As Long

		Private signed As Boolean



	  Public Enum DataClass
			FIXED_POINT
			FLOATING_POINT
			TIME
			[STRING]
			BIT_FIELD
			OPAQUE
			COMPOUND
			REFERENCE
			ENUMERATED
			VARIABLE_LENGTH
			ARRAY
	  End Enum


'JAVA TO VB CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
'ORIGINAL LINE: public DatatypeMessage(org.renjin.hdf5.HeaderReader reader) throws java.io.IOException
		Public Sub New(reader As org.renjin.hdf5.HeaderReader)


	'        
	'         * The version of the datatype message and the datatype’s class information are packed together in this field.
	'         * The version number is packed in the top 4 bits of the field and the class is contained in the bottom 4 bits.
	'         
			Dim classAndVersion As SByte = reader.readByte()
			Dim version As Integer = (classAndVersion >> 4) And &HF
			Dim dataClassIndex As Integer = (classAndVersion And &HF)

			If version <> 1 Then
				Throw New System.NotSupportedException("data type message version: " & version)
			End If

			Dim classBitField0 As org.renjin.hdf5.Flags = reader.readFlags()
			Dim classBitField8 As org.renjin.hdf5.Flags = reader.readFlags()
			Dim classBitField16 As org.renjin.hdf5.Flags = reader.readFlags()
			size = reader.readUInt32AsInt()

			dataClass = System.Enum.GetValues(GetType(DataClass))(dataClassIndex)

			If dataClass = DataClass.FLOATING_POINT Then
				If (Not classBitField0.isSet(0)) AndAlso (Not classBitField0.isSet(6)) Then
					byteOrder = java.nio.ByteOrder.LITTLE_ENDIAN
				ElseIf classBitField0.isSet(0) AndAlso (Not classBitField0.isSet(6)) Then
					byteOrder = java.nio.ByteOrder.BIG_ENDIAN
				Else
					Throw New System.NotSupportedException("Unsupported endianness")
				End If
				signLocation = org.renjin.repackaged.guava.primitives.UnsignedBytes.toInt(classBitField8.value())

				bitOffset = reader.readUInt16()
				bitPrecision = reader.readUInt16()
				exponentLocation = reader.readUInt8()
				exponentSize = reader.readUInt8()
				mantissaLocation = reader.readUInt8()
				mantissaSize = reader.readUInt8()
				exponentBias = reader.readUInt32()

			ElseIf dataClass = DataClass.FIXED_POINT Then
				If classBitField0.isSet(0) Then
					byteOrder = java.nio.ByteOrder.BIG_ENDIAN
				Else
					byteOrder = java.nio.ByteOrder.LITTLE_ENDIAN
				End If
				signed = classBitField0.isSet(3)
			End If
		End Sub

		Public Overridable Property DataClass As DataClass
			Get
				Return dataClass
			End Get
		End Property

		Public Overridable Property Signed As Boolean
			Get
				Return signed
			End Get
		End Property

		Public Overridable Property Size As Integer
			Get
				Return size
			End Get
		End Property


		Public Overridable Property DoubleIEE754 As Boolean
			Get
				Return dataClass = DataClass.FLOATING_POINT AndAlso size = 8
			End Get
		End Property

		Public Overridable Property SignedInteger32 As Boolean
			Get
				Return dataClass = DataClass.FIXED_POINT AndAlso size = 4 AndAlso signed
			End Get
		End Property

		Public Overridable Property ByteOrder As java.nio.ByteOrder
			Get
			  Return byteOrder
			End Get
		End Property


	  Public Overrides Function ToString() As String
			Dim sb As New StringBuilder("Datatype{")
			If DoubleIEE754 Then
				sb.Append("IEE754 Double")
			Else
				If Not signed Then
					sb.Append("UNSIGNED ")
				End If
				sb.Append(dataClass)
				sb.Append("(")
				sb.Append(size)
				sb.Append(")")
			End If
			sb.Append("}")
			Return sb.ToString()
	  End Function
	End Class

End Namespace
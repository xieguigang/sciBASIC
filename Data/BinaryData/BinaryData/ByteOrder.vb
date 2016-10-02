
''' <summary>
''' Represents the possible endianness of binary data.
''' </summary>
Public Enum ByteOrder As UShort
	''' <summary>
	''' The binary data is present in big endian.
	''' </summary>
	BigEndian = &Hfeff

	''' <summary>
	''' The binary data is present in little endian.
	''' </summary>
	LittleEndian = &Hfffe
End Enum

''' <summary>
''' Represents helper methods to handle data byte order.
''' </summary>
Public NotInheritable Class ByteOrderHelper
	Private Sub New()
	End Sub
	' ---- MEMBERS ------------------------------------------------------------------------------------------------

	Private Shared _systemByteOrder As ByteOrder

	' ---- PROPERTIES ---------------------------------------------------------------------------------------------

	''' <summary>
	''' Gets the <see cref="ByteOrder"/> of the system executing the assembly.
	''' </summary>
	Public Shared ReadOnly Property SystemByteOrder() As ByteOrder
		Get
			If _systemByteOrder = 0 Then
				_systemByteOrder = If(BitConverter.IsLittleEndian, ByteOrder.LittleEndian, ByteOrder.BigEndian)
			End If
			Return _systemByteOrder
		End Get
	End Property
End Class

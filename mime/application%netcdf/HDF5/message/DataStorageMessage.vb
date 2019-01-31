Imports System.Collections.Generic

Namespace org.renjin.hdf5.message



	''' <summary>
	''' This message describes the filter pipeline which should be applied to the data stream by providing filter
	''' identification numbers, flags, a name, and client data.
	''' 
	''' <p>This message may be present in the object headers of both dataset and group objects.
	''' For datasets, it specifies the filters to apply to raw data. For groups, it specifies the filters to apply to the
	''' group’s fractal heap. Currently, only datasets using chunked data storage use the filter pipeline on their raw data.</p>
	''' </summary>
	Public Class DataStorageMessage
        Inherits MessageBase

        Public Const MESSAGE_TYPE As Integer = &HB

        Public Overridable Property Filters As New List(Of Filter)

        Public Sub New(reader As org.renjin.hdf5.HeaderReader)
			Dim version As SByte = reader.readByte()
			If version = 1 Then
				readVersion1(reader)
			ElseIf version = 2 Then
				readVersion2(reader)
			Else
				Throw New System.NotSupportedException("version: " & version)
			End If
		End Sub

		Private Sub readVersion1(reader As org.renjin.hdf5.HeaderReader)
			Dim numFilters As Integer = reader.readUInt8()
			reader.readReserved(2)
			reader.readReserved(4)

            For i As Integer = 0 To numFilters - 1
                Dim filterId As Integer = reader.readUInt16()

                '            
                '             * Each filter has an optional null-terminated ASCII name and this field holds the length of the name
                '             * including the null termination padded with nulls to be a multiple of eight. If the filter has
                '             * no name then a value of zero is stored in this field.
                '             
                Dim nameLength As Integer = reader.readUInt16()

                Dim flags As Integer = reader.readUInt16()
                Dim [optional] As Boolean = (flags And &H1) <> 0

                Dim numClientDataValues As Integer = reader.readUInt16()

                Dim name As String = Nothing
                If nameLength <> 0 Then
                    name = reader.readNullTerminatedAsciiString(nameLength)
                End If

                Dim clientData() As Integer = reader.readIntArray(numClientDataValues)

                '            
                '             * Four bytes of zeroes are added to the message at this point if the Client Data Number of
                '             * Values field contains an odd number.
                '             
                If numClientDataValues Mod 2 <> 0 Then
                    reader.readReserved(4)
                End If

                filters.Add(New Filter(filterId, name, clientData, [optional]))
            Next
        End Sub

		Private Sub readVersion2(reader As org.renjin.hdf5.HeaderReader)
			Dim numFilters As Integer = reader.readUInt8()

			For i As Integer = 0 To numFilters - 1
				Dim filterId As Integer = reader.readUInt16()

	'            
	'             * Each filter has an optional null-terminated ASCII name and this field holds the length of the name
	'             * including the null termination padded with nulls to be a multiple of eight. If the filter has no name
	'             * then a value of zero is stored in this field.
	'             
				Dim nameLength As Integer = 0

	'            
	'             * Filters with IDs less than 256 (in other words, filters that are defined in this format documentation)
	'             * do not store the Name Length or Name fields.
	'             
				If filterId >= 256 Then
					nameLength = reader.readUInt16()
				End If

				Dim flags As Integer = reader.readUInt16()
				Dim [optional] As Boolean = (flags And &H1) <> 0

				Dim numClientDataValues As Integer = reader.readUInt16()

				Dim name As String = Nothing
				If nameLength <> 0 Then
					reader.readString(nameLength, org.renjin.repackaged.guava.base.Charsets.US_ASCII)
				End If

				Dim clientData() As Integer = reader.readIntArray(numClientDataValues)

				filters.Add(New Filter(filterId, name, clientData, [optional]))
			Next i
		End Sub

    End Class

End Namespace
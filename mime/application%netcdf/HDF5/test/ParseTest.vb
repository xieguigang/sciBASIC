
'
' * Written by iychoi@email.arizona.edu
' 



Imports DataChunk = edu.arizona.cs.hdf5.structure.DataChunk
Imports DataTypeMessage = edu.arizona.cs.hdf5.structure.DataTypeMessage
Imports Layout = edu.arizona.cs.hdf5.structure.Layout
Imports LayoutField = edu.arizona.cs.hdf5.structure.LayoutField
Imports BinaryReader = edu.arizona.cs.hdf5.io.BinaryReader
Imports System.Collections.Generic
Imports System.IO
Namespace edu.arizona.cs.hdf5.test

	Public Class ParseTest
		Public Shared Sub Main(args As String())

			If args.Length < 1 Then
				Console.WriteLine("Error : inputfile is necessary")
				Return
			End If

			Dim [option] As String = ""
			Dim filename As String = ""

			If args.Length = 2 Then
				[option] = args(0)
				filename = args(1)
			ElseIf args.Length = 1 Then
				filename = args(0)
			End If

			If filename.Length = 0 Then
				Console.WriteLine("Error : inputfile is necessary")
				Return
			End If

			' check option
			Dim showHeader As Boolean = False
			Dim showData As Boolean = False
			If [option].Contains("h") Then
				' header
				showHeader = True
			End If
			If [option].Contains("d") Then
				' data
				showData = True
			End If

			Try
				Dim reader As New HDF5Reader(filename, "dset")
				reader.parseHeader()

				If showHeader Then
					Dim headerSize As Long = reader.headerSize
					Console.WriteLine("header size : " & headerSize)
				End If
				' layout
				Dim layout As Layout = reader.layout

				Dim dims As Integer = layout.numberOfDimensions
				If showHeader Then
					Console.WriteLine("dimensions : " & dims)
				End If

				Dim chunkSize As Integer() = layout.chunkSize
				Dim dlength As Integer() = layout.dimensionLength
				Dim maxdlength As Integer() = layout.maxDimensionLength

				If showHeader Then
					For i As Integer = 0 To dims - 1
						If chunkSize.Length > i Then
							Console.WriteLine("chunk size[" & i & "] : " & chunkSize(i))
						End If

						If dlength.Length > i Then
							Console.WriteLine("dimension length[" & i & "] : " & dlength(i))
						End If

						If maxdlength.Length > i Then
							Console.WriteLine("max dimension length[" & i & "] : " & maxdlength(i))
						End If
					Next
				End If

				Dim fields As List(Of LayoutField) = layout.fields

				' chunk
				Dim chunkReader As BinaryReader = reader.reader
				chunkReader.setLittleEndian()


				Dim dataTotal As Integer = dlength(0)
				Dim readCount As Integer = 0

				Dim chunks As List(Of DataChunk) = reader.chunks
				For Each chunk As DataChunk In chunks

					Dim filepos As Long = chunk.filePosition
					If showHeader Then
						chunk.printValues()
					End If

					chunkReader.offset = filepos

					If showData Then
						Dim dataCountPerChunk As Integer = chunk.size \ chunkSize(0)
						For i As Integer = 0 To dataCountPerChunk - 1
							Dim bytes As SByte() = chunkReader.readBytes(chunkSize(0))

							For j As Integer = 0 To fields.Count - 1
								Dim field As LayoutField = fields(j)

								Dim offset As Integer = field.offset
								Dim len As Integer = field.byteLength
								Dim dataType As Integer = field.dataType
								Dim ndims As Integer = field.nDims
								Dim name As String = field.name

								If dataType = DataTypeMessage.DATATYPE_STRING Then
									Dim val As String = StringHelperClass.NewString(bytes, offset, len)
									Console.WriteLine(name & " : " & val.Trim())
								End If
							Next

							readCount += 1

							If readCount >= dataTotal Then
								Exit For
							End If
						Next
					End If

					If readCount >= dataTotal Then
						Exit For
					End If
				Next


				reader.close()
			Catch e As IOException
				' TODO Auto-generated catch block
				Console.WriteLine(e.ToString())
				Console.Write(e.StackTrace)
			End Try
		End Sub
	End Class

End Namespace

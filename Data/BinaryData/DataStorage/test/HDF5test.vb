#Region "Microsoft.VisualBasic::00ef9efaf437169c92c0ad2bf0375155, mime\application%netcdf\test\HDF5test.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Class ParseTest
' 
'         Sub: Main
' 
' 
' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 


Imports Microsoft.VisualBasic.Data.IO.HDF5
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace edu.arizona.cs.hdf5.test

    Public Class ParseTest
        Public Shared Sub Main(args As String())
            Dim [option] As String = "hd"
            Dim filename As String = "D:\GCModeller\src\runtime\sciBASIC#\Data\BinaryData\data\EP388069_K40_BS1D.otu_table.biom"

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

            Dim reader As New HDF5Reader(filename, "sample")
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
                        Dim bytes As Byte() = chunkReader.readBytes(chunkSize(0))

                        For j As Integer = 0 To fields.Count - 1
                            Dim field As LayoutField = fields(j)

                            Dim offset As Integer = field.offset
                            Dim len As Integer = field.byteLength
                            Dim dataType As DataTypes = field.dataType
                            Dim ndims As Integer = field.nDims
                            Dim name As String = field.name

                            If dataType = DataTypes.DATATYPE_STRING Then
                                Dim val As String = bytes.ByteString(offset, len)
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

            reader.Dispose()
        End Sub
    End Class

End Namespace

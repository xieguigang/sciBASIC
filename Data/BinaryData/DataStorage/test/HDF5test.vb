#Region "Microsoft.VisualBasic::68854b1dac6d0150133a56b17793d6de, Data\BinaryData\DataStorage\test\HDF5test.vb"

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


    ' Code Statistics:

    '   Total Lines: 144
    '    Code Lines: 91
    ' Comment Lines: 14
    '   Blank Lines: 39
    '     File Size: 5.27 KB


    '     Class ParseTest
    ' 
    '         Sub: dumpData, Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Written by iychoi@email.arizona.edu
' 


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5
Imports Microsoft.VisualBasic.Data.IO.HDF5.device
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.type
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace edu.arizona.cs.hdf5.test

    Public Class ParseTest

        Private Shared Sub dumpData(reader As HDF5Reader, showHeader As Boolean, showData As Boolean, dumpFile$)

            Dim result = reader.dataset.data(reader.superblock)

            If showHeader Then
                Dim headerSize As Long = reader.headerSize
                Console.WriteLine("header size : " & headerSize)
            End If
            ' layout
            Dim layout As Layout = reader.layout

            Dim dims As Integer = layout.numberOfDimensions
            Dim chunkSize As Integer() = layout.chunkSize
            Dim dlength As Integer() = layout.dimensionLength
            Dim maxdlength As Integer() = layout.maxDimensionLength
            Dim fields As List(Of LayoutField) = layout.fields

            ' chunk
            Dim chunkReader As BinaryReader = reader.reader
            Dim dataTotal As Integer = dlength(0)
            Dim readCount As Integer = 0
            Dim chunks As List(Of DataChunk) = reader.chunks

            Call chunkReader.SetByteOrder(ByteOrder.LittleEndian)

            Using text As StreamWriter = dumpFile.OpenWriter

                Call DirectCast(layout, IFileDump).printValues(text)

                For Each chunk As DataChunk In chunks

                    Dim filepos As Long = chunk.filePosition

                    If showHeader Then
                        DirectCast(chunk, IFileDump).printValues(console:=text)
                    End If

                    chunkReader.offset = filepos

                    If showData Then

                        '  Dim dataValue = reader.dataType.reader.readDataset(filepos, reader.dataSpace, reader.superblock, reader.dataSpace.dimensionLength)


                        Dim dataCountPerChunk As Integer = chunk.sizeOfChunk \ chunkSize(0)
                        For i As Integer = 0 To dataCountPerChunk - 1
                            Dim bytes As Byte() = chunkReader.readBytes(chunkSize(0))

                            Dim dataValue = reader.dataType.reader.ParseDataChunk(bytes, reader.dataSpace.dimensionLength)


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
            End Using
        End Sub

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

            Dim file As New HDF5File(filename)
            Dim reader As HDF5Reader = file!sample
            ' reader.parseHeader()

            ' Dim ids = reader.ParseDataObject("matrix")

            '  Dim data = ids.ParseDataObject("data")

            '  Call dumpData(data, True, True, "./test.dump")

            ' Data = Nothing
            Dim Data = file("/sample/matrix/data")

            Call dumpData(data, True, True, "./test2.dump")
        End Sub
    End Class

End Namespace

#Region "Microsoft.VisualBasic::31acbad62043abcfca0f22231e974719, sciBASIC#\Data\BinaryData\test\bztest.vb"

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

    '   Total Lines: 108
    '    Code Lines: 62
    ' Comment Lines: 31
    '   Blank Lines: 15
    '     File Size: 4.15 KB


    '     Class UnitTests
    ' 
    '         Sub: CompressAndDecompress, CompressSmokeLarge, CompressSmokeSmall, CrcAlgorithmDifferentValues, CrcAlgorithmSameValues
    '              Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Data.IO.Bzip2
Imports Microsoft.VisualBasic.Data.IO.Bzip2.Math

Namespace Bzip2

    ''' <summary>
    ''' Unit test for the BZip2 compression library
    ''' </summary>
    Public Class UnitTests
        Private Const BufferSizeLarge As Integer = 10000000 ' Almost 10 Mb
        Private Const BufferSizeSmall As Integer = 100000   ' Around 100 Kb
        Private Shared Buffer As Byte() = New Byte(9999999) {}

        ''' <summary>
        ''' Fills the test buffer with random values
        ''' </summary>
        Public Shared Sub Main()
            'Dim random = New Random()
            ' Random.NextBytes(Buffer)

            Buffer = Encoding.UTF8.GetBytes("Hello World!")

            '  Call CrcAlgorithmDifferentValues()
            '  Call CrcAlgorithmSameValues()
            '  Call CompressSmokeLarge()
            '  Call CompressSmokeSmall()
            Call CompressAndDecompress()

            Pause()
        End Sub

        ''' <summary>
        ''' Performs a CRC check and compare against well-known results
        ''' The buffer has different values
        ''' </summary>
        Public Shared Sub CrcAlgorithmDifferentValues()
            Dim buffer As Byte() = {&H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8, &H9, &HA, &HF1, &HF2, &HF3, &HF4, &HF5, &HF6, &HF7, &HF8, &HF9, &HFA}
            Dim crc = New CRC32()

            For i = 0 To buffer.Length - 1
                crc.UpdateCrc(buffer(i))
            Next

            Assert.AreEqual(CInt(crc.CRC), &H8AEE127A)
        End Sub

        ''' <summary>
        ''' Performs a CRC check and compare against well-known results
        ''' The buffer has different values
        ''' </summary>
        Public Shared Sub CrcAlgorithmSameValues()
            Dim crc = New CRC32()
            crc.UpdateCrc(&H55, 10)
            Assert.AreEqual(CInt(crc.CRC), &HA1E07747)
        End Sub

        ''' <summary>
        ''' Compresses the full buffer and checks for a reasonable compressed size
        ''' </summary>
        Public Shared Sub CompressSmokeLarge()
            Dim input = New MemoryStream(Buffer)
            Dim output = New MemoryStream()
            Dim compressor = New BZip2OutputStream(output, False)
            input.CopyTo(compressor)
            compressor.Close()

            ' Estimated size between inputSize*0.5 and inputSize*1.1
            Assert.IsTrue(output.Length > BufferSizeLarge * 0.5)
            Assert.IsTrue(output.Length < BufferSizeLarge * 1.1)
        End Sub

        ''' <summary>
        ''' Compresses a portion of the buffer and checks for a reasonable compressed size
        ''' </summary>
        Public Shared Sub CompressSmokeSmall()
            Dim input = New MemoryStream(Buffer, 0, BufferSizeSmall)
            Dim output = New MemoryStream()
            Dim compressor = New BZip2OutputStream(output, False)
            input.CopyTo(compressor)
            compressor.Close()

            ' Estimated size between inputSize*0.5 and inputSize*1.1
            Assert.IsTrue(output.Length > BufferSizeSmall * 0.5)
            Assert.IsTrue(output.Length < BufferSizeSmall * 1.1)
        End Sub

        ''' <summary>
        ''' Compresses and decompresses a long random buffer
        ''' </summary>
        Public Shared Sub CompressAndDecompress()
            Dim input = New MemoryStream(Buffer)
            Dim output = New MemoryStream()
            Dim compressor = New BZip2OutputStream(output, False)
            input.CopyTo(compressor)
            compressor.Close()
            Assert.IsTrue(output.Length > 4)
            output.Position = 0
            Dim output2 = New MemoryStream()
            Dim decompressor = New BZip2InputStream(output, False)
            decompressor.CopyTo(output2)
            Assert.AreEqual(Buffer.Length, output2.Length)
            Assert.IsTrue(Buffer.SequenceEqual(output2.ToArray))
        End Sub
    End Class
End Namespace

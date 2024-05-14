#Region "Microsoft.VisualBasic::fd6f945e33e1ee3140e0d6e29af3e70e, Data\BinaryData\HDSPack\BinaryStream\TreeWriter.vb"

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

    '   Total Lines: 60
    '    Code Lines: 44
    ' Comment Lines: 6
    '   Blank Lines: 10
    '     File Size: 2.25 KB


    ' Module TreeWriter
    ' 
    '     Function: (+2 Overloads) GetBuffer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Text

Friend Module TreeWriter

    ''' <summary>
    ''' save tree data into data buffer
    ''' </summary>
    ''' <param name="root"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetBuffer(root As StreamGroup, type As Index(Of String)) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms, encoding:=Encodings.UTF8WithoutBOM) With {.ByteOrder = ByteOrder.BigEndian}
            Dim buf As Byte()
            Dim attrs As Byte() = root.Pack(type)

            Call bin.Write(root.files.Length)
            Call bin.Write(root.referencePath.ToString, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(attrs.Length)
            Call bin.Write(attrs)

            For Each file As StreamObject In root.files
                If TypeOf file Is StreamGroup Then
                    buf = DirectCast(file, StreamGroup).GetBuffer(type)
                    bin.Write(DirectCast(file, StreamGroup).files.Length + 1)
                Else
                    buf = DirectCast(file, StreamBlock).GetBuffer(type)
                    bin.Write(0)
                End If

                bin.Write(buf)
                bin.Flush()
            Next

            Return ms.ToArray
        End Using
    End Function

    <Extension>
    Private Function GetBuffer(file As StreamBlock, type As Index(Of String)) As Byte()
        Using ms As New MemoryStream, bin As New BinaryDataWriter(ms, encoding:=Encodings.UTF8WithoutBOM) With {.ByteOrder = ByteOrder.BigEndian}
            Dim attrs As Byte() = file.Pack(type)

            Call bin.Write(file.referencePath.ToString, BinaryStringFormat.ZeroTerminated)
            Call bin.Write(file.offset)
            Call bin.Write(file.size)
            Call bin.Write(attrs.Length)
            Call bin.Write(attrs)
            Call bin.Flush()

            Return ms.ToArray
        End Using
    End Function

End Module

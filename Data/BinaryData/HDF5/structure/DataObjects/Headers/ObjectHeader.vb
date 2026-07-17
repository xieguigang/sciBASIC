#Region "Microsoft.VisualBasic::56669ab2ff152a6f9c1da4adcfa15860, Data\BinaryData\HDF5\structure\DataObjects\Headers\ObjectHeader.vb"

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

    '   Total Lines: 96
    '    Code Lines: 62 (64.58%)
    ' Comment Lines: 9 (9.38%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 25 (26.04%)
    '     File Size: 3.67 KB


    '     Class ObjectHeader
    ' 
    '         Properties: headerMessages, objectHeaderSize, objectReferenceCount, totalNumberOfHeaderMessages, version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: readVersion1
    ' 
    '         Sub: printValues, readVersion2
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * Mostly copied from NETCDF4 source code.
' * refer : http://www.unidata.ucar.edu
' * 
' * Modified by iychoi@email.arizona.edu
' 


Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.device.BinaryReader

Namespace struct

    Public Class ObjectHeader : Inherits HDF5Ptr

        Shared ReadOnly OHDR_SIGNATURE As Byte() = {&H4F, &H48, &H44, &H52} _
            .Select(Function(i) CByte(i)) _
            .ToArray

        Public ReadOnly Property version As Integer
        Public ReadOnly Property totalNumberOfHeaderMessages As Integer
        Public ReadOnly Property objectReferenceCount As Integer
        Public ReadOnly Property objectHeaderSize As Integer
        Public ReadOnly Property headerMessages As New List(Of ObjectHeaderMessage)

        Public Sub New(sb As Superblock, address As Long)
            Call MyBase.New(address)

            Dim [in] As BinaryReader = sb.FileReader(address)

            Me.version = [in].readByte()

            If Me.version = 1 Then

                [in].skipBytes(1)

                Me.totalNumberOfHeaderMessages = [in].readShort()
                Me.objectReferenceCount = [in].readInt()
                Me.objectHeaderSize = [in].readInt()

                [in].skipBytes(4)

                readVersion1([in], sb, [in].offset, Me.totalNumberOfHeaderMessages, Long.MaxValue)
            ElseIf Me.version = &H4F Then
                ' The first byte is 'O' of the "OHDR" signature of a version 2 object header.
                Call readVersion2([in], sb, address)
            Else
                Throw New IOException("unsupported object header version: " & Me.version)
            End If
        End Sub

        Private Function readVersion1([in] As BinaryReader, sb As Superblock, address As Long, readMessages As Integer, maxBytes As Long) As Integer
            Dim count As Integer = 0
            Dim byteRead As Integer = 0
            ' read messages
            Dim messageOffset As Long = address

            [in].offset = address

            While count < readMessages AndAlso byteRead < maxBytes
                Dim msg As New ObjectHeaderMessage([in], sb, messageOffset)

                messageOffset += msg.headerLength + msg.sizeOfHeaderMessageData
                byteRead += msg.headerLength + msg.sizeOfHeaderMessageData

                count += 1

                If msg.headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
                    ' CONTINUE
                    Dim cmsg As ContinueMessage = msg.continueMessage
                    Dim continuationBlockFilePos As Long = cmsg.offset

                    count += readVersion1([in], sb, continuationBlockFilePos, readMessages - count, cmsg.length)
                ElseIf msg.headerMessageType IsNot ObjectHeaderMessageType.NIL Then
                    ' NOT NIL
                    Me.headerMessages.Add(msg)
                End If
            End While
            Return count
        End Function

        Private Sub readVersion2([in] As BinaryReader, sb As Superblock, address As Long)
            Call readHeaderChunk([in], sb, address, 0)
        End Sub

        ''' <summary>
        ''' Reads a single OHDR chunk located at <paramref name="chunkAddress"/> and appends its
        ''' (non-NIL) messages to <see cref="headerMessages"/>. Continuation messages are followed
        ''' recursively. Messages in a version 2 object header are stored packed (no 8-byte padding)
        ''' and are preceded by a 5-byte prefix (type, size, flags) instead of the 8-byte v1 prefix.
        ''' </summary>
        Private Sub readHeaderChunk([in] As BinaryReader, sb As Superblock, chunkAddress As Long, depth As Integer)
            If depth > 64 Then
                Throw New IOException("too many object header continuation blocks")
            End If

            [in].offset = chunkAddress

            Dim sig = [in].readBytes(4)
            If Not sig.SequenceEqual(OHDR_SIGNATURE) Then
                Throw New IOException("invalid object header (OHDR) signature")
            End If

            Me.version = [in].readByte()
            Dim ohFlags = [in].readByte()

            ' bit 5 (0x20): access/modification/change/birth times (4 x 32-bit)
            If (ohFlags And &H20) <> 0 Then
                [in].skipBytes(16)
            End If

            ' bit 4 (0x10): non-default attribute storage phase change values (2 x 16-bit)
            If (ohFlags And &H10) <> 0 Then
                [in].skipBytes(4)
            End If

            ' bits 0-1: size of the "Size of Chunk #0" field (1/2/4/8 bytes)
            Dim chunkSizeField = ({1, 2, 4, 8})(ohFlags And &H3)
            Dim chunkSize = ReadHelper.readVariableSizeUnsigned([in], chunkSizeField)
            Dim chunkEnd = [in].offset + chunkSize

            ' bit 2 (0x04): per-message creation order field present
            Dim messageCreationOrder = (ohFlags And &H4) <> 0

            While [in].offset < chunkEnd
                Dim msgStart = [in].offset
                Dim msg As New ObjectHeaderMessage([in], sb, msgStart, 2, messageCreationOrder)

                If msg.headerMessageType Is ObjectHeaderMessageType.ObjectHeaderContinuation Then
                    Dim cmsg = msg.continueMessage
                    Call readHeaderChunk([in], sb, cmsg.offset, depth + 1)
                ElseIf msg.headerMessageType IsNot ObjectHeaderMessageType.NIL Then
                    Me.headerMessages.Add(msg)
                End If

                [in].offset = msgStart + msg.headerLength + msg.sizeOfHeaderMessageData
            End While

            ' 4-byte checksum at the end of the chunk
            [in].skipBytes(4)
        End Sub

        Protected Friend Overrides Sub printValues(console As TextWriter)
            console.WriteLine("ObjectHeader >>>")
            console.WriteLine("address : " & Me.m_address)
            console.WriteLine("version : " & Me.version)
            console.WriteLine("number of messages : " & Me.totalNumberOfHeaderMessages)
            console.WriteLine("object reference count : " & Me.objectReferenceCount)
            console.WriteLine("object header size : " & Me.objectHeaderSize)

            For i As Integer = 0 To Me.headerMessages.Count - 1
                Me.headerMessages(i).printValues(console)
            Next

            console.WriteLine("ObjectHeader <<<")
        End Sub
    End Class

End Namespace

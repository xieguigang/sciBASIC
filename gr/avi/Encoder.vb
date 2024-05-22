#Region "Microsoft.VisualBasic::aad705cc49500f777372edcd446ab136, gr\avi\Encoder.vb"

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

    '   Total Lines: 99
    '    Code Lines: 71 (71.72%)
    ' Comment Lines: 6 (6.06%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 22 (22.22%)
    '     File Size: 3.05 KB


    ' Class Encoder
    ' 
    '     Properties: main, settings, streams
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: getVideoDataLength, getVideoHeaderLength
    ' 
    '     Sub: WriteBuffer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

''' <summary>
''' A simple VB.NET AVI encoder
''' 
''' > https://github.com/Sebmaster/avi.js
''' </summary>
Public Class Encoder

    Public ReadOnly Property settings As Settings
    Public ReadOnly Property streams As New List(Of AVIStream)
    Public ReadOnly Property main As AVIMainHeader

    Sub New(settings As Settings)
        Me.settings = settings
        Me.main = New AVIMainHeader(Me)
    End Sub

    Public Sub WriteBuffer(path As String)
        Dim dataOffset As Integer() = New Integer(Me.streams.Count - 1) {}
        Dim offset& = 0
        Dim streamHeaderLength = 0

        For i As Integer = 0 To Me.streams.Count - 1
            streamHeaderLength += getVideoHeaderLength(Me.streams(i).frames.Count)
            dataOffset(i) = offset
            offset += getVideoDataLength(streams(i))
        Next

        Dim moviOffset = streamHeaderLength + 12 + ' RIFF 
            12 + ' hdrl 
            8 +  ' avih  
            56 + ' struct  
            12   ' movi 

        Dim buffer As New UInt8Array(path, moviOffset + offset)

        buffer.writeString(0, "RIFF") ' 0
        buffer.writeString(8, "AVI ") ' 8

        buffer.writeString(12, "LIST")
        buffer.writeInt(16, 68 + streamHeaderLength)
        buffer.writeString(20, "hdrl") ' hdrl list

        Call main.Write(buffer)

        Dim len& = 88
        Dim dataOffsetValue&
        Dim subChunk As UInt8Array

        offset = Scan0

        For i As Integer = 0 To Me.streams.Count - 1
            dataOffsetValue = moviOffset + dataOffset(i)
            subChunk = buffer.subarray(88 + offset)
            len += Me.streams(i).writeHeaderBuffer(subChunk, i, dataOffsetValue)
        Next

        buffer.writeString(len, "LIST")
        buffer.writeString(len + 8, "movi")

        Dim moviLen& = 4

        For i As Integer = 0 To Me.streams.Count - 1
            dataOffsetValue = len + 8 + moviLen
            subChunk = buffer.subarray(dataOffsetValue)
            moviLen += streams(i).writeDataBuffer(subChunk, i)
        Next

        buffer.writeInt(len + 4, moviLen)
        buffer.writeInt(4, len + moviLen)

        Call buffer.Dispose()
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function getVideoHeaderLength(frameLen As Integer) As Integer
        Return 12 + ' strl 
            8 +     ' strh 
            56 +    ' struct  
            8 +     ' strf 
            40 +    ' struct  
            8 +     ' indx 
            24 +    ' struct 
            frameLen * 4 * 2
    End Function

    Public Shared Function getVideoDataLength(stream As AVIStream) As Long
        Dim len& = 0
        Dim frames = stream.frames

        For i As Integer = 0 To frames.Count - 1
            ' Pad if chunk Not in word boundary
            len += 8 + frames(i).length + If(frames(i).length Mod 2 = 0, 0, 1)
        Next

        Return len
    End Function
End Class

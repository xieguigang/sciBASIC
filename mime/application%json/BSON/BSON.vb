#Region "Microsoft.VisualBasic::25b3b0256c1dcc3a95046b555ec29ab2, mime\application%json\BSON\BSON.vb"

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

    '   Total Lines: 79
    '    Code Lines: 54 (68.35%)
    ' Comment Lines: 12 (15.19%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 13 (16.46%)
    '     File Size: 2.48 KB


    '     Module BSONFormat
    ' 
    '         Function: GetBuffer, (+3 Overloads) Load, SafeGetBuffer
    ' 
    '         Sub: SafeWriteBuffer, WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.Parallel

Namespace BSON

    Public Module BSONFormat

        ''' <summary>
        ''' 解析BSON
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <returns></returns>
        Public Function Load(buf As Byte()) As JsonObject
            Using decoder As New Decoder(buf)
                Return decoder.decodeDocument()
            End Using
        End Function

        Public Function Load(buf As RequestStream) As JsonObject
            Return Load(buf.ChunkBuffer)
        End Function

        Public Function Load(buf As Stream, Optional leaveOpen As Boolean = False) As JsonObject
            If buf.Length = 0 Then
                ' 20221008
                ' is empty object?
                Return New JsonObject
            Else
                Using decoder As New Decoder(buf, leaveOpen:=leaveOpen)
                    Return decoder.decodeDocument()
                End Using
            End If
        End Function

        ''' <summary>
        ''' usually apply this function for load MongoDB database file
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <param name="tqdm"></param>
        ''' <returns></returns>
        Public Iterator Function LoadList(buf As Stream, Optional tqdm As Boolean = False) As IEnumerable(Of JsonObject)
            If buf.Length = 0 Then
                Return
            End If

            Dim target As Long = buf.Length - 3

            Using decoder As New Decoder(buf)
                If tqdm Then
                    For Each obj As JsonObject In TqdmWrapper.WrapStreamReader(
                        bytesOfStream:=target,
                        request:=Function(ByRef offset, bar)
                                     offset = buf.Position

                                     If offset >= target Then
                                         Return Nothing
                                     Else
                                         Return decoder.decodeDocument
                                     End If
                                 End Function)

                        If obj Is Nothing AndAlso buf.Position >= target Then
                            Exit For
                        Else
                            Yield obj
                        End If
                    Next
                Else
                    Do While buf.Position < target
                        Yield decoder.decodeDocument
                    Loop
                End If
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub WriteBuffer(obj As JsonObject, buffer As Stream)
            Call New Encoder().encodeDocument(buffer, obj)
        End Sub

        ''' <summary>
        ''' 只兼容array或者object
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function SafeGetBuffer(obj As JsonElement) As MemoryStream
            Dim ms As New MemoryStream

            Call obj.SafeWriteBuffer(ms)
            Call ms.Flush()
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return ms
        End Function

        <Extension>
        Public Sub SafeWriteBuffer(obj As JsonElement, ms As Stream)
            If TypeOf obj Is JsonObject Then
                Call WriteBuffer(obj, buffer:=ms)
            ElseIf TypeOf obj Is JsonArray Then
                Call New Encoder().encodeArray(ms, obj)
            Else
                Throw New NotSupportedException
            End If
        End Sub

        Public Function GetBuffer(obj As JsonObject) As MemoryStream
            Dim ms As New MemoryStream

            Call WriteBuffer(obj, buffer:=ms)
            Call ms.Flush()
            Call ms.Seek(Scan0, SeekOrigin.Begin)

            Return ms
        End Function
    End Module
End Namespace

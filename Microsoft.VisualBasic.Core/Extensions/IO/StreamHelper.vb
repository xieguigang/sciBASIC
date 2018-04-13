#Region "Microsoft.VisualBasic::d65ce5e32de13636b6b46ef7b2d3c7f9, Microsoft.VisualBasic.Core\Extensions\IO\StreamHelper.vb"

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

    ' Module StreamHelper
    ' 
    '     Function: CopyStream
    ' 
    '     Sub: Write, WriteLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Module StreamHelper

    ''' <summary>
    ''' Download stream data from the http response.
    ''' </summary>
    ''' <param name="stream">
    ''' Create from <see cref="WebServiceUtils.GetRequestRaw(String, Boolean, String)"/>
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("Stream.Copy", Info:="Download stream data from the http response.")>
    <Extension> Public Function CopyStream(stream As Stream) As MemoryStream
        If stream Is Nothing Then
            Return New MemoryStream
        End If

        Dim buffer As Byte() = New Byte(64 * 1024) {}
        Dim i As New Value(Of Integer)

        With New MemoryStream()
            Do While i = stream.Read(buffer, 0, buffer.Length) > 0
                Call .Write(buffer, 0, i)
            Loop

            Return .ByRef
        End With
    End Function

    <Extension>
    Public Sub Write(stream As Stream, value$, Optional encoding As Encoding = Nothing)
        With (encoding Or UTF8).GetBytes(value)
            Call stream.Write(.ByRef, Scan0, .Length)
            Call stream.Flush()
        End With
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Sub WriteLine(stream As Stream, Optional value$ = "", Optional encoding As Encoding = Nothing, Optional newLine$ = vbCrLf)
        Call stream.Write(value & newLine, encoding)
    End Sub
End Module


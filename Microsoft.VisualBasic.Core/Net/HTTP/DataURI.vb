#Region "Microsoft.VisualBasic::cf8bdfeb8b26eaa233423c9071f9767d, Microsoft.VisualBasic.Core\Net\HTTP\DataURI.vb"

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

    '     Class DataURI
    ' 
    '         Properties: base64, chartSet, mime
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: FromFile, IsWellFormedUriString, SVGImage, ToStream, ToString
    '                   URIParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Text

Namespace Net.Http

    ''' <summary>
    ''' Data URI scheme
    ''' </summary>
    Public Class DataURI

        ''' <summary>
        ''' File mime type
        ''' </summary>
        Public ReadOnly Property mime As String
        ''' <summary>
        ''' The base64 string
        ''' </summary>
        Public ReadOnly Property base64 As String
        Public ReadOnly Property chartSet As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="codepage$">
        ''' The chartset codepage name, by default is ``ASCII``.
        ''' </param>
        Sub New(file$, Optional codepage$ = Nothing)
            mime = Strings.LCase(file.FileMimeType.MIMEType)
            base64 = file.ReadBinary.ToBase64String
            chartSet = codepage
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(image As Image)
            Call Me.New(image.ToBase64String, ContentTypes.MIME.Png, Nothing)
        End Sub

        Sub New(stream As Stream, mime$, Optional charset$ = Nothing)
            Me.mime = mime
            Me.chartSet = charset

            Dim chunkbuffer As New List(Of Byte)
            Dim buffer As Byte() = New Byte(1024) {}
            Dim nreads As Integer

            Do While stream.Position < stream.Length
                nreads = stream.Read(buffer, Scan0, buffer.Length)

                If nreads <= 0 Then
                    Exit Do
                Else
                    Call chunkbuffer.AddRange(buffer.Take(nreads))
                End If
            Loop

            Me.base64 = chunkbuffer.ToBase64String
        End Sub

        Public Sub New(base64$, mime$, Optional charset$ = Nothing)
            Me.base64 = base64
            Me.mime = mime
            Me.chartSet = charset
        End Sub

        ''' <summary>
        ''' <see cref="Convert.FromBase64String"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function ToStream() As Stream
            Return New MemoryStream(Convert.FromBase64String(base64))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromFile(file As String) As DataURI
            Return New DataURI(file)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function SVGImage(svg As String) As DataURI
            Return New DataURI(base64:=TextEncodings.UTF8WithoutBOM.GetBytes(svg).ToBase64String, mime:="image/svg+xml")
        End Function

        ''' <summary>
        ''' 这个只能够从字符串的特征来初步判断是否是Data URI字符串
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        Public Shared Function IsWellFormedUriString(str As String) As Boolean
            If InStr(str, "data:") <> 1 Then
                Return False
            Else
                ' to do
            End If

            Return True
        End Function

        Public Shared Function URIParser(uri As String) As DataURI
            Dim t = uri.Split(";"c) _
                .Select(Function(p) p.StringSplit("[:=,]")) _
                .ToDictionary(Function(k) k(0).ToLower,
                              Function(value)
                                  Return value(1)
                              End Function)

            Return New DataURI(
                base64:=t.TryGetValue("base64"),
                charset:=t.TryGetValue("charset"),
                mime:=t.TryGetValue("data")
            )
        End Function

        ''' <summary>
        ''' Output this data uri string text
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If chartSet.StringEmpty Then
                Return $"data:{mime};base64,{base64}"
            Else
                Return $"data:{mime};charset={chartSet};base64,{base64}"
            End If
        End Function
    End Class
End Namespace

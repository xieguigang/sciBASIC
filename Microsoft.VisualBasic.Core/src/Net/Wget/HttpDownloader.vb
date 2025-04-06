#Region "Microsoft.VisualBasic::f9d8a840dbf30a6bedfddaf363275f36, Microsoft.VisualBasic.Core\src\Net\Wget\HttpDownloader.vb"

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

    '   Total Lines: 77
    '    Code Lines: 60 (77.92%)
    ' Comment Lines: 1 (1.30%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 16 (20.78%)
    '     File Size: 2.80 KB


    '     Class HttpDownloader
    ' 
    '         Properties: LocalSaveFile
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: OpenSaveStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Http

Namespace Net.WebClient

    Public Class HttpDownloader : Inherits WebClient

        ReadOnly _url As String
        ReadOnly _localPath As String = Nothing
        ReadOnly _bufferSize As Integer = 8192
        ReadOnly _buffer As Stream = Nothing

        Public Overrides ReadOnly Property LocalSaveFile As String
            Get
                Return _localPath
            End Get
        End Property

        Public Sub New(url As String, localPath As String)
            _url = url
            _localPath = localPath
        End Sub

        Sub New(url As String, save As Stream)
            _url = url
            _buffer = save
        End Sub

        Public Overrides Async Function DownloadFileAsync() As Task
            Using httpClient As New HttpClient()
                Using response As HttpResponseMessage = Await httpClient.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead)
                    Call response.EnsureSuccessStatusCode()
                    ' implements download file
                    Await RequestStream(response)
                End Using
            End Using

            Call ProgressFinished()
        End Function

        Private Async Function RequestStream(response As HttpResponseMessage) As Task
            Dim totalBytes As Long? = response.Content.Headers.ContentLength

            Using contentStream As Stream = Await response.Content.ReadAsStreamAsync()
                Dim fileStream As Stream = OpenSaveStream()
                Dim totalRead As Long = 0L
                Dim buffer As Byte() = New Byte(_bufferSize - 1) {}
                Dim isMoreToRead As Boolean = True
                Dim bytesRead As Integer

                Call ProgressUpdate(New ProgressChangedEventArgs(0, CLng(totalBytes)))

                While isMoreToRead
                    bytesRead = Await contentStream.ReadAsync(buffer, 0, buffer.Length)

                    If bytesRead <= 0 Then
                        isMoreToRead = False
                    Else
                        Await fileStream.WriteAsync(buffer, 0, bytesRead)
                        totalRead += bytesRead

                        Call ProgressUpdate(New ProgressChangedEventArgs(totalRead, CLng(totalBytes)))
                    End If
                End While

                Await fileStream.FlushAsync
            End Using
        End Function

        Protected Overrides Function OpenSaveStream() As Stream
            If Not _buffer Is Nothing Then
                Return _buffer
            End If
            Return LocalSaveFile.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False)
        End Function
    End Class
End Namespace

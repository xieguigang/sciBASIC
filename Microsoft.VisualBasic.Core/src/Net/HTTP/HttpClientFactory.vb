#Region "Microsoft.VisualBasic::91ea3617eb0369ab1a45cd4ff9184371, Microsoft.VisualBasic.Core\src\Net\HTTP\HttpClientFactory.vb"

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

    '   Total Lines: 166
    '    Code Lines: 99 (59.64%)
    ' Comment Lines: 41 (24.70%)
    '    - Xml Docs: 97.56%
    ' 
    '   Blank Lines: 26 (15.66%)
    '     File Size: 6.51 KB


    '     Module HttpClientFactory
    ' 
    '         Properties: Client, Proxy, UserAgent
    ' 
    '         Function: CreateClient, DefaultUA, GetStreamSync, GetStringSync, PostBytesSync
    '                   PostFormSync, (+2 Overloads) SendSync
    ' 
    '         Sub: Reset, SetProxy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates

Namespace Net.Http

    ''' <summary>
    ''' Shared, configured <see cref="HttpClient"/> infrastructure used to
    ''' replace the obsolete <c>HttpWebRequest</c>/<c>WebRequest</c> API.
    ''' </summary>
    Public Module HttpClientFactory

        ''' <summary>
        ''' Configurable proxy. When <see langword="Nothing"/> the system
        ''' default proxy is used (mirrors the previous behaviour).
        ''' </summary>
        Public Property Proxy As IWebProxy

        ''' <summary>
        ''' Default user-agent header sent with every request.
        ''' </summary>
        Public Property UserAgent As String = DefaultUA()

        Private Function DefaultUA() As String
            Return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) GCModeller/HttpClient"
        End Function

        Private _client As HttpClient
        Private ReadOnly _sync As New Object()

        ''' <summary>
        ''' A shared <see cref="HttpClient"/> instance configured to behave
        ''' like the previous <c>HttpWebRequest</c> based pipeline.
        ''' </summary>
        Public ReadOnly Property Client As HttpClient
            Get
                If _client Is Nothing Then
                    SyncLock _sync
                        If _client Is Nothing Then
                            _client = CreateClient()
                        End If
                    End SyncLock
                End If

                Return _client
            End Get
        End Property

        Private Function CreateClient() As HttpClient
            Dim handler As New HttpClientHandler()

            ' Equivalent to ServicePointManager.ServerCertificateValidationCallback = always True
            handler.ServerCertificateCustomValidationCallback =
                Function(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors)
                    Return True
                End Function

            handler.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            handler.AllowAutoRedirect = True

            If Not Proxy Is Nothing Then
                handler.Proxy = Proxy
                handler.UseDefaultCredentials = False
            End If

            Dim client As New HttpClient(handler)
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent)
            client.Timeout = TimeSpan.FromMinutes(30)
            Return client
        End Function

        ''' <summary>
        ''' Reset the cached <see cref="HttpClient"/> (e.g. after changing
        ''' <see cref="Proxy"/> or <see cref="UserAgent"/>).
        ''' </summary>
        Public Sub Reset()
            SyncLock _sync
                If Not _client Is Nothing Then
                    Call _client.Dispose()
                    _client = Nothing
                End If
            End SyncLock
        End Sub

        ''' <summary>
        ''' Configure the proxy used by the shared <see cref="HttpClient"/>.
        ''' An empty/null value clears the proxy (system default is used).
        ''' </summary>
        Public Sub SetProxy(proxyUrl As String)
            If String.IsNullOrEmpty(proxyUrl) Then
                Proxy = Nothing
            Else
                Proxy = New WebProxy(proxyUrl)
            End If

            Call Reset()
        End Sub

        ''' <summary>
        ''' Perform a synchronous GET and return the response body as text.
        ''' </summary>
        Public Function GetStringSync(url As String) As String
            Return Client.GetStringAsync(url).GetAwaiter().GetResult()
        End Function

        ''' <summary>
        ''' Perform a synchronous GET and return the response stream.
        ''' The caller is responsible for disposing the returned stream
        ''' (it is a copy of the network stream that can be read fully).
        ''' </summary>
        Public Function GetStreamSync(url As String) As Stream
            Dim bytes As Byte() = Client.GetByteArrayAsync(url).GetAwaiter().GetResult()
            Return New MemoryStream(bytes)
        End Function

        ''' <summary>
        ''' Send an arbitrary <see cref="HttpRequestMessage"/> synchronously.
        ''' </summary>
        Public Function SendSync(request As HttpRequestMessage) As HttpResponseMessage
            Return Client.SendAsync(request).GetAwaiter().GetResult()
        End Function

        Public Function SendSync(request As HttpRequestMessage, timeout As TimeSpan) As HttpResponseMessage
            If timeout <= TimeSpan.Zero Then
                Return Client.SendAsync(request).GetAwaiter().GetResult()
            End If

            Using cts As New Threading.CancellationTokenSource(timeout)
                Return Client.SendAsync(request, cts.Token).GetAwaiter().GetResult()
            End Using
        End Function

        ''' <summary>
        ''' Synchronous POST of raw bytes with an explicit content-type.
        ''' </summary>
        Public Function PostBytesSync(url As String, data As Byte(), contentType As String) As HttpResponseMessage
            Using content As New ByteArrayContent(data)
                If Not contentType Is Nothing Then
                    content.Headers.ContentType = New Headers.MediaTypeHeaderValue(contentType)
                End If

                Return Client.PostAsync(url, content).GetAwaiter().GetResult()
            End Using
        End Function

        ''' <summary>
        ''' Synchronous POST of form-urlencoded name/value pairs.
        ''' </summary>
        Public Function PostFormSync(url As String, data As NameValueCollection) As HttpResponseMessage
            Dim pairs As New List(Of KeyValuePair(Of String, String))

            For Each key As String In data.AllKeys
                If Not key Is Nothing Then
                    Call pairs.Add(New KeyValuePair(Of String, String)(key, data(key)))
                End If
            Next

            Using content As New FormUrlEncodedContent(pairs)
                Return Client.PostAsync(url, content).GetAwaiter().GetResult()
            End Using
        End Function
    End Module
End Namespace

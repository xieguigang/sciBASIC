#Region "Microsoft.VisualBasic::1c50331d812e3a259c8474d97699cfc9, sciBASIC#\mime\text%markdown\LinkToTitle.vb"

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

    '   Total Lines: 141
    '    Code Lines: 82
    ' Comment Lines: 37
    '   Blank Lines: 22
    '     File Size: 5.03 KB


    ' Class LinkToTitle
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: GetExtension, LinkEvaluator, RequestToGoogleApi, Transform, videoIdAlreadyExist
    ' 
    '     Sub: ParseApiResponse
    ' 
    ' /********************************************************************************/

#End Region

'*
' * This file is part of the MarkdownSharp package
' * For the full copyright and license information,
' * view the LICENSE file that was distributed with this source code.
' 


Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Add title to youtube link
''' </summary>
Public Class LinkToTitle

    ''' <summary>
    ''' Array of links: videoID/title
    ''' </summary>
    Private _links As String(,)

    ''' <summary>
    ''' Google api key
    ''' </summary>
    Private _apiKey As String
    Private _maxLinks As Integer

    Private Shared _youtubeLink As New Regex(vbCr & vbLf & "                    (?:https?\:\/\/)" & vbCr & vbLf & "                    (?:www\.)?" & vbCr & vbLf & "                    (?:youtu\.be|youtube\.com)\/" & vbCr & vbLf & "                    (?:embed\/|v\/|watch\?v=)?" & vbCr & vbLf & "                    ([\w\-]{10,12})", RegexOptions.Multiline Or RegexOptions.IgnorePatternWhitespace)

    ''' <summary>
    ''' FiXME: max ids?
    ''' </summary>
    ''' <param name="apiKey"></param>
    ''' <param name="maxLinks"></param>
    Public Sub New(apiKey As String, Optional maxLinks As Integer = 10)
        If maxLinks < 1 Then
            Throw New ArgumentException("Max links should be equal or greater than 1.")
        End If
        _apiKey = apiKey
        _maxLinks = maxLinks
        _links = New String(_maxLinks - 1, 1) {}
    End Sub

    ''' <summary>
    ''' FiXME: max ids?
    ''' </summary>
    ''' <param name="apiKey"></param>
    ''' <param name="maxLinks"></param>
    Public Shared Function GetExtension(apiKey As String, Optional maxLinks As Integer = 10) As ExtensionTransform
        Return AddressOf New LinkToTitle(apiKey, maxLinks).Transform
    End Function

    Public Function Transform(text As String) As String
        Dim linksCount As Integer = 0
        For Each match As Match In _youtubeLink.Matches(text)
            If linksCount = _maxLinks Then
                Exit For
            End If
            If videoIdAlreadyExist(match.Groups(1).Value) Then
                Continue For
            End If

            ' Set video id
            _links(linksCount, 0) = match.Groups(1).Value
            linksCount += 1
        Next

        If linksCount > 0 Then
            Dim ids As String = ""
            For i As Integer = 0 To linksCount - 1
                ids += If((i = linksCount - 1), _links(i, 0), _links(i, 0) & ",")
            Next

            ' Load titles
            Dim res As String = RequestToGoogleApi(ids)
            If Not String.IsNullOrEmpty(res) Then
                ParseApiResponse(res)
            End If
        End If

        Return _youtubeLink.Replace(text, New MatchEvaluator(AddressOf LinkEvaluator))
    End Function


    Private Function videoIdAlreadyExist(id As String) As Boolean
        For i As Integer = 0 To _maxLinks - 1
            If _links(i, 0) = id Then
                Return True
            End If
        Next
        Return False
    End Function


    ''' <summary>
    ''' Get videos titles from youtube API
    ''' More info: https://developers.google.com/youtube/v3/
    ''' </summary>
    ''' <param name="ids"></param>
    ''' <returns>Return null string if request failed</returns>
    Private Function RequestToGoogleApi(ids As String) As String
        Try
            Using client As New WebClient()
                client.Encoding = Encoding.UTF8
                Return client.DownloadString(String.Format("https://www.googleapis.com/youtube/v3/videos?id={0}&key={1}" & "&part=snippet&fields=items(id,snippet(title))", ids, _apiKey))
            End Using
        Catch
        End Try
        Return Nothing
    End Function


    ''' <summary>
    ''' Parse API JSON response
    ''' </summary>
    ''' <param name="res"></param>
    Private Sub ParseApiResponse(res As String)
        Dim json = res.LoadJSON(Of Dictionary(Of String, Object))

        For Each item As Dictionary(Of String, Object) In DirectCast(json("items"), ArrayList)
            For i As Integer = 0 To _maxLinks - 1
                If _links(i, 0) = DirectCast(item("id"), String) Then
                    Dim snippet = DirectCast(item("snippet"), Dictionary(Of String, Object))
                    ' Set video title
                    _links(i, 1) = DirectCast(snippet("title"), String)
                End If
            Next
        Next
    End Sub


    Private Function LinkEvaluator(match As Match) As String
        For x As Integer = 0 To _maxLinks - 1
            If match.Groups(1).Value = _links(x, 0) AndAlso _links(x, 1) IsNot Nothing Then
                Return String.Format("[YouTube: {0}](https://youtube.com/watch?v={1})", _links(x, 1), _links(x, 0))
            End If
        Next
        Return match.Groups(0).Value
    End Function
End Class

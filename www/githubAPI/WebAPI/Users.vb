#Region "Microsoft.VisualBasic::3f48d0527a05310eeb0a6b9d0c840843, ..\sciBASIC#\www\githubAPI\WebAPI\Users.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports r = System.Text.RegularExpressions.Regex

Namespace WebAPI

    Public Module Users

        Public Const Github$ = "https://github.com"

        ''' <summary>
        ''' Get user's github followers
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        <Extension> Public Function Followers(username As String, Optional maxFollowers% = Integer.MaxValue) As User()
            Dim url As String = Github & "/{0}?page={1}&tab=followers"
            Return ParserIterator(url, username, maxFollowers)
        End Function

        Private Function ParserIterator(url$, username$, maxLimits%) As User()
            Dim out As New List(Of User)
            Dim i As int = 1
            Dim [get] As New Value(Of User())

            Do While Not ([get] = ParserInternal(username, ++i, url)).IsNullOrEmpty
                out += (+[get])

                If out.Count > maxLimits Then
                    Exit Do
                Else
                    Call Thread.Sleep(300)  ' Decrease the server load 
                End If
            Loop

            Return out
        End Function

        Public Function GetUserData(usrName$) As User
            Dim url$ = "https://github.com/" & usrName
            Dim html$ = url.GET
            Dim avatar$ = r.Match(html, "<img [^<]+ class=""avatar width-full rounded-2"" .*? />", RegexICSng).Value
            avatar = avatar.img.src

            Dim vcardNames = r.Match(html, "<h1 class=""vcard-names"">.+?</h1>", RegexICSng).Value
            Dim names = Regex.Matches(vcardNames, "<span .+?>.+?</span>", RegexICSng).ToArray(Function(s) s.GetValue)
            Dim bio$ = r.Match(html, "<div class[=]""(p-note )?user-profile-bio"">.+?</div>", RegexICSng) _
                .Value _
                .GetValue _
                .Substring(5) _
                .TrimNewLine

            Return New User With {
                .login = usrName,
                .avatar_url = avatar,
                .name = names(Scan0),
                .bio = bio,
                .url = url
            }
        End Function

        ReadOnly UserSplitter$ = (<div class="d-table col-12 width-full py-4 border-bottom border-gray-light"/>).ToString
        ReadOnly Splitter$ = (<div class="js-repo-filter position-relative"/>).ToString

        Const locationPattern$ = "<svg .+? class=""octicon octicon-location"".+?</p>"
        Const organizationPattern$ = "<svg .+? class=""octicon octicon-organization"".+?</span>"

        Private Function ParserInternal(user$, page%, url$) As User()
            Dim html$
            Dim sp$

            url = String.Format(url, user, page)
            sp = "<div class=""position-relative"">"
            html = url.GET
            html = Strings.Split(html, sp).Last
            sp = UserSplitter.Replace(" /", "")

            Dim users$() = Strings.Split(html, sp).Skip(1).ToArray
            Dim out As New List(Of User)

            For Each u$ In users
                Dim userName As String = Regex _
                    .Match(u, "<a href=""/.+?"">") _
                    .Value _
                    .href _
                    .Replace("/", "")
                Dim avatar As String = Regex.Match(u, "<img .+? />").Value.img.src
                Dim display As String = Regex.Match(u, "<span class=""f4 link-gray-dark"">.*?</span>").Value.GetValue
                Dim bio As String = Regex.Match(u, "<p class="".*?text-gray text-small"">.*?</p>").Value.GetValue
                Dim location = TryInvoke(Function() u.Match(locationPattern, RegexICSng).GetBetween("</svg>", "</p>").lTokens.FirstOrDefault?.Trim)
                Dim org = TryInvoke(Function() u.Match(organizationPattern, RegexICSng).GetBetween("</svg>", "</span>").lTokens.FirstOrDefault?.Trim)

                out += New User With {
                    .login = userName,
                    .avatar_url = avatar,
                    .name = display,
                    .bio = bio,
                    .location = location,
                    .organizations_url = org
                }
            Next

            Return out
        End Function

        ''' <summary>
        ''' Get user's github following
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        <Extension> Public Function Following(username As String, Optional maxFollowing% = Integer.MaxValue) As User()
            Dim url As String = Github & "/{0}?page={1}&tab=following"
            Return ParserIterator(url, username, maxFollowing)
        End Function
    End Module
End Namespace

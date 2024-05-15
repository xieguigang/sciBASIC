#Region "Microsoft.VisualBasic::ca26e4f988f3279f585699c2d881cb82, www\githubAPI\WebAPI\Users.vb"

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

    '   Total Lines: 171
    '    Code Lines: 132
    ' Comment Lines: 14
    '   Blank Lines: 25
    '     File Size: 7.52 KB


    '     Class Counter
    ' 
    '         Properties: Followers, Following, Repositories, Stars
    ' 
    '         Function: Parse, ToString
    ' 
    '     Module Users
    ' 
    '         Function: Followers, Following, GetUserData, ParserInternal, ParserIterator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports r = System.Text.RegularExpressions.Regex

Namespace WebAPI

    Public Class Counter

        Public Property Repositories As Integer
        Public Property Stars As Integer
        Public Property Followers As Integer
        Public Property Following As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function Parse(page As String) As Counter
            Dim links$() = page.Matches("UnderlineNav.+?</a>", RegexICSng) _
                               .Select(Function(c)
                                           Return c.Match(">.+?</span>", RegexICSng).TrimNewLine.StringReplace("\s+", "")
                                       End Function) _
                               .Where(Function(s) Not s.StringEmpty) _
                               .ToArray
            Dim counters = links _
                .Select(Function(a)
                            Dim title$ = a.GetValue.Split("<"c).FirstOrDefault
                            Dim count$ = a.Match("<span.+</span>", RegexICSng).Match("\d+")
                            Return New NamedValue(Of Integer)(title, CInt(Val(count)))
                        End Function) _
                .Where(Function(n) Not n.Name.StringEmpty) _
                .ToDictionary _
                .FlatTable

            Return New Counter With {
                .Followers = counters(NameOf(.Followers)),
                .Following = counters(NameOf(.Following)),
                .Repositories = counters(NameOf(.Repositories)),
                .Stars = counters(NameOf(.Stars))
            }
        End Function
    End Class

    Public Module Users

        Public Const Github$ = "https://github.com"

        ''' <summary>
        ''' Get user's github followers
        ''' </summary>
        ''' <param name="username"></param>
        ''' <param name="maxFollowers">
        ''' 限制性参数，如果超过了这个数量，将会停止解析
        ''' </param>
        ''' <returns></returns>
        <Extension> Public Function Followers(username$, Optional maxFollowers% = Integer.MaxValue) As User()
            Dim url As String = Github & "/{0}?page={1}&tab=followers"
            Dim counter As Counter = Counter.Parse($"https://github.com/{username}".GET)
            Return ParserIterator(url, username, maxFollowers, counter.Followers)
        End Function

        Private Function ParserIterator(url$, username$, maxLimits%, count%) As User()
            Dim out As New List(Of User)
            Dim i As i32 = 1
            Dim [get] As New Value(Of User())

            Do While Not ([get] = ParserInternal(username, ++i, url)).IsNullOrEmpty
                out += (+[get])

                If out.Count > maxLimits OrElse out.Count >= count Then
                    Exit Do
                Else
                    ' Decrease the server load 
                    Call Thread.Sleep(300)
                End If
            Loop

            Return out.GroupBy(Function(u) u.login) _
                      .Select(Function(u) u.First) _
                      .ToArray
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

        ReadOnly UserSplitter$ = (<div class="d-table .+?"/>).ToString.Replace("=", "[=]")
        ReadOnly Splitter$ = (<div class="js-repo-filter position-relative"/>).ToString

        Const organizationPattern$ = "<svg.+?class=""octicon octicon-organization"".+?</span>"
        Const locationPattern$ = "<svg.+?class=""octicon octicon-location"".+?</p>"

        Private Function ParserInternal(user$, page%, url$) As User()
            Dim html$
            Dim sp$

            url = String.Format(url, user, page)
            sp = "<div class=""position-relative"">"
            html = url.GET
            html = Strings.Split(html, sp).Last
            sp = UserSplitter.Replace("""/", "")

            Dim users$() = r.Split(html, sp, RegexICSng).Skip(1).ToArray
            Dim out As New List(Of User)

            For Each u$ In users
                Dim userName As String = Regex _
                    .Match(u, "<a .+?href=""/.+?"">") _
                    .Value _
                    .href _
                    .Replace("/", "")
                Dim userID = r.Match(u, "user[-]id[=]""\d+""").Value.GetStackValue("""", """")
                Dim avatar As String = $"https://avatars0.githubusercontent.com/u/{userID}?s=100&v=4"
                Dim display As String = Regex.Match(u, "<span class=""f4 link-gray-dark"">.*?</span>").Value.GetValue
                Dim bio As String = Regex.Match(u, "<div class="".*?text[-]gray text[-]small.+?"">.*?</div>").Value.GetValue
                Dim location = TryInvoke(Function() u.Match(locationPattern, RegexICSng).GetBetween("</svg>", "</p>").LineTokens.FirstOrDefault?.Trim)
                Dim org = TryInvoke(Function() u.Match(organizationPattern, RegexICSng).GetBetween("</svg>", "</span>").LineTokens.FirstOrDefault?.Trim)

                out += New User With {
                    .login = userName,
                    .avatar_url = avatar,
                    .name = display,
                    .bio = bio,
                    .location = location,
                    .organizations_url = org,
                    .updated_at = Now.ToLocalTime,
                    .id = userID
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
            Dim counter As Counter = Counter.Parse($"https://github.com/{username}".GET)
            Return ParserIterator(url, username, maxFollowing, counter.Following)
        End Function
    End Module
End Namespace

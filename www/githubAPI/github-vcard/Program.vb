#Region "Microsoft.VisualBasic::cd235725c046c86b1ca2b14e48df350d, ..\sciBASIC#\www\githubAPI\github-vcard\Program.vb"

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

Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Webservices.Github.Visualizer
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI

Module Program

    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    <ExportAPI("/write",
               Info:="Draw user github vcard.",
               Usage:="/write /user <userName, example: xieguigang> [/schema$ <default=YlGnBu:c8> /out <vcard.png>]")>
    <Argument("/user", Description:="The user github account name.")>
    <Argument("/schema", Description:="The color schema name of the user contributions 3D plot data.")>
    <Argument("/out", Description:="The png image output path.")>
    Public Function vcard(args As CommandLine) As Integer
        Dim user$ = args <= "/user"
        Dim schema$ = args.GetValue("/schema", "YlGnBu:c8")
        Dim out$ = args.GetValue("/out", $"./{user}_github-vcard.png")

        Return IsometricContributions.Plot(
            user.GetUserContributions,
            schema:=schema,
            user:=Users.GetUserData(user)) _
            .Save(out) _
            .CLICode
    End Function

    <ExportAPI("/relationships",
               Usage:="/relationships /user <userName> [/out <out.DIR>]")>
    Public Function relationships(args As CommandLine) As Integer
        Dim user$ = args <= "/user"
        Dim out$ = args.GetValue("out", App.CurrentDirectory & "/github-relationships/")

        Dim followers As User() = WebAPI.Users.Followers(user)
        Call followers _
            .GetJson(True) _
            .SaveTo(out & "/followers.json")

        Dim following As User() = WebAPI.Users.Following(user)
        Call following _
            .GetJson(True) _
            .SaveTo(out & "/following.json")

        Dim notFollings = following.WhoIsNotFollowMe(followers).ToArray
        Call notFollings _
            .GetJson(True) _
            .SaveTo(out & "/notfollowing.json")

        Dim IamNotFollowings = following.WhoIamNotFollow(followers).ToArray
        Call IamNotFollowings _
            .GetJson(True) _
            .SaveTo(out & "/I-am-not-Following.json")

        Return 0
    End Function
End Module


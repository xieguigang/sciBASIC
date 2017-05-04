#Region "Microsoft.VisualBasic::4899f1daaca5d29acd6ff4224872ecdb, ..\sciBASIC#\www\githubAPI\Test\Module1.vb"

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

Imports Microsoft.VisualBasic.Webservices.Github.API
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github

Module Module1

    Sub Main()

        ' WebAPI.Proxy = "http://127.0.0.1:8087"


        Dim followers As User() = WebAPI.Users.Followers("xieguigang") '= "J:\GCModeller\src\runtime\sciBASIC#\www\data\github\followers.json".ReadAllText.LoadObject(Of User()) '"xieguigang".Followers
        Call followers.GetJson(True).SaveTo("./followers.json")

        Dim following As User() = WebAPI.Users.Following("xieguigang")  '"xieguigang".Following
        Call following.GetJson(True).SaveTo("./following.json")

        Dim notFollings = following.WhoIsNotFollowMe(followers).ToArray
        Call notFollings.GetJson(True).SaveTo("./notfollowing.json")

        Dim IamNotFollowings = following.WhoIamNotFollow(followers).ToArray
        Call IamNotFollowings.GetJson(True).SaveTo("./I-am-not-Following.json")

        Pause()
    End Sub
End Module

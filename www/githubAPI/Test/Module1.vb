#Region "Microsoft.VisualBasic::8c82fa439c2a50b3e1adce094578398f, www\githubAPI\Test\Module1.vb"

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

    '   Total Lines: 44
    '    Code Lines: 19
    ' Comment Lines: 9
    '   Blank Lines: 16
    '     File Size: 1.74 KB


    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Webservices.Github.Visualizer
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI

Module Module1

    Sub Main()

        'Dim my As User = WebAPI.Users.GetUserData("xieguigang")

        WebServiceUtils.Proxy = "http://127.0.0.1:8087"

        ''   Call "xieguigang".GetUserContributions .GetJson .SaveTo ("x:\xieguigang_contributions.json")

        'Call IsometricContributions.Plot(
        '    "xieguigang".GetUserContributions,
        '    schema:="Spectral:c8", user:=my).Save("G:\GCModeller\src\runtime\sciBASIC#\www\data\github\xieguigang_contributions.png")

        'Pause()

        'Call IsometricContributions.Plot("xieguigang").Save("x:\text.png")

        'Pause()

        'Dim contributions = "xieguigang".GetUserContributions


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

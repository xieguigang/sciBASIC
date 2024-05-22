#Region "Microsoft.VisualBasic::b24225380c97842b4ff709bd8f82609c, www\githubAPI\API\Users.vb"

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

    '   Total Lines: 35
    '    Code Lines: 25 (71.43%)
    ' Comment Lines: 5 (14.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.29%)
    '     File Size: 1.32 KB


    '     Module Users
    ' 
    '         Function: Followers, Following, GetUser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace API

    Public Module Users

        ''' <summary>
        ''' Get a single user
        ''' </summary>
        ''' <param name="username"></param>
        ''' <returns></returns>
        Public Function GetUser(username As String) As User
            Dim url As String = GithubAPI & $"/users/{username}"
            Dim json As String = url.GetRequest(https:=True)
            Dim user As User = json.LoadJSON(Of User)
            Return user
        End Function

        <Extension> Public Function Followers(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/followers"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function

        <Extension> Public Function Following(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/following"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function
    End Module
End Namespace

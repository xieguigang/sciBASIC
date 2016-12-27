#Region "Microsoft.VisualBasic::3ebf9df571feac566baaea84de023af6, ..\sciBASIC#\www\githubAPI\API\Organizations.vb"

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

Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace API

    Public Class Organizations

        Public Function UserOrgs(username As String) As Organization()
            Dim url As String = GithubAPI & $"/users/{username}/orgs"
            Dim json As String = url.GetRequest(https:=True)
            Dim orgs As Organization() = json.LoadObject(Of Organization())
            Return orgs
        End Function

        Public Function OrgMembers(org As String) As User()
            Dim url As String = GithubAPI & $"/orgs/{org}/members"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadObject(Of User())
            Return users
        End Function
    End Class
End Namespace

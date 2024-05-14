#Region "Microsoft.VisualBasic::8369886bcfef0891b65464cda7985cb9, www\githubAPI\API\Organizations.vb"

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

    '   Total Lines: 22
    '    Code Lines: 18
    ' Comment Lines: 0
    '   Blank Lines: 4
    '     File Size: 815 B


    '     Class Organizations
    ' 
    '         Function: OrgMembers, UserOrgs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace API

    Public Class Organizations

        Public Function UserOrgs(username As String) As Organization()
            Dim url As String = GithubAPI & $"/users/{username}/orgs"
            Dim json As String = url.GetRequest(https:=True)
            Dim orgs As Organization() = json.LoadJSON(Of Organization())
            Return orgs
        End Function

        Public Function OrgMembers(org As String) As User()
            Dim url As String = GithubAPI & $"/orgs/{org}/members"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function
    End Class
End Namespace

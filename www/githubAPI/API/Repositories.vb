#Region "Microsoft.VisualBasic::12e56d4e675e834e1032eb79570efb4d, ..\visualbasic_App\www\githubAPI\API\Repositories.vb"

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

    Public Class Repositories

        Public Function UserRepositories(username As String) As Repository()
            Dim url As String = GithubAPI & $"/users/{username}/repos"
            Dim json As String = url.GetRequest(https:=True)
            Dim repos As Repository() = json.LoadObject(Of Repository())
            Return repos
        End Function
    End Class
End Namespace

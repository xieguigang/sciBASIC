#Region "Microsoft.VisualBasic::b0210c02ab2cb097f3b13323cabc124b, www\githubAPI\API\Repositories.vb"

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

    '   Total Lines: 14
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 462 B


    '     Class Repositories
    ' 
    '         Function: UserRepositories
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace API

    Public Class Repositories

        Public Function UserRepositories(username As String) As repository()
            Dim url As String = GithubAPI & $"/users/{username}/repos"
            Dim json As String = url.GetRequest(https:=True)
            Dim repos As repository() = json.LoadJSON(Of repository())
            Return repos
        End Function
    End Class
End Namespace

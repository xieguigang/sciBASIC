#Region "Microsoft.VisualBasic::ed9ba498bb06a26ce6a84e56f433e7b7, www\githubAPI\Class\Organization.vb"

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

    '   Total Lines: 16
    '    Code Lines: 15
    ' Comment Lines: 0
    '   Blank Lines: 1
    '     File Size: 574 B


    '     Class Organization
    ' 
    '         Properties: avatar_url, description, events_url, hooks_url, id
    '                     issues_url, login, members_url, public_members_url, repos_url
    '                     url
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace [Class]

    Public Class Organization
        Public Property login As String
        Public Property id As String
        Public Property url As String
        Public Property repos_url As String
        Public Property events_url As String
        Public Property hooks_url As String
        Public Property issues_url As String
        Public Property members_url As String
        Public Property public_members_url As String
        Public Property avatar_url As String
        Public Property description As String
    End Class
End Namespace

#Region "Microsoft.VisualBasic::6a54032767601bb55ba241a3cc9cbe7d, ..\sciBASIC#\www\githubAPI\Extensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Webservices.Github.API
Imports Microsoft.VisualBasic.Webservices.Github.Class

Public Module Extensions

    <Extension> Public Function WhoIsNotFollowMe(username$) As IEnumerable(Of User)
        Dim myFollowing As User() = username.Following
        Dim myFollowers As User() = username.Followers
        Return myFollowing.WhoIsNotFollowMe(myFollowers)
    End Function

    <Extension> Public Iterator Function WhoIsNotFollowMe(following As User(), followers As User()) As IEnumerable(Of User)
        Dim followersId$() = followers _
            .Select(Function(u) DirectCast(u, INamedValue).Key) _
            .ToArray

        For Each u As User In following
            Dim uid$ = DirectCast(u, INamedValue).Key

            If Array.IndexOf(followersId, uid) = -1 Then
                Yield u
            End If
        Next
    End Function
End Module


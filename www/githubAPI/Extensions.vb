#Region "Microsoft.VisualBasic::dd0ec0417469eee2159136d1538d1f0e, www\githubAPI\Extensions.vb"

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

    '   Total Lines: 56
    '    Code Lines: 47 (83.93%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (16.07%)
    '     File Size: 2.04 KB


    ' Module Extensions
    ' 
    '     Function: __subExcludes, (+2 Overloads) WhoIamNotFollow, (+2 Overloads) WhoIsNotFollowMe
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Webservices.Github.API
Imports Microsoft.VisualBasic.Webservices.Github.Class

<HideModuleName>
Public Module Extensions

    <Extension> Public Function WhoIsNotFollowMe(username$) As IEnumerable(Of User)
        Dim myFollowing As User() = username.Following
        Dim myFollowers As User() = username.Followers
        Return myFollowing.WhoIsNotFollowMe(myFollowers)
    End Function

    <Extension> Public Function WhoIsNotFollowMe(following As User(), followers As User()) As IEnumerable(Of User)
        Dim followersId$() = followers _
            .Select(Function(u) DirectCast(u, INamedValue).Key) _
            .ToArray
        Dim out = followersId.__subExcludes(following)
        Return out
    End Function

    <Extension>
    Private Function __subExcludes(list$(), users As User()) As IEnumerable(Of User)
        Dim indexOf As New Index(Of String)(list)
        Dim out As New List(Of User)

        For Each u As User In users
            Dim uid$ = DirectCast(u, INamedValue).Key

            If indexOf(uid) = -1 Then
                out += u
            End If
        Next

        Return out
    End Function

    <Extension>
    Public Function WhoIamNotFollow(userName$) As IEnumerable(Of User)
        Dim myFollowing As User() = userName.Following
        Dim myFollowers As User() = userName.Followers
        Return myFollowing.WhoIamNotFollow(myFollowers)
    End Function

    <Extension>
    Public Function WhoIamNotFollow(following As User(), followers As User()) As IEnumerable(Of User)
        Dim followingId$() = following _
           .Select(Function(u) DirectCast(u, INamedValue).Key) _
           .ToArray
        Dim out = followingId.__subExcludes(followers)
        Return out
    End Function
End Module

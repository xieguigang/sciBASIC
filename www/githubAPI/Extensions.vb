#Region "Microsoft.VisualBasic::b71c3f95f961b06fe3e1cf19138437d0, www\githubAPI\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

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

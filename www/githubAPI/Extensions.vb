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
            .Select(Function(u) DirectCast(u, sIdEnumerable).Identifier) _
            .ToArray

        For Each u As User In following
            Dim uid$ = DirectCast(u, sIdEnumerable).Identifier

            If Array.IndexOf(followersId, uid) = -1 Then
                Yield u
            End If
        Next
    End Function
End Module

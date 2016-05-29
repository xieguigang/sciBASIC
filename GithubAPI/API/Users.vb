Imports Microsoft.VisualBasic.Serialization
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
            Dim user As User = json.LoadObject(Of User)
            Return user
        End Function

        Public Function Followers(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/followers"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadObject(Of User())
            Return users
        End Function

        Public Function Following(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/following"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadObject(Of User())
            Return users
        End Function
    End Module
End Namespace
#Region "Microsoft.VisualBasic::b24225380c97842b4ff709bd8f82609c, www\githubAPI\API\Users.vb"

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

    '     Module Users
    ' 
    '         Function: Followers, Following, GetUser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
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
            Dim user As User = json.LoadJSON(Of User)
            Return user
        End Function

        <Extension> Public Function Followers(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/followers"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function

        <Extension> Public Function Following(username As String) As User()
            Dim url As String = GithubAPI & $"/users/{username}/following"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function
    End Module
End Namespace

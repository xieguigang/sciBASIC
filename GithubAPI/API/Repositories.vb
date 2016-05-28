Imports Microsoft.VisualBasic.Serialization
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
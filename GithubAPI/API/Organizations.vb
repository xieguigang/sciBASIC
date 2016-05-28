Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Webservices.Github.Class

Public Class Organizations

    Public Function UserOrgs(username As String) As Organization()
        Dim url As String = GithubAPI & $"/users/{username}/orgs"
        Dim json As String = url.GetRequest(https:=True)
        Dim orgs As Organization() = json.LoadObject(Of Organization())
        Return orgs
    End Function
End Class

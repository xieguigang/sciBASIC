#Region "Microsoft.VisualBasic::8369886bcfef0891b65464cda7985cb9, www\githubAPI\API\Organizations.vb"

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

    '     Class Organizations
    ' 
    '         Function: OrgMembers, UserOrgs
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace API

    Public Class Organizations

        Public Function UserOrgs(username As String) As Organization()
            Dim url As String = GithubAPI & $"/users/{username}/orgs"
            Dim json As String = url.GetRequest(https:=True)
            Dim orgs As Organization() = json.LoadJSON(Of Organization())
            Return orgs
        End Function

        Public Function OrgMembers(org As String) As User()
            Dim url As String = GithubAPI & $"/orgs/{org}/members"
            Dim json As String = url.GetRequest(https:=True)
            Dim users As User() = json.LoadJSON(Of User())
            Return users
        End Function
    End Class
End Namespace

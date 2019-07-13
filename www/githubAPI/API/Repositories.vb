#Region "Microsoft.VisualBasic::b0210c02ab2cb097f3b13323cabc124b, www\githubAPI\API\Repositories.vb"

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

    '     Class Repositories
    ' 
    '         Function: UserRepositories
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace API

    Public Class Repositories

        Public Function UserRepositories(username As String) As repository()
            Dim url As String = GithubAPI & $"/users/{username}/repos"
            Dim json As String = url.GetRequest(https:=True)
            Dim repos As repository() = json.LoadJSON(Of repository())
            Return repos
        End Function
    End Class
End Namespace

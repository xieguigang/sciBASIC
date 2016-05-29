Imports Microsoft.VisualBasic.Language

Namespace [Class]

    Public Class SearchResult(Of T As Class) : Inherits ClassObject
        Public Property total_count As Integer
        Public Property incomplete_results As Boolean
        Public Property items As T()
    End Class

    Public Class User : Inherits ClassObject
        Public Property score As Double
        Public Property login As String
        Public Property id As String
        Public Property avatar_url As String
        Public Property gravatar_id As String
        Public Property url As String
        Public Property html_url As String
        Public Property followers_url As String
        Public Property following_url As String
        Public Property gists_url As String
        Public Property starred_url As String
        Public Property subscriptions_url As String
        Public Property organizations_url As String
        Public Property repos_url As String
        Public Property events_url As String
        Public Property received_events_url As String
        Public Property type As String
        Public Property site_admin As String
        Public Property name As String
        Public Property company As String
        Public Property blog As String
        Public Property location As String
        Public Property email As String
        Public Property hireable As String
        Public Property bio As String
        Public Property public_repos As String
        Public Property public_gists As String
        Public Property followers As String
        Public Property following As String
        Public Property created_at As String
        Public Property updated_at As String
    End Class
End Namespace
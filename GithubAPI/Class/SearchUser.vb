Imports Microsoft.VisualBasic.Language

Namespace [Class]

    Public Class SearchResult(Of T As Class) : Inherits ClassObject
        Public Property total_count As Integer
        Public Property incomplete_results As Boolean
        Public Property items As T()
    End Class

    Public Class SearchUser : Inherits ClassObject
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
        Public Property site_admin As Boolean
        Public Property score As Double
    End Class
End Namespace
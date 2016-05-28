Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace [Class]

    Public Class Repository

        Public Property id As String
        Public Property owner As User
        Public Property name As String
        Public Property full_name As String
        Public Property description As String
        Public Property [private] As String
        Public Property fork As String
        Public Property url As String
        Public Property html_url As String
        Public Property archive_url As String
        Public Property assignees_url As String
        Public Property blobs_url As String
        Public Property branches_url As String
        Public Property clone_url As String
        Public Property collaborators_url As String
        Public Property comments_url As String
        Public Property commits_url As String
        Public Property compare_url As String
        Public Property contents_url As String
        Public Property contributors_url As String
        Public Property deployments_url As String
        Public Property downloads_url As String
        Public Property events_url As String
        Public Property forks_url As String
        Public Property git_commits_url As String
        Public Property git_refs_url As String
        Public Property git_tags_url As String
        Public Property git_url As String
        Public Property hooks_url As String
        Public Property issue_comment_url As String
        Public Property issue_events_url As String
        Public Property issues_url As String
        Public Property keys_url As String
        Public Property labels_url As String
        Public Property languages_url As String
        Public Property merges_url As String
        Public Property milestones_url As String
        Public Property mirror_url As String
        Public Property notifications_url As String
        Public Property pulls_url As String
        Public Property releases_url As String
        Public Property ssh_url As String
        Public Property stargazers_url As String
        Public Property statuses_url As String
        Public Property subscribers_url As String
        Public Property subscription_url As String
        Public Property svn_url As String
        Public Property tags_url As String
        Public Property teams_url As String
        Public Property trees_url As String
        Public Property homepage As String
        Public Property language As String
        Public Property forks_count As String
        Public Property stargazers_count As String
        Public Property watchers_count As String
        Public Property size As String
        Public Property default_branch As String
        Public Property open_issues_count As String
        Public Property has_issues As String
        Public Property has_wiki As String
        Public Property has_pages As String
        Public Property has_downloads As String
        Public Property pushed_at As String
        Public Property created_at As String
        Public Property updated_at As String
        Public Property permissions As permissions
    End Class

    Public Class permissions
        Public Property admin As String
        Public Property push As String
        Public Property pull As String
    End Class
End Namespace
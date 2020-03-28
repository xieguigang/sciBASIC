#Region "Microsoft.VisualBasic::35dcc72116e7db220850b5bf81385f3b, www\githubAPI\Class\SearchUser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class SearchResult
    ' 
    '         Properties: incomplete_results, items, total_count
    ' 
    '     Class User
    ' 
    '         Properties: avatar_url, bio, blog, company, created_at
    '                     email, events_url, followers, followers_url, following
    '                     following_url, gists_url, gravatar_id, hireable, html_url
    '                     id, location, login, name, organizations_url
    '                     public_gists, public_repos, received_events_url, repos_url, score
    '                     site_admin, starred_url, subscriptions_url, type, updated_at
    '                     url
    ' 
    '         Function: ToString
    ' 
    '     Structure UserModel
    ' 
    '         Properties: Followers, Followings, Repositories, Stars, User
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace [Class]

    Public Class SearchResult(Of T As Class)
        Public Property total_count As Integer
        Public Property incomplete_results As Boolean
        Public Property items As T()
    End Class

    ''' <summary>
    ''' <see cref="login"/>是主键<see cref="INamedValue.Key"/>
    ''' </summary>
    Public Class User
        Implements INamedValue

        Public Property score As Double
        Public Property login As String Implements INamedValue.Key
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

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Structure UserModel

        Public Property User As User
        Public Property Followers As String()
        Public Property Followings As String()
        Public Property Repositories As String()
        ''' <summary>
        ''' username/repository
        ''' </summary>
        ''' <returns></returns>
        Public Property Stars As NamedValue(Of String)()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace

#Region "Microsoft.VisualBasic::219ae357375e43a8ba23aedb0f91603c, www\githubAPI\API\Search.vb"

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

    '     Module Search
    ' 
    ' 
    '         Enum UserSorts
    ' 
    '             [default], followers, joined, repositories
    ' 
    ' 
    ' 
    '         Enum UserSortOrders
    ' 
    '             asc, desc
    ' 
    '  
    ' 
    ' 
    ' 
    '         Structure UsersQuery
    ' 
    '             Properties: [in], created, followers, language, location
    '                         repos, term, type
    ' 
    '             Function: ToString, (+2 Overloads) Users
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Specialized
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github.Class

Namespace API

    Public Module Search

        Public Enum UserSorts
            [default]
            followers
            repositories
            joined
        End Enum

        Public Enum UserSortOrders
            asc
            desc
        End Enum

        ''' <summary>
        ''' [Search users]
        '''
        ''' Find users via various criteria. (This method returns up To 100 results per page.)
        ''' </summary>
        ''' <param name="q">The search terms.</param>
        ''' <param name="sort">
        ''' The sort field. Can be <see cref="UserSorts.followers"/>, <see cref="UserSorts.repositories"></see>, 
        ''' or <see cref="UserSorts.joined"></see>. Default: results are sorted by best match.
        ''' </param>
        ''' <param name="[order]">
        ''' The sort order if sort parameter is provided. One of <see cref="UserSortOrders.asc"/> or 
        ''' <see cref="UserSortOrders.desc"/>. Default: <see cref="UserSortOrders.desc"/>
        ''' </param>
        ''' <returns></returns>
        Public Function Users(q As NameValueCollection,
                              Optional sort As UserSorts = UserSorts.default,
                              Optional [order] As UserSortOrders = UserSortOrders.desc) As SearchResult(Of User)
            Dim url As String = UsersQuery.API & q.BuildQueryArgs
            If sort <> UserSorts.default Then
                url &= $"+{NameOf(sort)}:{sort.ToString}"
            End If
            ' url &= $"+{NameOf(order)}:{order.ToString}"

            Dim json As String = url.GetRequest(https:=True)
            Return json.LoadJSON(Of SearchResult(Of User))
        End Function

        Public Function Users(q As UsersQuery,
                              Optional sort As UserSorts = UserSorts.default,
                              Optional [order] As UserSortOrders = UserSortOrders.desc) As SearchResult(Of User)
            Return Users(q.Build, sort, order)
        End Function

        Public Structure UsersQuery

            <Term> Public Property term As String
            Public Property type As String
            Public Property [in] As String
            Public Property repos As String
            Public Property location As String
            Public Property language As String
            Public Property created As String
            Public Property followers As String

            Public Const API As String = "https://api.github.com/search/users?q="

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure
    End Module
End Namespace

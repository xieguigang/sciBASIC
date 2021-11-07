#Region "Microsoft.VisualBasic::5eaa0c470aa3f638ac1a9b97eca11515, Data\Trinity\Html\Article.vb"

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

' Structure Article
' 
'     Properties: Content, ContentWithTags, PublishDate, Title
' 
'     Function: ToString
' 
' /********************************************************************************/

#End Region

''' <summary>
''' 文章正文数据模型
''' </summary>
Public Structure Article

    ''' <summary>
    ''' 文章标题
    ''' </summary>
    Public Property title As String

    ''' <summary>
    ''' 正文文本
    ''' </summary>
    Public Property content As String

    ''' <summary>
    ''' 带标签正文
    ''' </summary>
    Public Property contentWithTags As String

    ''' <summary>
    ''' 文章发布时间
    ''' </summary>
    Public Property publishDate As DateTime

    Public Overrides Function ToString() As String
        Return title
    End Function

    Public Shared Function ParseText(html As String,
                                     Optional depth As Integer = 6,
                                     Optional limitCount As Integer = 180,
                                     Optional appendMode As Boolean = False) As Article
        Return New Html2Article With {
            .AppendMode = appendMode,
            .Depth = depth,
            .LimitCount = limitCount
        }.GetArticle(html)
    End Function

End Structure

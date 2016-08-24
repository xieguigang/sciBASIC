#Region "Microsoft.VisualBasic::e516918d9baa61b0a232bff61b2eec7a, ..\visualbasic_App\Microsoft.VisualBasic.Webservices.Google\News\RSS.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

' http://news.google.com/news?pz=1&cf=all&ned=us&hl=en&as_maxm=11&q=allintitle:+zika&as_qdr=a&as_drrb=q&as_mind=26&as_minm=10&cf=all&as_maxd=25&scoring=n&output=rss

''' <summary>
''' URL Example:
''' 
''' ```
''' http://news.google.com/news?pz=1&amp;cf=all&amp;ned=us&amp;hl=en&amp;as_maxm=11&amp;q=allintitle:+zika&amp;as_qdr=a&amp;as_drrb=q&amp;as_mind=26&amp;as_minm=10&amp;cf=all&amp;as_maxd=25&amp;scoring=n&amp;output=rss
''' ```
''' </summary>
<XmlType("rss")>
Public Class RSS : Inherits ClassObject

    <XmlAttribute> Public Property version As String
    Public Property channel As Channel

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function Fetch(url As String) As RSS
        Dim xml As String = url.GET
        Dim value As RSS = xml.CreateObjectFromXml(Of RSS)
        Return value
    End Function

    Public Shared Function GetCurrent(query As String) As RSS
        Dim url As String = $"https://news.google.com/news?q={query.Replace(" ", "+")}&output=rss"
        Return Fetch(url)
    End Function
End Class

Public Class Channel

    Public Property generator As String
    Public Property title As String
    Public Property link As String
    Public Property Language As String
    Public Property webMaster As String
    Public Property copyright As String
    Public Property pubDate As String
    Public Property lastBuildDate As String
    Public Property description As String
    Public Property image As image()
    Public Property item As item()

End Class

Public Class image
    Public Property title As String
    Public Property url As String
    Public Property link As String
End Class

Public Class item
    Public Property title As String
    Public Property link As String
    Public Property guid As guid
    Public Property pubDate As String
    Public Property description As String
End Class

Public Class guid
    <XmlAttribute> Public Property isPermaLink As Boolean
    <XmlText> Public Property text As String
End Class

#Region "Microsoft.VisualBasic::2a9ff72631e7159c86e6568bf38593bb, sciBASIC#\www\WWW.Google\News\RSS.vb"

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


    ' Code Statistics:

    '   Total Lines: 60
    '    Code Lines: 22
    ' Comment Lines: 31
    '   Blank Lines: 7
    '     File Size: 3.48 KB


    '     Class RSS
    ' 
    '         Properties: channel, version
    ' 
    '         Function: Fetch, GetCurrent, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

' http://news.google.com/news?pz=1&cf=all&ned=us&hl=en&as_maxm=11&q=allintitle:+zika&as_qdr=a&as_drrb=q&as_mind=26&as_minm=10&cf=all&as_maxd=25&scoring=n&output=rss

Namespace News

    ''' <summary>
    ''' RSS (Rich Site Summary; originally RDF Site Summary; often called Really Simple Syndication) uses a family of standard web feed formats[2] to publish frequently 
    ''' updated information: blog entries, news headlines, audio, video. An RSS document (called "feed", "web feed",[3] or "channel") includes full or summarized text, 
    ''' and metadata, like publishing date and author's name.
    ''' RSS feeds enable publishers To syndicate data automatically. A standard XML file format ensures compatibility With many different machines/programs. RSS feeds 
    ''' also benefit users who want To receive timely updates from favourite websites Or To aggregate data from many sites.
    ''' Subscribing to a website RSS removes the need for the user to manually check the website for New content. Instead, their browser constantly monitors the site And 
    ''' informs the user of any updates. The browser can also be commanded to automatically download the New data for the user.
    ''' Software termed "RSS reader", "aggregator", Or "feed reader", which can be web-based, desktop-based, Or mobile-device-based, presents RSS feed data To users. 
    ''' Users subscribe To feeds either by entering a feed's URI into the reader or by clicking on the browser's feed icon. The RSS reader checks the user's feeds 
    ''' regularly for new information and can automatically download it, if that function is enabled. The reader also provides a user interface.
    ''' 
    ''' Google news RSS URL Example:
    ''' 
    ''' ```
    ''' http://news.google.com/news?pz=1&amp;cf=all&amp;ned=us&amp;hl=en&amp;as_maxm=11&amp;q=allintitle:+zika&amp;as_qdr=a&amp;as_drrb=q&amp;as_mind=26&amp;as_minm=10&amp;cf=all&amp;as_maxd=25&amp;scoring=n&amp;output=rss
    ''' ```
    ''' </summary>
    <XmlType("rss")>
    Public Class RSS : Inherits BaseClass

        <XmlAttribute> Public Property version As String
        Public Property channel As Channel

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' Download rss data from a exists url specific.
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="proxy">Some region required of proxy server for visit google.</param>
        ''' <returns></returns>
        Public Shared Function Fetch(url As String, Optional proxy As String = Nothing) As RSS
            Dim xml As String = url.GET(proxy:=proxy)
            Dim value As RSS = xml.LoadFromXml(Of RSS)
            Return value
        End Function

        ''' <summary>
        ''' Download rss info from google news by keyword query.
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="proxy"></param>
        ''' <returns></returns>
        Public Shared Function GetCurrent(query As String, Optional proxy As String = Nothing) As RSS
            Dim url As String = $"https://news.google.com/news?q={query.Replace(" ", "+")}&output=rss"
            Return Fetch(url, proxy)
        End Function
    End Class
End Namespace

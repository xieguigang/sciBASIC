Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

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
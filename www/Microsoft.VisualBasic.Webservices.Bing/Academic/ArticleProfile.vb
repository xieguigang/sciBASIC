Imports System.Xml.Serialization

Namespace Academic

    ''' <summary>
    ''' 文献的一些摘要信息
    ''' </summary>
    Public Class ArticleProfile

        Public Property title As String
        Public Property URL As String
        Public Property authors As Link()
        Public Property abstract As String
        <XmlElement("pub-date")>
        Public Property PubDate As Date
        Public Property journal As Link
        Public Property DOI As String
        Public Property keywords As Link()
        ''' <summary>
        ''' 按照年计数的被引用量
        ''' </summary>
        Public Property cites As cites()
        Public Property pages As String
        ''' <summary>
        ''' 卷号
        ''' </summary>
        Public Property volume As String
        ''' <summary>
        ''' 期号
        ''' </summary>
        Public Property issue As String
        ''' <summary>
        ''' 有效的原文来源地址url
        ''' </summary>
        Public Property source As Link()

        Public Overrides Function ToString() As String
            Return $"[{GetProfileID}] {title}"
        End Function
    End Class

    <XmlType("link")>
    Public Structure Link
        <XmlAttribute> Public Property title As String
        <XmlAttribute> Public Property attr As String
        <XmlText>
        Public Property href As String

        Public Overrides Function ToString() As String
            Return $"{title} ({href})"
        End Function
    End Structure

    Public Structure cites

        <XmlAttribute("date")>
        Public Property [Date] As String
        <XmlAttribute("volumn")>
        Public Property Volume As Integer

        Public Overrides Function ToString() As String
            Return $"{Me.Date} := {Volume}"
        End Function
    End Structure
End Namespace
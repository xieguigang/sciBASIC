#Region "Microsoft.VisualBasic::cffaed43fd4e5f56d3c526224668218b, ..\sciBASIC#\www\Microsoft.VisualBasic.Webservices.Bing\Academic\ArticleProfile.vb"

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

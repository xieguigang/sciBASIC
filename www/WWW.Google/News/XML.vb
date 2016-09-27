#Region "Microsoft.VisualBasic::6aea6eebe531b1076f8f49f7bfd4e21a, ..\visualbasic_App\www\WWW.Google\News\XML.vb"

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
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace News

    Public Class Channel

        Public Property generator As String
        Public Property title As String
        Public Property link As String
        Public Property language As String
        Public Property webMaster As String
        Public Property copyright As String
        Public Property pubDate As String
        Public Property lastBuildDate As String
        Public Property description As String
        Public Property ttl As String
        <XmlElement> Public Property image As image()
        <XmlElement> Public Property item As item()

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
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
End Namespace

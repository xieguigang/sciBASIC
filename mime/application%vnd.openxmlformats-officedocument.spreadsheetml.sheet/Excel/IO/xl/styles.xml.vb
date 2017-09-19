#Region "Microsoft.VisualBasic::9575178c459a7845a7d18e42d1b89c9a, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\xl\styles.xml.vb"

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

Namespace XML.xl

    <XmlRoot("styleSheet")>
    Public Class styles
        Public Property fonts As fonts
    End Class

    Public Class List(Of T)
        <XmlAttribute> Public Property count As Integer
    End Class

    Public Class fonts : Inherits List(Of font)
        <XmlElement> Public Property fonts As font()
    End Class

    Public Class font
        Public Property b As Bold
        Public Property sz As StringValue
        Public Property color As ColorValue
        Public Property name As StringValue
        Public Property family As StringValue
        Public Property scheme As StringValue
    End Class

    Public Class StringValue
        <XmlAttribute> Public Property val As String
    End Class

    Public Class ColorValue
        <XmlAttribute> Public Property theme As String
        <XmlAttribute> Public Property rgb As String
    End Class

    Public Class Bold
    End Class
End Namespace
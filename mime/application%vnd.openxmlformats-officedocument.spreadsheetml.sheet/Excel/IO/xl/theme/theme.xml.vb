#Region "Microsoft.VisualBasic::98a962ffb0a69207472f28163e424709, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\xl\theme\theme.xml.vb"

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

Namespace XML.xl.theme

    <XmlRoot("theme", Namespace:="http://schemas.openxmlformats.org/drawingml/2006/main")>
    Public Class theme

        <XmlAttribute>
        Public Property name As String
    End Class

    Public Class font
        <XmlAttribute> Public Property script As String
        <XmlAttribute> Public Property typeface As String
    End Class
End Namespace

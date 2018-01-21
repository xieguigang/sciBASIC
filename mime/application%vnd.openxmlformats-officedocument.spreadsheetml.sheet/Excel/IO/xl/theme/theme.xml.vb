#Region "Microsoft.VisualBasic::e1c6c2c51d632a62bbd3d20c11414c01, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\xl\theme\theme.xml.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

    <XmlRoot("theme", Namespace:=Excel.Xmlns.a)>
    Public Class theme

        <XmlAttribute>
        Public Property name As String
        Public Property themeElements As themeElements

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("a", Excel.Xmlns.a)
        End Sub
    End Class

    Public Class themeElements
        Public Property clrScheme As clrScheme
        Public Property fontScheme As fontScheme
    End Class

    Public Class clrScheme
        <XmlAttribute>
        Public Property name As String

        Public Property dk1 As ThemeColor
        Public Property lt1 As ThemeColor
        Public Property dk2 As ThemeColor
        Public Property lt2 As ThemeColor
        Public Property accent1 As ThemeColor
        Public Property accent2 As ThemeColor
        Public Property accent3 As ThemeColor
        Public Property accent4 As ThemeColor
        Public Property accent5 As ThemeColor
        Public Property accent6 As ThemeColor
        Public Property hlink As ThemeColor
        Public Property folHlink As ThemeColor
    End Class

    Public Class ThemeColor
        Public Property sysClr As ColorValue
        Public Property srgbClr As ColorValue
    End Class

    Public Class fontScheme
        <XmlAttribute>
        Public Property name As String
        Public Property majorFont As Fonts
        Public Property minorFont As Fonts
    End Class

    Public Class Fonts
        Public Property latin As font
        Public Property ea As font
        Public Property cs As font
        <XmlElement("font")>
        Public Property fonts As font()
    End Class

    Public Class font
        <XmlAttribute> Public Property script As String
        <XmlAttribute> Public Property typeface As String
        <XmlAttribute> Public Property panose As String
    End Class
End Namespace

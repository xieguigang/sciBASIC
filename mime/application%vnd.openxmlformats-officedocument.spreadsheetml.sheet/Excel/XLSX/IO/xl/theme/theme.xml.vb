#Region "Microsoft.VisualBasic::9d9e7671efb5687eef30b43d7aa6836e, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\IO\xl\theme\theme.xml.vb"

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

    '   Total Lines: 71
    '    Code Lines: 58
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 2.16 KB


    '     Class theme
    ' 
    '         Properties: name, themeElements
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class themeElements
    ' 
    '         Properties: clrScheme, fontScheme
    ' 
    '     Class clrScheme
    ' 
    '         Properties: accent1, accent2, accent3, accent4, accent5
    '                     accent6, dk1, dk2, folHlink, hlink
    '                     lt1, lt2, name
    ' 
    '     Class ThemeColor
    ' 
    '         Properties: srgbClr, sysClr
    ' 
    '     Class fontScheme
    ' 
    '         Properties: majorFont, minorFont, name
    ' 
    '     Class Fonts
    ' 
    '         Properties: cs, ea, fonts, latin
    ' 
    '     Class font
    ' 
    '         Properties: panose, script, typeface
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX.XML.xl.theme

    <XmlRoot("theme", Namespace:=OpenXML.a)>
    Public Class theme

        <XmlAttribute>
        Public Property name As String
        Public Property themeElements As themeElements

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces

            xmlns.Add("a", OpenXML.a)
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

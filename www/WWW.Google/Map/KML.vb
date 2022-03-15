#Region "Microsoft.VisualBasic::4065fabee788aa15e161924e237c9587, sciBASIC#\www\WWW.Google\Map\KML.vb"

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
    '    Code Lines: 44
    ' Comment Lines: 0
    '   Blank Lines: 16
    '     File Size: 1.69 KB


    '     Class KML
    ' 
    '         Properties: Documents
    ' 
    '     Class node
    ' 
    '         Properties: name
    ' 
    '     Class Document
    ' 
    '         Properties: description, Folder, NetworkLink, StyleMaps, Styles
    ' 
    '         Function: ToString
    ' 
    '     Class NetworkLink
    ' 
    '         Properties: Link
    ' 
    '     Class Folder
    ' 
    '         Properties: marks
    ' 
    '     Class Placemark
    ' 
    '         Properties: description, ExtendedData, Point, styleUrl
    ' 
    '     Class Point
    ' 
    '         Properties: coordinates
    ' 
    '     Class ExtendedData
    ' 
    '         Properties: data
    ' 
    '     Class Data
    ' 
    '         Properties: name, value
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Map

    <XmlRoot("kml")> Public Class KML

        Public Const xmlns As String = "http://www.opengis.net/kml/2.2"

        <XmlElement("Document")>
        Public Property Documents As Document()

    End Class

    Public MustInherit Class node
        Public Property name As String
    End Class

    Public Class Document : Inherits node

        Public Property description As String
        Public Property Folder As Folder
        <XmlElement("Style")> Public Property Styles As Style()
        <XmlElement("StyleMap")> Public Property StyleMaps As StyleMap()
        Public Property NetworkLink As NetworkLink

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Class NetworkLink : Inherits node
        Public Property Link As Link
    End Class

    Public Class Folder : Inherits node

        <XmlElement("Placemark")> Public Property marks As Placemark()
    End Class

    Public Class Placemark : Inherits node
        Public Property description As String
        Public Property styleUrl As String
        Public Property ExtendedData As ExtendedData
        Public Property Point As Point
    End Class

    Public Class Point
        Public Property coordinates As String
    End Class

    Public Class ExtendedData
        <XmlElement("Data")> Public Property data As Data()
    End Class

    Public Class Data
        <XmlAttribute> Public Property name As String
        Public Property value As String
    End Class
End Namespace

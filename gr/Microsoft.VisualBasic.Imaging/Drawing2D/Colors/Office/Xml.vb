#Region "Microsoft.VisualBasic::f01be840fdbaea60c129e724ba0c78fa, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Office\Xml.vb"

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

    '   Total Lines: 55
    '    Code Lines: 45 (81.82%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (18.18%)
    '     File Size: 1.99 KB


    '     Class OfficeColorTheme
    ' 
    '         Properties: accents, dk1, dk2, folHlink, hlink
    '                     lt1, lt2, name
    ' 
    '         Function: GetAccentColors, LoadFromXml, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

Namespace Drawing2D.Colors.OfficeAccent

    <XmlRoot("clrScheme")> Public Class OfficeColorTheme : Implements INamedValue

        <XmlAttribute>
        Public Property name As String Implements INamedValue.Key
        Public Property dk1 As ObjectColor
        Public Property lt1 As ObjectColor
        Public Property dk2 As Accent
        Public Property lt2 As Accent
        Public Property hlink As Accent
        Public Property folHlink As Accent

        <XmlElement("accent")>
        Public Property accents As Accent()

        Public Function GetAccentColors() As Color()
            Return accents _
                .Select(Function(x) x.srgbClr.Color) _
                .ToArray
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        Public Shared Function LoadFromXml(xml$) As OfficeColorTheme
            Dim s As New StringBuilder(
                If(xml.FileExists, xml.ReadAllText, xml))

            Call s.Replace("xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main""", "")
            Call s.Replace("a:clrScheme", "clrScheme")
            Call s.Replace("a:srgbClr", "srgbClr")
            Call s.Replace("a:sysClr", "sysClr")
            Call s.Replace("a:folHlink", "folHlink")
            Call s.Replace("a:hlink", "hlink")
            Call s.Replace("a:lt", "lt")
            Call s.Replace("a:dk", "dk")

            xml = s.ToString
            xml = Regex.Replace(xml, "a:accent\d+", "accent")

            Dim t As OfficeColorTheme = xml.LoadFromXml(Of OfficeColorTheme)
            Return t
        End Function
    End Class
End Namespace

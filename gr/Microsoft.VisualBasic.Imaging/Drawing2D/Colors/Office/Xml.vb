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
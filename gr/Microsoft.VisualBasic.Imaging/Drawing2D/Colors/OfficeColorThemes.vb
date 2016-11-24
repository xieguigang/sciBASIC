Imports System.Drawing
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Module OfficeColorThemes

        Public ReadOnly Property Office2016 As Theme
        Public ReadOnly Property Office2010 As Theme
        Public ReadOnly Property Slipstream As Theme

        Sub New()
            Office2016 = Theme.LoadFromXml(My.Resources.Default_Office)
            Office2010 = Theme.LoadFromXml(My.Resources.Default_Office2007_2010)
            Slipstream = Theme.LoadFromXml(My.Resources.Default_Slipstream)
        End Sub

        ''' <summary>
        ''' If found failure, default is reutrns the theme <see cref="Office2016"/>
        ''' </summary>
        ''' <param name="theme$"></param>
        ''' <returns></returns>
        Public Function GetAccentColors(theme$) As Color()
            Select Case LCase(theme)
                Case LCase(NameOf(Office2010)), LCase(Office2010.name)
                    Return Office2010.accents _
                        .Select(Function(x) x.srgbClr.Color) _
                        .ToArray
                Case LCase(NameOf(Slipstream)), LCase(Slipstream.name)
                    Return Slipstream.accents _
                        .Select(Function(x) x.srgbClr.Color) _
                        .ToArray
                Case Else
                    Return Office2016.accents _
                        .Select(Function(x) x.srgbClr.Color) _
                        .ToArray
            End Select
        End Function
    End Module

    <XmlRoot("clrScheme")> Public Class Theme

        <XmlAttribute>
        Public Property name As String
        Public Property dk1 As ObjectColor
        Public Property lt1 As ObjectColor
        Public Property dk2 As Accent
        Public Property lt2 As Accent
        Public Property hlink As Accent
        Public Property folHlink As Accent

        <XmlElement("accent")>
        Public Property accents As Accent()

        Public Shared Function LoadFromXml(xml$) As Theme
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

            Dim t As Theme = xml.LoadFromXml(Of Theme)
            Return t
        End Function

        Public Structure ObjectColor
            Public Property sysClr As sysClr
        End Structure

        Public Structure Accent
            Public Property srgbClr As srgbClr
        End Structure

        Public Structure sysClr
            <XmlAttribute> Public Property val As String
            <XmlAttribute> Public Property lastClr As String

            Public Overrides Function ToString() As String
                Return Me.GetJson
            End Function
        End Structure

        ''' <summary>
        ''' 颜色值
        ''' </summary>
        Public Structure srgbClr

            ''' <summary>
            ''' 颜色值
            ''' </summary>
            ''' <returns></returns>
            <XmlAttribute> Public Property val As String

            Public ReadOnly Property Color As Color
                Get
                    Return ColorTranslator.FromHtml("#" & val)
                End Get
            End Property

            Public Overrides Function ToString() As String
                Return val
            End Function
        End Structure
    End Class
End Namespace
#Region "Microsoft.VisualBasic::30bd3db2485adb086a3387c042653c12, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\OfficeColorThemes.vb"

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

Imports System.Drawing
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Module OfficeColorThemes

        Public ReadOnly Property Office2016 As Theme
        Public ReadOnly Property Office2010 As Theme
        Public ReadOnly Property Slipstream As Theme
        Public ReadOnly Property Marquee As Theme
        Public ReadOnly Property Aspect As Theme
        Public ReadOnly Property Paper As Theme

        Sub New()
            Office2016 = Theme.LoadFromXml(My.Resources.Default_Office)
            Office2010 = Theme.LoadFromXml(My.Resources.Default_Office2007_2010)
            Marquee = Theme.LoadFromXml(My.Resources.Default_Marquee)
            Aspect = Theme.LoadFromXml(My.Resources.Default_Aspect)
            Paper = Theme.LoadFromXml(My.Resources.Default_Paper)
            Slipstream = Theme.LoadFromXml(My.Resources.Default_Slipstream)

            Call __loadAllThemes()
        End Sub

        ''' <summary>
        ''' All office color themes
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Themes As New Dictionary(Of Theme)

        Private Sub __loadAllThemes()
            Dim resMgr As Type = GetType(My.Resources.Resources)
            Dim datas As IEnumerable(Of PropertyInfo) =
                DataFramework _
                .Schema(resMgr, PropertyAccess.Readable, BindingFlags.NonPublic Or BindingFlags.Static, True) _
                .Where(Function(k) InStr(k.Key, "Default_") = 1) _
                .Select(Function(x) x.Value) _
                .ToArray

            For Each theme As PropertyInfo In datas
                Dim xml As String = TryCast(theme.GetValue(Nothing, Nothing), String)
                Dim t As Theme = Drawing2D.Colors.Theme.LoadFromXml(xml)

                t.name = t.name.Replace("Default_", "")
                Call Themes.Add(t) ' 顺序不能变换，否则键名就不一致了
            Next
        End Sub

        ''' <summary>
        ''' If found failure, default is reutrns the theme <see cref="Office2016"/>
        ''' </summary>
        ''' <param name="theme$"></param>
        ''' <returns></returns>
        Public Function GetAccentColors(theme$) As Color()
            If Themes.ContainsKey(theme) Then
                Return Themes(theme).GetAccentColors
            Else
                For Each t As Theme In Themes.Values
                    If t.name.TextEquals(theme) Then
                        Return t.GetAccentColors
                    End If
                Next
            End If

            Return Office2016.GetAccentColors
        End Function
    End Module

    <XmlRoot("clrScheme")> Public Class Theme : Implements INamedValue

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

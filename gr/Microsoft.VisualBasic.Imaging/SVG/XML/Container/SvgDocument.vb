﻿#Region "Microsoft.VisualBasic::880a5c941ef14a401467161e07a574af, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgDocument.vb"

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

    '   Total Lines: 279
    '    Code Lines: 167 (59.86%)
    ' Comment Lines: 80 (28.67%)
    '    - Xml Docs: 76.25%
    ' 
    '   Blank Lines: 32 (11.47%)
    '     File Size: 10.58 KB


    '     Class SvgDocument
    ' 
    '         Properties: BackgroundFill, defs, Height, ViewBox, ViewSize
    '                     Width, X, Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Background, Create, GetSVGXml, Parse, Save
    '                   (+2 Overloads) SaveAsXml, SetStyle, Size
    ' 
    '         Sub: SetCommentText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Xml
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' SVG: Scalable Vector Graphics
    ''' 
    ''' Scalable Vector Graphics (SVG) is an XML-based markup language for describing 
    ''' two-dimensional based vector graphics.
    ''' </summary>
    ''' <remarks>
    ''' As such, it's a text-based, open Web standard for describing images that can 
    ''' be rendered cleanly at any size and are designed specifically to work well with 
    ''' other web standards including CSS, DOM, JavaScript, and SMIL. SVG is, 
    ''' essentially, to graphics what HTML is to text.
    '''
    ''' SVG images And their related behaviors are defined In XML text files, which means
    ''' they can be searched, indexed, scripted, And compressed. Additionally, this means 
    ''' they can be created And edited With any text editor Or With drawing software.
    '''
    ''' Compared to classic bitmapped image formats such as JPEG Or PNG, SVG-format vector 
    ''' images can be rendered at any size without loss of quality And can be easily 
    ''' localized by updating the text within them, without the need of a graphical editor
    ''' to do so. With proper libraries, SVG files can even be localized on-the-fly.
    '''
    ''' SVG has been developed by the World Wide Web Consortium (W3C) since 1999.
    ''' </remarks>
    Public NotInheritable Class SvgDocument : Inherits SvgContainer
        Implements ISaveHandle

        ''' <summary>
        ''' the xml document model of the current svg document object
        ''' </summary>
        ReadOnly _document As XmlDocument

        Dim _defs As SvgDefs

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this property ensure that always has the value.
        ''' </returns>
        Public ReadOnly Property defs As SvgDefs
            Get
                If _defs Is Nothing Then
                    For Each element As SvgElement In GetElements()
                        If TypeOf element Is SvgDefs Then
                            _defs = element
                            Return _defs
                        End If
                    Next

                    Return SvgDefs.Create(Element)
                End If

                Return _defs
            End Get
        End Property

        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        Public Property Width As Double?
            Get
                Return Element.GetAttribute("width", Attributes.Size.Width)
            End Get
            Set(value As Double?)
                If value IsNot Nothing Then
                    Element.SetAttribute("width", value.Value)
                ElseIf Element.HasAttribute("width") Then
                    Element.RemoveAttribute("width")
                End If
            End Set
        End Property

        Public Property Height As Double?
            Get
                Return Element.GetAttribute("height", Attributes.Size.Height)
            End Get
            Set(value As Double?)
                If value IsNot Nothing Then
                    Element.SetAttribute("height", value.Value)
                ElseIf Element.HasAttribute("height") Then
                    Element.RemoveAttribute("height")
                End If
            End Set
        End Property

        Public Property ViewBox As SvgViewBox
            Get
                Return Element.GetAttribute("viewBox", New SvgViewBox())
            End Get
            Set(value As SvgViewBox)
                Element.SetAttribute("viewBox", value.ToString())
            End Set
        End Property

        ''' <summary>
        ''' get the canvas view size of the current svg document.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ViewSize As Size
            Get
                With ViewBox
                    Return New Size(.Width, .Height)
                End With
            End Get
        End Property

        Public ReadOnly Property BackgroundFill As String
            Get
                ' style="background-color: lightblue;"
                Dim style = CssParser.ParseStyle(Me.Style)
                Dim color As String = style("background-color")

                Return color
            End Get
        End Property

        Private Sub New(document As XmlDocument, element As XmlElement)
            MyBase.New(element)
            _document = document
        End Sub

        ''' <summary>
        ''' set svg canvas size and viewbox size
        ''' </summary>
        ''' <param name="sz">set the viewbox size only</param>
        ''' <returns></returns>
        Public Function Size(sz As Size) As SvgDocument
            ' 20240910 svg width/height is a fixed layout data, if we set width via the css, then height will not change
            ' this will caused a layout error
            ' viewbox is better than the width/height
            ' so removes the width and height when config size
            Width = Nothing
            Height = Nothing
            ViewBox = New Double() {0, 0, sz.Width, sz.Height}
            Return Me
        End Function

        Public Overloads Function SetStyle(css As String) As SvgDocument
            Style = css
            Return Me
        End Function

        Public Function Background(bg As String) As SvgDocument
            Call SetStyle("background", bg)
            Return Me
        End Function

        Public Overloads Shared Function Create() As SvgDocument
            Dim document = New XmlDocument()
            Dim rootElement = document.CreateElement("svg")
            document.AppendChild(rootElement)
            rootElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg")
            rootElement.SetAttribute("preserveAspectRatio", "xMaxYMax")
            Return New SvgDocument(document, rootElement)
        End Function

        ''' <summary>
        ''' Parse the given xml document text as svg doucment model
        ''' </summary>
        ''' <param name="xml"></param>
        ''' <returns>
        ''' this function will returns nothing if this parser function is not 
        ''' in strict mode and also the given <paramref name="xml"/> text is 
        ''' empty.
        ''' </returns>
        Public Shared Function Parse(xml As String, Optional strict As Boolean = True) As SvgDocument
            Dim xmlDoc As New XmlDocument
            Dim svgEle As XmlElement

            Const bom1 As Char = ChrW(&HFEFF)

            xml = xml.Replace(bom1, "")

            If strict Then
                ' 20250117 the exception is not captured in strict mode
                ' and the .net internal error of xml parser will be throw
                ' if the given xml is invalid
                Call xmlDoc.LoadXml(xml)
            ElseIf xml.StringEmpty(, True) Then
                ' given xml string is empty and also 
                ' not in strict mode
                ' returns nothing directly
                Return Nothing
            Else
                Dim settings As New XmlReaderSettings()
                settings.IgnoreWhitespace = True
                settings.ConformanceLevel = ConformanceLevel.Fragment
                settings.ValidationType = ValidationType.None
                settings.XmlResolver = Nothing

                Using stringReader As New StringReader(xml)
                    Using reader As XmlReader = XmlReader.Create(stringReader, settings)
                        xmlDoc.XmlResolver = Nothing
                        xmlDoc.Load(reader)
                    End Using
                End Using
            End If

            svgEle = xmlDoc.DocumentElement

            Return New SvgDocument(xmlDoc, svgEle)
        End Function

        Public Sub SetCommentText(text As String)
            Call _document.CreateComment(text)
        End Sub

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function SaveAsXml(path$, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using s As Stream = path.Open(FileMode.OpenOrCreate, doClear:=True)
                Return Save(s, encoding)
            End Using
        End Function

        Public Function Save(s As Stream, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Call _document.Save(s)
            Call s.Flush()
            Return True
        End Function

        ''' <summary>
        ''' get xml document string of current svg object
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSVGXml() As String
            Dim s As New MemoryStream
            Call Save(s, Encoding.UTF8)
            Return Encoding.UTF8.GetString(s.ToArray)
        End Function

        ''' <summary>
        ''' Save this svg document object into the file system.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SaveAsXml(path$, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return SaveAsXml(path, encoding.CodePage)
        End Function
    End Class
End Namespace

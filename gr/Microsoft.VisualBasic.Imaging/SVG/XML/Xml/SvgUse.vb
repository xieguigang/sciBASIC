#Region "Microsoft.VisualBasic::01e2e39eb02cb342c307f1d07ae7c134, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Xml\SvgUse.vb"

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

    '   Total Lines: 113
    '    Code Lines: 62 (54.87%)
    ' Comment Lines: 41 (36.28%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (8.85%)
    '     File Size: 4.07 KB


    '     Class SvgUse
    ' 
    '         Properties: height, href, width, x, xlink_href
    '                     y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;use> element takes nodes from within the SVG document, and 
    ''' duplicates them somewhere else. The effect is the same as if the 
    ''' nodes were deeply cloned into a non-exposed DOM, then pasted where
    ''' the use element is, much like cloned template elements.
    ''' </summary>
    Public Class SvgUse : Inherits SvgElement

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        ''' <summary>
        ''' The URL to an element/fragment that needs to be duplicated. 
        ''' See Usage notes for details on common pitfalls.
        ''' Value type: &lt;URL> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property href As String
            Get
                Return Element.GetAttribute("href", defaultValue:="")
            End Get
            Set(value As String)
                Element.SetAttribute("href", value)
            End Set
        End Property

        ''' <summary>
        ''' An &lt;IRI> reference to an element/fragment that needs to be 
        ''' duplicated. If both href and xlink:href are present, the value
        ''' given by href is used.
        ''' Value type: &lt;IRI> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property xlink_href As String
            Get
                Return Element.GetAttribute("href", SVGWriter.Xlink, "")
            End Get
            Set(value As String)
                Element.SetAttribute("href", SVGWriter.Xlink, value)
            End Set
        End Property

        ''' <summary>
        ''' The x coordinate of an additional final offset transformation
        ''' applied to the &lt;use> element.
        ''' Value type: &lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property x As Single
            Get
                Return Element.GetAttribute("x", 0)
            End Get
            Set(value As Single)
                Element.SetAttribute("x", value)
            End Set
        End Property

        ''' <summary>
        ''' The y coordinate of an additional final offset transformation
        ''' applied to the &lt;use> element.
        ''' Value type: &lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property y As Single
            Get
                Return Element.GetAttribute("y", 0)
            End Get
            Set(value As Single)
                Element.SetAttribute("y", value)
            End Set
        End Property

        ''' <summary>
        ''' The width of the use element.
        ''' Value type: &lt;length> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property width As Single
            Get
                Return Element.GetAttribute("width", 0.0)
            End Get
            Set(value As Single)
                Element.SetAttribute("width", value)
            End Set
        End Property

        ''' <summary>
        ''' The height of the use element.
        ''' Value type: &lt;length> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property height As Single
            Get
                Return Element.GetAttribute("height", 0.0)
            End Get
            Set(value As Single)
                Element.SetAttribute("height", value)
            End Set
        End Property

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgUse
            Dim element = parent.OwnerDocument.CreateElement("use")
            parent.AppendChild(element)
            Return New SvgUse(element)
        End Function
    End Class
End Namespace

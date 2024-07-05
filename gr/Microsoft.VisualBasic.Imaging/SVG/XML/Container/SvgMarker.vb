#Region "Microsoft.VisualBasic::7c158be157f84d8d6ec4e88acf63a208, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgMarker.vb"

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

    '   Total Lines: 96
    '    Code Lines: 54 (56.25%)
    ' Comment Lines: 33 (34.38%)
    '    - Xml Docs: 96.97%
    ' 
    '   Blank Lines: 9 (9.38%)
    '     File Size: 3.55 KB


    '     Class SvgMarker
    ' 
    '         Properties: Height, RefX, RefY, ViewBox, Width
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
    ''' The &lt;marker> element defines a graphic used for drawing arrowheads or
    ''' polymarkers on a given &lt;path>, &lt;line>, &lt;polyline> or &lt;polygon>
    ''' element.
    '''
    ''' Markers can be attached To shapes Using the marker-start, marker-mid, And 
    ''' marker-End properties.
    ''' </summary>
    Public NotInheritable Class SvgMarker : Inherits SvgContainer

        ''' <summary>
        ''' This attribute defines the x coordinate for the reference point of the marker. 
        ''' Value type: left|center|right|&lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property RefX As Double
            Get
                Return Element.GetAttribute("refX", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refX", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the y coordinate for the reference point of the marker. 
        ''' Value type: top|center|bottom|&lt;coordinate> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property RefY As Double
            Get
                Return Element.GetAttribute("refY", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("refY", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the width of the marker viewport. Value type: &lt;length> ; 
        ''' Default value: 3; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property Width As Double
            Get
                Return Element.GetAttribute("markerWidth", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerWidth", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the height of the marker viewport. Value type: &lt;length> ; 
        ''' Default value: 3; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property Height As Double
            Get
                Return Element.GetAttribute("markerHeight", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("markerHeight", value)
            End Set
        End Property

        ''' <summary>
        ''' This attribute defines the bound of the SVG viewport for the current SVG fragment. 
        ''' Value type: &lt;list-of-numbers> ; Default value: none; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property ViewBox As SvgViewBox
            Get
                Return Element.GetAttribute("viewBox", New SvgViewBox())
            End Get
            Set(value As SvgViewBox)
                Element.SetAttribute("viewBox", value.ToString())
            End Set
        End Property

        Public Property orient As String
            Get
                Return Element.GetAttribute("orient")
            End Get
            Set(value As String)
                Element.SetAttribute("orient", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgMarker
            Dim element = parent.OwnerDocument.CreateElement("marker")
            parent.AppendChild(element)
            Return New SvgMarker(element)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::b689c42ed4208d898c889d16056dbfd3, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\rect.vb"

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

    '   Total Lines: 110
    '    Code Lines: 76
    ' Comment Lines: 22
    '   Blank Lines: 12
    '     File Size: 3.55 KB


    '     Class SvgRect
    ' 
    '         Properties: Height, rx, ry, Width, X
    '                     Y
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Create
    ' 
    '         Sub: (+2 Overloads) SetRectangle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;rect> element draws a rectangle on the screen. There are six basic attributes
    ''' that control the position and shape of the rectangles on screen. The one on the right
    ''' has its rx and ry parameters set, giving it rounded corners. If they're not set, they 
    ''' default to 0.
    ''' </summary>
    Public NotInheritable Class SvgRect
        Inherits SvgBasicShape

        ''' <summary>
        ''' The x position of the top left corner of the rectangle.
        ''' </summary>
        ''' <returns></returns>
        Public Property X As Double
            Get
                Return Element.GetAttribute("x", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of the top left corner of the rectangle.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y As Double
            Get
                Return Element.GetAttribute("y", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y", value)
            End Set
        End Property

        ''' <summary>
        ''' The width of the rectangle.
        ''' </summary>
        ''' <returns></returns>
        Public Property Width As Double
            Get
                Return Element.GetAttribute("width", Attributes.Size.Width)
            End Get
            Set(value As Double)
                Element.SetAttribute("width", value)
            End Set
        End Property

        ''' <summary>
        ''' The height of the rectangle.
        ''' </summary>
        ''' <returns></returns>
        Public Property Height As Double
            Get
                Return Element.GetAttribute("height", Attributes.Size.Height)
            End Get
            Set(value As Double)
                Element.SetAttribute("height", value)
            End Set
        End Property

        Public Property rx As Double
            Get
                Return Element.GetAttribute("rx", Attributes.Radius.RX)
            End Get
            Set(value As Double)
                Element.SetAttribute("rx", value)
            End Set
        End Property

        Public Property ry As Double
            Get
                Return Element.GetAttribute("ry", Attributes.Radius.RY)
            End Get
            Set(value As Double)
                Element.SetAttribute("ry", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Sub SetRectangle(rect As RectangleF)
            X = rect.Left
            Y = rect.Top
            Width = rect.Width
            Height = rect.Height
        End Sub

        Public Sub SetRectangle(location As PointF, size As SizeF)
            X = location.X
            Y = location.Y
            Width = size.Width
            Height = size.Height
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgRect
            Dim element = parent.OwnerDocument.CreateElement("rect")
            parent.AppendChild(element)
            Return New SvgRect(element)
        End Function
    End Class
End Namespace

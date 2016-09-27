#Region "Microsoft.VisualBasic::e4168db06ccf899918c96abd7fcfc460, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\SVG\g.vb"

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

Imports System.Xml.Serialization

Namespace SVG

    ''' <summary>
    ''' SVG之中的画布对象，<see cref="SVGXml"/>和<see cref="g"/>都属于这种类型
    ''' </summary>
    Public Interface ICanvas
        Property transform As String
        Property texts As text()
        Property gs As g()
        Property path As path()
        Property rect As rect()
        Property polygon As polygon()
        Property lines As line()
        Property circles As circle()
        Property title As String
    End Interface

    ''' <summary>
    ''' SVG graphics unit
    ''' </summary>
    Public Class g : Inherits node
        Implements ICanvas

        <XmlAttribute> Public Property transform As String Implements ICanvas.transform
        <XmlElement("text")> Public Property texts As text() Implements ICanvas.texts
        <XmlElement("g")> Public Property gs As g() Implements ICanvas.gs
        <XmlElement> Public Property path As path() Implements ICanvas.path
        <XmlElement> Public Property rect As rect() Implements ICanvas.rect
        <XmlElement> Public Property polygon As polygon() Implements ICanvas.polygon
        <XmlElement("line")> Public Property lines As line() Implements ICanvas.lines
        <XmlElement("circle")> Public Property circles As circle() Implements ICanvas.circles
        <XmlAttribute> Public Property fill As String
        <XmlElement> Public Property title As String Implements ICanvas.title
    End Class
End Namespace

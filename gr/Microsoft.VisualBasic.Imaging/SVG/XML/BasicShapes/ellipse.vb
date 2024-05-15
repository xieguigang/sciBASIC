#Region "Microsoft.VisualBasic::bc01d1ad2c683f9097d0565d3cab7fb9, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\ellipse.vb"

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

    '   Total Lines: 76
    '    Code Lines: 47
    ' Comment Lines: 21
    '   Blank Lines: 8
    '     File Size: 2.46 KB


    '     Class SvgEllipse
    ' 
    '         Properties: CX, CY, RX, RY
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
    ''' An &lt;ellipse> is a more general form of the &lt;circle> element, where you 
    ''' can scale the x and y radius (commonly referred to as the semimajor and 
    ''' semiminor axes in maths) of the circle separately.
    ''' </summary>
    Public NotInheritable Class SvgEllipse
        Inherits SvgBasicShape

        ''' <summary>
        ''' The x position of the center of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property CX As Double
            Get
                Return Element.GetAttribute("cx", Attributes.Position.CX)
            End Get
            Set(value As Double)
                Element.SetAttribute("cx", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of the center of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property CY As Double
            Get
                Return Element.GetAttribute("cy", Attributes.Position.CY)
            End Get
            Set(value As Double)
                Element.SetAttribute("cy", value)
            End Set
        End Property

        ''' <summary>
        ''' The x radius of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property RX As Double
            Get
                Return Element.GetAttribute("rx", Attributes.Radius.RX)
            End Get
            Set(value As Double)
                Element.SetAttribute("rx", value)
            End Set
        End Property

        ''' <summary>
        ''' The y radius of the ellipse.
        ''' </summary>
        ''' <returns></returns>
        Public Property RY As Double
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

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgEllipse
            Dim element = parent.OwnerDocument.CreateElement("ellipse")
            parent.AppendChild(element)
            Return New SvgEllipse(element)
        End Function
    End Class
End Namespace

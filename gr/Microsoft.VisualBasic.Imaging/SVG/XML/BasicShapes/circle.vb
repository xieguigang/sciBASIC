#Region "Microsoft.VisualBasic::984b5701aa169afe37a96cfb3f1a3f80, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\circle.vb"

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

    '   Total Lines: 70
    '    Code Lines: 44 (62.86%)
    ' Comment Lines: 18 (25.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (11.43%)
    '     File Size: 2.48 KB


    '     Class SvgCircle
    ' 
    '         Properties: CX, CY, R
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create, SetCircle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;circle> SVG element is an SVG basic shape, used to draw circles based on a center point and a radius.
    ''' </summary>
    Public NotInheritable Class SvgCircle : Inherits SvgBasicShape

        ''' <summary>
        ''' The x-axis coordinate of the center of the circle. Value type: &lt;length>|&lt;percentage>; 
        ''' Default value: 0; Animatable: yes
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
        ''' The y-axis coordinate of the center of the circle. Value type: &lt;length>|&lt;percentage>; 
        ''' Default value: 0; Animatable: yes
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
        ''' The radius of the circle. A value lower or equal to zero disables rendering of the circle.
        ''' Value type: &lt;length>|&lt;percentage> ; Default value: 0; Animatable: yes
        ''' </summary>
        ''' <returns></returns>
        Public Property R As Double
            Get
                Return Element.GetAttribute("r", Attributes.Radius.R)
            End Get
            Set(value As Double)
                Element.SetAttribute("r", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function SetCircle(x As Double, y As Double, r As Double) As SvgCircle
            Me.CX = x
            Me.CY = y
            Me.R = r
            Return Me
        End Function

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgCircle
            Dim element = parent.OwnerDocument.CreateElement("circle")
            parent.AppendChild(element)
            Return New SvgCircle(element)
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::696032cb1627afa45ca0ff7b3ffe3468, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\line.vb"

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

    '   Total Lines: 91
    '    Code Lines: 60
    ' Comment Lines: 20
    '   Blank Lines: 11
    '     File Size: 2.73 KB


    '     Class SvgLine
    ' 
    '         Properties: X1, X2, Y1, Y2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Create, SetPoint
    ' 
    '         Sub: SetPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Xml
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' The &lt;line> element takes the positions of two points as 
    ''' parameters and draws a straight line between them.
    ''' </summary>
    Public NotInheritable Class SvgLine : Inherits SvgBasicShape

        ''' <summary>
        ''' The x position of point 1.
        ''' </summary>
        ''' <returns></returns>
        Public Property X1 As Double
            Get
                Return Element.GetAttribute("x1", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x1", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of point 1.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y1 As Double
            Get
                Return Element.GetAttribute("y1", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y1", value)
            End Set
        End Property

        ''' <summary>
        ''' The x position of point 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property X2 As Double
            Get
                Return Element.GetAttribute("x2", Attributes.Position.X)
            End Get
            Set(value As Double)
                Element.SetAttribute("x2", value)
            End Set
        End Property

        ''' <summary>
        ''' The y position of point 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property Y2 As Double
            Get
                Return Element.GetAttribute("y2", Attributes.Position.Y)
            End Get
            Set(value As Double)
                Element.SetAttribute("y2", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Sub SetPoint(a As PointF, b As PointF)
            X1 = a.X
            Y1 = a.Y
            X2 = b.X
            Y2 = b.Y
        End Sub

        Public Function SetPoint(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As SvgLine
            Me.X1 = x1
            Me.X2 = x2
            Me.Y1 = y1
            Me.Y2 = y2

            Return Me
        End Function

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgLine
            Dim element = parent.OwnerDocument.CreateElement("line")
            parent.AppendChild(element)
            Return New SvgLine(element)
        End Function
    End Class
End Namespace

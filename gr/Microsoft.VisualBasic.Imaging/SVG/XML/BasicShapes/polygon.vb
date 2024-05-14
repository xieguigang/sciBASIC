#Region "Microsoft.VisualBasic::457b91a12f0f8cae21955c9a630a20a4, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\polygon.vb"

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

    '   Total Lines: 79
    '    Code Lines: 58
    ' Comment Lines: 13
    '   Blank Lines: 8
    '     File Size: 3.29 KB


    '     Class SvgPolygon
    ' 
    '         Properties: FillRule, Points
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Create
    ' 
    '         Sub: SetPolygonPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' A &lt;polygon> is similar to a &lt;polyline>, in that it is composed of straight
    ''' line segments connecting a list of points. For polygons though, the path 
    ''' automatically connects the last point with the first, creating a closed shape.
    ''' </summary>
    Public NotInheritable Class SvgPolygon
        Inherits SvgElement

        ''' <summary>
        ''' A list of points, each number separated by a space, comma, EOL, or a line
        ''' feed character with additional whitespace permitted. Each point must contain 
        ''' two numbers: an x coordinate and a y coordinate. So, the list (0,0), (1,1), 
        ''' and (2,2) could be written as 0, 0 1, 1 2, 2. The drawing then closes the 
        ''' path, so a final straight line would be drawn from (2,2) to (0,0).
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As Double()
            Get
                Dim stringArray = Element.GetAttribute("points")
                Dim d = stringArray.StringSplit("\s*,\s*")

                Return d _
                    .Select(Function(value)
                                Return Double.Parse(value, CultureInfo.InvariantCulture)
                            End Function) _
                    .ToArray()
            End Get
            Set(value As Double())
                Element.SetAttribute("points", value.Select(Function(x) x.ToString("G", CultureInfo.InvariantCulture)).JoinBy(","))
            End Set
        End Property

        Public Property FillRule As SvgFillRule
            Get
                Return Element.GetAttribute(Of SvgFillRule)("fill-rule", Attributes.FillAndStroke.FillRule)
            End Get
            Set(value As SvgFillRule)
                Element.SetAttribute("fill-rule", value)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetPolygonPath(points As IEnumerable(Of PointF))
            Call Element.SetAttribute(
                name:="points",
                value:=points _
                    .SafeQuery _
                    .Select(Iterator Function(pt) As IEnumerable(Of Double)
                                Yield pt.X
                                Yield pt.Y
                            End Function) _
                    .IteratesALL _
                    .Select(Function(d)
                                Return d.ToString("G", CultureInfo.InvariantCulture)
                            End Function) _
                    .JoinBy(","))
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgPolygon
            Dim element = parent.OwnerDocument.CreateElement("polygon")
            parent.AppendChild(element)
            Return New SvgPolygon(element)
        End Function
    End Class
End Namespace

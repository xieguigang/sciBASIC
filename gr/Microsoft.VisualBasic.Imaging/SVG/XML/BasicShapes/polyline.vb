#Region "Microsoft.VisualBasic::fb34045d0f32bd3b0d8195b19e50d32b, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\polyline.vb"

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

    '   Total Lines: 41
    '    Code Lines: 24
    ' Comment Lines: 12
    '   Blank Lines: 5
    '     File Size: 1.76 KB


    '     Class SvgPolyLine
    ' 
    '         Properties: Points
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' A &lt;polyline> is a group of connected straight lines. Since the 
    ''' list of points can get quite long, all the points are included in 
    ''' one attribute
    ''' </summary>
    Public NotInheritable Class SvgPolyLine : Inherits SvgElement

        ''' <summary>
        ''' A list of points. Each number must be separated by a space, comma, EOL,
        ''' or a line feed character with additional whitespace permitted. Each point 
        ''' must contain two numbers: an x coordinate and a y coordinate. So, the 
        ''' list (0,0), (1,1), and (2,2) could be written as 0, 0 1, 1 2, 2.
        ''' </summary>
        ''' <returns></returns>
        Public Property Points As Double()
            Get
                Dim stringArray = Element.GetAttribute("points")
                Return stringArray.Split(New String() {", "}, StringSplitOptions.RemoveEmptyEntries).Select(Function(value) Double.Parse(value, CultureInfo.InvariantCulture)).ToArray()
            End Get
            Set(value As Double())
                Dim lPoints = String.Join(", ", value.Select(Function(x) x.ToString("G", CultureInfo.InvariantCulture)))
                Element.SetAttribute("points", lPoints)
            End Set
        End Property

        Friend Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgPolyLine
            Dim element = parent.OwnerDocument.CreateElement("polyline")
            parent.AppendChild(element)
            Return New SvgPolyLine(element)
        End Function
    End Class
End Namespace

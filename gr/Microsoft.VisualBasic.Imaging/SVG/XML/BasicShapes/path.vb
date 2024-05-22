#Region "Microsoft.VisualBasic::10fd715c14c19effb11ea39fe9d7537a, gr\Microsoft.VisualBasic.Imaging\SVG\XML\BasicShapes\path.vb"

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

    '   Total Lines: 73
    '    Code Lines: 40 (54.79%)
    ' Comment Lines: 26 (35.62%)
    '    - Xml Docs: 92.31%
    ' 
    '   Blank Lines: 7 (9.59%)
    '     File Size: 2.55 KB


    '     Class SvgPath
    ' 
    '         Properties: D, FillRule, Length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml
Imports Microsoft.VisualBasic.Imaging.SVG.XML.Enums
Imports Microsoft.VisualBasic.Text.Xml

Namespace SVG.XML

    ''' <summary>
    ''' A &lt;path> is the most general shape that can be used in SVG. Using a path element, 
    ''' you can draw rectangles (with or without rounded corners), circles, ellipses, 
    ''' polylines, and polygons. Basically any of the other types of shapes, bezier curves, 
    ''' quadratic curves, and many more.
    ''' </summary>
    Public NotInheritable Class SvgPath
        Inherits SvgElement

        ''' <summary>
        ''' A list of points and other information about how to draw the path. 
        ''' </summary>
        ''' <remarks>
        ''' 下面的命令可用于路径数据：
        ''' 
        ''' M = moveto
        ''' L = lineto
        ''' H = horizontal lineto
        ''' V = vertical lineto
        ''' C = curveto
        ''' S = smooth curveto
        ''' Q = quadratic Belzier curve
        ''' T = smooth quadratic Belzier curveto
        ''' A = elliptical Arc
        ''' Z = closepath
        ''' 
        ''' 注释：以上所有命令均允许小写字母。大写表示绝对定位，小写表示相对定位。
        ''' </remarks>
        ''' <returns></returns>
        Public Property D As String
            Get
                Return Element.GetAttribute("d")
            End Get
            Set(value As String)
                Element.SetAttribute("d", value)
            End Set
        End Property

        Public Property Length As Double
            Get
                Return Element.GetAttribute("pathLength", 0.0)
            End Get
            Set(value As Double)
                Element.SetAttribute("pathLength", value)
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

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgPath
            Dim element = parent.OwnerDocument.CreateElement("path")
            parent.AppendChild(element)
            Return New SvgPath(element)
        End Function
    End Class
End Namespace

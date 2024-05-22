#Region "Microsoft.VisualBasic::d28260b6b85fae5f03f85b7b97085479, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgContainer.vb"

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

    '   Total Lines: 77
    '    Code Lines: 57 (74.03%)
    ' Comment Lines: 3 (3.90%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 17 (22.08%)
    '     File Size: 2.28 KB


    '     Class SvgContainer
    ' 
    '         Properties: filter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AddCircle, AddClipPath, AddEllipse, AddGroup, AddImage
    '                   AddLine, AddMarker, AddPath, AddPolygon, AddPolyLine
    '                   AddRect, AddText, GetElements
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    ''' <summary>
    ''' SVG之中的画布对象
    ''' </summary>
    Public MustInherit Class SvgContainer : Inherits SvgElement

        Public Property filter As String
            Get
                Return Element.GetAttribute("filter")
            End Get
            Set(value As String)
                Element.SetAttribute("filter", value)
            End Set
        End Property

        Protected Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function AddCircle() As SvgCircle
            Return SvgCircle.Create(Element)
        End Function

        Public Function AddClipPath() As SvgClipPath
            Return SvgClipPath.Create(Element)
        End Function

        Public Function AddEllipse() As SvgEllipse
            Return SvgEllipse.Create(Element)
        End Function

        Public Function AddGroup() As SvgGroup
            Return SvgGroup.Create(Element)
        End Function

        Public Function AddImage() As SvgImage
            Return SvgImage.Create(Element)
        End Function

        Public Function AddLine() As SvgLine
            Return SvgLine.Create(Element)
        End Function

        Public Function AddMarker() As SvgMarker
            Return SvgMarker.Create(Element)
        End Function

        Public Function AddPath() As SvgPath
            Return SvgPath.Create(Element)
        End Function

        Public Function AddPolygon() As SvgPolygon
            Return SvgPolygon.Create(Element)
        End Function

        Public Function AddPolyLine() As SvgPolyLine
            Return SvgPolyLine.Create(Element)
        End Function

        Public Function AddRect() As SvgRect
            Return SvgRect.Create(Element)
        End Function

        Public Function AddText() As SvgText
            Return SvgText.Create(Element)
        End Function

        Public Iterator Function GetElements() As IEnumerable(Of SvgElement)
            For i As Integer = 0 To Element.ChildNodes.Count - 1
                Yield SvgElement.Create(DirectCast(Element.ChildNodes(i), XmlElement))
            Next
        End Function
    End Class
End Namespace

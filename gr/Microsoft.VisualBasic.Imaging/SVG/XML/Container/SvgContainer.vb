#Region "Microsoft.VisualBasic::b117225c2553e1d891923cfb05aa0f08, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgContainer.vb"

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

    '   Total Lines: 117
    '    Code Lines: 71 (60.68%)
    ' Comment Lines: 23 (19.66%)
    '    - Xml Docs: 13.04%
    ' 
    '   Blank Lines: 23 (19.66%)
    '     File Size: 4.71 KB


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
            Dim list As XmlNodeList = Element.ChildNodes
            Dim node As XmlElement

            ' 20240528
            '  Error in <globalEnvironment> -> R_invoke$pathway_highlights -> for_loop_[[1]: "gut_microbiome"] -> for_loop_[[1]: "lysine"] -> "findText"(&svg_map, &text_str, "ignore.cas...)(&svg_map, &text_str, "ignore.cas...) -> findText
            '   1. InvalidCastException: Unable to cast object of type 'System.Xml.XmlSignificantWhitespace' to type 'System.Xml.XmlElement'.
            '   2. stackFrames:
            '    at Microsoft.VisualBasic.Imaging.SVG.XML.SvgContainer.GetElements()+MoveNext()
            '    at R_graphics.grSVG.findText(SvgDocument svg, String text, Boolean ignore_case)
            '
            '    for (let ["text"] in Call SVG::"findText"(&svg_map, &text_str, "ignore.case" <- True)) %do% {
            '        # forloop_internal
            '        <in_memory> forloop_internal:
            '    Call "text_style"(&text, "color" <- &highlights[&text_str], "strong" <- True);
            '    }
            '    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            '
            ' SVG.R#_clr_interop::.findText at [graphics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]:line &Hx0abc64302
            ' biodeepdb_v3.call_function."findText"(&svg_map, &text_str, "ignore.cas...)(&svg_map, &text_str, "ignore.cas...) at pathway_highlights.R:line 16
            ' biodeepdb_v3.forloop.for_loop_[[1]: "lysine"] at pathway_highlights.R:line 15
            ' biodeepdb_v3.forloop.for_loop_[[1]: "gut_microbiome"] at pathway_highlights.R:line 4
            ' biodeepdb_v3.declare_function.R_invoke$pathway_highlights at pathway_highlights.R:line 1
            ' SMRUCC/R#.global.<globalEnvironment> at <globalEnvironment>:line n/a

            For i As Integer = 0 To list.Count - 1
                node = TryCast(Element.ChildNodes(i), XmlElement)

                If Not node Is Nothing Then
                    If node.Name = "i:pgf" Then
                        Continue For
                    End If

                    Dim svgElement As SvgElement = SvgElement.Create(node)

                    Yield svgElement

                    If svgElement.GetType.IsInheritsFrom(GetType(SvgContainer)) Then
                        For Each svgElm As SvgElement In DirectCast(svgElement, SvgContainer).GetElements
                            Yield svgElm
                        Next
                    End If
                End If
            Next
        End Function
    End Class
End Namespace

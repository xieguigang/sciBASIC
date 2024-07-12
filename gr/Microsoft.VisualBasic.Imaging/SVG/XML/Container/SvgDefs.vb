#Region "Microsoft.VisualBasic::c9913b916f78ebcee31f24227c8aa132, gr\Microsoft.VisualBasic.Imaging\SVG\XML\Container\SvgDefs.vb"

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

    '   Total Lines: 40
    '    Code Lines: 33 (82.50%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 7 (17.50%)
    '     File Size: 1.32 KB


    '     Class SvgDefs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Create, CreateMarker, GetMarkerById
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml

Namespace SVG.XML

    Public Class SvgDefs : Inherits SvgContainer

        Public Sub New(element As XmlElement)
            MyBase.New(element)
        End Sub

        Public Function CreateMarker(id As String, w As Single, h As Single) As SvgMarker
            Dim marker = SvgMarker.Create(Element)
            marker.Id = id
            marker.Width = w
            marker.Height = h
            marker.ViewBox = New SvgViewBox(0, 0, 10, 10)
            marker.RefX = 5
            marker.RefY = 5
            marker.orient = "auto-start-reverse"
            marker.AddPath.D = "M 0 0 L 10 5 L 0 10 z"
            Return marker
        End Function

        Public Function GetMarkerById(id As String) As SvgMarker
            For Each element As SvgElement In GetElements()
                If TypeOf element Is SvgMarker AndAlso element.Id = id Then
                    Return element
                End If
            Next

            Return Nothing
        End Function

        Friend Overloads Shared Function Create(parent As XmlElement) As SvgDefs
            Dim element = parent.OwnerDocument.CreateElement("defs")
            parent.AppendChild(element)
            Return New SvgDefs(element)
        End Function
    End Class
End Namespace

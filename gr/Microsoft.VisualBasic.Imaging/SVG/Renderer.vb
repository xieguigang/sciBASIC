#Region "Microsoft.VisualBasic::bc09b231248d9abaf45f2d3f13fe6122, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\SVG\Renderer.vb"

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

    '   Total Lines: 89
    '    Code Lines: 67
    ' Comment Lines: 12
    '   Blank Lines: 10
    '     File Size: 3.19 KB


    '     Module Renderer
    ' 
    '         Function: DrawImage, PopulateLayers, SVGColorHelper
    ' 
    '         Sub: drawLayer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG.XML
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace SVG

    ''' <summary>
    ''' Rendering svg vector image to gdi+ pixel image.
    ''' (将SVG图像渲染为gdi+图像<see cref="Drawing.Image"/>)
    ''' </summary>
    Public Module Renderer

        <Extension>
        Public Function SVGColorHelper(br As Brush) As String
            If br Is Nothing Then
                ' 透明色
                Return Nothing
            ElseIf TypeOf br Is SolidBrush Then
                Dim color As Color = DirectCast(br, SolidBrush).Color

                If color.IsTransparent Then
                    Return Nothing
                Else
                    Return color.ToHtmlColor
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' Rendering the SVG document as bitmap image.
        ''' </summary>
        ''' <param name="svg"></param>
        ''' <returns></returns>
        Public Function DrawImage(svg As SVGData) As Drawing.Image
            Using g As Graphics2D = svg.Layout.Size.CreateGDIDevice
                With g
                    Call .Clear(svg.SVG.bg.GetBrush)
                    Call .drawLayer(svg)

                    Return .ImageResource
                End With
            End Using
        End Function

        <Extension>
        Private Sub drawLayer(g As Graphics2D, layer As ICanvas)
            ' draw layer components, order by CSS zindex asc
            For Each element As CSSLayer In layer.PopulateLayers.OrderBy(Function(l) l.zIndex)
                Select Case element.GetType
                    Case GetType(g)

                        ' recursively draw svg document tree
                        Call g.drawLayer(element)

                    Case GetType(XML.circle)
                    Case GetType(XML.Image)
                    Case GetType(XML.line)
                    Case GetType(XML.node)
                    Case GetType(XML.path)
                    Case GetType(XML.polygon)
                    Case GetType(XML.polyline)
                    Case GetType(XML.rect)
                    Case GetType(XML.text)
                    Case GetType(XML.title)
                    Case Else
                        Throw New NotImplementedException(element.GetType.FullName)
                End Select
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PopulateLayers(obj As ICanvas) As IEnumerable(Of CSSLayer)
            Return obj.circles.Select(Function(e) DirectCast(e, CSSLayer)).AsList _
                 + obj.images _
                 + obj.lines _
                 + obj.path _
                 + obj.polygon _
                 + obj.polyline _
                 + obj.rect _
                 + obj.texts _
                 + obj.Layers
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::38f8e6e841217fd8370ad3d4eb2d44dd, gr\Microsoft.VisualBasic.Imaging\SVG\Renderer.vb"

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

    '   Total Lines: 72
    '    Code Lines: 52 (72.22%)
    ' Comment Lines: 12 (16.67%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 8 (11.11%)
    '     File Size: 2.55 KB


    '     Module Renderer
    ' 
    '         Function: DrawImage, SVGColorHelper
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
                    Call .drawLayer(svg.SVG.svg)

                    Return .ImageResource
                End With
            End Using
        End Function

        <Extension>
        Private Sub drawLayer(g As Graphics2D, layer As SvgContainer)
            ' draw layer components, order by CSS zindex asc
            For Each element As SvgElement In layer.GetElements
                Select Case element.GetType
                    Case GetType(SvgContainer)
                        ' recursively draw svg document tree
                        Call g.drawLayer(element)

                    Case GetType(SvgCircle)
                    Case GetType(SvgImage)
                    Case GetType(SvgLine)
                    Case GetType(SvgPath)
                    Case GetType(SvgPolygon)
                    Case GetType(SvgPolyLine)
                    Case GetType(SvgRect)
                    Case GetType(SvgText)
                    Case GetType(SvgTitle)
                    Case Else
                        Throw New NotImplementedException(element.GetType.FullName)
                End Select
            Next
        End Sub
    End Module
End Namespace

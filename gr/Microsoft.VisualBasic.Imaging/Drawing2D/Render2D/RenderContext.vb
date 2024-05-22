#Region "Microsoft.VisualBasic::c84b0b12df6d5ab507030fcddf7556e5, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Render2D\RenderContext.vb"

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
    '    Code Lines: 47 (67.14%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 23 (32.86%)
    '     File Size: 1.89 KB


    '     Class RenderContext
    ' 
    '         Properties: fillStyle, globalAlpha, lineWidth, strokeStyle
    ' 
    '         Function: createLinearGradient, getCurrentFill, getCurrentPen
    ' 
    '         Sub: beginPath, bezierCurveTo, closePath, lineTo, moveTo
    '              quadraticCurveTo, Render, stroke
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.SVG.CSS
Imports Microsoft.VisualBasic.Linq
Imports number = System.Double

Namespace Drawing2D

    Public Class RenderContext

        Public Property fillStyle As String
        Public Property lineWidth As number
        Public Property strokeStyle As String
        Public Property globalAlpha As number

        Dim currentShape As Path2D
        Dim drawingShapes As New List(Of RenderShape)

        Public Sub Render(g As IGraphics)
            For Each shape As RenderShape In drawingShapes
                Call g.FillPath(shape.fill, shape)
                Call g.DrawPath(shape.pen, shape)
            Next
        End Sub

        Public Sub beginPath()
            currentShape = New Path2D
        End Sub

        Public Function createLinearGradient(a As number, b As number, c As number, d As number) As Gradient

        End Function

        Public Sub moveTo(a As number, b As number)

        End Sub

        Public Sub quadraticCurveTo(a As number, b As number, c As number, d As number)

        End Sub

        Public Sub lineTo(a As number, b As number)

        End Sub

        Public Sub stroke()

        End Sub

        Private Function getCurrentPen() As Pen

        End Function

        Private Function getCurrentFill() As Brush

        End Function

        Public Sub closePath()
            Call currentShape.CloseAllFigures()
            Call New RenderShape With {
                .shape = currentShape.Path,
                .pen = getCurrentPen(),
                .fill = getCurrentFill()
            }.DoCall(AddressOf drawingShapes.Add)
        End Sub

        Public Sub bezierCurveTo(a As number, b As number, c As number, d As number, end1 As number, end2 As number)

        End Sub
    End Class
End Namespace
